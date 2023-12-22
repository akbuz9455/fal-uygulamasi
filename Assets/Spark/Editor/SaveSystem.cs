using System;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class SaveSystem
    {
        private static string ProjectPrefix => GetProjectName();
        private static string GlobalPrefix => string.Empty;

        public static int GetProjectInt(string key, int defaultValue = default)
        {
            return GetProjectData(key, defaultValue, EditorPrefs.GetInt, EditorPrefs.SetInt);
        }
        
        public static bool GetProjectBool(string key, bool defaultValue = default)
        {
            return GetProjectData(key, defaultValue, EditorPrefs.GetBool, EditorPrefs.SetBool);
        }
        
        public static float GetProjectFloat(string key, float defaultValue = default)
        {
            return GetProjectData(key, defaultValue, EditorPrefs.GetFloat, EditorPrefs.SetFloat);
        }
        
        public static string GetProjectString(string key, string defaultValue = default)
        {
            return GetProjectData(key, defaultValue, EditorPrefs.GetString, EditorPrefs.SetString);
        }
        
        public static void SetProjectInt(string key, int value)
        {
            SetProjectData(key, value, EditorPrefs.SetInt);
        }
        
        public static void SetProjectBool(string key, bool value)
        {
            SetProjectData(key, value, EditorPrefs.SetBool);
        }
        
        public static void SetProjectFloat(string key, float value)
        {
            SetProjectData(key, value, EditorPrefs.SetFloat);
        }
        
        public static void SetProjectString(string key, string value)
        {
            SetProjectData(key, value, EditorPrefs.SetString);
        }
        
        public static string GetGlobalString(string key, string defaultValue = default)
        {
            return GetGlobalData(key, defaultValue, EditorPrefs.GetString);
        }
        
        public static void SetGlobalString(string key, string value)
        {
            SetGlobalData(key, value, EditorPrefs.SetString);
        }
        
        public static bool GetGlobalBool(string key, bool defaultValue = default)
        {
            return GetGlobalData(key, defaultValue, EditorPrefs.GetBool);
        }
        
        public static void SetGlobalBool(string key, bool value)
        {
            SetGlobalData(key, value, EditorPrefs.SetBool);
        }
        
        public static float GetGlobalFloat(string key, float defaultValue = default)
        {
            return GetGlobalData(key, defaultValue, EditorPrefs.GetFloat);
        }
        
        public static void SetGlobalFloat(string key, float value)
        {
            SetGlobalData(key, value, EditorPrefs.SetFloat);
        }
        
        private static void SetProjectData<T>(string key, T value, Action<string, T> Set)
        {
            var projectKey = ProjectPrefix + key;
            var globalKey = GlobalPrefix + key;
            if (EditorPrefs.HasKey(globalKey))
            {
                Debug.Log($"Should not reach this point. {globalKey}, {value}");
                return;
            }
            Set(projectKey, value);
        }
        
        private static T GetProjectData<T>(string key, T defaultValue, Func<string, T, T> Get, Action<string, T> Set)
        {
            var projectKey = ProjectPrefix + key;
            var globalKey = GlobalPrefix + key;
            if (!EditorPrefs.HasKey(globalKey))
            {
                return Get(projectKey, defaultValue);
            }
            // Migrate the value and key
            var data = Get(globalKey, defaultValue);
            Set(projectKey, data);
            EditorPrefs.DeleteKey(globalKey);
            return data;
        }
        
        private static void SetGlobalData<T>(string key, T value, Action<string, T> Set)
        {
            Set(key, value);
        }
        
        private static T GetGlobalData<T>(string key, T defaultValue, Func<string, T, T> Get)
        {
            return Get(key, defaultValue);
        }

        public static void DeleteProjectKey(string key)
        {
            var projectKey = ProjectPrefix + key;
            EditorPrefs.DeleteKey(projectKey);
        }

        private static string GetProjectName()
        {
            var s = Application.dataPath.Split('/');
            var projectName = s[s.Length - 2];
            return projectName;
        }
    }
}