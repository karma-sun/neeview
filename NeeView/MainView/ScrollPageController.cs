using System;

namespace NeeView
{
    public class ScrollPageController
    {
        private MainViewComponent _viewContent;
        private BookSettingPresenter _bookSettingPresenter;
        private BookOperation _bookOperation;
        private RepeatLimiter _repeatLimiter = new RepeatLimiter();


        public ScrollPageController(MainViewComponent viewContent, BookSettingPresenter bookSettingPresenter, BookOperation bookOperation)
        {
            _viewContent = viewContent;
            _bookSettingPresenter = bookSettingPresenter;
            _bookOperation = bookOperation;
        }

        /// <summary>
        /// N字スクロール
        /// </summary>
        public void ScrollNTypeUp(ViewScrollNTypeCommandParameter parameter)
        {
            int bookReadDirection = (_bookSettingPresenter.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            if (CanScroll())
            {
                _viewContent.DragTransformControl.ScrollN(-1, bookReadDirection, true, true, parameter);
            }
        }

        /// <summary>
        /// N字スクロール
        /// </summary>
        public void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter)
        {
            int bookReadDirection = (_bookSettingPresenter.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            if (CanScroll())
            {
                _viewContent.DragTransformControl.ScrollN(+1, bookReadDirection, true, true, parameter);
            }
        }


        /// <summary>
        /// スクロール＋前のページに戻る。
        /// </summary>
        public void PrevScrollPage(object sender, ScrollPageCommandParameter parameter)
        {
            int bookReadDirection = (_bookSettingPresenter.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isLineBreakStop = parameter.LineBreakStopMode == LineBreakStopMode.Line;
            bool isScrolled = CanScroll() && _viewContent.DragTransformControl.ScrollN(-1, bookReadDirection, parameter.IsNScroll, isLineBreakStop, parameter);

            if (!isScrolled && !_repeatLimiter.IsLimit((int)(parameter.LineBreakStopTime * 1000.0)))
            {
                _viewContent.ContentCanvas.NextViewOrigin = (_bookSettingPresenter.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightBottom : DragViewOrigin.LeftBottom;
                _bookOperation.PrevPage(sender);
            }

            _repeatLimiter.Reset();
        }

        /// <summary>
        /// スクロール＋次のページに進む。
        /// </summary>
        public void NextScrollPage(object sender, ScrollPageCommandParameter parameter)
        {
            int bookReadDirection = (_bookSettingPresenter.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isLineBreakStop = parameter.LineBreakStopMode == LineBreakStopMode.Line;
            bool isScrolled = CanScroll() && _viewContent.DragTransformControl.ScrollN(+1, bookReadDirection, parameter.IsNScroll, isLineBreakStop, parameter);

            if (!isScrolled && !_repeatLimiter.IsLimit((int)(parameter.LineBreakStopTime * 1000.0)))
            {
                _viewContent.ContentCanvas.NextViewOrigin = (_bookSettingPresenter.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightTop : DragViewOrigin.LeftTop;
                _bookOperation.NextPage(sender);
            }

            _repeatLimiter.Reset();
        }

        /// <summary>
        /// スクロール可能判定
        /// </summary>
        /// <remarks>
        /// ホバースクロール等の他のスクロールが優先されるときはN字スクロールはできない
        /// </remarks>
        private bool CanScroll()
        {
            return !Config.Current.Mouse.IsHoverScroll && !_viewContent.IsLoupeMode;
        }
    }

}
