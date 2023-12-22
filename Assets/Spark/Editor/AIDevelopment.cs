using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class AIDevelopment : EditorWindow
    {
        private static (SerializableEditorTool, string)[] _tabs;
        private int _currentTab;

        [MenuItem("Window/Spark AI/Spark &j")]
        public static void ShowWindow()
        {
            GetWindow<AIDevelopment>("Spark AI");
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            var openSettings = SaveSystem.GetProjectBool("SparkOpenSettings");
            if (openSettings && _tabs != null)
            {
                _currentTab = _tabs.Length - 1;
                SaveSystem.SetProjectBool("SparkOpenSettings", false);
            }
            
            if (_tabs != null) return;
            _tabs = new (SerializableEditorTool, string)[]
            {
                (LoadOrCreate<AIConversation>(), ExtractDisplayName(nameof(AIConversation))),
                (LoadOrCreate<IterativeCoder>(), ExtractDisplayName(nameof(IterativeCoder))),
                (LoadOrCreate<IterativeShaderCoder>(), "Shader Coder"),
                (LoadOrCreate<AIArtist>(), ExtractDisplayName(nameof(AIArtist))),
                (LoadOrCreate<AIDebugger>(), ExtractDisplayName(nameof(AIDebugger))),
                (LoadOrCreate<AIWriter>(), ExtractDisplayName(nameof(AIWriter))),
                (LoadOrCreate<AISettings>(), ExtractDisplayName(nameof(AISettings))),
            };
            foreach (var tab in _tabs)
            {
                tab.Item1.Initialize(this);
            }
        }
        
        private void OnGUI()
        {
            Initialize();
            EditorGUILayout.LabelField("Choose a tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            var prevTab = _currentTab;
            _currentTab = GUILayout.Toolbar((int)_currentTab, _tabs.Select(t => t.Item2).ToArray());
            if (_currentTab != prevTab)
            {
                GUIUtility.keyboardControl = 0;
                _tabs[_currentTab].Item1.Reload();
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(_tabs[_currentTab].Item2, EditorStyles.boldLabel);
            _tabs[_currentTab].Item1.OnGUI();
        }
        
        private string ExtractDisplayName(string className)
        {
            string displayName = Regex.Replace(className, "([A-Z][a-z])", " $1");
            displayName = Regex.Replace(displayName, "([a-z])([A-Z])", "$1 $2");
            return displayName.TrimStart();
        }

        private T LoadOrCreate<T>() where T : SerializableEditorTool
        {
            var fileName = typeof(T).Name;
            var assetPath = Paths.GetPathInProjectTempFolderRelative($"{fileName}.asset");
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                asset = CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
            }
            return asset;
        }
    }
}