using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    internal class NewCodingSessionWindow : EditorWindow
    {
        private List<CodingSession> codingSessions;
        private IterativeCoder iterativeCoder;
        private Object _scriptSelected;
        private string _selectedScriptPath;
        private string _prompt;

        public void Initialize(List<CodingSession> codingSessions, IterativeCoder iterativeCoder)
        {
            this.codingSessions = codingSessions;
            this.iterativeCoder = iterativeCoder;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            // Create new conversation
            CreateNewSession();

            EditorGUILayout.EndVertical();
        }
        

        private void CreateNewSession()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                EditorGUILayout.LabelField("Enter a prompt for the new session:", EditorStyles.wordWrappedLabel);
                var style = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true
                };
                _prompt = EditorGUILayout.TextArea(_prompt, style, GUILayout.ExpandWidth(true), GUILayout.Height(125));

                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                {
                    EditorGUILayout.LabelField($"Create a new {iterativeCoder.ObjectName}:",
                        EditorStyles.boldLabel);
                    CreateNewScript();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                {
                    EditorGUILayout.LabelField($"Edit existing {iterativeCoder.ObjectName}:",
                        EditorStyles.boldLabel);
                    SelectExistingScript();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                /*
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                {
                    EditorGUILayout.LabelField($"Create a new {iterativeCoder.ObjectName} and attach:",
                        EditorStyles.boldLabel);
                    AddToGameObjectGUI();
                }
                EditorGUILayout.EndVertical();
                */
            }
            EditorGUILayout.EndVertical();
        }

        private void AddToGameObjectGUI()
        {
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_prompt));
            if (GUILayout.Button($"Create and attach a new {iterativeCoder.ObjectName}", GUILayout.ExpandWidth(true)))
            {
                AddToGameObject();
            }
            EditorGUI.EndDisabledGroup();
        }
        
        private void AddToGameObject()
        {
           /* var selectedGameObject = _selectedGameObject;
            ComponentActions.OpenComponentWindow<IterativeCoderComponentWindow>(null, "Create and add component", window =>
                {
                    window.selectedGameObject = selectedGameObject;
                    window.SetMode(CodeMode.AddComponent);
                }
            );*/
        }

        private void SelectExistingScript()
        {
            LeastSquaresGUI.SelectScript(ref _selectedScriptPath, ref _scriptSelected);
            EditorGUI.BeginDisabledGroup(_scriptSelected == null || string.IsNullOrEmpty(_prompt));
            {
                if (GUILayout.Button($"Edit {iterativeCoder.ObjectName}", GUILayout.ExpandWidth(true)))
                {
                    StartNewSession();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void CreateNewScript()
        {
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_prompt));
            if (GUILayout.Button($"Create a new {iterativeCoder.ObjectName}", GUILayout.ExpandWidth(true)))
            {
                StartNewSession();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void StartNewSession()
        {
            iterativeCoder.StartNewCodingSession(_selectedScriptPath, _prompt);
            CloseWindow();
        }

        private void CloseWindow()
        {
            iterativeCoder.Repaint();
            Close();
        }
        
        internal static string GetBaseScriptPrompt()
        {
            //return
                //@"As ChatGPT, create Unity C# scripts given '|NEW_SCRIPT|`<prompt>`' or edit them with '|EDIT_SCRIPT|`<source_code>`|`<prompt>`'. Respond with '|CODE|`<code>`'.";
            return @"
You're ChatGPT, a skilled AI language model specializing in Unity & C# programming. Reply to user requests:

1. For '|NEW_SCRIPT|`<prompt>`', return code for a Unity C# script matching <prompt>.
Ex:
Input: '|NEW_SCRIPT|`Move a GameObject right each frame.`'
Output: '|CODE|`using UnityEngine;

public class MoveRight : MonoBehaviour
{
    public float speed = 1.0f;

    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;
    }
}`'

2. For '|EDIT_SCRIPT|`<source_code>`|`<prompt>`', update <source_code> according to <prompt>.
Ex:
Input: '|EDIT_SCRIPT|`using UnityEngine;

public class MoveRight : MonoBehaviour
{
    public float speed = 1.0f;

    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;
    }
}`|`Move GameObject with W & S keys.`'
Output: '|CODE|`using UnityEngine;

public class MoveRight : MonoBehaviour
{
    public float speed = 1.0f;
    public float verticalSpeed = 1.0f;

    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.up * verticalSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.down * verticalSpeed * Time.deltaTime;
        }
    }
}`'

Always respond with '|CODE|`<code>`'. Include necessary imports, RequireComponent attributes, tooltips, and fix errors if prompted. If unable to generate code, return original code with a C# comment explaining why.
";
        }
        
        internal static string GetBaseShaderPrompt()
        {
            return @"
You are ChatGPT, a highly proficient AI language model with expertise in Unity and Shader programming. Respond to the following requests from the user:

1. If the user writes '|NEW_SCRIPT|`<prompt>`', where <prompt> is the description of a Unity shader, return the code for that shader that best matches <prompt>. Ensure that the shader is cross-platform and compatible with most rendering pipelines.
Example:
Input: '|NEW_SCRIPT|`Create a simple unlit color shader.`'
Output: '|CODE|`Shader ""Custom/UnlitColor""
{
    Properties
    {
        _Color(""Color"", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags {""Queue""=""Transparent"" ""RenderType""=""Transparent""}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        float4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
        }
        ENDCG
    }
    FallBack ""Diffuse""
}`'

2. If the user writes '|EDIT_SCRIPT|`<source_code>`|`<prompt>`', where <source_code> is the current source code of a shader and <prompt> is the description of the modifications the user wants, return the updated source code for that shader that best matches <prompt>. Ensure that the shader remains cross-platform and compatible with most rendering pipelines.
Example:
Input: '|EDIT_SCRIPT|`Shader ""Custom/UnlitColor""
{
    Properties
    {
        _Color(""Color"", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags {""Queue""=""Transparent"" ""RenderType""=""Transparent""}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        float4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
        }
        ENDCG
    }
    FallBack ""Diffuse""
}`|`Add a texture property and multiply the texture with the color.`'
Output: '|CODE|`Shader ""Custom/UnlitColor""
{
    Properties
    {
        _Color(""Color"", Color) = (1, 1, 1, 1)
        _MainTex(""Texture"", 2D) = ""white"" {}
    }

    SubShader
    {
        Tags {""Queue""=""Transparent"" ""RenderType""=""Transparent""}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        float4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = _Color.rgb * tex.rgb;
        }
        ENDCG
    }
    FallBack ""Diffuse""
}`'

In all cases, your response should only be in the format of '|CODE|`<code>`', where <code> is the code that best matches the user's request.
If for any reason, you refuse the generate the code or need to write some non code text then return the original code and add a code comment at the top with the text.
";
        }
    }
}