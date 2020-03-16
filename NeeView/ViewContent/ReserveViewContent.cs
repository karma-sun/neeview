using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// Reserve ViewContent
    /// </summary>
    public class ReserveViewContent : ViewContent
    {
        #region Constructors

        public ReserveViewContent(ViewContentSource source, ViewContent old) : base(source)
        {
            this.Size = new Size(480, 680);
            this.Color = old != null ? old.Color : Colors.Black;
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = new ViewContentControl(CreateView(this.Source, parameter));
        }

        /// <summary>
        /// 読み込み中ビュー生成
        /// </summary>
        private FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter)
        {
            var rectangle = new Rectangle();
            rectangle.Fill = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));

            return rectangle;
        }

        public override bool IsBitmapScalingModeSupported() => false;

        #endregion

        #region Static Methods

        public static ViewContent Create(ViewContentSource source, ViewContent oldViewContent)
        {
            ViewContent viewContent = oldViewContent;
            if (!Config.Current.Performance.IsLoadingPageVisible || oldViewContent?.View is null)
            {
                 var newViewContent = new ReserveViewContent(source, oldViewContent);
                newViewContent.Initialize();
                viewContent = newViewContent;
            }

            viewContent.View.SetMessage(LoosePath.GetFileName(source.Page.EntryFullName));
            return viewContent;
        }

        #endregion
    }
}
