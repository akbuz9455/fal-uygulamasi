using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LeastSquares.Spark
{
    public abstract class BaseDialog : MonoBehaviour
    {
        /// <summary>
        /// The speed at which the dialogue text is written on screen.
        /// </summary>
        public float writeSpeed = 0.04f;
        
        /// <summary>
        /// The name of the character that the AI or NPC is acting as.
        /// </summary>
        public string characterName;
        
        /// <summary>
        /// The role that the AI or NPC is playing.
        /// </summary>
        public string actAs;
        
        /// <summary>
        /// An array of things that the AI or NPC can mention during the conversation.
        /// </summary>
        public string[] thingsToMention;
        
        /// <summary>
        /// The text object where the dialogue will be displayed.
        /// </summary>
        public TMP_Text dialogueText;
        
        protected List<ChatCompletionMessage> _messages = new ();

        private void Start()
        {
            if (!ChatEngine._loaded)
            {
                Debug.LogError("ChatEngine has not been loaded properly. Please ensure that the ChatEngine script is in the scene and that the auth file is set.");
                return;
            }
            
            _messages.Add(CreateStartingPrompt());
        }

        /// <summary>
        /// Sets the text of the dialogue text object and writes it on screen.
        /// </summary>
        /// <param name="text">The text to be displayed.</param>
        protected void SetText(string text)
        {
            StartCoroutine(WriteText(text));
        }
        
        /// <summary>
        /// Coroutine that writes the text on screen one character at a time.
        /// </summary>
        /// <param name="textToWrite">The text to be written.</param>
        /// <returns></returns>
        private IEnumerator WriteText(string textToWrite)
        {
            dialogueText.enableAutoSizing = false;

            var total = string.Empty;
            for (var i = 0; i < textToWrite.Length; ++i)
            {
                total += textToWrite[i];
                dialogueText.text = total;
                int textLength = dialogueText.text.Length;

               
                yield return new WaitForSeconds(writeSpeed);

                if (textLength >= 300)
                {

                    dialogueText.enableAutoSizing = true;

                   
                }
            }
        }

        protected abstract ChatCompletionMessage CreateStartingPrompt();
    }
}