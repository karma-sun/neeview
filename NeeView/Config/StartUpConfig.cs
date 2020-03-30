using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class StartUpConfig : BindableBase
    {
        private string _lastBookPath;
        private string _lastFolderPath;
        private bool _isSplashScreenEnabled = true;
        private bool _isMultiBootEnabled;
        private bool _isRestoreWindowPlacement = true;
        private bool _isRestoreSecondWindowPlacement = true;
        private bool _isRestoreFullScreen;
        private bool _isOpenLastBook;
        private bool _isOpenLastFolder;
        private bool _isAutoPlaySlideShow;


        // スプラッシュスクリーン
        [PropertyMember("@ParamIsSplashScreenEnabled")]
        public bool IsSplashScreenEnabled
        {
            get { return _isSplashScreenEnabled; }
            set { SetProperty(ref _isSplashScreenEnabled, value); }
        }

        // 多重起動を許可する
        [PropertyMember("@ParamIsMultiBootEnabled")]
        public bool IsMultiBootEnabled
        {
            get { return _isMultiBootEnabled; }
            set { SetProperty(ref _isMultiBootEnabled, value); }
        }

        // ウィンドウ座標を復元する
        [PropertyMember("@ParamIsSaveWindowPlacement")]
        public bool IsRestoreWindowPlacement
        {
            get { return _isRestoreWindowPlacement; }
            set { SetProperty(ref _isRestoreWindowPlacement, value); }
        }

        // 複数ウィンドウの座標復元
        [PropertyMember("@ParamIsRestoreSecondWindow", Tips = "@ParamIsRestoreSecondWindowTips")]
        public bool IsRestoreSecondWindowPlacement
        {
            get { return _isRestoreSecondWindowPlacement; }
            set { SetProperty(ref _isRestoreSecondWindowPlacement, value); }
        }

        // フルスクリーン状態を復元する
        [PropertyMember("@ParamIsSaveFullScreen")]
        public bool IsRestoreFullScreen
        {
            get { return _isRestoreFullScreen; }
            set { SetProperty(ref _isRestoreFullScreen, value); }
        }

        // 前回開いていたブックを開く
        [PropertyMember("@ParamIsOpenLastBook")]
        public bool IsOpenLastBook
        {
            get { return _isOpenLastBook; }
            set { SetProperty(ref _isOpenLastBook, value); }
        }

        // 前回開いていた本棚を復元
        [PropertyMember("@ParamHistoryIsKeepLastFolder", Tips = "@ParamHistoryIsKeepLastFolderTips")]
        public bool IsOpenLastFolder
        {
            get { return _isOpenLastFolder; }
            set { SetProperty(ref _isOpenLastFolder, value); }
        }

        /// <summary>
        /// 起動時にスライドショーを開始する
        /// </summary>
        [PropertyMember("@ParamIsAutoPlaySlideShow")]
        public bool IsAutoPlaySlideShow
        {
            get { return _isAutoPlaySlideShow; }
            set { SetProperty(ref _isAutoPlaySlideShow, value); }
        }


        #region HiddenParameters

        [PropertyMapIgnore]
        public string LastBookPath
        {
            get { return IsOpenLastBook ? _lastBookPath : null; }
            set { SetProperty(ref _lastBookPath, IsOpenLastBook ? value : null); }
        }

        [PropertyMapIgnore]
        public string LastFolderPath
        {
            get { return IsOpenLastFolder ? _lastFolderPath : null; }
            set { SetProperty(ref _lastFolderPath, IsOpenLastFolder ? value : null); }
        }

        #endregion HiddenParameters
    }
}