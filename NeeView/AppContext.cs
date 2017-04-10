// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アプリの内部状態を保持するクラス。
    /// 保存される静的な情報は<see cref="Preference"/>で。
    /// </summary>
    public class AppContext : INotifyPropertyChanged
    {
        /// <summary>
        /// システムオブジェクト
        /// </summary>
        public static AppContext Current { get; private set; } = new AppContext();
        
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// すべての変更を通知
        /// 設定変更直後等
        /// </summary>
        /// <returns></returns>
        public void RaizeAllPropertyChanged()
        {
            RaisePropertyChanged("");
            IsPlayingSlideShowChanged?.Invoke(this, null);
        }
        

        /// <summary>
        /// IsPlayingSlideShow property.
        /// </summary>
        private bool _IsPlayingSlideShow;
        public bool IsPlayingSlideShow
        {
            get { return _IsPlayingSlideShow; }
            set { if (_IsPlayingSlideShow != value) { _IsPlayingSlideShow = value; RaisePropertyChanged(); IsPlayingSlideShowChanged?.Invoke(this, null); } }
        }

        //
        public event EventHandler IsPlayingSlideShowChanged;

        //
        private bool _isPlayingSlideShowMemento;

        //
        public void PauseSlideShow()
        {
            _isPlayingSlideShowMemento = IsPlayingSlideShow;
            IsPlayingSlideShow = false;
        }

        //
        public void ResumeSlideShow()
        {
            IsPlayingSlideShow = _isPlayingSlideShowMemento;
        }
    }
}
