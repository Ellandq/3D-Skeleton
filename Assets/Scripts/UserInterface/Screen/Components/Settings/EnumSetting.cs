using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Enum;
using Utils.SO;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen.Components.Settings
{
    public class EnumSetting : SettingBase<int>
    {
        [Header("Object References")] 
        [SerializeField] private Button leftButton;
        [SerializeField] private Image leftButtonImage;
        [SerializeField] private Button rightButton;
        [SerializeField] private Image rightButtonImage;
        [SerializeField] private Image frame;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text optionName;
        [SerializeField] private Transform itemSelectionPreviewParent;

        [SerializeField] private List<Image> previewItems;

        [Header("Prefabs")] 
        [SerializeField] private GameObject previewItemPrefab;

        [Header("Settings")] 
        [SerializeField] private List<string> availableValues;
        [SerializeField] private int selectedIndex;
        [SerializeField] private int startingValue;
        [SerializeField] private bool wasChanged;
        
        public override void Initialize(
            SettingsPageItemSO asset, 
            Action<string> onSelect,  
            Action<(string key, int value)> onValueChange,
            Action<(string key, int value)> onValueReset, 
            UIComponentState defaultState)
        {
            wasChanged = false;
            var enumType = Type.GetType(asset.EnumTypeName);

            if (enumType is not { IsEnum: true })
            {
                throw new ArgumentException($"Invalid enum type name: {asset.EnumTypeName}");
            }
            
            availableValues = Enum.GetNames(enumType).Select(FormatEnumValue).ToList();
            selectedIndex = availableValues.IndexOf(FormatEnumValue(asset.EnumDefaultValue));
            
            InitializePreview();
            base.Initialize(asset, onSelect, onValueChange, onValueReset, defaultState);
            
            leftButton.interactable = selectedIndex != 0;
            rightButton.interactable = selectedIndex != availableValues.Count - 1;
            optionName.text = availableValues[selectedIndex];
            startingValue = selectedIndex;
        }

        private void InitializePreview()
        {
            for (var i = itemSelectionPreviewParent.childCount - 1; i >= 0; i--)
            {
                var child = itemSelectionPreviewParent.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.Undo.DestroyObjectImmediate(child.gameObject);
                else
                    Destroy(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
            previewItems = new List<Image>();
            foreach (var item in availableValues.Select(_ => Instantiate(previewItemPrefab, itemSelectionPreviewParent)))
            {
                previewItems.Add(item.GetComponent<Image>());
            }
        }

        public override void ChangeState(UIComponentState newState)
        {
            base.ChangeState(newState);
            var colors = UITheme.GetColors(newState);
            var lighterC = colors[UIColorType.Lighter];
            var lightC = colors[UIColorType.Light];
            var darkerC = colors[UIColorType.Darker];

            leftButtonImage.color = lighterC;
            rightButtonImage.color = lighterC;
            frame.color = lighterC;
            optionName.color = lighterC;
            background.color = darkerC;

            for (var i = 0; i < previewItems.Count; i++)
            {
                previewItems[i].color = i == selectedIndex
                    ? lighterC
                    : lightC;
            }
        }
        
        public void ChangeSelection(bool increment)
        {
            if (increment && selectedIndex < availableValues.Count - 1)
            {
                previewItems[selectedIndex].color = UITheme.GetColor(State, UIColorType.Light);
                selectedIndex++;
                previewItems[selectedIndex].color = UITheme.GetColor(State, UIColorType.Lighter);
            } 
            else if (selectedIndex > 0)
            {
                    previewItems[selectedIndex].color = UITheme.GetColor(State, UIColorType.Light);
                selectedIndex--;
                previewItems[selectedIndex].color = UITheme.GetColor(State, UIColorType.Lighter);
            }
            
            leftButton.interactable = selectedIndex != 0;
            rightButton.interactable = selectedIndex != availableValues.Count - 1;
            optionName.text = availableValues[selectedIndex];

            if (startingValue == selectedIndex && wasChanged)
            {
                _onValueReset?.Invoke((fullName, selectedIndex));
                wasChanged = false;
            }
            else if (startingValue != selectedIndex)
            {
                _onValueChange?.Invoke((fullName, selectedIndex));
                wasChanged = true;
            }
        }
        
        private static string FormatEnumValue(string enumValue)
        {
            if (string.IsNullOrEmpty(enumValue))
                return enumValue;

            if (enumValue.Length > 1 && char.IsLetter(enumValue[0]) && char.IsDigit(enumValue[1]))
            {
                if (enumValue[0] == 'X')
                    enumValue = "x" + enumValue[1..];
                else
                    enumValue = enumValue[1..];
            }

            enumValue = enumValue.Replace("_", "x");

            var result = new StringBuilder();
            for (var i = 0; i < enumValue.Length; i++)
            {
                var c = enumValue[i];
                if (i > 0 && char.IsUpper(c) && (char.IsLower(enumValue[i - 1]) || char.IsDigit(enumValue[i - 1])))
                {
                    result.Append(' ');
                }
                result.Append(c);
            }

            return result.ToString();
        }
    }
}