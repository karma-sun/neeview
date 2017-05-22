// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Property;
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
        private PreferenceEditWindowVM _VM;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="pref"></param>
        public PreferenceEditWindow(PropertyMemberElement pref)
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
            this.ItemsControl.Items.Refresh();
        }
    }


    /// <summary>
    /// PreferenceEditWindow ViewModel
    /// </summary>
    public class PreferenceEditWindowVM : BindableBase
    {
        public PropertyMemberElement Element { get; set; }

        public string Title => this.Element.GetValueTypeString() + "を入力してください";

        public List<PropertyValue> Items { get; set; }

        //
        public PreferenceEditWindowVM(PropertyMemberElement pref)
        {
            this.Element = pref;

            Items = new List<PropertyValue>() { pref.TypeValue };
        }

        public void ResetValue()
        {
            this.Element.ResetValue();
        }
    }
}
