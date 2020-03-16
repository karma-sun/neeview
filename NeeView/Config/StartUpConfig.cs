using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class StartUpConfig : BindableBase
    {
        public StartUpConfig()
        {
            Constructor();
        }


        // スプラッシュスクリーン
        [DataMember]
        [PropertyMember("@ParamIsSplashScreenEnabled")]
        public bool IsSplashScreenEnabled { get; set; }

        // 多重起動を許可する
        [DataMember]
        [PropertyMember("@ParamIsMultiBootEnabled")]
        public bool IsMultiBootEnabled { get; set; }

        // ウィンドウ座標を復元する
        [DataMember]
        [PropertyMember("@ParamIsSaveWindowPlacement")]
        public bool IsRestoreWindowPlacement { get; set; }

        // フルスクリーン状態を復元する
        [DataMember]
        [PropertyMember("@ParamIsSaveFullScreen")]
        public bool IsRestoreFullScreen { get; set; }

        // 前回開いていたブックを開く
        [DataMember]
        [PropertyMember("@ParamIsOpenLastBook")]
        public bool IsOpenLastBook { get; set; }

        // 最後に開いたフォルダーの場所記憶
        [DataMember]
        [PropertyMember("@ParamHistoryIsKeepLastFolder", Tips = "@ParamHistoryIsKeepLastFolderTips")]
        public bool IsKeepLastFolder { get; set; }


        private void Constructor()
        {
            IsSplashScreenEnabled = true;
            IsRestoreWindowPlacement = true;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            Constructor();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
        }
    }
}