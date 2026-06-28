using System;
using System.Collections.Generic;
using System.Linq;
using GameInput;
using UnityEditor;
using UnityEngine;
using UserInterface.Screen.Components.Settings.UserInterface.Screen.Components.Settings;

namespace Editor.Custom
{
    [CustomPropertyDrawer(typeof(InputKeySpriteDictionary))]
    public class InputKeySpriteDictionaryEditor : PropertyDrawer
    {
        private Dictionary<string, string[]> _categories;


        private void BuildCategories()
        {
            if (_categories != null)
                return;

            _categories = new Dictionary<string, string[]>
            {
                {
                    nameof(MouseKey),
                    Enum.GetNames(typeof(MouseKey))
                },

                {
                    "KeyCode/Letters",
                    Enumerable.Range('A', 26)
                        .Select(i =>
                            ((KeyCode)((int)KeyCode.A + (i - 'A'))).ToString())
                        .ToArray()
                },

                {
                    "KeyCode/Numbers",
                    Enumerable.Range(0, 10)
                        .Select(i =>
                            ((KeyCode)((int)KeyCode.Alpha0 + i)).ToString())
                        .ToArray()
                },

                {
                    "KeyCode/FKeys",
                    Enumerable.Range(1, 12)
                        .Select(i =>
                            ((KeyCode)((int)KeyCode.F1 + (i - 1))).ToString())
                        .ToArray()
                },

                {
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
                    }
                },

                {
                    "KeyCode/Other",
                    Enum.GetNames(typeof(KeyCode))
                        .Except(Enum.GetNames(typeof(MouseKey)))
                        .Except(
                            Enumerable.Range('A',26)
                                .Select(i =>
                                    ((KeyCode)((int)KeyCode.A + (i-'A'))).ToString()))
                        .Except(
                            Enumerable.Range(0,10)
                                .Select(i =>
                                    ((KeyCode)((int)KeyCode.Alpha0+i)).ToString()))
                        .Except(
                            Enumerable.Range(1,12)
                                .Select(i =>
                                    ((KeyCode)((int)KeyCode.F1+(i-1))).ToString()))
                        .Except(new[]
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
                        .ToArray()
                }
            };
        }


        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            var entries = property.FindPropertyRelative("entries");

            return
                EditorGUIUtility.singleLineHeight +
                (entries.arraySize + 2) *
                (EditorGUIUtility.singleLineHeight + 4);
        }


        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            BuildCategories();

            float line = EditorGUIUtility.singleLineHeight;

            Rect header = new(
                position.x,
                position.y,
                position.width,
                line
            );


            Rect foldout = new(
                header.x,
                header.y,
                header.width - 70,
                line
            );


            Rect refresh = new(
                header.xMax - 65,
                header.y,
                65,
                line
            );


            property.isExpanded =
                EditorGUI.Foldout(
                    foldout,
                    property.isExpanded,
                    label,
                    true);


            if (GUI.Button(refresh, "↻"))
            {
                var entries =
                    property.FindPropertyRelative("entries");

                PopulateKeys(entries);

                property.serializedObject.ApplyModifiedProperties();
            }


            if (!property.isExpanded)
                return;


            var list =
                property.FindPropertyRelative("entries");


            float y = position.y + line + 4;


            for (int i = 0; i < list.arraySize; i++)
            {
                var entry =
                    list.GetArrayElementAtIndex(i);


                var key =
                    entry.FindPropertyRelative("key");

                var value =
                    entry.FindPropertyRelative("value");


                Rect keyRect = new(
                    position.x,
                    y,
                    position.width * .45f,
                    line);


                Rect valueRect = new(
                    position.x + position.width * .48f,
                    y,
                    position.width * .42f,
                    line);


                Rect removeRect = new(
                    position.xMax - 22,
                    y,
                    22,
                    line);


                DrawDropdown(keyRect, key);

                EditorGUI.PropertyField(
                    valueRect,
                    value,
                    GUIContent.none);


                if (GUI.Button(removeRect, "-"))
                {
                    list.DeleteArrayElementAtIndex(i);
                    property.serializedObject.ApplyModifiedProperties();
                    break;
                }


                y += line + 4;
            }


            if (GUI.Button(
                new Rect(
                    position.x,
                    y,
                    position.width,
                    line),
                "Add"))
            {
                list.InsertArrayElementAtIndex(list.arraySize);

                var newEntry =
                    list.GetArrayElementAtIndex(list.arraySize - 1);

                newEntry.FindPropertyRelative("key").stringValue = "";
                newEntry.FindPropertyRelative("value").objectReferenceValue = null;

                property.serializedObject.ApplyModifiedProperties();
            }
        }


        private void PopulateKeys(SerializedProperty entries)
        {
            entries.arraySize = 0;


            foreach (var category in _categories)
            {
                if (category.Key == "KeyCode/Other")
                    continue;


                foreach (var option in category.Value)
                {
                    int index = entries.arraySize;

                    entries.InsertArrayElementAtIndex(index);


                    var entry =
                        entries.GetArrayElementAtIndex(index);


                    entry.FindPropertyRelative("key").stringValue =
                        option;

                    entry.FindPropertyRelative("value").objectReferenceValue =
                        null;
                }
            }
        }


        private void DrawDropdown(
            Rect rect,
            SerializedProperty property)
        {
            string current = property.stringValue;


            if (!GUI.Button(
                    rect,
                    string.IsNullOrEmpty(current)
                        ? "None"
                        : current,
                    EditorStyles.popup))
                return;


            GenericMenu menu = new();


            menu.AddItem(
                new GUIContent("None"),
                string.IsNullOrEmpty(current),
                () =>
                {
                    property.stringValue = "";
                    property.serializedObject.ApplyModifiedProperties();
                });


            foreach (var category in _categories)
            {
                foreach (var option in category.Value)
                {
                    string selected = option;

                    menu.AddItem(
                        new GUIContent(
                            $"{category.Key}/{option}"),
                        current == selected,
                        () =>
                        {
                            property.stringValue = selected;
                            property.serializedObject.ApplyModifiedProperties();
                        });
                }
            }


            menu.DropDown(rect);
        }
    }
}