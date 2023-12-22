using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LeastSquares.Spark
{
    [CustomEditor(typeof(TranscribeAudio))]
    public class EditorTranscribeAudio : Editor
    {
        SerializedProperty recordButton;
        SerializedProperty transcriptionText;
        SerializedProperty maxRecordingLength;
        SerializedProperty language;

        private void OnEnable()
        {
            recordButton = serializedObject.FindProperty("recordButton");
            transcriptionText = serializedObject.FindProperty("transcriptionText");
            maxRecordingLength = serializedObject.FindProperty("maxRecordingLength");
            language = serializedObject.FindProperty("language");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Separator();
            GUILayout.Label("Transcriber Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(recordButton, new GUIContent("Record Button", "Button to use for recording"));
            EditorGUILayout.PropertyField(transcriptionText, new GUIContent("Transcription Text", "Where to put the transcription text"));
            EditorGUILayout.PropertyField(maxRecordingLength, new GUIContent("Max Recording Length", "Max length we should record with the microphone."));
            EditorGUILayout.PropertyField(language, new GUIContent("Language", "The language to use for the transcription. Leave null for auto detecting the language."));

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}