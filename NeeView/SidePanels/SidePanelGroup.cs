using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    public class SelectedPanelChangedEventArgs : EventArgs
    {
        public SelectedPanelChangedEventArgs(IPanel selectedPanel)
        {
            this.SelectedPanel = selectedPanel;
        }

        public IPanel SelectedPanel { get; set; }
    }

    /// <summary>
    /// SidePanel.
    /// パネル集合と選択されたパネルの管理
    /// </summary>
    public class SidePanelGroup : BindableBase
    {
        private IPanel _selectedPanel;
        private IPanel _lastSelectedPane;
        private double _width = 300.0;
        private bool _isVisible = true;


        public SidePanelGroup()
        {
            Panels.CollectionChanged += Panels_CollectionChanged;
        }


        /// <summary>
        /// 選択変更通知
        /// </summary>
        public event EventHandler<SelectedPanelChangedEventArgs> SelectedPanelChanged;

        /// <summary>
        /// 管理下のパネル集合
        /// </summary>
        public ObservableCollection<IPanel> Panels { get; } = new ObservableCollection<IPanel>();


        /// <summary>
        /// 選択中のパネル
        /// </summary>
        public IPanel SelectedPanel
        {
            get { return _selectedPanel; }
            private set
            {
                if (_selectedPanel != value)
                {
                    if (_selectedPanel != null)
                    {
                        _selectedPanel.IsVisibleLockChanged -= SelectedPanel_IsVisibleLockChanged;
                        _selectedPanel.IsVisible = false;
                    }

                    _selectedPanel = value;
                    RaisePropertyChanged();

                    if (_selectedPanel != null)
                    {
                        _selectedPanel.IsVisibleLockChanged += SelectedPanel_IsVisibleLockChanged;
                        _selectedPanel.IsVisible = IsVisible;
                        _lastSelectedPane = _selectedPanel;
                    }
                }
                // 本棚の各要素を表示する用途のため、変更にかかわらず通知
                SelectedPanelChanged?.Invoke(this, new SelectedPanelChangedEventArgs(_selectedPanel));

                void SelectedPanel_IsVisibleLockChanged(object sender, EventArgs e)
                {
                    RaisePropertyChanged(nameof(IsVisibleLocked));
                }
            }
        }

        /// <summary>
        /// パネル表示ロックフラグの参照
        /// </summary>
        public bool IsVisibleLocked => SelectedPanel?.IsVisibleLock == true;

        /// <summary>
        /// 最新の有効な選択パネル
        /// </summary>
        public IPanel LastSelectedPanel
        {
            get { return Panels.Contains(_lastSelectedPane) ? _lastSelectedPane : Panels.FirstOrDefault(); }
        }

        /// <summary>
        /// Width property.
        /// </summary>
        public double Width
        {
            get { return _width; }
            set { if (_width != value) { _width = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// パネル自体の表示状態。自動非表示機能等で変化する
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (SetProperty(ref _isVisible, value))
                {
                    if (SelectedPanel != null)
                    {
                        SelectedPanel.IsVisible = _isVisible;
                    }
                }
            }
        }



        /// <summary>
        /// 管理パネル初期化
        /// </summary>
        public void Initialize(IEnumerable<IPanel> panels, string selectedPanelTypeCode, double width)
        {
            Panels.Clear();
            foreach (var panel in panels)
            {
                Panels.Add(panel);
            }
            var selectedPanel = Panels.FirstOrDefault(e => e.TypeCode == selectedPanelTypeCode);
            if (selectedPanel != null)
            {
                selectedPanel.IsSelected = true;
            }

            Width = width;
        }

        /// <summary>
        /// パネルのIsSelectedプロパティの変更イベント処理
        /// </summary>
        public void Panel_IsSelectedChanged(object sender, EventArgs e)
        {
            var panel = sender as IPanel;
            Debug.Assert(panel != null);
            if (panel == null) return;

            if (Panels.Contains(panel))
            {
                if (panel.IsSelected)
                {
                    SelectedPanel = panel;

                    foreach (var other in Panels.Where(p => p != panel))
                    {
                        other.IsSelected = false;
                    }
                }
                else if (panel == SelectedPanel)
                {
                    SelectedPanel = null;
                }
            }
            else
            {
                if (SelectedPanel == panel)
                {
                    SelectedPanel = null;
                }
            }
        }

        /// <summary>
        /// パネル集合のコレクション変更イベント処理。追加削除でのパネルの初期化を行う
        /// </summary>
        private void Panels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                return;
            }

            if (e.OldItems != null)
            {
                foreach (IPanel panel in e.OldItems)
                {
                    panel.IsSelected = false;
                    panel.IsVisible = false;
                    panel.IsSelectedChanged -= Panel_IsSelectedChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (IPanel panel in e.NewItems)
                {
                    panel.IsSelected = false;
                    panel.IsVisible = false;
                    panel.IsSelectedChanged += Panel_IsSelectedChanged;
                }
            }
        }

        /// <summary>
        /// パネル存在チェック
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        public bool Contains(IPanel panel)
        {
            return Panels.Contains(panel);
        }

        /// <summary>
        /// パネル表示状態を判定。
        /// </summary>
        /// <param name="panel">パネル</param>
        /// <returns></returns>
        public bool IsVisiblePanel(IPanel panel)
        {
            return IsVisible && SelectedPanel == panel;
        }

        /// <summary>
        /// パネル選択を設定
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="isSelected"></param>
        public void SetSelectedPanel(IPanel panel, bool isSelected, bool flush)
        {
            Debug.Assert(Panels.Contains(panel));

            if (panel.IsSelected != isSelected)
            {
                panel.IsSelected = isSelected;
            }
            else if (flush && isSelected)
            {
                // 選択が変更されたことにして、自動非表示の表示状態更新を要求する
                SelectedPanelChanged?.Invoke(this, new SelectedPanelChangedEventArgs(SelectedPanel));
            }
        }

        /// <summary>
        /// パネル選択をトグル。
        /// 非表示の場合は入れ替えよりも表示させることを優先する
        /// </summary>
        /// <param name="panel">パネル</param>
        /// <param name="force">表示状態にかかわらず切り替える</param>
        public void ToggleSelectedPanel(IPanel panel, bool force)
        {
            Debug.Assert(Panels.Contains(panel));

            if (force || !panel.IsSelected)
            {
                panel.IsSelected = !panel.IsSelected;
            }
            else
            {
                if (IsVisible)
                {
                    panel.IsSelected = false;
                }
                else
                {
                    // 選択が変更されたことにして、自動非表示の表示状態更新を要求する
                    SelectedPanelChanged?.Invoke(this, new SelectedPanelChangedEventArgs(SelectedPanel));
                }
            }
        }

        /// <summary>
        /// すべてのパネルを閉じる
        /// </summary>
        public void CloseAllPanels()
        {
            foreach (var panel in Panels)
            {
                panel.IsSelected = false;
            }
        }

        /// <summary>
        /// Toggle.
        /// アイコンボダンによる切り替え
        /// </summary>
        /// <param name="panel"></param>
        public void Toggle(IPanel panel)
        {
            if (panel != null && Panels.Contains(panel))
            {
                panel.IsSelected = !panel.IsSelected;
            }
        }

        /// <summary>
        /// Toggle.
        /// 余白クリック時の切り替え
        /// </summary>
        public void Toggle()
        {
            var panel = SelectedPanel ?? LastSelectedPanel;
            if (panel != null)
            {
                panel.IsSelected = !panel.IsSelected;
            }
        }

        /// <summary>
        /// パネル削除
        /// </summary>
        /// <param name="panel"></param>
        public void Remove(IPanel panel)
        {
            Panels.Remove(panel);
        }

        /// <summary>
        /// パネル追加
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="index"></param>
        public void Add(IPanel panel, int index)
        {
            if (Panels.Contains(panel))
            {
                var current = Panels.IndexOf(panel);
                Panels.Move(current, Math.Min(index, Panels.Count - 1));
            }
            else
            {
                Panels.Insert(index, panel);
            }
        }

        public void Refresh()
        {
            foreach (var panel in Panels)
            {
                panel.Refresh();
            }
        }

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public List<string> PanelTypeCodes { get; set; }

            [DataMember]
            public string SelectedPanelTypeCode { get; set; }

            [DataMember]
            public double Width { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.PanelTypeCodes = Panels.Select(e => e.TypeCode).ToList();
            memento.SelectedPanelTypeCode = SelectedPanel?.TypeCode;
            memento.Width = Width;

            return memento;
        }

        #endregion

    }
}
