using System;
using System.Collections.Generic;
using System.Linq;
using GameInput;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.SO.Settings.Screen;

namespace Editor.CommandCenter.Screens.Modules.Settings
{
    public class SettingsPageItemModule : ISettingsPageModule
    {
        private VisualElement Element { get; set; }
        private SettingsPageItemSO _item;
        private readonly Action<SettingsPageItemSO> _onRemove;
        private static List<Type> _cachedEnums;
        private VisualElement _typeContainer;

        public SettingsPageItemModule(SettingsPageItemSO item, Action<SettingsPageItemSO> onRemove)
        {
            _item = item;
            _onRemove = onRemove;
        }

        public VisualElement CreateUI()
        {
            Element = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginTop = 6,
                    marginBottom = 6,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 6,
                    paddingBottom = 6,
                    backgroundColor = Color.gray3,
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftColor = new Color(0.65f, 0.65f, 0.65f),
                    borderRightColor = new Color(0.65f, 0.65f, 0.65f),
                    borderTopColor = new Color(0.65f, 0.65f, 0.65f),
                    borderBottomColor = new Color(0.65f, 0.65f, 0.65f)
                }
            };

            DrawHeader(Element);
            DrawTypeSpecific(Element);
            
            return Element;
        }

        
        private void DrawTypeSpecific(VisualElement root)
        {
            _typeContainer?.RemoveFromHierarchy();
            _typeContainer = new VisualElement { style = { flexDirection = FlexDirection.Column, marginTop = 4 } };
            root.Add(_typeContainer);

            switch (_item.itemType)
            {
                case SettingsItemType.Float: DrawFloat(_typeContainer, _item); break;
                case SettingsItemType.Boolean: DrawBool(_typeContainer, _item); break;
                case SettingsItemType.InputKey: DrawInputKey(_typeContainer, _item); break;
                case SettingsItemType.Enum: DrawEnum(_typeContainer, _item); break;
                case SettingsItemType.Custom: DrawCustom(_typeContainer, _item); break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawHeader(VisualElement root)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            var nameLabel = new Label("Setting Name:")
            {
                style = { unityTextAlign = TextAnchor.MiddleLeft, width = 90 }
            };

            var nameField = new TextField { value = _item.settingName, style = { flexGrow = 1 } };
            nameField.RegisterValueChangedCallback(e =>
            {
                _item.settingName = e.newValue;
                _item.ConvertToString();
            });

            var typeDropdown = new EnumField(_item.itemType);
            typeDropdown.RegisterValueChangedCallback(e =>
            {
                _item.itemType = (SettingsItemType)e.newValue;
                DrawTypeSpecific(Element);
            });

            var removeBtn = new Button(() =>
            {
                _onRemove?.Invoke(_item);
                Element?.RemoveFromHierarchy();
            })
            {
                text = "X",
                style =
                {
                    width = 24,
                    height = 22,
                    backgroundColor = new Color(0.5f, 0.2f, 0.2f),
                    color = Color.white,
                    marginLeft = 6
                }
            };

            row.Add(nameLabel);
            row.Add(nameField);
            row.Add(typeDropdown);
            row.Add(removeBtn);
            root.Add(row);

            var separator = new VisualElement { style = { height = 1, backgroundColor = new Color(0.5f, 0.5f, 0.5f), marginBottom = 4 } };
            root.Add(separator);
        }
        
        private void DrawCustom(VisualElement root, SettingsPageItemSO item)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            var label = new Label("Custom setting (no parameters)")
            {
                style = { unityTextAlign = TextAnchor.MiddleLeft }
            };

            row.Add(label);
            root.Add(row);
        }

        private void DrawFloat(VisualElement root, SettingsPageItemSO item)
        {
            var container = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 } };

            var min = item.MinValue != 0 ? item.MinValue : 0f;
            var max = item.MaxValue != 0 ? item.MaxValue : 100f;
            var step = item.MinIncrement != 0 ? item.MinIncrement : 1f;
            var defaultValue = Mathf.Clamp(item.FloatDefaultValue, min, max);

            item.MinValue = min;
            item.MaxValue = max;
            item.FloatDefaultValue = defaultValue;
            item.MinIncrement = step;

            var minField = new FloatField { value = min, style = { width = 60 } };
            var maxField = new FloatField { value = max, style = { width = 60 } };
            var stepField = new FloatField { value = step, style = { width = 60 } };
            var defaultValueField = new FloatField { value = defaultValue, style = { width = 60 } };

            AddFieldWithLabel(container, "Min Value:", minField);
            AddFieldWithLabel(container, "Max Value:", maxField);
            AddFieldWithLabel(container, "Min Step:", stepField);
            AddFieldWithLabel(container, "Default Value:", defaultValueField);
            root.Add(container);

            var sliderRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 } };
            var slider = new Slider(min, max)
            {
                value = defaultValue,
                lowValue = min,
                highValue = max,
                style = { flexGrow = 1 }
            };

            var valueField = new FloatField { value = defaultValue, isReadOnly = true, style = { width = 60 } };
            
            slider.RegisterValueChangedCallback(e =>
            {
                var singleStep = item.MinIncrement <= 0 ? 1f : item.MinIncrement;
                var snappedValue = Mathf.Round(e.newValue / singleStep) * singleStep;

                snappedValue = Mathf.Clamp(snappedValue, slider.lowValue, slider.highValue);

                item.FloatDefaultValue = snappedValue;

                slider.SetValueWithoutNotify(snappedValue);
                valueField.SetValueWithoutNotify(snappedValue);
                defaultValueField.SetValueWithoutNotify(snappedValue);

                _item.ConvertToString();
            });
            
            minField.RegisterValueChangedCallback(e =>
            {
                item.MinValue = e.newValue;
                slider.lowValue = Mathf.Min(e.newValue, slider.highValue - 0.01f);
                slider.value = Mathf.Clamp(slider.value, slider.lowValue, slider.highValue);
                defaultValueField.SetValueWithoutNotify(slider.value);
            });

            maxField.RegisterValueChangedCallback(e =>
            {
                item.MaxValue = e.newValue;
                slider.highValue = Mathf.Max(e.newValue, slider.lowValue + 0.01f);
                slider.value = Mathf.Clamp(slider.value, slider.lowValue, slider.highValue);
                defaultValueField.SetValueWithoutNotify(slider.value);
            });

            stepField.RegisterValueChangedCallback(e => item.MinIncrement = e.newValue);

            defaultValueField.RegisterValueChangedCallback(e =>
            {
                item.FloatDefaultValue = Mathf.Clamp(e.newValue, slider.lowValue, slider.highValue);
                slider.SetValueWithoutNotify(item.FloatDefaultValue);
            });

            sliderRow.Add(slider);
            sliderRow.Add(valueField);
            root.Add(sliderRow);
            return;

            void AddFieldWithLabel(VisualElement fieldContainer, string labelText, FloatField field)
            {
                var fieldRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginRight = 4 } };
                var label = new Label(labelText) { style = { width = 80 } };
                fieldRow.Add(label);
                fieldRow.Add(field);
                fieldContainer.Add(fieldRow);
            }
        }

        private void DrawBool(VisualElement root, SettingsPageItemSO item)
        {
            var container = new VisualElement
            {
                style = { flexDirection = FlexDirection.Column, marginBottom = 4 }
            };

            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };

            var toggleLabel = new Label("Default State:") { style = { width = 110 } };
            var toggle = new Toggle { value = item.BoolDefaultValue };

            toggle.RegisterValueChangedCallback(e =>
            {
                item.BoolDefaultValue = e.newValue;
                item.ConvertToString();
            });

            row.Add(toggleLabel);
            row.Add(toggle);
            container.Add(row);

            var conditionalRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };

            var conditionalLabel = new Label("Is Conditional:") { style = { width = 110 } };
            var conditionalToggle = new Toggle { value = item.BooleanIsConditional };

            conditionalRow.Add(conditionalLabel);
            conditionalRow.Add(conditionalToggle);
            container.Add(conditionalRow);

            var nestedContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginTop = 6,
                    marginLeft = 10,
                    borderLeftWidth = 2,
                    borderLeftColor = new Color(0.4f, 0.4f, 0.4f),
                    paddingLeft = 6
                }
            };

            container.Add(nestedContainer);

            conditionalToggle.RegisterValueChangedCallback(e =>
            {
                item.BooleanIsConditional = e.newValue;

                if (!e.newValue)
                    item.ConditionalItems?.Clear();

                RefreshNested();
                item.ConvertToString();
            });

            RefreshNested();

            root.Add(container);
            return;

            void RefreshNested()
            {
                nestedContainer.Clear();

                if (!item.BooleanIsConditional)
                    return;

                item.ConditionalItems ??= new List<SettingsPageItemSO>();

                foreach (var module in item.ConditionalItems.ToList().Select(child => new SettingsPageItemModule(child, removed =>
                         {
                             item.ConditionalItems.Remove(removed);
                             RefreshNested();
                             item.ConvertToString();
                         })))
                {
                    nestedContainer.Add(module.CreateUI());
                }

                var addBtn = new Button(() =>
                {
                    var newItem = ScriptableObject.CreateInstance<SettingsPageItemSO>();
                    item.ConditionalItems.Add(newItem);

                    RefreshNested();
                    item.ConvertToString();
                })
                {
                    text = "+ Add Conditional Item"
                };

                nestedContainer.Add(addBtn);
            }
        }

        private void DrawInputKey(VisualElement root, SettingsPageItemSO item)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 } };
            var label = new Label("Action:") { style = { width = 60 } };
            var dropdown = new EnumField(item.ActionName) { style = { width = 180 } };

            dropdown.RegisterValueChangedCallback(e =>
            {
                item.ActionName = (PlayerAction)e.newValue;
                _item.ConvertToString();
            });

            row.Add(label);
            row.Add(dropdown);
            root.Add(row);
        }

        private void DrawEnum(VisualElement root, SettingsPageItemSO item)
        {
            CacheEnums();

            var container = new VisualElement { style = { flexDirection = FlexDirection.Column, marginBottom = 4 } };

            var typeRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 2 } };
            var typeLabel = new Label("Enum Type:") { style = { width = 80 } };

            var defaultType = _cachedEnums.FirstOrDefault(t => t.FullName == item.EnumTypeName)
                              ?? _cachedEnums.FirstOrDefault();

            var typeDropdown = new PopupField<Type>(_cachedEnums, defaultType, t => t.Name)
            {
                style = { width = 180 }
            };

            typeRow.Add(typeLabel);
            typeRow.Add(typeDropdown);

            var valueRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 } };
            var valueLabel = new Label("Default Value:") { style = { width = 80 } };
            var values = defaultType != null 
                ? Enum.GetNames(defaultType).ToList() 
                : new List<string>();

            var safeValue = values.Contains(item.EnumDefaultValue)
                ? item.EnumDefaultValue
                : values.FirstOrDefault();

            var valueDropdown = new PopupField<string>(values, safeValue);

            valueRow.Add(valueLabel);
            valueRow.Add(valueDropdown);

            typeDropdown.RegisterValueChangedCallback(evt =>
            {
                item.EnumTypeName = evt.newValue.FullName;
                var newValues = Enum.GetNames(evt.newValue).ToList();
                valueDropdown.choices = newValues;
                valueDropdown.value = newValues.FirstOrDefault();
                item.EnumDefaultValue = valueDropdown.value;
                _item.ConvertToString();
            });

            valueDropdown.RegisterValueChangedCallback(ev =>
            {
                item.EnumDefaultValue = ev.newValue;
                _item.ConvertToString();
            });

            container.Add(typeRow);
            container.Add(valueRow);
            root.Add(container);
        }
        
        private void CacheEnums()
        {
            _cachedEnums = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsEnum && t.Namespace == "Utils.Enum.Settings")
                .ToList();
        }
    }
}