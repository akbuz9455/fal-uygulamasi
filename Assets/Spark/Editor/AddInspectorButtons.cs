using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LeastSquares.Spark
{
    internal static class ComponentActions
    {
        public static void OpenComponentWindow<T>(Button button, string buttonText, Action<T> configure) where T : EditorWindow
        {
            var componentWindow = EditorWindow.GetWindow<T>(true, buttonText);
            configure(componentWindow);
            componentWindow.Show();

            var inspectorWindow = Resources.FindObjectsOfTypeAll<EditorWindow>()
                .FirstOrDefault(w => w.GetType().Name == "InspectorWindow");
            if (inspectorWindow != null)
            {
                float x = button != null ? button.worldBound.xMax / 2 : 500, y = button != null ? button.worldBound.yMax / 2 : 500;
                componentWindow.position = new Rect(x, y, inspectorWindow.position.width, height: 500);
            }
        }    
    }

    [CustomEditor(typeof(MonoBehaviour), true)]
    internal class CustomMonoBehaviourInspector : Editor
    {
        private const string ButtonText = "Edit Script with AI";
        private const float ButtonWidth = 120f;
        private const float ButtonHeight = 20f;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var monoBehaviour = target as MonoBehaviour;
            var monoScript = MonoScript.FromMonoBehaviour(monoBehaviour);
            var scriptPath = AssetDatabase.GetAssetPath(monoScript);

            if (scriptPath.StartsWith("Assets/"))
            {
                EditorGUILayout.Space();
                if (LeastSquaresGUI.CenteredButton(ButtonText, GUILayout.Width(ButtonWidth), GUILayout.Height(ButtonHeight)))
                {
                    OnButtonClick(MonoScript.FromMonoBehaviour(monoBehaviour));
                }
            }
        }
        
        public static void OnButtonClick(MonoScript monoScript)
        {
            ComponentActions.OpenComponentWindow<IterativeCoderComponentWindow>(null , ButtonText, window =>
                {
                    window.SetMode(CodeMode.EditComponentScript);
                    window.selectedScript = monoScript;
                }
            );
        }
    }
    
    [CustomEditor(typeof(Shader), true)]
    internal class CustomShaderBehaviour : Editor
    {
        private const string ButtonText = "Edit Shader with AI";
        private const float ButtonWidth = 120f;
        private const float ButtonHeight = 20f;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var shader = target as Shader;
            var scriptPath = AssetDatabase.GetAssetPath(shader);

            if (scriptPath.StartsWith("Assets/"))
            {
                EditorGUILayout.Space();
                if (LeastSquaresGUI.CenteredButton(ButtonText, GUILayout.Width(ButtonWidth), GUILayout.Height(ButtonHeight)))
                {
                    OnButtonClick(shader);
                }
            }
        }
        public static void OnButtonClick(Shader shader)
        {
            ComponentActions.OpenComponentWindow<IterativeShaderCoderComponentWindow>(null , ButtonText, window =>
                {
                    window.SetMode(CodeMode.EditComponentScript);
                    window.selectedShader = shader;
                }
            );
        }
    }
}