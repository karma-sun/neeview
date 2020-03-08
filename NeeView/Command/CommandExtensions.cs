// TODO: コマンド引数にコマンドパラメータを渡せないだろうか。（現状メニュー呼び出しであることを示すタグが指定されることが有る)

namespace NeeView
{
    public static class CommandExtensions
    {
        public static CommandElement ToCommand(this string key)
        {
            return CommandTable.Current.TryGetValue(key, out CommandElement command) ? command : CommandElement.None;
        }
    }
}
