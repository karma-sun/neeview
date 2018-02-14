using System.Windows;
using System.Windows.Controls;

namespace NeeView.Configure
{
    public class FormattedTextBlock : TextBlock
    {
        //
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register("Format", typeof(string), typeof(FormattedTextBlock), new PropertyMetadata("{0}"));

        //
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(FormattedTextBlock), new PropertyMetadata(null, Value_Changed));

        private static void Value_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FormattedTextBlock control)
            {
                control.Flush();
            }
        }

        //
        public void Flush()
        {
            base.Text = this.Format != null ? string.Format(this.Format, this.Value) : this.Value.ToString();
        }
    }
}
