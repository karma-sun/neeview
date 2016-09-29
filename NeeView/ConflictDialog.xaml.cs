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
    /// ConflictDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ConflictDialog : Window
    {
        public ConflictDialog(ConflictDialogContext context)
        {
            InitializeComponent();

            this.DataContext = new ConflictDialogVM(context);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    public class ConflictItem
    {
        public bool IsChecked { get; set; }
        public CommandType CommandType { get; set; }
        public string Name => CommandType.ToDispString();
    }

    public class ConflictDialogVM : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private ConflictDialogContext _Context;

        public string Header => _Context.Header;
        public List<ConflictItem> Commands => _Context.Commands;

        public ConflictDialogVM(ConflictDialogContext context)
        {
            _Context = context;
            //Commands = new List<ConflictItem>(_Context.Commands);
        }
    }

    public class ConflictDialogContext
    {
        public string Header { get; set; }
        public string Gesture { get; set; }
        public List<ConflictItem> Commands { get; set; }

        public ConflictDialogContext(CommandType commandType, string gesture, List<CommandType> commands)
        {
            Header = $"{commandType.ToDispString()} - 競合の解消";
            Gesture = gesture;
            Commands = commands.Select(e => new ConflictItem() { CommandType = e }).ToList();
            Commands[0].IsChecked = true;
        }
    }
}
