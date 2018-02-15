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

namespace NeeView.Setting
{
    public enum EditCommandWindowTab
    {
        Default,
        InputGesture,
        MouseGesture,
        InputTouch,
        Parameter,
    }

    /// <summary>
    /// EditCommandWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditCommandWindow : Window
    {
        private CommandTable.Memento _memento;
        
        public EditCommandWindow()
        {
            InitializeComponent();
        }

        public void Initialize(CommandType key,  EditCommandWindowTab start = EditCommandWindowTab.Default)
        {
            _memento = CommandTable.Current.CreateMemento();

            this.Title = $"{key.ToDispString()} - コマンド設定";

            this.InputGesture.Initialize(_memento, key);
            this.MouseGesture.Initialize(_memento, key);
            this.InputTouch.Initialize(_memento, key);
            this.Parameter.Initialize(_memento, key);

            switch(start)
            {
                case EditCommandWindowTab.InputGesture:
                    this.InputGestureTab.IsSelected = true;
                    break;
                case EditCommandWindowTab.MouseGesture:
                    this.MouseGestureTab.IsSelected = true;
                    break;
                case EditCommandWindowTab.InputTouch:
                    this.InputTouchTab.IsSelected = true;
                    break;
                case EditCommandWindowTab.Parameter:
                    this.ParameterTab.IsSelected = true;
                    break;
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.InputGesture.Flush();
            this.MouseGesture.Flush();
            this.InputTouch.Flush();
            this.Parameter.Flush();
            CommandTable.Current.Restore(_memento);
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
