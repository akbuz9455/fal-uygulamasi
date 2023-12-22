using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LeastSquares.Spark
{
    
    public class AIConversation : BaseConversation<Conversation, SerializableConversations<Conversation>>
    {
        private Vector2 scrollPosition;
        private string newMessage = string.Empty;
        protected override string SaveKeyName => "ChatGPT_Conversations";

        public override void OnGUI()
        {
            WrapIndex();
            GUI.enabled = !_isWaiting;
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            // Button to manage conversations
            if (GUILayout.Button("Add new conversation", GUILayout.ExpandWidth(true)))
            {
                var manageConversationsWindow = CreateInstance<AddConversationWindow>();
                manageConversationsWindow.ShowAuxWindow();
                manageConversationsWindow.Initialize(conversations, this);
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUILayout.Label("Conversations:", EditorStyles.toolbarButton, GUILayout.Width(100));
            
            selectedConversationIndex = EditorGUILayout.Popup(
                selectedConversationIndex, 
                conversations.Select(C => C.Name).ToArray(),
                EditorStyles.toolbarPopup,
                GUILayout.ExpandWidth(true)
            );
            
            if (conversations.Count > 0 && GUILayout.Button("Delete", GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("Delete Conversation", $"Are you sure you want to delete '{conversations[selectedConversationIndex].Name}' conversation?", "Delete", "Cancel"))
                {
                    DeleteConversation(selectedConversationIndex);
                }
            }

            EditorGUILayout.EndHorizontal();

            // Show messages in the selected conversation
            if (conversations.Count > 0)
            {
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                EditorGUILayout.LabelField("Messages", EditorStyles.boldLabel);

                EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                SetGUIEnabledState(true);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                SetGUIEnabledState();
                
                Conversation selectedConversation = conversations[selectedConversationIndex];
                for (int i = 0; i < selectedConversation.Messages.Count; i++)
                {
                    Message message = selectedConversation.Messages[i];
                    GUIStyle messageStyle = new GUIStyle(message.role == "user" ? EditorStyles.boldLabel : EditorStyles.label)
                    {
                        wordWrap = true,
                        richText = true,
                    };

                    Color originalColor = GUI.backgroundColor;
                    GUI.backgroundColor = message.role == "user" ? new Color(0.75f, 0.75f, 1) : new Color(1, 0.75f, 0.75f);
                    EditorGUILayout.BeginVertical("box");
                    GUI.backgroundColor = originalColor;
                    Rect textAreaRect = GUILayoutUtility.GetRect(new GUIContent("<b>" + MapRoleToName(message.role) + "</b>: " + message.content), messageStyle, GUILayout.ExpandWidth(true));
                    EditorGUI.SelectableLabel(textAreaRect, "<b>" + MapRoleToName(message.role) + "</b>: " + message.content, messageStyle);

                    // Handle mouse events
                    if (textAreaRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUIUtility.AddCursorRect(textAreaRect, MouseCursor.Text);
                        if (Event.current.type == EventType.MouseDown)
                        {
                            EditorGUIUtility.systemCopyBuffer = "<b>" + MapRoleToName(message.role) + "</b>: " + message.content;
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                SetGUIEnabledState(true);
                EditorGUILayout.EndScrollView();
                SetGUIEnabledState();
                EditorGUILayout.EndVertical();

                // Textbox to send a new message
                EditorGUILayout.LabelField("New Message");
                newMessage = EditorGUILayout.TextArea(newMessage, GUILayout.ExpandWidth(true), GUILayout.Height(100));
                if (GUILayout.Button("Send Message", GUILayout.ExpandWidth(true)) && !string.IsNullOrEmpty(newMessage))
                {
                    selectedConversation.AddMessage("user", newMessage);
                    UpdateGPTResponse();
                    newMessage = "";
                    GUIUtility.keyboardControl = 0;
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("No conversations found. Click the 'Add New Conversation' button to create one.",
                        MessageType.Info);
            }

            EditorGUILayout.EndVertical();
            GUI.enabled = true;

            if (_isWaiting && GUILayout.Button("Cancel", GUILayout.ExpandWidth(true)))
            {
                _cancellationTokenSource?.Cancel();
                var convo = conversations[selectedConversationIndex];
                convo.CancelLast();
                _isWaiting = false;
                GUIUtility.keyboardControl = 0;
                Repaint();
            }
            
            if (!string.IsNullOrEmpty(_error))
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
            }
        }

        private string MapRoleToName(string role)
        {
            return $"{role[0].ToString().ToUpperInvariant()}{role.Substring(1)}";
        }

        private void DeleteConversation(int index)
        {
            conversations.RemoveAt(index);
            WrapIndex();
            Repaint();
            Save();
        }

        protected override ChatCompletionMessage[] BuildMessageList(Conversation conversation)
        {
            return conversation.Messages.ToArray();
        }

        protected override void OnSuccessRequest(Conversation selectedConversation, string chatbotResponse)
        {
            selectedConversation.AddOrReplacePartialMessage("assistant", chatbotResponse, true);
            Save();
        }

        protected override void OnStreamingProgress(Conversation selectedConversation, string text)
        {
            selectedConversation.AddOrReplacePartialMessage("assistant", text);
        }

        public void SelectNewConversation(int index)
        {
            selectedConversationIndex = index;
            Save();
        }

        public override void Reload()
        {
            LoadConversations();
        }
        
        void OnEnable()
        {
            LoadConversations();
        }
    }
}