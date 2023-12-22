using System;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    internal class ComponentEvents
    {
        private string _uniqueKey;
        private CodeMode _mode;
        public bool isOnlyCreating;
        private Action _after;
        private Action<string> _do;

        public ComponentEvents(string uniqueKey, Action<string> Do, Action After)
        {
            _do = Do;
            _after = After;
            _uniqueKey = uniqueKey;
        }
        
        public void SetMode(CodeMode mode)
        {
            _mode = mode;
        }

        private string DontAddKey => $"{_uniqueKey}_DontAdd";
        
        private string SparkCoderComponentKey => $"{_uniqueKey}_SparkCoderComponent";
        
        public void OnAcceptChanges(CodingSession session)
        {
            if (_mode == CodeMode.AddComponent)
            {
                SaveSystem.SetProjectString(SparkCoderComponentKey, "Assets/" + session.ScriptPath.Replace(Application.dataPath, ""));
                if (isOnlyCreating)
                    SaveSystem.SetProjectBool(DontAddKey, true);
                ScriptCreator.CreateScriptAndReload(session.LastCodeVersion, session.ScriptPath);
            }
            else
            {
                ScriptCreator.EditScript(session.ScriptPath, session.LastCodeVersion);
            }
        }

        public void OnEnable() => AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

        public void OnDisable() => AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

        private void OnAfterAssemblyReload()
        {
            try
            {
                var path = SaveSystem.GetProjectString(SparkCoderComponentKey);
                if (string.IsNullOrEmpty(path) || SaveSystem.GetProjectBool(DontAddKey))
                {
                    return;
                }

                _do(path);
            }
            finally
            {
                SaveSystem.SetProjectBool(DontAddKey, false);
                SaveSystem.SetProjectString(SparkCoderComponentKey, null);
                _after();
            }
        }
    }
}