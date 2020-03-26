using System;

namespace NeeView
{
    // この属性が定義されている場合、リファレンスの変更のみとする
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class PropertyMergeReferenceCopyAttribute : Attribute
    {
    }
}