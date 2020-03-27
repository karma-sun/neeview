using System;

namespace NeeView
{
    // この属性が定義されている場合、リファレンスの変更のみとする
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class ObjectMergeReferenceCopyAttribute : Attribute
    {
    }

    /// <summary>
    /// この属性が指定されている場合、オプションによってはマージされない
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ObjectMergeIgnoreAttribute : Attribute
    {
    }
}