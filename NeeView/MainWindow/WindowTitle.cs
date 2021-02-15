using NeeLaboratory.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{


    /// <summary>
    /// ウィンドウタイトル
    /// </summary>
    public class WindowTitle : BindableBase
    {
        static WindowTitle() => Current = new WindowTitle();
        public static WindowTitle Current { get; }


        private string _defaultWindowTitle;
        private string _title = "";

        private BookHub _bookHub;
        private MainViewComponent _mainViewComponent;
        private TitleStringService _titleStringService;
        private TitleString _titleString;


        public WindowTitle()
        {
            _bookHub = BookHub.Current;
            _mainViewComponent = MainViewComponent.Current;
            _titleStringService = TitleStringService.Default;

            _titleString = new TitleString(_titleStringService);
            _titleString.AddPropertyChanged(nameof(TitleString.Title), TitleString_TitleChanged);

            _defaultWindowTitle = $"{Environment.ApplicationName} {Environment.DispVersion}";

            _mainViewComponent.ContentCanvas.ContentChanged += (s, e) =>
            {
                UpdateFormat();
            };

            _bookHub.Loading += (s, e) =>
            {
                UpdateTitle();
            };

            Config.Current.WindowTitle.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(WindowTitleConfig.WindowTitleFormat1):
                    case nameof(WindowTitleConfig.WindowTitleFormat2):
                    case nameof(WindowTitleConfig.WindowTitleFormatMedia):
                        UpdateFormat();
                        break;
                }
            };

            UpdateTitle();
        }


        public string Title
        {
            get { return _title; }
            private set { _title = value; RaisePropertyChanged(); }
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
                ? Config.Current.WindowTitle.WindowTitleFormatMedia
                : subContent.IsValid && !subContent.IsDummy ? Config.Current.WindowTitle.WindowTitleFormat2 : Config.Current.WindowTitle.WindowTitleFormat1;

            _titleString.SetFormat(format);
        }

        private void UpdateTitle()
        {
            var address = _bookHub.Book?.Address;

            if (_bookHub.IsLoading)
            {
                Title = LoosePath.GetFileName(_bookHub.LoadingPath) + " " + Properties.Resources.Notice_LoadingTitle;
            }
            else if (address == null)
            {
                Title = _defaultWindowTitle;
            }
            else if (_mainViewComponent.ContentCanvas.MainContent?.Source == null)
            {
                Title = LoosePath.GetDispName(address);
            }
            else
            {
                Title = _titleString.Title;
            }
        }

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember(EmitDefaultValue = false)]
            public string WindowTitleFormat1 { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string WindowTitleFormat2 { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string WindowTitleFormatMedia { get; set; }


            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
                // before 34.0
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    const string WindowTitleFormat1Default = "$Book($Page/$PageMax) - $FullName";
                    const string WindowTitleFormat2Default = "$Book($Page/$PageMax) - $FullNameL | $NameR";
                    const string WindowTitleFormatMediaDefault = "$Book";

                    if (WindowTitleFormat1 == WindowTitleFormat1Default)
                    {
                        WindowTitleFormat1 = null;
                    }
                    if (WindowTitleFormat2 == WindowTitleFormat2Default)
                    {
                        WindowTitleFormat2 = null;
                    }
                    if (WindowTitleFormatMedia == WindowTitleFormatMediaDefault)
                    {
                        WindowTitleFormatMedia = null;
                    }
                }
            }

            public void RestoreConfig(Config config)
            {
                config.WindowTitle.WindowTitleFormat1 = WindowTitleFormat1;
                config.WindowTitle.WindowTitleFormat2 = WindowTitleFormat2;
                config.WindowTitle.WindowTitleFormatMedia = WindowTitleFormatMedia;
            }
        }

        #endregion
    }

}
