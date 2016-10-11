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

// TODO: コマンド引数にコマンドパラメータを渡せないだろうか。（現状メニュー呼び出しであることを示すタグが指定されることが有る)

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

        // .. あまりかわらん
        public T Parameter<T>(CommandType commandType) where T : class
        {
            return _Elements[commandType].Parameter as T;
        }


        // ショートカット重複チェック
        public List<CommandType> GetOverlapShortCut(string shortcut)
        {
            var overlaps = _Elements
                .Where(e => !string.IsNullOrEmpty(e.Value.ShortCutKey) &&  e.Value.ShortCutKey.Split(',').Contains(shortcut))
                .Select(e => e.Key)
                .ToList();

            return overlaps;
        }

        // マウスジェスチャー重複チェック
        public List<CommandType> GetOverlapMouseGesture(string gesture)
        {
            var overlaps = _Elements
                .Where(e => !string.IsNullOrEmpty(e.Value.MouseGesture) && e.Value.MouseGesture.Split(',').Contains(gesture))
                .Select(e => e.Key)
                .ToList();

            return overlaps;
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
                    writer.WriteLine($"<th>コマンド<th>ショートカットキー<th>マウスジェスチャー<th>説明<tr>");
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
            _Elements = new Dictionary<CommandType, CommandElement>();

            // None
            // 欠番
            {
                var element = new CommandElement();
                element.Group = "dummy";
                element.Text = "dummy";
                element.Execute = (s, e) => { return; };
                _Elements[CommandType.None] = element;
            }

#if false
            // OpenContextMenu
            // コマンドでコンテキストメニューを開くと正常に動作しない。
            {
                var element = new CommandElement();
                element.Group = "その他";
                element.Text = "コンテキストメニューを開く";
                element.Execute = (s, e) => { return; };
                //element.Execute = (s, e) => _VM.OpenContextMenu();
                //element.CanExecute = () => _VM.CanOpenContextMenu(); 
                element.IsShowMessage = false;
                _Elements[CommandType.OpenContextMenu] = element;
            }
#endif

            // LoadAs
            {
                var element = new CommandElement();
                element.Group = "ファイル";
                element.Text = "ファイルを開く";
                element.MenuText = "開く...";
                element.Note = "圧縮ファイルか画像ファイルを選択して開きます";
                element.ShortCutKey = "Ctrl+O";
                element.IsShowMessage = false;
                _Elements[CommandType.LoadAs] = element;
            }

            // ReLoad
            {
                var element = new CommandElement();
                element.Group = "ファイル";
                element.Text = "再読み込み";
                element.Note = "フォルダーを再読み込みします";
                element.MouseGesture = "UD";
                element.CanExecute = () => _Book.CanReload();
                element.Execute = (s, e) => _Book.ReLoad();
                element.IsShowMessage = false;
                _Elements[CommandType.ReLoad] = element;
            }

            // OpenApplication
            {
                var element = new CommandElement();
                element.Group = "ファイル";
                element.Text = "外部アプリで開く";
                element.Note = "表示されている画像を外部アプリで開きます。設定ウィンドウの<code>外部連携</code>でアプリを設定します";
                element.Execute = (s, e) => _Book.OpenApplication();
                element.CanExecute = () => _Book.CanOpenFilePlace();
                element.IsShowMessage = false;
                _Elements[CommandType.OpenApplication] = element;
            }
            // OpenFilePlace
            {
                var element = new CommandElement();
                element.Group = "ファイル";
                element.Text = "エクスプローラーで開く";
                element.Note = "表示しているページのファイルをエクスプローラーで開きます";
                element.Execute = (s, e) => _Book.OpenFilePlace();
                element.CanExecute = () => _Book.CanOpenFilePlace();
                element.IsShowMessage = false;
                _Elements[CommandType.OpenFilePlace] = element;
            }
            // Export
            {
                var element = new CommandElement();
                element.Group = "ファイル";
                element.Text = "名前をつけてファイルに保存";
                element.MenuText = "保存...";
                element.Note = "画像をファイルに保存します";
                element.ShortCutKey = "Ctrl+S";
                element.Execute = (s, e) => _Book.Export();
                element.CanExecute = () => _Book.CanOpenFilePlace();
                element.IsShowMessage = false;
                _Elements[CommandType.Export] = element;
            }
            // DeleteFile
            {
                var element = new CommandElement();
                element.Group = "ファイル";
                element.Text = "ファイルを削除";
                element.MenuText = "削除...";
                element.Note = "ファイルを削除します。圧縮ファイルの場合は削除できません ";
                element.ShortCutKey = "Delete";
                element.Execute = (s, e) => _Book.DeleteFile();
                element.CanExecute = () => _Book.CanDeleteFile();
                element.IsShowMessage = false;
                _Elements[CommandType.DeleteFile] = element;
            }
            // CopyFile
            {
                var element = new CommandElement();
                element.Group = "ファイル";
                element.Text = "ファイルをコピー";
                element.MenuText = "コピー";
                element.Note = "ファイルをクリップボードにコピーします";
                element.ShortCutKey = "Ctrl+C";
                element.Execute = (s, e) => _Book.CopyToClipboard();
                element.CanExecute = () => _Book.CanOpenFilePlace();
                element.IsShowMessage = true;
                _Elements[CommandType.CopyFile] = element;
            }
            // CopyImage
            {
                var element = new CommandElement();
                element.Group = "ファイル";
                element.Text = "画像をコピー";
                element.MenuText = "画像コピー";
                element.Note = "画像をクリップボードにコピーします。2ページ表示の場合はメインとなるページのみコピーします";
                element.ShortCutKey = "Ctrl+Shift+C";
                element.Execute = (s, e) => _VM.CopyImageToClipboard();
                element.CanExecute = () => _VM.CanCopyImageToClipboard();
                element.IsShowMessage = true;
                _Elements[CommandType.CopyImage] = element;
            }
            // Paste
            {
                var element = new CommandElement();
                element.Group = "ファイル";
                element.Text = "貼り付け";
                element.MenuText = "貼り付け";
                element.Note = "クリップボードのファイルや画像を貼り付けます";
                element.ShortCutKey = "Ctrl+V";
                element.IsShowMessage = false;
                _Elements[CommandType.Paste] = element;
            }


            // ClearHistory
            {
                var element = new CommandElement();
                element.Group = "ファイル";
                element.Text = "履歴を消去";
                element.Note = "履歴を全て削除します";
                element.Execute = (s, e) => _VM.ClearHistor();
                element.IsShowMessage = true;
                _Elements[CommandType.ClearHistory] = element;
            }

            // ToggleStretchMode
            {
                var element = new CommandElement();
                element.Group = "表示サイズ";
                element.Text = "表示サイズを切り替える";
                element.Note = "画像の表示サイズを順番に切り替えます";
                element.ShortCutKey = "LeftButton+WheelDown";
                element.Execute = (s, e) => _VM.StretchMode = _VM.GetToggleStretchMode((ToggleStretchModeCommandParameter)element.Parameter);
                element.ExecuteMessage = e => _VM.GetToggleStretchMode((ToggleStretchModeCommandParameter)element.Parameter).ToDispString();
                element.DefaultParameter = new ToggleStretchModeCommandParameter() { IsLoop = true };
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleStretchMode] = element;
            }
            // ToggleStretchModeReverse
            {
                var element = new CommandElement();
                element.Group = "表示サイズ";
                element.Text = "表示サイズを切り替える(逆順)";
                element.Note = "画像の表示サイズを順番に切り替えます(逆順)";
                element.ShortCutKey = "LeftButton+WheelUp";
                element.Execute = (s, e) => _VM.StretchMode = _VM.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)element.Parameter);
                element.ExecuteMessage = e => _VM.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)element.Parameter).ToDispString();
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.ToggleStretchMode };
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleStretchModeReverse] = element;
            }
            // SetStretchModeNone
            {
                var element = new CommandElement();
                element.Group = "表示サイズ";
                element.Text = "オリジナルサイズ";
                element.Note = "画像のサイズそのままで表示します";
                element.Execute = (s, e) => _VM.StretchMode = PageStretchMode.None;
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.None);
                element.IsShowMessage = true;
                _Elements[CommandType.SetStretchModeNone] = element;
            }
            // SetStretchModeInside
            {
                var element = new CommandElement();
                element.Group = "表示サイズ";
                element.Text = "大きい場合ウィンドウサイズに合わせる";
                element.Note = "ウィンドウに収まるように画像を縮小して表示します";
                element.Execute = (s, e) => _VM.SetStretchMode(PageStretchMode.Inside, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_VM.TestStretchMode(PageStretchMode.Inside, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.Inside);
                element.DefaultParameter = new StretchModeCommandParameter();
                element.IsShowMessage = true;
                _Elements[CommandType.SetStretchModeInside] = element;
            }
            // SetStretchModeOutside
            {
                var element = new CommandElement();
                element.Group = "表示サイズ";
                element.Text = "小さい場合ウィンドウサイズに広げる";
                element.Note = "ウィンドウに収まるように画像をできるだけ拡大して表示します";
                element.Execute = (s, e) => _VM.SetStretchMode(PageStretchMode.Outside, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_VM.TestStretchMode(PageStretchMode.Outside, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.Outside);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.SetStretchModeInside };
                element.IsShowMessage = true;
                _Elements[CommandType.SetStretchModeOutside] = element;
            }
            // SetStretchModeUniform
            {
                var element = new CommandElement();
                element.Group = "表示サイズ";
                element.Text = "ウィンドウサイズに合わせる";
                element.Note = "画像をウィンドウサイズに合わせるよう拡大縮小します";
                element.Execute = (s, e) => _VM.SetStretchMode(PageStretchMode.Uniform, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_VM.TestStretchMode(PageStretchMode.Uniform, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.Uniform);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.SetStretchModeInside };
                element.IsShowMessage = true;
                _Elements[CommandType.SetStretchModeUniform] = element;
            }
            // SetStretchModeUniformToFill
            {
                var element = new CommandElement();
                element.Group = "表示サイズ";
                element.Text = "ウィンドウいっぱいに広げる";
                element.Note = "縦横どちらかをウィンドウサイズに合わせるように拡大縮小します。画像はウィンドウサイズより大きくなります";
                element.Execute = (s, e) => _VM.SetStretchMode(PageStretchMode.UniformToFill, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_VM.TestStretchMode(PageStretchMode.UniformToFill, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.UniformToFill);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.SetStretchModeInside };
                element.IsShowMessage = true;
                _Elements[CommandType.SetStretchModeUniformToFill] = element;
            }
            // SetStretchModeUniformToSize
            {
                var element = new CommandElement();
                element.Group = "表示サイズ";
                element.Text = "面積をウィンドウに合わせる";
                element.Note = "ウィンドウの面積と等しくなるように画像を拡大縮小します";
                element.Execute = (s, e) => _VM.SetStretchMode(PageStretchMode.UniformToSize, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_VM.TestStretchMode(PageStretchMode.UniformToSize, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.UniformToSize);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.SetStretchModeInside };
                element.IsShowMessage = true;
                _Elements[CommandType.SetStretchModeUniformToSize] = element;
            }
            // SetStretchModeUniformToVertical
            {
                var element = new CommandElement();
                element.Group = "表示サイズ";
                element.Text = "高さをウィンドウに合わせる";
                element.Note = "ウィンドウの高さに画像の高さを合わせるように拡大縮小します";
                element.Execute = (s, e) => _VM.SetStretchMode(PageStretchMode.UniformToVertical, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_VM.TestStretchMode(PageStretchMode.UniformToVertical, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.UniformToVertical);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.SetStretchModeInside };
                element.IsShowMessage = true;
                _Elements[CommandType.SetStretchModeUniformToVertical] = element;
            }

            // ToggleIsEnabledNearestNeighbor
            {
                var element = new CommandElement();
                element.Group = "拡大モード";
                element.Text = "ドットのまま拡大ON/OFF";
                element.MenuText = "ドットのまま拡大";
                element.Note = "ONにすると拡大するときにドットのまま拡大します。OFFの時にはスケール変換処理(Fant)が行われます";
                element.Execute = (s, e) => _VM.IsEnabledNearestNeighbor = !_VM.IsEnabledNearestNeighbor;
                element.ExecuteMessage = e => _VM.IsEnabledNearestNeighbor ? "高品質に拡大する" : "ドットのまま拡大する";
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsEnabledNearestNeighbor));
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleIsEnabledNearestNeighbor] = element;
            }

            // ToggleBackground
            {
                var element = new CommandElement();
                element.Group = "背景";
                element.Text = "背景を切り替える";
                element.Note = "背景を順番に切り替えます";
                element.Execute = (s, e) => _VM.Background = _VM.Background.GetToggle();
                element.ExecuteMessage = e => _VM.Background.GetToggle().ToDispString();
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleBackground] = element;
            }

            // SetBackgroundBlack
            {
                var element = new CommandElement();
                element.Group = "背景";
                element.Text = "背景を黒色にする";
                element.Note = "背景を黒色にします";
                element.Execute = (s, e) => _VM.Background = BackgroundStyle.Black;
                element.CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.Black);
                element.IsShowMessage = true;
                _Elements[CommandType.SetBackgroundBlack] = element;
            }

            // SetBackgroundWhite
            {
                var element = new CommandElement();
                element.Group = "背景";
                element.Text = "背景を白色にする";
                element.Note = "背景を白色にします";
                element.Execute = (s, e) => _VM.Background = BackgroundStyle.White;
                element.CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.White);
                element.IsShowMessage = true;
                _Elements[CommandType.SetBackgroundWhite] = element;
            }

            // SetBackgroundAuto
            {
                var element = new CommandElement();
                element.Group = "背景";
                element.Text = "背景を画像に合わせた色にする";
                element.Note = "背景色を画像から設定します。具体的には画像の左上ピクセルの色が使用されます";
                element.Execute = (s, e) => _VM.Background = BackgroundStyle.Auto;
                element.CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.Auto);
                element.IsShowMessage = true;
                _Elements[CommandType.SetBackgroundAuto] = element;
            }

            // SetBackgroundCheck
            {
                var element = new CommandElement();
                element.Group = "背景";
                element.Text = "背景をチェック模様にする";
                element.Note = "背景をチェック模様にします";
                element.Execute = (s, e) => _VM.Background = BackgroundStyle.Check;
                element.CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.Check);
                element.IsShowMessage = true;
                _Elements[CommandType.SetBackgroundCheck] = element;
            }

            // ToggleTopmost
            {
                var element = new CommandElement();
                element.Group = "ウィンドウ";
                element.Text = "常に手前に表示ON/OFF";
                element.MenuText = "常に手前に表示";
                element.Note = "ウィンドウを常に手前に表示します";
                element.Execute = (s, e) => _VM.ToggleTopmost();
                element.ExecuteMessage = e => _VM.IsTopmost ? "「常に手前に表示」を解除" : "常に手前に表示する";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsTopmost));
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleTopmost] = element;
            }
            // ToggleHideMenu
            {
                var element = new CommandElement();
                element.Group = "ウィンドウ";
                element.Text = "メニューを自動的に隠すON/OFF";
                element.MenuText = "メニューを自動的に隠す";
                element.Note = "メニュー、スライダーを非表示にします。カーソルをウィンドウ上端、下端に合わせることで表示されます";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.ToggleHideMenu();
                element.ExecuteMessage = e => _VM.IsHideMenu ? "メニューを表示する" : "メニューを自動的に隠す";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsHideMenu));
                _Elements[CommandType.ToggleHideMenu] = element;
            }
            // ToggleHidePanel
            {
                var element = new CommandElement();
                element.Group = "ウィンドウ";
                element.Text = "パネルを自動的に隠すON/OFF";
                element.MenuText = "パネルを自動的に隠す";
                element.Note = "左右のパネルを自動的に隠します。カーソルをウィンドウ左端、右端に合わせることで表示されます";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.ToggleHidePanel();
                element.ExecuteMessage = e => _VM.IsHidePanel ? "パネルを表示する" : "パネルを自動的に隠す";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsHidePanel));
                _Elements[CommandType.ToggleHidePanel] = element;
            }
            // ToggleHideTitleBar
            // 欠番
            {
                var element = new CommandElement();
                element.Group = "dummy";
                element.Text = "dummy";
                element.Execute = (s, e) => { return; };
                _Elements[CommandType.ToggleHideTitleBar] = element;
            }
            // ToggleVisibleTitleBar
            {
                var element = new CommandElement();
                element.Group = "ウィンドウ";
                element.Text = "タイトルバーON/OFF";
                element.MenuText = "タイトルバー";
                element.Note = "ウィンドウタイトルバーの表示/非表示を切り替えます";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.ToggleVisibleTitleBar();
                element.ExecuteMessage = e => _VM.IsVisibleTitleBar ? "タイトルバーを消す" : "タイトルバー表示する";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleTitleBar));
                _Elements[CommandType.ToggleVisibleTitleBar] = element;
            }
            // ToggleVisibleAddressBar
            {
                var element = new CommandElement();
                element.Group = "ウィンドウ";
                element.Text = "アドレスバーON/OFF";
                element.MenuText = "アドレスバー";
                element.Note = "アドレスバーの表示/非表示を切り替えます";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.ToggleVisibleAddressBar();
                element.ExecuteMessage = e => _VM.IsVisibleAddressBar ? "アドレスバーを消す" : "アドレスバーを表示する";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleAddressBar));
                _Elements[CommandType.ToggleVisibleAddressBar] = element;
            }
            // ToggleVisibleFileInfo
            {
                var element = new CommandElement();
                element.Group = "パネル";
                element.Text = "ファイル情報の表示ON/OFF";
                element.MenuText = "ファイル情報";
                element.Note = "ファイル情報パネルの表示/非表示を切り替えます。ファイル情報パネルは右側に表示されます";
                element.ShortCutKey = "I";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.ToggleVisibleFileInfo(e is MenuCommandTag);
                element.ExecuteMessage = e => _VM.IsVisibleFileInfo ? "ファイル情報を消す" : "ファイル情報を表示する";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleFileInfo));
                _Elements[CommandType.ToggleVisibleFileInfo] = element;
            }
            // ToggleVisibleEffectInfo
            {
                var element = new CommandElement();
                element.Group = "パネル";
                element.Text = "エフェクト設定の表示ON/OFF";
                element.MenuText = "エフェクト設定";
                element.Note = "エフェクト設定パネルの表示/非表示を切り替えます。エフェクト設定パネルは右側に表示されます";
                element.ShortCutKey = "E";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.ToggleVisibleEffectInfo(e is MenuCommandTag);
                element.ExecuteMessage = e => _VM.IsVisibleEffectInfo ? "エフェクト設定を消す" : "エフェクト設定を表示する";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleEffectInfo));
                _Elements[CommandType.ToggleVisibleEffectInfo] = element;
            }
            // ToggleVisibleFolderList
            {
                var element = new CommandElement();
                element.Group = "パネル";
                element.Text = "フォルダーリストの表示ON/OFF";
                element.MenuText = "フォルダーリスト";
                element.Note = "フォルダーリストパネルの表示/非表示を切り替えます";
                element.ShortCutKey = "F";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.ToggleVisibleFolderList(e is MenuCommandTag);
                element.ExecuteMessage = e => _VM.IsVisibleFolderList ? "フォルダーリストを消す" : "フォルダーリストを表示する";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleFolderList));
                _Elements[CommandType.ToggleVisibleFolderList] = element;
            }
            // ToggleVisibleBookmarkList
            {
                var element = new CommandElement();
                element.Group = "パネル";
                element.Text = "ブックマークの表示ON/OFF";
                element.MenuText = "ブックマークリスト";
                element.Note = "ブックマークリストパネルの表示/非表示を切り替えます";
                element.ShortCutKey = "B";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.ToggleVisibleBookmarkList(e is MenuCommandTag);
                element.ExecuteMessage = e => _VM.IsVisibleBookmarkList ? "ブックマークリストを消す" : "ブックマークリストを表示する";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleBookmarkList));
                _Elements[CommandType.ToggleVisibleBookmarkList] = element;
            }
            // ToggleVisiblePagemarkList
            {
                var element = new CommandElement();
                element.Group = "パネル";
                element.Text = "ページマークの表示ON/OFF";
                element.MenuText = "ページマークリスト";
                element.Note = "ページマークリストパネルの表示/非表示を切り替えます";
                element.ShortCutKey = "M";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.ToggleVisiblePagemarkList(e is MenuCommandTag);
                element.ExecuteMessage = e => _VM.IsVisiblePagemarkList ? "ページマークリストを消す" : "ページマークリストを表示する";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisiblePagemarkList));
                _Elements[CommandType.ToggleVisiblePagemarkList] = element;
            }
            // ToggleVisibleHistoryList
            {
                var element = new CommandElement();
                element.Group = "パネル";
                element.Text = "履歴リストの表示ON/OFF";
                element.MenuText = "履歴リスト";
                element.Note = "履歴リストパネルの表示/非表示を切り替えます";
                element.ShortCutKey = "H";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.ToggleVisibleHistoryList(e is MenuCommandTag);
                element.ExecuteMessage = e => _VM.IsVisibleHistoryList ? "履歴リストを消す" : "履歴リストを表示する";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisibleHistoryList));
                _Elements[CommandType.ToggleVisibleHistoryList] = element;
            }
            // ToggleVisiblePageList
            {
                var element = new CommandElement();
                element.Group = "パネル";
                element.Text = "ページリストの表示ON/OFF";
                element.MenuText = "ページリスト";
                element.Note = "ページリスト表示/非表示を切り替えます。フォルダーリストは表示状態になります";
                element.ShortCutKey = "P";
                element.IsShowMessage = false;
                element.ExecuteMessage = e => _VM.IsVisiblePageList ? "ページリストを消す" : "ページリストを表示する";
                element.Execute = (s, e) => _VM.ToggleVisiblePageList();
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsVisiblePageListMenu), System.Windows.Data.BindingMode.OneWay);
                _Elements[CommandType.ToggleVisiblePageList] = element;
            }

            // ToggleVisibleThumbnailList
            {
                var element = new CommandElement();
                element.Group = "サムネイルリスト";
                element.Text = "サムネイルリストの表示ON/OFF";
                element.MenuText = "サムネイルリスト";
                element.Note = "サムネイルリスト表示/非表示を切り替えます";
                element.IsShowMessage = false;
                element.ExecuteMessage = e => _VM.IsEnableThumbnailList ? "サムネイルリストを消す" : "サムネイルリストを表示する";
                element.Execute = (s, e) => _VM.ToggleVisibleThumbnailList();
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsEnableThumbnailList));
                _Elements[CommandType.ToggleVisibleThumbnailList] = element;
            }
            // ToggleHideThumbnailList
            {
                var element = new CommandElement();
                element.Group = "サムネイルリスト";
                element.Text = "サムネイルリストを自動的に隠すON/OFF";
                element.MenuText = "サムネイルリストを自動的に隠す";
                element.Note = "スライダーを使用している時だけサムネイルリストを表示するようにします";
                element.IsShowMessage = false;
                element.ExecuteMessage = e => _VM.IsHideThumbnailList ? "サムネイルリストを表示する" : "サムネイルリストを自動的に隠す";
                element.Execute = (s, e) => _VM.ToggleHideThumbnailList();
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsHideThumbnailList));
                _Elements[CommandType.ToggleHideThumbnailList] = element;
            }


            // ToggleFullScreen
            {
                var element = new CommandElement();
                element.Group = "ウィンドウ";
                element.Text = "フルスクリーンON/OFF";
                element.MenuText = "フルスクリーン";
                element.Note = "フルスクリーン状態を切替ます";
                element.ShortCutKey = "F11";
                element.MouseGesture = "U";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.ToggleFullScreen();
                element.ExecuteMessage = e => _VM.IsFullScreen ? "フルスクリーンOFF" : "フルスクリーンON";
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.IsFullScreen));
                _Elements[CommandType.ToggleFullScreen] = element;
            }
            // SetFullScreen
            {
                var element = new CommandElement();
                element.Group = "ウィンドウ";
                element.Text = "フルスクリーンにする";
                element.Note = "フルスクリーンにします";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.IsFullScreen = true;
                element.CanExecute = () => true;
                _Elements[CommandType.SetFullScreen] = element;
            }
            // CancelFullScreen
            {
                var element = new CommandElement();
                element.Group = "ウィンドウ";
                element.Text = "フルスクリーン解除";
                element.Note = "フルスクリーンを解除します";
                element.ShortCutKey = "Escape";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.IsFullScreen = false;
                element.CanExecute = () => true;
                _Elements[CommandType.CancelFullScreen] = element;
            }

            // ToggleSlideShow
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "スライドショー再生/停止";
                element.MenuText = "スライドショー";
                element.Note = "スライドショーの再生/停止を切り替えます";
                element.ShortCutKey = "F5";
                element.Execute = (s, e) => _Book.ToggleSlideShow();
                element.ExecuteMessage = e => _Book.IsEnableSlideShow ? "スライドショー停止" : "スライドショー再生";
                element.CreateIsCheckedBinding = () => BindingGenerator.BindingBookHub(nameof(_Book.IsEnableSlideShow));
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleSlideShow] = element;
            }
            // ViewScrollUp
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "スクロール↑";
                element.Note = "画像を上方向にするロールさせます。縦スクロールできないときは横スクロールになります";
                element.ShortCutKey = "WheelUp";
                element.IsShowMessage = false;
                _Elements[CommandType.ViewScrollUp] = element;
            }
            // ViewScrollDown
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "スクロール↓";
                element.Note = "画像を下方向にするロールさせます。縦スクロールできないときは横スクロールになります";
                element.ShortCutKey = "WheelDown";
                element.IsShowMessage = false;
                _Elements[CommandType.ViewScrollDown] = element;
            }
            // ViewScaleUp
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "拡大";
                element.Note = "画像を拡大します";
                element.ShortCutKey = "RightButton+WheelUp";
                element.IsShowMessage = false;
                element.DefaultParameter = new ViewScaleCommandParameter() { Scale = 20 };
                _Elements[CommandType.ViewScaleUp] = element;
            }
            // ViewScaleDown
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "縮小";
                element.Note = "画像を縮小します";
                element.ShortCutKey = "RightButton+WheelDown";
                element.IsShowMessage = false;
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.ViewScaleUp };
                _Elements[CommandType.ViewScaleDown] = element;
            }
            // ViewRotateLeft
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "左回転";
                element.Note = "画像を左回転させます";
                element.IsShowMessage = false;
                element.DefaultParameter = new ViewRotateCommandParameter() { Angle = 45 };
                _Elements[CommandType.ViewRotateLeft] = element;
            }
            // ViewRotateRight
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "右回転";
                element.Note = "画像を右回転させます";
                element.IsShowMessage = false;
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.ViewRotateLeft };
                _Elements[CommandType.ViewRotateRight] = element;
            }
            // ToggleViewFlipHorizontal
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "左右反転";
                element.Note = "画像を左右反転させます";
                element.IsShowMessage = false;
                element.CreateIsCheckedBinding = () => BindingGenerator.IsFlipHorizontal();
                _Elements[CommandType.ToggleViewFlipHorizontal] = element;
            }
            // ViewFlipHorizontalOn
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "左右反転ON";
                element.Note = "左右反転状態にします";
                element.IsShowMessage = false;
                _Elements[CommandType.ViewFlipHorizontalOn] = element;
            }
            // ViewFlipHorizontalOff
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "左右反転OFF";
                element.Note = "左右反転状態を解除します";
                element.IsShowMessage = false;
                _Elements[CommandType.ViewFlipHorizontalOff] = element;
            }


            // ToggleViewFlipVertical
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "上下反転";
                element.Note = "画像を上下反転させます";
                element.IsShowMessage = false;
                element.CreateIsCheckedBinding = () => BindingGenerator.IsFlipVertical();
                _Elements[CommandType.ToggleViewFlipVertical] = element;
            }
            // ViewFlipVerticalOn
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "上下反転ON";
                element.Note = "上下反転状態にします";
                element.IsShowMessage = false;
                _Elements[CommandType.ViewFlipVerticalOn] = element;
            }
            // ViewFlipVerticalOff
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "上下反転OFF";
                element.Note = "上下反転状態を解除します";
                element.IsShowMessage = false;
                _Elements[CommandType.ViewFlipVerticalOff] = element;
            }

            // ViewReset
            {
                var element = new CommandElement();
                element.Group = "ビュー操作";
                element.Text = "ビューリセット";
                element.Note = "ビュー操作での回転、拡縮、移動、反転を初期化します";
                element.IsShowMessage = false;
                _Elements[CommandType.ViewReset] = element;
            }

            // PrevPage
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "前のページに戻る";
                element.Note = "ページ前方向に移動します。2ページ表示の場合は2ページ分移動します";
                element.ShortCutKey = "Right,RightClick";
                element.MouseGesture = "R";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.PrevPage();
                _Elements[CommandType.PrevPage] = element;
            }
            // NextPage
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "次のページへ進む";
                element.Note = "ページ次方向に移動します。2ページ表示の場合は2ページ分移動します";
                element.ShortCutKey = "Left,LeftClick";
                element.MouseGesture = "L";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.NextPage();
                _Elements[CommandType.NextPage] = element;
            }
            // PrevOnePage
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "1ページ戻る";
                element.Note = "1ページだけ前方向に移動します";
                element.MouseGesture = "LR";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.PrevOnePage();
                _Elements[CommandType.PrevOnePage] = element;
            }
            // NextOnePage
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "1ページ進む";
                element.Note = "1ページだけ次方向に移動します";
                element.MouseGesture = "RL";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.NextOnePage();
                _Elements[CommandType.NextOnePage] = element;
            }


            // PrevScrollPage
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "スクロール＋前のページに戻る";
                element.Note = "ページ前方向に画像をスクロールさせます。スクロールできない場合は前ページに移動します";
                element.IsShowMessage = false;
                _Elements[CommandType.PrevScrollPage] = element;
            }
            // NextScrollPage
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "スクロール＋次のページへ進む";
                element.Note = "ページ次方向に画像をスクロールさせます。スクロールできない場合は次ページに移動します";
                element.IsShowMessage = false;
                _Elements[CommandType.NextScrollPage] = element;
            }
            // MovePageWithCursor
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "マウス位置依存でページを前後させる";
                element.Note = "マウスカーソル位置によって移動方向が決まります。 ウィンドウ左にカーソルがあるときは次のページへ進み、右にカーソルがあるときは前のページに戻ります";
                element.IsShowMessage = false;
                _Elements[CommandType.MovePageWithCursor] = element;
            }

            // PrevSizePage
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "設定ページ数戻る";
                element.Note = "設定されたページ数だけ前方向に移動します";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.PrevSizePage(((MoveSizePageCommandParameter)element.Parameter).Size);
                element.DefaultParameter = new MoveSizePageCommandParameter() { Size = 10 };
                _Elements[CommandType.PrevSizePage] = element;
            }

            // NextSizePage
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "設定ページ数進む";
                element.Note = "設定されたページ数だけ次方向に移動します";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.NextSizePage(((MoveSizePageCommandParameter)element.Parameter).Size);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.PrevSizePage };
                _Elements[CommandType.NextSizePage] = element;
            }

            // FirstPage
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "最初のページに移動";
                element.Note = "先頭ページに移動します";
                element.ShortCutKey = "Ctrl+Right";
                element.MouseGesture = "UR";
                element.Execute = (s, e) => _Book.FirstPage();
                element.IsShowMessage = true;
                _Elements[CommandType.FirstPage] = element;
            }
            // LastPage
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "最後のページへ移動";
                element.Note = "終端ページに移動します";
                element.ShortCutKey = "Ctrl+Left";
                element.MouseGesture = "UL";
                element.Execute = (s, e) => _Book.LastPage();
                element.IsShowMessage = true;
                _Elements[CommandType.LastPage] = element;
            }
            // PrevFolder
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "前のフォルダに移動";
                element.Note = "フォルダーリスト上での前のフォルダを読み込みます";
                element.ShortCutKey = "Up";
                element.MouseGesture = "LU";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.PrevFolder();
                _Elements[CommandType.PrevFolder] = element;
            }
            // NextFolder
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "次のフォルダへ移動";
                element.Note = "フォルダーリスト上での次のフォルダを読み込みます";
                element.ShortCutKey = "Down";
                element.MouseGesture = "LD";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.NextFolder();
                _Elements[CommandType.NextFolder] = element;
            }
            // PrevHistory
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "前の履歴に戻る";
                element.Note = "前の古い履歴のフォルダを読み込みます";
                element.ShortCutKey = "Back";
                element.IsShowMessage = false;
                element.CanExecute = () => _Book.CanPrevHistory();
                element.Execute = (s, e) => _Book.PrevHistory();
                _Elements[CommandType.PrevHistory] = element;
            }
            // NextHistory
            {
                var element = new CommandElement();
                element.Group = "移動";
                element.Text = "次の履歴へ進む";
                element.Note = "次の新しい履歴のフォルダを読み込みます";
                element.ShortCutKey = "Shift+Back";
                element.IsShowMessage = false;
                element.CanExecute = () => _Book.CanNextHistory();
                element.Execute = (s, e) => _Book.NextHistory();
                _Elements[CommandType.NextHistory] = element;
            }


            // ToggleFolderOrder
            {
                var element = new CommandElement();
                element.Group = "フォルダ列";
                element.Text = "フォルダーの並び順を切り替える";
                element.Note = "フォルダーの並び順を順番に切り替えます";
                element.Execute = (s, e) => _Book.ToggleFolderOrder();
                element.ExecuteMessage = e => _Book.GetFolderOrder().GetToggle().ToDispString();
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleFolderOrder] = element;
            }
            // SetFolderOrderByFileName
            {
                var element = new CommandElement();
                element.Group = "フォルダ列";
                element.Text = "フォルダ列はファイル名順";
                element.Note = "フォルダーの並びを名前順(昇順)にします";
                element.Execute = (s, e) => _Book.SetFolderOrder(FolderOrder.FileName);
                element.CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.FileName);
                element.IsShowMessage = true;
                _Elements[CommandType.SetFolderOrderByFileName] = element;
            }
            // SetFolderOrderByTimeStamp
            {
                var element = new CommandElement();
                element.Group = "フォルダ列";
                element.Text = "フォルダ列は日付順";
                element.Note = "フォルダーの並びを日付順(降順)にします";
                element.Execute = (s, e) => _Book.SetFolderOrder(FolderOrder.TimeStamp);
                element.CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.TimeStamp);
                element.IsShowMessage = true;
                _Elements[CommandType.SetFolderOrderByTimeStamp] = element;
            }
            // SetFolderOrderByRandom
            {
                var element = new CommandElement();
                element.Group = "フォルダ列";
                element.Text = "フォルダ列はシャッフル";
                element.Note = "フォルダーの並びをシャッフルします";
                element.Execute = (s, e) => _Book.SetFolderOrder(FolderOrder.Random);
                element.CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.Random);
                element.IsShowMessage = true;
                _Elements[CommandType.SetFolderOrderByRandom] = element;
            }

            // TogglePageMode
            {
                var element = new CommandElement();
                element.Group = "ページ表示";
                element.Text = "ページ表示モードを切り替える";
                element.Note = "1ページ表示/2ページ表示を切り替えます";
                element.CanExecute = () => true;
                element.Execute = (s, e) => _Book.TogglePageMode();
                element.ExecuteMessage = e => _Book.BookMemento.PageMode.GetToggle().ToDispString();
                element.IsShowMessage = true;
                _Elements[CommandType.TogglePageMode] = element;
            }
            // SetPageMode1
            {
                var element = new CommandElement();
                element.Group = "ページ表示";
                element.Text = "1ページ表示";
                element.Note = "1ページ表示にします";
                element.ShortCutKey = "Ctrl+1";
                element.MouseGesture = "RU";
                element.Execute = (s, e) => _Book.SetPageMode(PageMode.SinglePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.PageMode(PageMode.SinglePage);
                element.IsShowMessage = true;
                _Elements[CommandType.SetPageMode1] = element;
            }
            // SetPageMode2
            {
                var element = new CommandElement();
                element.Group = "ページ表示";
                element.Text = "2ページ表示";
                element.Note = "2ページ表示にします";
                element.ShortCutKey = "Ctrl+2";
                element.MouseGesture = "RD";
                element.Execute = (s, e) => _Book.SetPageMode(PageMode.WidePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.PageMode(PageMode.WidePage);
                element.IsShowMessage = true;
                _Elements[CommandType.SetPageMode2] = element;
            }
            // ToggleBookReadOrder
            {
                var element = new CommandElement();
                element.Group = "ページ表示";
                element.Text = "右開き、左開きを切り替える";
                element.Note = "右開き、左開きを切り替えます";
                element.CanExecute = () => true;
                element.Execute = (s, e) => _Book.ToggleBookReadOrder();
                element.ExecuteMessage = e => _Book.BookMemento.BookReadOrder.GetToggle().ToDispString();
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleBookReadOrder] = element;
            }
            // SetBookReadOrderRight
            {
                var element = new CommandElement();
                element.Group = "ページ表示";
                element.Text = "右開き";
                element.Note = "読み進む方向を右開きにします。2ページ表示のときに若いページが右になります";
                element.Execute = (s, e) => _Book.SetBookReadOrder(PageReadOrder.RightToLeft);
                element.CreateIsCheckedBinding = () => BindingGenerator.BookReadOrder(PageReadOrder.RightToLeft);
                element.IsShowMessage = true;
                _Elements[CommandType.SetBookReadOrderRight] = element;
            }
            // SetBookReadOrderLeft
            {
                var element = new CommandElement();
                element.Group = "ページ表示";
                element.Text = "左開き";
                element.Note = "読み進む方向を左開きにします。2ページ表示のときに若いページが左になります";
                element.Execute = (s, e) => _Book.SetBookReadOrder(PageReadOrder.LeftToRight);
                element.CreateIsCheckedBinding = () => BindingGenerator.BookReadOrder(PageReadOrder.LeftToRight);
                element.IsShowMessage = true;
                _Elements[CommandType.SetBookReadOrderLeft] = element;
            }

            // ToggleIsSupportedDividePage
            {
                var element = new CommandElement();
                element.Group = "1ページ表示設定";
                element.Text = "横長ページを分割する";
                element.Note = "1ページ表示時、横長ページを分割してページにします";
                element.Execute = (s, e) => _Book.ToggleIsSupportedDividePage();
                element.ExecuteMessage = e => _Book.BookMemento.IsSupportedDividePage ? "横長ページの区別をしない" : "横長ページを分割する";
                element.CanExecute = () => _Book.CanPageModeSubSetting(PageMode.SinglePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_Book.BookMemento.IsSupportedDividePage));
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleIsSupportedDividePage] = element;
            }

            // ToggleIsSupportedWidePage
            {
                var element = new CommandElement();
                element.Group = "2ページ表示設定";
                element.Text = "横長ページを2ページとみなす";
                element.Note = " 2ページ表示時、横長の画像を2ページ分とみなして単独表示します";
                element.Execute = (s, e) => _Book.ToggleIsSupportedWidePage();
                element.ExecuteMessage = e => _Book.BookMemento.IsSupportedWidePage ? "横長ページの区別をしない" : "横長ページを2ページとみなす";
                element.CanExecute = () => _Book.CanPageModeSubSetting(PageMode.WidePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_Book.BookMemento.IsSupportedWidePage));
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleIsSupportedWidePage] = element;
            }
            // ToggleIsSupportedSingleFirstPage
            {
                var element = new CommandElement();
                element.Group = "2ページ表示設定";
                element.Text = "最初のページを単独表示";
                element.Note = "2ページ表示でも最初のページは1ページ表示にします";
                element.Execute = (s, e) => _Book.ToggleIsSupportedSingleFirstPage();
                element.ExecuteMessage = e => _Book.BookMemento.IsSupportedSingleFirstPage ? "最初のページを区別しない" : "最初のページを単独表示";
                element.CanExecute = () => _Book.CanPageModeSubSetting(PageMode.WidePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_Book.BookMemento.IsSupportedSingleFirstPage));
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleIsSupportedSingleFirstPage] = element;
            }
            // ToggleIsSupportedSingleLastPage
            {
                var element = new CommandElement();
                element.Group = "2ページ表示設定";
                element.Text = "最後のページを単独表示";
                element.Note = "2ページ表示でも最後のページは1ページ表示にします";
                element.Execute = (s, e) => _Book.ToggleIsSupportedSingleLastPage();
                element.ExecuteMessage = e => _Book.BookMemento.IsSupportedSingleLastPage ? "最後のページを区別しない" : "最後のページを単独表示";
                element.CanExecute = () => _Book.CanPageModeSubSetting(PageMode.WidePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_Book.BookMemento.IsSupportedSingleLastPage));
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleIsSupportedSingleLastPage] = element;
            }

            // ToggleIsRecursiveFolder
            {
                var element = new CommandElement();
                element.Group = "ページ読込";
                element.Text = "サブフォルダを読み込む";
                element.Note = "フォルダから画像を読み込むときにサブフォルダまたは圧縮ファイルも同時に読み込みます";
                element.Execute = (s, e) => _Book.ToggleIsRecursiveFolder();
                element.ExecuteMessage = e => _Book.BookMemento.IsRecursiveFolder ? "サブフォルダは読み込まない" : "サブフォルダも読み込む";
                element.CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_Book.BookMemento.IsRecursiveFolder));
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleIsRecursiveFolder] = element;
            }

            // ToggleSortMode
            {
                var element = new CommandElement();
                element.Group = "ページ列";
                element.Text = "ページの並び順を切り替える";
                element.Note = "ページの並び順を順番に切り替えます";
                element.CanExecute = () => true;
                element.Execute = (s, e) => _Book.ToggleSortMode();
                element.ExecuteMessage = e => _Book.BookMemento.SortMode.GetToggle().ToDispString();
                element.IsShowMessage = true;
                _Elements[CommandType.ToggleSortMode] = element;
            }
            // SetSortModeFileName
            {
                var element = new CommandElement();
                element.Group = "ページ列";
                element.Text = "ファイル名昇順";
                element.Note = "ページの並び順をファイル名昇順にします";
                element.Execute = (s, e) => _Book.SetSortMode(PageSortMode.FileName);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.FileName);
                element.IsShowMessage = true;
                _Elements[CommandType.SetSortModeFileName] = element;
            }
            // SetSortModeFileNameDescending
            {
                var element = new CommandElement();
                element.Group = "ページ列";
                element.Text = "ファイル名降順";
                element.Note = "ページの並び順をファイル名降順にします";
                element.Execute = (s, e) => _Book.SetSortMode(PageSortMode.FileNameDescending);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.FileNameDescending);
                element.IsShowMessage = true;
                _Elements[CommandType.SetSortModeFileNameDescending] = element;
            }
            // SetSortModeTimeStamp
            {
                var element = new CommandElement();
                element.Group = "ページ列";
                element.Text = "ファイル日付昇順";
                element.Note = "ページの並び順をファイル日付昇順にします";
                element.Execute = (s, e) => _Book.SetSortMode(PageSortMode.TimeStamp);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.TimeStamp);
                element.IsShowMessage = true;
                _Elements[CommandType.SetSortModeTimeStamp] = element;
            }
            // SetSortModeTimeStampDescending
            {
                var element = new CommandElement();
                element.Group = "ページ列";
                element.Text = "ファイル日付降順";
                element.Note = "ページの並び順をファイル日付降順にします";
                element.Execute = (s, e) => _Book.SetSortMode(PageSortMode.TimeStampDescending);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.TimeStampDescending);
                element.IsShowMessage = true;
                _Elements[CommandType.SetSortModeTimeStampDescending] = element;
            }
            // SetSortModeRandom
            {
                var element = new CommandElement();
                element.Group = "ページ列";
                element.Text = "シャッフル";
                element.Note = "ページの並び順をシャッフルます";
                element.Execute = (s, e) => _Book.SetSortMode(PageSortMode.Random);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.Random);
                element.IsShowMessage = true;
                _Elements[CommandType.SetSortModeRandom] = element;
            }


            // Bookmark
            // 欠番
            {
                var element = new CommandElement();
                element.Group = "dummy";
                element.Text = "dummy";
                element.Execute = (s, e) => { return; };
                _Elements[CommandType.Bookmark] = element;
            }
            // ToggleBookmark
            {
                var element = new CommandElement();
                element.Group = "ブックマーク";
                element.Text = "ブックマーク登録/解除";
                element.MenuText = "ブックマーク";
                element.Note = "現在開いているフォルダーのブックマークの登録/解除を切り替えます";
                element.Execute = (s, e) => _Book.ToggleBookmark();
                element.CanExecute = () => _Book.CanBookmark();
                element.ExecuteMessage = e => _Book.IsBookmark(null) ? "ブックマーク解除" : "ブックマークに登録";
                element.IsShowMessage = true;
                element.CreateIsCheckedBinding = () => BindingGenerator.IsBookmark();
                element.ShortCutKey = "Ctrl+D";
                _Elements[CommandType.ToggleBookmark] = element;
            }

            // PrevBookmark
            {
                var element = new CommandElement();
                element.Group = "ブックマーク";
                element.Text = "前のブックマークに移動";
                element.Note = "ブックマークリスト順で前のフォルダーに移動します";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.PrevBookmark();
                _Elements[CommandType.PrevBookmark] = element;
            }
            // NextBookmark
            {
                var element = new CommandElement();
                element.Group = "ブックマーク";
                element.Text = "次のブックマークへ移動";
                element.Note = "ブックマークリスト順で次のフォルダーに移動します";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.NextBookmark();
                _Elements[CommandType.NextBookmark] = element;
            }

            // TogglePagemark
            {
                var element = new CommandElement();
                element.Group = "ページマーク";
                element.Text = "ページマーク登録/解除";
                element.MenuText = "ページマーク";
                element.Note = "現在開いているページのページマークの登録/解除を切り替えます";
                element.Execute = (s, e) => _Book.TogglePagemark();
                element.CanExecute = () => _Book.CanPagemark();
                element.ExecuteMessage = e => _Book.IsMarked() ? "ページマーク解除" : "ページマーク登録";
                element.IsShowMessage = true;
                element.CreateIsCheckedBinding = () => BindingGenerator.IsPagemark();
                element.ShortCutKey = "Ctrl+M";
                _Elements[CommandType.TogglePagemark] = element;
            }

            // PrevPagemark
            {
                var element = new CommandElement();
                element.Group = "ページマーク";
                element.Text = "前のページマークに移動";
                element.Note = "前のページマークに移動します";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.PrevPagemark();
                _Elements[CommandType.PrevPagemark] = element;
            }
            // NextPagemark
            {
                var element = new CommandElement();
                element.Group = "ページマーク";
                element.Text = "次のページマークへ移動";
                element.Note = "次のページマークへ移動します";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _Book.NextPagemark();
                _Elements[CommandType.NextPagemark] = element;
            }

            // PrevPagemarkInBook
            {
                var element = new CommandElement();
                element.Group = "ページマーク";
                element.Text = "フォルダ内の前のページマークに移動";
                element.Note = "現在のフォルダ内で前のページマークに移動します";
                element.IsShowMessage = false;
                element.CanExecute = () => _Book.CanPrevPagemarkInPlace((MovePagemarkCommandParameter)element.Parameter);
                element.Execute = (s, e) => _Book.PrevPagemarkInPlace((MovePagemarkCommandParameter)element.Parameter);
                element.DefaultParameter = new MovePagemarkCommandParameter();
                _Elements[CommandType.PrevPagemarkInBook] = element;
            }
            // NextPagemarkInBook
            {
                var element = new CommandElement();
                element.Group = "ページマーク";
                element.Text = "フォルダ内の次のページマークへ移動";
                element.Note = "現在のフォルダ内で次のページマークへ移動します";
                element.IsShowMessage = false;
                element.CanExecute = () => _Book.CanNextPagemarkInPlace((MovePagemarkCommandParameter)element.Parameter);
                element.Execute = (s, e) => _Book.NextPagemarkInPlace((MovePagemarkCommandParameter)element.Parameter);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.PrevPagemarkInBook };
                _Elements[CommandType.NextPagemarkInBook] = element;
            }



            // ToggleEffectGrayscale
            // 欠番
            {
                var element = new CommandElement();
                element.Group = "dummy";
                element.Text = "(なし)";
                element.Execute = (s, e) => { return; };
                element.CanExecute = () => false;
                _Elements[CommandType.ToggleEffectGrayscale] = element;
            }


