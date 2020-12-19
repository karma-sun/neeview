using System;

namespace NeeView
{
    // 複数ページのときの動作
    public enum MultiPagePolicy
    {
        [AliasName]
        Once,

        [AliasName]
        All,

        [Obsolete] // ver.37
        [AliasName(IsVisibled = false)]
        Twice = All,
    };
}
