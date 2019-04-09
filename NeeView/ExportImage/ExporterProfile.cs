using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// 画像ファイル出力設定
    /// </summary>
    public class ExporterProfile
    {
        static ExporterProfile() => Current = new ExporterProfile();
        public static ExporterProfile Current { get; }

        private ExporterProfile()
        {
        }

        public bool IsHintCloneDefault { get; set; } = true;

        [PropertyRange("@ParamExportImageQualityLevel", 5, 100, TickFrequency =5, Tips = "@ParamExportImageQualityLevelTips")]
        public int QualityLevel { get; set; } = 80;

        public string ExportFolder { get; set; } = null;

        [PropertyMember("@ParamExportImageIsEnableExportFolder")]
        public bool IsEnableExportFolder { get; set; } = true;


        #region Memento

        //
        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsHintCloneDefault { get; set; }

            [DataMember]
            public int QualityLevel { get; set; }

            [DataMember]
            public string ExportFolder { get; set; }

            [DataMember]
            public bool IsEnableExportFolder { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsHintCloneDefault = this.IsHintCloneDefault;
            memento.QualityLevel = this.QualityLevel;
            memento.ExportFolder = this.ExportFolder;
            memento.IsEnableExportFolder = this.IsEnableExportFolder;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsHintCloneDefault = memento.IsHintCloneDefault;
            this.QualityLevel = memento.QualityLevel;
            this.ExportFolder = memento.ExportFolder;
            this.IsEnableExportFolder = memento.IsEnableExportFolder;
        }

        #endregion
    }
}
