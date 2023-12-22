using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public static class LeastSquaresGUI
    {
        public static bool CenteredButton(string label, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            bool clicked = GUILayout.Button(label, options);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return clicked;
        }

        public static void SelectScript(ref string _selectedScriptPath, ref Object _selectedScript)
        {
            //EditorGUILayout.LabelField("Select Script", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _selectedScript = EditorGUILayout.ObjectField("Script", _selectedScript, typeof(MonoScript), false);
            if (EditorGUI.EndChangeCheck())
            {
                _selectedScriptPath = AssetDatabase.GetAssetPath(_selectedScript);
            }
/*
            EditorGUILayout.Space();

            if (GUILayout.Button("Select .cs Script File"))
            {
                _selectedScriptPath = EditorUtility.OpenFilePanel("Select .cs Script", Application.dataPath, "cs");
                if (!string.IsNullOrEmpty(_selectedScriptPath))
                {
                    _selectedScriptPath = "Assets" + _selectedScriptPath.Substring(Application.dataPath.Length);
                    _selectedScript = AssetDatabase.LoadAssetAtPath(_selectedScriptPath, typeof(MonoScript));
                }
            }*/

            if (!string.IsNullOrEmpty(_selectedScriptPath))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Selected Script Path:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(_selectedScriptPath, EditorStyles.wordWrappedLabel);
            }
        }
        
        public static void RenderColoredCode(string code)
        {
            GUIStyle codeStyle = new GUIStyle(EditorStyles.textArea);
            codeStyle.richText = true;
            codeStyle.wordWrap = true;
            codeStyle.font = EditorStyles.standardFont;
            codeStyle.fontSize = 12; // Set the font size

            // Set text area to non-interactable
            codeStyle.active.textColor = codeStyle.normal.textColor;
            codeStyle.active.background = codeStyle.normal.background;
            codeStyle.onActive.textColor = codeStyle.normal.textColor;
            codeStyle.onActive.background = codeStyle.normal.background;
            codeStyle.focused.textColor = codeStyle.normal.textColor;
            codeStyle.focused.background = codeStyle.normal.background;
            codeStyle.onFocused.textColor = codeStyle.normal.textColor;
            codeStyle.onFocused.background = codeStyle.normal.background;

            string coloredCode = ApplySyntaxHighlighting(code);
            EditorGUILayout.TextArea(coloredCode, codeStyle, GUILayout.ExpandHeight(true));
        }

        public static string ApplySyntaxHighlighting(string code)
        {
            if (code == null) return null;
            // Define Monokai syntax highlighting colors
            string keywordColor = "#F92672";
            string commentColor = "#75715E";
            string stringColor = "#E6DB74";
            string typeColor = "#66D9EF";

            // Regex patterns for keywords, comments, strings, and types
            string keywordPattern = @"\b(private|public|protected|void|if|else|for|foreach|while|do|return|break|continue|true|false|null|new|using|try|catch|throw)\b";
            string commentPattern = @"(\/\/[^\r\n]*|\/\*[\s\S]*?\*\/)";
            string stringPattern = @"(""[^""\\\n]*(?:\\.[^""\\\n]*)*"")";
            string typePattern = @"\b(int|float|string|bool|class|object)\b";

            // Apply highlighting
            string highlightedCode = Regex.Replace(code, keywordPattern, $"<color={keywordColor}>$1</color>");
            highlightedCode = Regex.Replace(highlightedCode, commentPattern, $"<color={commentColor}>$1</color>");
            highlightedCode = Regex.Replace(highlightedCode, stringPattern, $"<color={stringColor}>$1</color>");
            highlightedCode = Regex.Replace(highlightedCode, typePattern, $"<color={typeColor}>$1</color>");

            return highlightedCode;
        }
    }
}