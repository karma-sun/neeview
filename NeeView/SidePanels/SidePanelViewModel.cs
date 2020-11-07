using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NeeView.Windows;
using NeeView.Windows.Data;
using NeeView.Windows.Media;
using NeeLaboratory.Windows.Input;
using NeeLaboratory.ComponentModel;
using NeeView.Runtime.LayoutPanel;

namespace NeeView
{
    /// <summary>
    /// SidePanel ViewModel.
    /// パネル単位のVM. SidePanelFrameViewModelの子
    /// </summary>
    public class SidePanelViewModel : BindableBase
    {
        public static string DragDropFormat = $"{Environment.ProcessId}.PanelContent";

        private LayoutDockPanelContent _dock;
        private SidePanelDropAcceptor _dropAcceptor;
        private double _width = 300.0;
        private double _maxWidth;
        private bool _isDragged;
        private bool _isAutoHide;
        private Visibility _visibility;


        public SidePanelViewModel(ItemsControl itemsControl, LayoutDockPanelContent dock)
        {
            _dock = dock;
            _dropAcceptor = new SidePanelDropAcceptor(itemsControl, dock);

            _dock.AddPropertyChanged(nameof(_dock.SelectedItem), (s, e) =>
            {
                RaisePropertyChanged(nameof(PanelVisibility));
                if (_dock.SelectedItem != null)
                {
                    AutoHideDescription.VisibleOnce();
                }
            });

            AutoHideDescription = new SidePanelAutoHideDescription(this);
            AutoHideDescription.VisibilityChanged += (s, e) =>
            {
                Visibility = e.Visibility;
            };
        }


        public DropAcceptDescription Description => _dropAcceptor.Description;

        public SidePanelAutoHideDescription AutoHideDescription { get; }

        public virtual double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, Math.Min(value, MaxWidth)); }
        }

        public double MaxWidth
        {
            get { return _maxWidth; }
            set { if (_maxWidth != value) { _maxWidth = value; RaisePropertyChanged(); } }
        }

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
                if (IsDragged) return true;

                if (_dock.SelectedItem != null)
                {
                    var layoutPanelManager = (MainLayoutPanelManager)_dock.LayoutPanelManager;
                    return _dock.SelectedItem.Any(e => layoutPanelManager.PanelsSource[e.Key].IsVisibleLock);
                }

                return false;
            }
        }

        /// <summary>
        /// パネルの表示状態。自動非表示機能により変化
        /// </summary>
        public Visibility Visibility
        {
            get { return _visibility; }
            private set
            {
                if (SetProperty(ref _visibility, value))
                {
                    RaisePropertyChanged(nameof(PanelVisibility));
                }

                //// TODO: これは??
                ////Panel.IsVisible = Visibility == Visibility.Visible;
            }
        }

        /// <summary>
        /// サイドバーを含むパネル全体の表示状態。
        /// </summary>
        public Visibility PanelVisibility
        {
            get
            {
                return Visibility == Visibility.Visible && _dock.SelectedItem != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        /// <summary>
        /// パネルの表示/非表示トグル
        /// </summary>
        public void Toggle()
        {
            _dock.ToggleSelectedItem();
        }

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
            // TODO: ひとまず無効にしておく。あとで精査せよ
#if false
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
#endif

            return false;
        }
    }


    /// <summary>
    /// LeftPanel ViewModel
    /// </summary>
    public class LeftPanelViewModel : SidePanelViewModel
    {
        public LeftPanelViewModel(ItemsControl itemsControl, LayoutDockPanelContent dock) : base(itemsControl, dock)
        {
        }

        public override double Width
        {
            get { return Config.Current.Panels.LeftPanelWidth; }
            set
            {
                if (Config.Current.Panels.LeftPanelWidth != value)
                {
                    Config.Current.Panels.LeftPanelWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// RightPanel ViewModel
    /// </summary>
    public class RightPanelViewModel : SidePanelViewModel
    {
        public RightPanelViewModel(ItemsControl itemsControl, LayoutDockPanelContent dock) : base(itemsControl, dock)
        {
        }

        public override double Width
        {
            get { return Config.Current.Panels.RightPanelWidth; }
            set
            {
                if (Config.Current.Panels.RightPanelWidth != value)
                {
                    Config.Current.Panels.RightPanelWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
