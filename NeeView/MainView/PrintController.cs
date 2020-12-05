using NeeView.Effects;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class PrintController
    {
        private MainViewComponent _viewComponent;
        private MainView _mainView;

        public PrintController(MainViewComponent viewComponent, MainView mainView)
        {
            _viewComponent = viewComponent;
            _mainView = mainView;
        }

        public bool CanPrint()
        {
            var mainContent = _viewComponent.ContentCanvas.MainContent;
            return mainContent != null && mainContent.IsValid;
        }

        public void Print()
        {
            Print(Window.GetWindow(_mainView), _mainView.PageContents, _mainView.MainContent.RenderTransform, _mainView.View.ActualWidth, _mainView.View.ActualHeight);
        }


        private void Print(Window owner, FrameworkElement element, Transform transform, double width, double height)
        {
            if (!CanPrint()) return;

            // 掃除しておく
            GC.Collect();

            var contents = _viewComponent.ContentCanvas.Contents;
            var mainContent = _viewComponent.ContentCanvas.MainContent;

            // スケールモード退避
            var scaleModeMemory = contents.ToDictionary(e => e, e => e.BitmapScalingMode);

            // アニメーション停止
            foreach (var content in contents)
            {
                content.AnimationImageVisibility = Visibility.Visible;
                content.AnimationPlayerVisibility = Visibility.Collapsed;
            }

            // 読み込み停止
            BookHub.Current.IsEnabled = false;

            // スライドショー停止
            SlideShow.Current.PauseSlideShow();

            try
            {
                var context = new PrintContext();
                context.MainContent = mainContent;
                context.Contents = contents;
                context.View = element;
                context.ViewTransform = transform;
                context.ViewWidth = width;
                context.ViewHeight = height;
                context.ViewEffect = ImageEffect.Current.Effect;
                context.Background = _viewComponent.ContentCanvasBrush.CreateBackgroundBrush();
                context.BackgroundFront = _viewComponent.ContentCanvasBrush.CreateBackgroundFrontBrush(new DpiScale(1, 1));

                var dialog = new PrintWindow(context);
                dialog.Owner = owner;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.ShowDialog();
            }
            finally
            {
                // スケールモード、アニメーション復元
                foreach (var content in contents)
                {
                    content.BitmapScalingMode = scaleModeMemory[content];
                    content.AnimationImageVisibility = Visibility.Collapsed;
                    content.AnimationPlayerVisibility = Visibility.Visible;
                }

                // 読み込み再会
                BookHub.Current.IsEnabled = true;

                // スライドショー再開
                SlideShow.Current.ResumeSlideShow();
            }
        }
    }

}
