#if UNITY_EDITOR

using Managers;
using UnityEditor;
using UnityEngine;

namespace Editor.Custom
{
    [CustomEditor(typeof(SettingsManager))]
    public class SettingsManagerEditor : UnityEditor.Editor
    {
        private SettingsManager _manager;

        private bool _showInts;
        private bool _showFloats;
        private bool _showStrings;

        private void OnEnable()
        {
            _manager = (SettingsManager)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "Cached Settings",
                EditorStyles.boldLabel
            );

            if (!_manager)
                return;


            DrawIntSettings();
            DrawFloatSettings();
            DrawStringSettings();

            if (GUILayout.Button("Clear Cache"))
            {
                ClearCache();
            }

            if (GUILayout.Button("Save PlayerPrefs"))
            {
                PlayerPrefs.Save();
            }
        }


        private void DrawIntSettings()
        {
            _showInts = EditorGUILayout.Foldout(
                _showInts,
                $"Int Settings ({_manager.DebugInts.Count})",
                true
            );

            if (!_showInts)
                return;


            EditorGUI.indentLevel++;

            foreach (var pair in _manager.DebugInts)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(pair.Key);

                EditorGUILayout.IntField(
                    pair.Value,
                    GUILayout.Width(80)
                );

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }


        private void DrawFloatSettings()
        {
            _showFloats = EditorGUILayout.Foldout(
                _showFloats,
                $"Float Settings ({_manager.DebugFloats.Count})",
                true
            );

            if (!_showFloats)
                return;


            EditorGUI.indentLevel++;

            foreach (var pair in _manager.DebugFloats)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(pair.Key);

                EditorGUILayout.FloatField(
                    pair.Value,
                    GUILayout.Width(80)
                );

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }


        private void DrawStringSettings()
        {
            _showStrings = EditorGUILayout.Foldout(
                _showStrings,
                $"String Settings ({_manager.DebugStrings.Count})",
                true
            );

            if (!_showStrings)
                return;


            EditorGUI.indentLevel++;

            foreach (var pair in _manager.DebugStrings)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(pair.Key);

                EditorGUILayout.TextField(
                    pair.Value
                );

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }


        private void ClearCache()
        {
            var intField =
                typeof(SettingsManager)
                    .GetField(
                        "cachedIntChanges",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            var floatField =
                typeof(SettingsManager)
                    .GetField(
                        "cachedFloatChanges",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            var stringField =
                typeof(SettingsManager)
                    .GetField(
                        "cachedStringChanges",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);


            ((System.Collections.IDictionary)intField.GetValue(_manager)).Clear();
            ((System.Collections.IDictionary)floatField.GetValue(_manager)).Clear();
            ((System.Collections.IDictionary)stringField.GetValue(_manager)).Clear();
        }
    }
}

#endif