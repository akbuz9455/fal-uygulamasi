using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Spark
{
   /* public class CustomAIDebugConsole : EditorWindow
   {
       private Vector2 scrollPosition;
       private List<LogEntry> logEntries = new List<LogEntry>();

       [MenuItem("Window/Custom AI Debug Console")]
       public static void ShowWindow()
       {
           GetWindow<CustomAIDebugConsole>("Custom AI Debug Console");
       }

       private void OnEnable()
       {
           Application.logMessageReceived += HandleLog;
       }

       private void OnDisable()
       {
           Application.logMessageReceived -= HandleLog;
       }

       private void HandleLog(string logString, string stackTrace, LogType type)
       {
           logEntries.Add(new LogEntry(logString, stackTrace, type));
           Repaint();
       }

       private void OnGUI()
       {
           EditorGUILayout.BeginVertical();

           scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

           foreach (var entry in logEntries)
           {
               EditorGUILayout.BeginHorizontal();

               EditorGUILayout.LabelField($"[{entry.type}] {entry.logString}");

               if (entry.type == LogType.Error || entry.type == LogType.Exception)
               {
                   if (GUILayout.Button("Debug with AI", GUILayout.Width(100)))
                   {
                       OnErrorButtonClicked(entry);
                   }
               }

               EditorGUILayout.EndHorizontal();
           }

           EditorGUILayout.EndScrollView();

           EditorGUILayout.EndVertical();
       }

       private void OnErrorButtonClicked(LogEntry entry)
       {
           Debug.Log($"Debug with AI button clicked for: {entry.logString}");
           // Add your custom AI debugging functionality here.
       }

       private class LogEntry
       {
           public string logString;
           public string stackTrace;
           public LogType type;

           public LogEntry(string logString, string stackTrace, LogType type)
           {
               this.logString = logString;
               this.stackTrace = stackTrace;
               this.type = type;
           }
       }
   }*/
}