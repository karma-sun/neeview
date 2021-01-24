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
    public enum EditCommandWindowTab
    {
        Default,
        General,
        InputGesture,
        MouseGesture,
        InputTouch,
        Parameter,
    }

    /// <summary>
    /// EditCommandWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditCommandWindow : Window, INotifyPropertyChanged, INotifyMouseHorizontalWheelChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

        private CommandCollection _memento;
        private string _key;

        public EditCommandWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            var mouseHorizontalWheel = new MouseHorizontalWheelService(this);
            mouseHorizontalWheel.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(s, e);

            this.Loaded += EditCommandWindow_Loaded;
            this.Closed += EditCommandWindow_Closed;
        }


        public event MouseWheelEventHandler MouseHorizontalWheelChanged;


        private bool _isShowMessage;
        public bool IsShowMessage
        {
            get { return _isShowMessage; }
            set { if (_isShowMessage != value) { _isShowMessage = value; RaisePropertyChanged(); } }
        }

        public string Note { get; private set; }


        private void EditCommandWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var tabItem = this.TabControl.ItemContainerGenerator.ContainerFromItem(this.TabControl.SelectedItem) as TabItem;
            tabItem?.Focus();
        }

        private void EditCommandWindow_Closed(object sender, EventArgs e)
        {
            if (this.DialogResult == true)
            {
                Flush();
            }
            else
            {
                CommandTable.Current.RestoreCommandCollection(_memento);
            }
        }

        public void Initialize(string key, EditCommandWindowTab start = EditCommandWindowTab.Default)
        {
            _memento = CommandTable.Current.CreateCommandCollectionMemento();
            _key = key;

            var commandMap = CommandTable.Current;

            this.Title = $"{commandMap[key].Text} - {Properties.Resources.EditCommandWindow_Title}";

            this.Note = commandMap[key].Remarks;
            this.IsShowMessage = commandMap[key].IsShowMessage;

            this.InputGesture.Initialize(commandMap, key);
            this.MouseGesture.Initialize(commandMap, key);
            this.InputTouch.Initialize(commandMap, key);
            this.Parameter.Initialize(commandMap, key);

            switch (start)
            {
                case EditCommandWindowTab.General:
                    this.GeneralTab.IsSelected = true;
                    break;
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

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Flush()
        {
            this.InputGesture.Flush();
            this.MouseGesture.Flush();
            this.InputTouch.Flush();
            this.Parameter.Flush();

            CommandTable.Current.GetElement(_key).IsShowMessage = this.IsShowMessage;
            CommandTable.Current.RaiseChanged();
        }

    }
}
