using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Globalization;

namespace NeeView
{
    public enum Language
    {
        [AliasName]
        English,

        [AliasName]
        Japanese,
    }

    public static class LanguageExtensions
    {
        public static string GetCultureName(this Language self)
        {
            switch (self)
            {
                default:
                case Language.English:
                    return "en-US";
                case Language.Japanese:
                    return "ja-JP";
            }
        }

        public static Language GetLanguage(string cultureName)
        {
            switch (cultureName)
            {
                default:
                case "en-US":
                    return Language.English;
                case "ja-JP":
                    return Language.Japanese;
            }
        }
    }

}
