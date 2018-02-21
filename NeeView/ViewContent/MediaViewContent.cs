// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NeeView
{
    public class MediaViewContent : BitmapViewContent
    {
        #region Fields

        private MediaPlayer _player;
        private VideoDrawing _videoDrawing;
        private TextBlock _errorMessageTextBlock;

        #endregion

        #region Constructors

        public MediaViewContent(ViewPage source, ViewContent old) : base(source, old)
        {
        }

        #endregion

        #region Events

        /// <summary>
        /// コントロールがUnloadされたときのイベント
        /// </summary>
        public event EventHandler Unloaded;

        #endregion

        #region Properties

        public MediaPlayer MediaPlayer => _player;

        public Uri MediaUri => new Uri(((MediaContent)Content).FileProxy.Path);

        public bool IsLastStart => this.Source.IsLastStart;

        #endregion

        #region Methods

        //
        public new void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = CreateView(this.Source, parameter);

            // content setting
            var animatedContent = this.Content as MediaContent;
            this.Color = animatedContent.Color;
            this.FileProxy = animatedContent.FileProxy;
        }


        /// <summary>
        /// アニメーションビュー生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private new FrameworkElement CreateView(ViewPage source, ViewContentParameters parameter)
        {
            _player = new MediaPlayer();

            _videoDrawing = new VideoDrawing()
            {
                Player = _player,
                Rect = new Rect(this.Content.Size),

            };
            var brush = new DrawingBrush()
            {
                Drawing = _videoDrawing,
                Stretch = Stretch.Fill,
                Viewbox = source.GetViewBox()
            };
            var rectangle = new Rectangle()
            {
                Fill = brush
            };

            _errorMessageTextBlock = new TextBlock()
            {
                Background = Brushes.Black,
                Foreground = Brushes.White,
                Padding = new Thickness(40, 20, 40, 20),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 20,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed,
            };

            var grid = new Grid();
            grid.Children.Add(rectangle);
            grid.Children.Add(_errorMessageTextBlock);

            //
            _player.MediaFailed += Media_MediaFailed;
            _player.MediaOpened += Media_MediaOpened;
            _player.MediaEnded += (s, e_) => _player.Position = TimeSpan.FromMilliseconds(1);

            grid.Unloaded += Grid_Unloaded;

            //// 再生開始はMediaControlViewで行うので、ここでは実行しない
            ////_player.Open(new Uri(((MediaContent)Content).FileProxy.Path));
            ////_player.Play();

            return grid;
        }

        //
        private void CloseMediaPlayer()
        {
            Unloaded?.Invoke(this, null);

            _player?.Stop();
            _player?.Close();
        }

        //
        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            CloseMediaPlayer();
        }

        //
        private void Media_MediaOpened(object sender, EventArgs e)
        {
            var content = this.Content as MediaContent;
            if (content == null) return;

            var size = new Size(_player.NaturalVideoWidth, _player.NaturalVideoHeight);
            content.SetSize(size);
            _videoDrawing.Rect = new Rect(size);

            ContentCanvas.Current.UpdateContentSize();
            FileInformation.Current.Flush();
        }

        //
        private void Media_MediaFailed(object sender, ExceptionEventArgs e)
        {
            _errorMessageTextBlock.Text = e.ErrorException != null ? e.ErrorException.Message : "再生エラー";
            _errorMessageTextBlock.Visibility = Visibility.Visible;
        }


        //
        public override bool Rebuild(double scale)
        {
            return true;
        }

        #endregion

        #region IDisposable Support

        protected override void Dispose(bool disposing)
        {
            CloseMediaPlayer();
            base.Dispose(disposing);
        }

        #endregion

        #region Static Methods

        public new static MediaViewContent Create(ViewPage source, ViewContent oldViewContent)
        {
            var viewContent = new MediaViewContent(source, oldViewContent);
            viewContent.Initialize();
            return viewContent;
        }

        #endregion
    }


}
