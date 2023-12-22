using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class ScriptCreator
    {
        public static void CreateScriptAndReload(string code, string path)
        {
            CreateScriptAsset(code, path);
            AssetDatabase.Refresh();
        }
        
        public static void CreateScriptAsset(string code, string path)
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
            method.Invoke(null, new object[]{path, code});
        }

        public static void EditScript(string path, string newSource)
        {
            File.WriteAllText(path, newSource);
            AssetDatabase.Refresh();
        }
    }
}