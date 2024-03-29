﻿using NeeLaboratory;
using NeeView.Windows.Property;

namespace NeeView
{
    public class PrevSizePageCommand : CommandElement
    {
        public PrevSizePageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.IsShowMessage = false;
            this.PairPartner = "NextSizePage";

            this.ParameterSource = new CommandParameterSource(new MoveSizePageCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.PrevSizePage(this, ((MoveSizePageCommandParameter)e.Parameter).Size);
        }
    }



    /// <summary>
    /// 指定ページ数移動コマンド用パラメータ
    /// </summary>
    public class MoveSizePageCommandParameter : ReversibleCommandParameter
    {
        private int _size = 10;

        [PropertyMember]
        public int Size
        {
            get { return _size; }
            set { SetProperty(ref _size, MathUtility.Clamp(value, 0, 1000)); }
        }
    }
}
