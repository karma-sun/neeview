using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class StartUpConfig : BindableBase
    {
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

        // 最後に開いたフォルダーの場所記憶
        [PropertyMember("@ParamHistoryIsKeepLastFolder", Tips = "@ParamHistoryIsKeepLastFolderTips")]
        public bool IsKeepLastFolder { get; set; }

    }
}