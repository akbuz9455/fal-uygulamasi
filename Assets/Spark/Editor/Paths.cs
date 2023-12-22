using System.IO;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class Paths
    {
        public static string RelativePathScriptSaveLocation { get; set; }

        private static void EnsureFoldersExist(string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static string GetPathInScriptsFolder(string fileName)
        {
            var path = $"{Application.dataPath}/{RelativePathScriptSaveLocation}/{fileName}";
            EnsureFoldersExist(path);
            return path;
        }
        
        public static string GetPathInProjectTempFolder(string fileName)
        {
            var path = $"{Application.dataPath}/Spark/Temp/{fileName}";
            EnsureFoldersExist(path);
            return path;
        }
        
        public static string GetPathInProjectTempFolderRelative(string fileName)
        {
            var path = GetPathInProjectTempFolder(fileName);
            return "Assets/" + path.Replace(Application.dataPath, "");
        }
    }
}