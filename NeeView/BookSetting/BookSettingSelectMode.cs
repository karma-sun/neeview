namespace NeeView
{
    public enum BookSettingSelectMode
    {
        [AliasName]
        Default,

        [AliasName]
        Continue,

        [AliasName]
        RestoreOrDefault,

        [AliasName]
        RestoreOrContinue,

        [AliasName(IsVisibled = false)]
        RestoreOrDefaultReset,
    }

    public enum BookSettingPageSelectMode
    {
        [AliasName]
        Default,

        [AliasName]
        RestoreOrDefault,

        [AliasName]
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
