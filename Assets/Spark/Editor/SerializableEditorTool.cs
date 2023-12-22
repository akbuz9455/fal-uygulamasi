using System;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public abstract class SerializableEditorTool : ScriptableObject, IEditorTool
    {
        private EditorWindow _window;
        
        public void Initialize(EditorWindow window)
        {
            _window = window;
        }

        public abstract void OnGUI();

        public virtual void Reload()
        {
        }

        public void Repaint()
        {
            _window.Repaint();
        }

        public void Close()
        {
            _window.Close();
        }

        public virtual void OnEnable()
        {
            Reload();
        }

        public virtual void OnDisable()
        {
            
        }

        protected Rect position => _window.position;
    }
}