using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutDockPanel : Grid
    {
        private List<LayoutPanelContainer> _containers;


        public LayoutPanelManager Manager
        {
            get { return (LayoutPanelManager)GetValue(ManagerProperty); }
            set { SetValue(ManagerProperty, value); }
        }

        public static readonly DependencyProperty ManagerProperty =
            DependencyProperty.Register("Manager", typeof(LayoutPanelManager), typeof(LayoutDockPanel), new PropertyMetadata(null));


        public LayoutPanelCollection ItemsSource
        {
            get { return (LayoutPanelCollection)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(LayoutPanelCollection), typeof(LayoutDockPanel), new PropertyMetadata(null, ItemsSourceProperty_Changed));

        private static void ItemsSourceProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LayoutDockPanel control)
            {
                control.ItemsSourceChanged((LayoutPanelCollection)e.NewValue, (LayoutPanelCollection)e.OldValue);
            }
        }


        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }
        
        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(LayoutDockPanel), new PropertyMetadata(Brushes.Transparent));




        private void ItemsSourceChanged(LayoutPanelCollection newValue, LayoutPanelCollection oldValue)
        {
            Debug.Assert(newValue == this.ItemsSource);

            if (oldValue != null)
            {
                oldValue.CollectionChanged -= ItemsSource_CollectionChanged;
            }

            if (newValue != null)
            {
                newValue.CollectionChanged += ItemsSource_CollectionChanged;
            }

            Flush();
        }


        private void ItemsSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Flush();
        }


        private void Flush()
        {
            if (Manager is null) throw new InvalidOperationException("need Provider");

            const double minLength = 40.0;
            const double splitterHeight = 5.0;

            _containers = ItemsSource != null ? ItemsSource.Select(e => new LayoutPanelContainer(Manager, e)).ToList() : new List<LayoutPanelContainer>();

            this.Children.Clear();
            this.RowDefinitions.Clear();

            if (_containers.Count == 0)
            {
                return;
            }
            else
            {
                foreach (var container in _containers)
                {
                    {
                        var rowDefinigion = new RowDefinition();
                        rowDefinigion.MinHeight = minLength;
                        rowDefinigion.SetBinding(RowDefinition.HeightProperty, new Binding(nameof(LayoutPanel.GridLength)) { Source = container.LayoutPanel, Mode = BindingMode.TwoWay });
                        this.RowDefinitions.Add(rowDefinigion);
                        Grid.SetRow(container, this.RowDefinitions.Count - 1);
                        this.Children.Add(container);
                    }

                    // splitter
                    bool isLast = container == _containers.Last();
                    if (!isLast)
                    {
                        var rowDefinigion = new RowDefinition();
                        rowDefinigion.Height = new GridLength(splitterHeight);
                        this.RowDefinitions.Add(rowDefinigion);
                        var splitter = new GridSplitter()
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Height = splitterHeight,
                        };
                        splitter.SetBinding(GridSplitter.BackgroundProperty, new Binding(nameof(BorderBrush)) { Source = this });
                        Grid.SetRow(splitter, this.RowDefinitions.Count - 1);
                        this.Children.Add(splitter);
                    }

                    container.Snap();
                }
            }
        }
    }
}
