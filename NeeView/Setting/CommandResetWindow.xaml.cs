﻿using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
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

namespace NeeView.Setting
{
    /// <summary>
    /// CommandResetWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandResetWindow : Window
    {
        private CommandResetWindowViewModel _vm;

        /// <summary>
        /// constructor
        /// </summary>
        public CommandResetWindow()
        {
            InitializeComponent();

            _vm = new CommandResetWindowViewModel();
            this.DataContext = _vm;
        }

        /// <summary>
        /// 現在の設定でコマンドテーブルを生成
        /// </summary>
        /// <returns></returns>
        public CommandTable.Memento CreateCommandMemento()
        {
            return CommandTable.CreateDefaultMemento(_vm.InputSceme);
        }
    }

    /// <summary>
    /// CommandResetWindow ViewModel
    /// </summary>
    public class CommandResetWindowViewModel : BindableBase
    {
        /// <summary>
        /// InputScheme 表示テーブル
        /// </summary>
        public Dictionary<InputSceme, string> InputScemeList { get; } = new Dictionary<InputSceme, string>
        {
            [InputSceme.TypeA] = Properties.Resources.ControlCommandResetTypeA,
            [InputSceme.TypeB] = Properties.Resources.ControlCommandResetTypeB,
            [InputSceme.TypeC] = Properties.Resources.ControlCommandResetTypeC
        };

        /// <summary>
        /// ImputScheme 説明テーブル
        /// </summary>
        public Dictionary<InputSceme, string> InputScemeNoteList { get; } = new Dictionary<InputSceme, string>
        {
            [InputSceme.TypeA] = ResourceService.Replace(Properties.Resources.ControlCommandResetTypeANote),
            [InputSceme.TypeB] = ResourceService.Replace(Properties.Resources.ControlCommandResetTypeBNote),
            [InputSceme.TypeC] = ResourceService.Replace(Properties.Resources.ControlCommandResetTypeCNote),
        };

        /// <summary>
        /// InputSceme property.
        /// </summary>
        private InputSceme _InputSceme;
        public InputSceme InputSceme
        {
            get { return _InputSceme; }
            set { if (_InputSceme != value) { _InputSceme = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(InputScemeNote)); } }
        }

        /// <summary>
        /// InputScemeNote property.
        /// </summary>
        public string InputScemeNote => InputScemeNoteList[InputSceme];

        /// <summary>
        /// OkCommand command.
        /// </summary>
        private RelayCommand<Window> _OkCommand;
        public RelayCommand<Window> OkCommand
        {
            get { return _OkCommand = _OkCommand ?? new RelayCommand<Window>(OkCommand_Executed); }
        }

        private void OkCommand_Executed(Window window)
        {
            window.DialogResult = true;
            window.Close();
        }

        /// <summary>
        /// CancelCommand command.
        /// </summary>
        private RelayCommand<Window> _CancelCommand;
        public RelayCommand<Window> CancelCommand
        {
            get { return _CancelCommand = _CancelCommand ?? new RelayCommand<Window>(CancelCommand_Executed); }
        }

        private void CancelCommand_Executed(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}
