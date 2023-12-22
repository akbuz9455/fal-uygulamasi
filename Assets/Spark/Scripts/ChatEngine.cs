using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LeastSquares.Spark
{
    class AuthCredentials
    {
        public string api_key;
        public float temperature = 0.2f;
    }
    
    /// <summary>
    /// The ChatEngine class is responsible for loading the OpenAI API credentials and setting the API key and temperature.
    /// </summary>
    public class ChatEngine : MonoBehaviour
    {
        public static bool _loaded;
        public TextAsset _authFile;
        
        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        private void Awake()
        {
            if(_loaded) return;
            
            if (_authFile != null)
            {
                // Parse the JSON data
                var jsonData = _authFile.text;
                var creds = JsonUtility.FromJson<AuthCredentials>(jsonData);
                if(creds.api_key.Contains("YOUR_API_KEY_HERE"))
                    Debug.Log("Please replace the API key in the auth file with your own API key from OpenAI. See documentation for more details.");
                
                OpenAIAccessManager.SetAPIKey(creds.api_key);
                OpenAIAccessManager.Temperature = creds.temperature;
                Debug.Log("Loaded auth file for the OpenAI API");
                _loaded = true;
            }
            else
            {
                Debug.Log("Failed to load or find auth file for the OpenAI API");
            }
        }
    }
}