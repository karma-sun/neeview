// from http://tnakamura.hatenablog.com/entry/20101001/dropdownmenu_attached_behavior

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace NeeView.Windows.Controls
{
    public static class DropDownMenuBehavior
    {
        public static readonly DependencyProperty DropDownMenuProperty = DependencyProperty.RegisterAttached(
            "DropDownMenu",
            typeof(ContextMenu),
            typeof(DropDownMenuBehavior),
            new PropertyMetadata(null, OnDropDownMenuChanged));

        private static void OnDropDownMenuChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button == null)
            {
                return;
            }

            var dropDownMenu = e.NewValue as ContextMenu;
            if (dropDownMenu == null)
            {
                return;
            }

            dropDownMenu.Placement = PlacementMode.Bottom;
            dropDownMenu.PlacementTarget = button;
            dropDownMenu.VerticalOffset = -5;
            dropDownMenu.SetBinding(ContextMenu.IsOpenProperty, new Binding(nameof(button.IsChecked))
            {
                Source = button,
                Delay = 200, // ボタンでも閉じれるようにコンテキストメニュからの更新を遅延
            });
        }

        public static void SetDropDownMenu(ToggleButton button, ContextMenu dropDownMenu)
        {
            button.SetValue(DropDownMenuProperty, dropDownMenu);
        }

        public static ContextMenu GetDropDownMenu(ToggleButton button)
        {
            return button.GetValue(DropDownMenuProperty) as ContextMenu;
        }
    }
}
