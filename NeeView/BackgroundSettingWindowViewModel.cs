// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.ComponentModel;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// BackgroundSettingWindow ViewModel
    /// </summary>
    public class BackgroundSettingWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// BackBrush property.
        /// </summary>
        private Brush _BackBrush;
        public Brush BackBrush
        {
            get
            {
                var brush = Source.CreateBackBrush();
                return brush;
            }
        }


        /// <summary>
        /// FrontBrush property.
        /// </summary>
        public Brush FrontBrush
        {
            get
            {
                var brush = Source.CreateFrontBrush();
                return brush;
            }
        }



        /// <summary>
        /// ブラシソース
        /// </summary>
        public BrushSource Source { get; set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="source"></param>
        public BackgroundSettingWindowViewModel(BrushSource source)
        {
            Source = source ?? new BrushSource();
            Source.PropertyChanged += Source_PropertyChanged;
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void Closed()
        {
            Source.PropertyChanged -= Source_PropertyChanged;
        }

        /// <summary>
        /// ブラシソース変更時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(BackBrush));
            RaisePropertyChanged(nameof(FrontBrush));
        }
    }
}
