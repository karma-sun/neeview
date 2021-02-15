using NeeLaboratory.ComponentModel;
using System.Linq;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// ページタイトル
    /// </summary>
    public class PageTitle : BindableBase
    {
        static PageTitle() => Current = new PageTitle();
        public static PageTitle Current { get; }


        private string _defaultPageTitle = null;
        private string _title = "";

        private BookHub _bookHub;
        private MainViewComponent _mainViewComponent;
        private TitleStringService _titleStringService;
        private TitleString _titleString;


        public PageTitle()
        {
            _bookHub = BookHub.Current;
            _mainViewComponent = MainViewComponent.Current;
            _titleStringService = TitleStringService.Default;

            _titleString = new TitleString(_titleStringService);
            _titleString.AddPropertyChanged(nameof(TitleString.Title), TitleString_TitleChanged);

            _mainViewComponent.ContentCanvas.ContentChanged += (s, e) =>
            {
                UpdateFormat();
            };

            _bookHub.Loading += (s, e) =>
            {
                UpdateTitle();
            };

            Config.Current.PageTitle.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(PageTitleConfig.PageTitleFormat1):
                    case nameof(PageTitleConfig.PageTitleFormat2):
                    case nameof(PageTitleConfig.PageTitleFormatMedia):
                        UpdateFormat();
                        break;
                }
            };

            UpdateTitle();
        }


        public string Title
        {
            get { return _title; }
            private set { SetProperty(ref _title, value); }
        }


        private void TitleString_TitleChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateTitle();
        }

        private void UpdateFormat()
        {
            var contents = _mainViewComponent.ContentCanvas.CloneContents;
            var mainContent = _mainViewComponent.ContentCanvas.MainContent;
            var subContent = contents.First(e => e != mainContent);

            string format = mainContent is MediaViewContent
                ? Config.Current.PageTitle.PageTitleFormatMedia
                : subContent.IsValid && !subContent.IsDummy ? Config.Current.PageTitle.PageTitleFormat2 : Config.Current.PageTitle.PageTitleFormat1;

            _titleString.SetFormat(format);
        }

        private void UpdateTitle()
        {
            if (_bookHub.IsLoading)
            {
                Title = _defaultPageTitle;
            }
            else if (_bookHub.Book?.Address == null)
            {
                Title = _defaultPageTitle;
            }
            else if (_mainViewComponent.ContentCanvas.MainContent?.Source == null)
            {
                Title = _defaultPageTitle;
            }
            else
            {
                Title = _titleString.Title;
            }
        }

    }

}
