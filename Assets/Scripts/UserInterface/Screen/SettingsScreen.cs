using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
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
        [Header("Screen Properties")]
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
        
        [Header("Runtime")]
        [SerializeField] private bool pagesInitialized;
        private Dictionary<string, ISettingItem> _settingItems;
        private List<(int startIndex, int endIndex)> _pageItemsIndexList = new();
        private int _activePageIndex;
        private string _selectedItem;
        
        [Header("Cached Changes")]
        private readonly Dictionary<string, int> cachedIntChanges = new();
        private readonly Dictionary<string, float> cachedFloatChanges = new();
        private readonly Dictionary<string, string> cachedStringChanges = new();

        public override void Activate(bool instant, Action onActivate = null)
        {
            _activePageIndex = -1;

            if (!pagesInitialized)
            {
                _customSettingsDict = new Dictionary<NamedCustomSetting, GameObject>();

                foreach (var prefab in customSettingItemPrefabs)
                {
                    var comp = prefab.GetComponent<ICustomSettingItem>();
                    _customSettingsDict.Add(comp.GetSettingType(), prefab);
                }
            }
            
            Initialize();
            base.Activate(instant, onActivate);
            SetActivePage(0);
        }

        [ContextMenu("Force Initialization")]
        public void ForceInitialize()
        {
            if (!pagesInitialized)
            {
                _customSettingsDict = new Dictionary<NamedCustomSetting, GameObject>();

                foreach (var prefab in customSettingItemPrefabs)
                {
                    var comp = prefab.GetComponent<ICustomSettingItem>();
                    _customSettingsDict.Add(comp.GetSettingType(), prefab);
                }
            }
            Initialize(true);
        }

        private void Initialize(bool force = false)
        {
            InitializePageButtons();
            InitializeScreenContent(force);
            InitializePageItemList();
            CleanCache();

            pagesInitialized = Application.isPlaying;
        }

        private void CleanCache()
        {
            cachedIntChanges.Clear();
            cachedFloatChanges.Clear();
            cachedStringChanges.Clear();
        }

        private void InitializePageItemList()
        {
            _pageItemsIndexList = new List<(int startIndex, int endIndex)>();
            var lastIndex = -1;
            foreach (var indexes in pageAssets.Select(page => (lastIndex + 1,
                         lastIndex + page.categories.Count + page.categories.Sum(category => category.items.Count))))
            {
                lastIndex = indexes.Item2;
                _pageItemsIndexList.Add(indexes);
            }
        }

        private void InitializePageButtons()
        {
            var pageCount = pageAssets.Count;
            var buttonCount = pageButtons.Count;

            if (pageCount < buttonCount)
            {
                for (var i = buttonCount - 1; i >= pageCount; i--)
                {
#if UNITY_EDITOR
                    DestroyImmediate(pageButtons[i].gameObject);
#else
                    Destroy(pageButtons[i].gameObject);
#endif
                    pageButtons.RemoveAt(i);
                }
            } 
            while (pageButtons.Count < pageAssets.Count)
            {
                var go = Instantiate(pageButtonPrefab, headerPagesButtonsParent);
                pageButtons.Add(go.GetComponent<PageButton>());
            }
    
            var index = 0;
            foreach (var pageButton in pageButtons)
            {
                pageButton.Initialize(SetActivePage, index, pageAssets[index].pageName);
                index++;
            }
        }

        private void InitializeScreenContent(bool force = false)
        {
            if (!force && (pagesInitialized
                           || viewParent.childCount == pageAssets.Sum(page =>
                               page.categories.Count + page.categories.Sum(category => category.items.Count)))) return;
            for (var i = viewParent.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(viewParent.GetChild(i).gameObject);
                else
                    Destroy(viewParent.GetChild(i).gameObject);
#else
                    Destroy(viewParent.GetChild(i).gameObject);
#endif
            }
                
            CreatePageAssets();
        }
        
        #region ASSET CREATION

        private void CreatePageAssets()
        {
            _settingItems ??= new Dictionary<string, ISettingItem>();

            foreach (var cat in pageAssets.SelectMany(page => page.categories))
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

            comp.Initialize(
                itemAsset,
                ChangeSelected,
                CacheChange,
                RemoveCachedChange);

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

        #endregion
        
        private void SetActivePage(int pageIndex)
        {
            if (_activePageIndex == -1)
            {
                for (var i = 0; i < viewParent.childCount; i++)
                {
                    viewParent.GetChild(i).gameObject.SetActive(false);
                }
            }
            else
            {
                pageButtons[_activePageIndex].ChangeState(UIComponentState.Enabled);
                for (var i = _pageItemsIndexList[_activePageIndex].startIndex;
                     i <= _pageItemsIndexList[_activePageIndex].endIndex;
                     i++)
                {
                    viewParent.GetChild(i).gameObject.SetActive(false);
                }
            }
            
            _activePageIndex = pageIndex;
            pageButtons[_activePageIndex].ChangeState(UIComponentState.Selected);
            for (var i = _pageItemsIndexList[_activePageIndex].startIndex;
                 i <= _pageItemsIndexList[_activePageIndex].endIndex;
                 i++)
            {
                viewParent.GetChild(i).gameObject.SetActive(true);
            }
            
            ChangeSelected("");
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

        private void CacheChange(string key, object value)
        {
            switch (value)
            {
                case int intValue:
                    cachedIntChanges[key] = intValue;
                    break;

                case float floatValue:
                    cachedFloatChanges[key] = floatValue;
                    break;

                case string stringValue:
                    cachedStringChanges[key] = stringValue;
                    break;

                default:
                    Debug.LogWarning($"Unsupported setting type: {value.GetType()}");
                    break;
            }
        }

        private void RemoveCachedChange(string key, object value)
        {
            switch (value)
            {
                case int:
                    cachedIntChanges.Remove(key);
                    break;

                case float:
                    cachedFloatChanges.Remove(key);
                    break;

                case string:
                    cachedStringChanges.Remove(key);
                    break;
            }
        }
        
        #region UI STACK

        public void OnPush(bool instant, Action onActivate = null)
        {
            if (!IsClosing)
            {
                Activate(instant, onActivate);
                return;
            }
            Activate(true, onActivate);
            EnableInteractions();
        }

        public void OnPushOther()
        {
            DisableInteractions();
        }

        public void OnPop(bool instant, Action onDeactivate = null)
        {
            if (!IsClosing)
            {
                Deactivate(instant, onDeactivate);
                return;
            }
            Deactivate(true, onDeactivate);
        }

        public void OnPopOther()
        {
            EnableInteractions();
        }

        #endregion
    }
}