#if false
            // ToggleIsLoupe
            {
                var element = new CommandElement();
                element.Group = "ルーペ";
                element.Text = "ルーペON/OFF";
                element.MenuText = "ルーペ";
                element.Note = "ルーペの有効/無効を切り替えます";
                element.Execute = (s, e) => _VM.ToggleIsLoupe();
                element.CanExecute = () => true;
                element.IsShowMessage = false;
                element.CreateIsCheckedBinding = () => BindingGenerator.Binding(nameof(_VM.LoupeIsVisibled), System.Windows.Data.BindingMode.OneWay);
                _Elements[CommandType.ToggleIsLoupe] = element;
            }

            // LoupeZoomIn
            {
                var element = new CommandElement();
                element.Group = "ルーペ";
                element.Text = "ルーペ拡大";
                element.MenuText = "ルーペ拡大";
                element.Note = "ルーペを有効化し、ルーペの拡大率を増加させます";
                element.Execute = (s, e) => _VM.LoupeZoomIn();
                element.CanExecute = () => true;
                element.IsShowMessage = false;
                _Elements[CommandType.LoupeZoomIn] = element;
            }

            // LoupeZoomOut
            {
                var element = new CommandElement();
                element.Group = "ルーペ";
                element.Text = "ルーペ縮小";
                element.MenuText = "ルーペ縮小";
                element.Note = "ルーペを有効化し、ルーペの拡大率を減少させます。等倍まで下げるとルーペ機能は無効になります";
                element.Execute = (s, e) => _VM.LoupeZoomOut();
                element.CanExecute = () => true;
                element.IsShowMessage = false;
                _Elements[CommandType.LoupeZoomOut] = element;
            }

