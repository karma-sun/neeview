using NeeView.Data;
using NeeView.Susie;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView.Setting
{
    /// <summary>
    /// 設定ウィンドウ項目基底
    /// </summary>
    public class SettingItem 
    {
        private SettingItem _searchResultItem;


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

        /// <summary>
        /// 検索結果の項目表示用
        /// </summary>
        public SettingItem SearchResultItem
        {
            get => _searchResultItem ?? this;
            set => _searchResultItem = value;
        }


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

        public virtual string GetSearchText()
        {
            return Header + " " + Tips;
        }

        public virtual IEnumerable<SettingItem> GetItemCollection()
        {
            yield return this;
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


        public List<SettingItem> Children { get; private set; } = new List<SettingItem>();

        public DataTriggerSource IsEnabledTrigger { get; set; }

        protected override UIElement CreateContentInner()
        {
            var dockPanel = new DockPanel();

            if (IsEnabledTrigger != null)
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

        public override  IEnumerable<SettingItem> GetItemCollection()
        {
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach(var item in child.GetItemCollection())
                    {
                        yield return item;
                    }
                }
            }
            else
            {
                yield return this;
            }
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

        protected override UIElement CreateContentInner()
        {
            var dockPanel = new DockPanel()
            {
                Margin = new Thickness(0, 10, 0, 10),
                UseLayoutRounding = true,
            };

            var title = new StackPanel()
            {
                Margin = new Thickness(0, 15, 0, 0),
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

        public SettingItemProperty(PropertyMemberElement element) : base(element.Name)
        {
            Debug.Assert(element != null);
            _element = element;
            this.Tips = _element.Tips;
        }

        public SettingItemProperty(PropertyMemberElement element, object content) : this(element)
        {
            _content = content;
        }

        public bool IsStretch { get; set; }

        protected override UIElement CreateContentInner()
        {
            return new SettingItemControl(this.Header, this.Tips, _content ?? _element.TypeValue, this.IsStretch);
        }

        public override string GetSearchText()
        {
            return this.Header + " " + this.Tips;
        }
    }


    public class SettingItemHeader : SettingItem
    {
        public SettingItemHeader(string header) : base(header)
        {
        }

        protected override UIElement CreateContentInner()
        {
            return new SettingItemControl(this.Header, this.Tips, null, false);
        }

        public override string GetSearchText()
        {
            return this.Header + " " + this.Tips;
        }
    }

    /// <summary>
    /// 複数の PropertyMemberElement を設定項目としたもの
    /// </summary>
    public class SettingItemMultiProperty : SettingItem
    {
        private PropertyMemberElement _element1;
        private PropertyMemberElement _element2;

        public SettingItemMultiProperty(PropertyMemberElement element1, PropertyMemberElement element2) : base(element1?.ToString())
        {
            Debug.Assert(element1 != null);
            Debug.Assert(element2 != null);
            _element1 = element1;
            _element2 = element2;
        }

        public object Content1 { get; set; }
        public object Content2 { get; set; }

        protected override UIElement CreateContentInner()
        {
            var content1 = Content1 ?? _element1.TypeValue;
            var content2 = Content2 ?? _element2.TypeValue;
            return new SettingItemMultiControl(_element1.Name, _element1.Tips ?? this.Tips, content1, content2);
        }

        public override string GetSearchText()
        {
            return _element1.Name + " " + (_element1.Tips ?? this.Tips);
        }
    }

    /// <summary>
    /// PropertyMemberElement を補足設定項目としたもの
    /// </summary>
    public class SettingItemSubProperty : SettingItem
    {
        private PropertyMemberElement _element;
        private object _content;

        public SettingItemSubProperty(PropertyMemberElement element) : base(element.Name)
        {
            Debug.Assert(element != null);
            _element = element;
            this.Tips = element.Tips;
        }

        public SettingItemSubProperty(PropertyMemberElement element, object content) : this(element)
        {
            _content = content;
        }

        public bool IsStretch { get; set; } = true;

        protected override UIElement CreateContentInner()
        {
            return new SettingItemSubControl(this.Header, this.Tips, _content ?? _element.TypeValue, this.IsStretch);
        }

        public override string GetSearchText()
        {
            return this.Header + " " + this.Tips;
        }
    }


    /// <summary>
    /// フォント選択
    /// </summary>
    public class SettingItemPropertyFont : SettingItem
    {
        private PropertyMemberElement _element;

        public SettingItemPropertyFont(PropertyMemberElement element) : base(element?.ToString())
        {
            Debug.Assert(element != null);
            _element = element;
        }

        protected override UIElement CreateContentInner()
        {
            var comboBox = new ComboBox();
            var currentLang = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
            var fonts = Fonts.SystemFontFamilies.Select(v => v.FamilyNames.FirstOrDefault(o => o.Key == currentLang).Value ?? v.Source);
            comboBox.ItemsSource = fonts;
            comboBox.IsEditable = false;

            var binding = new Binding(nameof(PropertyValue_String.Value)) { Source = _element.TypeValue as PropertyValue_String };
            BindingOperations.SetBinding(comboBox, ComboBox.SelectedItemProperty, binding);

            return new SettingItemControl(_element.Name, _element.Tips ?? this.Tips, comboBox, false);
        }

        public override string GetSearchText()
        {
            return _element.Name + " " + (_element.Tips ?? this.Tips);
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

            _indexValue.Property = (PropertyValue<T>)_element.TypeValue;
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

        public override string GetSearchText()
        {
            return _element.Name + " " + (_element.Tips ?? this.Tips);
        }
    }


    /// <summary>
    /// ボタンの SettingItem
    /// </summary>
    public class SettingItemButton : SettingItem
    {
        private object _buttonContent;
        private ICommand _command;

        public bool IsContentOnly { get; set; }

        public SettingItemButton(string header, ICommand command)
            : base(header)
        {
            _command = command;
        }

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
                Content = _buttonContent ?? this.Header,
                Command = _command,
                Padding = new Thickness(20, 10, 20, 10),
                MinWidth = 150,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            // 自身をコマンドパラメータとする
            button.CommandParameter = button;

            if (this.IsContentOnly)
            {
                button.Margin = new Thickness(0, 5, 0, 5);
                return button;
            }
            else
            {
                return new SettingItemControl(this.Header, this.Tips, button, true);
            }
        }
    }


    /// <summary>
    /// リンクの SettingItem
    /// </summary>
    public class SettingItemLink : SettingItem
    {
        private ICommand _command;

        public SettingItemLink(string header, ICommand command)
            : base(header)
        {
            _command = command;
        }

        public bool IsContentOnly { get; set; }

        protected override UIElement CreateContentInner()
        {
            var textBlock = new TextBlock();
            var link = new Hyperlink();
            link.Inlines.Add(this.Header);
            link.Command = _command;
            textBlock.Inlines.Add(link);

            if (this.IsContentOnly)
            {
                textBlock.Margin = new Thickness(0, 5, 0, 5);
                return textBlock;
            }
            else
            {
                return new SettingItemControl(this.Header, this.Tips, textBlock, true);
            }
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
                Margin = new Thickness(0, 20, 0, 20),
            };
            return textBlock;
        }

        public override string GetSearchText()
        {
            return "";
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
        private SusiePluginType _pluginType;
        public SettingItemSusiePlugin(SusiePluginType pluginType) : base(null)
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

        public override string GetSearchText()
        {
            return string.Join(" ",
                Properties.Resources.WordCommand,
                Properties.Resources.ControlEditCommandTabShortcut,
                Properties.Resources.ControlEditCommandTabGesture,
                Properties.Resources.ControlEditCommandTabTouch,
                Properties.Resources.ControlEditCommandTabParameter);
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
                ContextMenuSetting = ContextMenuManager.Current
            };

            return control;
        }
    }
}
