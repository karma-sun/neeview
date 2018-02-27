using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // ファイルページ用コンテンツのアイコン
    public enum FilePageIcon
    {
        File,
        Archive,
        Folder,
        Alart,
    }

    /// <summary>
    /// ファイルページ用コンテンツ
    /// FilePageControl のパラメータとして使用される
    /// </summary>
    public class FilePageContent
    {
        public FilePageIcon Icon { get; set; }
        public string FileName { get; set; }
        public string Message { get; set; }
    }
}
