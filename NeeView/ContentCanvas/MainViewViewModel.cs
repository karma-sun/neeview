using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class MainViewViewModel : BindableBase
    {
        private ViewComponent _viewComponent;
        private Thickness _mainViewMergin;


        public MainViewViewModel(ViewComponent viewComponent)
        {
            _viewComponent = viewComponent;

            InitializeContextMenu();
            InitializeBusyVisibility();
        }


        public ViewComponent ViewComponent => _viewComponent;

        public ContentCanvas ContentCanvas => _viewComponent.ContentCanvas;

        public ContentCanvasBrush ContentCanvasBrush => _viewComponent.ContentCanvasBrush;

        public ImageEffect ImageEffect => ImageEffect.Current;

        public WindowTitle WindowTitle => WindowTitle.Current;

        public InfoMessage InfoMessage => InfoMessage.Current;

        public LoupeTransform LoupeTransform => _viewComponent.LoupeTransform;

        public MouseInput MouseInput => _viewComponent.MouseInput;

        public TouchInput TouchInput => _viewComponent.TouchInput;


        public Thickness MainViewMergin
        {
            get { return _mainViewMergin; }
            set { SetProperty(ref _mainViewMergin, value); }
        }

        #region BusyVisibility

        private Visibility _busyVisibility = Visibility.Collapsed;

        public Visibility BusyVisibility
        {
            get { return _busyVisibility; }
            set { if (_busyVisibility != value) { _busyVisibility = value; RaisePropertyChanged(); } }
        }

        private void InitializeBusyVisibility()
        {
            _viewComponent.ContentRebuild.AddPropertyChanged(nameof(ContentRebuild.IsBusy),
                (s, e) => UpdateBusyVisibility());

            BookOperation.Current.AddPropertyChanged(nameof(BookOperation.IsBusy),
                (s, e) => UpdateBusyVisibility());

            BookHub.Current.AddPropertyChanged(nameof(BookHub.IsLoading),
                (s, e) => UpdateBusyVisibility());
        }

        private void UpdateBusyVisibility()
        {
            ////Debug.WriteLine($"IsBusy: {BookHub.Current.IsLoading}, {BookOperation.Current.IsBusy}, {ContentRebuild.Current.IsBusy}");
            this.BusyVisibility = Config.Current.Notice.IsBusyMarkEnabled && (BookHub.Current.IsLoading || BookOperation.Current.IsBusy || _viewComponent.ContentRebuild.IsBusy) && !SlideShow.Current.IsPlayingSlideShow ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion BusyVisibility

        #region ContextMenu

        private ContextMenu _contextMenu;

        public ContextMenu ContextMenu
        {
            get
            {
                if (ContextMenuManager.Current.IsDarty)
                {
                    Debug.WriteLine($"new ContextMenu.");
                    _contextMenu = ContextMenuManager.Current.ContextMenu;
                    _contextMenu?.UpdateInputGestureText();
                }
                return _contextMenu;
            }
        }



        private void InitializeContextMenu()
        {
            ContextMenuManager.Current.AddPropertyChanged(nameof(ContextMenuManager.Current.SourceTree),
                (s, e) => UpdateContextMenu());

            RoutedCommandTable.Current.Changed +=
                (s, e) => UpdateContextMenu();

            UpdateContextMenu();
        }

        public void UpdateContextMenu()
        {
            if (ContextMenuManager.Current.IsDarty)
            {
                RaisePropertyChanged(nameof(ContextMenu));
            }
        }


        #endregion ContextMenu


        public void SetViewSize(double width, double height)
        {
            _viewComponent.ContentCanvas.SetViewSize(width, height);
            _viewComponent.DragTransformControl.SnapView();
        }

        public void Stretch()
        {
            _viewComponent.ContentCanvas.Stretch();
        }

        public PageStretchMode GetStretchMode()
        {
            return _viewComponent.ContentCanvas.GetStretchMode();
        }
    }
}
