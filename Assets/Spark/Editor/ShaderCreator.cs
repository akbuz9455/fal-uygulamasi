using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class ShaderCreator
    {
        public static Shader CreateTempShader(string name, string code)
        {
            var filename = Paths.GetPathInProjectTempFolder($"_{name}.shader");
            
            File.WriteAllText(filename, code);
            AssetDatabase.Refresh();
            return Shader.Find(ExtractShaderName(code));
        }

        private static string ExtractShaderName(string source)
        {
            var match = Regex.Match(source, @"Shader\s*""(.*?)""");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}