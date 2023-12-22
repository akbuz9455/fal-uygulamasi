using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class AddConversationWindow : EditorWindow
    {
        private List<Conversation> conversations;
        private AIConversation _aiConversation;
        private string newConversationName = "";
        private string customPrompt = "";

        private int selectedPromptIndex = 0;
        private Dictionary<string, string> prompts = new Dictionary<string, string>
        {
            { "Unity Developer", "You are a senior Unity 3D developer expert, ready to answer all questions and teach. Do not break character" },
            { "Gamer or Potential Customer", "You are a gamer, answer any questions and do not hesitate to talk about your preferences. Talking to you is a game developer. Do not break character" },
            { "Game Critic", "You are a game critic. Debate and provide feedback about the different game ideas and features. Critique. Do not break character" },
            { "Executive", "You an executive who is about to get pitched a game that is currently under development. Do not break character" },
            { "ChatGPT", "" },
            { "Prompt Engineer", GetPromptEngineerPrompt()}
        };

        private string[] predefinedPrompts;

        public void Initialize(List<Conversation> conversations, AIConversation aiConversation)
        {
            this.conversations = conversations;
            this._aiConversation = aiConversation;
            predefinedPrompts = prompts.Keys.Concat(new[] { "Custom Prompt" }).ToArray();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            // Create new conversation
            CreateNewConversation();

            EditorGUILayout.EndVertical();
        }

        private void CreateNewConversation()
        {
            EditorGUILayout.LabelField("Create New Conversation", EditorStyles.boldLabel);

            newConversationName = EditorGUILayout.TextField("Name", newConversationName);

            selectedPromptIndex = EditorGUILayout.Popup("Select Chat Role", selectedPromptIndex, predefinedPrompts);

            if (predefinedPrompts[selectedPromptIndex] == "Custom Prompt")
            {
                customPrompt = EditorGUILayout.TextField("Custom Prompt", customPrompt);
            }

            if (GUILayout.Button("Create", GUILayout.ExpandWidth(true)))
            {
                Conversation newConversation = new Conversation
                {
                    Name = newConversationName
                };

                string selectedPrompt = predefinedPrompts[selectedPromptIndex];

                if (selectedPrompt == "Custom Prompt")
                {
                    newConversation.AddMessage("system", customPrompt);
                }
                else
                {
                    newConversation.AddMessage("system", prompts[selectedPrompt]);
                }

                conversations.Add(newConversation);
                _aiConversation.SelectNewConversation(conversations.Count - 1);
                CloseWindow();
            }
        }

        private void CloseWindow()
        {
            newConversationName = "";
            customPrompt = "";
            _aiConversation.Repaint();
            Close();
        }

        private static string GetPromptEngineerPrompt()
        {
            return @"
I want you to become my Prompt engineer. Your goal is to help me craft the best possible prompt for my needs. The prompt will be used by you, ChatGPT. You will follow the
following process: 1. Your first response will be to ask me what the prompt should be about. I will provide my answer, but we will need to improve it through continual iterations by going through the
next steps. 2. Based on my input, you will generate 2 sections. a) Revised prompt (provide your rewritten prompt. it should be clear, concise, and easily understood by you), b) Questions (ask any relevant questions pertaining to what additional information is needed from me to
improve the prompt). 3. We will continue this iterative process with me providing additional information to you
and you updating the prompt in the Revised prompt section until I say we are done.
";
        }
    }
}