#endif

            // ToggleIsReverseSort
            // 欠番
            {
                var element = new CommandElement();
                element.Group = "dummy";
                element.Text = "dummy";
                element.Execute = (s, e) => { return; };
                _Elements[CommandType.ToggleIsReverseSort] = element;
            }

            // OpenSettingWindow
            {
                var element = new CommandElement();
                element.Group = "その他";
                element.Text = "設定ウィンドウを開く";
                element.MenuText = "設定...";
                element.Note = "設定ウィンドウを開きます";
                element.IsShowMessage = false;
                _Elements[CommandType.OpenSettingWindow] = element;
            }
            // OpenSettingFilesFolder
            {
                var element = new CommandElement();
                element.Group = "その他";
                element.Text = "設定ファイルの場所を開く";
                 element.Note = "設定ファイルが保存されているフォルダを開きます";
                element.IsShowMessage = false;
                _Elements[CommandType.OpenSettingFilesFolder] = element;
            }

            // OpenVersionWindow
            {
                var element = new CommandElement();
                element.Group = "その他";
                element.Text = "バージョン情報を表示する";
                element.MenuText = "NeeView について...";
                element.Note = "バージョン情報を表示します";
                element.IsShowMessage = false;
                _Elements[CommandType.OpenVersionWindow] = element;
            }
            // CloseApplication
            {
                var element = new CommandElement();
                element.Group = "その他";
                element.Text = "アプリを終了する";
                element.MenuText = "アプリを終了";
                element.Note = "このアプリケーションを終了させます";
                element.ShortCutKey = "Alt+F4";
                element.IsShowMessage = false;
                element.CanExecute = () => true;
                _Elements[CommandType.CloseApplication] = element;
            }

            // HelpOnline
            {
                var element = new CommandElement();
                element.Group = "その他";
                element.Text = "オンラインヘルプ";
                element.MenuText = "オンラインヘルプ";
                element.Note = "オンラインヘルプを表示します";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.OpenOnlineHelp();
                element.CanExecute = () => true;
                _Elements[CommandType.HelpOnline] = element;
            }

            // HelpCommandList
            {
                var element = new CommandElement();
                element.Group = "その他";
                element.Text = "コマンドリストを表示する";
                element.MenuText = "コマンド一覧";
                element.Note = "コマンドのヘルプをブラウザで表示します";
                element.IsShowMessage = false;
                element.Execute = (s, e) => this.OpenCommandListHelp();
                element.CanExecute = () => true;
                _Elements[CommandType.HelpCommandList] = element;
            }

            // HelpMainMenu
            {
                var element = new CommandElement();
                element.Group = "その他";
                element.Text = "メインメニューのヘルプを表示する";
                element.MenuText = "メインメニューの説明";
                element.Note = "メインメニューのヘルプをブラウザで表示します";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _VM.OpenMainMenuHelp();
                element.CanExecute = () => true;
                _Elements[CommandType.HelpMainMenu] = element;
            }

            // 並び替え
            //_Elements = _Elements.OrderBy(e => e.Key).ToDictionary(e => e.Key, e => e.Value);

            // デフォルト設定として記憶
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

            // ToggleStrechModeの復元(1.14互換用)
            if (_Elements[CommandType.ToggleStretchMode].IsToggled)
            {
                var flags = ((ToggleStretchModeCommandParameter)_Elements[CommandType.ToggleStretchMode].Parameter).StretchModes;

                Dictionary< PageStretchMode, CommandType > _CommandTable = new Dictionary<PageStretchMode, CommandType>
                {
                    [PageStretchMode.None] = CommandType.SetStretchModeNone,
                    [PageStretchMode.Inside] = CommandType.SetStretchModeInside,
                    [PageStretchMode.Outside] = CommandType.SetStretchModeOutside,
                    [PageStretchMode.Uniform] = CommandType.SetStretchModeUniform,
                    [PageStretchMode.UniformToFill] = CommandType.SetStretchModeUniformToFill,
                    [PageStretchMode.UniformToSize] = CommandType.SetStretchModeUniformToSize,
                    [PageStretchMode.UniformToVertical] = CommandType.SetStretchModeUniformToVertical,
                };

                foreach (var item in _CommandTable)
                {
                    flags[item.Key] = _Elements[item.Value].IsToggled;
                }
            }
        }

#endregion
    }
}
