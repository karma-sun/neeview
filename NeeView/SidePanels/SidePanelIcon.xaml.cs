using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// SidePanelIcon.xaml の相互作用ロジック<br/>
    /// DataContextに <see cref="LayoutPanel"/> を要求する
    /// </summary>
    public partial class SidePanelIcon : UserControl
    {
        public SidePanelIcon()
        {
            InitializeComponent();
        }


        public ISidePanelIconDescriptor Descriptor
        {
            get { return (ISidePanelIconDescriptor)GetValue(DescriptorProperty); }
            set { SetValue(DescriptorProperty, value); }
        }

        public static readonly DependencyProperty DescriptorProperty =
            DependencyProperty.Register("Descriptor", typeof(ISidePanelIconDescriptor), typeof(SidePanelIcon), new PropertyMetadata(null, DescriptorPropertyChanged));

        private static void DescriptorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SidePanelIcon control)
            {
                control.Update();
            }
        }


        private void Update()
        {
            if (this.DataContext is LayoutPanel layoutPanel)
            {
                this.ButtonContent.Content = Descriptor?.CreateButtonContent(layoutPanel);
            }
            else
            {
                this.ButtonContent.Content = null;
            }
        }


        private void TogglePanelCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is LayoutPanel layoutPanel)
            {
                Descriptor?.ToggleLayoutPanel(layoutPanel);
            }
        }

        private void OpenDockCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is LayoutPanel layoutPanel)
            {
                CustomLayoutPanelManager.Current.OpenDock(layoutPanel);
            }
        }

        private void OpenWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is LayoutPanel layoutPanel)
            {
                CustomLayoutPanelManager.Current.OpenWindow(layoutPanel, WindowPlacement.None);
            }
        }

        private void DragStartBehavior_DragBegin(object sender, DragStartEventArgs e)
        {
            Descriptor?.DragBegin();
        }

        private void DragStartBehavior_DragEnd(object sender, EventArgs e)
        {
            Descriptor?.DragEnd();
        }
    }

    public interface ISidePanelIconDescriptor
    {
        FrameworkElement CreateButtonContent(LayoutPanel panel);
        void ToggleLayoutPanel(LayoutPanel panel);
        void DragBegin();
        void DragEnd();
    }
}
