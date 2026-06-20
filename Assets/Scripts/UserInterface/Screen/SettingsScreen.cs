using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Screen.Components.Settings;
using Utils.Contract;
using Utils.Enum;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen
{
    [ExecuteAlways]
    public class SettingsScreen : ScreenBase, IUIStackable
    {
        public override NamedScreen Name => NamedScreen.Settings;
        public override UIPriority Priority => UIPriority.Medium;
        
        [Header("Prefabs")] 
        [SerializeField] private GameObject pageButtonPrefab;
        [SerializeField] private GameObject categoryHeaderPrefab;
        [SerializeField] private GameObject booleanSettingItemPrefab;
        [SerializeField] private GameObject floatSettingItemPrefab;
        [SerializeField] private GameObject enumSettingItemPrefab;
        [SerializeField] private GameObject inputKeySettingItemPrefab;
        [SerializeField] private List<GameObject> customSettingItemPrefabs;
        private Dictionary<NamedCustomSetting, GameObject> _customSettingsDict;
        
        [Header("Object References - Header")] 
        [SerializeField] private Transform headerPagesButtonsParent;
        [SerializeField] private Button headerLeftButton;
        [SerializeField] private Button headerRightButton;

        [Header("Object References - View")] 
        [SerializeField] private Transform viewParent;

        [Header("Settings Assets")] 
        public List<SettingsPageSO> pageAssets;

        [Header("Instantiated Objects")] 
        [SerializeField] private List<PageButton> pageButtons;
        [SerializeField] private int pageToInitializeIndex;
        private int _activePageIndex = -1;
        private Dictionary<string, ISettingItem> _settingItems;
        private string _selectedItem;
        

        private void Start()
        {
            _activePageIndex = -1;
            Initialize();
        }

        [ContextMenu("Initialize Screen")]
        private void InitializeFromEditor()
        {
            Initialize();
        }

        public override void Activate(bool instant, Action onActivate = null)
        {
            Initialize();
            base.Activate(instant, onActivate);
        }

        private void Initialize()
        {
            _customSettingsDict = new Dictionary<NamedCustomSetting, GameObject>();

            foreach (var prefab in customSettingItemPrefabs)
            {
                var comp = prefab.GetComponent<ICustomSettingItem>();
                _customSettingsDict.Add(comp.GetSettingType(), prefab);
            }

            InitializeScreen();
        }

        private void InitializeScreen()
        {
            for (var i = headerPagesButtonsParent.childCount - 1; i >= 0; i--)
            {
                var child = headerPagesButtonsParent.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child.gameObject);
                else
                    Destroy(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }

            pageButtons = new List<PageButton>();
            foreach (var page in pageAssets)
            {
                var go = Instantiate(pageButtonPrefab, headerPagesButtonsParent);
                var comp = go.GetComponent<PageButton>();
                comp.Initialize(InitializePage, page.index, page.pageName);
                pageButtons.Add(comp);
            }

            if (pageButtons.Count == 0)
            {
                return;
            }
            
            pageButtons[pageToInitializeIndex].ChangeState(UIComponentState.Selected);
            InitializePage(pageToInitializeIndex);
        }

        private void InitializePage(int index)
        {
            _selectedItem = null;
            _settingItems = new Dictionary<string, ISettingItem>();
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _activePageIndex = -1;
            }
#endif
            
            if (_activePageIndex != -1)
            {
                pageButtons[_activePageIndex].ChangeState(UIComponentState.Enabled);
            }

            _activePageIndex = index;
            
            for (var i = viewParent.childCount - 1; i >= 0; i--)
            {
                var child = viewParent.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child.gameObject);
                else
                    Destroy(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
            
            CreatePageAssets();
            ChangeSelected("");
        }

        private void CreatePageAssets()
        {
            foreach (var cat in pageAssets[_activePageIndex].categories)
            {
                CreateCategory(cat);
                foreach (var it in cat.items)
                {
                    CreateSetting(it, viewParent);
                }
            }
        }

        private void CreateCategory(SettingsPageCategorySO categoryAsset)
        {
            var go = Instantiate(categoryHeaderPrefab, viewParent);
            var comp = go.GetComponent<SettingsCategoryDisplay>();
            comp.Initialize(categoryAsset.categoryName);
        }

        private void CreateSetting(SettingsPageItemSO itemAsset, Transform parent)
        {
            itemAsset.ConvertFromString();
            GameObject prefab = null;
            switch (itemAsset.itemType)
            {
                case SettingsItemType.Float:
                    prefab = floatSettingItemPrefab;
                    break;
                case SettingsItemType.Enum:
                    prefab = enumSettingItemPrefab; 
                    break;
                case SettingsItemType.Boolean:
                    prefab = booleanSettingItemPrefab;
                    break;
                case SettingsItemType.InputKey:
                    prefab = inputKeySettingItemPrefab;
                    break;
                case SettingsItemType.Custom:
                    if (Enum.TryParse<NamedCustomSetting>(itemAsset.settingName, true, out var key))
                    {
                        prefab = _customSettingsDict[key];
                    }
                    else
                    {
                        Debug.LogError($"Invalid enum value: {itemAsset.settingName}");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var go = Instantiate(prefab, parent);
            var comp = go.GetComponent<ISettingItem>();
            comp.Initialize(itemAsset, ChangeSelected);
            _settingItems.Add(comp.GetId(), comp);

            if (itemAsset.itemType != SettingsItemType.Boolean)
            {
                return;
            }

            var booleanComp = (BooleanSetting)comp;
            booleanComp.SetRootLayout((RectTransform)viewParent);
            var condParent = booleanComp.GetConditionalParent();
            var prevState = condParent.gameObject.activeSelf;
            if (!prevState)
            {
                condParent.gameObject.SetActive(true);
            }
            
            foreach (var conditional in itemAsset.ConditionalItems)
            {
                CreateSetting(conditional, condParent);
            }
            
            condParent.gameObject.SetActive(prevState);
        }

        private void ChangeSelected(string id)
        {
            if (_selectedItem != null)
            {
                if (_settingItems.TryGetValue(_selectedItem, out var item))
                {
                    item.DeselectItem();
                }
            }
            _selectedItem = id;
        }

        #region UI STACK

        public void OnPush()
        {
            EnableInteractions();
        }

        public void OnPushOther()
        {
            DisableInteractions();
        }

        public void OnPop()
        {
            if (!IsClosing)
            {
                Deactivate(false);
                return;
            }
            Deactivate(true);
        }

        public void OnPopOther()
        {
            EnableInteractions();
        }

        #endregion
    }
}