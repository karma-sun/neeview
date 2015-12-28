using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public enum BookCommandType
    {
        OpenSettingWindow,

        LoadAs,

        ClearHistory,

        PrevPage,
        NextPage,

        FirstPage,
        LastPage,

        PrevFolder,
        NextFolder,

        ToggleFullScreen,

        ToggleStretchMode,
        SetStretchModeNone,
        SetStretchModeInside,
        SetStretchModeOutside,
        SetStretchModeUniform,
        SetStretchModeUniformToFill,

        TogglePageMode,
        SetPageMode1,
        SetPageMode2,

        ToggleBookReadOrder,
        SetBookReadOrderRight,
        SetBookReadOrderLeft,

        ToggleIsSupportedTitlePage,
        ToggleIsSupportedWidePage,

        ToggleIsRecursiveFolder,

        ToggleSortMode,
        SetSortModeFileName,
        SetSortModeFileNameDictionary,
        SetSortModeTimeStamp,
        SetSortModeRandom,

        ToggleIsReverseSort,
        //SetIsReverseSortFalse,
        //SetIsReverseSortTrue,

        ViewScrollUp,
        ViewScrollDown,
        ViewScaleUp,
        ViewScaleDown,
    }


    public static class BookCommandExtension
    {
        public class Header
        {
            public string Group { get; set; }
            public string Text { get; set; }
            public Header(string group, string text)
            {
                Group = group;
                Text = text;
            }
        }

        public static Dictionary<BookCommandType, Header> Headers { get; } = new Dictionary<BookCommandType, Header>
        {
            [BookCommandType.OpenSettingWindow] = new Header("設定", "設定ウィンドウを開く"),

            [BookCommandType.LoadAs] = new Header("ファイル", "ファイルを開く"),
            [BookCommandType.ClearHistory] = new Header("ファイル", "履歴を消去する"),

            [BookCommandType.PrevPage] = new Header("移動", "前のページに戻る"),
            [BookCommandType.NextPage] = new Header("移動", "次のページへ進む"),
            [BookCommandType.FirstPage] = new Header("移動", "最初のページに戻る"),
            [BookCommandType.LastPage] = new Header("移動", "最後のページへ進む"),
            [BookCommandType.PrevFolder] = new Header("移動", "前のフォルダ(書庫)に戻る"),
            [BookCommandType.NextFolder] = new Header("移動", "次のフォルダ(書庫)へ進む"),

            [BookCommandType.ToggleFullScreen] = new Header("ウィンドウ", "フルスクリーン切り替え"),

            [BookCommandType.ToggleStretchMode] = new Header("スケール", "スケール方法を切り替える"),
            [BookCommandType.SetStretchModeNone] = new Header("スケール", "元のサイズで表示する"),
            [BookCommandType.SetStretchModeInside] = new Header("スケール", "大きい場合、ウィンドウサイズに合わせる"),
            [BookCommandType.SetStretchModeOutside] = new Header("スケール", "小さい場合、ウィンドウサイズに広げる"),
            [BookCommandType.SetStretchModeUniform] = new Header("スケール", "ウィンドウサイズに合わせる"),
            [BookCommandType.SetStretchModeUniformToFill] = new Header("スケール", "ウィンドウいっぱいに広げる"),

            [BookCommandType.TogglePageMode] = new Header("ページ表示", "ページ表示を切り替える"),
            [BookCommandType.SetPageMode1] = new Header("ページ表示", "１ページ表示にする"),
            [BookCommandType.SetPageMode2] = new Header("ページ表示", "２ページ表示にする"),

            [BookCommandType.ToggleBookReadOrder] = new Header("２ページ設定", "右開き、左開きを切り替える"),
            [BookCommandType.SetBookReadOrderRight] = new Header("２ページ設定", "右開きにする"),
            [BookCommandType.SetBookReadOrderLeft] = new Header("２ページ設定", "左開きにする"),

            [BookCommandType.ToggleIsSupportedTitlePage] = new Header("２ページ設定", "最初のページをタイトルとみなす"),
            [BookCommandType.ToggleIsSupportedWidePage] = new Header("２ページ設定", "横長のページを２ページ分とみなす"),

            [BookCommandType.ToggleIsRecursiveFolder] = new Header("本設定", "サブフォルダ読み込みON/OFF"),

            [BookCommandType.ToggleSortMode] = new Header("ページ整列", "ページ並び順を切り替える"),
            [BookCommandType.SetSortModeFileName] = new Header("ページ整列", "ファイル名順にする"),
            [BookCommandType.SetSortModeFileNameDictionary] = new Header("ページ整列", "ファイル名(辞書)順にする"),
            [BookCommandType.SetSortModeTimeStamp] = new Header("ページ整列", "ファイル日付順にする"),
            [BookCommandType.SetSortModeRandom] = new Header("ページ整列", "ランダムに並べる"),

            [BookCommandType.ToggleIsReverseSort] = new Header("ページ整列", "正順逆順を切り替える"),
            //[BookCommandType.SetIsReverseSortFalse] = new Header("ページ整列", "正順にする"),
            //[BookCommandType.SetIsReverseSortTrue] = new Header("ページ整列", "逆順にする"),

            [BookCommandType.ViewScrollUp] = new Header("ビュー操作", "スクロール↑"),
            [BookCommandType.ViewScrollDown] = new Header("ビュー操作", "スクロール↓"),
            [BookCommandType.ViewScaleUp] = new Header("ビュー操作", "拡大"),
            [BookCommandType.ViewScaleDown] = new Header("ビュー操作", "縮小"),
        };
    }



    //
    public class BookCommandCollection : Dictionary<BookCommandType, BookCommand>
    {
        public BookCommandShortcutSource ShortcutSource { get; private set; }

        public void Initialize(MainWindowVM vm, BookProxy book, BookCommandShortcutSource source)
        {
            Add(BookCommandType.OpenSettingWindow, new BookCommand(null));
            Add(BookCommandType.LoadAs, new BookCommand(e => book.Load((string)e)));
            Add(BookCommandType.ClearHistory, new BookCommand(null));
            Add(BookCommandType.PrevPage, new BookCommand(e => book.PrevPage()));
            Add(BookCommandType.NextPage, new BookCommand(e => book.NextPage()));
            Add(BookCommandType.FirstPage, new BookCommand(e => book.FirstPage()));
            Add(BookCommandType.LastPage, new BookCommand(e => book.LastPage()));
            Add(BookCommandType.PrevFolder, new BookCommand(e => book.PrevFolder()));
            Add(BookCommandType.NextFolder, new BookCommand(e => book.NextFolder()));
            Add(BookCommandType.ToggleFullScreen, new BookCommand(null));
            Add(BookCommandType.ToggleStretchMode, new BookCommand(e => vm.StretchMode = vm.StretchMode.GetToggle()));
            Add(BookCommandType.SetStretchModeNone, new BookCommand(e => vm.StretchMode = PageStretchMode.None));
            Add(BookCommandType.SetStretchModeInside, new BookCommand(e => vm.StretchMode = PageStretchMode.Inside));
            Add(BookCommandType.SetStretchModeOutside, new BookCommand(e => vm.StretchMode = PageStretchMode.Outside));
            Add(BookCommandType.SetStretchModeUniform, new BookCommand(e => vm.StretchMode = PageStretchMode.Uniform));
            Add(BookCommandType.SetStretchModeUniformToFill, new BookCommand(e => vm.StretchMode = PageStretchMode.UniformToFill));
            Add(BookCommandType.TogglePageMode, new BookCommand(e => book.TogglePageMode()));
            Add(BookCommandType.SetPageMode1, new BookCommand(e => book.SetPageMode(1)));
            Add(BookCommandType.SetPageMode2, new BookCommand(e => book.SetPageMode(2)));
            Add(BookCommandType.ToggleBookReadOrder, new BookCommand(e => book.ToggleBookReadOrder()));
            Add(BookCommandType.SetBookReadOrderRight, new BookCommand(e => book.SetBookReadOrder(BookReadOrder.RightToLeft)));
            Add(BookCommandType.SetBookReadOrderLeft, new BookCommand(e => book.SetBookReadOrder(BookReadOrder.LeftToRight)));
            Add(BookCommandType.ToggleIsSupportedTitlePage, new BookCommand(e => book.ToggleIsSupportedTitlePage()));
            Add(BookCommandType.ToggleIsSupportedWidePage, new BookCommand(e => book.ToggleIsSupportedWidePage()));
            Add(BookCommandType.ToggleIsRecursiveFolder, new BookCommand(e => book.ToggleIsRecursiveFolder()));
            Add(BookCommandType.ToggleSortMode, new BookCommand(e => book.ToggleSortMode()));
            Add(BookCommandType.SetSortModeFileName, new BookCommand(e => book.SetSortMode(BookSortMode.FileName)));
            Add(BookCommandType.SetSortModeFileNameDictionary, new BookCommand(e => book.SetSortMode(BookSortMode.FileNameDictionary)));
            Add(BookCommandType.SetSortModeTimeStamp, new BookCommand(e => book.SetSortMode(BookSortMode.TimeStamp)));
            Add(BookCommandType.SetSortModeRandom, new BookCommand(e => book.SetSortMode(BookSortMode.Random)));
            Add(BookCommandType.ToggleIsReverseSort, new BookCommand(e => book.ToggleIsReverseSort()));
            //Add(BookCommandType.SetIsReverseSortFalse, new BookCommand(e => book.IsReverseSort = false));
            //Add(BookCommandType.SetIsReverseSortTrue, new BookCommand(e => book.IsReverseSort = true));
            Add(BookCommandType.ViewScrollUp, new BookCommand(null));
            Add(BookCommandType.ViewScrollDown, new BookCommand(null));
            Add(BookCommandType.ViewScaleUp, new BookCommand(null));
            Add(BookCommandType.ViewScaleDown, new BookCommand(null));

            SetShortcut(source ?? BookCommandShortcutSource.CreateDefaultShortcutSource());
        }


        public void SetShortcut(BookCommandShortcutSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            ShortcutSource = source;

            foreach (var pair in this)
            {
                var a = source[pair.Key].ShortCutKey;
                pair.Value.ShortCutKey = a;
                pair.Value.MouseGesture = source[pair.Key].MouseGesture;
            }
        }
    }

    [DataContract]
    public class BookCommandIntpuGesture
    {
        [DataMember]
        public string ShortCutKey;
        [DataMember]
        public string MouseGesture;

        public BookCommandIntpuGesture()
        {
        }

        public BookCommandIntpuGesture(string shortCutKey, string mouseGesture)
        {
            ShortCutKey = shortCutKey;
            MouseGesture = mouseGesture;
        }

        public BookCommandIntpuGesture Clone()
        {
            return new BookCommandIntpuGesture(ShortCutKey, MouseGesture);
        }
    }

    //
    public class BookCommandShortcutSource : Dictionary<BookCommandType, BookCommandIntpuGesture>
    {
        public static BookCommandShortcutSource CreateDefaultShortcutSource()
        {
            var source = new BookCommandShortcutSource();
            source.ResetToDefault();
            return source;
        }

        private void ResetToDefault()
        {
            Add(BookCommandType.LoadAs, new BookCommandIntpuGesture("Ctrl+O", null));
            Add(BookCommandType.PrevPage, new BookCommandIntpuGesture("Right,RightClick", "R"));
            Add(BookCommandType.NextPage, new BookCommandIntpuGesture("Left,LeftClick", "L"));
            Add(BookCommandType.PrevFolder, new BookCommandIntpuGesture("Shift+Right", "UR"));
            Add(BookCommandType.NextFolder, new BookCommandIntpuGesture("Shift+Left", "UL"));
            Add(BookCommandType.ToggleFullScreen, new BookCommandIntpuGesture("F12", "U"));
            Add(BookCommandType.TogglePageMode, new BookCommandIntpuGesture("LeftButton+WheelUp", null));
            Add(BookCommandType.SetPageMode1, new BookCommandIntpuGesture("Ctrl+1", null));
            Add(BookCommandType.SetPageMode2, new BookCommandIntpuGesture("Ctrl+2", null));
            Add(BookCommandType.ViewScrollUp, new BookCommandIntpuGesture("WheelUp", null));
            Add(BookCommandType.ViewScrollDown, new BookCommandIntpuGesture("WheelDown", null));
            Add(BookCommandType.ViewScaleUp, new BookCommandIntpuGesture("RightButton+WheelUp", null));
            Add(BookCommandType.ViewScaleDown, new BookCommandIntpuGesture("RightButton+WheelDown", null));

            foreach (BookCommandType type in Enum.GetValues(typeof(BookCommandType)))
            {
                if (!this.ContainsKey(type))
                {
                    Add(type, new BookCommandIntpuGesture(null, null));
                }
            }
        }

        public void Store(BookCommandCollection commands)
        {
            commands.ShortcutSource.CopyTo(this);
        }

        public void Restore(BookCommandCollection commands)
        {
            var source = commands.ShortcutSource;

            // 上書き
            foreach (var pair in this)
            {
                source[pair.Key] = pair.Value;
            }

            commands.SetShortcut(source);
        }

        public void CopyTo(BookCommandShortcutSource target)
        {
            target.Clear();
            foreach (var pair in this)
            {
                target.Add(pair.Key, pair.Value.Clone());
            }
        }
    }

    //
    public class BookCommand
    {
        public Action<object> Command { get; set; }
        public string ShortCutKey { get; set; }
        public string MouseGesture { get; set; }

        public BookCommand(Action<object> command)
        {
            Command = command;
        }

        public void Execute(object param)
        {
            Command(param);
        }
    }




    public class InputGestureConverter
    {
        public InputGesture ConvertFromString(string source)
        {
            try
            {
                KeyGestureConverter converter = new KeyGestureConverter();
                return (KeyGesture)converter.ConvertFromString(source);
            }
            catch { }

            try
            {
                MouseGestureConverter converter = new MouseGestureConverter();
                return (MouseGesture)converter.ConvertFromString(source);
            }
            catch { }

            // 以下、拡張キーバインド

            try
            {
                KeyExGestureConverter converter = new KeyExGestureConverter();
                return (InputGesture)converter.ConvertFromString(source);
            }
            catch { }

            try
            {
                MouseWheelGestureConverter converter = new MouseWheelGestureConverter();
                return (InputGesture)converter.ConvertFromString(source);
            }
            catch { }

            try
            {
                MouseExGestureConverter converter = new MouseExGestureConverter();
                return (InputGesture)converter.ConvertFromString(source);
            }
            catch { }

            Debug.WriteLine("no support gesture: " + source);
            return null;
        }
    }



}
