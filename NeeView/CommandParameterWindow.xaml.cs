// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.Windows.Property;
using System;
using System.Collections.Generic;
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
    /// CommandParameterWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandParameterWindow : Window
    {
        private CommandParameter _defaultParameter;
        private PropertyDocument _context;

        public CommandParameterWindow(PropertyDocument context, CommandParameter defaultParameter)
        {
            InitializeComponent();

            _context = context;
            _defaultParameter = defaultParameter;
            this.DataContext = _context;
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            _context.Set(_defaultParameter);

            this.Inspector.Reflesh();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
