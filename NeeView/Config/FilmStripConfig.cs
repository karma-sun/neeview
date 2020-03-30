using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class FilmStripConfig : BindableBase
    {
        private bool _isEnabled;
        private bool _isHideFilmStrip;
        private double _thumbnailSize = 96.0;
        private bool _isVisibleNumber;
        private bool _isVisiblelPlate = true;
        private bool _isSelectedCenter;
        private bool _isManipulationBoundaryFeedbackEnabled = true;


        /// <summary>
        /// フィルムストリップ表示
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// <summary>
        /// サムネイルを自動的に隠す
        /// </summary>
        public bool IsHideFilmStrip
        {
            get { return _isHideFilmStrip; }
            set { SetProperty(ref _isHideFilmStrip, value); }
        }

        /// <summary>
        /// サムネイルサイズ
        /// </summary>
        [PropertyRange("@ParamFilmStripThumbnailSize", 16, 256, TickFrequency = 8, Format = "{0}×{0}")]
        public double ThumbnailSize
        {
            get { return _thumbnailSize; }
            set { SetProperty(ref _thumbnailSize, MathUtility.Clamp(value, 16, 256)); }
        }

        /// <summary>
        /// ページ番号の表示
        /// </summary>
        [PropertyMember("@ParamFilmStripIsVisibleThumbnailNumber")]
        public bool IsVisibleNumber
        {
            get { return _isVisibleNumber; }
            set { SetProperty(ref _isVisibleNumber, value); }
        }

        /// <summary>
        /// サムネイル台紙の表示
        /// </summary>
        [PropertyMember("@ParamFilmStripIsVisibleThumbnailPlate", Tips = "@ParamFilmStripIsVisibleThumbnailPlateTips")]
        public bool IsVisiblePlate
        {
            get { return _isVisiblelPlate; }
            set { SetProperty(ref _isVisiblelPlate, value); }
        }

        /// <summary>
        /// スクロールビュータッチ操作の終端挙動
        /// </summary>
        [PropertyMember("@ParamFilmStripIsManipulationBoundaryFeedbackEnabled")]
        public bool IsManipulationBoundaryFeedbackEnabled
        {
            get { return _isManipulationBoundaryFeedbackEnabled; }
            set { SetProperty(ref _isManipulationBoundaryFeedbackEnabled, value); }
        }

        /// <summary>
        /// 選択した項目が中央に表示されるようにスクロールする
        /// </summary>
        [PropertyMember("@ParamFilmStripIsSelectedCenter")]
        public bool IsSelectedCenter
        {
            get { return _isSelectedCenter; }
            set { SetProperty(ref _isSelectedCenter, value); }
        }
    }
}



