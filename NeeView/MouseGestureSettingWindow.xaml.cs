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
    /// MouseGestureSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MouseGestureSettingWindow : Window
    {
        private MouseGestureSettingViewModel _VM;

        //private MouseGestureManager _MouseGesture { get; set; }

        //public SettingWindow.CommandParam Command { get; set; }

        public MouseGestureSettingWindow(MouseGestureSettingContext context)
        {
            InitializeComponent();

            //Command = command;
            _VM = new MouseGestureSettingViewModel(context, this.GestureBox);
            DataContext = _VM;

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _VM.Set(_VM.MouseGesture.GestureText);

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

    //
    public class MouseGestureSettingViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion


        private MouseGestureSettingContext _Context;

        //
        public MouseGestureManager MouseGesture { get; set; }


        /// <summary>
        /// Property: GestureToken
        /// </summary>
        private GestureToken _GestureToken;
        public GestureToken GestureToken
        {
            get { return _GestureToken; }
            set { if (_GestureToken != value) { _GestureToken = value; OnPropertyChanged(); } }
        }



        /// <summary>
        /// Property: Gesture
        /// </summary>
        public string Gesture
        {
            get { return _Context.Gestures[_Context.Command]; }
            set { if (_Context.Gestures[_Context.Command] != value) { _Context.Gestures[_Context.Command] = value; OnPropertyChanged(); } }
        }

        public string Header => _Context.Command.ToDispString();

        //
        public MouseGestureSettingViewModel(MouseGestureSettingContext context, FrameworkElement gestureSender)
        {
            _Context = context;

            MouseGesture = new MouseGestureManager(gestureSender);
            MouseGesture.PropertyChanged += MouseGesture_PropertyChanged;

            MouseGesture.GestureText = Gesture;
        }

        private void MouseGesture_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MouseGesture.GestureText))
            {
                UpdateGestureToken(MouseGesture.GestureText);
            }
        }

        public void UpdateGestureToken(string gesture)
        {
            // Check Conflict
            var token = new GestureToken();
            token.Gesture = gesture;

            if (!string.IsNullOrEmpty(token.Gesture))
            {
                token.Conflicts = _Context.Gestures
                    .Where(i => i.Key != _Context.Command && i.Value == token.Gesture)
                    .Select(i => i.Key)
                    .ToList();

                if (token.Conflicts.Count > 0)
                {
                    token.OverlapsText = string.Join("", token.Conflicts.Select(i => $"「{i.ToDispString()}」")) + "と競合しています";
                }
            }

            GestureToken = token;
        }

        //
        public void Set(string gesture)
        {
            Gesture = gesture;
            // _Context.Gestures[_Context.Command] = gesture;
        }

        /// <summary>
        /// Command: ClearCommand
        /// </summary>
        private RelayCommand _ClearCommand;
        public RelayCommand ClearCommand
        {
            get { return _ClearCommand = _ClearCommand ?? new RelayCommand(ClearCommand_Executed); }
        }

        private void ClearCommand_Executed()
        {
            MouseGesture.GestureText = null;
        }

    }

    //
    public class MouseGestureSettingContext
    {
        public CommandType Command { get; set; }

        public Dictionary<CommandType, string> Gestures { get; set; }
    }
}
