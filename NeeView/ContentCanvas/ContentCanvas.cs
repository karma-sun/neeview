using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.Media.Imaging;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace NeeView
{
    /// <summary>
    /// 回転の初期化モード
    /// </summary>
    public enum AngleResetMode
    {
        /// <summary>
        /// 現在の角度を維持
        /// </summary>
        None,

        /// <summary>
        /// 通常。AutoRotateするかを判定し角度を求める
        /// </summary>
        Normal,

        /// <summary>
        /// AutoRotateの角度を強制適用する
        /// </summary>
        ForceAutoRotate,
    }

    // 自動回転タイプ
    public enum AutoRotateType
    {
        [AliasName("@EnumAutoRotateNone")]
        None,

        [AliasName("@EnumAutoRotateLeft")]
        Left,

        [AliasName("@EnumAutoRotateRight")]
        Right,
    }

    public static class AutoRotateTypeExtensions
    {
        public static double ToAngle(this AutoRotateType self)
        {
            switch (self)
            {
                default:
                case AutoRotateType.None:
                    return 0.0;
                case AutoRotateType.Left:
                    return -90.0;
                case AutoRotateType.Right:
                    return 90.0;
            }
        }
    }

    /// <summary>
    /// ページ表示コンテンツ管理
    /// </summary>
    public class ContentCanvas : BindableBase, IDisposable
    {
        static ContentCanvas() => Current = new ContentCanvas();
        public static ContentCanvas Current { get; }

        #region Fields

        // コンテンツサイズ計算機
        private ContentSizeCalcurator _contentSizeCalcurator;

        private DragTransform _dragTransform;
        private DragTransformControl _dragTransformControl;

        private PageStretchMode _stretchModePrev = PageStretchMode.Uniform;

        private object _lock = new object();

        #endregion

        #region Constructors

        private ContentCanvas()
        {
            _contentSizeCalcurator = new ContentSizeCalcurator(this);

            _dragTransform = DragTransform.Current;
            _dragTransformControl = DragTransformControl.Current;

            DragTransform.Current.TransformChanged += Transform_TransformChanged;
            LoupeTransform.Current.TransformChanged += Transform_TransformChanged;

            // Contents
            Contents = new ObservableCollection<ViewContent>();
            Contents.Add(new ViewContent());
            Contents.Add(new ViewContent());

            MainContent = Contents[0];

            BookHub.Current.BookChanging +=
                (s, e) => IgnoreViewContentsReservers();

            // TODO: BookOperationから？
            BookHub.Current.ViewContentsChanged +=
                OnViewContentsChanged;
            BookHub.Current.NextContentsChanged +=
                OnNextContentsChanged;

            BookHub.Current.EmptyMessage +=
                (s, e) => EmptyPageMessage = e.Message;

            Config.Current.ImageDotKeep.AddPropertyChanged(nameof(ImageDotKeepConfig.IsEnabled), (s, e) =>
            {
                UpdateContentScalingMode();
            });

            Config.Current.Book.AddPropertyChanged(nameof(BookConfig.ContentsSpace), (s, e) =>
            {
                UpdateContentSize(); ;
            });

            Config.Current.View.PropertyChanging += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ViewConfig.StretchMode):
                        _stretchModePrev = Config.Current.View.StretchMode;
                        break;
                }
            };

            Config.Current.View.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ViewConfig.StretchMode):
                        Stretch();
                        break;

                    case nameof(ViewConfig.AllowStretchScaleUp):
                    case nameof(ViewConfig.AllowStretchScaleDown):
                    case nameof(ViewConfig.IsBaseScaleEnabled):
                    case nameof(ViewConfig.BaseScale):
                        ResetContentSize();
                        break;

                    case nameof(ViewConfig.AutoRotate):
                        RaisePropertyChanged(nameof(IsAutoRotateLeft));
                        RaisePropertyChanged(nameof(IsAutoRotateRight));
                        ResetContentSizeAndTransform();
                        break;
                }
            };
        }

        #endregion

        #region Events

        //
        public event EventHandler ContentChanged;

        #endregion

        #region Properties

        // 空フォルダー通知表示のON/OFF
        private bool _isVisibleEmptyPageMessage = false;
        public bool IsVisibleEmptyPageMessage
        {
            get { return _isVisibleEmptyPageMessage; }
            set { SetProperty(ref _isVisibleEmptyPageMessage, value && Config.Current.Notice.IsEmptyMessageEnabled); }
        }

        // 空フォルダー通知表示の詳細テキスト
        private string _emptyPageMessage;
        public string EmptyPageMessage
        {
            get { return _emptyPageMessage; }
            set { _emptyPageMessage = value; RaisePropertyChanged(); }
        }

        public bool IsAutoRotateLeft
        {
            get { return Config.Current.View.AutoRotate == AutoRotateType.Left; }
            set
            {
                if (value)
                {
                    Config.Current.View.AutoRotate = AutoRotateType.Left;
                }
                else if (Config.Current.View.AutoRotate == AutoRotateType.Left)
                {
                    Config.Current.View.AutoRotate = AutoRotateType.None;
                }
            }
        }

        public bool IsAutoRotateRight
        {
            get { return Config.Current.View.AutoRotate == AutoRotateType.Right; }
            set
            {
                if (value)
                {
                    Config.Current.View.AutoRotate = AutoRotateType.Right;
                }
                else if (Config.Current.View.AutoRotate == AutoRotateType.Right)
                {
                    Config.Current.View.AutoRotate = AutoRotateType.None;
                }
            }
        }

        // ビューエリアサイズ
        public Size ViewSize { get; private set; }

        // コンテンツ
        public ObservableCollection<ViewContent> Contents { get; private set; }

        // コンテンツ複製。処理時のコレクション変更の例外を避けるため。
        public List<ViewContent> CloneContents
        {
            get
            {
                lock (_lock)
                {
                    return Contents.ToList();
                }
            }
        }

        // 見開き時のメインとなるコンテンツ
        private ViewContent _mainContent;
        public ViewContent MainContent
        {
            get { return _mainContent; }
            set
            {
                if (_mainContent != value)
                {
                    _mainContent = value;
                    RaisePropertyChanged();

                    this.IsMediaContent = _mainContent is MediaViewContent;
                }
            }
        }

        // メインコンテンツがメディアコンテンツ？
        private bool _isMediaContent;
        public bool IsMediaContent
        {
            get { return _isMediaContent; }
            set { if (_isMediaContent != value) { _isMediaContent = value; RaisePropertyChanged(); } }
        }


        // コンテンツマージン
        private Thickness _contentsMargin;
        public Thickness ContentsMargin
        {
            get { return _contentsMargin; }
            set { _contentsMargin = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 次のページ更新時の表示開始位置
        /// TODO: ちゃんとBookから情報として上げるようにするべき
        /// </summary>
        public DragViewOrigin NextViewOrigin { get; set; }

        /// <summary>
        /// ContentAngle property.
        /// </summary>
        private double _contentAngle;
        public double ContentAngle
        {
            get { return _contentAngle; }
            set { if (_contentAngle != value) { _contentAngle = value; RaisePropertyChanged(); } }
        }

        // メインコンテンツのオリジナル表示スケール
        public double MainContentScale => MainContent != null ? MainContent.Scale * Environment.Dpi.DpiScaleX : 0.0;

        //
        public GridLine GridLine { get; private set; } = new GridLine();

        #endregion

        #region Methods

        /// <summary>
        /// 角度設定モードを取得
        /// </summary>
        /// <param name="precedeAutoRotate">AutoRotate設定を優先する</param>
        /// <returns></returns>
        private AngleResetMode GetAngleResetMode(bool precedeAutoRotate)
        {
            if (Config.Current.View.IsKeepAngle)
            {
                if (precedeAutoRotate)
                {
                    if (Config.Current.View.AutoRotate != AutoRotateType.None)
                    {
                        return AngleResetMode.ForceAutoRotate;
                    }
                    else
                    {
                        return AngleResetMode.Normal;
                    }
                }
                else
                {
                    return AngleResetMode.None;
                }
            }
            else
            {
                return AngleResetMode.Normal;
            }
        }


        // トランスフォーム変更イベント処理
        private void Transform_TransformChanged(object sender, TransformEventArgs e)
        {
            UpdateContentScalingMode();
            MouseInput.Current.ShowMessage(e.ActionType, MainContent);
        }

        // コンテンツカラー
        public Color GetContentColor()
        {
            return Contents[Contents[1].IsValid ? 1 : 0].Color;
        }

        // 現在のビューコンテンツのリザーバーを無効化
        private void IgnoreViewContentsReservers()
        {
            foreach (var content in CloneContents)
            {
                content.IgnoreReserver = true;
            }
        }

        /// <summary>
        /// 表示コンテンツ更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewContentsChanged(object sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            var contents = new List<ViewContent>();

            // ViewContent作成
            if (e?.ViewPageCollection?.Collection != null)
            {
                foreach (var source in e.ViewPageCollection.Collection)
                {
                    if (source != null)
                    {
                        var old = Contents[contents.Count];
                        var content = ViewContentFactory.Create(source, old);
                        contents.Add(content);
                    }
                }
            }

            // ページが存在しない場合、専用メッセージを表示する
            IsVisibleEmptyPageMessage = e?.ViewPageCollection != null && contents.Count == 0;

            // メインとなるコンテンツを指定
            var validContents = contents.Where(x => !x.IsDummy).ToList();
            var mainContent = validContents.Count > 0 ? (validContents.First().Position < validContents.Last().Position ? validContents.First() : validContents.Last()) : null;

            // ViewModelプロパティに反映
            lock (_lock)
            {
                MainContent = mainContent;

                for (int index = 0; index < 2; ++index)
                {
                    Contents[index] = index < contents.Count ? contents[index] : new ViewContent();
                }
            }

            // ルーペ解除
            if (Config.Current.Loupe.IsResetByPageChanged)
            {
                MouseInput.Current.IsLoupeMode = false;
            }

            if (e == null || e.IsFirst)
            {
                // コンテンツサイズ更新
                // ブック最初のページであればビューも初期化
                ResetContentSizeAndTransform();
            }
            else
            {
                // コンテンツサイズ更新
                UpdateContentSize(GetAutoRotateAngle(GetAngleResetMode(e.IsFirst || Config.Current.View.IsKeepScale)));

                // リザーブコンテンツでなければ座標初期化
                // HACK: ルーペ時の挙動があやしい
                bool isReserveContent = e?.ViewPageCollection?.Collection?.Any(x => x.GetContentType() == ViewContentType.Reserve) ?? false;
                if (!isReserveContent)
                {
                    ResetTransform(false, e != null ? e.ViewPageCollection.Range.Direction : 0, NextViewOrigin, GetAngleResetMode(e.IsFirst));
                    NextViewOrigin = DragViewOrigin.None;
                }
            }

            ContentChanged?.Invoke(this, null);

            // GC
            MemoryControl.Current.GarbageCollect();

            ////DebugTimer.Check("UpdatedContentCanvas");
        }

        // 先読みコンテンツ更新
        // 表示サイズを確定し、フィルター適用時にリサイズ処理を行う
        private void OnNextContentsChanged(object sender, ViewContentSourceCollectionChangedEventArgs source)
        {
            if (source?.ViewPageCollection?.Collection == null) return;

            bool includeLoupeScale = LoupeTransform.Current.IsEnabled && !Config.Current.Loupe.IsResetByPageChanged;
            ResizeConten(source.ViewPageCollection, includeLoupeScale, source.CancellationToken);
        }


        /// <summary>
        /// コンテンツリサイズ
        /// </summary>
        private void ResizeConten(ViewContentSourceCollection viewPageCollection, bool includeLoupeScale, CancellationToken token)
        {
            if (viewPageCollection?.Collection == null) return;

            token.ThrowIfCancellationRequested();


            var sizes = viewPageCollection.Collection.Select(e => e.Size).ToList();
            while (sizes.Count() < 2)
            {
                sizes.Add(SizeExtensions.Zero);
            }

            // 表示サイズ計算
            var result = MainContent is MediaViewContent
                ? _contentSizeCalcurator.GetFixedContentSize(sizes, 0.0)
                : _contentSizeCalcurator.GetFixedContentSize(sizes, GetAngleResetMode(false || Config.Current.View.IsKeepScale));

            // 表示スケール推定
            var scale = (Config.Current.View.IsKeepScale ? _dragTransform.Scale : 1.0) * (includeLoupeScale ? LoupeTransform.Current.FixedScale : 1.0) * Environment.RawDpi.DpiScaleX;

            // リサイズ
            for (int i = 0; i < 2; ++i)
            {
                var size0 = sizes[i];
                if (size0.IsZero()) continue;

                var size1 = result.ContentSizeList[i].Multi(scale);
                if (i < viewPageCollection.Collection.Count && viewPageCollection.Collection[i].IsHalf) // 分割前サイズでリサイズ
                {
                    size1 = new Size(size1.Width * 2.0, size1.Height);
                }
                ////Debug.WriteLine($"{i}: {size0} => {size1.Truncate()}");


                var content = viewPageCollection.Collection[i].Content;
                try
                {
                    if (content.PageMessage == null && content.CanResize && content is BitmapContent bitmapContent)
                    {
                        var dispSize = new Size(size1.Width, size1.Height);
                        var resized = bitmapContent.Picture?.CreateImageSource(bitmapContent.GetRenderSize(dispSize), token);
                        if (resized == true)
                        {
                            viewPageCollection.Collection[i].Page.DebugRaiseContentPropertyChanged();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("OnNextContentChanged: " + ex.Message);
                    content.SetPageMessage(ex);
                }
            }
        }

        /// <summary>
        /// コンテンツサイズを初期化
        /// </summary>
        public void ResetContentSize()
        {
            UpdateContentSize();
            ContentRebuild.Current.Request();
            ResetTransformRaw(true, false, false, 0.0);
        }

        /// <summary>
        /// コンテンツサイズと座標系を初期化
        /// </summary>
        public void ResetContentSizeAndTransform()
        {
            var angleResetMode = GetAngleResetMode(true);

            UpdateContentSize(GetAutoRotateAngle(angleResetMode));
            ContentRebuild.Current.Request();
            ResetTransform(true, 0, DragViewOrigin.None, angleResetMode);
        }

        // 座標系初期化
        public void ResetTransform(bool isForce, int pageDirection, DragViewOrigin viewOrigin, AngleResetMode angleResetMode)
        {
            // NOTE: ルーペモードのときは初期化しない
            if (MouseInput.Current.IsLoupeMode) return;

            _dragTransformControl.SetMouseDragSetting(pageDirection, viewOrigin, BookSettingPresenter.Current.LatestSetting.BookReadOrder);

            bool isResetScale = isForce || !Config.Current.View.IsKeepScale;
            bool isResetAngle = isForce || !Config.Current.View.IsKeepAngle || angleResetMode != AngleResetMode.None;
            bool isResetFlip = isForce || !Config.Current.View.IsKeepFlip;

            ResetTransformRaw(isResetScale, isResetAngle, isResetFlip, GetAutoRotateAngle(angleResetMode));
        }

        /// <summary>
        /// 座標系の初期化。
        /// フラグに関係なく移動は初期化される
        /// </summary>
        /// <param name="isResetScale">スケールを初期化する</param>
        /// <param name="isResetAngle">角度をangleで初期化する</param>
        /// <param name="isResetFlip">反転を初期化する</param>
        /// <param name="angle">角度初期化の値</param>
        public void ResetTransformRaw(bool isResetScale, bool isResetAngle, bool isResetFlip, double angle)
        {
            _dragTransformControl.Reset(isResetScale, isResetAngle, isResetFlip, angle);
        }

        /// <summary>
        /// ページ開始時の回転
        /// </summary>
        /// <returns></returns>
        public double GetAutoRotateAngle(AngleResetMode angleResetMode)
        {
            if (angleResetMode == AngleResetMode.None)
            {
                return DragTransform.Current.Angle;
            }
            else if (MainContent is MediaViewContent)
            {
                return 0.0;
            }
            else
            {
                return _contentSizeCalcurator.GetAutoRotateAngle(GetContentSizeList(), angleResetMode);
            }
        }

        /// <summary>
        /// 有効な表示コンテンツサイズのリストを取得
        /// </summary>
        /// <returns></returns>
        private List<Size> GetContentSizeList()
        {
            return CloneContents.Select(e => (e.Source?.Size ?? SizeExtensions.Zero).EmptyOrZeroCoalesce(GetViewContentSize(e))).ToList();
        }

        // TODO: ViewContent.Size の廃止
        private Size GetViewContentSize(ViewContent viewContent)
        {
            return viewContent.Size;
        }

        // ビューエリアサイズを更新
        public void SetViewSize(double width, double height)
        {
            this.ViewSize = new Size(width, height);

            UpdateContentSize();

            ContentRebuild.Current.Request();
        }


        //
        public void UpdateContentSize(double angle)
        {
            this.ContentAngle = angle;
            UpdateContentSize();
        }

        // コンテンツ表示サイズを更新
        public void UpdateContentSize()
        {
            if (!CloneContents.Any(e => e.IsValid)) return;

            var result = _contentSizeCalcurator.GetFixedContentSize(GetContentSizeList(), this.ContentAngle);

            this.ContentsMargin = result.ContentsMargin;

            for (int i = 0; i < 2; ++i)
            {
                if (Contents[i] is ArchiveViewContent)
                {
                    Contents[i].Width = 64;
                    Contents[i].Height = 64;
                }
                else
                {
                    Contents[i].Width = result.ContentSizeList[i].Width;
                    Contents[i].Height = result.ContentSizeList[i].Height;
                }
            }

            UpdateContentScalingMode();

            this.GridLine.SetSize(result.Width, result.Height);
        }


        // コンテンツスケーリングモードを更新
        public void UpdateContentScalingMode(ViewContent target = null)
        {
            double finalScale = _dragTransform.Scale * LoupeTransform.Current.FixedScale * Environment.RawDpi.DpiScaleX;

            foreach (var content in CloneContents)
            {
                if (target != null && target != content) continue;

                if (content.View != null && content.IsBitmapScalingModeSupported)
                {
                    var bitmapContent = content as BitmapViewContent;
                    if (bitmapContent == null) continue;

                    var image = bitmapContent.GetViewImage();
                    if (image == null) continue;

                    var pixelHeight = image.GetPixelHeight();
                    var pixelWidth = image.GetPixelWidth();
                    var viewHeight = content.Height * finalScale;
                    var viewWidth = content.Width * finalScale;

                    var diff = Math.Abs(pixelHeight - viewHeight) + Math.Abs(pixelWidth - viewWidth);
                    var diffAngle = Math.Abs(_dragTransform.Angle % 90.0);
                    if (Environment.IsDpiSquare && diff < 2.2 && diffAngle < 0.1 && !Config.Current.ImageTrim.IsEnabled)
                    {
                        content.BitmapScalingMode = BitmapScalingMode.NearestNeighbor;
                        content.SetViewMode(ContentViewMode.Pixeled, finalScale);
                    }
                    else
                    {
                        var isImageDotKeep = Config.Current.ImageDotKeep.IsImgeDotKeep(new Size(viewWidth, viewHeight), new Size(pixelWidth, pixelHeight));
                        content.BitmapScalingMode = isImageDotKeep ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality;
                        content.SetViewMode(ContentViewMode.Scale, finalScale);
                    }

                    // ##
                    DebugInfo.Current?.SetMessage($"{content.BitmapScalingMode}: s={pixelHeight}: v={viewHeight:0.00}: a={_dragTransform.Angle:0.00}");

                    if (bitmapContent.IsDarty())
                    {
                        ContentRebuild.Current.Request();
                    }
                }
            }
        }


        #region スケールモード

        // トグル
        public PageStretchMode GetToggleStretchMode(ToggleStretchModeCommandParameter param)
        {
            PageStretchMode mode = Config.Current.View.StretchMode;
            int length = Enum.GetNames(typeof(PageStretchMode)).Length;
            int count = 0;
            do
            {
                var next = (int)mode + 1;
                if (!param.IsLoop && next >= length) return Config.Current.View.StretchMode;
                mode = (PageStretchMode)(next % length);
                if (param.StretchModes[mode]) return mode;
            }
            while (count++ < length);
            return Config.Current.View.StretchMode;
        }

        // 逆トグル
        public PageStretchMode GetToggleStretchModeReverse(ToggleStretchModeCommandParameter param)
        {
            PageStretchMode mode = Config.Current.View.StretchMode;
            int length = Enum.GetNames(typeof(PageStretchMode)).Length;
            int count = 0;
            do
            {
                var prev = (int)mode - 1;
                if (!param.IsLoop && prev < 0) return Config.Current.View.StretchMode;
                mode = (PageStretchMode)((prev + length) % length);
                if (param.StretchModes[mode]) return mode;
            }
            while (count++ < length);
            return Config.Current.View.StretchMode;
        }

        public void SetStretchMode(PageStretchMode mode, bool isToggle)
        {
            Config.Current.View.StretchMode = GetFixedStretchMode(mode, isToggle);
            Stretch();
        }

        public bool TestStretchMode(PageStretchMode mode, bool isToggle)
        {
            return mode == GetFixedStretchMode(mode, isToggle);
        }

        private PageStretchMode GetFixedStretchMode(PageStretchMode mode, bool isToggle)
        {
            if (isToggle && Config.Current.View.StretchMode == mode)
            {
                return (mode == PageStretchMode.None) ? _stretchModePrev : PageStretchMode.None;
            }
            else
            {
                return mode;
            }
        }

        #endregion

        #region 回転コマンド

        public void ViewRotateLeft(ViewRotateCommandParameter parameter)
        {
            _dragTransformControl.Rotate(-parameter.Angle);

            if (parameter.IsStretch)
            {
                Stretch();
            }
        }

        public void ViewRotateRight(ViewRotateCommandParameter parameter)
        {
            _dragTransformControl.Rotate(+parameter.Angle);

            if (parameter.IsStretch)
            {
                Stretch();
            }
        }

        public void Stretch()
        {
            UpdateContentSize(_dragTransform.Angle);
            ContentRebuild.Current.Request();
            ResetTransformRaw(true, false, false, 0.0);
        }

        #endregion

        #region クリップボード関連

        private ImageSource CurrentImageSource
        {
            get { return (this.MainContent?.Content as BitmapContent)?.ImageSource; }
        }

        public bool CanCopyImageToClipboard()
        {
            return CurrentImageSource is BitmapSource;
        }

        public void CopyImageToClipboard()
        {
            try
            {
                if (CanCopyImageToClipboard() && CurrentImageSource is BitmapSource bitmapSource)
                {
                    ClipboardUtility.CopyImage(bitmapSource);
                }
            }
            catch (Exception e)
            {
                new MessageDialog($"{Resources.WordCause}: {e.Message}", Resources.DialogCopyImageErrorTitle).ShowDialog();
            }
        }

        #endregion

        #region 印刷

        /// <summary>
        /// 印刷可能判定
        /// </summary>
        /// <returns></returns>
        public bool CanPrint()
        {
            return this.MainContent != null && this.MainContent.IsValid;
        }

        /// <summary>
        /// 印刷
        /// </summary>
        public void Print(Window owner, FrameworkElement element, Transform transform, double width, double height)
        {
            if (!CanPrint()) return;

            // 掃除しておく
            GC.Collect();

            var contents = this.Contents;
            var mainContent = this.MainContent;

            // スケールモード退避
            var scaleModeMemory = contents.ToDictionary(e => e, e => e.BitmapScalingMode);

            // アニメーション停止
            foreach (var content in contents)
            {
                content.AnimationImageVisibility = Visibility.Visible;
                content.AnimationPlayerVisibility = Visibility.Collapsed;
            }

            // 読み込み停止
            BookHub.Current.IsEnabled = false;

            // スライドショー停止
            SlideShow.Current.PauseSlideShow();

            try
            {
                var context = new PrintContext();
                context.MainContent = mainContent;
                context.Contents = contents;
                context.View = element;
                context.ViewTransform = transform;
                context.ViewWidth = width;
                context.ViewHeight = height;
                context.ViewEffect = ImageEffect.Current.Effect;
                context.Background = ContentCanvasBrush.Current.CreateBackgroundBrush();
                context.BackgroundFront = ContentCanvasBrush.Current.CreateBackgroundFrontBrush(new DpiScale(1, 1));

                var dialog = new PrintWindow(context);
                dialog.Owner = owner;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.ShowDialog();
            }
            finally
            {
                // スケールモード、アニメーション復元
                foreach (var content in contents)
                {
                    content.BitmapScalingMode = scaleModeMemory[content];
                    content.AnimationImageVisibility = Visibility.Collapsed;
                    content.AnimationPlayerVisibility = Visibility.Visible;
                }

                // 読み込み再会
                BookHub.Current.IsEnabled = true;

                // スライドショー再開
                SlideShow.Current.ResumeSlideShow();
            }
        }

        #endregion

        #endregion

        #region IDisposable Support

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (this.Contents != null)
                    {
                        foreach (var content in this.Contents)
                        {
                            content.Dispose();
                        }
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [Obsolete, DataMember(Name = "StretchMode", EmitDefaultValue = false)]
            public PageStretchModeV1 StretchModeV1 { get; set; }

            [DataMember(Name = "StretchModeV2")]
            public PageStretchMode StretchMode { get; set; }


            [DataMember, DefaultValue(true)]
            public bool AllowEnlarge { get; set; }

            [DataMember, DefaultValue(true)]
            public bool AllowReduce { get; set; }

            [DataMember]
            public bool IsEnabledNearestNeighbor { get; set; }

            [DataMember]
            public double ContentsSpace { get; set; }

            [DataMember]
            public AutoRotateType AutoRotateType { get; set; }

            [DataMember]
            public GridLine.Memento GridLine { get; set; }


            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsAutoRotate { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                // before 34.0
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    AutoRotateType = IsAutoRotate ? AutoRotateType.Right : AutoRotateType.None;
                }

                // before 35.0
                if (_Version < Environment.GenerateProductVersionNumber(35, 0, 0))
                {
                    StretchMode = StretchModeV1.ToPageStretchMode();
                }
#pragma warning restore CS0612
            }

            public void RestoreConfig(Config config)
            {
                config.ImageDotKeep.IsEnabled = IsEnabledNearestNeighbor;
                config.Book.ContentsSpace = ContentsSpace;

                config.View.StretchMode = StretchMode;
                config.View.AllowStretchScaleUp = AllowEnlarge;
                config.View.AllowStretchScaleDown = AllowReduce;
                config.View.AutoRotate = AutoRotateType;

                this.GridLine.RestoreConfig(config);
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.StretchMode = Config.Current.View.StretchMode;
            memento.AllowEnlarge = Config.Current.View.AllowStretchScaleUp;
            memento.AllowReduce = Config.Current.View.AllowStretchScaleDown;
            memento.IsEnabledNearestNeighbor = Config.Current.ImageDotKeep.IsEnabled;
            memento.ContentsSpace = Config.Current.Book.ContentsSpace;
            memento.AutoRotateType = Config.Current.View.AutoRotate;
            memento.GridLine = this.GridLine.CreateMemento();
            return memento;
        }

        #endregion
    }
}
