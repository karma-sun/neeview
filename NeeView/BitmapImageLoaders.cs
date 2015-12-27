using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public interface IBitmapLoader
    {
        BitmapSource Load(Stream stream, string fileName);
    }

    public enum BitmapLoaderType
    {
        None,
        Default,
        Susie,
    }

    public class BitmapLoaderManager
    {
        Dictionary<BitmapLoaderType, string[]> _SupprtedFileTypes = new Dictionary<BitmapLoaderType, string[]>()
        {
            [BitmapLoaderType.Default] = new string[] { ".bmp", ".dib", ".jpg", ".jpeg", ".jpe", ".jfif", ".gif", ".tif", ".tiff", ".png", ".ico" },
            [BitmapLoaderType.Susie] = new string[] { },
        };

        Dictionary<BitmapLoaderType, List<BitmapLoaderType>> _OrderList = new Dictionary<BitmapLoaderType, List<BitmapLoaderType>>()
        {
            [BitmapLoaderType.Default] = new List<BitmapLoaderType>()
            {
                BitmapLoaderType.Default,
                BitmapLoaderType.Susie,
            },
            [BitmapLoaderType.Susie] = new List<BitmapLoaderType>()
            {
                BitmapLoaderType.Susie,
                BitmapLoaderType.Default,
            },
        };

        public BitmapLoaderType OrderType { set; get; } = BitmapLoaderType.Default;


        public bool IsSupported(string fileName)
        {
            return GetSupportedType(fileName) != BitmapLoaderType.None;
        }

        public BitmapLoaderType GetSupportedType(string fileName)
        {
            string ext = LoosePath.GetExtension(fileName);

            foreach (var type in _OrderList[OrderType])
            {
                if (_SupprtedFileTypes[type].Contains(ext))
                {
                    return type;
                }
            }
            return BitmapLoaderType.None;
        }

        public void UpdateSusieSupprtedFileTypes(Susie.Susie susie)
        {
            var list = new List<string>();
            foreach (var plugin in susie.INPlgunList)
            {
                foreach (var supportType in plugin.SupportFileTypeList)
                {
                    foreach (var filter in supportType.Extension.Split(';'))
                    {
                        list.Add(filter.TrimStart('*').ToLower());
                    }
                }
            }
            _SupprtedFileTypes[BitmapLoaderType.Susie] = list.Distinct().ToArray();
        }

        public static IBitmapLoader Create(BitmapLoaderType type)
        {
            switch (type)
            {
                case BitmapLoaderType.Default:
                    return new DefaultBitmapLoader();

                case BitmapLoaderType.Susie:
                    return new SusieBitmapLoader();

                default:
                    throw new ArgumentException("no support BitmapLoaderType.", nameof(type));
            }
        }
    }



    public static class BitmapLoaderFactory
    {
        public static IBitmapLoader Create(BitmapLoaderType type)
        {
            switch (type)
            {
                case BitmapLoaderType.Default:
                    return new DefaultBitmapLoader();

                case BitmapLoaderType.Susie:
                    return new SusieBitmapLoader();

                default:
                    throw new ArgumentException("no support BitmapLoaderType.", nameof(type));
            }
        }
    }


    public class DefaultBitmapLoader : IBitmapLoader
    {
        public BitmapSource Load(Stream stream, string fileName)
        {
            BitmapImage bmpImage = new BitmapImage();

            bmpImage.BeginInit();
            bmpImage.CacheOption = BitmapCacheOption.OnLoad;
            bmpImage.StreamSource = stream;
            bmpImage.EndInit();
            bmpImage.Freeze();

            return bmpImage;
        }
    }


    public class SusieBitmapLoader : IBitmapLoader
    {
        public static bool IsEnable;

        public BitmapSource Load(Stream stream, string fileName)
        {
            if (!IsEnable) return null;

            byte[] buff;
            if (stream is MemoryStream)
            {
                buff = ((MemoryStream)stream).GetBuffer();
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    buff = ms.GetBuffer();
                }
            }
            return ModelContext.Susie.GetPicture(fileName, buff); // ファイル名は識別用
        }
    }
}
