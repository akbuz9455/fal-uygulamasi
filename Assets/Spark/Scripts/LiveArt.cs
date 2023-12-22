using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeastSquares;
using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LeastSquares.Spark
{
    public class LiveArt : MonoBehaviour
    {
        public string Prompt => inputField.text;
        public static string[] validSizes = new string[] { "256x256", "512x512", "1024x1024" };
        [HideInInspector] public int sizeIndex = 1;
        public RawImage image;
        public TMP_InputField inputField;
        public Button button;

        public string Size
        {
            get { return validSizes[sizeIndex]; }
        }

        private bool isRegenerating = false;

        void Start()
        {
            LoadArt();
            button.onClick.AddListener(Regenerate);
        }

        public void Regenerate()
        {
            if (isRegenerating) return;
            LoadArt();
        }

        private async void LoadArt()
        {
            isRegenerating = true;
            button.interactable = false;
            button.GetComponentInChildren<TMP_Text>().text = "Loading..."; // Added line to change button text
            try
            {
                if (!ChatEngine._loaded) return;
                var imageResponse = await OpenAIAccessManager.RequestImageGeneration(Prompt, 1, Size);
                var url = imageResponse.urls[0];
                image.texture = await DownloadImage(url);
            }
            finally
            {
                isRegenerating = false;
                button.interactable = true;
                button.GetComponentInChildren<TMP_Text>().text =
                    "Regenerate"; // Added line to change button text back to original
            }
        }

        private void OnValidate()
        {
            if (sizeIndex < 0 || sizeIndex >= validSizes.Length)
            {
                Debug.LogWarning("Invalid size index. Please choose one of the following: " +
                                 string.Join(", ", validSizes));
                sizeIndex = 1;
            }
        }

        private async Task<Texture2D> DownloadImage(string url)
        {
            Debug.Log("Downloading texture");
            using var www = UnityWebRequestTexture.GetTexture(url);
            var downloadHandler = new DownloadHandlerBuffer();
            www.downloadHandler = downloadHandler;
            await www.SendWebRequestAsync(null);

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download image: " + www.error);
                return null;
            }

            var texture = new Texture2D(2, 2);
            return texture.LoadImage(downloadHandler.data) ? texture : null;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LiveArt))]
    public class LiveArtEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            LiveArt liveArt = (LiveArt)target;

            EditorGUI.BeginChangeCheck();
            int sizeIndex = EditorGUILayout.Popup("Size", liveArt.sizeIndex, LiveArt.validSizes);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(liveArt, "Change Size");
                liveArt.sizeIndex = sizeIndex;
            }
        }
    }
#endif
}