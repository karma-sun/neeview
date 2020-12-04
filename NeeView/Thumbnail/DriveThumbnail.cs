using NeeLaboratory.ComponentModel;
using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// ドライブサムネイル
    /// </summary>
    public class DriveThumbnail : BindableBase, IThumbnail
    {
        private string _path;
        private ImageSource _bitmapSource;
        private bool _initialized;

        public DriveThumbnail(string path)
        {
            _path = path;
        }

        public ImageSource ImageSource => CreateBitmap();
        public double Width => ImageSource is BitmapSource bitmap ? bitmap.PixelWidth : 0;
        public double Height => ImageSource is BitmapSource bitmap ? bitmap.PixelHeight : 0;
        public bool IsUniqueImage => true;
        public bool IsNormalImage => false;
        public Brush Background => Brushes.Transparent;

        private ImageSource CreateBitmap()
        {
            if (!_initialized)
            {
                _initialized = true;
                DriveIconUtility.CreateDriveIconAsync(_path,
                    image =>
                    {
                        _bitmapSource = image.GetBitmapSource(256.0);
                        DriveIconUtility.SetDriveIconCache(_path, _bitmapSource);
                        RaisePropertyChanged("");
                    });
            }

            return DriveIconUtility.GetDriveIconCache(_path);
        }


    }

    public static class DriveIconUtility
    {
        private static Dictionary<string, ImageSource> _iconCache = new Dictionary<string, ImageSource>();

        /// <summary>
        /// 非同期のドライブアイコン画像生成
        /// </summary>
        /// <param name="path">ドライブパス</param>
        /// <param name="callback">画像生成後のコールバック</param>
        public static void CreateDriveIconAsync(string path, Action<BitmapSourceCollection> callback)
        {
            var task = new Task(async () =>
            {
                for (int i = 0; i < 2; ++i) // retry 2 time.
                {
                    try
                    {
                        var bitmapSource = FileIconCollection.Current.CreateFileIcon(path, IO.FileIconType.Drive, true, false);
                        if (bitmapSource != null)
                        {
                            bitmapSource.Freeze();
                            callback?.Invoke(bitmapSource);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"CreateDriveIcon({path}): " + ex.Message);
                    }
                    await Task.Delay(500);
                }
            });

            task.Start(SingleThreadedApartment.TaskScheduler); // STA
        }


        public static void SetDriveIconCache(string path, ImageSource bitmapSource)
        {
            _iconCache[path] = bitmapSource;
        }

        public static ImageSource GetDriveIconCache(string path)
        {
            if (_iconCache.TryGetValue(path, out ImageSource bitmapSource))
            {
                return bitmapSource;
            }
            else
            {
                return FileIconCollection.Current.CreateDefaultFolderIcon().GetBitmapSource(256.0);
            }
        }
    }
}
