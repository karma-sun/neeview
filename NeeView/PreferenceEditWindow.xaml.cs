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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// PreferenceEditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PreferenceEditWindow : Window
    {
        PreferenceEditWindowVM _VM;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="pref"></param>
        public PreferenceEditWindow(PreferenceElement pref)
        {
            InitializeComponent();

            _VM = new PreferenceEditWindowVM(pref);
            this.DataContext = _VM;
        }

        /// <summary>
        /// OK Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Cancel Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }


        /// <summary>
        /// Reset Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            _VM.ResetValue();
        }
    }


    /// <summary>
    /// PreferenceEditWindow ViewModel
    /// </summary>
    public class PreferenceEditWindowVM : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private PreferenceElement _Element;

        public string Title => _Element.GetValueTypeString() +  "を入力してください";

        public string Key => _Element.Key;

        public string Note => _Element.Note;

        public Visibility BooleanEditControlVisibility { get; private set; }

        public Visibility ValueEditControlVisibility { get; private set; }

        public bool BooleanValue
        {
            get { return _Element.GetValueType() == typeof(bool) ? _Element.Boolean : false; }
            set { _Element.Set(value); OnPropertyChanged(); }
        }

        public string Value
        {
            get { return _Element.Value.ToString(); }
            set { _Element.SetParseValue(value); OnPropertyChanged(); }
        }

        public PreferenceEditWindowVM(PreferenceElement pref)
        {
            _Element = pref;

            if (_Element.GetValueType() == typeof(bool))
            {
                BooleanEditControlVisibility = Visibility.Visible;
                ValueEditControlVisibility = Visibility.Collapsed;
            }
            else
            {
                BooleanEditControlVisibility = Visibility.Collapsed;
                ValueEditControlVisibility = Visibility.Visible;
            }
        }

        public void ResetValue()
        {
            _Element.Reset();
            OnPropertyChanged(nameof(BooleanValue));
            OnPropertyChanged(nameof(Value));
        }
    }
}
