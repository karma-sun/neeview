using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NeeLaboratory.ComponentModel;
using NeeView.Runtime.LayoutPanel;
using NeeView.Windows;

namespace NeeView
{
    public class SidePanelDropAcceptor : BindableBase
    {
        /// <summary>
        /// ドロップ受け入れ先コントロール.
        /// ドロップイベント受信コントロールとは異なるために用意した.
        /// </summary>
        private ItemsControl _itemsControl;
        private LayoutDockPanelContent _dock;

        private DropAcceptDescription _description;

        public SidePanelDropAcceptor(ItemsControl itemsControl, LayoutDockPanelContent dock)
        {
            _itemsControl = itemsControl;
            _dock = dock;

            this.Description = new DropAcceptDescription();
            this.Description.DragOver += Description_DragOver;
            this.Description.DragDrop += Description_DragDrop;
        }



        /// <summary>
        /// ドロップイベント
        /// </summary>
        public EventHandler<LayoutPanelDropedEventArgs> PanelDroped;


        /// <summary>
        /// ドロップ処理設定プロパティ
        /// </summary>
        public DropAcceptDescription Description
        {
            get { return _description; }
            set { if (_description != value) { _description = value; RaisePropertyChanged(); } }
        }



        /// <summary>
        /// ドロップ処理
        /// </summary>
        /// <param name="e"></param>
        private void Description_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var panel = e.Data.GetData<LayoutPanel>();
                if (panel == null) return;

                var index = GetItemInsertIndex(e);
                PanelDrop(index, panel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Drop failed: {ex.Message}");
            }
        }

        private void PanelDrop(int index, LayoutPanel panel)
        {
            _dock.MovePanel(index, panel);

            // NOTE: 未使用？
            PanelDroped?.Invoke(this, new LayoutPanelDropedEventArgs(panel, index));
        }

        /// <summary>
        /// カーソルからリストの挿入位置を求める
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private int GetItemInsertIndex(DragEventArgs args)
        {
            if (_itemsControl == null) return -1;

            var cursor = args.GetPosition(_itemsControl);
            //Debug.WriteLine($"cursor: {cursor}");

            var count = _itemsControl.Items.Count;
            for (int index = 0; index < count; ++index)
            {
                var item = _itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as ContentPresenter;
                var center = item.TranslatePoint(new Point(0, item.ActualHeight), _itemsControl);

                //Debug.WriteLine($"{i}: {pos}: {item.ActualWidth}x{item.ActualHeight}");
                if (cursor.Y < center.Y)
                {
                    return index;
                }
            }

            return Math.Max(count, 0);
        }

        /// <summary>
        /// ドロップ受け入れ判定
        /// </summary>
        /// <param name="e"></param>
        private void Description_DragOver(object sender, DragEventArgs e)
        {
            if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
            {
                if (e.Data.GetDataPresent(typeof(LayoutPanel)))
                {
                    e.Effects = DragDropEffects.Move;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }
    }
}
