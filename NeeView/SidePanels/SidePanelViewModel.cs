using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using NeeView.Windows;
using NeeView.Windows.Data;
using NeeView.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class SidePanelViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        /// <summary>
        /// Width property.
        /// </summary>
        public double Width
        {
            get { return Panel.Width; }
            set
            {
                if (Panel.Width != value)
                {
                    Panel.Width = Math.Min(value, MaxWidth);
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// MaxWidth property.
        /// </summary>
        private double _MaxWidth;
        public double MaxWidth
        {
            get { return _MaxWidth; }
            set { if (_MaxWidth != value) { _MaxWidth = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// IsDragged property.
        /// </summary>
        private bool _isDragged;
        public bool IsDragged
        {
            get { return _isDragged; }
            set { if (_isDragged != value) { _isDragged = value; RaisePropertyChanged(); UpdateVisibillity(); } }
        }

        /// <summary>
        /// IsNearCursor property.
        /// </summary>
        private bool _isNearCursor;
        public bool IsNearCursor
        {
            get { return _isNearCursor; }
            set { if (_isNearCursor != value) { _isNearCursor = value; RaisePropertyChanged(); UpdateVisibillity(); } }
        }

        /// <summary>
        /// IsAutoHide property.
        /// </summary>
        public bool IsAutoHide
        {
            get { return _isAutoHide; }
            set { if (_isAutoHide != value) { _isAutoHide = value; RaisePropertyChanged(); UpdateVisibillity(true); } }
        }

        //
        private bool _isAutoHide;


        /// <summary>
        /// IsVisibleLocked property.
        /// </summary>
        public bool IsVisibleLocked
        {
            get { return _isVisibleLocked; }
            set { if (_isVisibleLocked != value) { _isVisibleLocked = value; RaisePropertyChanged(); UpdateVisibillity(); } }
        }

        //
        private bool _isVisibleLocked;

        //
        public void UpdateVisibleLocked()
        {
            IsVisibleLocked = this.Panel.SelectedPanel != null && this.Panel.SelectedPanel.IsVisibleLock;
        }


        /// <summary>
        /// Visibility property.
        /// </summary>
        /// 
        public Visibility Visibility
        {
            get { return _visibility.Value; }
        }

        //
        private DelayValue<Visibility> _visibility;

        /// <summary>
        /// Visibility更新
        /// </summary>
        public void UpdateVisibillity(bool now = false)
        {
            if (_isVisibleLocked || _isDragged || (Panel.Panels.Any() ? _isAutoHide ? _isNearCursor : true : false))
            {
                _visibility.SetValue(Visibility.Visible, 0.0);
            }
            else
            {
                _visibility.SetValue(Visibility.Collapsed, now ? 0.0 : Preference.Current.panel_autohide_delaytime * 1000.0);
            }
        }


        /// <summary>
        /// PanelVisibility property.
        /// </summary>
        public Visibility PanelVisibility => Visibility == Visibility.Visible && this.Panel.SelectedPanel != null ? Visibility.Visible : Visibility.Collapsed;


        //
        public SidePanel Panel { get; private set; }

        //
        public SidePanelViewModel(SidePanel panel, ItemsControl itemsControl)
        {
            InitializeDropAccept(itemsControl);

            Panel = panel;
            Panel.PropertyChanged += Panel_PropertyChanged;

            Panel.SelectedPanelChanged += (s, e) =>
            {
                _visibility.SetValue(Visibility.Visible, 0.0);
                UpdateVisibillity();
            };


            _visibility = new DelayValue<Visibility>(Visibility.Collapsed);
            _visibility.ValueChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(Visibility));
                RaisePropertyChanged(nameof(PanelVisibility));
                Panel.IsVisible = Visibility == Visibility.Visible;
            };

            UpdateVisibillity();
        }

        private void Panel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Panel.SelectedPanel):
                    RaisePropertyChanged(nameof(PanelVisibility));
                    break;
            }
        }


        /// <summary>
        /// PanelIconClicked command.
        /// </summary>
        private RelayCommand<IPanel> _panelIconClicked;
        public RelayCommand<IPanel> PanelIconClicked
        {
            get { return _panelIconClicked = _panelIconClicked ?? new RelayCommand<IPanel>(PanelIconClicked_Executed); }
        }

        private void PanelIconClicked_Executed(IPanel content)
        {
            Panel.Toggle(content);
        }

        //
        protected const double _margin = 32;


        #region DropAccept

        private ItemsControl _itemsControl;

        /// <summary>
        /// カーソルからリストの挿入位置を求める
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private int GetItemInsertIndex(DragEventArgs args )
        {
            if (_itemsControl == null) return -1;

            var cursor = args.GetPosition(_itemsControl);
            //Debug.WriteLine($"cursor: {cursor}");

            var count = _itemsControl.Items.Count;
            for (int index = 0; index < count; ++index)
            {
                var item = _itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as ContentPresenter;
                var center = item.TranslatePoint(new Point(0, item.ActualHeight), _itemsControl);

                //Debug.WriteLine($"{i}: {pos}: {item.ActualWidth}x{item.ActualHeight}");
                if (cursor.Y < center.Y)
                {
                    return index;
                }
            }

            return Math.Max(count, 0);
        }

        public void InitializeDropAccept(ItemsControl itemsControl)
        {
            _itemsControl = itemsControl;

            this.Description = new DropAcceptDescription();
            this.Description.DragOver += Description_DragOver;
            this.Description.DragDrop += Description_DragDrop;
        }

        private DropAcceptDescription _description;
        public DropAcceptDescription Description
        {
            get { return this._description; }
            set
            {
                if (this._description == value)
                {
                    return;
                }
                this._description = value;
                this.RaisePropertyChanged();
            }
        }


        public EventHandler<PanelDropedEventArgs> PanelDroped;

        //
        private void Description_DragDrop(System.Windows.DragEventArgs args)
        {
            Debug.WriteLine($"Dropped");

            var panel = args.Data.GetData("PanelContent") as IPanel;
            if (panel == null) return;


            // ##
            var index = GetItemInsertIndex(args);
            Debug.WriteLine($"Insert: {index}");

            //
            PanelDroped?.Invoke(this, new PanelDropedEventArgs(panel, index));

            /*
            if (!args.Data.GetDataPresent(typeof(ViewModel2))) return;
            var data = args.Data.GetData(typeof(ViewModel2)) as ViewModel2;
            if (data == null) return;
            var fe = args.OriginalSource as FrameworkElement;
            if (fe == null) return;
            var target = fe.DataContext as ViewModel1;
            if (target == null) return;
            */
        }

        //
        private void Description_DragOver(System.Windows.DragEventArgs args)
        {
            if (args.AllowedEffects.HasFlag(DragDropEffects.Move))
            {
                if (args.Data.GetDataPresent("PanelContent"))
                {
                    args.Effects = DragDropEffects.Move;
                    return;
                }
            }
            args.Effects = DragDropEffects.None;

            //args.Effects = DragDropEffects.All;

            /*
            if (args.AllowedEffects.HasFlag(DragDropEffects.Copy))
            {
                if (args.Data.GetDataPresent(typeof(ViewModel2)))
                {
                    return;
                }
            }
            args.Effects = DragDropEffects.None;
            */
        }

        #endregion
    }


    //
    public class LeftPanelViewModel : SidePanelViewModel
    {
        public LeftPanelViewModel(SidePanel panel, ItemsControl itemsControl) : base(panel, itemsControl)
        {
        }

        //
        internal void UpdateVisibility(Point point, Point limit)
        {
            this.IsNearCursor = point.X < limit.X + _margin;
            /*Visibility != Visibility.Visible
                ? point.X < leftX + _margin
                : point.X < leftX + (Panel.SelectedPanel != null ? Width : 0.0) + _margin;
                */

            UpdateVisibleLocked();
        }
    }

    //
    public class RightPanelViewModel : SidePanelViewModel
    {
        public RightPanelViewModel(SidePanel panel, ItemsControl itemsControl) : base(panel, itemsControl)
        {
        }

        internal void UpdateVisibility(Point point, Point limit)
        {
            this.IsNearCursor = point.X > limit.X - _margin;
            /*Visibility != Visibility.Visible
                ? point.X > rightX - _margin
                : point.X > rightX - (Panel.SelectedPanel != null ? Width : 0.0) - _margin;
                */

            UpdateVisibleLocked();
        }
    }
}
