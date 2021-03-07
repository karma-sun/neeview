using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public abstract class ItemsListMoreMenuDescription : MoreMenuDescription
    {
        private static PanelListItemStyleToBooleanConverter _converter = new PanelListItemStyleToBooleanConverter();

        protected MenuItem CreateListItemStyleMenuItem(string header, ICommand command, PanelListItemStyle style, IHasPanelListItemStyle source)
        {
            var item = CreateCommandMenuItem(header, command);
            item.CommandParameter = style;
            item.SetBinding(MenuItem.IsCheckedProperty, CreateListItemStyleBinding(style, source));
            return item;
        }

        protected Binding CreateListItemStyleBinding(PanelListItemStyle style, IHasPanelListItemStyle source)
        {
            var binding = new Binding(nameof(IHasPanelListItemStyle.PanelListItemStyle))
            {
                Converter = _converter,
                ConverterParameter = style,
                Source = source,
            };

            return binding;
        }
    }
}
