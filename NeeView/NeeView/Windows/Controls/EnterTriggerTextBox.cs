using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// Enter キーで UpdateSource を発行する TextBox
    /// </summary>
    public class EnterTriggerTextBox : TextBox
    {
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Enter)
            {
                var expression = GetBindingExpression(TextBox.TextProperty);
                expression?.UpdateSource();
            }
        }
    }
}
