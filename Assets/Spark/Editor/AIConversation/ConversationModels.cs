using System;
using System.Collections.Generic;
using UnityEngine;

namespace LeastSquares.Spark
{

    [Serializable]
    public class SerializableConversations<T> : ISerializableConversation<T>
    {
        [SerializeField]
        private List<T> conversations;

        public SerializableConversations(List<T> conversations)
        {
            this.conversations = conversations;
        }

        public List<T> ToConversationList()
        {
            return conversations;
        }
    }

    [Serializable]
    public class Conversation
    {
        [SerializeField]
        private string name;
        public string Name
        {
            get => string.IsNullOrEmpty(name) ? "Unnamed" : name;
            set => name = value;
        }

        [SerializeField] 
        public List<Message> messages = new();
        public List<Message> Messages => messages;

        public void AddMessage(string sender, string content, bool isPartial = false)
        {
            messages.Add(new Message(sender, content, isPartial));
        }

        public void AddOrReplacePartialMessage(string sender, string content, bool isFinal = false)
        {
            if (Messages.Count > 0 && Messages[Messages.Count - 1].isPartial)
                Messages.RemoveAt(Messages.Count - 1);
            
            AddMessage(sender, content, !isFinal);
        }

        public virtual void Migrate()
        {
            
        }

        public virtual void CancelLast()
        {
            if (messages[messages.Count - 1].isPartial)
            {
                messages.RemoveAt(messages.Count - 1);
            }
            messages.RemoveAt(messages.Count - 1);
        }
    }

    [Serializable]
    public class Message : ChatCompletionMessage
    {
        public bool isPartial;
        public Message(string senderRole, string messageContent, bool isPartial = false)
        {
            role = senderRole;
            content = messageContent;
            this.isPartial = isPartial;
        }
    }
}