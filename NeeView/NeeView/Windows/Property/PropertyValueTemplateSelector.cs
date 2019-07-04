using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows.Property
{
    class PropertyValueTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (element == null) return null;

            if (item is PropertyValue_Boolean booleanValue)
            {
                if (booleanValue.VisualType == PropertyVisualType.ToggleSwitch)
                {
                    return element.FindResource("PropertyValue_Boolean_ToggleSwitch") as DataTemplate;
                }
            }
            else if (item is PropertyValue_Color colorValue)
            {
                if (colorValue.VisualType == PropertyVisualType.ComboColorPicker)
                {
                    return element.FindResource("PropertyValue_Color_ComboColorPicker") as DataTemplate;
                }
            }

            return null;
        }
    }
}
