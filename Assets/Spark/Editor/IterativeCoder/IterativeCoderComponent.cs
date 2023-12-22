using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeastSquares.Spark
{
    internal class IterativeCoderComponent : IterativeCoder
    {
        private const string GameObjectKey = "SparkGameObjectToAddComponentTo";
        public GameObject selectedGameObject;
        private MonoScript _script;
        private ComponentEvents _events;

        protected override void OnCancel()
        {
            if (SelectedConversation.messages.Count == 1)
                Close();
        }
        
        public bool isOnlyCreating
        {
            get => _events.isOnlyCreating;
            set => _events.isOnlyCreating = value;
        }
        
        public IterativeCoderComponent()
        {
            _events = new ComponentEvents("coder", path =>
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null)
                {
                    var gameObject = selectedGameObject;
                    if (gameObject == null)
                    {
                        var instanceId = SaveSystem.GetProjectInt(GameObjectKey);
                        gameObject = FindGameObjectByInstanceId(instanceId);
                    }
                    
                    if (gameObject == null)
                        gameObject = Selection.activeGameObject;

                    if (gameObject != null)
                        gameObject.AddComponent(script.GetClass());
                    else
                        Debug.LogError("Failed to find target GameObject.");
                }
                else
                {
                    Debug.LogError("Failed to find script after reload.");
                }
            }, () =>
            {
                Close();
                SaveSystem.DeleteProjectKey(GameObjectKey);
            });
        }
        
        private static GameObject FindGameObjectByInstanceId(int instanceId)
        {
            GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject go in allGameObjects)
            {
                if (go.GetInstanceID() == instanceId)
                {
                    return go;
                }
            }

            return null;
        }
        
        public MonoScript selectedScript
        {
            get => _script;
            set {
                var fullPath = Application.dataPath + AssetDatabase.GetAssetPath(value).Substring(6);
                _editComponentDefaultScriptPath = fullPath;
                _script = value;
            }
        }
        
        public override void OnEnable() 
        {
            base.OnEnable();
            _events.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _events.OnDisable();
        }
        
        public override void SetMode(CodeMode mode)
        {
            base.SetMode(mode);
            _events.SetMode(mode);
        }

        protected override void OnAcceptChanges(CodingSession codingSession)
        {
            if (selectedGameObject != null && _mode == CodeMode.AddComponent)
            {
                SaveSystem.SetProjectInt(GameObjectKey, selectedGameObject.GetInstanceID());
            }

            _events.OnAcceptChanges(codingSession);
        }
    }
}