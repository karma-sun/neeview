using System;

namespace NeeView
{
    // この属性がクラスに定義されている場合、リファレンスの変更のみとする
    [AttributeUsage(AttributeTargets.Class)]
    public class PropertyMergeReferenceCopyAttribute : Attribute
    {
    }
}