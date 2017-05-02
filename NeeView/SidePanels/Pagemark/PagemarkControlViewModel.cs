﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Input;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class PagemarkControlViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion


        // 項目変更イベント。フォーカス保存用
        public class SelectedItemChangeEventArgs
        {
            public bool IsFocused { get; set; }
        }
        public event EventHandler<SelectedItemChangeEventArgs> SelectedItemChanging;
        public event EventHandler<SelectedItemChangeEventArgs> SelectedItemChanged;


        public BookHub BookHub { get; private set; }

        public PagemarkCollection Pagemarks => ModelContext.Pagemarks;
        

        #region MoreMenu

        /// <summary>
        /// MoreMenu property.
        /// </summary>
        public ContextMenu MoreMenu
        {
            get { return _MoreMenu; }
            set { if (_MoreMenu != value) { _MoreMenu = value; RaisePropertyChanged(); } }
        }

        //
        private ContextMenu _MoreMenu;


        //
        private void InitializeMoreMenu()
        {
            var menu = new ContextMenu();
            //menu.Items.Add(CreateCommandMenuItem("", CommandType.ToggleVisiblePageList, vm));
            //menu.Items.Add(new Separator());
            menu.Items.Add(CreateListItemStyleMenuItem("一覧表示", PanelListItemStyle.Normal));
            menu.Items.Add(CreateListItemStyleMenuItem("コンテンツ表示", PanelListItemStyle.Content));
            //menu.Items.Add(CreateListItemStyleMenuItem("バナー表示", PanelListItemStyle.Banner));

            this.MoreMenu = menu;
        }

        //
        private MenuItem CreateCommandMenuItem(string header, CommandType command, object source)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = ModelContext.BookCommands[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            if (ModelContext.CommandTable[command].CreateIsCheckedBinding != null)
            {
                var binding = ModelContext.CommandTable[command].CreateIsCheckedBinding();
                binding.Source = source;
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }

            return item;
        }

        //
        private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = SetListItemStyle;
            item.CommandParameter = style;
            var binding = new Binding(nameof(PanelListItemStyle))
            {
                Converter = _PanelListItemStyleToBooleanConverter,
                ConverterParameter = style
            };
            item.SetBinding(MenuItem.IsCheckedProperty, binding);

            return item;
        }


        private PanelListItemStyleToBooleanConverter _PanelListItemStyleToBooleanConverter = new PanelListItemStyleToBooleanConverter();


        /// <summary>
        /// SetListItemStyle command.
        /// </summary>
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _SetListItemStyle = _SetListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        //
        private RelayCommand<PanelListItemStyle> _SetListItemStyle;

        //
        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            this.PanelListItemStyle = style;
        }


        /// <summary>
        /// PanelListItemStyle property.
        /// TODO: 保存されるものなのでモデル的なクラスでの実装が望ましい
        /// </summary>
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _PanelListItemStyle; }
            set
            {
                if (_PanelListItemStyle != value)
                {
                    _PanelListItemStyle = value;
                    ////this.FolderListView?.SetPanelListItemStyle(_PanelListItemStyle);
                    RaisePropertyChanged();
                }
            }
        }

        //
        private PanelListItemStyle _PanelListItemStyle;



        #endregion


        //
        public void Initialize(BookHub bookHub)
        {
            BookHub = bookHub;

            InitializeMoreMenu();
        }

        //
        public void Load(Pagemark mark)
        {
            BookHub?.RequestLoad(mark);
        }

        //
        public void Remove(Pagemark mark)
        {
            if (mark == null) return;

            var args = new SelectedItemChangeEventArgs();
            SelectedItemChanging?.Invoke(this, args);
            Pagemarks.SelectedItem = Pagemarks.GetNeighbor(mark);
            SelectedItemChanged?.Invoke(this, args);

            ModelContext.Pagemarks.Remove(mark);
            BookHub.UpdatePagemark(mark);
        }

        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            if (Pagemarks == null) return;
            if (PanelListItemStyle.HasThumbnail())
            {
                ThumbnailManager.Current.RequestThumbnail(Pagemarks.Marks, QueueElementPriority.PagemarkThumbnail, start, count, margin, direction);
            }
        }

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PanelListItemStyle = memento.PanelListItemStyle;
        }
        #endregion
    }
}
