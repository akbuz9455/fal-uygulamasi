using System;
using System.Linq;

namespace LeastSquares.Spark
{
    /// <summary>
    /// Represents a request for text completion.
    /// </summary>
    [Serializable]
    class TextCompletionRequest
    {
        /// <summary>
        /// The name of the model to use for text completion.
        /// </summary>
        public string model;
        
        /// <summary>
        /// The prompt to use for text completion.
        /// </summary>
        public string prompt;
        
        /// <summary>
        /// The maximum number of tokens to generate for the completed text.
        /// </summary>
        public int max_tokens;
        
        /// <summary>
        /// The temperature to use for text completion. A higher temperature will result in more diverse text.
        /// </summary>
        public double temperature;
    }

    /// <summary>
    /// Represents a choice for text completion.
    /// </summary>
    [Serializable]
    class TextCompletionChoice
    {
        /// <summary>
        /// The completed text.
        /// </summary>
        public string text;
    }
    
    /// <summary>
    /// Represents a response for text completion.
    /// </summary>
    [Serializable]
    class TextCompletionResponse
    {
        /// <summary>
        /// The choices for text completion.
        /// </summary>
        public TextCompletionChoice[] choices;
        
        /// <summary>
        /// The completed text from the first choice.
        /// </summary>
        public string Text => choices[0].text;
    }
    
    /// <summary>
    /// Represents a request for image generation.
    /// </summary>
    [Serializable]
    class ImageGenerationRequest
    {
        /// <summary>
        /// The size of the generated image.
        /// </summary>
        public string size;
        
        /// <summary>
        /// The prompt to use for image generation.
        /// </summary>
        public string prompt;
        
        /// <summary>
        /// The number of images to generate.
        /// </summary>
        public int n;
    }

    /// <summary>
    /// Represents an item for image generation.
    /// </summary>
    [Serializable]
    class ImageGenerationItem
    {
        /// <summary>
        /// The URL of the generated image.
        /// </summary>
        public string url;
    }
    
    /// <summary>
    /// Represents a response for image generation.
    /// </summary>
    [Serializable]
    class ImageGenerationResponse
    {
        /// <summary>
        /// The data for the generated images.
        /// </summary>
        public ImageGenerationItem[] data;
    }
    
    /// <summary>
    /// Represents a request for audio transcription.
    /// </summary>
    [Serializable]
    class AudioTranscriptionRequest
    {
        /// <summary>
        /// The file to transcribe.
        /// </summary>
        public string file;
        
        /// <summary>
        /// The name of the model to use for transcription.
        /// </summary>
        public string model;
        
        /// <summary>
        /// The language of the audio file.
        /// </summary>
        public string language;

        /// <summary>
        /// The temperature of the model
        /// </summary>
        public double temperature;
    }
    
    /// <summary>
    /// Represents a response for audio transcription.
    /// </summary>
    [Serializable]
    class AudioTranscriptionResponse
    {
        /// <summary>
        /// The transcribed text.
        /// </summary>
        public string text;
    }
    
    /// <summary>
    /// Represents a request for chat completion.
    /// </summary>
    [Serializable]
    class ChatCompletionRequest
    {
        /// <summary>
        /// The name of the model to use for chat completion.
        /// </summary>
        public string model;
        
        /// <summary>
        /// The messages to use for chat completion.
        /// </summary>
        public ChatCompletionMessage[] messages;
        
        /// <summary>
        /// The temperature to use for chat completion. A higher temperature will result in more diverse responses.
        /// </summary>
        public double temperature;

        public bool stream;
    }
    
    /// <summary>
    /// Represents a message for chat completion.
    /// </summary>
    [Serializable]
    public class ChatCompletionMessage
    {
        /// <summary>
        /// The role of the message sender.
        /// </summary>
        public string role;
        
        /// <summary>
        /// The content of the message.
        /// </summary>
        public string content;
    }
    
    /// <summary>
    /// Represents a choice for chat completion.
    /// </summary>
    [Serializable]
    class ChatCompletionChoice
    {
        /// <summary>
        /// The completed message.
        /// </summary>
        public ChatCompletionMessage message;
    }
    
    /// <summary>
    /// Represents a response for chat completion.
    /// </summary>
    [Serializable]
    class ChatCompletionResponse
    {
        /// <summary>
        /// The completed text from the first choice.
        /// </summary>
        public string Text => choices[0].message.content;
        
        /// <summary>
        /// The choices for chat completion.
        /// </summary>
        public ChatCompletionChoice[] choices;
    }

    [Serializable]
    public class ImageEditRequest
	{
		public string prompt;
		public int n;
		public string size;
		public string response_format;
	}

    [Serializable]
	public class ImageVariationRequest
	{
		public int n;
		public string size;
		public string response_format;
	}

    [Serializable]
	public class ImageResponse
	{
		public long created;
		public ImageData[] data;

        public string[] urls => data.Select(d => d.url).ToArray();
    }

    [Serializable]
	public class ImageData
	{
		public string url;
	}
}
