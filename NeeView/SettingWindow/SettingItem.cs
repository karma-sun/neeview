//using NeeLaboratory.Windows.Input;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
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

        public SettingItemGroup(string header, params SettingItem[] children) : base(header)
        {
            this.Children = children.Where(e => e != null).ToList();
        }

        public List<SettingItem> Children { get; private set; }

        protected override UIElement CreateContentInner()
        {
            var stackPanel = new StackPanel();

            foreach (var content in CreateChildContenCollection())
            {
                stackPanel.Children.Add(content);
            }

            return stackPanel;
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

        public SettingItemSection(string header, params SettingItem[] children)
            : base(header, children)
        {
        }

        protected override UIElement CreateContentInner()
        {
            var stackPanel = new StackPanel()
            {
                Margin = new Thickness(0, 10, 0, 10),
                UseLayoutRounding = true,
            };

            var title = new Grid();
            title.Children.Add(new TextBlock()
            {
                Text = this.Header,
                FontSize = 24.0,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
            });
            if (!string.IsNullOrWhiteSpace(this.Tips))
            {
                var popup = new HelpPopupControl()
                {
                    PopupContent = new TextBlock()
                    {
                        Text = this.Tips,
                        FontSize = 12,
                        Margin = new Thickness(10, 0, 0, 0)
                    },
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                };
                title.Children.Add(popup);
            }
            stackPanel.Children.Add(title);

            var subStackPanel = new StackPanel()
            {
                Margin = new Thickness(0, 5, 0, 5),
            };
            foreach (var content in CreateChildContenCollection())
            {
                subStackPanel.Children.Add(content);
            }
            stackPanel.Children.Add(subStackPanel);

            return stackPanel;
        }
    }

    /// <summary>
    /// PropertyMemberElement を設定項目としたもの
    /// </summary>
    public class SettingItemProperty : SettingItem
    {
        private PropertyMemberElement _element;

        public SettingItemProperty(PropertyMemberElement element) : base(element?.ToString())
        {
            Debug.Assert(element != null);
            _element = element;
        }

        public bool IsStretch { get; set; }

        protected override UIElement CreateContentInner()
        {
            return new SettingItemControl(_element.Name, _element.Tips ?? this.Tips, _element.TypeValue, this.IsStretch);
        }
    }

    /// <summary>
    /// サムネイルサイズに特化した設定項目
    /// TODO: RangeValueの一種。汎用化を。
    /// </summary>
    public class SettingItemThumbnailSize : SettingItem
    {
        private PropertyMemberElement _element;

        public SettingItemThumbnailSize(PropertyMemberElement element) : base(element?.ToString())
        {
            Debug.Assert(element != null);
            _element = element;
        }

        protected override UIElement CreateContentInner()
        {
            var content = new SettingItemThumbnailSizeControl()
            {
                PropertyValue = (PropertyValue_Double)_element.TypeValue,
            };
            return new SettingItemControl(_element.Name, _element.Tips ?? this.Tips, content, true);
        }
    }

    /// <summary>
    /// IndexValueに対応したプロパティの設定項目
    /// </summary>
    public class SettingItemIndexValue : SettingItem
    {
        private PropertyMemberElement _element;
        private IndexDoubleValue _indexValue;

        public SettingItemIndexValue(PropertyMemberElement element, IndexDoubleValue indexValue) : base(element?.ToString())
        {
            Debug.Assert(element != null);

            _element = element;
            _indexValue = indexValue;

            _indexValue.Property = (PropertyValue_Double)_element.TypeValue;
        }

        protected override UIElement CreateContentInner()
        {
            var content = new SettingItemIndexValueControl()
            {
                IndexValue = _indexValue
            };

            return new SettingItemControl(_element.Name, _element.Tips ?? this.Tips, content, true);
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
            };
            return new SettingItemControl(this.Header, this.Tips, button, false);
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
}
