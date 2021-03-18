using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using System;

namespace NeeView
{
    /// <summary>
    /// 「指定ページに移動」コマンド用
    /// </summary>
    public class PageSelecteDialogModel : BindableBase
    {
        private int _value;

        public PageSelecteDialogModel(int value, int min, int max)
        {
            Value = value;
            Min = min;
            Max = max;
        }

        public int Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public int Min { get; set; }
        public int Max { get; set; }

        public string Caption => Properties.Resources.JumpPageCommand;

        public string Label => string.Format(Properties.Resources.Notice_JumpPageLabel, Min, Max);

        public void AddValue(int delta)
        {
            if (delta < 0 && Value > Min)
            {
                Value = Math.Max(Value + delta, Min);
            }
            else if (delta > 0 && Value < Max)
            {
                Value = Math.Min(Value + delta, Max);
            }
        }
    }
}
