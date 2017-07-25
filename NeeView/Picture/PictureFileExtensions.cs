using System.Collections.Generic;

namespace NeeView
{
    //
    public class PictureFileExtensions
    {
        // singleton
        //private static PictureFileExtensions _current;
        //public static PictureFileExtensions Current => _current = _current ?? new PictureFileExtensions();

        //
        public List<string> DefaultExtensoins { get; set; }

        //
        public List<string> SusieExtensions { get; set; }


        public PictureFileExtensions()
        {
            UpdateDefaultSupprtedFileTypes();

            this.SusieExtensions = new List<string>();
        }

        // デフォルトローダーのサポート拡張子を更新
        private void UpdateDefaultSupprtedFileTypes()
        {
            var list = new List<string>();

            foreach (var pair in DefaultBitmapLoader.GetExtensions())
            {
                list.AddRange(pair.Value.Split(','));
            }

            DefaultExtensoins = list;
        }

        /*
        // Susieローダーのサポート拡張子を更新
        // TODO: 引数を直接拡張子のリストにする
        public void UpdateSusieSupprtedFileTypes(Susie.Susie susie)
        {
            var list = new List<string>();
            foreach (var plugin in susie.INPlgunList)
            {
                if (plugin.IsEnable)
                {
                    list.AddRange(plugin.Extensions);
                }
            }
            SusieExtensions = list.Distinct().ToList();
        }
        */


        // サポートしている拡張子か
        public bool IsSupported(string fileName)
        {
            string ext = LoosePath.GetExtension(fileName);

            if (this.DefaultExtensoins.Contains(ext)) return true;

            if (PictureProfile.Current.IsSusieEnabled)
            {
                if (this.SusieExtensions.Contains(ext)) return true;
            }

            return false;
        }
    }
}
