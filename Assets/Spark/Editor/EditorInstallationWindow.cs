using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class EditorInstallationWindow : EditorWindow
    {
        private static bool showOnStart = true;
        private const string showOnStartPrefKey = "GPTShowOnStart";
        private const string SuperToolName = "Spark";
        private bool IsAPIKeySaved => !string.IsNullOrEmpty(AISettings.ApiKey);
        private Vector2 scrollPosition;

        [MenuItem("Window/Spark AI/Installation")]
        public static void ShowWindow()
        {
            GetWindow<EditorInstallationWindow>("Spark AI");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginVertical("Box");
            {
                RenderWelcomeLabel();
                RenderAPIKeySection();
                RenderTools();
                RenderDocsAndLinks();
                RenderStartupToggle();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void RenderWelcomeLabel()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Welcome to Spark AI", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        private void RenderAPIKeySection()
        {
            var prevColor = GUI.backgroundColor;
            if (IsAPIKeySaved) GUI.backgroundColor = Color.green;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Step 1: Get Your API Key",
                    IsAPIKeySaved ? EditorStyles.boldLabel : EditorStyles.label);
                EditorGUILayout.HelpBox(
                    "To use Spark AI, you need an API key from OpenAI. Visit https://platform.openai.com/account/api-keys to sign up and obtain your API key. For more detailed steps check the documentation",
                    MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Open Settings", GUILayout.Height(25), GUILayout.ExpandWidth(true)))
                    {
                        SaveSystem.SetProjectBool("SparkOpenSettings", true);
                        EditorApplication.ExecuteMenuItem($"Window/Spark AI/Spark");
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (IsAPIKeySaved)
                {
                    EditorGUILayout.HelpBox("API key saved successfully.", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = prevColor;
            EditorGUILayout.Space();
        }

        private void RenderTools()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Step 2: Check out the super tool", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("You can find all the tools under Window > Spark AI. The available tools are:",
                    MessageType.Info);
                if (GUILayout.Button("Open 'Spark AI' Tool", GUILayout.Height(25), GUILayout.ExpandWidth(true)))
                {
                    EditorApplication.ExecuteMenuItem($"Window/Spark AI/{SuperToolName}");
                }
                EditorGUILayout.Space();
                for (int i = 0; i < toolNames.Length; i++)
                {
                    GUILayout.Label($"* {toolNames[i]}", EditorStyles.boldLabel, GUILayout.Height(25), GUILayout.ExpandWidth(true));
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void RenderDocsAndLinks()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Step 3: Check out the documentation & more", EditorStyles.boldLabel);
                if (GUILayout.Button("Open Documentation")) Application.OpenURL("https://leastsquares.io/docs/unity/spark");
                if (GUILayout.Button("Join Discord Community"))
                    Application.OpenURL("https://discord.gg/DZpBsTYNPD");
                if (GUILayout.Button("Check Other Assets"))
                    Application.OpenURL("https://assetstore.unity.com/publishers/39777");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void RenderStartupToggle()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Show this window on startup", GUILayout.Width(200));
                showOnStart = EditorGUILayout.Toggle(showOnStart);
                SaveSystem.SetProjectBool(showOnStartPrefKey, showOnStart);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void OnEnable()
        {
            showOnStart = SaveSystem.GetProjectBool(showOnStartPrefKey, true);
        }

        private static readonly string[] toolNames =
        {
            "AI Conversations (ChatGPT)",
            "Iterative Coder",
            "AI Artist",
            "Shader Coder",
            "AI Debugger",
            "AI Writer",
            "AI Settings"
        };
    }
}