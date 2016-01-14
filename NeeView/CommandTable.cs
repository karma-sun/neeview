// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// コマンド設定テーブル
    /// </summary>
    public class CommandTable : IEnumerable<KeyValuePair<CommandType, CommandElement>>
    {
        // インテグザ
        public CommandElement this[CommandType key]
        {
            get { return _Elements[key]; }
            set { _Elements[key] = value; }
        }

        // Enumerator
        public IEnumerator<KeyValuePair<CommandType, CommandElement>> GetEnumerator()
        {
            foreach (var pair in _Elements)
            {
                yield return pair;
            }
        }

        // Enumerator
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        // コマンドリスト
        private Dictionary<CommandType, CommandElement> _Elements;

        // コマンドターゲット
        private MainWindowVM _VM;
        private BookHub _Book;

        // 初期設定
        private static Memento _DefaultMemento;

        // 初期設定取得
        public static Memento CreateDefaultMemento()
        {
            return _DefaultMemento.Clone();
        }

        // コマンドターゲット設定
        public void SetTarget(MainWindowVM vm, BookHub book)
        {
            _VM = vm;
            _Book = book;
        }

        // コンストラクタ
        public CommandTable()
        {
            // コマンドの設定定義
            _Elements = new Dictionary<CommandType, CommandElement>
            {

                [CommandType.LoadAs] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "ファイルを開く",
                    ShortCutKey = "Ctrl+O",
                    IsShowMessage = false,
                },
                [CommandType.ClearHistory] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "履歴を消去",
                    Execute = e => _VM.ClearHistor()
                },

                [CommandType.ToggleStretchMode] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "表示サイズを切り替える",
                    ShortCutKey = "LeftButton+WheelDown",
                    Execute = e => _VM.StretchMode = _VM.StretchMode.GetToggle(),
                    ExecuteMessage = e => _VM.StretchMode.GetToggle().ToDispString()
                },
                [CommandType.SetStretchModeNone] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "オリジナルサイズで表示する",
                    Execute = e => _VM.StretchMode = PageStretchMode.None
                },
                [CommandType.SetStretchModeInside] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "大きい場合、ウィンドウサイズに合わせる",
                    Execute = e => _VM.StretchMode = PageStretchMode.Inside
                },
                [CommandType.SetStretchModeOutside] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "小さい場合、ウィンドウサイズに広げる",
                    Execute = e => _VM.StretchMode = PageStretchMode.Outside
                },
                [CommandType.SetStretchModeUniform] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "ウィンドウサイズに合わせる",
                    Execute = e => _VM.StretchMode = PageStretchMode.Uniform
                },
                [CommandType.SetStretchModeUniformToFill] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "ウィンドウいっぱいに広げる",
                    Execute = e => _VM.StretchMode = PageStretchMode.UniformToFill
                },

                [CommandType.ToggleFullScreen] = new CommandElement
                {
                    Group = "フルスクリーン",
                    Text = "フルスクリーン切り替え",
                    ShortCutKey = "F12",
                    MouseGesture = "U",
                    IsShowMessage = false,
                },
                [CommandType.SetFullScreen] = new CommandElement
                {
                    Group = "フルスクリーン",
                    Text = "フルスクリーンにする",
                    IsShowMessage = false,
                },
                [CommandType.CancelFullScreen] = new CommandElement
                {
                    Group = "フルスクリーン",
                    Text = "フルスクリーン解除",
                    ShortCutKey = "Escape",
                    IsShowMessage = false,
                },
                [CommandType.ToggleSlideShow] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "スライドショーON/OFF",
                    ShortCutKey = "F5",
                    Execute = e => _Book.ToggleSlideShow(),
                    ExecuteMessage = e => _Book.IsEnableSlideShow ? "スライドショー停止" : "スライドショー開始"
                },
                [CommandType.ViewScrollUp] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "スクロール↑",
                    ShortCutKey = "WheelUp",
                    IsShowMessage = false
                },
                [CommandType.ViewScrollDown] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "スクロール↓",
                    ShortCutKey = "WheelDown",
                    IsShowMessage = false
                },
                [CommandType.ViewScaleUp] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "拡大",
                    ShortCutKey = "RightButton+WheelUp",
                    IsShowMessage = false
                },
                [CommandType.ViewScaleDown] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "縮小",
                    ShortCutKey = "RightButton+WheelDown",
                    IsShowMessage = false
                },
                [CommandType.ViewRotateLeft] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "左回転",
                    IsShowMessage = false
                },
                [CommandType.ViewRotateRight] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "右回転",
                    IsShowMessage = false
                },

                [CommandType.PrevPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "前のページに戻る",
                    ShortCutKey = "Right,RightClick",
                    MouseGesture = "R",
                    IsShowMessage = false,
                    Execute = e => _Book.PrevPage(),
                },
                [CommandType.NextPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "次のページへ進む",
                    ShortCutKey = "Left,LeftClick",
                    MouseGesture = "L",
                    IsShowMessage = false,
                    Execute = e => _Book.NextPage(),
                },
                [CommandType.PrevOnePage] = new CommandElement
                {
                    Group = "移動",
                    Text = "1ページ戻る",
                    IsShowMessage = false,
                    Execute = e => _Book.PrevOnePage(),
                },
                [CommandType.NextOnePage] = new CommandElement
                {
                    Group = "移動",
                    Text = "1ページ進む",
                    IsShowMessage = false,
                    Execute = e => _Book.NextOnePage(),
                },
                [CommandType.FirstPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "最初のページに移動",
                    ShortCutKey = "Ctrl+Right",
                    MouseGesture = "UR",
                    Execute = e => _Book.FirstPage(),
                },
                [CommandType.LastPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "最後のページへ移動",
                    ShortCutKey = "Ctrl+Left",
                    MouseGesture = "UL",
                    Execute = e => _Book.LastPage(),
                },
                [CommandType.PrevFolder] = new CommandElement
                {
                    Group = "移動",
                    Text = "前のフォルダに移動",
                    ShortCutKey = "Up",
                    MouseGesture = "LU",
                    IsShowMessage = false,
                    Execute = e => _Book.PrevFolder(),
                },
                [CommandType.NextFolder] = new CommandElement
                {
                    Group = "移動",
                    Text = "次のフォルダへ移動",
                    ShortCutKey = "Down",
                    MouseGesture = "LD",
                    IsShowMessage = false,
                    Execute = e => _Book.NextFolder(),
                },

                [CommandType.ToggleFolderOrder] = new CommandElement
                {
                    Group = "フォルダ列",
                    Text = "フォルダの並び順を切り替える",
                    Execute = e => _Book.ToggleFolderOrder(),
                    ExecuteMessage = e => _Book.FolderOrder.GetToggle().ToDispString()
                },
                [CommandType.SetFolderOrderByFileName] = new CommandElement
                {
                    Group = "フォルダ列",
                    Text = "フォルダ列はファイル名順",
                    Execute = e => _Book.SetFolderOrder(FolderOrder.FileName)
                },
                [CommandType.SetFolderOrderByTimeStamp] = new CommandElement
                {
                    Group = "フォルダ列",
                    Text = "フォルダ列は日付順",
                    Execute = e => _Book.SetFolderOrder(FolderOrder.TimeStamp)
                },
                [CommandType.SetFolderOrderByRandom] = new CommandElement
                {
                    Group = "フォルダ列",
                    Text = "フォルダ列はランダム",
                    Execute = e => _Book.SetFolderOrder(FolderOrder.Random)
                },

                [CommandType.TogglePageMode] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "単ページ/見開き表示を切り替える",
                    ShortCutKey = "LeftButton+WheelUp",
                    Execute = e => _Book.TogglePageMode(),
                    ExecuteMessage = e => _Book.GetTogglePageMode() == 1 ? "単ページ表示" : "見開き表示"
                },
                [CommandType.SetPageMode1] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "単ページ表示にする",
                    ShortCutKey = "Ctrl+1",
                    Execute = e => _Book.SetPageMode(1)
                },
                [CommandType.SetPageMode2] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "見開き表示にする",
                    ShortCutKey = "Ctrl+2",
                    Execute = e => _Book.SetPageMode(2)
                },
                [CommandType.ToggleBookReadOrder] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "右開き、左開きを切り替える",
                    Execute = e => _Book.ToggleBookReadOrder(),
                    ExecuteMessage = e => _Book.BookMemento.BookReadOrder.GetToggle().ToDispString()
                },
                [CommandType.SetBookReadOrderRight] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "右開きにする",
                    Execute = e => _Book.SetBookReadOrder(PageReadOrder.RightToLeft),
                },
                [CommandType.SetBookReadOrderLeft] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "左開きにする",
                    Execute = e => _Book.SetBookReadOrder(PageReadOrder.LeftToRight),
                },

                [CommandType.ToggleIsSupportedWidePage] = new CommandElement
                {
                    Group = "見開き設定",
                    Text = "横長ページを見開きとみなす",
                    Execute = e => _Book.ToggleIsSupportedWidePage(),
                    ExecuteMessage = e => _Book.BookMemento.IsSupportedWidePage ? "横長ページの区別をしない" : "横長ページを見開きとみなす"
                },
                [CommandType.ToggleIsSupportedSingleFirstPage] = new CommandElement
                {
                    Group = "見開き設定",
                    Text = "最初のページを単ページ表示",
                    Execute = e => _Book.ToggleIsSupportedSingleFirstPage(),
                    ExecuteMessage = e => _Book.BookMemento.IsSupportedSingleFirstPage ? "最初のページを区別しない" : "最初のページを単ページ表示"
                },
                [CommandType.ToggleIsSupportedSingleLastPage] = new CommandElement
                {
                    Group = "見開き設定",
                    Text = "最後のページを単ページ表示",
                    Execute = e => _Book.ToggleIsSupportedSingleLastPage(),
                    ExecuteMessage = e => _Book.BookMemento.IsSupportedSingleLastPage ? "最後のページを区別しない" : "最後のページを単ページ表示"
                },

                [CommandType.ToggleIsRecursiveFolder] = new CommandElement
                {
                    Group = "フォルダ",
                    Text = "サブフォルダ読み込みON/OFF",
                    Execute = e => _Book.ToggleIsRecursiveFolder(),
                    ExecuteMessage = e => _Book.BookMemento.IsRecursiveFolder ? "サブフォルダは読み込まない" : "サブフォルダも読み込む"
                },

                [CommandType.ToggleSortMode] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ページの並び順を切り替える",
                    Execute = e => _Book.ToggleSortMode(),
                    ExecuteMessage = e => _Book.BookMemento.SortMode.GetToggle().ToDispString()
                },
                [CommandType.SetSortModeFileName] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ファイル名順にする",
                    Execute = e => _Book.SetSortMode(PageSortMode.FileName)
                },
                [CommandType.SetSortModeTimeStamp] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ファイル日付順にする",
                    Execute = e => _Book.SetSortMode(PageSortMode.TimeStamp)
                },
                [CommandType.SetSortModeRandom] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ランダムに並べる",
                    Execute = e => _Book.SetSortMode(PageSortMode.Random)
                },
                [CommandType.ToggleIsReverseSort] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "正順、逆順を切り替える",
                    Execute = e => _Book.ToggleIsReverseSort(),
                    ExecuteMessage = e => _Book.BookMemento.IsReverseSort ? "正順にする" : "逆順にする"
                },

                [CommandType.OpenSettingWindow] = new CommandElement
                {
                    Group = "設定",
                    Text = "設定ウィンドウを開く",
                    IsShowMessage = false,
                },
            };

            _DefaultMemento = CreateMemento();
        }


        #region Memento

        // 
        [DataContract]
        public class Memento
        {
            [DataMember]
            public Dictionary<CommandType, CommandElement.Memento> Elements { get; set; }

            public CommandElement.Memento this[CommandType type]
            {
                get { return Elements[type]; }
                set { Elements[type] = value; }
            }

            //
            private void Constructor()
            {
                Elements = new Dictionary<CommandType, CommandElement.Memento>();
            }

            //
            public Memento()
            {
                Constructor();
            }

            //
            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            //
            public Memento Clone()
            {
                var memento = new Memento();
                foreach (var pair in Elements)
                {
                    memento.Elements.Add(pair.Key, pair.Value.Clone());
                }
                return memento;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            foreach (var pair in _Elements)
            {
                memento.Elements.Add(pair.Key, pair.Value.CreateMemento());
            }

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            foreach (var pair in memento.Elements)
            {
                if (_Elements.ContainsKey(pair.Key))
                {
                    _Elements[pair.Key].Restore(pair.Value);
                }
            }
        }

        #endregion
    }
}
