using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class AIWriter : AIEditorTool
    {
        private enum ToolFunction
        {
            LoreWriter,
            NameGenerator,
            DescriptionGenerator
        }

        private ToolFunction _currentFunction = ToolFunction.LoreWriter;
        private Dictionary<ToolFunction, string> _prompts;
        private Dictionary<ToolFunction, string> _answers;
        private int _numNames = 1;

        public AIWriter()
        {
            _prompts = new Dictionary<ToolFunction, string>
            {
                { ToolFunction.DescriptionGenerator , string.Empty},
                { ToolFunction.LoreWriter , string.Empty},
                { ToolFunction.NameGenerator , string.Empty},
            };
            
            _answers = new Dictionary<ToolFunction, string>
            {
                { ToolFunction.DescriptionGenerator , string.Empty},
                { ToolFunction.LoreWriter , string.Empty},
                { ToolFunction.NameGenerator , string.Empty},
            };
        }

        protected override void OnStartGUI()
        {
            var prev = _currentFunction;
            _currentFunction = (ToolFunction)GUILayout.Toolbar((int)_currentFunction,
                new string[] { "Lore Writer", "Name Generator", "Description Generator" });

            if (_currentFunction != prev)
                GUIUtility.keyboardControl = 0;
            
            SwitchAndSave(prev);
            switch (_currentFunction)
            {
                case ToolFunction.LoreWriter:
                    GUILayout.Label("Describe to the AI the character you want to write lore for:");
                    break;
                case ToolFunction.NameGenerator:
                    GUILayout.Label("Number of names to generate:");
                    _numNames = EditorGUILayout.IntField(_numNames);
                    GUILayout.Label("Briefly describe attributes the name must have:");
                    break;
                case ToolFunction.DescriptionGenerator:
                    GUILayout.Label("Briefly describe the attributes of the item or object:");
                    break;
            }
        }

        private void SwitchAndSave(ToolFunction previous)
        {
            if (previous == _currentFunction) return;
            _prompts[previous] = _prompt;
            _answers[previous] = _answer;
                    
            _prompt = _prompts[_currentFunction];
            _answer = _answers[_currentFunction];
        }

        protected override ChatCompletionMessage[] CreatePrompt(string prompt)
        {
            switch (_currentFunction)
            {
                case ToolFunction.LoreWriter:
                    return new[]
                    {
                        new ChatCompletionMessage
                        {
                            role = "system",
                            content =
                                "You are an expert at writing lore for videogames. You are writing lore for a character in a videogame. Describe the character and write lore for them. Provide all the context you can when possible "
                        },
                        new ChatCompletionMessage
                        {
                            role = "user",
                            content = prompt
                        }
                    };

                case ToolFunction.NameGenerator:
                    return new[]
                    {
                        new ChatCompletionMessage
                        {
                            role = "system",
                            content =
                                $"You are an expert at naming things. You are naming a character in a videogame. Please provide {_numNames} names that best suit the following prompt"
                        },
                        new ChatCompletionMessage
                        {
                            role = "user",
                            content = prompt
                        }
                    };

                case ToolFunction.DescriptionGenerator:
                    return new[]
                    {
                        new ChatCompletionMessage
                        {
                            role = "system",
                            content =
                                "You are an expert writer. You are writing a description for an object. Please provide a description that best suits the following prompt"
                        },
                        new ChatCompletionMessage
                        {
                            role = "user",
                            content = prompt
                        }
                    };

                default:
                    throw new System.InvalidOperationException("Invalid tool function selected.");
            }
        }
    }
}