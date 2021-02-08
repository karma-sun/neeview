using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class FileInformation : BindableBase
    {
        public static FileInformation Current { get; }
        static FileInformation() => Current = new FileInformation();


        private List<FileInformationSource> _fileInformations;


        private FileInformation()
        {
            var mainViewComponent = MainViewComponent.Current;

            mainViewComponent.ContentCanvas.ContentChanged +=
                (s, e) => Update(mainViewComponent.ContentCanvas.Contents);
        }


        public List<FileInformationSource> FileInformations
        {
            get { return _fileInformations; }
            set { SetProperty(ref _fileInformations, value); }
        }


        public void Update(IEnumerable<ViewContent> viewContents)
        {
            FileInformations = viewContents?
                .Reverse()
                .Where(e => e.IsInformationValid)
                .Select(e => new FileInformationSource(e))
                .ToList();
        }

        public void Update()
        {
            if (FileInformations is null) return;

            foreach(var item in FileInformations)
            {
                item.Update();
            }
        }


        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsVisibleBitsPerPixel { get; set; }
            [DataMember]
            public bool IsVisibleLoader { get; set; }
            [DataMember]
            public bool IsVisibleFilePath { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Information.IsVisibleBitsPerPixel = IsVisibleBitsPerPixel;
                config.Information.IsVisibleLoader = IsVisibleLoader;
                config.Information.IsVisibleFilePath = IsVisibleFilePath;
            }
        }

        #endregion
    }


    /// <summary>
    /// 旧：ファイル情報パネル設定
    /// 互換性のために残してあります
    /// </summary>
    [Obsolete]
    [DataContract]
    public class FileInfoSetting
    {
        [DataMember]
        public bool IsUseExifDateTime { get; set; }

        [DataMember]
        public bool IsVisibleBitsPerPixel { get; set; }

        [DataMember]
        public bool IsVisibleLoader { get; set; }
    }
}
