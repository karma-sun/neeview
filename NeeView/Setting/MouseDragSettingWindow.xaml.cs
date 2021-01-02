using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// MouseDragSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MouseDragSettingWindow : Window
    {
        private MouseDragSettingViewModel _vm;
        private DragActionCollection _memento;
        private string _key;

        public MouseDragSettingWindow()
        {
            InitializeComponent();

            this.Loaded += MouseDragSettingWindow_Loaded;
            this.KeyDown += MouseDragSettingWindow_KeyDown;
        }


        private void MouseDragSettingWindow_Loaded(object sender, RoutedEventArgs e)
        {   
            this.OkButton.Focus();
        }

        private void MouseDragSettingWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        public void Initialize(string key)
        {
            _memento = DragActionTable.Current.CreateDragActionCollection();
            _key = key;

            var note = DragActionTable.Current.Elements[_key].Note;
            this.Title = $"{note} - {Properties.Resources.MouseDragSettingWindow_Title}";

            _vm = new MouseDragSettingViewModel(_memento, _key, this.GestureBox);
            DataContext = _vm;

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.Decide();
            DragActionTable.Current.RestoreDragActionCollection(_memento);

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }



    /// <summary>
    /// 
    /// </summary>
    public class DragToken
    {
        // ジェスチャー文字列（１ジェスチャー）
        public string Gesture { get; set; }

        // 競合しているコマンド群
        public List<string> Conflicts { get; set; }

        // 競合メッセージ
        public string OverlapsText { get; set; }

        public bool IsConflict => Conflicts != null && Conflicts.Count > 0;
    }
}
