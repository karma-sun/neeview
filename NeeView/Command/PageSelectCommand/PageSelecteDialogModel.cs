using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// 「指定ページに移動」コマンド用
    /// </summary>
    public class PageSelecteDialogModel : BindableBase
    {
        public int Value { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }

        public string Caption => Properties.Resources.JumpPageCommand_Text;

        public string Label => string.Format(Properties.Resources.NotifyJumpPageLabel, Min, Max);

        public string GetValue()
        {
            return Value.ToString();
        }

        public bool SetValue(string source)
        {
            if (CanParse(source))
            {
                Value = int.Parse(source);
                return true;
            }

            return false;
        }

        public bool CanParse(string source)
        {
            if (int.TryParse(source, out int value))
            {
                return Min <= value && value <= Max;
            }

            return false;
        }

        public string AddValue(string source, int delta)
        {
            if (!int.TryParse(source, out int value))
            {
                value = Value;
            }

            value = NeeLaboratory.MathUtility.Clamp(value + delta, Min, Max);
            return value.ToString();
        }
    }
}
