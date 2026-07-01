using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Enum.UI;
using Utils.SO;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen.Components.Settings.Common
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
        private Enum defaultEnumValue;

        public override void Initialize(
            SettingsPageItemSO itemAsset,
            Action<string> onSelect,
            Action<(string key, int value)> onValueChange,
            Action<(string key, int value)> onValueReset,
            UIComponentState defaultState)
        {
            wasChanged = false;

            var enumType = Type.GetType(itemAsset.EnumTypeName);
            if (enumType is not { IsEnum: true })
            {
                throw new ArgumentException($"Invalid enum type name: {itemAsset.EnumTypeName}");
            }

            defaultEnumValue = (Enum)Enum.Parse(enumType, itemAsset.EnumDefaultValue);

            startingValue = SettingsManager.GetIntSetting(
                itemAsset.fullName,
                Convert.ToInt32(defaultEnumValue)
            );
            
            selectedIndex = startingValue;
            
            availableValues = Enum.GetNames(enumType)
                .Select(FormatEnumValue)
                .ToList();

            InitializePreview();

            base.Initialize(itemAsset, onSelect, onValueChange, onValueReset, defaultState);

            UpdateSelectionUI();
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

            for (var i = 0; i < availableValues.Count; i++)
            {
                var item = Instantiate(previewItemPrefab, itemSelectionPreviewParent);
                previewItems.Add(item.GetComponent<Image>());
            }
        }

        public override void ChangeState(UIComponentState newState)
        {
            base.ChangeState(newState);

            var colors = UITheme.GetColors(newState);

            var lighter = colors[UIColorType.Lighter];
            var light = colors[UIColorType.Light];
            var darker = colors[UIColorType.Darker];

            leftButtonImage.color = lighter;
            rightButtonImage.color = lighter;
            frame.color = lighter;
            optionName.color = lighter;
            background.color = darker;

            for (var i = 0; i < previewItems.Count; i++)
            {
                previewItems[i].color = i == selectedIndex ? lighter : light;
            }
        }

        public void ChangeSelection(bool increment)
        {
            if (increment)
            {
                if (selectedIndex >= availableValues.Count - 1)
                    return;

                previewItems[selectedIndex].color = UITheme.GetColor(State, UIColorType.Light);
                selectedIndex++;
            }
            else
            {
                if (selectedIndex <= 0)
                    return;

                previewItems[selectedIndex].color = UITheme.GetColor(State, UIColorType.Light);
                selectedIndex--;
            }

            previewItems[selectedIndex].color = UITheme.GetColor(State, UIColorType.Lighter);

            UpdateSelectionUI();

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

        private void UpdateSelectionUI()
        {
            leftButton.interactable = selectedIndex != 0;
            rightButton.interactable = selectedIndex != availableValues.Count - 1;
            optionName.text = availableValues[selectedIndex];
        }

        private static string FormatEnumValue(string enumValue)
        {
            if (string.IsNullOrEmpty(enumValue))
                return enumValue;

            if (enumValue.Length > 1 &&
                char.IsLetter(enumValue[0]) &&
                char.IsDigit(enumValue[1]))
            {
                enumValue = enumValue[0] == 'X'
                    ? "x" + enumValue[1..]
                    : enumValue[1..];
            }

            enumValue = enumValue.Replace("_", "x");

            var result = new StringBuilder();

            for (var i = 0; i < enumValue.Length; i++)
            {
                var c = enumValue[i];

                if (i > 0 &&
                    char.IsUpper(c) &&
                    (char.IsLower(enumValue[i - 1]) || char.IsDigit(enumValue[i - 1])))
                {
                    result.Append(' ');
                }

                result.Append(c);
            }

            return result.ToString();
        }
        
        public override void ResetSetting(bool toDefault = false)
        {
            var newValue = toDefault ? Convert.ToInt32(defaultEnumValue) : startingValue;
            if (newValue == selectedIndex)
                return;
            selectedIndex = newValue;
            UpdateSelectionUI();
            _onValueChange?.Invoke((fullName, selectedIndex));
        }
    }
}