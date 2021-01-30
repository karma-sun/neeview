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
        private MainViewComponent _viewComponent;
        private KeyPressWatcher _keyPressWatcher;
        private bool _isResizingWindow;
        private bool _isUpdateContentSize;
        private bool _isUpdateContentViewBox;
        private bool _isRequested;
        private bool _isBusy;
        private bool _isKeyUpChance;



        #region Constructors

        public ContentRebuild(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;

            // コンテンツ変更監視
            _viewComponent.ContentCanvas.ContentChanged += (s, e) => Request();
            _viewComponent.ContentCanvas.ContentSizeChanged += (s, e) => Request();

            // DPI変化に追従
            _viewComponent.MainView.DpiProvider.DpiChanged += (s, e) => RequestWithResize();

            // スケール変化に追従
            _viewComponent.DragTransform.AddPropertyChanged(nameof(DragTransform.Scale), (s, e) => Request());

            // ルーペ状態に追従
            _viewComponent.LoupeTransform.AddPropertyChanged(nameof(LoupeTransform.FixedScale), (s, e) => Request());

            // リサイズフィルター状態監視
            ////Config.Current.ImageResizeFilter.AddPropertyChanged(nameof(ImageResizeFilterConfig.IsEnabled), (s, e) => Request());
            Config.Current.ImageResizeFilter.PropertyChanged += (s, e) => Request();

            // ドット表示監視
            Config.Current.ImageDotKeep.AddPropertyChanged(nameof(ImageDotKeepConfig.IsEnabled), (s, e) => Request());

            // サイズ指定状態監視
            Config.Current.ImageCustomSize.PropertyChanged += (s, e) => RequestWithResize();
            Config.Current.ImageTrim.PropertyChanged += (s, e) => RequestWithTrim();

            WindowMessage.Current.EnterSizeMove += (s, e) => _isResizingWindow = true;
            WindowMessage.Current.ExitSizeMove += (s, e) => _isResizingWindow = false;

            _keyPressWatcher = new KeyPressWatcher(MainWindow.Current);
            _keyPressWatcher.PreviewKeyDown += (s, e) => _isKeyUpChance = false;
            _keyPressWatcher.PreviewKeyUp += (s, e) => _isKeyUpChance = true;

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
                _viewComponent.ContentCanvas.UpdateContentSize();
                _viewComponent.DragTransformControl.SnapView();
            }

            // トリミングによる更新
            if (_isUpdateContentViewBox)
            {
                _isUpdateContentSize = false;
                foreach (var viewConent in _viewComponent.ContentCanvas.CloneContents.Where(e => e.IsValid))
                {
                    viewConent.UpdateViewBox();
                }
            }

            if (!_isKeyUpChance && _keyPressWatcher.IsPressed) return;
            _isKeyUpChance = false;

            var mouseButtonBits = MouseButtonBitsExtensions.Create();
            if (_viewComponent.IsLoupeMode && Config.Current.Mouse.LongButtonDownMode == LongButtonDownMode.Loupe)
            {
                mouseButtonBits = MouseButtonBits.None;
            }
            if (mouseButtonBits != MouseButtonBits.None) return;

            bool isSuccessed = true;
            var dpiScaleX = _viewComponent.MainView.DpiProvider.DpiScale.DpiScaleX;
            var scale = _viewComponent.DragTransform.Scale * _viewComponent.LoupeTransform.FixedScale * dpiScaleX;
            foreach (var viewConent in _viewComponent.ContentCanvas.CloneContents.Where(e => e.IsValid))
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

        // トリミング更新要求
        public void RequestWithTrim()
        {
            _isUpdateContentSize = true;
            _isUpdateContentViewBox = true;
            this.IsRequested = true;
        }

        public void UpdateStatus()
        {
            this.IsBusy = _viewComponent.ContentCanvas.CloneContents.Where(e => e.IsValid).Any(e => e.IsResizing);
        }

        private void Start()
        {
            CompositionTarget.Rendering += OnRendering;
        }

        private void Stop()
        {
            CompositionTarget.Rendering -= OnRendering;
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
