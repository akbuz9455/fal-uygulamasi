using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    internal class IterativeCoder : BaseConversation<CodingSession, SerializableConversations<CodingSession>>
    {
        private const string CodingSaveKeyName = "ChatGPT_CodingSessions";
        public virtual string ObjectName => "Script";

        protected override string SaveKeyName => CodingSaveKeyName;
 
        private Vector2 scrollPosition;
        private string newMessage = "";
        private string _componentPrompt;
        private bool _disableComponentPrompt;
        protected CodeMode _mode;
        protected string _editComponentDefaultScriptPath;
        private Vector2 _scrollViewEdit;
        private CachedValue<List<DiffLine>> _diffValue = new ();
        private bool ShowHistory => _mode == CodeMode.FullEditor;
        private string[] CompilationErrors { get; set; }
        protected virtual bool ShowCompilationErrors => true;
        private string _lastCompiledSource;

        public override void OnGUI()
        {
            WrapIndex();
            GUI.enabled = !_isWaiting;
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.Space();

            switch (_mode)
            {
                case CodeMode.FullEditor:
                    ShowNewSessionButton();
                    break;
                case CodeMode.EditComponentScript:
                    ShowEditComponentSection();
                    break;
                case CodeMode.AddComponent:
                    ShowAddComponentSection();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_mode == CodeMode.FullEditor || (_mode == CodeMode.EditComponentScript || _mode == CodeMode.AddComponent) && _disableComponentPrompt)
            {
                
                if (conversations.Count == 0)
                    EditorGUILayout.HelpBox("No coding sessions found. Click the 'New Session' button to create one.",
                        MessageType.Info);
                else
                    RenderCodingSessions();

                if (_isWaiting)
                {
                    EditorGUILayout.Space();
                    EditorGUI.ProgressBar(GUILayoutUtility.GetRect(18, 25), (float)_progress, "Waiting...");
                }
            }

            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            
            if (_isWaiting && GUILayout.Button("Cancel", GUILayout.ExpandWidth(true)))
            {
                _cancellationTokenSource?.Cancel();
                SelectedConversation.CancelLast();
                _isWaiting = false;
                GUIUtility.keyboardControl = 0;
                OnCancel();
                Repaint();
            }

            if (!string.IsNullOrEmpty(_error))
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
            }
        }
        
        private void ShowNewSessionButton()
        {
            if (GUILayout.Button("New Session", GUILayout.ExpandWidth(true), GUILayout.Height(25)))
            {
                var manageSessionsWindow = CreateInstance<NewCodingSessionWindow>();
                manageSessionsWindow.ShowUtility();
                manageSessionsWindow.Initialize(conversations, this);
            }
            EditorGUILayout.Space();
        }

        private void ShowEditComponentSection()
        {
            if (_disableComponentPrompt) return;
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2-1));
            EditorGUILayout.LabelField($"Script '{_editComponentDefaultScriptPath}' code:", EditorStyles.wordWrappedLabel);
            var source = File.ReadAllText(_editComponentDefaultScriptPath);
            
            _scrollViewEdit = EditorGUILayout.BeginScrollView(_scrollViewEdit);
            RenderCodeMessage(source, source, false);
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2-1), GUILayout.Height(position.height-10));
            ShowComponentSection("Say the changes you need. You will have a chance to review them after:", "Edit component", _editComponentDefaultScriptPath);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void ShowAddComponentSection()
        {
            if (_disableComponentPrompt) return;
            ShowComponentSection("Describe the component:", "Generate Component", null);
        }

        private void ShowComponentSection(string label, string buttonLabel, string scriptPath)
        {
            EditorGUILayout.LabelField(label, EditorStyles.wordWrappedLabel);
            var style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
            _componentPrompt = EditorGUILayout.TextArea(_componentPrompt, style, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (GUILayout.Button(buttonLabel, GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                StartNewCodingSession(scriptPath, _componentPrompt);
                _disableComponentPrompt = true;
                Repaint();
            }
        }
        
        protected virtual void OnCancel()
        {
        }
        
        private void RenderCodingSessions()
        {
            
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            if (_mode == CodeMode.FullEditor)
            {
                GUILayout.Label("Coding Sessions:", EditorStyles.toolbarButton, GUILayout.Width(120));

                selectedConversationIndex = EditorGUILayout.Popup(
                    selectedConversationIndex,
                    conversations.Select(C => C.Name).ToArray(),
                    EditorStyles.toolbarPopup,
                    GUILayout.ExpandWidth(true)
                );

                if (conversations.Count > 0)
                {

                    if (GUILayout.Button("Delete", GUILayout.Width(50)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Session",
                                $"Are you sure you want to delete the '{SelectedConversation.Name}' session?",
                                "Delete", "Cancel"))
                        {
                            DeleteConversation(selectedConversationIndex);
                        }
                    }

                    if (!string.IsNullOrEmpty(SelectedConversation.ScriptPath) && File.Exists(SelectedConversation.ScriptPath) &&
                        GUILayout.Button("\u21BB Reload Source", GUILayout.Width(120)))
                    {
                        SelectedConversation.ReloadCodeFromSource();
                        Save();
                        Debug.Log($"Reload from {SelectedConversation.ScriptPath}");
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            // Show messages in the selected conversation
            if (conversations.Count > 0)
            {
                var renderModificationButtons = false;
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                if (ShowHistory) EditorGUILayout.LabelField("History", EditorStyles.boldLabel);

                EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                SetGUIEnabledState(true);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                SetGUIEnabledState();
                
                var count = SelectedConversation.Messages.Count;
                for (var i = (ShowHistory ? 1 : count -1); i < count; i++)
                {
                    var isLast = i == count - 1;
                    var message = SelectedConversation.Messages[i];

                    if (message.content.StartsWith("|NEW_SCRIPT|"))
                    {
                        string prompt = Messages.ParseNewScriptMessage(message.content);
                        RenderNewScriptMessage(prompt);
                    }
                    else if (message.content.StartsWith("|EDIT_SCRIPT|"))
                    {
                        string changes;
                        string _ = Messages.ParseEditScriptMessage(message.content, out changes);
                        RenderEditScriptMessage(changes);
                    }
                    else if (IsCodeMessage(message.content))
                    {
                        if (!isLast) continue;
                        renderModificationButtons = isLast;
                        RenderCodeMessage(
                            SelectedConversation.Versions[SelectedConversation.Versions.Count-2].code,
                            SelectedConversation.LastCodeVersion, 
                            isLast,
                            SelectedConversation.IsLastCodeVersionPartial
                        );
                    }
                    else
                    {
                        RenderMessage(message.role, message.content);
                    }
                }
                SetGUIEnabledState(true);
                EditorGUILayout.EndScrollView();
                SetGUIEnabledState();
                if (renderModificationButtons)
                    RenderModificationRequestButtons();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
        }

        internal string CreatePrompt(ScriptActionType action, string prompt, string source = null)
        {
            return action switch
            {
                ScriptActionType.NewScript => Messages.NewScriptMessage(prompt),
                ScriptActionType.EditScript => Messages.EditScriptMessage(source, prompt),
                _ => throw new ArgumentException()
            };
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

        internal void StartNewCodingSession(string scriptPath, string prompt)
        {
            var filename = scriptPath != null ? $"Edit {Path.GetFileName(scriptPath)}" : $"New {ObjectName} '{prompt}'";
            var source = scriptPath != null ? File.ReadAllText(scriptPath) : null;
            var newSession = new CodingSession
            {
                Versions = new List<CodeVersion>
                {
                    new ()
                    {
                        code = source ?? string.Empty
                    }
                },
                ScriptPath = scriptPath,
                Name = filename,
                messages = new List<Message>
                {
                    new("user", GetBasePrompt()),
                    new("user", CreatePrompt(
                        scriptPath != null ? IterativeCoder.ScriptActionType.EditScript : IterativeCoder.ScriptActionType.NewScript,
                        prompt,
                        source
                    ))
                }
            };

            conversations.Add(newSession);
            SelectNewSession(conversations.Count - 1);
            UpdateGPTResponse();
        }
        
        public void SelectNewSession(int index)
        {
            selectedConversationIndex = index;
            Save();
        }

        public override void Reload()
        {
            LoadConversations();
        }
        
        public override void OnEnable()
        {
            LoadConversations();
        }

        private bool IsCodeMessage(string messageContent)
        {
            return messageContent.StartsWith("|CODE|");
        }

        private void RenderNewScriptMessage(string prompt)
        {
            string messageText = $"<color=#007ACC><b>New {ObjectName}:</b></color> {prompt}";
            RenderMessage("user", messageText, false);
        }

        private void RenderEditScriptMessage(string changes)
        {
            string messageText = $"<color=#007ACC><b>Requested changes:</b></color> '{changes}' on {ObjectName} {SelectedConversation.ScriptPath}";
            RenderMessage("user", messageText);
        }

        private void ShowDiff(string oldCode, string newCode, bool isPartial)
        {
            var diffLines = !isPartial
                ? _diffValue.Get(() => DiffGenerator.GenerateDiff(oldCode, newCode), oldCode, newCode)
                : new List<DiffLine>
                {
                    new ()
                    {
                        Content = newCode,
                        Type = DiffLine.LineType.Unchanged
                    }
                };
            GUIStyle diffStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                richText = true,
            };

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            {
                foreach (var line in diffLines)
                {
                    var lineContent = line.Content;

                    lineContent = line.Type switch
                    {
                        DiffLine.LineType.Added => $"<color=#B9EAB9FF>+ {lineContent}</color>",
                        DiffLine.LineType.Deleted => $"<color=#FF8484FF>- {lineContent}</color>",
                        _ => LeastSquaresGUI.ApplySyntaxHighlighting(lineContent)
                    };

                    EditorGUILayout.LabelField(lineContent, diffStyle);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void AcceptChanges()
        {
            var nameWasNull = false;
            try
            {
                if (string.IsNullOrEmpty(SelectedConversation.ScriptPath))
                {
                    var name = ExtractName(SelectedConversation.LastCodeVersion);
                    SelectedConversation.ScriptPath = Paths.GetPathInScriptsFolder(name);
                    nameWasNull = true;
                }

                Debug.Log(SelectedConversation.ScriptPath);

                if (EditorUtility.DisplayDialog(
                        "Save File",
                        $"Are you want to save the {ObjectName} to '{SelectedConversation.ScriptPath}'? This might overwrite the existing file.",
                        "Save",
                        "Cancel"
                    ))
                {
                    OnAcceptChanges(SelectedConversation);

                    Debug.Log("Accepted changes and saved the script.");
                }
            }
            finally
            {
                if (nameWasNull)
                    SelectedConversation.ScriptPath = null;
            }
        }
        
        private void RevertChanges()
        {
            if (EditorUtility.DisplayDialog(
                    "Revert changes", 
                    $"Are you want to revert the last changes? You can always re-ask the AI", 
                    "Revert", 
                    "Cancel"
                ))
            {
                SelectedConversation.Pop();
                Save();
            }
        }
        
        protected virtual string ExtractName(string classContent)
        {
            const string regex = @"\bclass\s+(\w+)\b";
            var groups = Regex.Match(classContent, regex).Groups;
            if (groups.Count <= 1) return "Default.cs";
            return $"{groups[1].Value}.cs";
        }

        protected virtual void OnAcceptChanges(CodingSession codingSession)
        {
            // Save the updated script to the chosen path
            File.WriteAllText(codingSession.ScriptPath, codingSession.LastCodeVersion);
            AssetDatabase.Refresh();
        }
        
        private void FixErrors(string[] errors)
        {
            OnRequestedChangesReceived($"The script compiled with the following errors, Please edit the script and fix them. Remove conflicting code if you need to:\n{string.Join("\n", errors)}.");
        }

        private void AskForModifications()
        {
            ModificationRequestWindow.Init(OnRequestedChangesReceived);
        }

        private void OnRequestedChangesReceived(string requestedChanges)
        {
            if (!string.IsNullOrEmpty(requestedChanges))
            {
                SelectedConversation.AddMessage("user", Messages.EditScriptMessage(SelectedConversation.LastCodeVersion, requestedChanges) );
                UpdateGPTResponse();

                Debug.Log("Asking for modifications: " + requestedChanges);
            }
            else
            {
                Debug.LogWarning("Modification request was canceled.");
            }
        }
        
        private void RenderCodeMessage(string oldCode, string newCode, bool showControls = true, bool isPartial = false)
        {
            EditorGUILayout.BeginVertical();
            {
                if (showControls)
                    EditorGUILayout.LabelField("Last code changes:", EditorStyles.boldLabel);
                ShowDiff(oldCode, newCode, isPartial);
                RenderExtraPreview();
            }
            EditorGUILayout.EndVertical();
        }

        private void CompileIfNecessary(bool force = false)
        {
            if (!ShowCompilationErrors) return;
            
            var isFinal = !SelectedConversation.IsLastCodeVersionPartial;
            if (!isFinal && !force) return;
            
            var newSource = SelectedConversation.LastCodeVersion;
            if (newSource == _lastCompiledSource && !force) return;
            
            var errors = RuntimeCompiler.CheckForCompilationErrors(newSource);
            CompilationErrors = errors?.Select(E => E.ToString()).ToArray();
            Debug.Log($"Compiled with {CompilationErrors?.Length ?? 0} errors");
            _lastCompiledSource = newSource;
        }

        private void RenderModificationRequestButtons()
        {
            CompileIfNecessary();
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (CompilationErrors is { Length: > 0 } && !_isWaiting)
            {
                Color originalBackgroundColor;
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Compilation errors detected:", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                var totalErrors = CompilationErrors.Length;
                var minShow = 2;
                for (var i = 0; i < Math.Min(totalErrors, minShow); i++)
                {
                    var error = CompilationErrors[i];
                    originalBackgroundColor = GUI.contentColor;
                    GUI.contentColor = new Color(1f, 0.517f, 0.517f, 1f);
                    GUILayout.Label($"\u2022 {error}");
                    GUI.contentColor = originalBackgroundColor;
                }

                if (CompilationErrors.Length > minShow)
                {
                    originalBackgroundColor = GUI.contentColor;
                    GUI.contentColor = new Color(1f, 0.517f, 0.517f, 1f);
                    GUILayout.Label($"and {totalErrors - minShow} more errors...");
                    GUI.contentColor = originalBackgroundColor;
                }
                
                EditorGUILayout.Space();
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                originalBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1.0f, 0.7f, 0.4f);//new Color(1f, 0.517f, 0.517f, 1f);
                if (GUILayout.Button("\u2192 Ask to fix errors", GUILayout.Width(160), GUILayout.Height(30)))
                {
                    FixErrors(CompilationErrors);
                }
                GUI.backgroundColor = originalBackgroundColor;
                
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                Color originalBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.725f, 0.925f, 0.725f, 1f);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("\u2713 Accept changes", GUILayout.Width(160), GUILayout.Height(30)))
                {
                    AcceptChanges();
                }

                GUI.backgroundColor = new Color(1f, 0.929f, 0.678f, 1f);
                if (GUILayout.Button("\u2717 Ask for modifications", GUILayout.Width(160), GUILayout.Height(30)))
                {
                    AskForModifications();
                }
                
                if (SelectedConversation.messages.Count > 3)
                {
                    GUI.backgroundColor = new Color(1f, 0.517f, 0.517f, 1f);
                    if (GUILayout.Button("\u20E0  Revert changes", GUILayout.Width(160), GUILayout.Height(30)))
                    {
                        RevertChanges();
                    }
                }

                GUILayout.FlexibleSpace();
                GUI.backgroundColor = originalBackgroundColor;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RenderMessage(string role, string content, bool bold = true)
        {
            GUIStyle messageStyle = new GUIStyle(role == "user" && bold ? EditorStyles.boldLabel : EditorStyles.label)
            {
                wordWrap = true,
                richText = true,
            };

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = role == "user" ? new Color(0.75f, 0.75f, 1) : new Color(1, 0.75f, 0.75f);
            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = originalColor;
            Rect textAreaRect = GUILayoutUtility.GetRect(new GUIContent(content), messageStyle, GUILayout.ExpandWidth(true));
            EditorGUI.SelectableLabel(textAreaRect, content, messageStyle);

            // Handle mouse events
            if (textAreaRect.Contains(Event.current.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(textAreaRect, MouseCursor.Text);
                if (Event.current.type == EventType.MouseDown)
                {
                    EditorGUIUtility.systemCopyBuffer = content;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        public virtual void SetMode(CodeMode Mode)
        {
            _mode = Mode;
        }

        internal enum ScriptActionType
        {
            NewScript,
            EditScript
        }

        protected virtual string PostProcessCode(string code)
        {
            return CodeCleaner.CleanExtraCommentsAndQuotes(code);
        }

        protected virtual void RenderExtraPreview()
        {
            
        }
        
        protected override void OnSuccessRequest(CodingSession selectedConversation, string chatbotResponse)
        {
            var preSource = Messages.ParseCodeScriptMessage(chatbotResponse);
            var newSource = PostProcessCode(preSource);
            selectedConversation.AddOrReplaceCodeVersion(newSource, true);
            selectedConversation.AddOrReplacePartialMessage("assistant", chatbotResponse, true);
            Save();
        }

        protected override void OnStreamingProgress(CodingSession selectedConversation, string text)
        { 
            var preSource = Messages.ParseCodeScriptMessage(text);
            var newSource = PostProcessCode(preSource);
            selectedConversation.AddOrReplaceCodeVersion(newSource);
            selectedConversation.AddOrReplacePartialMessage("assistant", text);
        }

        protected override ChatCompletionMessage[] BuildMessageList(CodingSession conversation)
        {
            var messages = conversation.messages;
            return new []{messages[0], messages[messages.Count-1]};
        }

        protected virtual string GetBasePrompt() => NewCodingSessionWindow.GetBaseScriptPrompt();
    }
}