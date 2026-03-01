using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Editor.CommandCenter.Utils
{
    public static class HierarchyStateHelper
    {
        private static readonly Type SceneHierarchyWindowType;
        private static readonly EditorWindow SceneHierarchyWindow;

        static HierarchyStateHelper()
        {
            SceneHierarchyWindowType = typeof(EditorWindow).Assembly
                .GetType("UnityEditor.SceneHierarchyWindow");
            if (SceneHierarchyWindowType != null)
            {
                SceneHierarchyWindow = EditorWindow.GetWindow(SceneHierarchyWindowType);
            }
        }

        public static HashSet<int> CacheExpanded()
        {
            var expanded = new HashSet<int>();
            if (SceneHierarchyWindow == null) return expanded;

            var isExpandedMethod = SceneHierarchyWindowType.GetMethod(
                "IsExpanded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (isExpandedMethod != null &&
                    (bool)isExpandedMethod.Invoke(SceneHierarchyWindow, new object[] { go.GetInstanceID() }))
                    expanded.Add(go.GetInstanceID());
            }

            return expanded;
        }
        
        public static void RestoreExpanded(HashSet<int> expanded)
        {
            if (!SceneHierarchyWindow) return;

            var setExpandedMethod = SceneHierarchyWindowType.GetMethod(
                "SetExpandedRecursive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var shouldExpand = expanded.Contains(go.GetInstanceID());
                if (setExpandedMethod != null)
                    setExpandedMethod.Invoke(SceneHierarchyWindow, new object[] { go.GetInstanceID(), shouldExpand });
            }
        }
        
        public static void PreserveHierarchy(Action action)
        {
            var expanded = CacheExpanded();
            var previousSelection = Selection.objects;
            action.Invoke();
            RestoreExpanded(expanded);
            Selection.objects = previousSelection;
        }
    }
}