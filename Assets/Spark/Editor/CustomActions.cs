using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class CustomContextMenuItems : MonoBehaviour
    {
        [MenuItem("Assets/Spark AI - Edit Script", false, 1000)]
        public static void EditCSharpScript()
        {
            Edit();
        }

        [MenuItem("Assets/Spark AI - Edit Shader", false, 1000)]
        public static void EditShader()
        {
            Edit();
        }
        
        [MenuItem("Assets/Spark AI - Create Script", false, 1000)]
        public static void CreateCSharpScript()
        {
            DoCreateScript();
        }

        [MenuItem("Assets/Spark AI - Create Shader", false, 1000)]
        public static void CreateShader()
        {
            DoCreateShader();
        }
        
        private static void DoCreateShader()
        {
            ComponentActions.OpenComponentWindow<IterativeShaderCoderComponentWindow>(null, "New shader", w =>
            {
                w.SetMode(CodeMode.AddComponent);
                w.isOnlyCreating = true;
            });
        }
        
        private static void DoCreateScript()
        {
            ComponentActions.OpenComponentWindow<IterativeCoderComponentWindow>(null, "New script", w =>
            {
                w.SetMode(CodeMode.AddComponent);
                w.isOnlyCreating = true;
            });
        }
        
        private static void Edit()
        {
            var selectedObject = Selection.activeObject;
            var assetPath = AssetDatabase.GetAssetPath(selectedObject);

            if (selectedObject == null) return;
            if (assetPath.EndsWith(".cs"))
            {
                CustomMonoBehaviourInspector.OnButtonClick(selectedObject as MonoScript);
            }
            else if (assetPath.EndsWith(".shader"))
            {
                var shader = selectedObject as Shader;
                CustomShaderBehaviour.OnButtonClick(shader);
            }
        }
        

        [MenuItem("Assets/Spark AI - Edit Shader", true)]
        public static bool ValidateShaderEdit() => ValidateFileSelection(".shader", false);
        
        [MenuItem("Assets/Spark AI - Create Shader", true)]
        public static bool ValidateShaderCreate() => ValidateFileSelection(".shader", true);
        
        [MenuItem("Assets/Spark AI - Edit Script", true)]
        public static bool ValidateScriptEdit() => ValidateFileSelection(".cs", false);
        
        [MenuItem("Assets/Spark AI - Create Script", true)]
        public static bool ValidateScriptCreate() => ValidateFileSelection(".cs", true);
        
        public static bool ValidateFileSelection(string endsWidth, bool needsFolder)
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(selectedObject);
            if (needsFolder)
            {
                if (AssetDatabase.IsValidFolder(path))
                {
                    var folder = System.IO.Path.GetDirectoryName(path);
                    //TODO create the script in the folder
                    return true;
                }

                return false;
            }
            
            return path.EndsWith(endsWidth);
        }
    }
}