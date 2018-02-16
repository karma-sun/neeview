// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        private DragActionTable.Memento _memento;
        private DragActionType _key;

        public MouseDragSettingWindow()
        {
            InitializeComponent();
        }


        //
        public void Initialize(DragActionType key)
        {
            _memento = DragActionTable.Current.CreateMemento();
            _key = key;

            this.Title = $"{_key.ToAliasName()} - ドラッグ操作設定";

            _vm = new MouseDragSettingViewModel(_memento, _key, this.GestureBox);
            DataContext = _vm;

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }

        //
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.Decide();
            DragActionTable.Current.Restore(_memento);

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
    /// 
    /// </summary>
    public class DragToken
    {
        // ジェスチャー文字列（１ジェスチャー）
        public string Gesture { get; set; }

        // 競合しているコマンド群
        public List<DragActionType> Conflicts { get; set; }

        // 競合メッセージ
        public string OverlapsText { get; set; }

        public bool IsConflict => Conflicts != null && Conflicts.Count > 0;
    }
}
