using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeastSquares.Spark
{
    /// <summary>
    /// This class handles the audio recording and transcription functionality.
    /// </summary>
    public class TranscribeAudio : MonoBehaviour
    {
        // The button used to start and stop recording.
        public Button recordButton;

        // The text field used to display the transcription.
        public TMP_Text transcriptionText;

        // The sample rate used for recording.
        private int sampleRate = 16000;

        // The maximum length of the recording in seconds.
        public int maxRecordingLength = 10;

        // The language used for transcription.
        public string language;

        // Flag indicating whether recording is currently in progress.
        private bool isRecording = false;

        private void Start()
        {
            // Add a listener to the record button to toggle recording.
            recordButton.onClick.AddListener(ToggleRecording);

            // Disable the record button if the ChatEngine has not been loaded yet.
            if (!ChatEngine._loaded)
            {
                recordButton.enabled = false;
                Debug.Log("Cannot enable recording as the ChatEngine has not been loaded yet.");
            }
        }

        /// <summary>
        /// Toggles recording on and off.
        /// </summary>
        private void ToggleRecording()
        {
            if (isRecording)
            {
                StartCoroutine(SendAudioForTranscription());
            }
            else
            {
                StartRecording();
            }
        }

        /// <summary>
        /// Starts recording audio.
        /// </summary>
        private void StartRecording()
        {
            isRecording = true;
            recordButton.GetComponentInChildren<TMP_Text>().text = "Stop Recording";
            CrossPlatformMicrophone.Start(null, false, maxRecordingLength, sampleRate);
        }

        /// <summary>
        /// Stops recording audio.
        /// </summary>
        private void StopRecording()
        {
            isRecording = false;
            recordButton.GetComponentInChildren<TMP_Text>().text = "Start Recording";
            CrossPlatformMicrophone.End(null);
        }

        /// <summary>
        /// Sends the recorded audio clip for transcription and displays the result in the transcription text field.
        /// </summary>
        private IEnumerator SendAudioForTranscription()
        {
            // Disable the record button while transcription is in progress.
            recordButton.interactable = false;

            // Display a message indicating that transcription is in progress.
            transcriptionText.text = "Transcribing...";

            var length = CrossPlatformMicrophone.GetRecordingPosition(null);
            var samples = new float[length];
            CrossPlatformMicrophone.GetData(null, samples, 0);
            StopRecording();

            var transcriptionTask = OpenAIAccessManager.RequestAudioCompletion(samples, language);
            yield return new WaitUntil(() => transcriptionTask.IsCompleted);

            // Display the transcription result in the transcription text field.
            transcriptionText.text = transcriptionTask.Result;

            // Re-enable the record button.
            recordButton.interactable = true;
        }
    }
}