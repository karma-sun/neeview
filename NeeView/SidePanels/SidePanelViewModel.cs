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
using NeeLaboratory.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// SidePanel ViewModel.
    /// パネル単位のVM. SidePanelFrameViewModelの子
    /// </summary>
    public class SidePanelViewModel : BindableBase
    {
        public static string DragDropFormat = $"{Config.Current.ProcessId}.PanelContent";


        public SidePanelViewModel(SidePanelGroup panel, ItemsControl itemsControl)
        {
            InitializeDropAccept(itemsControl);

            Panel = panel;
            Panel.PropertyChanged += Panel_PropertyChanged;

            Panel.SelectedPanelChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(PanelVisibility));
                AutoHideDescription.VisibleOnce();
                SelectedPanelChanged?.Invoke(s, e);
            };

            AutoHideDescription = new SidePanelAutoHideDescription(this);
            AutoHideDescription.VisibilityChanged += (s, e) =>
            {
                Visibility = e.Visibility;
            };
        }


        public event EventHandler<SelectedPanelChangedEventArgs> SelectedPanelChanged;


        public SidePanelAutoHideDescription AutoHideDescription { get; }

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
            set
            {
                if (SetProperty(ref _isDragged, value))
                {
                    RaisePropertyChanged(nameof(IsVisibleLocked));
                }
            }
        }

        private bool _isAutoHide;
        public bool IsAutoHide
        {
            get { return _isAutoHide; }
            set { if (_isAutoHide != value) { _isAutoHide = value; RaisePropertyChanged(); } }
        }

        // VisibleLock 条件
        // - v サイドバーアイコンドラッグ中
        // - パネルからのロック要求(項目名変更、コンテキストメニュー、ドラッグ等)
        public bool IsVisibleLocked
        {
            get
            {
                return IsDragged || Panel.IsVisibleLocked;
            }
        }

        /// <summary>
        /// パネルの表示状態。自動非表示機能により変化
        /// </summary>
        private Visibility _visibility;
        public Visibility Visibility
        {
            get { return _visibility; }
            private set
            {
                if (SetProperty(ref _visibility, value))
                {
                    RaisePropertyChanged(nameof(PanelVisibility));
                }

                Panel.IsVisible = Visibility == Visibility.Visible;
            }
        }

        /// <summary>
        /// サイドバーを含むパネル全体の表示状態。
        /// </summary>
        public Visibility PanelVisibility
        {
            get
            {
                return Visibility == Visibility.Visible && this.Panel.SelectedPanel != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // Model
        public SidePanelGroup Panel { get; private set; }



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
                case nameof(Panel.IsVisibleLocked):
                    RaisePropertyChanged(nameof(IsVisibleLocked));
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
    /// SidePanel用AutoHideBehavior補足
    /// </summary>
    public class SidePanelAutoHideDescription : AutoHideDescription
    {
        private SidePanelViewModel _self;

        public SidePanelAutoHideDescription(SidePanelViewModel self)
        {
            _self = self;
        }

        public override bool IsVisibleLocked()
        {
            var targetElement = ContextMenuWatcher.TargetElement;
            if (targetElement != null)
            {
                return VisualTreeUtility.HasParentElement(targetElement, _self.Panel.SelectedPanel?.View);
            }

            var dragElement = DragDropWatcher.DragElement;
            if (dragElement != null)
            {
                return VisualTreeUtility.HasParentElement(dragElement, _self.Panel.SelectedPanel?.View);
            }

            var renameElement = RenameManager.Current.RenameElement;
            if (renameElement != null)
            {
                return VisualTreeUtility.HasParentElement(renameElement, _self.Panel.SelectedPanel?.View);
            }

            return false;
        }
    }

}
