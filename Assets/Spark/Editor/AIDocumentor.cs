using System.IO;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    /// <summary>
    /// AIDocumentor is a GPT3Tool that allows the user to add C# doc strings to a selected script.
    /// </summary>
    internal class AIDocumentor : AIEditorTool
    {
        protected override bool RenderAnswerAsCode => true;
        private Object _selectedScript;
        protected override bool HidePrompt => true;

        /// <summary>
        /// OnStartGUI allows the user to select a script.
        /// </summary>
        protected override void OnStartGUI()
        {
            var prevObject = _selectedScript;
            _selectedScript = (Object)EditorGUILayout.ObjectField("Script", _selectedScript, typeof(Object), false);
            if (_selectedScript != null && Path.GetExtension(AssetDatabase.GetAssetPath(_selectedScript)) != ".cs")
            {
                _selectedScript = null;
                Debug.LogWarning("Only C# scripts are supported. PLease select a C# script.");
            }

            if (_selectedScript != prevObject)
                _answer = null;
        }

        /// <summary>
        /// OnEndGUI allows the user to replace the existing file with the new code.
        /// </summary>
        protected override void OnEndGUI()
        {
            if (string.IsNullOrEmpty(_answer)) return;
            EditorGUILayout.Separator();
            if (GUILayout.Button("Replace existing file"))
            {
                var path = AssetDatabase.GetAssetPath(_selectedScript);
                File.WriteAllText(path, _answer);
                EditorUtility.DisplayDialog("Success", "Saved successfully", "OK");
            }
        }

        /// <summary>
        /// CreatePrompt creates a prompt for the GPT-3 AI to generate the new code.
        /// </summary>
        /// <param name="_">The string to be used for the prompt.</param>
        /// <returns>The prompt for the GPT-3 AI.</returns>
        protected override ChatCompletionMessage[] CreatePrompt(string _)
        {
            var script = AssetDatabase.GetAssetPath(_selectedScript);
            var contents = File.ReadAllText(script);
            return new[]
            {
                new ChatCompletionMessage
                {
                    role = "system",
                    content = "You are a senior Unity 3D programmer who loves writing documentation. You will be given a script" +
                              " and you will need to add C# doc strings to the script. " +
                              "The doc strings should explain the functionality of the function. Do not skip the using statements." +
                              " Do not add comments to variables. Only add comments to functions."
                },
                new ChatCompletionMessage
                {
                    role = "user",
                    content = contents
                }
            };
            
        }
    }
}