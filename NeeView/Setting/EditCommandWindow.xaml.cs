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
    public partial class EditCommandWindow : Window, INotifyPropertyChanged
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

        private CommandTable.Memento _memento;
        private string _key;

        public EditCommandWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            this.Loaded += EditCommandWindow_Loaded;
        }

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

        public void Initialize(string key, EditCommandWindowTab start = EditCommandWindowTab.Default)
        {
            _memento = CommandTable.Current.CreateMemento();
            _key = key;

            this.Title = $"{CommandTable.Current.GetElement(key).Text} - {Properties.Resources.ControlEditCommandTitle}";

            this.Note = CommandTable.Current.GetElement(key).Note;
            this.IsShowMessage = _memento.Elements[key].IsShowMessage;

            this.InputGesture.Initialize(_memento, key);
            this.MouseGesture.Initialize(_memento, key);
            this.InputTouch.Initialize(_memento, key);
            this.Parameter.Initialize(_memento, key);

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
            this.InputGesture.Flush();
            this.MouseGesture.Flush();
            this.InputTouch.Flush();
            this.Parameter.Flush();
            _memento.Elements[_key].IsShowMessage = this.IsShowMessage;

            CommandTable.Current.Restore(_memento, false);

            this.DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
