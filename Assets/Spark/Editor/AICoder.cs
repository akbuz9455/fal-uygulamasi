using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    /// <summary>
    /// A class that represents the AI Engineer tool window
    /// </summary>
    public class AICoder : AIEditorTool
    {
        protected bool _outputOnlyCode = true;
        protected bool _highlightCode = true;
        protected bool _existingScriptModified;
        private TextAsset _existingScript = null;

        protected enum ActionMode
        {
            CreateNewScript,
            UpdateExistingScript,
        }

        protected ActionMode _actionMode;

        protected override bool RenderAnswerAsCode => _highlightCode;

        /// <summary>
        /// Creates the prompt for the AI
        /// </summary>
        /// <param name="Prompt">The prompt to be used</param>
        /// <returns>The prompt for the AI</returns>
        protected override ChatCompletionMessage[] CreatePrompt(string prompt)
        {
            var outputOnlyCode = _outputOnlyCode
                ? "Only return the full C# class with imports and with no formatting nor explanations. Do not write any explanations."
                : string.Empty;
            var msg = _actionMode == ActionMode.CreateNewScript ? new ChatCompletionMessage
            {
                role = "user",
                content =
                    $"The prompt to write a script for is:\n```\n{prompt}\n```.\n Return the full code for the unity script that best fulfills the prompt. {outputOnlyCode}"
            }
            : new ChatCompletionMessage
            {
                role = "user",
                content =
                    $"Edit this script:\n```\n{_existingScript.text}\n```.According to the following instructions:\n```\n{prompt}\n```\n. Return the full code for the unity script that best fulfills the prompt. {outputOnlyCode}"
            };
            
            return new[]
            {
                new ChatCompletionMessage
                {
                    role = "system",
                    content =
                        "You are a senior Unity 3D programmer and C# programmer. A user will provide a prompt and you will write a script that best fulfills the prompt." +
                        " You can use the Unity API to help you write the script." +
                        outputOnlyCode
                },
                msg
            };
        }

        private void SaveNewScript()
        {
            var fileName = ExtractName(_answer);
            if (GUILayout.Button($"Save as {fileName}"))
            {
                File.WriteAllText(Paths.GetPathInScriptsFolder(fileName), _answer);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", "Saved successfully", "OK");
            }
        }

        private void UpdateExistingScriptGUIStart()
        {
            EditorGUILayout.HelpBox("Select an existing script to update with the AI-generated code.",
                MessageType.Info);
            _existingScript = EditorGUILayout.ObjectField("Script", _existingScript, typeof(TextAsset), false) as TextAsset;
        }

        private void UpdateExistingScriptGUIEnd()
        {
            if (_existingScript != null)
            {
                if (GUILayout.Button("Update Script"))
                {

                    // Confirm the user wants to overwrite the existing script
                    if (EditorUtility.DisplayDialog("Warning",
                            "Are you sure you want to overwrite the existing script with the AI-generated code? This action cannot be undone.",
                            "Yes, overwrite",
                            "No, cancel"))
                    {
                        File.WriteAllText(AssetDatabase.GetAssetPath(_existingScript), _answer);
                        AssetDatabase.Refresh();
                        EditorUtility.DisplayDialog("Success", "Script updated successfully", "OK");
                    }
                }
            }
        }

        /// <summary>
        /// Additional settings GUI code to be executed
        /// </summary>
        protected override void AdditionalSettingsGUI()
        {
            _outputOnlyCode = EditorGUILayout.ToggleLeft("Output only code", _outputOnlyCode);
            _highlightCode = EditorGUILayout.ToggleLeft("Highlight code", _highlightCode);
        }

        /// <summary>
        /// Extracts the name of the class from the code
        /// </summary>
        /// <param name="classContent">The content of the class</param>
        /// <returns>The name of the class</returns>
        private string ExtractName(string classContent)
        {
            const string regex = @"\bclass\s+(\w+)\b";
            var groups = Regex.Match(classContent, regex).Groups;
            if (groups.Count <= 1) return "Default.cs";
            return $"{groups[1].Value}.cs";
        }

        /// <summary>
        /// GUI code to be executed at the start
        /// </summary>
        protected override void OnStartGUI()
        {
            GUILayout.Label("Describe to the AI the script you want to write:");
            EditorGUILayout.Separator();

            _actionMode = (ActionMode)EditorGUILayout.EnumPopup("Action Mode", _actionMode);

            switch (_actionMode)
            {
                case ActionMode.CreateNewScript:
                    break;
                case ActionMode.UpdateExistingScript:
                    UpdateExistingScriptGUIStart();
                    break;
            }
        }

        protected override void OnEndGUI()
        {
            if (_answer != null)
            {
                EditorGUILayout.Separator();

                switch (_actionMode)
                {
                    case ActionMode.CreateNewScript:
                        SaveNewScript();
                        break;
                    case ActionMode.UpdateExistingScript:
                        UpdateExistingScriptGUIEnd();
                        break;
                }
            }
        }

        protected override string ProcessOutput(string output)
        {
            if (!_outputOnlyCode) return output;

            var trimmed = output.Trim();
            if (trimmed.Length == 0) return trimmed;
            if (trimmed[0] == '`')
                return output
                    .Replace("```csharp\n", string.Empty)
                    .Replace("```\n", string.Empty)
                    .Replace("\n```", string.Empty);
            return output;
        }
    }
}