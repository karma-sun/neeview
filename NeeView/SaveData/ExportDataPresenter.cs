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


        public void Export()
        {
            var param = new SaveExportDataDialogParameter();
            if (_dialogService.ShowDialog("SaveExportDataDialog", param) == true)
            {
                try
                {
                    SaveDataSync.Current.Flush();

                    var exporter = new Exporter();
                    exporter.Export(param.FileName);
                }
                catch (Exception ex)
                {
                    new MessageDialog($"{Resources.Word_Cause}: {ex.Message}", Resources.ExportErrorDialog_Title).ShowDialog();
                }
            }
        }

        public void Import()
        {
            var param = new OpenExportDataDialogParameter();
            if (_dialogService.ShowDialog("OpenExportDataDialog", param) == true)
            {
                using (var importer = new Importer(param.FileName))
                {
                    if (_dialogService.ShowDialog("ImportDialog", importer) == true)
                    {
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

    }
}
