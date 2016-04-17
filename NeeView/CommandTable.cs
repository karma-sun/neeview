// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            get
            {
                if (!_Elements.ContainsKey(key)) throw new ArgumentOutOfRangeException(key.ToString());
                return _Elements[key];
            }
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

                [CommandType.ReLoad] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "再読み込み",
                    MouseGesture = "UD",
                    Execute = e => _Book.ReLoad(false),
                    IsShowMessage = false,
                },

                [CommandType.OpenApplication] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "外部アプリで開く",
                    Execute = e => _Book.OpenApplication(),
                    CanExecute = () => _Book.CanOpenFilePlace(),
                    IsShowMessage = false
                },
                [CommandType.OpenFilePlace] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "エクスプローラーで開く",
                    Execute = e => _Book.OpenFilePlace(),
                    CanExecute = () => _Book.CanOpenFilePlace(),
                    IsShowMessage = false
                },
                [CommandType.Export] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "名前をつけてファイルに保存",
                    ShortCutKey = "Ctrl+S",
                    Execute = e => _Book.Export(),
                    CanExecute = () => _Book.CanOpenFilePlace(),
                    IsShowMessage = false
                },
                [CommandType.DeleteFile] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "ファイルを削除する",
                    ShortCutKey = "Delete",
                    Execute = e => _Book.DeleteFile(),
                    CanExecute = () => _Book.CanDeleteFile(),
                    IsShowMessage = false
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
                [CommandType.ToggleStretchModeReverse] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "表示サイズを切り替える(逆方向)",
                    Execute = e => _VM.StretchMode = _VM.StretchMode.GetToggleReverse(),
                    ExecuteMessage = e => _VM.StretchMode.GetToggleReverse().ToDispString()
                },
                [CommandType.SetStretchModeNone] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "オリジナルサイズで表示する",
                    Execute = e => _VM.StretchMode = PageStretchMode.None,
                    Attribute = CommandAttribute.ToggleEditable | CommandAttribute.ToggleLocked,
                },
                [CommandType.SetStretchModeInside] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "大きい場合ウィンドウサイズに合わせる",
                    Execute = e => _VM.StretchMode = PageStretchMode.Inside,
                    Attribute = CommandAttribute.ToggleEditable,
                },
                [CommandType.SetStretchModeOutside] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "小さい場合ウィンドウサイズに広げる",
                    Execute = e => _VM.StretchMode = PageStretchMode.Outside,
                    Attribute = CommandAttribute.ToggleEditable,
                },
                [CommandType.SetStretchModeUniform] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "ウィンドウサイズに合わせる",
                    Execute = e => _VM.StretchMode = PageStretchMode.Uniform,
                    Attribute = CommandAttribute.ToggleEditable,
                },
                [CommandType.SetStretchModeUniformToFill] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "ウィンドウいっぱいに広げる",
                    Execute = e => _VM.StretchMode = PageStretchMode.UniformToFill,
                    Attribute = CommandAttribute.ToggleEditable,
                },
                [CommandType.SetStretchModeUniformToSize] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "面積をウィンドウに合わせる",
                    Execute = e => _VM.StretchMode = PageStretchMode.UniformToSize,
                    Attribute = CommandAttribute.ToggleEditable,
                },
                [CommandType.SetStretchModeUniformToVertical] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "高さをウィンドウに合わせる",
                    Execute = e => _VM.StretchMode = PageStretchMode.UniformToVertical,
                    Attribute = CommandAttribute.ToggleEditable,
                },

                [CommandType.ToggleIsEnabledNearestNeighbor] = new CommandElement
                {
                    Group = "拡大モード",
                    Text = "ドットのまま拡大ON/OFF",
                    Execute = e => _VM.IsEnabledNearestNeighbor = !_VM.IsEnabledNearestNeighbor,
                    ExecuteMessage = e => _VM.IsEnabledNearestNeighbor ? "高品質に拡大する" : "ドットのまま拡大する"

                },

                [CommandType.ToggleBackground] = new CommandElement
                {
                    Group = "背景",
                    Text = "背景を切り替える",
                    Execute = e => _VM.Background = _VM.Background.GetToggle(),
                    ExecuteMessage = e => _VM.Background.GetToggle().ToDispString(),
                },

                [CommandType.SetBackgroundBlack] = new CommandElement
                {
                    Group = "背景",
                    Text = "背景を黒色にする",
                    Execute = e => _VM.Background = BackgroundStyle.Black,
                },

                [CommandType.SetBackgroundWhite] = new CommandElement
                {
                    Group = "背景",
                    Text = "背景を白色にする",
                    Execute = e => _VM.Background = BackgroundStyle.White,
                },

                [CommandType.SetBackgroundAuto] = new CommandElement
                {
                    Group = "背景",
                    Text = "背景を画像に合わせた色にする",
                    Execute = e => _VM.Background = BackgroundStyle.Auto,
                },

                [CommandType.SetBackgroundCheck] = new CommandElement
                {
                    Group = "背景",
                    Text = "背景をチェック模様にする",
                    Execute = e => _VM.Background = BackgroundStyle.Check,
                },

                [CommandType.ToggleTopmost] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "常に手前に表示ON/OFF",
                    Execute = e => _VM.ToggleTopmost(),
                    ExecuteMessage = e => _VM.IsTopmost ? "「常に手前に表示」を解除" : "常に手前に表示する",
                    CanExecute = () => true,
                },
                [CommandType.ToggleHideMenu] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "メニューを自動的に隠すON/OFF",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleHideMenu(),
                    ExecuteMessage = e => _VM.IsHideMenu ? "メニューを表示する" : "メニューを自動的に隠す",
                    CanExecute = () => true,
                },
                [CommandType.ToggleHideTitleBar] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "タイトルバーを消すON/OFF",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleHideTitleBar(),
                    ExecuteMessage = e => _VM.IsHideTitleBar ? "タイトルバーを表示する" : "タイトルバーを消す",
                    CanExecute = () => true,
                },
                [CommandType.ToggleVisibleFileInfo] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "ファイル情報の表示ON/OFF",
                    ShortCutKey = "I",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleVisibleFileInfo(),
                    ExecuteMessage = e => _VM.IsVisibleFileInfo ? "ファイル情報を消す" : "ファイル情報を表示する",
                    CanExecute = () => true,
                },
                [CommandType.ToggleVisibleFolderList] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "フォルダーリストの表示ON/OFF",
                    ShortCutKey = "F",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleVisibleFolderList(),
                    ExecuteMessage = e => _VM.IsVisibleFolderList ? "フォルダーリストを消す" : "フォルダーリストを表示する",
                    CanExecute = () => true,
                },
                [CommandType.ToggleVisibleHistoryList] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "履歴の表示ON/OFF",
                    ShortCutKey = "H",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleVisibleHistoryList(),
                    ExecuteMessage = e => _VM.IsVisibleHistoryList ? "履歴リストを消す" : "履歴リストを表示する",
                    CanExecute = () => true,
                },


                [CommandType.ToggleFullScreen] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "フルスクリーン切り替え",
                    ShortCutKey = "F11",
                    MouseGesture = "U",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleFullScreen(),
                    CanExecute = () => true,
                },
                [CommandType.SetFullScreen] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "フルスクリーンにする",
                    IsShowMessage = false,
                    Execute = e => _VM.IsFullScreen = true,
                    CanExecute = () => true,
                },
                [CommandType.CancelFullScreen] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "フルスクリーン解除",
                    ShortCutKey = "Escape",
                    IsShowMessage = false,
                    Execute = e => _VM.IsFullScreen = false,
                    CanExecute = () => true,
                },

                [CommandType.ToggleSlideShow] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "スライドショー再生/停止",
                    ShortCutKey = "F5",
                    Execute = e => _Book.ToggleSlideShow(),
                    ExecuteMessage = e => _Book.IsEnableSlideShow ? "スライドショー停止" : "スライドショー再生"
                },
                [CommandType.ViewScrollUp] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "スクロール↑",
                    Tips = "縦スクロールできないときは横スクロールになります",
                    ShortCutKey = "WheelUp",
                    IsShowMessage = false
                },
                [CommandType.ViewScrollDown] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "スクロール↓",
                    Tips = "縦スクロールできないときは横スクロールになります",
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
                [CommandType.ToggleViewFlipHorizontal] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "左右反転の切り替え",
                    IsShowMessage = false
                },
                [CommandType.ViewFlipHorizontalOn] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "左右反転ON",
                    IsShowMessage = false
                },
                [CommandType.ViewFlipHorizontalOff] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "左右反転OFF",
                    IsShowMessage = false
                },

                [CommandType.ViewReset] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "ビューリセット",
                    Tips = "ビュー操作での回転、拡縮、移動を初期化する",
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
                [CommandType.PrevScrollPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "スクロール＋前のページに戻る",
                    Tips = "前ページ方向にスクロールする\nスクロールできないときは前のページに戻る",
                    IsShowMessage = false,
                },
                [CommandType.NextScrollPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "スクロール＋次のページへ進む",
                    Tips = "次ページ方向にスクロールする\nスクロールできないときは次のページへ進む",
                    IsShowMessage = false,
                },
                [CommandType.MovePageWithCursor] = new CommandElement
                {
                    Group = "移動",
                    Text = "マウス位置依存でページを前後させる",
                    Tips = "左にカーソルがあるときは次のページへ進む\n右にカーソルがあるときは前のページに戻る",
                    IsShowMessage = false,
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
                    Tips = "フォルダ列をファイル名順(昇順)に並べる",
                    Execute = e => _Book.SetFolderOrder(FolderOrder.FileName)
                },
                [CommandType.SetFolderOrderByTimeStamp] = new CommandElement
                {
                    Group = "フォルダ列",
                    Text = "フォルダ列は日付順",
                    Tips = "フォルダ列を日付順(降順)に並べる",
                    Execute = e => _Book.SetFolderOrder(FolderOrder.TimeStamp)
                },
                [CommandType.SetFolderOrderByRandom] = new CommandElement
                {
                    Group = "フォルダ列",
                    Text = "フォルダ列はランダム",
                    Tips = "フォルダ列をランダムに並べる",
                    Execute = e => _Book.SetFolderOrder(FolderOrder.Random)
                },

                [CommandType.TogglePageMode] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "ページ表示モードを切り替える",
                    ShortCutKey = "LeftButton+WheelUp",
                    Execute = e => _Book.TogglePageMode(),
                    ExecuteMessage = e => _Book.BookMemento.PageMode.GetToggle().ToDispString(),
                },
                [CommandType.SetPageMode1] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "1ページ表示にする",
                    ShortCutKey = "Ctrl+1",
                    Execute = e => _Book.SetPageMode(PageMode.SinglePage)
                },
                [CommandType.SetPageMode2] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "2ページ表示にする",
                    ShortCutKey = "Ctrl+2",
                    Execute = e => _Book.SetPageMode(PageMode.WidePage)
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

                [CommandType.ToggleIsSupportedDividePage] = new CommandElement
                {
                    Group = "1ページ表示設定",
                    Text = "横長ページを分割する",
                    Execute = e => _Book.ToggleIsSupportedDividePage(),
                    ExecuteMessage = e => _Book.BookMemento.IsSupportedDividePage ? "横長ページの区別をしない" : "横長ページを分割する"
                },

                [CommandType.ToggleIsSupportedWidePage] = new CommandElement
                {
                    Group = "2ページ表示設定",
                    Text = "横長ページを2ページとみなす",
                    Execute = e => _Book.ToggleIsSupportedWidePage(),
                    ExecuteMessage = e => _Book.BookMemento.IsSupportedWidePage ? "横長ページの区別をしない" : "横長ページを2ページとみなす"
                },
                [CommandType.ToggleIsSupportedSingleFirstPage] = new CommandElement
                {
                    Group = "2ページ表示設定",
                    Text = "最初のページを単独表示",
                    Execute = e => _Book.ToggleIsSupportedSingleFirstPage(),
                    ExecuteMessage = e => _Book.BookMemento.IsSupportedSingleFirstPage ? "最初のページを区別しない" : "最初のページを単独表示"
                },
                [CommandType.ToggleIsSupportedSingleLastPage] = new CommandElement
                {
                    Group = "2ページ表示設定",
                    Text = "最後のページを単独表示",
                    Execute = e => _Book.ToggleIsSupportedSingleLastPage(),
                    ExecuteMessage = e => _Book.BookMemento.IsSupportedSingleLastPage ? "最後のページを区別しない" : "最後のページを単独表示"
                },

                [CommandType.ToggleIsRecursiveFolder] = new CommandElement
                {
                    Group = "ページ読込",
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
                    Text = "ファイル名昇順にする",
                    Execute = e => _Book.SetSortMode(PageSortMode.FileName)
                },
                [CommandType.SetSortModeFileNameDescending] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ファイル名降順にする",
                    Execute = e => _Book.SetSortMode(PageSortMode.FileNameDescending)
                },
                [CommandType.SetSortModeTimeStamp] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ファイル日付昇順にする",
                    Execute = e => _Book.SetSortMode(PageSortMode.TimeStamp)
                },
                [CommandType.SetSortModeTimeStampDescending] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ファイル日付降順にする",
                    Execute = e => _Book.SetSortMode(PageSortMode.TimeStampDescending)
                },
                [CommandType.SetSortModeRandom] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ランダムに並べる",
                    Execute = e => _Book.SetSortMode(PageSortMode.Random)
                },

                [CommandType.ToggleIsReverseSort] = new CommandElement // 欠番
                {
                    Group = "dummy",
                    Text = "dummy",
                    Execute = e => { return; }
                },

                [CommandType.OpenSettingWindow] = new CommandElement
                {
                    Group = "その他",
                    Text = "設定ウィンドウを開く",
                    IsShowMessage = false,
                },
                [CommandType.OpenVersionWindow] = new CommandElement
                {
                    Group = "その他",
                    Text = "バージョン情報を表示する",
                    IsShowMessage = false,
                },
                [CommandType.CloseApplication] = new CommandElement
                {
                    Group = "その他",
                    Text = "アプリを終了する",
                    ShortCutKey = "Alt+F4",
                    IsShowMessage = false,
                    CanExecute = () => true,
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
