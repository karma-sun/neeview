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
    }

    public enum BookSettingPageSelectMode
    {
        [AliasName("@EnumBookSettingSelectModeDefault")]
        Default,

        [AliasName("@EnumBookSettingSelectModeRestoreOrDefault")]
        RestoreOrDefault,
    }

    public static class BookSettingSelectorForPageExtensions
    {
        public static BookSettingPageSelectMode ToPageSelectMode(this BookSettingSelectMode self)
        {
            switch (self)
            {
                case BookSettingSelectMode.RestoreOrDefault:
                case BookSettingSelectMode.RestoreOrContinue:
                    return BookSettingPageSelectMode.RestoreOrDefault;
                default:
                    return BookSettingPageSelectMode.Default;
            }
        }

        public static BookSettingSelectMode ToNormalSelectMode(this BookSettingPageSelectMode self)
        {
            return self == BookSettingPageSelectMode.Default ? BookSettingSelectMode.Default : BookSettingSelectMode.RestoreOrDefault;
        }
    }

}
