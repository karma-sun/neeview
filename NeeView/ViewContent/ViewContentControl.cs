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

        public ViewContentControl(FrameworkElement content)
        {
            SetContent(content);
        }

        public void SetContent(FrameworkElement content)
        {
            Debug.Assert(content != null);

            if (_content != null)
            {
                this.Children.Remove(_content);
            }

            _content = content;
            this.Children.Insert(0, _content);
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
