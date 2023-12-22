using System;
using UnityEngine;
using UnityEngine.Networking;

namespace LeastSquares.Spark
{
    public class StreamingChatCompletionDownloadHandler : DownloadHandlerScript
    {
        private string _answer;
        private string _text;
        private OnStreamingCallback _progress;
        
        public string GetReceivedContent()
        {
            return JsonUtility.ToJson(new ChatCompletionResponse
            {
                choices = new []
                {
                    new ChatCompletionChoice
                    {
                        message = new ChatCompletionMessage
                        {
                            content = _answer,
                            role = "assistant"
                        }
                    }
                }
            });
        }

        public StreamingChatCompletionDownloadHandler(OnStreamingCallback progress)
        {
            _progress = progress;
        }
        
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength <= 0)
                return false;

            _text += System.Text.Encoding.UTF8.GetString(data, 0, dataLength);
            string[] lines = _text.Split('\n');

            for (int i = 0; i < lines.Length - 1; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("data:"))
                {
                    string content = line.Substring(5).Trim();
                    if (content != "[DONE]")
                    {
                        var response = JsonUtility.FromJson<ChatCompletionChunkResponse>(content);
                        if (response.choices.Length > 0)
                        {
                            string deltaContent = response.choices[0].delta.content;
                            _answer += deltaContent;
                            _progress(_answer);
                            //Debug.Log("Delta content: " + deltaContent);
                        }
                        else
                        {
                            Debug.Log("No choices found in the response.");
                        }
                        //Debug.Log("Received: " + content);
                    }
                    else
                        Debug.Log("Streaming completed.");
                }
            }

            _text = lines[lines.Length - 1];
            return true;
        }
        
        [Serializable]
        public class ChatCompletionChunkResponse
        {
            public string id;
            public string obj;
            public long created;
            public string model;
            public Choice[] choices;
        }

        [Serializable]
        public class Choice
        {
            public Delta delta;
            public int index;
            public string finish_reason;
        }

        [Serializable]
        public class Delta
        {
            public string content;
        }
    }
}