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
        Default,
        Susie,
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
            return Book.Susie.GetPicture(fileName, buff); // ファイル名は識別用
        }
    }
}
