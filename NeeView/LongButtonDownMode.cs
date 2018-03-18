namespace NeeView
{
    // 長押しモード
    public enum LongButtonDownMode
    {
        [AliasName("@LongButtonDownModeNone")]
        None,

        [AliasName("@LongButtonDownModeLoupe", Tips = "@LongButtonDownModeLoupeTips")]
        Loupe,

        [AliasName("@LongButtonDownModeRepeat", Tips = "@LongButtonDownModeRepeatTips")]
        Repeat,
    }

    //
    public static class LongButtonDownModeExtensions
    {
        public static string ToTips(this LongButtonDownMode element)
        {
            return AliasNameExtensions.GetTips(element);
        }
    }

}
