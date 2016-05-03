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
    public class RenameWindowParam
    {
        public string Text { get; set; }
        public string DefaultText { get; set; }
    }


    /// <summary>
    /// RenameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class RenameWindow : Window, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Property: Text
        private string _Text;
        public string Text
        {
            get { return _Text; }
            set { _Text = value; OnPropertyChanged(); }
        }
        #endregion

        private RenameWindowParam _Param;

        //
        public RenameWindow(RenameWindowParam param)
        {
            _Param = param;
            Text = _Param.Text;

            InitializeComponent();
            this.DataContext = this;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Text = _Param.DefaultText;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            _Param.Text = Text;
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
