using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                ////_messageTextBlock.Text = LoosePath.GetFileName(source.Page.EntryFullName);
                _messageTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
                _messageTextBlock.FontSize = 20;
                _messageTextBlock.Margin = new Thickness(10);
                _messageTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                _messageTextBlock.VerticalAlignment = VerticalAlignment.Center;

                this.Children.Add(_messageTextBlock);
            }

            _messageTextBlock.Text = message;

            // 操作不能にする
            this.IsHitTestVisible = false;
        }
    }
}
