using System;
using System.Collections.Generic;
using System.Linq;
using Model.Enum.GameInput;
using Model.SO.Input;
using Model.SO.Settings;
using Model.SO.Settings.Screen;
using Model.SO.Settings.Utils.SO.Settings;
using UnityEditor;
using UnityEngine;

namespace Editor.Custom
{
    [CustomEditor(typeof(InputAssignments))]
    public class InputAssignmentsEditor : UnityEditor.Editor
    {
        private InputAssignments _target;
        private string[] _allKeys;
        private AllowedInputKeys _allowedInputKeys;

        private const float MinColumnWidth = 80f;


        private void OnEnable()
        {
            _target = (InputAssignments)target;

            _allKeys = FindInputSettings();

            _allowedInputKeys = LoadAllowedInputKeys();
        }

        private static AllowedInputKeys LoadAllowedInputKeys()
        {
            var guids = AssetDatabase.FindAssets("t:AllowedInputKeys");

            if (guids.Length != 0)
                return AssetDatabase.LoadAssetAtPath<AllowedInputKeys>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
            Debug.LogError("AllowedInputKeys asset not found.");
            return null;

        }

        public override void OnInspectorGUI()
        {
            _target.assignments ??= new List<InputAssignmentEntry>();

            var existing = _target.assignments
                .ToDictionary(
                    x => x.settingName,
                    x => x);

            var rebuilt = new List<InputAssignmentEntry>();

            foreach (var key in _allKeys)
            {
                if (existing.TryGetValue(key, out var entry))
                {
                    rebuilt.Add(entry);
                }
                else
                {
                    rebuilt.Add(new InputAssignmentEntry
                    {
                        settingName = key,
                        action = PlayerAction.Action1,
                        baseValue = "",
                        altValue = ""
                    });
                }
            }

            _target.assignments = rebuilt;


            var totalWidth = EditorGUIUtility.currentViewWidth - 40f;

            var columnWidth =
                Mathf.Max(
                    MinColumnWidth,
                    totalWidth / 4f);


            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };


            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                "Setting",
                headerStyle,
                GUILayout.Width(columnWidth));

            EditorGUILayout.LabelField(
                "Action",
                headerStyle,
                GUILayout.Width(columnWidth));

            EditorGUILayout.LabelField(
                "Base",
                headerStyle,
                GUILayout.Width(columnWidth));

            EditorGUILayout.LabelField(
                "Alt",
                headerStyle,
                GUILayout.Width(columnWidth));

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space(4);


            for (var i = 0; i < _allKeys.Length; i++)
            {
                var entry = _target.assignments[i];

                entry.settingName = _allKeys[i];
                
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                
                EditorGUILayout.LabelField(
                    entry.settingName,
                    GUILayout.Width(columnWidth));


                var newAction = (PlayerAction)EditorGUILayout.EnumPopup(
                    entry.action,
                    GUILayout.Width(columnWidth));

                if (newAction != entry.action)
                {
                    Undo.RecordObject(_target, "Change Input Action");
                    entry.action = newAction;
                    EditorUtility.SetDirty(_target);
                }


                DrawCustomDropdown(
                    entry.baseValue,
                    columnWidth,
                    value => entry.baseValue = value);

                DrawCustomDropdown(
                    entry.altValue,
                    columnWidth,
                    value => entry.altValue = value);


                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(2);
            }


            if (!GUI.changed) return;
            Undo.RecordObject(_target, "Modify Input Assignments");
            EditorUtility.SetDirty(_target);
        }


        private void DrawCustomDropdown(
            string current,
            float width,
            Action<string> onChanged)
        {
            var rect = GUILayoutUtility.GetRect(
                width,
                EditorGUIUtility.singleLineHeight);


            if (!GUI.Button(
                    rect,
                    string.IsNullOrEmpty(current)
                        ? "None"
                        : current,
                    EditorStyles.popup))
            {
                return;
            }


            var menu = new GenericMenu();


            menu.AddItem(
                new GUIContent("None"),
                string.IsNullOrEmpty(current),
                () =>
                {
                    Undo.RecordObject(
                        _target,
                        "Clear Input Assignment");

                    onChanged("");

                    EditorUtility.SetDirty(_target);
                    AssetDatabase.SaveAssets();
                });


            foreach (var category in _allowedInputKeys.categories)
            {
                foreach (var selected in from option in category.keys where option.isAllowed select option.value)
                {
                    menu.AddItem(
                        new GUIContent(
                            $"{category.categoryName}/{selected}"),
                        current == selected,
                        () =>
                        {
                            Undo.RecordObject(
                                _target,
                                "Change Input Assignment");

                            onChanged(selected);

                            EditorUtility.SetDirty(_target);
                            AssetDatabase.SaveAssets();
                        });
                }
            }


            menu.DropDown(rect);
        }


        private static string[] FindInputSettings()
        {
            const string root =
                "Assets/ScriptableObjects/Settings/Pages";


            var pages =
                AssetDatabase.FindAssets(
                        "t:SettingsPageSO",
                        new[] { root })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(
                        AssetDatabase
                            .LoadAssetAtPath<SettingsPageSO>)
                    .Where(x => x != null);


            var keyBindingPage =
                pages.FirstOrDefault(
                    x => x.pageName.Equals(
                        "Key Bindings",
                        StringComparison.OrdinalIgnoreCase));


            if (keyBindingPage != null)
            {
                return keyBindingPage.categories
                    .SelectMany(x => x.items)
                    .Where(x => x != null)
                    .Select(x => x.settingName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToArray();
            }


            Debug.LogWarning(
                "Could not find Key Bindings settings page.");

            return Array.Empty<string>();
        }
    }
}