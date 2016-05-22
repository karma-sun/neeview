// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;

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


        // コマンドリストをブラウザで開く
        public void OpenCommandListHelp()
        {
            // グループ分け
            var groups = new Dictionary<string, List<CommandElement>>();
            foreach (var command in _Elements.Values)
            {
                if (command.Group == "dummy") continue;

                if (!groups.ContainsKey(command.Group))
                {
                    groups.Add(command.Group, new List<CommandElement>());
                }

                groups[command.Group].Add(command);
            }


            // 
            Directory.CreateDirectory(Temporary.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.TempSystemDirectory, "CommandList.html");

            //
            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.WriteLine(NVUtility.HtmlHelpHeader("NeeView Command List"));
                writer.WriteLine("<body><h1>NeeView コマンド一覧</h1>");
                // グループごとに出力
                foreach (var pair in groups)
                {
                    writer.WriteLine($"<h3>{pair.Key}</h3>");
                    writer.WriteLine("<table>");
                    writer.WriteLine($"<th>コマンド<th>ショートカットキー<th>マウスゼスチャー<th>説明<tr>");
                    foreach (var command in pair.Value)
                    {
                        writer.WriteLine($"<td>{command.Text}<td>{command.ShortCutKey}<td>{new MouseGestureSequence(command.MouseGesture).ToDispString()}<td>{command.Note}<tr>");
                    }
                    writer.WriteLine("</table>");
                }
                writer.WriteLine("</body>");

                writer.WriteLine(NVUtility.HtmlHelpFooter());
            }

            System.Diagnostics.Process.Start(fileName);
        }


        // コンストラクタ
        public CommandTable()
        {
            // コマンドの設定定義
            _Elements = new Dictionary<CommandType, CommandElement>
            {
                [CommandType.None] = new CommandElement // 欠番
                {
                    Group = "dummy",
                    Text = "dummy",
                    Execute = e => { return; }
                },

                [CommandType.LoadAs] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "ファイルを開く",
                    MenuText = "開く...",
                    Note = "圧縮ファイルか画像ファイルを選択して開きます",
                    ShortCutKey = "Ctrl+O",
                    IsShowMessage = false,
                },

                [CommandType.ReLoad] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "再読み込み",
                    Note = "フォルダーを再読み込みします",
                    MouseGesture = "UD",
                    CanExecute = () => _Book.CanReload(),
                    Execute = e => _Book.ReLoad(),
                    IsShowMessage = false,
                },

                [CommandType.OpenApplication] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "外部アプリで開く",
                    Note = "表示されている画像を外部アプリで開きます。設定ウィンドウの<code>外部連携</code>でアプリを設定します",
                    Execute = e => _Book.OpenApplication(),
                    CanExecute = () => _Book.CanOpenFilePlace(),
                    IsShowMessage = false
                },
                [CommandType.OpenFilePlace] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "エクスプローラーで開く",
                    Note = "表示しているページのファイルをエクスプローラーで開きます",
                    Execute = e => _Book.OpenFilePlace(),
                    CanExecute = () => _Book.CanOpenFilePlace(),
                    IsShowMessage = false
                },
                [CommandType.Export] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "名前をつけてファイルに保存",
                    MenuText = "保存...",
                    Note = "画像をファイルに保存します",
                    ShortCutKey = "Ctrl+S",
                    Execute = e => _Book.Export(),
                    CanExecute = () => _Book.CanOpenFilePlace(),
                    IsShowMessage = false
                },
                [CommandType.DeleteFile] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "ファイルを削除",
                    MenuText = "削除...",
                    Note = "ファイルを削除します。圧縮ファイルの場合は削除できません ",
                    ShortCutKey = "Delete",
                    Execute = e => _Book.DeleteFile(),
                    CanExecute = () => _Book.CanDeleteFile(),
                    IsShowMessage = false
                },
                [CommandType.CopyFile] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "ファイルをコピー",
                    MenuText = "コピー",
                    Note = "ファイルをクリップボードにコピーします",
                    ShortCutKey = "Ctrl+C",
                    Execute = e => _Book.CopyToClipboard(),
                    CanExecute = () => _Book.CanOpenFilePlace(),
                    IsShowMessage = true
                },
                [CommandType.CopyImage] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "画像をコピー",
                    MenuText = "画像コピー",
                    Note = "画像をクリップボードにコピーします。2ページ表示の場合はメインとなるページのみコピーします",
                    ShortCutKey = "Ctrl+Shift+C",
                    Execute = e => _VM.CopyImageToClipboard(),
                    CanExecute = () => _VM.CanCopyImageToClipboard(),
                    IsShowMessage = true
                },
                [CommandType.Paste] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "貼り付け",
                    MenuText = "貼り付け",
                    Note = "クリップボードのファイルや画像を貼り付けます",
                    ShortCutKey = "Ctrl+V",
                    IsShowMessage = false
                },


                [CommandType.ClearHistory] = new CommandElement
                {
                    Group = "ファイル",
                    Text = "履歴を消去",
                    Note = "履歴を全て削除します",
                    Execute = e => _VM.ClearHistor()
                },

                [CommandType.ToggleStretchMode] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "表示サイズを切り替える",
                    Note = "画像の表示サイズを順番に切り替えます",
                    ShortCutKey = "LeftButton+WheelDown",
                    Execute = e => _VM.StretchMode = _VM.StretchMode.GetToggle(),
                    ExecuteMessage = e => _VM.StretchMode.GetToggle().ToDispString()
                },
                [CommandType.ToggleStretchModeReverse] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "表示サイズを切り替える(逆順)",
                    Note = "画像の表示サイズを順番に切り替えます(逆順)",
                    ShortCutKey = "LeftButton+WheelUp",
                    Execute = e => _VM.StretchMode = _VM.StretchMode.GetToggleReverse(),
                    ExecuteMessage = e => _VM.StretchMode.GetToggleReverse().ToDispString()
                },
                [CommandType.SetStretchModeNone] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "オリジナルサイズ",
                    Note = "画像のサイズそのままで表示します",
                    Execute = e => _VM.StretchMode = PageStretchMode.None,
                    Attribute = CommandAttribute.ToggleEditable | CommandAttribute.ToggleLocked,
                    CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.None),
                },
                [CommandType.SetStretchModeInside] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "大きい場合ウィンドウサイズに合わせる",
                    Note = "ウィンドウに収まるように画像を縮小して表示します",
                    Execute = e => _VM.StretchMode = PageStretchMode.Inside,
                    Attribute = CommandAttribute.ToggleEditable,
                    CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.Inside),
                },
                [CommandType.SetStretchModeOutside] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "小さい場合ウィンドウサイズに広げる",
                    Note = "ウィンドウに収まるように画像をできるだけ拡大して表示します",
                    Execute = e => _VM.StretchMode = PageStretchMode.Outside,
                    Attribute = CommandAttribute.ToggleEditable,
                    CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.Outside),
                },
                [CommandType.SetStretchModeUniform] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "ウィンドウサイズに合わせる",
                    Note = "画像をウィンドウサイズに合わせるよう拡大縮小します",
                    Execute = e => _VM.StretchMode = PageStretchMode.Uniform,
                    Attribute = CommandAttribute.ToggleEditable,
                    CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.Uniform),
                },
                [CommandType.SetStretchModeUniformToFill] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "ウィンドウいっぱいに広げる",
                    Note = "縦横どちらかをウィンドウサイズに合わせるように拡大縮小します。画像はウィンドウサイズより大きくなります",
                    Execute = e => _VM.StretchMode = PageStretchMode.UniformToFill,
                    Attribute = CommandAttribute.ToggleEditable,
                    CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.UniformToFill),
                },
                [CommandType.SetStretchModeUniformToSize] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "面積をウィンドウに合わせる",
                    Note = "ウィンドウの面積と等しくなるように画像を拡大縮小します",
                    Execute = e => _VM.StretchMode = PageStretchMode.UniformToSize,
                    Attribute = CommandAttribute.ToggleEditable,
                    CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.UniformToSize),
                },
                [CommandType.SetStretchModeUniformToVertical] = new CommandElement
                {
                    Group = "表示サイズ",
                    Text = "高さをウィンドウに合わせる",
                    Note = "ウィンドウの高さに画像の高さを合わせるように拡大縮小します",
                    Execute = e => _VM.StretchMode = PageStretchMode.UniformToVertical,
                    Attribute = CommandAttribute.ToggleEditable,
                    CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.UniformToVertical),
                },

                [CommandType.ToggleIsEnabledNearestNeighbor] = new CommandElement
                {
                    Group = "拡大モード",
                    Text = "ドットのまま拡大ON/OFF",
                    MenuText = "ドットのまま拡大",
                    Note = "ONにすると拡大するときにドットのまま拡大します。OFFの時にはスケール変換処理(Fant)が行われます",
                    Execute = e => _VM.IsEnabledNearestNeighbor = !_VM.IsEnabledNearestNeighbor,
                    ExecuteMessage = e => _VM.IsEnabledNearestNeighbor ? "高品質に拡大する" : "ドットのまま拡大する",
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsEnabledNearestNeighbor))
                },

                [CommandType.ToggleBackground] = new CommandElement
                {
                    Group = "背景",
                    Text = "背景を切り替える",
                    Note = "背景を順番に切り替えます",
                    Execute = e => _VM.Background = _VM.Background.GetToggle(),
                    ExecuteMessage = e => _VM.Background.GetToggle().ToDispString(),
                },

                [CommandType.SetBackgroundBlack] = new CommandElement
                {
                    Group = "背景",
                    Text = "背景を黒色にする",
                    Note = "背景を黒色にします",
                    Execute = e => _VM.Background = BackgroundStyle.Black,
                    CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.Black),
                },

                [CommandType.SetBackgroundWhite] = new CommandElement
                {
                    Group = "背景",
                    Text = "背景を白色にする",
                    Note = "背景を白色にします",
                    Execute = e => _VM.Background = BackgroundStyle.White,
                    CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.White),

                },

                [CommandType.SetBackgroundAuto] = new CommandElement
                {
                    Group = "背景",
                    Text = "背景を画像に合わせた色にする",
                    Note = "背景色を画像から設定します。具体的には画像の左上ピクセルの色が使用されます",
                    Execute = e => _VM.Background = BackgroundStyle.Auto,
                    CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.Auto),
                },

                [CommandType.SetBackgroundCheck] = new CommandElement
                {
                    Group = "背景",
                    Text = "背景をチェック模様にする",
                    Note = "背景をチェック模様にします",
                    Execute = e => _VM.Background = BackgroundStyle.Check,
                    CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.Check),
                },

                [CommandType.ToggleTopmost] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "常に手前に表示ON/OFF",
                    MenuText = "常に手前に表示",
                    Note = "ウィンドウを常に手前に表示します",
                    Execute = e => _VM.ToggleTopmost(),
                    ExecuteMessage = e => _VM.IsTopmost ? "「常に手前に表示」を解除" : "常に手前に表示する",
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsTopmost)),
                },
                [CommandType.ToggleHideMenu] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "メニューを自動的に隠すON/OFF",
                    MenuText = "メニューを自動的に隠す",
                    Note = "メニュー、スライダーを非表示にします。カーソルをウィンドウ上端、下端に合わせることで表示されます",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleHideMenu(),
                    ExecuteMessage = e => _VM.IsHideMenu ? "メニューを表示する" : "メニューを自動的に隠す",
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsHideMenu)),
                },
                [CommandType.ToggleHidePanel] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "パネルを自動的に隠すON/OFF",
                    MenuText = "パネルを自動的に隠す",
                    Note = "左右のパネルを自動的に隠します。カーソルをウィンドウ左端、右端に合わせることで表示されます",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleHidePanel(),
                    ExecuteMessage = e => _VM.IsHidePanel ? "パネルを表示する" : "パネルを自動的に隠す",
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsHidePanel)),
                },
                [CommandType.ToggleHideTitleBar] = new CommandElement // 欠番
                {
                    Group = "dummy",
                    Text = "dummy",
                    Execute = e => { return; }
                },
                [CommandType.ToggleVisibleTitleBar] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "タイトルバーON/OFF",
                    MenuText = "タイトルバー",
                    Note = "ウィンドウタイトルバーの表示/非表示を切り替えます",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleVisibleTitleBar(),
                    ExecuteMessage = e => _VM.IsVisibleTitleBar ? "タイトルバーを消す" : "タイトルバー表示する",
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleTitleBar)),
                },
                [CommandType.ToggleVisibleAddressBar] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "アドレスバーON/OFF",
                    MenuText = "アドレスバー",
                    Note = "アドレスバーの表示/非表示を切り替えます",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleVisibleAddressBar(),
                    ExecuteMessage = e => _VM.IsVisibleAddressBar ? "アドレスバーを消す" : "アドレスバーを表示する",
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleAddressBar)),
                },
                [CommandType.ToggleVisibleFileInfo] = new CommandElement
                {
                    Group = "パネル",
                    Text = "ファイル情報の表示ON/OFF",
                    MenuText = "ファイル情報",
                    Note = "ファイル情報パネルの表示/非表示を切り替えます。ファイル情報パネルは右側に表示されます",
                    ShortCutKey = "I",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleVisibleFileInfo(),
                    ExecuteMessage = e => _VM.IsVisibleFileInfo ? "ファイル情報を消す" : "ファイル情報を表示する",
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleFileInfo)),
                },
                [CommandType.ToggleVisibleFolderList] = new CommandElement
                {
                    Group = "パネル",
                    Text = "フォルダーリストの表示ON/OFF",
                    MenuText = "フォルダーリスト",
                    Note = "フォルダーリストパネルの表示/非表示を切り替えます",
                    ShortCutKey = "F",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleVisibleFolderList(),
                    ExecuteMessage = e => _VM.IsVisibleFolderList ? "フォルダーリストを消す" : "フォルダーリストを表示する",
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleFolderList)),
                },
                [CommandType.ToggleVisibleBookmarkList] = new CommandElement
                {
                    Group = "パネル",
                    Text = "ブックマークの表示ON/OFF",
                    MenuText = "ブックマークリスト",
                    Note = "ブックマークリストパネルの表示/非表示を切り替えます",
                    ShortCutKey = "B",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleVisibleBookmarkList(),
                    ExecuteMessage = e => _VM.IsVisibleBookmarkList ? "ブックマークリストを消す" : "ブックマークリストを表示する",
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleBookmarkList)),
                },
                [CommandType.ToggleVisibleHistoryList] = new CommandElement
                {
                    Group = "パネル",
                    Text = "履歴リストの表示ON/OFF",
                    MenuText = "履歴リスト",
                    Note = "履歴リストパネルの表示/非表示を切り替えます",
                    ShortCutKey = "H",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleVisibleHistoryList(),
                    ExecuteMessage = e => _VM.IsVisibleHistoryList ? "履歴リストを消す" : "履歴リストを表示する",
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleHistoryList)),
                },
                [CommandType.ToggleVisiblePageList] = new CommandElement
                {
                    Group = "パネル",
                    Text = "ページリストの表示ON/OFF",
                    MenuText = "ページリスト",
                    Note = "フォルダーリストパネルでのページリスト表示/非表示を切り替えます",
                    ShortCutKey = "P",
                    IsShowMessage = false,
                    ExecuteMessage = e => _VM.IsVisiblePageList ? "ページリストを消す" : "ページリストを表示する",
                    Execute = e => _VM.ToggleVisiblePageList(),
                    CanExecute = () => _VM.IsVisibleFolderList,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisiblePageList)),
                },

                [CommandType.ToggleVisibleThumbnailList] = new CommandElement
                {
                    Group = "サムネイルリスト",
                    Text = "サムネイルリストの表示ON/OFF",
                    MenuText = "サムネイルリスト",
                    Note = "サムネイルリスト表示/非表示を切り替えます",
                    IsShowMessage = false,
                    ExecuteMessage = e => _VM.IsEnableThumbnailList ? "サムネイルリストを消す" : "サムネイルリストを表示する",
                    Execute = e => _VM.ToggleVisibleThumbnailList(),
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsEnableThumbnailList)),
                },
                [CommandType.ToggleHideThumbnailList] = new CommandElement
                {
                    Group = "サムネイルリスト",
                    Text = "サムネイルリストを自動的に隠すON/OFF",
                    MenuText = "サムネイルリストを自動的に隠す",
                    Note = "スライダーを使用している時だけサムネイルリストを表示するようにします",
                    IsShowMessage = false,
                    ExecuteMessage = e => _VM.IsHideThumbnailList ? "サムネイルリストを表示する" : "サムネイルリストを自動的に隠す",
                    Execute = e => _VM.ToggleHideThumbnailList(),
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsHideThumbnailList)),
                },


                [CommandType.ToggleFullScreen] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "フルスクリーンON/OFF",
                    MenuText = "フルスクリーン",
                    Note = "フルスクリーン状態を切替ます",
                    ShortCutKey = "F11",
                    MouseGesture = "U",
                    IsShowMessage = false,
                    Execute = e => _VM.ToggleFullScreen(),
                    ExecuteMessage = e => _VM.IsFullScreen ? "フルスクリーンOFF" : "フルスクリーンON",
                    CanExecute = () => true,
                    CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsFullScreen)),
                },
                [CommandType.SetFullScreen] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "フルスクリーンにする",
                    Note = "フルスクリーンにします",
                    IsShowMessage = false,
                    Execute = e => _VM.IsFullScreen = true,
                    CanExecute = () => true,
                },
                [CommandType.CancelFullScreen] = new CommandElement
                {
                    Group = "ウィンドウ",
                    Text = "フルスクリーン解除",
                    Note = "フルスクリーンを解除します",
                    ShortCutKey = "Escape",
                    IsShowMessage = false,
                    Execute = e => _VM.IsFullScreen = false,
                    CanExecute = () => true,
                },

                [CommandType.ToggleSlideShow] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "スライドショー再生/停止",
                    MenuText = "スライドショー",
                    Note = "スライドショーの再生/停止を切り替えます",
                    ShortCutKey = "F5",
                    Execute = e => _Book.ToggleSlideShow(),
                    ExecuteMessage = e => _Book.IsEnableSlideShow ? "スライドショー停止" : "スライドショー再生",
                    CreateIsCheckedBinding = () => BindingGenerator.BindingBookHub(nameof(_Book.IsEnableSlideShow)),
                },
                [CommandType.ViewScrollUp] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "スクロール↑",
                    Note = "画像を上方向にするロールさせます。縦スクロールできないときは横スクロールになります",
                    ShortCutKey = "WheelUp",
                    IsShowMessage = false
                },
                [CommandType.ViewScrollDown] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "スクロール↓",
                    Note = "画像を下方向にするロールさせます。縦スクロールできないときは横スクロールになります",
                    ShortCutKey = "WheelDown",
                    IsShowMessage = false
                },
                [CommandType.ViewScaleUp] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "拡大",
                    Note = "画像を20%拡大します",
                    ShortCutKey = "RightButton+WheelUp",
                    IsShowMessage = false
                },
                [CommandType.ViewScaleDown] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "縮小",
                    Note = "画像を20%縮小します",
                    ShortCutKey = "RightButton+WheelDown",
                    IsShowMessage = false
                },
                [CommandType.ViewRotateLeft] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "左回転",
                    Note = "画像を45度左回転させます",
                    IsShowMessage = false
                },
                [CommandType.ViewRotateRight] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "右回転",
                    Note = "画像を45度右回転させます",
                    IsShowMessage = false
                },
                [CommandType.ToggleViewFlipHorizontal] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "左右反転",
                    Note = "画像を左右反転させます",
                    IsShowMessage = false,
                    CreateIsCheckedBinding = () => BindingGenerator.IsFlipHorizontal(),
                },
                [CommandType.ViewFlipHorizontalOn] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "左右反転ON",
                    Note = "左右反転状態にします",
                    IsShowMessage = false
                },
                [CommandType.ViewFlipHorizontalOff] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "左右反転OFF",
                    Note = "左右反転状態を解除します",

                    IsShowMessage = false
                },


                [CommandType.ToggleViewFlipVertical] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "上下反転",
                    Note = "画像を上下反転させます",
                    IsShowMessage = false,
                    CreateIsCheckedBinding = () => BindingGenerator.IsFlipVertical(),
                },
                [CommandType.ViewFlipVerticalOn] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "上下反転ON",
                    Note = "上下反転状態にします",
                    IsShowMessage = false
                },
                [CommandType.ViewFlipVerticalOff] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "上下反転OFF",
                    Note = "上下反転状態を解除します",
                    IsShowMessage = false
                },

                [CommandType.ViewReset] = new CommandElement
                {
                    Group = "ビュー操作",
                    Text = "ビューリセット",
                    Note = "ビュー操作での回転、拡縮、移動、反転を初期化します",
                    IsShowMessage = false
                },

                [CommandType.PrevPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "前のページに戻る",
                    Note = "ページ前方向に移動します。2ページ表示の場合は2ページ分移動します",
                    ShortCutKey = "Right,RightClick",
                    MouseGesture = "R",
                    IsShowMessage = false,
                    Execute = e => _Book.PrevPage(),
                },
                [CommandType.NextPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "次のページへ進む",
                    Note = "ページ次方向に移動します。2ページ表示の場合は2ページ分移動します",
                    ShortCutKey = "Left,LeftClick",
                    MouseGesture = "L",
                    IsShowMessage = false,
                    Execute = e => _Book.NextPage(),
                },
                [CommandType.PrevOnePage] = new CommandElement
                {
                    Group = "移動",
                    Text = "1ページ戻る",
                    Note = "1ページだけ前方向に移動します",
                    MouseGesture = "LR",
                    IsShowMessage = false,
                    Execute = e => _Book.PrevOnePage(),
                },
                [CommandType.NextOnePage] = new CommandElement
                {
                    Group = "移動",
                    Text = "1ページ進む",
                    Note = "1ページだけ次方向に移動します",
                    MouseGesture = "RL",
                    IsShowMessage = false,
                    Execute = e => _Book.NextOnePage(),
                },
                [CommandType.PrevScrollPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "スクロール＋前のページに戻る",
                    Note = "ページ前方向に画像をスクロールさせます。スクロールできない場合は前ページに移動します",
                    IsShowMessage = false,
                },
                [CommandType.NextScrollPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "スクロール＋次のページへ進む",
                    Note = "ページ次方向に画像をスクロールさせます。スクロールできない場合は次ページに移動します",
                    IsShowMessage = false,
                },
                [CommandType.MovePageWithCursor] = new CommandElement
                {
                    Group = "移動",
                    Text = "マウス位置依存でページを前後させる",
                    Note = "マウスカーソル位置によって移動方向が決まります。 ウィンドウ左にカーソルがあるときは次のページへ進み、右にカーソルがあるときは前のページに戻ります",
                    IsShowMessage = false,
                },

                [CommandType.FirstPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "最初のページに移動",
                    Note = "先頭ページに移動します",
                    ShortCutKey = "Ctrl+Right",
                    MouseGesture = "UR",
                    Execute = e => _Book.FirstPage(),
                },
                [CommandType.LastPage] = new CommandElement
                {
                    Group = "移動",
                    Text = "最後のページへ移動",
                    Note = "終端ページに移動します",
                    ShortCutKey = "Ctrl+Left",
                    MouseGesture = "UL",
                    Execute = e => _Book.LastPage(),
                },
                [CommandType.PrevFolder] = new CommandElement
                {
                    Group = "移動",
                    Text = "前のフォルダに移動",
                    Note = "フォルダーリスト上での前のフォルダを読み込みます",
                    ShortCutKey = "Up",
                    MouseGesture = "LU",
                    IsShowMessage = false,
                    Execute = e => _Book.PrevFolder(),
                },
                [CommandType.NextFolder] = new CommandElement
                {
                    Group = "移動",
                    Text = "次のフォルダへ移動",
                    Note = "フォルダーリスト上での次のフォルダを読み込みます",
                    ShortCutKey = "Down",
                    MouseGesture = "LD",
                    IsShowMessage = false,
                    Execute = e => _Book.NextFolder(),
                },
                [CommandType.PrevHistory] = new CommandElement
                {
                    Group = "移動",
                    Text = "前の履歴に戻る",
                    Note = "前の古い履歴のフォルダを読み込みます",
                    ShortCutKey = "Back",
                    IsShowMessage = false,
                    CanExecute = () => _Book.CanPrevHistory(),
                    Execute = e => _Book.PrevHistory(),
                },
                [CommandType.NextHistory] = new CommandElement
                {
                    Group = "移動",
                    Text = "次の履歴へ進む",
                    Note = "次の新しい履歴のフォルダを読み込みます",
                    ShortCutKey = "Shift+Back",
                    IsShowMessage = false,
                    CanExecute = () => _Book.CanNextHistory(),
                    Execute = e => _Book.NextHistory(),
                },


                [CommandType.ToggleFolderOrder] = new CommandElement
                {
                    Group = "フォルダ列",
                    Text = "フォルダーの並び順を切り替える",
                    Note = "フォルダーの並び順を順番に切り替えます",
                    Execute = e => _Book.ToggleFolderOrder(),
                    ExecuteMessage = e => _Book.GetFolderOrder().GetToggle().ToDispString(),
                },
                [CommandType.SetFolderOrderByFileName] = new CommandElement
                {
                    Group = "フォルダ列",
                    Text = "フォルダ列はファイル名順",
                    Note = "フォルダーの並びを名前順(昇順)にします",
                    Execute = e => _Book.SetFolderOrder(FolderOrder.FileName),
                    CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.FileName),
                },
                [CommandType.SetFolderOrderByTimeStamp] = new CommandElement
                {
                    Group = "フォルダ列",
                    Text = "フォルダ列は日付順",
                    Note = "フォルダーの並びを日付順(降順)にします",
                    Execute = e => _Book.SetFolderOrder(FolderOrder.TimeStamp),
                    CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.TimeStamp),
                },
                [CommandType.SetFolderOrderByRandom] = new CommandElement
                {
                    Group = "フォルダ列",
                    Text = "フォルダ列はシャッフル",
                    Note = "フォルダーの並びをシャッフルします",
                    Execute = e => _Book.SetFolderOrder(FolderOrder.Random),
                    CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.Random),
                },

                [CommandType.TogglePageMode] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "ページ表示モードを切り替える",
                    Note = "1ページ表示/2ページ表示を切り替えます",
                    CanExecute = () => true,
                    Execute = e => _Book.TogglePageMode(),
                    ExecuteMessage = e => _Book.BookMemento.PageMode.GetToggle().ToDispString(),
                },
                [CommandType.SetPageMode1] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "1ページ表示",
                    Note = "1ページ表示にします",
                    ShortCutKey = "Ctrl+1",
                    MouseGesture = "RU",
                    Execute = e => _Book.SetPageMode(PageMode.SinglePage),
                    CreateIsCheckedBinding = () => BindingGenerator.PageMode(PageMode.SinglePage),
                },
                [CommandType.SetPageMode2] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "2ページ表示",
                    Note = "2ページ表示にします",
                    ShortCutKey = "Ctrl+2",
                    MouseGesture = "RD",
                    Execute = e => _Book.SetPageMode(PageMode.WidePage),
                    CreateIsCheckedBinding = () => BindingGenerator.PageMode(PageMode.WidePage),
                },
                [CommandType.ToggleBookReadOrder] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "右開き、左開きを切り替える",
                    Note = "右開き、左開きを切り替えます",
                    CanExecute = () => true,
                    Execute = e => _Book.ToggleBookReadOrder(),
                    ExecuteMessage = e => _Book.BookMemento.BookReadOrder.GetToggle().ToDispString()
                },
                [CommandType.SetBookReadOrderRight] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "右開き",
                    Note = "読み進む方向を右開きにします。2ページ表示のときに若いページが右になります",
                    Execute = e => _Book.SetBookReadOrder(PageReadOrder.RightToLeft),
                    CreateIsCheckedBinding = () => BindingGenerator.BookReadOrder(PageReadOrder.RightToLeft),
                },
                [CommandType.SetBookReadOrderLeft] = new CommandElement
                {
                    Group = "ページ表示",
                    Text = "左開き",
                    Note = "読み進む方向を左開きにします。2ページ表示のときに若いページが左になります",
                    Execute = e => _Book.SetBookReadOrder(PageReadOrder.LeftToRight),
                    CreateIsCheckedBinding = () => BindingGenerator.BookReadOrder(PageReadOrder.LeftToRight),
                },

                [CommandType.ToggleIsSupportedDividePage] = new CommandElement
                {
                    Group = "1ページ表示設定",
                    Text = "横長ページを分割する",
                    Note = "1ページ表示時、横長ページを分割してページにします",
                    Execute = e => _Book.ToggleIsSupportedDividePage(),
                    ExecuteMessage = e => _Book.BookMemento.IsSupportedDividePage ? "横長ページの区別をしない" : "横長ページを分割する",
                    CanExecute = () => _Book.CanPageModeSubSetting(PageMode.SinglePage),
                    CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_Book.BookMemento.IsSupportedDividePage)),
                },

                [CommandType.ToggleIsSupportedWidePage] = new CommandElement
                {
                    Group = "2ページ表示設定",
                    Text = "横長ページを2ページとみなす",
                    Note = " 2ページ表示時、横長の画像を2ページ分とみなして単独表示します",
                    Execute = e => _Book.ToggleIsSupportedWidePage(),
                    ExecuteMessage = e => _Book.BookMemento.IsSupportedWidePage ? "横長ページの区別をしない" : "横長ページを2ページとみなす",
                    CanExecute = () => _Book.CanPageModeSubSetting(PageMode.WidePage),
                    CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_Book.BookMemento.IsSupportedWidePage)),
                },
                [CommandType.ToggleIsSupportedSingleFirstPage] = new CommandElement
                {
                    Group = "2ページ表示設定",
                    Text = "最初のページを単独表示",
                    Note = "2ページ表示でも最初のページは1ページ表示にします",
                    Execute = e => _Book.ToggleIsSupportedSingleFirstPage(),
                    ExecuteMessage = e => _Book.BookMemento.IsSupportedSingleFirstPage ? "最初のページを区別しない" : "最初のページを単独表示",
                    CanExecute = () => _Book.CanPageModeSubSetting(PageMode.WidePage),
                    CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_Book.BookMemento.IsSupportedSingleFirstPage)),
                },
                [CommandType.ToggleIsSupportedSingleLastPage] = new CommandElement
                {
                    Group = "2ページ表示設定",
                    Text = "最後のページを単独表示",
                    Note = "2ページ表示でも最後のページは1ページ表示にします",
                    Execute = e => _Book.ToggleIsSupportedSingleLastPage(),
                    ExecuteMessage = e => _Book.BookMemento.IsSupportedSingleLastPage ? "最後のページを区別しない" : "最後のページを単独表示",
                    CanExecute = () => _Book.CanPageModeSubSetting(PageMode.WidePage),
                    CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_Book.BookMemento.IsSupportedSingleLastPage)),
                },

                [CommandType.ToggleIsRecursiveFolder] = new CommandElement
                {
                    Group = "ページ読込",
                    Text = "サブフォルダを読み込む",
                    Note = "フォルダから画像を読み込むときにサブフォルダまたは圧縮ファイルも同時に読み込みます",
                    Execute = e => _Book.ToggleIsRecursiveFolder(),
                    ExecuteMessage = e => _Book.BookMemento.IsRecursiveFolder ? "サブフォルダは読み込まない" : "サブフォルダも読み込む",
                    CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_Book.BookMemento.IsRecursiveFolder)),
                },

                [CommandType.ToggleSortMode] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ページの並び順を切り替える",
                    Note = "ページの並び順を順番に切り替えます",
                    CanExecute = () => true,
                    Execute = e => _Book.ToggleSortMode(),
                    ExecuteMessage = e => _Book.BookMemento.SortMode.GetToggle().ToDispString(),
                },
                [CommandType.SetSortModeFileName] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ファイル名昇順",
                    Note = "ページの並び順をファイル名昇順にします",
                    Execute = e => _Book.SetSortMode(PageSortMode.FileName),
                    CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.FileName),
                },
                [CommandType.SetSortModeFileNameDescending] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ファイル名降順",
                    Note = "ページの並び順をファイル名降順にします",
                    Execute = e => _Book.SetSortMode(PageSortMode.FileNameDescending),
                    CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.FileNameDescending),
                },
                [CommandType.SetSortModeTimeStamp] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ファイル日付昇順",
                    Note = "ページの並び順をファイル日付昇順にします",
                    Execute = e => _Book.SetSortMode(PageSortMode.TimeStamp),
                    CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.TimeStamp),
                },
                [CommandType.SetSortModeTimeStampDescending] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "ファイル日付降順",
                    Note = "ページの並び順をファイル日付降順にします",
                    Execute = e => _Book.SetSortMode(PageSortMode.TimeStampDescending),
                    CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.TimeStampDescending),
                },
                [CommandType.SetSortModeRandom] = new CommandElement
                {
                    Group = "ページ列",
                    Text = "シャッフル",
                    Note = "ページの並び順をシャッフルます",
                    Execute = e => _Book.SetSortMode(PageSortMode.Random),
                    CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.Random),
                },

                [CommandType.ToggleBookmark] = new CommandElement
                {
                    Group = "ブックマーク",
                    Text = "ブックマーク登録/解除",
                    MenuText = "ブックマーク",
                    Note = "現在開いているフォルダのブックマークの登録/解除を切り替えます",
                    Execute = e => _Book.ToggleBookmark(),
                    CanExecute = () => true,
                    ExecuteMessage = e => _Book.IsBookmark(null) ? "ブックマーク解除" : "ブックマークに登録",
                    IsShowMessage = false,
                    CreateIsCheckedBinding = () => BindingGenerator.IsBookmark(),
                },
                [CommandType.Bookmark] = new CommandElement
                {
                    Group = "ブックマーク",
                    Text = "ブックマークに登録する",
                    Note = "現在開いているフォルダをブックマークに登録します",
                    Execute = e => _Book.Bookmark(),
                    CanExecute = () => _Book.CanBookmark(),
                    ExecuteMessage = e => "ブックマークに登録",
                    ShortCutKey = "Ctrl+D",
                },

