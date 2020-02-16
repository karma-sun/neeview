using NeeView.Windows.Data;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// コンテキストメニューが開くタイミングを監視する
    /// </summary>
    /// <remarks>
    /// NOTE: ContextMenuOpeningでキャンセルされたときの動作に非対応
    /// </remarks>
    public static class ContextMenuWatcher
    {
        private static bool _isInitialized;
        private static DelayValue<UIElement> _targetElement = new DelayValue<UIElement>();
        private static UIElement _openingEventElement;

        public static event EventHandler<TargetElementChangedEventArgs> TargetElementChanged;

        /// <summary>
        /// 最後に開いたコンテキストメニューターゲット。500msでクリアされる
        /// </summary>
        public static UIElement TargetElement => _targetElement?.Value;

        public static void Initialize()
        {
            if (_isInitialized) return;

            _isInitialized = true;

            _targetElement = new DelayValue<UIElement>();
            _targetElement.ValueChanged += (s, e) => TargetElementChanged?.Invoke(s, new TargetElementChangedEventArgs(_targetElement.Value));

            CompositionTarget.Rendering += CompositionTarget_Rendering;
            EventManager.RegisterClassHandler(typeof(UIElement), ContextMenuService.ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpening));
        }

        private static void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            _openingEventElement = null;
        }

        private static void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = ContextMenuService.GetContextMenu((DependencyObject)sender);
            if (menu != null)
            {
                if (_openingEventElement == null && sender is UIElement element)
                {
                    _openingEventElement = element;
                    SetTargetElement(element);
                }
            }
        }

        /// <summary>
        /// 直接コンテキストメニューターゲットを指定する。
        /// 独自操作でコンテキストメニューを開く場合等に使用する
        /// </summary>
        public static void SetTargetElement(UIElement element)
        {
            if (!_isInitialized) throw new InvalidOperationException();

            _targetElement.SetValue(element, 0.0);
            _targetElement.SetValue(null, 500.0); // keep 500ms.
        }

    }


    public class TargetElementChangedEventArgs : EventArgs
    {
        public TargetElementChangedEventArgs(UIElement targetElement)
        {
            TargetElement = targetElement;
        }

        public UIElement TargetElement { get; set; }
    }
}
