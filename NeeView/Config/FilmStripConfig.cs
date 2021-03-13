using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class FilmStripConfig : BindableBase
    {
        private bool _isEnabled;
        private bool _isHideFilmStrip;
        private double _imageWidth = 96.0;
        private bool _isVisibleNumber;
        private bool _isVisiblelPlate = true;
        private bool _isSelectedCenter;
        private bool _isManipulationBoundaryFeedbackEnabled = true;
        private bool _isVisiblePagemark;


        /// <summary>
        /// フィルムストリップ表示
        /// </summary>
        [PropertyMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// <summary>
        /// フィルムストリップ自動的に隠す
        /// </summary>
        [PropertyMember]
        public bool IsHideFilmStrip
        {
            get { return _isHideFilmStrip; }
            set { SetProperty(ref _isHideFilmStrip, value); }
        }

        /// <summary>
        /// サムネイルサイズ
        /// </summary>
        [PropertyRange(32, 512, TickFrequency = 8, IsEditable = true, Format = "{0} × {0}")]
        public double ImageWidth
        {
            get { return _imageWidth; }
            set { SetProperty(ref _imageWidth, Math.Max(value, 32)); }
        }

        /// <summary>
        /// ページマーク表示
        /// </summary>
        [PropertyMember]
        public bool IsVisiblePagemark
        {
            get { return _isVisiblePagemark; }
            set { SetProperty(ref _isVisiblePagemark, value); }
        }


        /// <summary>
        /// ページ番号の表示
        /// </summary>
        [PropertyMember]
        public bool IsVisibleNumber
        {
            get { return _isVisibleNumber; }
            set { SetProperty(ref _isVisibleNumber, value); }
        }

        /// <summary>
        /// サムネイル台紙の表示
        /// </summary>
        [PropertyMember]
        public bool IsVisiblePlate
        {
            get { return _isVisiblelPlate; }
            set { SetProperty(ref _isVisiblelPlate, value); }
        }

        /// <summary>
        /// スクロールビュータッチ操作の終端挙動
        /// </summary>
        [PropertyMember]
        public bool IsManipulationBoundaryFeedbackEnabled
        {
            get { return _isManipulationBoundaryFeedbackEnabled; }
            set { SetProperty(ref _isManipulationBoundaryFeedbackEnabled, value); }
        }

        /// <summary>
        /// 選択した項目が中央に表示されるようにスクロールする
        /// </summary>
        [PropertyMember]
        public bool IsSelectedCenter
        {
            get { return _isSelectedCenter; }
            set { SetProperty(ref _isSelectedCenter, value); }
        }

        #region Obsolete

        [Obsolete("Use ImageWidth instead.")] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double ThumbnailSize
        {
            get { return 0.0; }
            set { ImageWidth = (int)value; }
        }

        #endregion
    }
}



