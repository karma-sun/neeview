using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{

    public class RenameClosingEventArgs : EventArgs
    {
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public bool Cancel { get; set; }
    }

    public class RenameClosedEventArgs : EventArgs
    {
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int MoveRename { get; set; }
    }

    /// <summary>
    /// RenameControl.xaml の相互作用ロジック
    /// </summary>
    public partial class RenameControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        private char[] _invalidChars = System.IO.Path.GetInvalidFileNameChars();
        private string _old;
        private string _new;
        private int _moveRename;
        private int _keyCount;
        private bool _closing;


        public RenameControl()
        {
            InitializeComponent();
            this.RenameTextBox.DataContext = this;
        }


        public event EventHandler<RenameClosingEventArgs> Closing;
        public event EventHandler Close;
        public event EventHandler<RenameClosedEventArgs> Closed;


        // リネームを行うTextBlock
        private TextBlock _target;
        public TextBlock Target
        {
            get { return _target; }
            set
            {
                if (_target != value)
                {
                    _target = value;
                    RaisePropertyChanged();

                    Text = _target.Text;
                    _old = Text;
                    _new = Text;

                    this.RenameTextBox.FontFamily = Target.FontFamily;
                    this.RenameTextBox.FontSize = Target.FontSize;
                }
            }
        }

        // ファイル名禁則文字制御
        private bool _isInvalidFileNameChars;
        public bool IsInvalidFileNameChars
        {
            get { return _isInvalidFileNameChars; }
            set { SetProperty(ref _isInvalidFileNameChars, value); }
        }

        // パス区切り文字制御
        private bool _isInvalidSeparatorChars;
        public bool IsInvalidSeparatorChars
        {
            get { return _isInvalidSeparatorChars; }
            set { SetProperty(ref _isInvalidSeparatorChars, value); }
        }


        // 拡張子を除いた部分を選択
        public bool IsSeleftFileNameBody { get; set; } = false;

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    if (_isInvalidFileNameChars)
                    {
                        _text = new string(value.Where(e => !_invalidChars.Contains(e)).ToArray());
                        if (_text != value)
                        {
                            ToastService.Current.Show(new Toast(Properties.Resources.Notice_InvalidFileNameChars, null, ToastIcon.Information));
                        }
                    }
                    else if (_isInvalidSeparatorChars)
                    {
                        _text = new string(value.Where(e => !LoosePath.Separators.Contains(e)).ToArray());
                        if (_text != value)
                        {
                            ToastService.Current.Show(new Toast(Properties.Resources.Notice_InvalidSeparatorChars, null, ToastIcon.Information));
                        }
                    }
                    else
                    {
                        _text = value;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private void RenameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Stop(true);
        }

        public void Stop(bool isSuccess = true)
        {
            if (_closing) return;
            _closing = true;

            _new = isSuccess ? Text.Trim() : _old;

            var args = new RenameClosingEventArgs() { OldValue = _old, NewValue = _new };
            Closing?.Invoke(this, args);
            if (args.Cancel)
            {
                _new = _old;
            }

            Close?.Invoke(this, null);
        }

        private void RenameTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            // 拡張子以外を選択状態にする
            string name = this.IsSeleftFileNameBody ? System.IO.Path.GetFileNameWithoutExtension(Text) : System.IO.Path.GetFileName(Text);
            this.RenameTextBox.Select(0, name.Length);

            // 表示とともにフォーカスする
            this.RenameTextBox.Focus();
        }

        private void RenameTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            Closed?.Invoke(this, new RenameClosedEventArgs() { OldValue = _old, NewValue = _new, MoveRename = _moveRename });
        }

        private void RenameTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 最初の方向入力に限りカーソル位置を固定する
            if (_keyCount == 0 && (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right))
            {
                this.RenameTextBox.Select(this.RenameTextBox.SelectionStart + this.RenameTextBox.SelectionLength, 0);
                _keyCount++;
            }
        }

        private void RenameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                Stop(false);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                Stop(true);
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                _moveRename = (Keyboard.Modifiers == ModifierKeys.Shift) ? -1 : +1;
                Stop(true);
                e.Handled = true;
            }
        }

        private void RenameTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Stop(true);
            e.Handled = true;
        }

        private void MeasureText_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.RenameTextBox.MinWidth = Math.Min(this.MeasureText.ActualWidth + 30, this.MaxWidth);
        }

        // 単キーコマンド無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }
    }
}
