using NeeView.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace NeeView
{
    public class ExportImageSource
    {
        public string BookAddress { get; private set; }

        public List<Page> Pages { get; private set; }

        public FrameworkElement View { get; private set; }

        public FrameworkElement BackgroundView { get; private set; }

        public Brush Background { get; private set; }

        public Brush BackgroundFront { get; private set; }


        public Transform ViewTransform { get; private set; }

        public Effect ViewEffect { get; private set; }


        public static ExportImageSource Create()
        {
            var element = MainWindow.Current.PageContents;

#if false
            var transform = MainWindow.Current.MainContent.RenderTransform;
#else
            var rotateTransform = new RotateTransform(DragTransform.Current.Angle);
            var scaleTransform = new ScaleTransform(DragTransform.Current.ScaleX, DragTransform.Current.ScaleY);
            var transform = new TransformGroup();
            transform.Children.Add(scaleTransform);
            transform.Children.Add(rotateTransform);
#endif

            var context = new ExportImageSource();
            context.BookAddress = BookOperation.Current.Address;
            context.Pages = ContentCanvas.Current.CloneContents.Where(e => e?.Page != null).Select(e => e.Page).ToList();
            context.View = element;
            context.ViewTransform = transform;
            context.ViewEffect = ImageEffect.Current.Effect;
            context.Background = ContentCanvasBrush.Current.CreateBackgroundBrush();
            context.BackgroundFront = ContentCanvasBrush.Current.CreateBackgroundFrontBrush(new DpiScale(1, 1));

            return context;
        }

    }
}
