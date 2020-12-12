using System;

namespace NeeView
{
    /// <summary>
    /// ドキュメント候補属性
    /// </summary>
    public class DocumentableAttribute : Attribute
    {
        /// <summary>
        /// 名前。nullの場合はタイプ名がそのまま使われる
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 型。stringであるが、実態はEnumである場合等に指定する
        /// </summary>
        public Type DocumentType { get; set; }
    }
}
