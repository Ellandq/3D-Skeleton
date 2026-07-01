using System;
using System.Collections.Generic;
using System.Linq;
using GameInput;
using UnityEditor;
using UnityEngine;
using UserInterface.Screen.Components.Settings.UserInterface.Screen.Components.Settings;
using Utils.SO.Input;

namespace Editor.Custom
{
    [CustomPropertyDrawer(typeof(InputKeySpriteDictionary))]
    public class InputKeySpriteDictionaryEditor : PropertyDrawer
    {
        private AllowedInputKeys _allowedInputKeys;

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
            if (!_allowedInputKeys)
            {
                _allowedInputKeys = LoadAllowedInputKeys();
            }

            if (!_allowedInputKeys)
            {
                EditorGUI.HelpBox(
                    position,
                    "Missing AllowedInputKeys asset.",
                    MessageType.Error);

                return;
            }

            var line = EditorGUIUtility.singleLineHeight;

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


            var y = position.y + line + 4;


            for (var i = 0; i < list.arraySize; i++)
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


            if (!GUI.Button(
                    new Rect(
                        position.x,
                        y,
                        position.width,
                        line),
                    "Add")) return;
            list.InsertArrayElementAtIndex(list.arraySize);

            var newEntry =
                list.GetArrayElementAtIndex(list.arraySize - 1);

            newEntry.FindPropertyRelative("key").stringValue = "";
            newEntry.FindPropertyRelative("value").objectReferenceValue = null;

            property.serializedObject.ApplyModifiedProperties();
        }


        private void PopulateKeys(SerializedProperty entries)
        {
            entries.arraySize = 0;

            foreach (var category in _allowedInputKeys.categories)
            {
                foreach (var option in category.keys)
                {
                    if (!option.isAllowed)
                        continue;

                    var index = entries.arraySize;

                    entries.InsertArrayElementAtIndex(index);

                    var entry =
                        entries.GetArrayElementAtIndex(index);

                    entry.FindPropertyRelative("key").stringValue =
                        option.value;

                    entry.FindPropertyRelative("value").objectReferenceValue =
                        null;
                }
            }
        }


        private void DrawDropdown(
            Rect rect,
            SerializedProperty property)
        {
            var current = property.stringValue;


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


            foreach (var category in _allowedInputKeys.categories)
            {
                foreach (var option in category.keys)
                {
                    if (!option.isAllowed)
                        continue;
                    
                    var selected = option.value;

                    menu.AddItem(
                        new GUIContent(
                            $"{category.categoryName}/{option.value}"),
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
        
        private static AllowedInputKeys LoadAllowedInputKeys()
        {
            var guids = AssetDatabase.FindAssets("t:AllowedInputKeys");

            if (guids.Length != 0)
                return AssetDatabase.LoadAssetAtPath<AllowedInputKeys>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
            Debug.LogError("AllowedInputKeys asset not found.");
            return null;

        }
    }
}