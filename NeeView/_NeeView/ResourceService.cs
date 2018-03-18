using System.Diagnostics;

namespace NeeView
{
    internal static class ResourceService
    {
        /// <summary>
        /// @で始まる文字列はリソースキーとしてその値を返す。
        /// そうでない場合はそのまま返す。
        /// </summary>
        public static string GetString(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key[0] != '@')
            {
                return key;
            }
            else
            {
                var text = Properties.Resources.ResourceManager.GetString(key.Substring(1), Properties.Resources.Culture);
                if (text != null)
                {
                    return text;
                }
                else
                { 
                    Debug.WriteLine($"Error: Not found resource key: {key.Substring(1)}");
                    return key;
                }
            }
        }
    }
}
