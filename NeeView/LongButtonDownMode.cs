namespace NeeView
{
    // 長押しモード
    public enum LongButtonDownMode
    {
        [AliasName("なし")]
        None,

        [AliasName("ルーペ", Tips = "一時的に画像を拡大表示します。ルーペ表示中にホイール操作で拡大率を変更できます。")]
        Loupe,

        [AliasName("リピート入力", Tips ="クリックを連続した挙動になり、対応したコマンドを連続発行します。")]
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
