using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// リサイズによるコンテンツの再作成管理
    /// </summary>
    public class ContentRebuild : BindableBase, IDisposable
    {
        static ContentRebuild() => Current = new ContentRebuild();
        public static ContentRebuild Current { get; }

        #region Fields

        private KeyPressWatcher _keyPressWatcher;
        private bool _isResizingWindow;
        private bool _isUpdateContentSize;
        private bool _isRequested;
        private bool _isBusy;

        #endregion

        #region Constructors

        private ContentRebuild()
        {
            // コンテンツ変更監視
            ContentCanvas.Current.ContentChanged += (s, e) => Request();

            // スケール変化に追従
            DragTransform.Current.AddPropertyChanged(nameof(DragTransform.Scale), (s, e) => Request());

            // ルーペ状態に追従
            LoupeTransform.Current.AddPropertyChanged(nameof(LoupeTransform.FixedScale), (s, e) => Request());

            // リサイズフィルター状態監視
            PictureProfile.Current.AddPropertyChanged(nameof(PictureProfile.IsResizeFilterEnabled), (s, e) => Request());
            ImageFilter.Current.PropertyChanged += (s, e) => Request();

            // ドット表示監視
            ContentCanvas.Current.AddPropertyChanged(nameof(ContentCanvas.IsEnabledNearestNeighbor), (s, e) => Request());

            // サイズ指定状態監視
            PictureProfile.Current.CustomSize.PropertyChanged += (s, e) => RequestWithResize();

            WindowMessage.Current.EnterSizeMove += (s, e) => _isResizingWindow = true;
            WindowMessage.Current.ExitSizeMove += (s, e) => _isResizingWindow = false;

            _keyPressWatcher = new KeyPressWatcher(MainWindow.Current);

            Start();

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// 更新を停止させるために使用する
        /// </summary>
        public Locker Locker { get; } = new Locker();

        public bool IsRequested
        {
            get { return _isRequested; }
            set { if (_isRequested != value) { _isRequested = value; RaisePropertyChanged(); } }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { if (_isBusy != value) { _isBusy = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Medhots

        /// <summary>
        /// フレーム処理
        /// 必要ならば現在の表示サイズでコンテンツを再作成する
        /// </summary>
        private void OnRendering(object sender, EventArgs e)
        {
            RebuildFrame();
        }

        private void RebuildFrame()
        {
            if (!_isRequested || _isResizingWindow || Locker.IsLocked) return;

            // サイズ指定による更新
            if (_isUpdateContentSize)
            {
                _isUpdateContentSize = false;
                ContentCanvas.Current.UpdateContentSize();
                DragTransformControl.Current.SnapView();
            }

            var mouseButtonBits = MouseButtonBitsExtensions.Create();
            if (MouseInput.Current.IsLoupeMode && MouseInput.Current.Normal.LongButtonDownMode == LongButtonDownMode.Loupe)
            {
                mouseButtonBits = MouseButtonBits.None;
            }
            if (mouseButtonBits != MouseButtonBits.None) return;

            if (_keyPressWatcher.IsPressed) return;

            bool isSuccessed = true;
            var dpiScaleX = Environment.RawDpi.DpiScaleX;
            var scale = DragTransform.Current.Scale * LoupeTransform.Current.FixedScale * dpiScaleX;
            foreach (var viewConent in ContentCanvas.Current.CloneContents.Where(e => e.IsValid))
            {
                isSuccessed = viewConent.Rebuild(scale) && isSuccessed;
            }

            this.IsRequested = !isSuccessed;

            UpdateStatus();
        }

        // 更新要求
        public void Request()
        {
            this.IsRequested = true;
        }

        // リサイズ更新要求
        public void RequestWithResize()
        {
            _isUpdateContentSize = true;
            this.IsRequested = true;
        }

        public void UpdateStatus()
        {
            this.IsBusy = ContentCanvas.Current.CloneContents.Where(e => e.IsValid).Any(e => e.IsResizing);
        }

        private void Start()
        {
            MainWindowModel.Current.Rendering += OnRendering;
        }

        private void Stop()
        {
            MainWindowModel.Current.Rendering -= OnRendering;
        }
        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Stop();
                    _keyPressWatcher.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    public class Locker
    {
        public class Key : IDisposable
        {
            public Locker Locker { get; set; }

            public Key(Locker locker)
            {
                this.Locker = locker;
            }

            #region IDisposable Support
            private bool _disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                        this.Locker?.Unlock(this);
                    }

                    _disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }
            #endregion
        }

        private List<Key> _keys = new List<Key>();

        public bool IsLocked => _keys.Any();

        public Key Lock()
        {
            var key = new Key(this);
            _keys.Add(key);
            return key;
        }

        public void Unlock(Key key)
        {
            if (key.Locker == this)
            {
                _keys.Remove(key);
                key.Locker = null;
            }
        }
    }


}
