//using NeeLaboratory.Windows.Input;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView.Configure
{
    /// <summary>
    /// 設定ウィンドウ項目基底
    /// </summary>
    public class SettingItem
    {
        public SettingItem()
        {
        }

        public SettingItem(string header)
        {
            this.Header = header;
        }

        public SettingItem(string header, string tips)
        {
            this.Header = header;
            this.Tips = tips;
        }


        public string Header { get; set; }
        public string Tips { get; set; }
        public IsEnabledPropertyValue IsEnabled { get; set; }
        public VisibilityPropertyValue Visibility { get; set; }

        public UIElement CreateContent()
        {
            var control = CreateContentInner();
            if (control == null)
            {
                return null;
            }

            this.IsEnabled?.SetBind(control);

            this.Visibility?.SetBind(control);

            return control;
        }

        protected virtual UIElement CreateContentInner()
        {
            return null;
        }
    }

    public class DataTriggerSource
    {
        public DataTriggerSource(Binding binding, object value, bool isTrue)
        {
            Binging = binding;
            Value = value;
            IsTrue = isTrue;
        }

        public DataTriggerSource(object source, string path, object value, bool isTrue)
        {
            Binging = new Binding(path) { Source = source };
            Value = value;
            IsTrue = isTrue;
        }

        public Binding Binging { get; private set; }
        public object Value { get; private set; }

        /// <summary>
        /// 条件が成立する時に肯定する結果にする
        /// </summary>
        public bool IsTrue { get; private set; }
    }

    /// <summary>
    /// SettingItem を複数まとめたもの
    /// </summary>
    public class SettingItemGroup : SettingItem
    {
        public SettingItemGroup() : base()
        {
        }

        public SettingItemGroup(string header) : base(header)
        {
        }

        public SettingItemGroup(string header, string tips) : base(header, tips)
        {
        }

        public SettingItemGroup(params SettingItem[] children) : base()
        {
            this.Children = children.Where(e => e != null).ToList();
        }

        public SettingItemGroup(string header, params SettingItem[] children) : base(header)
        {
            this.Children = children.Where(e => e != null).ToList();
        }

        public SettingItemGroup(string header, string tips, params SettingItem[] children) : base(header, tips)
        {
            this.Children = children.Where(e => e != null).ToList();
        }

        public List<SettingItem> Children { get; private set; }

        public DataTriggerSource IsEnabledTrigger { get; set; }
        public DataTriggerSource VisibleTrigger { get; set; }

        protected override UIElement CreateContentInner()
        {
            var dockPanel = new DockPanel();

            if (IsEnabledTrigger != null || VisibleTrigger != null)
            {
                var style = new Style(typeof(DockPanel));

                if (IsEnabledTrigger != null)
                {
                    style.Setters.Add(new Setter()
                    {
                        Property = UIElement.IsEnabledProperty,
                        Value = !IsEnabledTrigger.IsTrue,
                    });
                    var dataTrigger = new DataTrigger()
                    {
                        Binding = IsEnabledTrigger.Binging,
                        Value = IsEnabledTrigger.Value,
                    };
                    dataTrigger.Setters.Add(new Setter()
                    {
                        Property = UIElement.IsEnabledProperty,
                        Value = IsEnabledTrigger.IsTrue,
                    });
                    style.Triggers.Add(dataTrigger);
                }

                if (VisibleTrigger != null)
                {
                    style.Setters.Add(new Setter()
                    {
                        Property = UIElement.VisibilityProperty,
                        Value = VisibleTrigger.IsTrue ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible,
                    });
                    var dataTrigger = new DataTrigger()
                    {
                        Binding = VisibleTrigger.Binging,
                        Value = VisibleTrigger.Value,
                    };
                    dataTrigger.Setters.Add(new Setter()
                    {
                        Property = UIElement.VisibilityProperty,
                        Value = VisibleTrigger.IsTrue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
                    });
                    style.Triggers.Add(dataTrigger);
                }

                dockPanel.Style = style;
            }


            foreach (var content in CreateChildContenCollection())
            {
                DockPanel.SetDock(content, Dock.Top);
                dockPanel.Children.Add(content);
            }

            return dockPanel;
        }

        protected IEnumerable<UIElement> CreateChildContenCollection()
        {
            return this.Children != null
                ? this.Children.Where(e => e != null).Select(e => e.CreateContent())
                : Enumerable.Empty<UIElement>();
        }
    }

    /// <summary>
    /// SettingItem を複数まとめてタイトルをつけたもの
    /// </summary>
    public class SettingItemSection : SettingItemGroup
    {
        public SettingItemSection(string header)
            : base(header)
        {
        }

        public SettingItemSection(string header, string tips)
            : base(header, tips)
        {
        }

        public SettingItemSection(string header, params SettingItem[] children)
            : base(header, children)
        {
        }

        public SettingItemSection(string header, string tips, params SettingItem[] children)
            : base(header, tips, children)
        {
        }

        protected override UIElement CreateContentInner()
        {
            var dockPanel = new DockPanel()
            {
                Margin = new Thickness(0, 10, 0, 10),
                UseLayoutRounding = true,
            };

            var title = new StackPanel()
            {
                Margin = new Thickness(0, 5, 0, 10),
            };
            {
                var style = new Style(typeof(StackPanel));
                var trigger = new Trigger()
                {
                    Property = UIElement.IsEnabledProperty,
                    Value = false,
                };
                trigger.Setters.Add(new Setter()
                {
                    Property = UIElement.OpacityProperty,
                    Value = 0.5,
                });
                style.Triggers.Add(trigger);
                title.Style = style;
            }

            var titleTextBlock = new TextBlock()
            {
                Text = this.Header,
                FontSize = 24.0,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            title.Children.Add(titleTextBlock);

            if (!string.IsNullOrWhiteSpace(this.Tips))
            {
                var tips = new TextBlock()
                {
                    Text = this.Tips,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 0),
                };
                title.Children.Add(tips);
            }
            
            DockPanel.SetDock(title, Dock.Top);
            dockPanel.Children.Add(title);

            var subDockPanel = new DockPanel()
            {
                Margin = new Thickness(0, 5, 0, 5),
            };
            foreach (var content in CreateChildContenCollection())
            {
                DockPanel.SetDock(content, Dock.Top);
                subDockPanel.Children.Add(content);
            }
            dockPanel.Children.Add(subDockPanel);

            return dockPanel;
        }
    }

    /// <summary>
    /// PropertyMemberElement を設定項目としたもの
    /// </summary>
    public class SettingItemProperty : SettingItem
    {
        private PropertyMemberElement _element;
        private object _content;

        public SettingItemProperty(PropertyMemberElement element) : base(element?.ToString())
        {
            Debug.Assert(element != null);
            _element = element;
        }

        public SettingItemProperty(PropertyMemberElement element, object content) : this(element)
        {
            _content = content;
        }

        public bool IsStretch { get; set; }

        protected override UIElement CreateContentInner()
        {
            return new SettingItemControl(_element.Name, _element.Tips ?? this.Tips, _content ?? _element.TypeValue, this.IsStretch);
        }
    }


    /// <summary>
    /// IndexValueに対応したプロパティの設定項目
    /// </summary>
    public class SettingItemIndexValue<T> : SettingItem
    {
        private PropertyMemberElement _element;
        private IndexValue<T> _indexValue;
        private bool _isEditable;

        public SettingItemIndexValue(PropertyMemberElement element, IndexValue<T> indexValue, bool isEditable) : base(element?.ToString())
        {
            Debug.Assert(element != null);

            _element = element;
            _indexValue = indexValue;
            _isEditable = isEditable;

            _indexValue.Property = (PropertyValue<T, PropertyMemberElement>)_element.TypeValue;
        }

        protected override UIElement CreateContentInner()
        {
            var content = new SettingItemIndexValueControl()
            {
                IndexValue = _indexValue,
                IsEditable = _isEditable,
            };

            return new SettingItemControl(_element.Name, _element.Tips ?? this.Tips, content, false);
        }
    }

    /// <summary>
    /// ボタンの SettingItem
    /// </summary>
    public class SettingItemButton : SettingItem
    {
        private object _buttonContent;
        private ICommand _command;

        public SettingItemButton(string header, object buttonContent, ICommand command)
            : base(header)
        {
            _buttonContent = buttonContent;
            _command = command;
        }

        protected override UIElement CreateContentInner()
        {
            var button = new Button()
            {
                Content = _buttonContent,
                Command = _command,
                Padding = new Thickness(10, 5, 10, 5),
                MinWidth = 150,
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            // 自身をコマンドパラメータとする
            button.CommandParameter = button;

            return new SettingItemControl(this.Header, this.Tips, button, false);
        }
    }


    /// <summary>
    /// 説明項目
    /// </summary>
    public class SettingItemNote : SettingItem
    {
        private string _text;

        public SettingItemNote(string text) : base(null)
        {
            _text = text;
        }

        protected override UIElement CreateContentInner()
        {
            var textBlock = new TextBlock()
            {
                Text = _text,
                Background = Brushes.LightGray,
                Padding = new Thickness(20),
                Margin = new Thickness(0,20,0,20),
            };
            return textBlock;
        }
    }

    /// <summary>
    /// マウスドラッグのキー設定項目
    /// </summary>
    public class SettingItemMouseDrag : SettingItem
    {
        public SettingItemMouseDrag() : base(null)
        {
        }

        protected override UIElement CreateContentInner()
        {
            return new SettingMouseDragControl();
        }
    }

    /// <summary>
    /// Susieプラグイン設定項目
    /// </summary>
    public class SettingItemSusiePlugin : SettingItem
    {
        private Susie.SusiePluginType _pluginType;
        public SettingItemSusiePlugin(Susie.SusiePluginType pluginType) : base(null)
        {
            _pluginType = pluginType;
        }

        protected override UIElement CreateContentInner()
        {
            return new SettingItemSusiePluginControl(_pluginType);
        }
    }

    /// <summary>
    /// コマンド設定項目
    /// </summary>
    public class SettingItemCommand : SettingItem
    {
        public SettingItemCommand() : base(null)
        {
        }

        protected override UIElement CreateContentInner()
        {
            return new SettingItemCommandControl();
        }
    }

    /// <summary>
    /// メニュー設定項目
    /// </summary>
    public class SettingItemContextMenu : SettingItem
    {
        public SettingItemContextMenu() : base(null)
        {
        }

        protected override UIElement CreateContentInner()
        {
            var control = new ContextMenuSettingControl()
            {
                ContextMenuSetting = MainWindowModel.Current.ContextMenuSetting
            };

            return control;
        }
    }


    /// <summary>
    /// 詳細設定項目
    /// </summary>
    public class SettingItemPreference : SettingItem
    {
        public SettingItemPreference() : base(null)
        {
        }

        protected override UIElement CreateContentInner()
        {
            return new SettingItemPreferenceControl();
        }
    }
}
