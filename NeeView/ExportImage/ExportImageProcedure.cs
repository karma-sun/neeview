using NeeLaboratory.IO;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeeView
{
    /// <summary>
    /// 画像出力の処理フロー
    /// </summary>
    public class ExportImageProcedure
    {
        public void Execute(ExportImageCommandParameter parameter)
        {
            var source = ExportImageSource.Create();

            var exporter = new ExportImage(source);
            exporter.ExportFolder = string.IsNullOrWhiteSpace(parameter.ExportFolder) ? System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) : parameter.ExportFolder;
            exporter.Mode = parameter.Mode;
            exporter.HasBackground = parameter.HasBackground;
            exporter.QualityLevel = parameter.QualityLevel;

            string filename = exporter.CreateFileName(parameter.FileNameMode, parameter.FileFormat);
            bool isOverweite;

            if (string.IsNullOrWhiteSpace(parameter.ExportFolder))
            {
                var dialog = new ExportImageSeveFileDialog
                {
                    InitialDirectory = exporter.ExportFolder,
                    FileName = filename,
                    CanSelectFormat = exporter.Mode == ExportImageMode.View
                };
                var result = dialog.ShowDialog(MainWindow.Current);
                if (result != true) return;
                filename = dialog.FileName;
                isOverweite = true;
            }
            else
            {
                filename = LoosePath.Combine(exporter.ExportFolder, filename);
                filename = PathUtility.CreateUniquePath(filename);
                isOverweite = false;
            }

            exporter.Export(filename, isOverweite);

            var toast = new Toast(string.Format(Resources.DialogExportImageSuccess, filename));
            ToastService.Current.Show(toast);
        }

    }
}
