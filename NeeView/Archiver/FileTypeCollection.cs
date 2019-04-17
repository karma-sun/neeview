using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NeeView.Text;

namespace NeeView
{
    /// <summary>
    /// ファイル拡張子コレクション
    /// </summary>
    [DataContract]
    public class FileTypeCollection : StringCollection
    {
        public FileTypeCollection()
        {
        }

        public FileTypeCollection(string exts) : base(exts)
        {
        }

        public FileTypeCollection(IEnumerable<string> exts) : base(exts)
        {
        }

        public override string ValidateItem(string item)
        {
            return string.IsNullOrWhiteSpace(item) ? null : "." + item.Trim().TrimStart('.').ToLower();
        }

        public FileTypeCollection Clone()
        {
            return (FileTypeCollection)MemberwiseClone();
        }
    }
}
