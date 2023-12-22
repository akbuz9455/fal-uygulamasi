using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class AIShader : AICoder
    {
        private Material _material;
        private MaterialEditor materialEditor;
        private string prevAnswer;
        private float _customTime;
        
        /// <summary>
        /// Creates the prompt for the AI
        /// </summary>
        /// <param name="Prompt">The prompt to be used</param>
        /// <returns>The prompt for the AI</returns>
        protected override ChatCompletionMessage[] CreatePrompt(string prompt)
        {
            var outputOnlyCode = _outputOnlyCode
                ? "Only return the shader code with no formatting nor explanations. Do not write any explanations."
                : string.Empty;

            var msg = new ChatCompletionMessage
            {
                role = "user",
                content = prompt
            };

            return new[]
            {
                new ChatCompletionMessage
                {
                    role = "user",
                    content =
                        "You are a senior Unity 3D programmer and ShaderLab expert. A user will provide you a spec and you will write the shader that best suits that spec. " +
                        $"You will make sure its compilable and cross platform. {outputOnlyCode}" +
                        outputOnlyCode
                },
                msg
            };
        }

        private void SaveNewShader()
        {
            var fileName = ExtractName(_answer);
            if (GUILayout.Button($"Save as {fileName}"))
            {
                File.WriteAllText(Paths.GetPathInScriptsFolder(fileName), _answer);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", "Saved successfully", "OK");
            }
        }

        /// <summary>
        /// Extracts the name of the shader from the code
        /// </summary>
        /// <param name="shaderContent">The content of the shader</param>
        /// <returns>The name of the shader</returns>
        private string ExtractName(string shaderContent)
        {
            const string regex = @"Shader\s+""([\w\s/]+)""";
            var groups = Regex.Match(shaderContent, regex).Groups;
            if (groups.Count <= 1) return "Default.shader";
            return $"{groups[1].Value.Replace('/', '_').Replace(' ', '_')}.shader";
        }

        /// <summary>
        /// GUI code to be executed at the start
        /// </summary>
        protected override void OnStartGUI()
        {
            GUILayout.Label("Describe to the AI the shader you want to write:");
            EditorGUILayout.Separator();
            _actionMode = ActionMode.CreateNewScript; // Only allow creating new shaders
        }

        protected override void OnEndGUI()
        {
            if (string.IsNullOrEmpty(_answer)) return;
            EditorGUILayout.BeginHorizontal();
            
            if (_material == null || _answer != prevAnswer)
            {
                var shader = ShaderCreator.CreateTempShader("AIShaderTemp", _answer);
                _material = new Material(shader);
                prevAnswer = _answer;
            }

            GUILayout.FlexibleSpace();

            if (_material != null)
            {
                if (materialEditor == null || materialEditor.target != _material)
                {
                    materialEditor = (MaterialEditor)Editor.CreateEditor(_material);
                }

                // Calculate the size of the preview rect to fit within the current editor window
                float previewSize = 128;//Mathf.Min(position.width * 0.5f, position.height - EditorGUIUtility.singleLineHeight);
                Rect previewRect = GUILayoutUtility.GetRect(previewSize, previewSize, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

                if (Event.current.type == EventType.Repaint)
                {
                    materialEditor.OnPreviewGUI(previewRect, GUIStyle.none);
                }
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            SaveNewShader();
        }

        protected override void AdditionalSettingsGUI()
        {
        }
    }
}