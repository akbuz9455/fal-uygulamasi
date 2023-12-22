using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    /// <summary>
    /// This class is used to create a GPT3Tool EditorWindow.
    /// </summary>
    public abstract class AIEditorTool : SerializableEditorTool
    {
        protected string _prompt;
        protected string _answer;
        protected string _error;

        protected bool _isWaiting;
        private double _progress;
        private float _waitTime;
        private double _lastTime;
        private Vector2 _scrollPositionAnswer;
        private Vector2 _scrollPositionPrompt;
        private CancellationTokenSource _cancellationTokenSource;

        protected virtual bool RenderAnswerAsCode => false;

        /// <summary>
        /// Boolean to check if the prompt should be hidden.
        /// </summary>
        protected virtual bool HidePrompt => false;

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events.
        /// </summary>
        public override void OnGUI()
        {
            EditorGUI.indentLevel++;

            SetGUIEnabledState();
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.Space();

                // Add a title to the window
                //EditorGUILayout.LabelField(WindowName, EditorStyles.largeLabel);
                
                OnStartGUI();

                EditorGUILayout.Space();

                if (!HidePrompt)
                {
                    EditorGUILayout.LabelField("Prompt", EditorStyles.boldLabel);
                    _scrollPositionPrompt = EditorGUILayout.BeginScrollView(_scrollPositionPrompt, GUILayout.Height(200));
                    GUI.SetNextControlName($"{this.GetType().Name}_Prompt");
                    var style = new GUIStyle(EditorStyles.textArea);
                    style.wordWrap = true;
                    _prompt = EditorGUILayout.TextArea(_prompt, style, GUILayout.ExpandHeight(true));
                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.Space();

                var dots = new string('.', (int)_waitTime + 1);
                var waitingMsg = $"Waiting {dots}";

                if (LeastSquaresGUI.CenteredButton("Generate", GUILayout.Height(25), GUILayout.MaxWidth(800)))
                {
                    if (!HidePrompt && string.IsNullOrEmpty(_prompt))
                    {
                        EditorUtility.DisplayDialog("Error", "Please write something into the prompt", "OK");
                    }
                    else
                    {
                        _error = null;
                        _ = RequestOutput(_prompt);
                    }
                }
                
                if (_isWaiting)
                {
                    EditorGUILayout.Space();
                    EditorGUI.ProgressBar(GUILayoutUtility.GetRect(18, 18), (float) _progress, waitingMsg);
                }

                EditorGUILayout.Space();
                if (_answer != null)
                {
                    SetGUIEnabledState(true);
                    EditorGUILayout.LabelField("AI Result", EditorStyles.boldLabel);

                    _scrollPositionAnswer =
                        EditorGUILayout.BeginScrollView(_scrollPositionAnswer, GUILayout.ExpandHeight(true));
                    SetGUIEnabledState();
                    if (RenderAnswerAsCode)
                    {
                        LeastSquaresGUI.RenderColoredCode(_answer);
                    }
                    else
                    {
                        var textAreaStyle = new GUIStyle(EditorStyles.textArea);
                        textAreaStyle.wordWrap = true;
                        EditorGUILayout.TextArea(_answer, textAreaStyle, GUILayout.ExpandHeight(true));
                    }

                    SetGUIEnabledState(true);
                    EditorGUILayout.EndScrollView();
                    SetGUIEnabledState();

                    EditorGUILayout.Space();
                }

                OnEndGUI();
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

                // Add your settings UI elements here
                EditorGUILayout.Space();

                OnSettingsGUI();

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();

            SetGUIEnabledState(true);
            
            if (_isWaiting && GUILayout.Button("Cancel", GUILayout.ExpandWidth(true)))
            {
                _cancellationTokenSource?.Cancel();
                _isWaiting = false;
            }
            
            if (!string.IsNullOrEmpty(_error))
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Called before OnGUI is called.
        /// </summary>
        protected virtual void OnStartGUI()
        {
        }

        /// <summary>
        /// Called after OnGUI is called.
        /// </summary>
        protected virtual void OnEndGUI()
        {
        }

        private void OnSettingsGUI()
        {
            AdditionalSettingsGUI();
            EditorGUILayout.LabelField("Check the general settings for more tool wide settings.");
        }

        protected virtual void AdditionalSettingsGUI()
        {
            
        }


        /// <summary>
        /// Requests output from the AI.
        /// </summary>
        /// <param name="Prompt">The prompt to be used for the AI.</param>
        protected async Task RequestOutput(string Prompt)
        {
            EditorApplication.update += Update;
            _isWaiting = true;
            _error = null;
            _cancellationTokenSource = new CancellationTokenSource();
            var source = _cancellationTokenSource;
            try
            {
                var prevAnswer = _answer;
                string answer = ProcessOutput(await OpenAIAccessManager.RequestChatCompletion(CreatePrompt(Prompt), new Progress<double>(P => _progress = P), source.Token,
                    (T) =>
                    {
                        if (!source.IsCancellationRequested)
                            _answer = T;
                        Repaint();
                    }));
                if (source.IsCancellationRequested)
                    return;
                _answer = answer;
            }
            catch (Exception e)
            {
                _error = e.Message;
                Debug.LogError(e);
                EditorUtility.DisplayDialog("Error", "An error ocurred while connecting to OpenAI. Please check the console and check the documentation for troubleshooting", "OK");
            }
            finally
            {
                _isWaiting = false;
                EditorApplication.update -= Update;
                Repaint();
            }
        }

        private void SetGUIEnabledState(bool force = false)
        {
            GUI.enabled = !_isWaiting || force;
        }
        
        protected virtual string ProcessOutput(string output)
        {
            return output;
        }

        /// <summary>
        /// Creates the prompt for the AI.
        /// </summary>
        /// <param name="Prompt">The prompt to be used for the AI.</param>
        /// <returns>The created prompt.</returns>
        protected abstract ChatCompletionMessage[] CreatePrompt(string Prompt);

        /// <summary>
        /// Updates the wait time for the AI.
        /// </summary>
        private void Update()
        {
            var prev = _waitTime;
            var time = EditorApplication.timeSinceStartup;
            _waitTime = (_waitTime + (float)(time - _lastTime)) % 3;
            _lastTime = time;
            if ((int)prev != (int)_waitTime)
                Repaint();
        }
    }
}