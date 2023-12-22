using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LeastSquares.Spark
{
    internal class AISceneEditor : EditorWindow
    {
        private string TempFilePath => Paths.GetPathInProjectTempFolder("_scene.cs");
        private string _prompt;

        bool TempFileExists => System.IO.File.Exists(TempFilePath);

        private static string GetPrompt(string prompt)
        {
            var sceneDescription = GenerateCompressedSceneState();
            return $@"
I have a Unity scene with the following state (in the format of GameObject names, positions, and components):

{sceneDescription}

I want to perform the following action: '{prompt}'

Please provide a Unity Editor script that does the following:
- Provides its functionality as a menu item placed 'Edit' > 'Do Task'.
                - Doesn't provide any editor window. It immediately does the task when the menu item is invoked.
            - Doesn't use GameObject.FindGameObjectsWithTag.
            - There is no selected object. Find game objects manually.
            - Only include the script body, without any explanation.
            - Dont forget to include all necessary imports.
            - Do not include backticks nor formatting. Only the code.

                Assume you are an expert Unity user and generate the code for this task.
";
        }

        private async void Run()
        {
            var finalPrompt = GetPrompt(_prompt);
            Debug.Log(finalPrompt);
            var code = await OpenAIAccessManager.RequestChatCompletion(new []
            {
                new ChatCompletionMessage
                {
                    role = "user",
                    content = finalPrompt
                }
            });
            ScriptCreator.CreateScriptAsset(code, TempFilePath);
        }
        
        void OnGUI()
        {
            _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.ExpandHeight(true));
            if (GUILayout.Button("Apply"))
            {
                Run();
            }
        }

        void OnEnable()
            => AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

        void OnDisable()
            => AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

        void OnAfterAssemblyReload()
        {
            if (!TempFileExists) return;
            EditorApplication.ExecuteMenuItem("Edit/Do Task");
            AssetDatabase.DeleteAsset(TempFilePath);
        }
        
        public static string GenerateCompressedSceneState()
        {
            var sceneStateBuilder = new StringBuilder();
            var activeScene = SceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();

            foreach (var rootObject in rootObjects)
            {
                GenerateCompressedGameObjectState(rootObject.transform, sceneStateBuilder);
            }

            return sceneStateBuilder.ToString();
        }

        private static void GenerateCompressedGameObjectState(Transform currentTransform, StringBuilder sceneStateBuilder)
        {
            var currentObject = currentTransform.gameObject;
            sceneStateBuilder.AppendFormat("{0}[", currentObject.name);

            var components = currentObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                sceneStateBuilder.Append(components[i].GetType().Name);
                if (i < components.Length - 1)
                {
                    sceneStateBuilder.Append(",");
                }
            }
            sceneStateBuilder.Append("]");

            for (var i = 0; i < currentTransform.childCount; i++)
            {
                var childTransform = currentTransform.GetChild(i);
                GenerateCompressedGameObjectState(childTransform, sceneStateBuilder);
            }
        }
    }
}