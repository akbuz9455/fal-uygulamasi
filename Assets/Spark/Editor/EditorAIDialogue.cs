using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{

    [CustomEditor(typeof(InteractiveAIDialogue))]
    [CanEditMultipleObjects]
    public class InteractiveAIDialogueEditor : BaseDialogueEditor {}

    [CustomEditor(typeof(BarkAIDialogue))]
    [CanEditMultipleObjects]
    public class BarkAIDialogueEditor : BaseDialogueEditor
    {
        protected override void ExtraOptionGUI()
        {
            var dialogue = (BarkAIDialogue)target;
            dialogue.wordCount = EditorGUILayout.IntSlider(
                new GUIContent("Word Count", "Average word count the response should have."),
                dialogue.wordCount,
                10,
                300
            );

        }
    }
    
    public class BaseDialogueEditor : Editor
    {
        protected virtual void ExtraOptionGUI()
        {
            
        }

        public override void OnInspectorGUI()
        {
            var dialogue = (BaseDialog)target;
            EditorGUILayout.Separator();
            GUILayout.Label("Character Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            dialogue.characterName = EditorGUILayout.TextField(
                new GUIContent("Name", "The name of this character. Leave null for no name/unknown."),
                dialogue.characterName
            );
            EditorGUILayout.LabelField(new GUIContent("Act as", "Brief description of the character the AI should act as."));
            var style = new GUIStyle(EditorStyles.textArea);
            style.wordWrap = true;
            dialogue.actAs = EditorGUILayout.TextArea(
                dialogue.actAs,
                style,
                GUILayout.Height(200)
            );
            GUILayout.Label("Things to mention", EditorStyles.boldLabel);
            var thingsToMention = serializedObject.FindProperty("thingsToMention");
            EditorGUILayout.PropertyField(thingsToMention, true);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Separator();
            GUILayout.Label("Dialogue Settings", EditorStyles.boldLabel);
            ExtraOptionGUI();
            dialogue.writeSpeed = EditorGUILayout.Slider(
                new GUIContent("Write Speed", "A float that represents the writing speed of the dialogue."),
                dialogue.writeSpeed,
                0f,
                0.1f
            );
            
            var dialogueText = serializedObject.FindProperty("dialogueText");
            EditorGUILayout.PropertyField(dialogueText, true);
            serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
        }
    }
}