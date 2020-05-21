using System;

namespace NeeView
{
    // 複数ページのときの動作
    public enum MultiPagePolicy
    {
        [AliasName("@EnumMultiPageOptionTypeOnce")]
        Once,

        [AliasName("@EnumMultiPageOptionTypeAll")]
        All,

        [Obsolete] // ver.37
        [AliasName("@EnumMultiPageOptionTypeTwice", IsVisibled = false)]
        Twice = All,
    };
}
