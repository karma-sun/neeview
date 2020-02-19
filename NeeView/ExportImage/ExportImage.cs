//using System.Drawing;

using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// 画像ファイル出力
    /// </summary>
    public class ExportImage : BindableBase
    {
        private ExportImageSource _source;

        private IImageExporter _exporter;

        public ExportImage(ExportImageSource source)
        {
            _source = source;

            UpdateExporter();
            UpdatePreview();
        }

        public string ExportFolder { get; set; }

        private ExportImageMode _mode;
        public ExportImageMode Mode
        {
            get { return _mode; }
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    UpdateExporter();
                    UpdatePreview();
                }
            }
        }

        /// <summary>
        /// ViewImage用：背景を含める
        /// </summary>
        private bool _hasBackground;
        public bool HasBackground
        {
            get { return _hasBackground; }
            set
            {
                if (SetProperty(ref _hasBackground, value))
                {
                    _exporter.HasBackground = _hasBackground;
                    UpdatePreview();
                }
            }
        }

        private FrameworkElement _preview;
        public FrameworkElement Preview
        {
            get { return _preview; }
            set { SetProperty(ref _preview, value); }
        }

        private string _imageFormatNote;
        public string ImageFormatNote
        {
            get { return _imageFormatNote; }
            set { SetProperty(ref _imageFormatNote, value); }
        }

        public int QualityLevel { get; internal set; }


        private static IImageExporter CreateExporter(ExportImageMode mode, ExportImageSource source, bool hasBackground)
        {
            switch (mode)
            {
                case ExportImageMode.Original:
                    return new OriginalImageExporter(source) { HasBackground = hasBackground };

                case ExportImageMode.View:
                    return new ViewImageExporter(source) { HasBackground = hasBackground };

                default:
                    throw new InvalidOperationException();
            }
        }


        public void UpdateExporter()
        {
            _exporter = CreateExporter(_mode, _source, _hasBackground);

            UpdatePreview();
        }

        public void UpdatePreview()
        {
            AppDispatcher.BeginInvoke(() =>
            {
                try
                {
                    var content = _exporter.CreateView();
                    Preview = content.View;
                    ImageFormatNote = content.Size.IsEmpty ? "" : $"{(int)content.Size.Width} x {(int)content.Size.Height}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Preview = null;
                    ImageFormatNote = "Error.";
                }
            });
        }


        public void Export(string path, bool isOverwrite)
        {
            path = System.IO.Path.GetFullPath(path);

            _exporter.Export(path, isOverwrite, QualityLevel);
            ExportFolder = System.IO.Path.GetDirectoryName(path);
        }

        public string CreateFileName(ExportImageFileNameMode fileNameMode, ExportImageFormat format)
        {
            var nameMode = fileNameMode == ExportImageFileNameMode.Default
                ? _mode == ExportImageMode.Original ? ExportImageFileNameMode.Original : ExportImageFileNameMode.BookPageNumber
                : fileNameMode;

            if (nameMode == ExportImageFileNameMode.Original)
            {
                return LoosePath.ValidFileName(_source.Pages[0].EntryLastName);
            }
            else
            {
                var bookName = LoosePath.GetFileNameWithoutExtension(_source.BookAddress);

                var indexLabel = _mode != ExportImageMode.Original && _source.Pages.Count > 1
                    ? $"{_source.Pages[0].Index:000}-{_source.Pages[1].Index:000}"
                    : $"{_source.Pages[0].Index:000}";
                
                var extension = _mode == ExportImageMode.Original
                    ? LoosePath.GetExtension(_source.Pages[0].EntryLastName).ToLower()
                    : format == ExportImageFormat.Png ? ".png" : ".jpg";

                return LoosePath.ValidFileName($"{bookName}_{indexLabel}{extension}");
            }
        }
    }
}
