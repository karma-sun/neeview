// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// MediaPlayer コンテンツ
    /// </summary>
    public class MediaContent : BitmapContent
    {
        public override bool IsLoaded => FileProxy != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entry"></param>
        public MediaContent(ArchiveEntry entry) : base(entry)
        {
        }


#pragma warning disable CS1998
        /// <summary>
        /// コンテンツロード.
        /// 動画情報の収集。サムネイル用に画像を読込つつ再生用にテンポラリファイル作成
        /// </summary>
        public override async Task LoadAsync(CancellationToken token)
        {
            if (IsLoaded) return;

            bool isThumbnail = !Thumbnail.IsValid;

            // 画像情報の取得
            var loader = new MediaInfoLoader(Entry.FullPath);
            var info = loader.GetMediaInfo(isThumbnail);
            this.Size = info.Size;

            this.Picture = new Picture(Entry);
            this.Picture.PictureInfo.OriginalSize = info.Size;
            this.Picture.PictureInfo.Size = info.Size;
            this.Picture.PictureInfo.BitsPerPixel = 32;

            if (!token.IsCancellationRequested)
            {
                // TempFileに出力し、これをMediaPlayerに再生させる
                CreateTempFile(true);

                RaiseLoaded();
                RaiseChanged();
            }

            // サムネイル作成
            if (Thumbnail.IsValid || info.Thumbnail == null) return;
            Thumbnail.Initialize(info.Thumbnail);
        }
    }
#pragma warning restore 

    //
    public class MediaInfo
    {
        public bool HasVideo { get; set; }
        public bool HasAudio { get; set; }
        public Size Size { get; set; }
        public double Width => Size.Width;
        public double Height => Size.Height;
        public Duration Duration { get; set; }
        public byte[] Thumbnail { get; set; }
    }

    //
    public class MediaInfoLoader
    {
        private string _fileName;
        private MediaInfo _info = new MediaInfo();
        private MediaPlayer _player;
        private volatile bool _wait;
        private volatile Exception _exception;

        public MediaInfoLoader(string fileName)
        {
            _fileName = fileName;
        }

        public async Task<MediaInfo> GetMediaInfoAsync(bool isThumbnail)
        {
            return await Task.Run(() =>
            {
                return GetMediaInfo(isThumbnail);
            });
        }

        public MediaInfo GetMediaInfo(bool isThumbnail)
        {
            _exception = null;
            _wait = true;

            _player = new MediaPlayer();
            _player.IsMuted = true;
            _player.ScrubbingEnabled = true;

            _player.MediaOpened += Player_MediaOpened;
            _player.MediaFailed += Player_MediaFailed;

            try
            {
                _player.Open(new Uri(_fileName));
                _player.Pause();

                if (isThumbnail)
                {
                    _player.Position = TimeSpan.FromSeconds(5); // for thumbnail position
                }

                while (_wait)
                {
                    ////Debug.WriteLine($"Download: {_player.Position} {_player.DownloadProgress} {_player.NaturalVideoWidth}");
                    DoEvents();
                    Thread.Sleep(50);
                }

                if (_exception != null) throw _exception;

                _info.HasVideo = _player.HasVideo;
                _info.HasAudio = _player.HasAudio;
                _info.Size = new Size(_player.NaturalVideoWidth, _player.NaturalVideoHeight);
                _info.Duration = _player.NaturalDuration;

                if (isThumbnail)
                {
                    var size = ThumbnailProfile.Current.GetThumbnailSize(_info.Size);
                    var visual = new DrawingVisual();
                    using (var context = visual.RenderOpen())
                    {
                        context.DrawVideo(_player, new Rect(0, 0, size.Width, size.Height));
                    }
                    var bitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(visual);
                    bitmap.Freeze();

                    using (var ms = new MemoryStream())
                    {
                        var encoder = DefaultBitmapFactory.CreateEncoder(ThumbnailProfile.Current.Format, ThumbnailProfile.Current.Quality);
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        encoder.Save(ms);

                        _info.Thumbnail = ms.ToArray();
                    }
                }

                return _info;
            }
            finally
            {
                _player.Close();
                _player = null;
            }
        }

        private void Player_MediaOpened(object sender, EventArgs e)
        {
            _wait = false;
        }

        private void Player_MediaFailed(object sender, ExceptionEventArgs e)
        {
            _exception = e.ErrorException;
            _wait = false;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        private object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }
    }

}
