using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class WindowTitleConfig : BindableBase
    {
        private const string WindowTitleFormat1Default = "$Book ($Page / $PageMax) - $FullName";
        private const string WindowTitleFormat2Default = "$Book ($Page / $PageMax) - $FullNameL | $NameR";
        private const string WindowTitleFormatMediaDefault = "$Book";
        private string _windowTitleFormat1;
        private string _windowTitleFormat2;
        private string _windowTitleFormatMedia;
        private bool _isMainViewDisplayEnabled = true;


        /// <summary>
        /// ウィンドウタイトルフォーマット 1P用
        /// </summary>
        [PropertyMember("@ParamWindowTitleFormat1")]
        public string WindowTitleFormat1
        {
            get { return _windowTitleFormat1 ?? WindowTitleFormat1Default; }
            set { SetProperty(ref _windowTitleFormat1, CleanUpTitleFormat(value, WindowTitleFormat1Default)); }
        }

        /// <summary>
        /// ウィンドウタイトルフォーマット 2P用
        /// </summary>
        [PropertyMember("@ParamWindowTitleFormat2")]
        public string WindowTitleFormat2
        {
            get { return _windowTitleFormat2 ?? WindowTitleFormat2Default; }
            set { SetProperty(ref _windowTitleFormat2, CleanUpTitleFormat(value, WindowTitleFormat2Default)); }
        }

        /// <summary>
        /// ウィンドウタイトルフォーマット メディア用
        /// </summary>
        [PropertyMember("@ParamWindowTitleFormatMedia")]
        public string WindowTitleFormatMedia
        {
            get { return _windowTitleFormatMedia ?? WindowTitleFormatMediaDefault; }
            set { SetProperty(ref _windowTitleFormatMedia, CleanUpTitleFormat(value, WindowTitleFormatMediaDefault)); }
        }

        /// <summary>
        /// タイトルバーが表示されておらず、スライダーにフォーカスがある場合等にキャンバスにタイトルを表示する
        /// </summary>
        [PropertyMember("@ParamIsVisibleWindowTitle")]
        public bool IsMainViewDisplayEnabled
        {
            get { return _isMainViewDisplayEnabled; }
            set { SetProperty(ref _isMainViewDisplayEnabled, value); }
        }


        private string CleanUpTitleFormat(string source, string defaultFormat)
        {
            return string.IsNullOrEmpty(source) ? defaultFormat : source;
        }
    }
}


