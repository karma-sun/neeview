using NeeLaboratory.ComponentModel;
using NeeView.Threading;
using System;
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
                DriveIconUtility.CreateDriveIconAsync(_path, 256.0,
                    image =>
                    {
                        _bitmapSource = image;
                        RaisePropertyChanged("");
                    });
            }

            return _bitmapSource ?? FileIconCollection.Current.CreateDefaultFolderIcon(256.0);
        }
    }

    public static class DriveIconUtility
    {
        /// <summary>
        /// 非同期のドライブアイコン画像生成
        /// </summary>
        /// <param name="path">ドライブパス</param>
        /// <param name="width">アイコンサイズ</param>
        /// <param name="callback">画像生成後のコールバック</param>
        public static void CreateDriveIconAsync(string path, double width, Action<ImageSource> callback)
        {
            var task = new Task(async () =>
            {
                for (int i = 0; i < 2; ++i) // retry 2 time.
                {
                    try
                    {
                        var bitmapSource = FileIconCollection.Current.CreateFileIcon(path, IO.FileIconType.Drive, width, true, false);
                        if (bitmapSource != null)
                        {
                            bitmapSource?.Freeze();
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
    }
}
