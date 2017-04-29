﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// SidePanel.
    /// パネル集合と選択されたパネルの管理
    /// </summary>
    public class SidePanel : INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        /// <summary>
        /// 選択変更通知
        /// </summary>
        public event EventHandler SelectedPanelChanged;


        /// <summary>
        /// Panels property.
        /// </summary>
        private ObservableCollection<IPanel> _panels;
        public ObservableCollection<IPanel> Panels
        {
            get { return _panels; }
            set { if (_panels != value) { _panels = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// SelectedPanel property.
        /// </summary>
        private IPanel _selectedPanel;
        public IPanel SelectedPanel
        {
            get { return _selectedPanel; }
            set { if (_selectedPanel != value) { _selectedPanel = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Width property.
        /// </summary>
        private double _width = 250.0;
        public double Width
        {
            get { return _width; }
            set { if (_width != value) { _width = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// パネル自体の表示状態。
        /// ビューから更新される
        /// </summary>
        public bool IsVisible { get; set; }


        /// <summary>
        /// constructor
        /// </summary>
        public SidePanel()
        {
            _panels = new ObservableCollection<IPanel>();
        }

        /// <summary>
        /// パネル存在チェック
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        public bool Contains(IPanel panel)
        {
            return _panels.Contains(panel);
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
        public void SetSelectedPanel(IPanel panel, bool isSelected)
        {
            SelectedPanel = isSelected ? panel : SelectedPanel != panel ? SelectedPanel : null;
            SelectedPanelChanged?.Invoke(this, null);
        }

        /// <summary>
        /// パネル選択をトグル。
        /// 非表示の場合は入れ替えよりも表示させることを優先する
        /// </summary>
        /// <param name="panel">パネル</param>
        /// <param name="force">表示状態にかかわらず切り替える</param>
        public void ToggleSelectedPanel(IPanel panel, bool force)
        {
            if (force || SelectedPanel != panel)
            {
                SelectedPanel = SelectedPanel != panel ? panel : null;
                SelectedPanelChanged?.Invoke(this, null);
            }
            else
            {
                if (IsVisible)
                {
                    SelectedPanel = null;
                }
                else
                {
                    SelectedPanelChanged?.Invoke(this, null);
                }
            }
        }

        /// <summary>
        /// Toggle.
        /// アイコンボダンによる切り替え
        /// </summary>
        /// <param name="content"></param>
        public void Toggle(IPanel content)
        {
            if (content != null && _panels.Contains(content))
            {
                SelectedPanel = SelectedPanel != content ? content : null;
            }
        }

        /// <summary>
        /// パネル削除
        /// </summary>
        /// <param name="panel"></param>
        public void Remove(IPanel panel)
        {
            Panels.Remove(panel);
            if (SelectedPanel == panel) SelectedPanel = null;
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


        //
        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public List<string> PanelTypeCodes { get; set; }

            [DataMember]
            public string SelectedPanelTypeCode { get; set; }

            [DataMember]
            public double Width { get; set; }

            /// <summary>
            /// constructor
            /// </summary>
            private void Constructor()
            {
                PanelTypeCodes = new List<string>();
                Width = 250.0;
            }

            /// <summary>
            /// constructor
            /// </summary>
            public Memento()
            {
                Constructor();
            }

            /// <summary>
            /// デシリアイズ前処理
            /// </summary>
            /// <param name="c"></param>
            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }
        }

        /// <summary>
        /// Memento作成
        /// </summary>
        /// <returns></returns>
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.PanelTypeCodes = Panels.Select(e => e.TypeCode).ToList();
            memento.SelectedPanelTypeCode = SelectedPanel?.TypeCode;
            memento.Width = Width;

            return memento;
        }

        /// <summary>
        /// Memento適用
        /// </summary>
        /// <param name="memento"></param>
        /// <param name="panels"></param>
        public void Restore(Memento memento, List<IPanel> panels)
        {
            if (memento == null) return;

            Panels = new ObservableCollection<IPanel>(memento.PanelTypeCodes.Select(e => panels.FirstOrDefault(panel => panel.TypeCode == e)).Where(e => e != null));
            SelectedPanel = Panels.FirstOrDefault(e => e.TypeCode == memento.SelectedPanelTypeCode);
            Width = memento.Width;
        }

        #endregion

    }
}