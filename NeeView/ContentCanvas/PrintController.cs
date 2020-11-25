using NeeView.Effects;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class PrintController
    {
        private ViewComponent _viewComponent;
        private MainWindow _mainWondow;

        public PrintController(ViewComponent viewComponent, MainWindow mainWondow)
        {
            _viewComponent = viewComponent;
            _mainWondow = mainWondow;
        }

        public bool CanPrint()
        {
            var mainContent = _viewComponent.ContentCanvas.MainContent;
            return mainContent != null && mainContent.IsValid;
        }

        public void Print()
        {
            Print(_mainWondow, _mainWondow.PageContents, _mainWondow.MainContent.RenderTransform, _mainWondow.MainView.ActualWidth, _mainWondow.MainView.ActualHeight);
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
