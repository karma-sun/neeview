using NeeView.Properties;
using System;

namespace NeeView
{
    public class ExportDataPresenter
    {
        static ExportDataPresenter() => Current = new ExportDataPresenter();
        public static ExportDataPresenter Current { get; }

        private ExportDataPresenter()
        {
        }


        private ExportDataDialogService _dialogService = new ExportDataDialogService();


        public void Export(ExportBackupCommandParameter parameter)
        {
            var fileName = parameter.FileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                var dialogParam = new SaveExportDataDialogParameter();
                if (_dialogService.ShowDialog("SaveExportDataDialog", dialogParam) != true) return;
                fileName = dialogParam.FileName;
            }

            try
            {
                SaveDataSync.Current.Flush();
                var exporter = new Exporter();
                exporter.Export(fileName);
            }
            catch (Exception ex)
            {
                new MessageDialog($"{Resources.Word_Cause}: {ex.Message}", Resources.ExportErrorDialog_Title).ShowDialog();
            }
        }


        public void Import(ImportBackupCommandParameter parameter)
        {
            var param = (ImportBackupCommandParameter)parameter.Clone();
            if (string.IsNullOrWhiteSpace(param.FileName) || !System.IO.File.Exists(param.FileName))
            {
                var dialogParam = new OpenExportDataDialogParameter();
                if (_dialogService.ShowDialog("OpenExportDataDialog", dialogParam) != true) return;
                param.FileName = dialogParam.FileName;
            }

            using (var importer = new Importer(param))
            {
                if (!param.IsImportActionValid())
                {
                    if (_dialogService.ShowDialog("ImportDialog", importer) != true) return;
                }

                try
                {
                    importer.Import();
                }
                catch (Exception ex)
                {
                    new MessageDialog($"{Resources.Word_Cause}: {ex.Message}", Resources.ImportErrorDialog_Title).ShowDialog();
                }
            }
        }

    }
}
