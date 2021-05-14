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

                this.Children.Add(_messageTextBlock);
            }

            _messageTextBlock.Text = message;

            // 操作不能にする
            this.IsHitTestVisible = false;
        }
    }
}
