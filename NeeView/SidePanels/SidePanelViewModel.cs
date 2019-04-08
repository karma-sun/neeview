using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using NeeView.Windows;
using NeeView.Windows.Data;
using NeeLaboratory.Windows.Input;
using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// SidePanel ViewModel.
    /// パネル単位のVM. SidePanelFrameViewModelの子
    /// </summary>
    public class SidePanelViewModel : BindableBase
    {
        public static string DragDropFormat = $"{Config.Current.ProcessId}.PanelContent";

        private bool _isForceVisibled;


        // Constructors

        public SidePanelViewModel(SidePanelGroup panel, ItemsControl itemsControl)
        {
            InitializeDropAccept(itemsControl);

            Panel = panel;
            Panel.PropertyChanged += Panel_PropertyChanged;

            Panel.SelectedPanelChanged += (s, e) =>
            {
                _visibility.SetValue(Visibility.Visible, 0.0);
                UpdateVisibility();

                SelectedPanelChanged?.Invoke(s, e);
            };

            _visibility = new DelayValue<Visibility>(Visibility.Collapsed);
            _visibility.ValueChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(Visibility));
                RaisePropertyChanged(nameof(PanelVisibility));
                Panel.IsVisible = Visibility == Visibility.Visible;
            };

            UpdateVisibility();
        }


        // Events

        public event EventHandler<SelectedPanelChangedEventArgs> SelectedPanelChanged;


        // Prototypes.

        public double Width
        {
            get { return Panel.Width; }
            set
            {
                if (Panel.Width != value)
                {
                    Panel.Width = Math.Min(value, MaxWidth);
                }
            }
        }

        private double _maxWidth;
        public double MaxWidth
        {
            get { return _maxWidth; }
            set { if (_maxWidth != value) { _maxWidth = value; RaisePropertyChanged(); } }
        }


        private bool _isDragged;
        public bool IsDragged
        {
            get { return _isDragged; }
            set { if (_isDragged != value) { _isDragged = value; RaisePropertyChanged(); UpdateVisibility(); } }
        }

        private bool _isNearCursor;
        public bool IsNearCursor
        {
            get { return _isNearCursor; }
            set { if (_isNearCursor != value) { _isNearCursor = value; RaisePropertyChanged(); UpdateVisibility(); } }
        }

        private bool _isAutoHide;
        public bool IsAutoHide
        {
            get { return _isAutoHide; }
            set { if (_isAutoHide != value) { _isAutoHide = value; RaisePropertyChanged(); UpdateVisibility(true); } }
        }


        private bool _isVisibleLocked;
        public bool IsVisibleLocked
        {
            get { return _isVisibleLocked; }
            set { if (_isVisibleLocked != value) { _isVisibleLocked = value; RaisePropertyChanged(); UpdateForceVisibled(); } }
        }


        private DelayValue<Visibility> _visibility;
        public Visibility Visibility
        {
            get { return _visibility.Value; }
        }


        public Visibility PanelVisibility => Visibility == Visibility.Visible && this.Panel.SelectedPanel != null ? Visibility.Visible : Visibility.Collapsed;

        // Model
        public SidePanelGroup Panel { get; private set; }



        // Methods

        public void UpdateForceVisibled()
        {
            _isForceVisibled = _isVisibleLocked || (this.Panel.SelectedPanel != null && this.Panel.SelectedPanel.IsVisibleLock);
            UpdateVisibility();
        }

        /// <summary>
        /// Visibility更新
        /// </summary>
        public void UpdateVisibility(bool now = false)
        {
            SetVisibility(CanVisible(), now, false);
        }

        //
        private bool CanVisible()
        {
            return _isForceVisibled || _isDragged || (Panel.Panels.Any() ? _isAutoHide ? _isNearCursor : true : false);
        }

        //
        private void SetVisibility(bool isVisible, bool now, bool isForce)
        {
            if (isVisible)
            {
                _visibility.SetValue(Visibility.Visible, 0.0, isForce);
            }
            else
            {
                _visibility.SetValue(Visibility.Collapsed, now ? 0.0 : App.Current.AutoHideDelayTime * 1000.0, isForce);
            }
        }

        /// <summary>
        /// 遅延非表示の場合に遅延時間をリセットする。
        /// キー入力等での表示更新遅延時間のリセットに使用
        /// </summary>
        public void ResetDelayHide()
        {
            if (!CanVisible()) SetVisibility(false, false, true);
        }

        /// <summary>
        /// モデルのプロパティ変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Panel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Panel.SelectedPanel):
                    RaisePropertyChanged(nameof(PanelVisibility));
                    break;
                case nameof(Panel.Width):
                    RaisePropertyChanged(nameof(Width));
                    break;
            }
        }


        #region Commands

        private RelayCommand<IPanel> _panelIconClicked;
        public RelayCommand<IPanel> PanelIconClicked
        {
            get { return _panelIconClicked = _panelIconClicked ?? new RelayCommand<IPanel>(PanelIconClicked_Executed); }
        }

        private void PanelIconClicked_Executed(IPanel content)
        {
            Panel.Toggle(content);
        }

        #endregion


        #region DropAccept

        /// <summary>
        /// ドロップイベント
        /// </summary>
        public EventHandler<PanelDropedEventArgs> PanelDroped;

        /// <summary>
        /// ドロップ受け入れ先コントロール.
        /// ドロップイベント受信コントロールとは異なるために用意した.
        /// </summary>
        private ItemsControl _itemsControl;

        /// <summary>
        /// ドロップ処理設定プロパティ
        /// </summary>
        public DropAcceptDescription Description
        {
            get { return _description; }
            set { if (_description != value) { _description = value; RaisePropertyChanged(); } }
        }

        private DropAcceptDescription _description;

        /// <summary>
        /// ドロップ処理初期化
        /// </summary>
        /// <param name="itemsControl">受け入れ先コントロール</param>
        public void InitializeDropAccept(ItemsControl itemsControl)
        {
            _itemsControl = itemsControl;

            this.Description = new DropAcceptDescription();
            this.Description.DragOver += Description_DragOver;
            this.Description.DragDrop += Description_DragDrop;
        }


        /// <summary>
        /// ドロップ処理
        /// </summary>
        /// <param name="e"></param>
        private void Description_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var panel = e.Data.GetData(DragDropFormat) as IPanel;
                if (panel == null) return;

                var index = GetItemInsertIndex(e);
                PanelDroped?.Invoke(this, new PanelDropedEventArgs(panel, index));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Drop failed: {ex.Message}");
            }
        }


        /// <summary>
        /// カーソルからリストの挿入位置を求める
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private int GetItemInsertIndex(DragEventArgs args)
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

        /// <summary>
        /// ドロップ受け入れ判定
        /// </summary>
        /// <param name="e"></param>
        private void Description_DragOver(object sender, DragEventArgs e)
        {
            if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
            {
                if (e.Data.GetDataPresent(DragDropFormat))
                {
                    e.Effects = DragDropEffects.Move;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        #endregion
    }


    /// <summary>
    /// パネルドロップイベント
    /// </summary>
    public class PanelDropedEventArgs : EventArgs
    {
        public PanelDropedEventArgs(IPanel panel, int index)
        {
            Panel = panel;
            Index = index;
        }

        /// <summary>
        /// ドロップされたパネル
        /// </summary>
        public IPanel Panel { get; set; }

        /// <summary>
        /// 挿入位置
        /// </summary>
        public int Index { get; set; }
    }


    /// <summary>
    /// 左パネル ViewModel
    /// </summary>
    public class LeftPanelViewModel : SidePanelViewModel
    {
        public LeftPanelViewModel(SidePanelGroup panel, ItemsControl itemsControl) : base(panel, itemsControl)
        {
        }

        /// <summary>
        /// 表示更新処理
        /// </summary>
        /// <param name="point"></param>
        /// <param name="limit"></param>
        internal void UpdateVisibility(Point point, Point limit, bool isMouseOverTarget)
        {
            this.IsNearCursor = point.X < limit.X + SidePanelProfile.Current.HitTestMargin && !MainWindowModel.Current.IsFontAreaMouseOver && isMouseOverTarget;
            UpdateForceVisibled();
        }
    }

    /// <summary>
    /// 右パネル ViewMdoel
    /// </summary>
    public class RightPanelViewModel : SidePanelViewModel
    {
        public RightPanelViewModel(SidePanelGroup panel, ItemsControl itemsControl) : base(panel, itemsControl)
        {
        }

        /// <summary>
        /// 表示更新処理
        /// </summary>
        /// <param name="point"></param>
        /// <param name="limit"></param>
        internal void UpdateVisibility(Point point, Point limit, bool isMouseOverTarget)
        {
            this.IsNearCursor = point.X > limit.X - SidePanelProfile.Current.HitTestMargin && !MainWindowModel.Current.IsFontAreaMouseOver && isMouseOverTarget;
            UpdateForceVisibled();
        }
    }
}
