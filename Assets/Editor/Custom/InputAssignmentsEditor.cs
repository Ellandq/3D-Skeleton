using System;
using System.Collections.Generic;
using System.Linq;
using GameInput;
using UnityEditor;
using UnityEngine;
using Utils.SO.Settings;

namespace Editor.Custom
{
    [CustomEditor(typeof(InputAssignments))]
    public class InputAssignmentsEditor : UnityEditor.Editor
    {
        private InputAssignments _target;
        private string[] _allKeys;
        private Dictionary<string, string[]> _categories;

        private const float MinColumnWidth = 80f;

        private void OnEnable()
        {
            _target = (InputAssignments)target;
            _allKeys = Enum.GetNames(typeof(PlayerAction));

            _categories = new Dictionary<string, string[]>
            {
                { "Mouse Buttons", Enum.GetNames(typeof(MouseKey)) },
                { "Letters", Enumerable.Range('A', 26).Select(i => ((char)i).ToString()).ToArray() },
                { "Numbers", Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray() },
                { "F Keys", Enumerable.Range(1, 12).Select(i => "F" + i).ToArray() },
                { "Common Keys", new[] { "Space", "Backspace", "Tab", "Enter", "Escape", "Shift", "Control", "Alt" } },
                { "Others", Enum.GetNames(typeof(KeyCode))
                    .Except(Enumerable.Range('A',26).Select(i => ((char)i).ToString()))
                    .Except(Enumerable.Range(0,10).Select(i => i.ToString()))
                    .Except(Enumerable.Range(1,12).Select(i => "F"+i))
                    .Except(new[] { "Space", "Backspace", "Tab", "Enter", "Escape", "Shift", "Control", "Alt" })
                    .ToArray()
                }
            };
        }

        public override void OnInspectorGUI()
        {
            _target.settingNames ??= new List<string>();
            _target.defaultValues ??= new List<string>();

            var rowCount = _allKeys.Length;
            while (_target.settingNames.Count < rowCount * 2)
            {
                _target.settingNames.Add("");
                _target.defaultValues.Add("");
            }

            EditorGUILayout.Space();

            var totalWidth = EditorGUIUtility.currentViewWidth - 40f;
            var columnWidth = Mathf.Max(MinColumnWidth, totalWidth / 3f);

            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key", headerStyle, GUILayout.Width(columnWidth));
            EditorGUILayout.LabelField("Base", headerStyle, GUILayout.Width(columnWidth));
            EditorGUILayout.LabelField("Alt", headerStyle, GUILayout.Width(columnWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f);

            for (var i = 0; i < rowCount; i++)
            {
                var key = _allKeys[i];
                _target.settingNames[i * 2] = key;
                _target.settingNames[i * 2 + 1] = key + "_Alt";

                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.LabelField(key, GUILayout.Width(columnWidth));

                _target.defaultValues[i * 2] = DrawCustomDropdown(i * 2, columnWidth);
                _target.defaultValues[i * 2 + 1] = DrawCustomDropdown(i * 2 + 1, columnWidth);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;

            if (GUI.changed)
                EditorUtility.SetDirty(_target);
        }

        private string DrawCustomDropdown(int index, float width)
        {
            var current = _target.defaultValues[index];
            var rect = GUILayoutUtility.GetRect(width, EditorGUIUtility.singleLineHeight);

            if (!GUI.Button(rect, string.IsNullOrEmpty(current) ? "None" : current, EditorStyles.popup))
                return _target.defaultValues[index];
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(current), () =>
            {
                _target.defaultValues[index] = "";
                EditorUtility.SetDirty(_target);
            });

            foreach (var category in _categories)
            {
                foreach (var option in category.Value)
                {
                    var selectedOption = option;
                    var path = category.Key + "/" + option;

                    menu.AddItem(new GUIContent(path), option == current, () =>
                    {
                        _target.defaultValues[index] = selectedOption;
                        EditorUtility.SetDirty(_target);
                    });
                }
            }

            menu.DropDown(rect);

            return _target.defaultValues[index];
        }
    }
}