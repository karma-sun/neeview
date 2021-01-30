using System;

namespace NeeView
{
    public class ScrollPageController
    {
        private MainViewComponent _viewContent;
        private BookSettingPresenter _bookSettingPresenter;
        private BookOperation _bookOperation;
        private DateTime _scrollPageTime;

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
            bool isScrolled = _viewContent.IsLoupeMode ? false : _viewContent.DragTransformControl.ScrollN(-1, bookReadDirection, true, parameter);
        }

        /// <summary>
        /// N字スクロール
        /// </summary>
        public void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter)
        {
            int bookReadDirection = (_bookSettingPresenter.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = _viewContent.IsLoupeMode ? false : _viewContent.DragTransformControl.ScrollN(+1, bookReadDirection, true, parameter);
        }

        /// <summary>
        /// スクロール＋前のページに戻る。
        /// ルーペ使用時はページ移動のみ行う。
        /// </summary>
        public void PrevScrollPage(object sender, ScrollPageCommandParameter parameter)
        {
            int bookReadDirection = (_bookSettingPresenter.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = _viewContent.IsLoupeMode ? false : _viewContent.DragTransformControl.ScrollN(-1, bookReadDirection, parameter.IsNScroll, parameter);

            if (!isScrolled)
            {
                var margin = TimeSpan.FromSeconds(parameter.PageMoveMargin);
                var span = DateTime.Now - _scrollPageTime;
                if (margin <= TimeSpan.Zero || margin <= span)
                {
                    _viewContent.ContentCanvas.NextViewOrigin = (_bookSettingPresenter.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightBottom : DragViewOrigin.LeftBottom;
                    _bookOperation.PrevPage(sender);
                    return;
                }
            }

            _scrollPageTime = DateTime.Now;
        }

        /// <summary>
        /// スクロール＋次のページに進む。
        /// ルーペ使用時はページ移動のみ行う。
        /// </summary>
        public void NextScrollPage(object sender, ScrollPageCommandParameter parameter)
        {
            int bookReadDirection = (_bookSettingPresenter.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = _viewContent.IsLoupeMode ? false : _viewContent.DragTransformControl.ScrollN(+1, bookReadDirection, parameter.IsNScroll, parameter);

            if (!isScrolled)
            {
                var margin = TimeSpan.FromSeconds(parameter.PageMoveMargin);
                var span = DateTime.Now - _scrollPageTime;
                if (margin <= TimeSpan.Zero || margin <= span)
                {
                    _viewContent.ContentCanvas.NextViewOrigin = (_bookSettingPresenter.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightTop : DragViewOrigin.LeftTop;
                    _bookOperation.NextPage(sender);
                    return;
                }
            }

            _scrollPageTime = DateTime.Now;
        }
    }

}
