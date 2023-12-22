using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class AIDebugger : AIEditorTool
    {
        private Object _selectedScript;
        
        /// <summary>
        /// GUI for the AI Debugger
        /// </summary>
        protected override void OnStartGUI()
        {
            _selectedScript = (Object)EditorGUILayout.ObjectField("Script", _selectedScript, typeof(Object), false);
            GUILayout.Label("Describe to the AI the issues the script has:");
        }

        /// <summary>
        /// GUI for the AI Debugger
        /// </summary>
        protected override void OnEndGUI()
        {

        }

        /// <summary>
        /// Creates a prompt for the AI Debugger
        /// </summary>
        /// <param name="Prompt">The prompt for the AI Debugger</param>
        /// <returns>The created prompt for the AI Debugger</returns>
        protected override ChatCompletionMessage[] CreatePrompt(string Prompt)
        {
            var script = AssetDatabase.GetAssetPath(_selectedScript);
            var contents = File.ReadAllText(script);
            var content = $"The following script:\n\n ```{contents}``` is having these issues: \n\n```{Prompt}```\n\nWhy:";
            return new[]
            {
                new ChatCompletionMessage
                {
                    role = "system",
                    content = "You are a senior Unity 3D programmer who is helping users with questions and problems." +
                              " Answer with the most of you knowledge and experience. Provide all the context you can when possible. " +
                              "The user will show you an script that is having issues. Describe the issues the script is having " +
                              "and why. Provide a solution if possible and explain why it is a good solution.:"
                },
                new ChatCompletionMessage
                {
                    role = "user",
                    content = content
                }
            };
        }
    }
}