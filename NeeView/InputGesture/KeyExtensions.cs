using System.Collections.Generic;
using System.Windows.Input;

namespace NeeView
{
    public static class KeyExtensions
    {
        private static Dictionary<Key, string> _keyNameMap = new Dictionary<Key, string>()
        {
            [Key.Return] = "Enter",
            [Key.Capital] = "CapsLock",
            [Key.KanaMode] = "HangulMode",
            [Key.HanjaMode] = "KanjiMode",
            [Key.Prior] = "PageUp",
            [Key.Next] = "PageDown",
            [Key.Snapshot] = "PrintScreen",
            [Key.Oem1] = "OemSemicolon",
            [Key.Oem2] = "OemQuestion",
            [Key.Oem3] = "OemTilde",
            [Key.Oem4] = "OemOpenBrackets",
            [Key.Oem5] = "OemPipe",
            [Key.Oem6] = "OemCloseBrackets",
            [Key.Oem7] = "OemQuotes",
            [Key.Oem102] = "OemBackslash",
            [Key.OemAttn] = "DbeAlphanumeric",
            [Key.OemFinish] = "DbeKatakana",
            [Key.OemCopy] = "DbeHiragana",
            [Key.OemAuto] = "DbeSbcsChar",
            [Key.OemEnlw] = "DbeDbcsChar",
            [Key.OemBackTab] = "DbeRoman",
            [Key.Attn] = "DbeNoRoman",
            [Key.CrSel] = "DbeEnterWordRegisterMode",
            [Key.ExSel] = "DbeEnterImeConfigureMode",
            [Key.EraseEof] = "DbeFlushString",
            [Key.Play] = "DbeCodeInput",
            [Key.Zoom] = "DbeNoCodeInput",
            [Key.NoName] = "DbeDetermineString",
            [Key.Pa1] = "DbeEnterDialogConversionMode",
        };

        public static string ToUniqueString(this Key self)
        {
            if (_keyNameMap.TryGetValue(self, out string name))
            {
                return name;
            }
            else
            {
                return self.ToString();
            }
        }

    }
}
