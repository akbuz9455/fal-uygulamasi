using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using UnityEditor;
using UnityEngine.Networking;

namespace LeastSquares.Spark
{
    public delegate void OnStreamingCallback(string text);
    
    /// <summary>
    /// A class that handles requests to the OpenAI API.
    /// </summary>
    public class OpenAIAPI 
    {
        public const string OpenAIDomain = "https://api.openai.com";
        private string _apiKey;
        private string _apiDomain;
        private bool _initialized;

        /// <summary>
        /// Constructor for the OpenAIAPI class.
        /// </summary>
        /// <param name="apiKey">The API key for the OpenAI API.</param>
        public OpenAIAPI(string apiKey, string apiDomain)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogWarning("To start using the asset, please input your API key in the settings.");
                _initialized = false;
            }
            else
            {
                _apiKey = apiKey;
                _apiDomain = apiDomain == string.Empty ? OpenAIDomain : apiDomain;
                _initialized = true;
            }
        }
        
        private string CreateUrl(string path)
        {
            if (_apiDomain == string.Empty || _apiDomain == null)
            {
                Debug.LogWarning("API domain is null");
                return OpenAIDomain + path;;
            }

            return $"{_apiDomain}{path}";
        } 

        /// <summary>
        /// Sends a request for chat completion to the OpenAI API.
        /// </summary>
        /// <param name="messages">The messages to use as context for the chat completion.</param>
        /// <param name="model">The model to use for chat completion.</param>
        /// <param name="temperature">The sampling temperature to use for generating the completion.</param>
        /// <returns>The completed chat message as a string.</returns>
        public async Task<string> RequestChatCompletion(ChatCompletionMessage[] messages, Model model, double temperature, IProgress<double> progress = null, CancellationToken token = default, OnStreamingCallback onProgress = null)
        {
            var result = await DoTextRequest<ChatCompletionResponse>(CreateUrl("/v1/chat/completions"), HttpMethod.Post, JsonUtility.ToJson(new ChatCompletionRequest
            {
                model = model.ModelName,
                messages = messages,
                temperature = temperature,
                stream = (onProgress != null)
            }), progress, token, onProgress);
            if (result == null && !token.IsCancellationRequested)
            {
                Debug.LogError("Failed to complete the request. Please check the documentation or ask in the discord for more details");
            }
            return result?.Text;
        }
        
        public async Task<ImageResponse> RequestImageGeneration(string prompt, int n = 1, string size = "1024x1024", IProgress<double> progress = null)
        {
            return await DoTextRequest<ImageResponse>(CreateUrl("/v1/images/generations"), HttpMethod.Post, JsonUtility.ToJson(new ImageGenerationRequest
            {
                prompt = prompt,
                n = n,
                size = size
            }), progress);
        }

        public async Task<ImageResponse> RequestImageEdit(Texture2D image, string prompt, Texture2D mask = null, int n = 1, string size = "1024x1024", IProgress<double> progress = null)
        {
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(prompt), "prompt");
            formData.Add(new StringContent(n.ToString()), "n");
            formData.Add(new StringContent(size), "size");

            var imageContent = new ByteArrayContent(image.EncodeToPNG());
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            formData.Add(imageContent, "image", "image.png");

            if (mask != null)
            {
                var maskContent = new ByteArrayContent(mask.EncodeToPNG());
                maskContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                formData.Add(maskContent, "mask", "mask.png");
            }

            return await DoRequest<ImageResponse>(CreateUrl("/v1/images/edits"), HttpMethod.Post, formData, progress);
        }

        public async Task<ImageResponse> RequestImageVariation(Texture2D image, int n = 1, string size = "1024x1024", IProgress<double> progress = null)
        {
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(n.ToString()), "n");
            formData.Add(new StringContent(size), "size");

            var imageContent = new ByteArrayContent(ImageConversion.EncodeToPNG(image));
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            formData.Add(imageContent, "image", "image.png");

            return await DoRequest<ImageResponse>(CreateUrl("/v1/images/variations"), HttpMethod.Post, formData, progress);
        }
        
        public async Task<string> RequestAudioTranscription(AudioClip audioClip, Model model, double temperature, string language = null, IProgress<double> progress = null)
        {
            var samples = new float[audioClip.channels * audioClip.samples];
            audioClip.GetData(samples, 0);
            var audioData = ConvertAudioClipToWav(samples, audioClip.channels, audioClip.frequency);
            return await RequestAudioTranscription(audioData, model, temperature, language, progress);
        }
        
        public async Task<string> RequestAudioTranscription(float[] pcmSamples16k1Ch, Model model, double temperature, string language = null, IProgress<double> progress = null)
        {
            var audioData = ConvertAudioClipToWav(pcmSamples16k1Ch, 1, 16000);
            return await RequestAudioTranscription(audioData, model, temperature, language, progress);
        }
        
        public async Task<string> RequestAudioTranscription(byte[] audioData, Model model, double temperature, string language = null, IProgress<double> progress = null)
        {
            using var formData = new MultipartFormDataContent();
    
            formData.Add(new StringContent(model.ModelName), "model");
            formData.Add(new StringContent(temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)), "temperature");
    
            if (language != null)
            {
                formData.Add(new StringContent(language), "language");
            }
    
            var audioContent = new ByteArrayContent(audioData);
            audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
            formData.Add(audioContent, "file", "audio.wav");
    
            var result = await DoRequest<AudioTranscriptionResponse>(CreateUrl("/v1/audio/transcriptions"), HttpMethod.Post, formData, progress);
            return result.text;
        }

        private async Task<T> DoTextRequest<T>(string url, HttpMethod method, string payload, IProgress<double> progress = null, CancellationToken token = default, OnStreamingCallback onprogress = null) where T : class
        {
            return await DoRequest<T>(url, method, new StringContent(payload, System.Text.Encoding.UTF8, "application/json"), progress, token, onprogress);
        }
        
        /// <summary>
        /// Do an http request to the specified url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="content"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private async Task<T> DoRequest<T>(string url, HttpMethod method, HttpContent content, IProgress<double> progress = null, CancellationToken token = default, OnStreamingCallback onprogress = null) where T : class
        {
            if (!InitializeGuard()) return default(T);
            
            if (_apiDomain != OpenAIDomain)
                Debug.LogWarning($"Connecting to non standard API endpoint '{_apiDomain}'");
            
            using (var request = new UnityWebRequest(url, method.ToString().ToUpperInvariant()))
            {
                request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
                request.SetRequestHeader("Accept", "application/json");
                if (content != null)
                {
                    byte[] contentBytes = await content.ReadAsByteArrayAsync();
                    request.uploadHandler = new UploadHandlerRaw(contentBytes);
                    request.SetRequestHeader("Content-Type", content.Headers.ContentType.ToString());
                }

                request.downloadHandler = onprogress != null ? new StreamingChatCompletionDownloadHandler(onprogress) : new DownloadHandlerBuffer();

                await request.SendWebRequestAsync(progress, token);

                if (request.result != UnityWebRequest.Result.Success && !token.IsCancellationRequested)
                {
                    Debug.LogError(MapError(request));
                }

                var responseJson = (request.downloadHandler is StreamingChatCompletionDownloadHandler handler) 
                    ? handler.GetReceivedContent() 
                    : request.downloadHandler.text;
                return JsonUtility.FromJson<T>(responseJson);
            }
        }

        private bool InitializeGuard()
        {
            if (!_initialized)
                Debug.LogError("No API key was provided. Please set an API key in settings");
            return _initialized;
        }

        private string MapError(UnityWebRequest request)
        {
            int errorCode = (int)request.responseCode;
            string errorMessage = request.error;

            switch (errorCode)
            {
                case 401:
                    if (request.downloadHandler.text.Contains("Invalid Authentication"))
                    {
                        errorMessage = "Error 401: Invalid Authentication. This might be because the API key is wrong or it has ran out of credits.";
                    }
                    break;
                case 429:
                    if (request.downloadHandler.text.Contains("Rate limit reached for requests"))
                    {
                        errorMessage = "Error 429: Rate limit reached for requests. Pace your requests. Read the Rate limit guide.";
                    }
                    else if (request.downloadHandler.text.Contains("You exceeded your current quota, please check your plan and billing details"))
                    {
                        errorMessage = "Error 429: You exceeded your current quota, please check your plan and billing details. Apply for a quota increase.";
                    }
                    else if (request.downloadHandler.text.Contains("The engine is currently overloaded, please try again later"))
                    {
                        errorMessage = "Error 429: The engine is currently overloaded, please try again later.";
                    }
                    break;
                case 500:
                    errorMessage = "Error 500: The server had an error while processing your request. Retry your request after a brief wait and contact us if the issue persists. Check the status page.";
                    break;
            }
            
            return errorMessage;
        }

        /// <summary>
        /// Converts an audio clip to a wav byte array
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        private byte[] ConvertAudioClipToWav(float[] samples, int channels, int frequency)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            
            int totalBytes = samples.Length * 2;

            writer.Write(0x46464952); // "RIFF"
            writer.Write(36 + totalBytes);
            writer.Write(0x45564157); // "WAVE"
            writer.Write(0x20746D66); // "fmt "
            writer.Write(16);
            writer.Write((short)1); // PCM format
            writer.Write((short)channels);
            writer.Write(frequency);
            writer.Write(frequency * channels * 2); // Byte rate
            writer.Write((short)(channels * 2)); // Block align
            writer.Write((short)16); // Bits per sample
            writer.Write(0x61746164); // "data"
            writer.Write(totalBytes);

            for (int i = 0; i < samples.Length; i++)
            {
                writer.Write((short)(samples[i] * 32767));
            }

            writer.Flush();
            byte[] wavData = stream.ToArray();
            writer.Close();
            stream.Close();

            return wavData;
        }
    }
}
