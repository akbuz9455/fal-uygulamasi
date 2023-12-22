using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace LeastSquares.Spark
{
    public interface ISerializableConversation<T>
    {
        List<T> ToConversationList();
    }
    
    public abstract class BaseConversation<T, J>: SerializableEditorTool where T : Conversation where J : ISerializableConversation<T>
    {
        protected bool _isWaiting;
        protected CancellationTokenSource _cancellationTokenSource;
        protected string _error;
        protected List<T> conversations = new ();
        protected int selectedConversationIndex;
        protected double _progress;
        protected abstract string SaveKeyName { get; }

        public T SelectedConversation
        {
            get
            {
                var convo = conversations[selectedConversationIndex];
                convo.Migrate();
                return convo;
            }
        }

        public void WrapIndex()
        {
            selectedConversationIndex = Mathf.Clamp(selectedConversationIndex, 0, conversations.Count - 1);
        }

        public async void UpdateGPTResponse()
        {
            _isWaiting = true;
            _error = null;
            _cancellationTokenSource = new CancellationTokenSource();
            var source = _cancellationTokenSource;
            try
            {
                var conversation = SelectedConversation;
                var messages = BuildMessageList(conversation);
                var chatbotResponse = await OpenAIAccessManager.RequestChatCompletion(
                    messages, 
                    new Progress<double>(P =>
                {
                    _progress = P;
                    Repaint();
                }), source.Token, T =>
                {
                    if (!source.IsCancellationRequested)
                        OnStreamingProgress(conversation, T);
                    Repaint();
                });
                
                if (source.IsCancellationRequested)
                {
                    Debug.Log("Request was cancelled.");
                    return;
                }

                OnSuccessRequest(conversation, chatbotResponse);
                Repaint();
            }
            catch (Exception e)
            {
                _error = e.Message;
                Debug.LogError(e);
            }
            finally
            {
                _isWaiting = false;
                Repaint();
            }
        }

        protected void SetGUIEnabledState(bool force = false)
        {
            GUI.enabled = !_isWaiting || force;
        }
        
        protected void Save()
        {
            var obj = (J)Activator.CreateInstance(typeof(J), conversations);
            var json = JsonUtility.ToJson(obj);
            SaveSystem.SetProjectString(SaveKeyName, json);
        } 
        
        protected void LoadConversations()
        {
            var json = SaveSystem.GetProjectString(SaveKeyName);
            if (!string.IsNullOrEmpty(json))
            {
                var serializableConversations = JsonUtility.FromJson<J>(json);
                conversations = serializableConversations.ToConversationList();
            }
        }

        protected abstract void OnStreamingProgress(T selectedConversation, string text);
        protected abstract ChatCompletionMessage[] BuildMessageList(T conversation);
        protected abstract void OnSuccessRequest(T conversation, string chatbotResponse);
    }
}