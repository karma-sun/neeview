namespace NeeView
{
    public enum BookSettingSelectMode
    {
        [AliasName("@EnumBookSettingSelectModeDefault")]
        Default,

        [AliasName("@EnumBookSettingSelectModeContinue")]
        Continue,

        [AliasName("@EnumBookSettingSelectModeRestoreOrDefault")]
        RestoreOrDefault,

        [AliasName("@EnumBookSettingSelectModeRestoreOrContinue")]
        RestoreOrContinue,

        [AliasName("@EnumBookSettingSelectModeRestoreOrDefaultReset", IsVisibled = false)]
        RestoreOrDefaultReset,
    }

    public enum BookSettingPageSelectMode
    {
        [AliasName("@EnumBookSettingSelectModeDefault")]
        Default,

        [AliasName("@EnumBookSettingSelectModeRestoreOrDefault")]
        RestoreOrDefault,

        [AliasName("@EnumBookSettingSelectModeRestoreOrDefaultReset")]
        RestoreOrDefaultReset,
    }

    public static class BookSettingSelectorForPageExtensions
    {
        public static BookSettingPageSelectMode ToPageSelectMode(this BookSettingSelectMode self)
        {
            switch (self)
            {
                case BookSettingSelectMode.RestoreOrDefaultReset:
                    return BookSettingPageSelectMode.RestoreOrDefaultReset;
                case BookSettingSelectMode.RestoreOrDefault:
                case BookSettingSelectMode.RestoreOrContinue:
                    return BookSettingPageSelectMode.RestoreOrDefault;
                default:
                    return BookSettingPageSelectMode.Default;
            }
        }

        public static BookSettingSelectMode ToNormalSelectMode(this BookSettingPageSelectMode self)
        {
            switch(self)
            {
                case BookSettingPageSelectMode.RestoreOrDefaultReset:
                    return BookSettingSelectMode.RestoreOrDefaultReset;
                case BookSettingPageSelectMode.RestoreOrDefault:
                    return BookSettingSelectMode.RestoreOrDefault;
                default:
                    return BookSettingSelectMode.Default;
            }
        }
    }

}
