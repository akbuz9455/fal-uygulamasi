using System;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    internal enum CodeMode
    {
        FullEditor,
        EditComponentScript,
        AddComponent
    }
    
    internal class IterativeCoderComponentWindow : EditorWindow
    {
        private IterativeCoderComponent _coderComponent;

        public GameObject selectedGameObject
        {
            get => _coderComponent.selectedGameObject;
            set => _coderComponent.selectedGameObject = value;
        }

        public bool isOnlyCreating
        {
            get => _coderComponent.isOnlyCreating;
            set => _coderComponent.isOnlyCreating = value;
        }

        public MonoScript selectedScript
        {
            get => _coderComponent.selectedScript;
            set => _coderComponent.selectedScript = value;
        }

        public void SetMode(CodeMode mode)
        {
            _coderComponent.SetMode(mode);
        }
        
        private void OnEnable()
        {
            _coderComponent = CreateInstance<IterativeCoderComponent>();
            _coderComponent.Initialize(this);
        }

        private void OnGUI()
        {
            _coderComponent.OnGUI();
        }
    }
}