using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace LeastSquares.Spark
{
    public class AIArtist : SerializableEditorTool
    {
        private int _toolbarIndex;
        private bool _isProcessing;
        private float _waitTime;
        private double _lastTime;
        private double _currentProgress;
        private Vector2 _scrollPosition;
        private static readonly string[] ToolbarOptions = { "Image Generation"};//, "Image Editing", "Image Variation" };

        private AIImageAction[] _imageActions = new[]
        {
            new AIImageAction("A beautiful landscape", (A, P) => OpenAIAccessManager.RequestImageGeneration(A.prompt, A.imageCount, A.size, P)),
            new AIImageAction("Add a rainbow to the image", (A, P) => OpenAIAccessManager.RequestImageEdit(A.sourceImage, A.prompt, A.imageCount, A.size, null, P)),
            new AIImageAction(null, (A, P) => OpenAIAccessManager.RequestImageVariation(A.sourceImage, A.imageCount, A.size, P))
        };

        public override void OnGUI()
        {
            _toolbarIndex = GUILayout.Toolbar(_toolbarIndex, ToolbarOptions);

            EditorGUILayout.BeginVertical("Box");

            DisplayImageActionUI(_imageActions[_toolbarIndex]);

            EditorGUILayout.EndVertical();
        }
        
        private void DisplayImageActionUI(AIImageAction action)
        {
            EditorGUI.BeginDisabledGroup(_isProcessing);
            GUILayout.Label(ToolbarOptions[_toolbarIndex], EditorStyles.boldLabel);
            action.prompt = EditorGUILayout.TextField("Prompt:", action.prompt);

            if (_toolbarIndex != 0)
            {
                action.sourceImage = (Texture2D)EditorGUILayout.ObjectField("Image:", action.sourceImage, typeof(Texture2D), false);
            }

            action.imageCount = EditorGUILayout.IntSlider("Number of images:", action.imageCount, 1, 4);
            action.selectedSizeIndex = EditorGUILayout.Popup("Image Size:", action.selectedSizeIndex, AIImageAction.Sizes);
            
            if (_isProcessing)
            {
                EditorGUILayout.Space();
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 16f), (float) _currentProgress, "Processing...");
                EditorGUILayout.Space();
            }
            
            if (GUILayout.Button(_toolbarIndex == 1 ? "Edit Image" : "Generate Image"))
            {
                _isProcessing = true;
                _currentProgress = 0;
                _ = ProcessImage(action, new Progress<double>(p =>
                {
                    _currentProgress = p;
                    Repaint();
                }));
            }
            EditorGUI.EndDisabledGroup();

            DisplayTextures(action.generatedTextures, action);
        }

        private async Task ProcessImage(AIImageAction action, IProgress<double> progress)
        {
            const int totalProgressReports = 5;
            var progressOffset = 0;
            EditorApplication.update += Update;
            try
            {
                var progressRequest = new Progress<double>(P => progress.Report(P / totalProgressReports));
                string[] imageUrls = (await action.action(action, progressRequest)).urls;
                progressOffset++;

                if (imageUrls == null)
                {
                    Debug.LogError("Failed to process image.");
                    return;
                }

                Debug.Log("Downloading images");

                action.generatedTextures = new Texture2D[imageUrls.Length];

                for (int i = 0; i < imageUrls.Length; i++)
                {
                    using (var www = UnityWebRequestTexture.GetTexture(imageUrls[i]))
                    {
                        var downloadHandler = new DownloadHandlerBuffer();
                        www.downloadHandler = downloadHandler;
                        var k = progressOffset;
                        var imageProgress = new Progress<double>(P => progress.Report((P + k) / totalProgressReports));
                        await www.SendWebRequestAsync(imageProgress);

                        if (www.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogError("Failed to download image: " + www.error);
                            continue;
                        }

                        Texture2D texture = new Texture2D(2, 2);
                        if (!texture.LoadImage(downloadHandler.data))
                        {
                            Debug.LogError("Failed to load image.");
                            continue;
                        }

                        action.generatedTextures[i] = texture;
                        progressOffset++;
                    }
                }
            } catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                _isProcessing = false;
                EditorApplication.update -= Update;
                Repaint();
            }
        }

        private void DisplayTextures(Texture2D[] textures, AIImageAction action)
        {
            if (textures == null || textures[0] == null) return;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < textures.Length; i++)
            {
                if(textures[i] == null) continue;
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Texture " + (i + 1) + ":");
                GUILayout.Box(textures[i], GUILayout.Width(textures[i].width), GUILayout.Height(textures[i].height));
                if (GUILayout.Button("Save"))
                {
                    SaveTexture(textures[i], Paths.GetPathInScriptsFolder($"Images/{DateTime.Now.ToString($"yyyy-MM-dd_HH-mm-ss")}.png"));
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
        
        private void SaveTexture(Texture2D texture, string filePath)
        {
            var textureBytes = texture.EncodeToPNG();

            File.WriteAllBytes(filePath, textureBytes);
            AssetDatabase.Refresh();
            //EditorUtility.DisplayDialog("Success", "Texture saved to: " + filePath, "Ok");
            Debug.Log($"Texture saved to: {filePath}");
        }

        /// <summary>
        /// Updates the wait time for the AI.
        /// </summary>
        private void Update()
        {
            var prev = _waitTime;
            var time = EditorApplication.timeSinceStartup;
            _waitTime = (_waitTime + (float)(time - _lastTime)) % 3;
            _lastTime = time;
            if ((int)prev != (int)_waitTime)
                Repaint();
        }
        
        private class AIImageAction
        {
            public static readonly string[] Sizes = {"256x256", "512x512", "1024x1024" };
            public string prompt;
            public int imageCount = 1;
            public int selectedSizeIndex;
            public Texture2D sourceImage;
            public Texture2D[] generatedTextures;
            public Func<AIImageAction, IProgress<double>, Task<ImageResponse>> action;

            public AIImageAction(string startingPrompt, Func<AIImageAction, IProgress<double>, Task<ImageResponse>> action)
            {
                this.action = action;
                this.prompt = startingPrompt;
            }
            
            public string size => Sizes[selectedSizeIndex];
        }
    }
}