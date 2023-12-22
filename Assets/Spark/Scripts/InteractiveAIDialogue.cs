using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeastSquares.Spark
{
    /// <summary>
    /// AIDialogue is a class that represents an AI or NPC character inside a game that can have a conversation with the player.
    /// </summary>
    public class InteractiveAIDialogue : BaseDialog
    {
        
        /// <summary>
        /// Initiates a conversation with the AI or NPC character.
        /// </summary>
        /// <param name="prompt">The initial prompt or message from the player.</param>
        public async void Talk(string prompt)
        {
            if (!ChatEngine._loaded) return;
            
            _messages.Add(new ChatCompletionMessage
            {
                role = "user",
                content = prompt
            });
            SetText("Yazýyor...");
            var result = await OpenAIAccessManager.RequestChatCompletion(_messages.ToArray());
            _messages.Add(new ChatCompletionMessage
            {
                role = "assistant",
                content = result
            });
            SetText(result);
        }
        
        /// <summary>
        /// Creates the starting prompt for the AI or NPC character.
        /// </summary>
        /// <returns>A ChatCompletionMessage object representing the starting prompt.</returns>
        protected override ChatCompletionMessage CreateStartingPrompt()
        {
            var prompt = "You are acting as an AI or NPC inside a game, a player might talk to you and you will have a pleasant conversation. The following are the instructions for your character:\n"; 
            prompt += characterName != null ? $"Your name is {characterName}. " : "";
            prompt += actAs != null ? $"You are a {actAs}." : "";
            prompt += thingsToMention != null ? $"Try to mention this things during your conversations:\n{string.Join("\n", thingsToMention)} " : "";
            prompt += "Do not break character. Be creative";
            prompt += "Do not talk excessively. Instead encourage the player to ask questions";
            return new ChatCompletionMessage
            {
                role = "system",
                content = prompt
            };
        }
    }
}