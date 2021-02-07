using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class MainViewViewModel : BindableBase
    {
        private MainViewComponent _viewComponent;


        public MainViewViewModel(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;

            InitializeContextMenu();
            InitializeBusyVisibility();

            Config.Current.View.AddPropertyChanged(nameof(ViewConfig.MainViewMergin),
                (s, e) => RaisePropertyChanged(nameof(MainViewMergin)));
        }


        public MainViewComponent ViewComponent => _viewComponent;

        public ContentCanvas ContentCanvas => _viewComponent.ContentCanvas;

        public ContentCanvasBrush ContentCanvasBrush => _viewComponent.ContentCanvasBrush;

        public ImageEffect ImageEffect => ImageEffect.Current;

        public WindowTitle WindowTitle => WindowTitle.Current;

        public InfoMessage InfoMessage => InfoMessage.Current;

        public LoupeTransform LoupeTransform => _viewComponent.LoupeTransform;

        public MouseInput MouseInput => _viewComponent.MouseInput;

        public TouchInput TouchInput => _viewComponent.TouchInput;

        public Thickness MainViewMergin => new Thickness(Config.Current.View.MainViewMergin);

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

        public void StretchWindow(Window window, Size canvasSize, Size contentSize)
        {
            if (contentSize.IsEmptyOrZero())
            {
                throw new ArgumentException($"canvasSize is 0.", nameof(canvasSize));
            }

            if (window.WindowState != WindowState.Normal)
            {
                throw new InvalidOperationException($"need Window.State is Normal");
            }

            var frameWidth = window.ActualWidth - canvasSize.Width;
            var frameHeight = window.ActualHeight - canvasSize.Height;
            if (frameWidth < 0.0 || frameHeight < 0.0)
            {
                throw new ArgumentException($"canvasSize must be smaller than Window.Size.", nameof(canvasSize));
            }

            var fixedSize = Config.Current.View.IsBaseScaleEnabled ? contentSize.Multi(1.0 / Config.Current.View.BaseScale) : contentSize;

            var limitSize = new Size(SystemParameters.VirtualScreenWidth - frameWidth, SystemParameters.VirtualScreenHeight - frameHeight);

            Size newCanvasSize;
            switch (_viewComponent.ContentCanvas.GetStretchMode())
            {
                case PageStretchMode.Uniform:
                case PageStretchMode.UniformToSize:
                    newCanvasSize = fixedSize.Limit(limitSize);
                    break;
                default:
                    newCanvasSize = fixedSize.Clamp(limitSize);
                    break;
            }

            window.Width = newCanvasSize.Width + frameWidth;
            window.Height = newCanvasSize.Height + frameHeight;

            _viewComponent.ContentCanvas.Stretch(ignoreViewOrigin: true);
        }

        public void StretchScale(Size contentSize, Size canvasSize)
        {
            var scaleX = canvasSize.Width / contentSize.Width;
            var scaleY = canvasSize.Height / contentSize.Height;
            var scale = Math.Max(scaleX, scaleY);

            switch (_viewComponent.ContentCanvas.GetStretchMode())
            {
                case PageStretchMode.UniformToHorizontal:
                    scale = scaleX;
                    break;
                case PageStretchMode.UniformToVertical:
                    scale = scaleY;
                    break;
            }

            if (Math.Abs(1.0 - scale) < 0.01)
            {
                scale = 1.0;
            }

            if (Config.Current.View.IsBaseScaleEnabled)
            {
                scale *= Config.Current.View.BaseScale;
            }

            _viewComponent.DragTransform.SetScale(scale, TransformActionType.None);
        }
    }
}
