namespace LeastSquares.Spark
{
    public class BarkAIDialogue : BaseDialog
    {
        public int wordCount = 100;
        
        /// <summary>
        /// Initiates a conversation with the AI or NPC character.
        /// </summary>
        /// <param name="prompt">The initial prompt or message from the player.</param>
        public async void Talk()
        {
            if (!ChatEngine._loaded) return;

            _messages.Add(new ChatCompletionMessage
            {
                role = "user",
                content = (_messages.Count > 0) ? "Hey. Tell me something" : "Tell me something different.",
            });
            SetText("Thinking...");
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
            var prompt = "You are acting as an AI or NPC inside a game, a player might talk to you and you will say something. The following are the instructions for your character:\n"; 
            prompt += characterName != null ? $"Your name is {characterName}. " : "";
            prompt += actAs != null ? $"You are a {actAs}." : "";
            prompt += thingsToMention != null ? $"Try to mention this things during your conversations:\n{string.Join("\n", thingsToMention)} " : "";
            prompt += "Do not break character. Be creative. Do not ask questions as the player cannot talk back";
            prompt += "Talk as much as you want as the player wont reply back, this is not an interactive conversation so try to say notable things.";
            prompt += $"On each response say {wordCount} words.";
            return new ChatCompletionMessage
            {
                role = "system",
                content = prompt
            };
        }
    }
}