using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    internal class IterativeShaderCoderComponent : IterativeShaderCoder
    {
        public Material selectedMaterial;
        private Shader _shader;
        public bool isOnlyCreating;
        public Shader selectedShader
        {
            get => _shader;
            set {
                var fullPath = Application.dataPath + AssetDatabase.GetAssetPath(value).Substring(6);
                _editComponentDefaultScriptPath = fullPath;
                _shader = value;
            }
        }
        
        protected override void OnCancel()
        {
            if (SelectedConversation.messages.Count == 1)
                Close();
        }

        protected override void OnAcceptChanges(CodingSession session)
        {
            if (_mode == CodeMode.AddComponent)
            {
                ScriptCreator.CreateScriptAsset(session.LastCodeVersion, session.ScriptPath);
            }
            else
            {
                ScriptCreator.EditScript(session.ScriptPath, session.LastCodeVersion);
            }
            Close();
        }
    }
}