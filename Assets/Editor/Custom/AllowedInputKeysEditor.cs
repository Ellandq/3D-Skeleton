using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Model.Enum.GameInput;
using Model.SO.Input;

namespace Editor.Custom
{
    [CustomEditor(typeof(AllowedInputKeys))]
    public class AllowedInputKeysEditor : UnityEditor.Editor
    {
        private AllowedInputKeys _target;


        private void OnEnable()
        {
            _target = (AllowedInputKeys)target;
        }


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();


            if (GUILayout.Button("Generate Input List"))
            {
                Generate();
            }


            EditorGUILayout.Space();


            foreach (var category in _target.categories)
            {
                EditorGUILayout.LabelField(
                    category.categoryName,
                    EditorStyles.boldLabel);


                EditorGUI.indentLevel++;

                foreach (var key in category.keys)
                {
                    key.isAllowed = EditorGUILayout.Toggle(
                        key.value,
                        key.isAllowed);
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
            }


            if (GUI.changed)
            {
                EditorUtility.SetDirty(_target);
            }
        }


        private void Generate()
        {
            _target.categories.Clear();


            _target.categories.Add(
                CreateCategory(
                    InputKeyType.MouseKey,
                    nameof(MouseKey),
                    Enum.GetNames(typeof(MouseKey)))
            );


            _target.categories.Add(
                CreateCategory(
                    InputKeyType.KeyCode,
                    "KeyCode/Letters",
                    Enumerable.Range('A', 26)
                        .Select(i =>
                            ((KeyCode)((int)KeyCode.A + (i - 'A')))
                            .ToString())
                        .ToArray())
            );


            _target.categories.Add(
                CreateCategory(
                    InputKeyType.KeyCode,
                    "KeyCode/Numbers",
                    Enumerable.Range(0, 10)
                        .Select(i =>
                            ((KeyCode)((int)KeyCode.Alpha0 + i))
                            .ToString())
                        .ToArray())
            );


            _target.categories.Add(
                CreateCategory(
                    InputKeyType.KeyCode,
                    "KeyCode/FKeys",
                    Enumerable.Range(1, 12)
                        .Select(i =>
                            ((KeyCode)((int)KeyCode.F1 + (i - 1)))
                            .ToString())
                        .ToArray())
            );


            _target.categories.Add(
                CreateCategory(
                    InputKeyType.KeyCode,
                    "KeyCode/Common",
                    new[]
                    {
                        nameof(KeyCode.Space),
                        nameof(KeyCode.Backspace),
                        nameof(KeyCode.Tab),
                        nameof(KeyCode.Return),
                        nameof(KeyCode.Escape),
                        nameof(KeyCode.LeftShift),
                        nameof(KeyCode.RightShift),
                        nameof(KeyCode.LeftControl),
                        nameof(KeyCode.RightControl),
                        nameof(KeyCode.LeftAlt),
                        nameof(KeyCode.RightAlt)
                    })
            );


            var excluded = _target.categories
                .SelectMany(x => x.keys)
                .Select(x => x.value)
                .ToHashSet();


            _target.categories.Add(
                CreateCategory(
                    InputKeyType.KeyCode,
                    "KeyCode/Other",
                    Enum.GetNames(typeof(KeyCode))
                        .Where(x => !excluded.Contains(x))
                        .ToArray())
            );


            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
        }


        private InputKeyCategory CreateCategory(
            InputKeyType type,
            string name,
            string[] values)
        {
            var enumType =
                type == InputKeyType.MouseKey
                    ? typeof(MouseKey)
                    : typeof(KeyCode);


            return new InputKeyCategory
            {
                type = type,
                categoryName = name,

                keys = values
                    .Select(x =>
                    {
                        var enumValue =
                            (Enum)Enum.Parse(
                                enumType,
                                x);


                        return new InputKeyEntry
                        {
                            value = x,
                            intValue = Convert.ToInt32(enumValue),
                            isAllowed = true
                        };
                    })
                    .ToList()
            };
        }
    }
}