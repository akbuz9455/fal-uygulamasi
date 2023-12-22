using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class AIEditorToggle
    {
        private string _key;
        private string _text;
        private bool _value;
        private bool _isGlobal;

        public AIEditorToggle(string Key, string text, bool defaultValue, bool isGlobal)
        {
            _key = Key;
            _text = text;
            _isGlobal = isGlobal;
            _value = defaultValue;
        }

        public void Render()
        {
            _value = EditorGUILayout.Toggle(_text, _value);
        }
        
        public void Load()
        {
            _value = _isGlobal ? SaveSystem.GetGlobalBool(_key, _value) : SaveSystem.GetProjectBool(_key, _value);
        }

        public void Save()
        {
            if (_isGlobal)
                SaveSystem.SetGlobalBool(_key, _value);
            else
                SaveSystem.SetProjectBool(_key, _value);
        }

        public bool GetValue()
        {
            return _value;
        }
    }
}