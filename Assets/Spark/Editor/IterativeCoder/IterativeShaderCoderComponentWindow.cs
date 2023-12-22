using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    internal class IterativeShaderCoderComponentWindow : EditorWindow
    {
        private IterativeShaderCoderComponent _coderComponent;

        public Material selectedMaterial
        {
            get => _coderComponent.selectedMaterial;
            set => _coderComponent.selectedMaterial = value;
        }

        public bool isOnlyCreating
        {
            get => _coderComponent.isOnlyCreating;
            set => _coderComponent.isOnlyCreating = value;
        }
        
        public Shader selectedShader
        {
            get => _coderComponent.selectedShader;
            set => _coderComponent.selectedShader = value;
        }

        public void SetMode(CodeMode mode)
        {
            _coderComponent.SetMode(mode);
        }
        
        private void OnEnable()
        {
            _coderComponent = CreateInstance<IterativeShaderCoderComponent>();
            _coderComponent.Initialize(this);
        }

        private void OnGUI()
        {
            _coderComponent.OnGUI();
        }
    }
}