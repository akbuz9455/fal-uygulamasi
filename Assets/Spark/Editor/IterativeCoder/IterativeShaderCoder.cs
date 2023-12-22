using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    internal class IterativeShaderCoder : IterativeCoder
    {
        private Material _material;
        private MaterialEditor materialEditor;
        private string prevSource;
        public override string ObjectName => "Shader";
        protected override string GetBasePrompt() => NewCodingSessionWindow.GetBaseShaderPrompt();
        
        private const string ShaderSaveKeyName = "ChatGPT_ShaderSessions";
        protected override string SaveKeyName => ShaderSaveKeyName;
        protected override bool ShowCompilationErrors => false;

        private void RenderPreviewShader()
        {
            if (SelectedConversation.IsLastCodeVersionPartial) return;
            EditorGUILayout.BeginHorizontal();

            var source = SelectedConversation.LastCodeVersion;
            if (_material == null || source != prevSource)
            {
                var shader = ShaderCreator.CreateTempShader("_temp", source);
                if (shader == null) return;
                
                _material = new Material(shader);
                prevSource = source;
            }

            GUILayout.FlexibleSpace();

            if (_material != null)
            {
                if (materialEditor == null || materialEditor.target != _material)
                {
                    materialEditor = (MaterialEditor)Editor.CreateEditor(_material);
                }

                // Calculate the size of the preview rect to fit within the current editor window
                float previewSize = 128 + 64;//Mathf.Min(position.width * 0.5f, position.height - EditorGUIUtility.singleLineHeight);
                var previewRect = GUILayoutUtility.GetRect(previewSize, previewSize, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

                if (Event.current.type == EventType.Repaint)
                {
                    materialEditor.OnPreviewGUI(previewRect, GUIStyle.none);
                }
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        protected override void RenderExtraPreview()
        {
            RenderPreviewShader();
        }
        
        protected override string ExtractName(string content)
        {
            const string regex = @"Shader\s+""([^""]+)""";
            var groups = Regex.Match(content, regex).Groups;
            if (groups.Count <= 1) return "UnnamedShader.shader";
            return $"{groups[1].Value.Replace("/", "_")}.shader";
        }


        protected override string PostProcessCode(string code)
        {
            return CodeCleaner.CleanExtraCommentsAndQuotes(code);
        }
    }
}