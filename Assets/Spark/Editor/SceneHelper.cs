using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
    public class SceneHelper : EditorWindow
    {
        private static bool _loaded;
       public static void Load()
       {
           if (!_loaded) 
               SceneView.duringSceneGui += OnSceneGUI;
           _loaded = true;
       }

       private static void OnSceneGUI(SceneView sceneView)
       {
           Handles.BeginGUI();
           const int width = 130;
           GUILayout.BeginArea(new Rect(sceneView.position.width / 2f - width / 2f, 50, width , 40));
   
           if (AISettings.ShowEditSceneButton.GetValue() && GUILayout.Button("Edit scene with AI"))
           {
               OnButtonClicked();
           }
   
           GUILayout.EndArea();
           
           Handles.EndGUI();
       }
   
       private static void OnButtonClicked()
       {
           // Write an editor dialog window saying coming soon but to contact us for early access
           EditorUtility.DisplayDialog("Coming soon", 
               "The AI scene editor is coming soon, we are still in the process of testing it. " +
               "If you would like to be an early access tester, please contact us", "OK");
           
           
           //var window = GetWindow<AISceneEditor>("Edit scene with AI");
           //window.Show();
       }
   }
}