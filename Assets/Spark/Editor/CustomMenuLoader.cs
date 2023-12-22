using UnityEditor;

namespace LeastSquares.Spark
{
    [InitializeOnLoad]
    public class CustomMenuLoader
    {
        private const string ProjectOpenedKey = "ProjectOpened";
        
        static CustomMenuLoader()
        {
            EditorApplication.delayCall += ShowCustomMenuWindow;
        }

        private static void ShowCustomMenuWindow()
        {
            var value = SaveSystem.GetProjectBool(ProjectOpenedKey);
            if (!value)
            {
                EditorInstallationWindow.ShowWindow();
                SaveSystem.SetProjectBool(ProjectOpenedKey, true);
            }

            SceneHelper.Load();
            AISettings.Load();
        }
    }
    
}