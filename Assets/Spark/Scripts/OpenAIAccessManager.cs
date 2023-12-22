using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LeastSquares.Spark
{
    /// <summary>
    /// A static class that manages access to the OpenAI API.
    /// </summary>
    public static class OpenAIAccessManager
    {
        private static string _apiKey;
        private static OpenAIAPI _api;
        private static string _apiDomain = OpenAIAPI.OpenAIDomain;

        /// <summary>
        /// The temperature to use for text completion.
        /// </summary>
        public static double Temperature { get; set; } = 0.1;

        /// <summary>
        /// The chat model to use for chat completion.
        /// </summary>
        public static ChatModel ChatModel { get; set; }
        
        /// <summary>
        /// The audio model to use for chat completion.
        /// </summary>
        public static AudioModel AudioModel { get; set; }

        /// <summary>
        /// Requests chat completion for a given set of messages.
        /// </summary>
        /// <param name="messages">The messages to request chat completion for.</param>
        /// <returns>The completed chat message.</returns>
        public static async Task<string> RequestChatCompletion(ChatCompletionMessage[] messages, IProgress<double> progress = null, CancellationToken token = default, OnStreamingCallback onProgress = null)
        {
            if (!ApiKeyGuard()) return null;
            return await _api.RequestChatCompletion(messages, Model.FromChatModel(ChatModel), Temperature, progress, token, onProgress);
        }
        
        public static async Task<string> RequestAudioCompletion(AudioClip clip, string language=null, IProgress<double> progress = null)
        {
            if (!ApiKeyGuard()) return null;
            return await _api.RequestAudioTranscription(clip, Model.FromAudioModel(AudioModel), Temperature, language, progress);
        }
        
        public static async Task<string> RequestAudioCompletion(float[] samples, string language=null, IProgress<double> progress = null)
        {
            if (!ApiKeyGuard()) return null;
            return await _api.RequestAudioTranscription(samples, Model.FromAudioModel(AudioModel), Temperature, language, progress);
        }
        
        public static async Task<ImageResponse> RequestImageGeneration(string prompt, int n, string size, IProgress<double> progress = null)
        {
            if (!ApiKeyGuard()) return null;
            return await _api.RequestImageGeneration(prompt, n, size, progress);
        }
        
        public static async Task<ImageResponse> RequestImageEdit(Texture2D image, string prompt, int n, string size, Texture2D mask=null, IProgress<double> progress = null)
        {
            if (!ApiKeyGuard()) return null;
            return await _api.RequestImageEdit(image, prompt, mask, n, size, progress);
        }
        
        public static async Task<ImageResponse> RequestImageVariation(Texture2D image, int n, string size, IProgress<double> progress = null)
        {
            if (!ApiKeyGuard()) return null;
            return await _api.RequestImageVariation(image, n, size, progress);
        }

        /// <summary>
        /// Sets the API key to use for accessing the OpenAI API.
        /// </summary>
        /// <param name="ApiKey">The API key to use.</param>
        public static void SetAPIKey(string ApiKey)
        {
            _apiKey = ApiKey;
            _api = new OpenAIAPI(_apiKey, _apiDomain);
        }
        
        public static void SetAPIDomain(string ApiDomain)
        {
            _apiDomain = ApiDomain;
            _api = new OpenAIAPI(_apiKey, _apiDomain);
        }

        private static bool ApiKeyGuard()
        {
            if (_api == null)
                Debug.LogWarning("Failed to access OpenAI API. Please set your API key first.");
            return _api != null;
        }
    }
}