using NeeView.Threading;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView
{
    public class ViewContentControl : Grid
    {
        private FrameworkElement _content;
        private TextBlock _messageTextBlock;
        private SimpleDelayAction _delayReconnect = new SimpleDelayAction();
        private object _lock = new object();

        public ViewContentControl(FrameworkElement content)
        {
            SetContent(content);
        }

        public ViewContentControl(FrameworkElement content, bool isAutoReconnect) : this(content)
        {
            IsAutoReconnectEnabled = isAutoReconnect;
        }


        public bool IsAutoReconnectEnabled { get; private set; }


        public void SetContent(FrameworkElement content)
        {
            Debug.Assert(content != null);

            lock (_lock)
            {
                if (_content != null)
                {
                    this.Children.Remove(_content);
                }

                _content = content;

                if (content != null)
                {
                    this.Children.Insert(0, _content);
                }
            }

            if (IsAutoReconnectEnabled)
            {
                _delayReconnect.Request(Reconnect, TimeSpan.FromMilliseconds(100));
            }
        }

        // 再接続。表示されないSVGを表示させる応急処置に使用する。
        public void Reconnect()
        {
            if (_content is null) return;

            lock (_lock)
            {
                this.Children.Remove(_content);
                this.Children.Insert(0, _content);
            }
        }

        public FrameworkElement Remove()
        {
            if (_content is null) return null;

            lock (_lock)
            {
                var content = _content;
                this.Children.Remove(_content);
                _content = null;
                return content;
            }
        }

        public void SetMessage(string message)
        {
            if (_messageTextBlock == null)
            {
                _messageTextBlock = new TextBlock();
                _messageTextBlock.Foreground = Brushes.White;
                _messageTextBlock.FontSize = 20;
                _messageTextBlock.Margin = new Thickness(10);
                _messageTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                _messageTextBlock.VerticalAlignment = VerticalAlignment.Center;

                _messageTextBlock.Effect = new DropShadowEffect()
                {
                    BlurRadius = 0,
                    Opacity = 0.5,
                    ShadowDepth = 2,
                };

                var stackPanel = new StackPanel();
                stackPanel.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                stackPanel.Children.Add(_messageTextBlock);
                stackPanel.Children.Add(new ProgressRing() { IsActive = true });

                var border = new Border();
                ////border.Background = new SolidColorBrush(Color.FromArgb(0xDD, 0xAA, 0xAA, 0xAA));
                border.Child = stackPanel;

                this.Children.Add(border);
            }

            _messageTextBlock.Text = message;

            // 操作不能にする
            this.IsHitTestVisible = false;
        }
    }
}
