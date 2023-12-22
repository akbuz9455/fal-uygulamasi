using System;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace LeastSquares.Spark
{
    public class AISettings : SerializableEditorTool
    {
        private const string APIDomainSaveName = "OpenAI_APIDomain";
        private const string APIKeySaveName = "OpenAI_APIKey";
        private const string TemperatureSaveName = "OpenAI_Temperature";
        private const string ChatModelSaveName = "OpenAI_ChatModel";
        private const string SaveScriptsLocationSaveName = "OpenAI_SaveScriptsLocation";
        public static AIEditorToggle ShowEditSceneButton = new("OpenAI_ShowEditSceneButton", "Show \"Edit Scene\" Button", true, true);
        private static string _apiKey;
        private static string _apiDomain = OpenAIAPI.OpenAIDomain;
        private static ChatModel _chatModel;
        private static double _temperature;

        public static string ApiDomain
        {
            get => _apiDomain;

            set
            {
                if (value == _apiDomain) return;
                _apiDomain = value;
                if (value != null)
                {
                    OpenAIAccessManager.SetAPIDomain(value);
                    SaveSystem.SetGlobalString(APIDomainSaveName, value);
                }
            }
        }
        
        /// <summary>
        /// The OpenAI API key to use for authentication.
        /// </summary>
        public static string ApiKey
        {
            get => _apiKey;

            set
            {
                if (value == _apiKey) return;
                _apiKey = value;
                if (value != null)
                {
                    OpenAIAccessManager.SetAPIKey(value);
                    SaveSystem.SetGlobalString(APIKeySaveName, value);
                }
            }
        }

        /// <summary>
        /// The sampling temperature to use for generating the text completion.
        /// </summary>
        public static double Temperature
        {
            get => _temperature;

            set
            {
                _temperature = value;
                OpenAIAccessManager.Temperature = value;
                SaveSystem.SetGlobalFloat(TemperatureSaveName, (float)value);
            }
        }
        
        /// <summary>
        /// The OpenAI model to use for code completion.
        /// </summary>
        public static ChatModel ChatModel
        {
            get => _chatModel;
            set
            {
                _chatModel = value;
                OpenAIAccessManager.ChatModel = value;
                SaveSystem.SetGlobalString(ChatModelSaveName, value.ToString());
            }
        }
        
        /// <summary>
        /// The location to save the generated scripts to.
        /// </summary>
        private static string SaveScriptsLocation
        {
            get => Paths.RelativePathScriptSaveLocation;
            set
            {
                Paths.RelativePathScriptSaveLocation = value;
                SaveSystem.SetProjectString(SaveScriptsLocationSaveName, value);
            }
        }

        public static void Load()
        {
            var modelValue = SaveSystem.GetGlobalString(ChatModelSaveName, null);
            modelValue = string.IsNullOrEmpty(modelValue) ? ChatModel.ChatGPT3.ToString() : modelValue; 
            
            ApiKey = SaveSystem.GetGlobalString(APIKeySaveName, null);
            ApiDomain = SaveSystem.GetGlobalString(APIDomainSaveName, OpenAIAPI.OpenAIDomain);
            Temperature = SaveSystem.GetGlobalFloat(TemperatureSaveName, 0.1f);
            SaveScriptsLocation = SaveSystem.GetProjectString(SaveScriptsLocationSaveName, "/SparkScripts");
            ChatModel = (ChatModel)Enum.Parse(typeof(ChatModel), modelValue);
            ShowEditSceneButton.Load();
        }

        /// <summary>
        /// Displays the GUI for the GPT-3 settings window.
        /// </summary>
        public override void OnGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                GUILayout.Label("Base Settings", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                ApiKey = EditorGUILayout.TextField("OpenAI API Key", ApiKey);
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                SaveScriptsLocation = EditorGUILayout.TextField("Save Folder:", SaveScriptsLocation);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    string tempFolderPath = EditorUtility.OpenFolderPanel("Select a folder", SaveScriptsLocation, "");
                    if (ValidatePath(tempFolderPath))
                    {
                        SaveScriptsLocation = tempFolderPath.Replace(Application.dataPath, "");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder within the Unity project.", "OK");
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                
                Temperature = EditorGUILayout.DoubleField("Temperature", Temperature);
                EditorGUILayout.Space();
                
                int selectedIndex = Array.IndexOf(Enum.GetValues(typeof(ChatModel)), ChatModel);
                int newIndex = EditorGUILayout.Popup("AI Model", selectedIndex, GetEnumDisplayNames(typeof(ChatModel)));

                if (newIndex != selectedIndex)
                {
                    ChatModel = (ChatModel)Enum.GetValues(typeof(ChatModel)).GetValue(newIndex);
                }
                
                EditorGUILayout.Space();
                ApiDomain = EditorGUILayout.TextField("API domain", ApiDomain);
                EditorGUILayout.HelpBox("Customize the API domain for spark to use. Useful for using proxies.", MessageType.Info);

                EditorGUILayout.Space();

                GUILayout.Label("Other Settings", EditorStyles.boldLabel);
                ShowEditSceneButton.Render();
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Save"))
                {
                    SaveSystem.SetGlobalString(APIKeySaveName, ApiKey);
                    SaveSystem.SetGlobalFloat(TemperatureSaveName, (float)Temperature);
                    SaveSystem.SetGlobalString(ChatModelSaveName, ChatModel.ToString());
                    SaveSystem.SetProjectString(SaveScriptsLocationSaveName, SaveScriptsLocation);
                    ShowEditSceneButton.Save();
                }
            }
            EditorGUILayout.EndVertical();
        }

        public override void Reload()
        {
            
        }

        private bool ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            string projectPath = Application.dataPath;
            return path.StartsWith(projectPath);
        }
        
        private static string[] GetEnumDisplayNames(Type enumType)
        {
            Array enumValues = Enum.GetValues(enumType);
            string[] displayNames = new string[enumValues.Length];

            for (int i = 0; i < enumValues.Length; i++)
            {
                Enum value = (Enum)enumValues.GetValue(i);
                FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                displayNames[i] = attributes.Length > 0 ? attributes[0].Description : value.ToString();
            }

            return displayNames;
        }
    }
}