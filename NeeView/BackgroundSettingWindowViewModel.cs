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
        /// PreviewBrush property.
        /// </summary>
        public Brush PreviewBrush
        {
            get
            {
                var brush = Source.CreateBrush();
                brush.Transform = new ScaleTransform(App.Config.Dpi.DpiScaleX, App.Config.Dpi.DpiScaleY);
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
            RaisePropertyChanged(nameof(PreviewBrush));
        }
    }
}
