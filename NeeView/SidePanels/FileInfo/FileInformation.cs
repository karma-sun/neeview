using NeeLaboratory.ComponentModel;
using NeeView.Windows.Data;
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
        private DelayValue<List<FileInformationSource>> _fileInformationsDelay;


        private FileInformation()
        {
            var mainViewComponent = MainViewComponent.Current;

            mainViewComponent.ContentCanvas.ContentChanged +=
                (s, e) => Update(mainViewComponent.ContentCanvas.Contents);

            _fileInformationsDelay = new DelayValue<List<FileInformationSource>>();
            _fileInformationsDelay.ValueChanged += (s, e) => RaisePropertyChanged(nameof(FileInformations));
        }


        public List<FileInformationSource> FileInformations
        {
            get { return _fileInformationsDelay.Value; }
        }


        public FileInformationSource GetMainFileInformation()
        {
            return FileInformations.OrderBy(e => e.Page?.Index ?? int.MaxValue).FirstOrDefault();
        }

        public void Update(IEnumerable<ViewContent> viewContents)
        {
            _fileInformations = viewContents?
                .Reverse()
                .Where(e => e.IsInformationValid)
                .Select(e => new FileInformationSource(e))
                .ToList();

            _fileInformationsDelay.SetValue(_fileInformations, 100); // 100ms delay
        }


        public void Update()
        {
            if (_fileInformations is null) return;

            foreach (var item in _fileInformations)
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
