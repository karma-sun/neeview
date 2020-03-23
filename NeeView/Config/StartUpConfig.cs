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

        // スプラッシュスクリーン
        [PropertyMember("@ParamIsSplashScreenEnabled")]
        public bool IsSplashScreenEnabled { get; set; } = true;

        // 多重起動を許可する
        [PropertyMember("@ParamIsMultiBootEnabled")]
        public bool IsMultiBootEnabled { get; set; }

        // ウィンドウ座標を復元する
        [PropertyMember("@ParamIsSaveWindowPlacement")]
        public bool IsRestoreWindowPlacement { get; set; } = true;

        // 複数ウィンドウの座標復元
        [PropertyMember("@ParamIsRestoreSecondWindow", Tips = "@ParamIsRestoreSecondWindowTips")]
        public bool IsRestoreSecondWindowPlacement { get; set; } = true;

        // フルスクリーン状態を復元する
        [PropertyMember("@ParamIsSaveFullScreen")]
        public bool IsRestoreFullScreen { get; set; }

        // 前回開いていたブックを開く
        [PropertyMember("@ParamIsOpenLastBook")]
        public bool IsOpenLastBook { get; set; }

        // 前回開いていた本棚を復元
        [PropertyMember("@ParamHistoryIsKeepLastFolder", Tips = "@ParamHistoryIsKeepLastFolderTips")]
        public bool IsOpenLastFolder { get; set; }


        #region 状態保存用

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

        #endregion
    }
}