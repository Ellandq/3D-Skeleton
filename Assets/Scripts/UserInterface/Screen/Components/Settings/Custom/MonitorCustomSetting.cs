using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Enum;
using Utils.SO;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen.Components.Settings.Custom
{
    public class MonitorSetting : SettingBase<int>, ICustomSettingItem
    {
        public NamedCustomSetting GetSettingType() => NamedCustomSetting.Monitor;

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
        [SerializeField] private GameObject previewItemPrefab;

        [SerializeField] private List<string> availableValues;
        [SerializeField] private int selectedIndex;

        public override void Initialize(
            SettingsPageItemSO asset, 
            Action<string> onSelect,  
            Action<(string key, int value)> onValueChange,
            Action<(string key, int value)> onValueReset, 
            UIComponentState defaultState)
        {
            availableValues = GetMonitors();

            if (availableValues.Count == 0)
                throw new InvalidOperationException("No monitors detected.");

            selectedIndex = 0;

            InitializePreview();
            base.Initialize(asset, onSelect, onValueChange, onValueReset, defaultState);

            leftButton.interactable = selectedIndex > 0;
            rightButton.interactable = selectedIndex < availableValues.Count - 1;
            optionName.text = availableValues[selectedIndex];
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
            switch (increment)
            {
                case true when selectedIndex < availableValues.Count - 1:
                    previewItems[selectedIndex].color = UITheme.GetColor(State, UIColorType.Light);
                    selectedIndex++;
                    previewItems[selectedIndex].color = UITheme.GetColor(State, UIColorType.Lighter);
                    break;
                case false when selectedIndex > 0:
                    previewItems[selectedIndex].color = UITheme.GetColor(State, UIColorType.Light);
                    selectedIndex--;
                    previewItems[selectedIndex].color = UITheme.GetColor(State, UIColorType.Lighter);
                    break;
            }

            leftButton.interactable = selectedIndex > 0;
            rightButton.interactable = selectedIndex < availableValues.Count - 1;
            optionName.text = availableValues[selectedIndex];
        }
        
        private static List<string> GetMonitors()
        {
            var displays = Display.displays;
            var result = new List<string>();

            for (var i = 0; i < displays.Length; i++)
            {
                result.Add($"Monitor {i + 1}");
            }

            return result;
        }
    }
}