using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// コマンド集 ： RoutedCommand
    /// </summary>
    public class RoutedCommandTable
    {
        public static RoutedCommandTable Current { get; private set; }

        //
        public Dictionary<CommandType, RoutedUICommand> Commands { get; set; } = new Dictionary<CommandType, RoutedUICommand>();

        // インテグザ
        public RoutedUICommand this[CommandType key]
        {
            get { return Commands[key]; }
            set { Commands[key] = value; }
        }

        //
        private CommandTable _commandTable;

        //
        public RoutedCommandTable(CommandTable commandTable)
        {
            Current = this;

            _commandTable = commandTable;

            // RoutedCommand作成
            foreach (CommandType type in Enum.GetValues(typeof(CommandType)))
            {
                Commands.Add(type, new RoutedUICommand(commandTable[type].Text, type.ToString(), typeof(MainWindow)));
            }
        }


        // コマンド実行 
        // CommandTableを純粋なコマンド定義のみにするため、コマンド実行に伴う処理はここで定義している
        public void Execute(CommandType type, object sender, object param)
        {
            // 通知
            if (_commandTable[type].IsShowMessage)
            {
                string message = _commandTable[type].ExecuteMessage(param);
                InfoMessage.Current.SetMessage(InfoMessageType.Command, message);
            }

            // 実行
            _commandTable[type].Execute(sender, param);
        }


        #region Memento
        // compatible before ver.23
        [DataContract]
        public class Memento
        {
            [DataMember]
            public ShowMessageStyle CommandShowMessageStyle { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            return null;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            InfoMessage.Current.CommandShowMessageStyle = memento.CommandShowMessageStyle;
        }

        #endregion
    }
}
