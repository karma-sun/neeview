// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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

using System.Collections.ObjectModel;
using System.Diagnostics;
using NeeView.Windows.Input;
using NeeView.ComponentModel;
using System.Globalization;

namespace NeeView
{
    /// <summary>
    ///  InputToucheSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InputTouchSettingWindow : Window
    {
        private InputTouchSettingViewModel _vm;

        //
        public InputTouchSettingWindow(InputTouchSettingContext context)
        {
            InitializeComponent();

            this.GestureBox.PreviewMouseLeftButtonUp += GestureBox_PreviewMouseLeftButtonUp;

            _vm = new InputTouchSettingViewModel(context, this.GestureBox);
            DataContext = _vm;

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }

        //
        private void GestureBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var width = this.GestureBox.ActualWidth;
            var pos = e.GetPosition(this.GestureBox);

            _vm.SetTouchGesture(pos, this.GestureBox.ActualWidth, this.GestureBox.ActualHeight);
        }

        //
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.Decide();

            this.DialogResult = true;
            this.Close();
        }

        //
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    /// <summary>
    /// MouseGestureSetting ViewModel
    /// </summary>
    public class InputTouchSettingViewModel : BindableBase
    {
        //
        private InputTouchSettingContext _context;

        /// <summary>
        /// GestureElements property.
        /// </summary>
        public ObservableCollection<GestureElement> GestureToken
        {
            get { return _gestureToken; }
            set { if (_gestureToken != value) { _gestureToken = value; RaisePropertyChanged(); } }
        }

        private ObservableCollection<GestureElement> _gestureToken = new ObservableCollection<GestureElement>();

        //
        /// <summary>
        /// GestoreTokenNote property.
        /// </summary>
        public string GestureTokenNote
        {
            get { return _gestureTokenNote; }
            set { if (_gestureTokenNote != value) { _gestureTokenNote = value; RaisePropertyChanged(); } }
        }

        private string _gestureTokenNote;


        //
        public TouchAreaMap TouchAreaMap { get; set; }
        

        /// <summary>
        /// Window Title
        /// </summary>
        public string Header => _context.Header ?? _context.Command.ToDispString();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="gestureSender"></param>
        public InputTouchSettingViewModel(InputTouchSettingContext context, FrameworkElement gestureSender)
        {
            _context = context;

            this.TouchAreaMap = new TouchAreaMap(_context.Gesture);
            UpdateGestureToken(this.TouchAreaMap);
        }

        //
        internal void SetTouchGesture(Point pos, double width, double height)
        {
            var gesture = TouchGestureExtensions.GetTouchGesture(pos.X / width, pos.Y / height);

            this.TouchAreaMap.Toggle(gesture);
            RaisePropertyChanged(nameof(TouchAreaMap));

            UpdateGestureToken(this.TouchAreaMap);
        }


        /// <summary>
        /// Update Gesture Information
        /// </summary>
        /// <param name="map"></param>
        public void UpdateGestureToken(TouchAreaMap map)
        {
            string gestures = map.ToString();
            this.GestureTokenNote = null;

            if (!string.IsNullOrEmpty(gestures))
            {
                var shortcuts = new ObservableCollection<GestureElement>();
                foreach (var key in gestures.Split(','))
                {
                    var overlaps = _context.Gestures
                        .Where(i => i.Key != _context.Command && i.Value.Split(',').Contains(key))
                        .Select(e => $"「{e.Key.ToDispString()}」")
                        .ToList();

                    if (overlaps.Count > 0)
                    {
                        if (this.GestureTokenNote != null) this.GestureTokenNote += "\n";
                        this.GestureTokenNote += $"{key} は {string.Join("", overlaps)} と競合しています";
                    }

                    var element = new GestureElement();
                    element.Gesture = key;
                    element.IsConflict = overlaps.Count > 0;
                    element.Splitter = ",";

                    shortcuts.Add(element);
                }

                if (shortcuts.Count > 0)
                {
                    shortcuts.Last().Splitter = null;
                }

                this.GestureToken = shortcuts;
            }
            else
            {
                this.GestureToken = new ObservableCollection<GestureElement>();
            }
        }


        /// <summary>
        /// 決定
        /// </summary>
        public void Decide()
        {
            _context.Gesture = this.TouchAreaMap.ToString();
        }
    }


    /// <summary>
    /// MouseGestureSetting Model
    /// </summary>
    public class InputTouchSettingContext
    {
        /// <summary>
        /// 表示名。nullの場合はCommand名を使用する
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// 設定対象のコマンド
        /// </summary>
        public CommandType Command { get; set; }

        /// <summary>
        /// 全てのコマンドのジェスチャー。競合判定に使用する
        /// </summary>
        public Dictionary<CommandType, string> Gestures { get; set; }


        /// <summary>
        /// Property: Gesture
        /// </summary>
        public string Gesture
        {
            get { return Gestures[Command]; }
            set { if (Gestures[Command] != value) { Gestures[Command] = value; } }
        }
    }
    

    /// <summary>
    /// タッチエリア管理用
    /// </summary>
    public class TouchAreaMap
    {
        //
        private Dictionary<TouchGesture, bool> _map;

        //
        public TouchAreaMap(string gestureString)
        {
            _map = Enum.GetValues(typeof(TouchGesture)).Cast<TouchGesture>().ToDictionary(e => e, e => false);

            if (gestureString != null)
            {
                foreach (var token in gestureString.Split(','))
                {
                    if (Enum.TryParse(token, out TouchGesture key))
                    {
                        _map[key] = true;
                    }
                }
            }
        }

        //
        public bool this[TouchGesture gesture]
        {
            get { return _map[gesture]; }
            set { _map[gesture] = value; }
        }

        //
        public void Toggle(TouchGesture gesture)
        {
            _map[gesture] = !_map[gesture];
        }

        //
        public void Clear()
        {
            foreach (var key in _map.Keys)
            {
                _map[key] = false;
            }
        }

        //
        public override string ToString()
        {
            return string.Join(",", _map.Where(e => e.Key != TouchGesture.None && e.Value == true).Select(e => e.Key.ToString()));
        }
    }


    /// <summary>
    /// タッチエリアを背景色に変換
    /// </summary>
    public class TouchAreaToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var map = (TouchAreaMap)value;
            var gesture = (TouchGesture)parameter;

            return map[gesture] ? Brushes.SteelBlue : Brushes.AliceBlue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
