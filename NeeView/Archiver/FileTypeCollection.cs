using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// ファイル拡張子コレクション
    /// </summary>
    public class FileTypeCollection : StringCollection
    {
        public FileTypeCollection()
        {
        }

        public FileTypeCollection(string exts) : base(exts)
        {
        }

        public override string Add(string token)
        {
            var ext = token?.Trim().TrimStart('.').ToLower();
            if (string.IsNullOrWhiteSpace(ext))
            {
                return null;
            }

            ext = "." + ext;
            if (Contains(ext))
            {
                return ext;
            }

            base.Add(ext);
            return ext;
        }
    }
}
