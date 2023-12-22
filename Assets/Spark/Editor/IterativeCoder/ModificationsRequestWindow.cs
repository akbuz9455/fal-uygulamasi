using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class ModificationRequestWindow : EditorWindow
    {
        public string RequestedChanges { get; private set; }
        public System.Action<string> OnSubmit;

        private Vector2 _scrollPosition;

        public static ModificationRequestWindow Init(System.Action<string> onSubmit)
        {
            ModificationRequestWindow
                window = GetWindow<ModificationRequestWindow>(true, "Request Modifications", true);
            window.RequestedChanges = "";
            window.OnSubmit = onSubmit;
            return window;
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Tip: You can ask the AI to document your code too!", MessageType.Info);
            EditorGUILayout.LabelField("Please specify the changes you want to make:", EditorStyles.wordWrappedLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));
            var style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
            RequestedChanges = EditorGUILayout.TextArea(RequestedChanges, style, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Submit", GUILayout.Height(30)))
                {
                    OnSubmit?.Invoke(RequestedChanges);
                    Close();
                }

                if (GUILayout.Button("Cancel", GUILayout.Height(30)))
                {
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}