#if false
                [CommandType.SetEffectNone] = new CommandElement
                {
                    Group = "エフェクト",
                    Text = "エフェクト無効",
                    MenuText = "エフェクトなし",
                    Note = "エフェクトを無効にします",
                    Execute = e => _VM.ShaderEffectType = ShaderEffectType.None,
                    CanExecute = () => true,
                    IsShowMessage = false,
                    CreateIsCheckedBinding = () => BindingGenerator.ShaderEffectType(ShaderEffectType.None),
                },
                [CommandType.SetEffectGrayscale] = new CommandElement
                {
                    Group = "エフェクト",
                    Text = "グレイスケール",
                    MenuText = "グレイスケール",
                    Note = "画像をグレイスケールにします",
                    Execute = e => _VM.ShaderEffectType = ShaderEffectType.Grayscale,
                    CanExecute = () => true,
                    IsShowMessage = false,
                    CreateIsCheckedBinding = () => BindingGenerator.ShaderEffectType(ShaderEffectType.Grayscale),
                },
#endif

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
                    MenuText = "設定...",
                    Note = "設定ウィンドウを開きます",
                    IsShowMessage = false,
                },
                [CommandType.OpenVersionWindow] = new CommandElement
                {
                    Group = "その他",
                    Text = "バージョン情報を表示する",
                    MenuText = "NeeView について...",
                    Note = "バージョン情報を表示します",
                    IsShowMessage = false,
                },
                [CommandType.CloseApplication] = new CommandElement
                {
                    Group = "その他",
                    Text = "アプリを終了する",
                    MenuText = "アプリを終了",
                    Note = "このアプリケーションを終了させます",
                    ShortCutKey = "Alt+F4",
                    IsShowMessage = false,
                    CanExecute = () => true,
                },


                [CommandType.HelpOnline] = new CommandElement
                {
                    Group = "その他",
                    Text = "オンラインヘルプ",
                    MenuText = "オンラインヘルプ",
                    Note = "オンラインヘルプを表示します",
                    IsShowMessage = false,
                    Execute = e => _VM.OpenOnlineHelp(),
                    CanExecute = () => true,
                },

                [CommandType.HelpCommandList] = new CommandElement
                {
                    Group = "その他",
                    Text = "コマンドリストを表示する",
                    MenuText = "コマンド一覧",
                    Note = "コマンドのヘルプをブラウザで表示します",
                    IsShowMessage = false,
                    Execute = e => this.OpenCommandListHelp(),
                    CanExecute = () => true,
                },

                [CommandType.HelpMainMenu] = new CommandElement
                {
                    Group = "その他",
                    Text = "メインメニューのヘルプを表示する",
                    MenuText = "メインメニューの説明",
                    Note = "メインメニューのヘルプをブラウザで表示します",
                    IsShowMessage = false,
                    Execute = e => _VM.OpenMainMenuHelp(),
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

            //
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
                if (pair.Key.IsDisable()) continue;
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
