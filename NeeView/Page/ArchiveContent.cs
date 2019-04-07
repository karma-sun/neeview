using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// アーカイブコンテンツ
    /// 対象のサムネイルを作成
    /// </summary>
    public class ArchiveContent : BitmapContent
    {
        private string _path;

        #region Constructors

        /// <param name="entry">対象アーカイブもしくはファイルのエントリ</param>
        public ArchiveContent(ArchiveEntry entry) : base(entry)
        {
            _path = entry?.SystemPath;

            // エントリが有効でない場合の処理
            if (!entry.IsValid && !entry.IsArchivePath)
            {
                Thumbnail.Initialize(null);
            }
        }

        public ArchiveContent(string path) : base(null)
        {
            _path = path;

            SetPageMessage(new PageMessage()
            {
                Icon = FilePageIcon.Alart,
                Message = "For thumbnail creation only",
            });
        }

        #endregion

        public string SourcePath => _path;

        /// <summary>
        /// コンテンツ有効フラグはサムネイルの存在で判定
        /// </summary>
        public override bool IsLoaded => Thumbnail.IsValid;

        public override bool IsViewReady => IsLoaded;

        /// <summary>
        /// コンテンツサイズは固定
        /// </summary>
        public override Size Size => new Size(512, 512);

        public override bool CanResize => false;


        public override string ToString()
        {
            return _path != null ? LoosePath.GetFileName(_path) : base.ToString();
        }

        public override IContentLoader CreateContentLoader()
        {
            return new ArchiveContentLoader(this);
        }
    }

}
