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
        PrevOnePage,
        NextOnePage,

        FirstPage,
        LastPage,

        PrevFolder,
        NextFolder,

        ToggleFolderOrder,
        SetFolderOrderByFileName,
        SetFolderOrderByTimeStamp,
        SetFolderOrderByRandom,

        ToggleFullScreen,
        CancelFullScreen,

        ToggleSlideShow,

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
        //SetSortModeFileNameDictionary,
        SetSortModeTimeStamp,
        SetSortModeRandom,

        ToggleIsReverseSort,

        ViewScrollUp,
        ViewScrollDown,
        ViewScaleUp,
        ViewScaleDown,
        ViewRotateLeft,
        ViewRotateRight,
    }


    public class BookCommandHeader
    {
        public string Group { get; set; }
        public string Text { get; set; }
        public BookCommandHeader(string group, string text)
        {
            Group = group;
            Text = text;
        }
    }

    public static class BookCommandExtension
    {
        public static Dictionary<BookCommandType, BookCommandHeader> Headers { get; } = new Dictionary<BookCommandType, BookCommandHeader>
        {
            [BookCommandType.OpenSettingWindow] = new BookCommandHeader("設定", "設定ウィンドウを開く"),

            [BookCommandType.LoadAs] = new BookCommandHeader("ファイル", "ファイルを開く"),
            [BookCommandType.ClearHistory] = new BookCommandHeader("ファイル", "履歴を消去する"),

            [BookCommandType.ToggleStretchMode] = new BookCommandHeader("表示サイズ", "サイズを切り替える"),
            [BookCommandType.SetStretchModeNone] = new BookCommandHeader("表示サイズ", "オリジナルサイズで表示する"),
            [BookCommandType.SetStretchModeInside] = new BookCommandHeader("表示サイズ", "大きい場合、ウィンドウサイズに合わせる"),
            [BookCommandType.SetStretchModeOutside] = new BookCommandHeader("表示サイズ", "小さい場合、ウィンドウサイズに広げる"),
            [BookCommandType.SetStretchModeUniform] = new BookCommandHeader("表示サイズ", "ウィンドウサイズに合わせる"),
            [BookCommandType.SetStretchModeUniformToFill] = new BookCommandHeader("表示サイズ", "ウィンドウいっぱいに広げる"),

            [BookCommandType.ToggleFullScreen] = new BookCommandHeader("ビュー操作", "フルスクリーン切り替え"),
            [BookCommandType.CancelFullScreen] = new BookCommandHeader("ビュー操作", "フルスクリーン解除"),
            [BookCommandType.ToggleSlideShow] = new BookCommandHeader("ビュー操作", "スライドショーON/OFF"),
            [BookCommandType.ViewScrollUp] = new BookCommandHeader("ビュー操作", "スクロール↑"),
            [BookCommandType.ViewScrollDown] = new BookCommandHeader("ビュー操作", "スクロール↓"),
            [BookCommandType.ViewScaleUp] = new BookCommandHeader("ビュー操作", "拡大"),
            [BookCommandType.ViewScaleDown] = new BookCommandHeader("ビュー操作", "縮小"),
            [BookCommandType.ViewRotateLeft] = new BookCommandHeader("ビュー操作", "左回転"),
            [BookCommandType.ViewRotateRight] = new BookCommandHeader("ビュー操作", "右回転"),

            [BookCommandType.PrevPage] = new BookCommandHeader("移動", "前のページに戻る"),
            [BookCommandType.NextPage] = new BookCommandHeader("移動", "次のページへ進む"),
            [BookCommandType.PrevOnePage] = new BookCommandHeader("移動", "1ページ戻る"),
            [BookCommandType.NextOnePage] = new BookCommandHeader("移動", "1ページ進む"),
            [BookCommandType.FirstPage] = new BookCommandHeader("移動", "最初のページに移動"),
            [BookCommandType.LastPage] = new BookCommandHeader("移動", "最後のページへ移動"),
            [BookCommandType.PrevFolder] = new BookCommandHeader("移動", "前のフォルダに移動"),
            [BookCommandType.NextFolder] = new BookCommandHeader("移動", "次のフォルダへ移動"),

            [BookCommandType.ToggleFolderOrder] = new BookCommandHeader("フォルダ列", "フォルダの並び順を切り替える"),
            [BookCommandType.SetFolderOrderByFileName] = new BookCommandHeader("フォルダ列", "フォルダ列はファイル名順"),
            [BookCommandType.SetFolderOrderByTimeStamp] = new BookCommandHeader("フォルダ列", "フォルダ列は日付順"),
            [BookCommandType.SetFolderOrderByRandom] = new BookCommandHeader("フォルダ列", "フォルダ列はランダム"),

            [BookCommandType.TogglePageMode] = new BookCommandHeader("ページ表示", "単ページ/見開き表示を切り替える"),
            [BookCommandType.SetPageMode1] = new BookCommandHeader("ページ表示", "単ページ表示にする"),
            [BookCommandType.SetPageMode2] = new BookCommandHeader("ページ表示", "見開き表示にする"),
            [BookCommandType.ToggleBookReadOrder] = new BookCommandHeader("ページ表示", "右開き、左開きを切り替える"),
            [BookCommandType.SetBookReadOrderRight] = new BookCommandHeader("ページ表示", "右開きにする"),
            [BookCommandType.SetBookReadOrderLeft] = new BookCommandHeader("ページ表示", "左開きにする"),

            [BookCommandType.ToggleIsSupportedTitlePage] = new BookCommandHeader("見開き設定", "最初のページを単ページ表示"),
            [BookCommandType.ToggleIsSupportedWidePage] = new BookCommandHeader("見開き設定", "横長ページを見開きとみなす"),

            [BookCommandType.ToggleIsRecursiveFolder] = new BookCommandHeader("フォルダ", "サブフォルダ読み込みON/OFF"),

            [BookCommandType.ToggleSortMode] = new BookCommandHeader("ページ列", "ページの並び順を切り替える"),
            [BookCommandType.SetSortModeFileName] = new BookCommandHeader("ページ列", "ファイル名順にする"),
            //[BookCommandType.SetSortModeFileNameDictionary] = new BookCommandHeader("ページ列", "ファイル名辞書順にする"),
            [BookCommandType.SetSortModeTimeStamp] = new BookCommandHeader("ページ列", "ファイル日付順にする"),
            [BookCommandType.SetSortModeRandom] = new BookCommandHeader("ページ列", "ランダムに並べる"),
            [BookCommandType.ToggleIsReverseSort] = new BookCommandHeader("ページ列", "正順、逆順を切り替える"),

        };
    }


    //
    public class BookCommand
    {
        public BookCommandHeader Header { get; set; }
        public Action<object> Command { get; set; }
        public Func<object, string> CommandMessage { get; set; }
        public BookCommandSetting Setting { get; set; }

        public string ShortCutKey => Setting.ShortCutKey;
        public string MouseGesture => Setting.MouseGesture;

        public string GetExecuteMessage(object param)
        {
            if (CommandMessage != null)
            {
                return CommandMessage(param);
            }
            else
            {
                return Header.Text;
            }
        }

        public void Execute(object param)
        {
            Command(param);
        }
    }


    //
    public class BookCommandCollection : Dictionary<BookCommandType, BookCommand>
    {
        private Dictionary<BookCommandType, Action<object>> _Actions;
        private Dictionary<BookCommandType, Func<object, string>> _CommandMessage;

        private void InitializeActions(MainWindowVM vm, BookHub book)
        {
            _Actions = new Dictionary<BookCommandType, Action<object>>();

            _Actions.Add(BookCommandType.OpenSettingWindow, null);
            _Actions.Add(BookCommandType.LoadAs, null);
            _Actions.Add(BookCommandType.ClearHistory, null);
            _Actions.Add(BookCommandType.PrevPage, e => book.PrevPage());
            _Actions.Add(BookCommandType.NextPage, e => book.NextPage());
            _Actions.Add(BookCommandType.PrevOnePage, e => book.PrevOnePage());
            _Actions.Add(BookCommandType.NextOnePage, e => book.NextOnePage());
            _Actions.Add(BookCommandType.FirstPage, e => book.FirstPage());
            _Actions.Add(BookCommandType.LastPage, e => book.LastPage());
            _Actions.Add(BookCommandType.PrevFolder, e => book.PrevFolder());
            _Actions.Add(BookCommandType.NextFolder, e => book.NextFolder());

            _Actions.Add(BookCommandType.ToggleFolderOrder, e => book.ToggleFolderOrder());
            _Actions.Add(BookCommandType.SetFolderOrderByFileName, e => book.SetFolderOrder(FolderOrder.FileName));
            _Actions.Add(BookCommandType.SetFolderOrderByTimeStamp, e => book.SetFolderOrder(FolderOrder.TimeStamp));
            _Actions.Add(BookCommandType.SetFolderOrderByRandom, e => book.SetFolderOrder(FolderOrder.Random));

            _Actions.Add(BookCommandType.ToggleFullScreen, null);
            _Actions.Add(BookCommandType.CancelFullScreen, null);
            _Actions.Add(BookCommandType.ToggleSlideShow, e => book.ToggleSlideShow());
            _Actions.Add(BookCommandType.ToggleStretchMode, e => vm.StretchMode = vm.StretchMode.GetToggle());
            _Actions.Add(BookCommandType.SetStretchModeNone, e => vm.StretchMode = PageStretchMode.None);
            _Actions.Add(BookCommandType.SetStretchModeInside, e => vm.StretchMode = PageStretchMode.Inside);
            _Actions.Add(BookCommandType.SetStretchModeOutside, e => vm.StretchMode = PageStretchMode.Outside);
            _Actions.Add(BookCommandType.SetStretchModeUniform, e => vm.StretchMode = PageStretchMode.Uniform);
            _Actions.Add(BookCommandType.SetStretchModeUniformToFill, e => vm.StretchMode = PageStretchMode.UniformToFill);
            _Actions.Add(BookCommandType.TogglePageMode, e => book.TogglePageMode());
            _Actions.Add(BookCommandType.SetPageMode1, e => book.SetPageMode(1));
            _Actions.Add(BookCommandType.SetPageMode2, e => book.SetPageMode(2));
            _Actions.Add(BookCommandType.ToggleBookReadOrder, e => book.ToggleBookReadOrder());
            _Actions.Add(BookCommandType.SetBookReadOrderRight, e => book.SetBookReadOrder(PageReadOrder.RightToLeft));
            _Actions.Add(BookCommandType.SetBookReadOrderLeft, e => book.SetBookReadOrder(PageReadOrder.LeftToRight));
            _Actions.Add(BookCommandType.ToggleIsSupportedTitlePage, e => book.ToggleIsSupportedTitlePage());
            _Actions.Add(BookCommandType.ToggleIsSupportedWidePage, e => book.ToggleIsSupportedWidePage());
            _Actions.Add(BookCommandType.ToggleIsRecursiveFolder, e => book.ToggleIsRecursiveFolder());
            _Actions.Add(BookCommandType.ToggleSortMode, e => book.ToggleSortMode());
            _Actions.Add(BookCommandType.SetSortModeFileName, e => book.SetSortMode(PageSortMode.FileName));
            //_Actions.Add(BookCommandType.SetSortModeFileNameDictionary, e => book.SetSortMode(BookSortMode.FileNameDictionary));
            _Actions.Add(BookCommandType.SetSortModeTimeStamp, e => book.SetSortMode(PageSortMode.TimeStamp));
            _Actions.Add(BookCommandType.SetSortModeRandom, e => book.SetSortMode(PageSortMode.Random));
            _Actions.Add(BookCommandType.ToggleIsReverseSort, e => book.ToggleIsReverseSort());
            _Actions.Add(BookCommandType.ViewScrollUp, null);
            _Actions.Add(BookCommandType.ViewScrollDown, null);
            _Actions.Add(BookCommandType.ViewScaleUp, null);
            _Actions.Add(BookCommandType.ViewScaleDown, null);
            _Actions.Add(BookCommandType.ViewRotateLeft, null);
            _Actions.Add(BookCommandType.ViewRotateRight, null);

            // execute message
            _CommandMessage = new Dictionary<BookCommandType, Func<object, string>>();
            _CommandMessage.Add(BookCommandType.ToggleFolderOrder, e => book.FolderOrder.GetToggle().ToDispString());
            _CommandMessage.Add(BookCommandType.ToggleSlideShow, e => book.IsEnableSlideShow ? "スライドショー停止" : "スライドショー開始");
            _CommandMessage.Add(BookCommandType.ToggleStretchMode, e => vm.StretchMode.GetToggle().ToDispString());
            _CommandMessage.Add(BookCommandType.TogglePageMode, e => book.GetTogglePageMode() == 1 ? "単ページ表示" : "見開き表示");
            _CommandMessage.Add(BookCommandType.ToggleBookReadOrder, e => book.BookMemento.BookReadOrder.GetToggle().ToDispString());
            _CommandMessage.Add(BookCommandType.ToggleIsSupportedTitlePage, e => book.BookMemento.IsSupportedTitlePage ? "最初のページを区別しない" : "最初のページを単ページ表示");
            _CommandMessage.Add(BookCommandType.ToggleIsSupportedWidePage, e => book.BookMemento.IsSupportedWidePage ? "横長ページの区別をしない" : "横長ページを見開きとみなす");
            _CommandMessage.Add(BookCommandType.ToggleIsRecursiveFolder, e => book.BookMemento.IsRecursiveFolder ? "サブフォルダは読み込まない" : "サブフォルダも読み込む");
            _CommandMessage.Add(BookCommandType.ToggleSortMode, e => book.BookMemento.SortMode.GetToggle().ToDispString());
            _CommandMessage.Add(BookCommandType.ToggleIsReverseSort, e => book.BookMemento.IsReverseSort ? "正順にする" : "逆順にする");
        }


        public void Initialize(MainWindowVM vm, BookHub book, BookCommandMemento settings)
        {
            InitializeActions(vm, book);

            settings = settings ?? new BookCommandMemento(true);

            foreach (BookCommandType type in Enum.GetValues(typeof(BookCommandType)))
            {
                var item = new BookCommand();
                item.Header = BookCommandExtension.Headers[type];
                item.Command = _Actions[type];
                item.CommandMessage = _CommandMessage.ContainsKey(type) ? _CommandMessage[type] : null;
                item.Setting = settings[type];

                this.Add(type, item);
            }
        }

        //
        public BookCommandMemento CreateMemento()
        {
            var memento = new BookCommandMemento(false);
            foreach (BookCommandType type in Enum.GetValues(typeof(BookCommandType)))
            {
                memento.Add(type, this[type].Setting.Clone());
            }
            return memento;
        }

        //
        public void Restore(BookCommandMemento memento)
        {
            foreach (BookCommandType type in Enum.GetValues(typeof(BookCommandType)))
            {
                this[type].Setting = memento[type];
            }
        }



    }

    [DataContract]
    public class BookCommandSetting
    {
        [DataMember]
        public string ShortCutKey { get; set; }
        [DataMember]
        public string MouseGesture { get; set; }
        [DataMember]
        public bool IsShowMessage { get; set; }

        public BookCommandSetting()
        {
        }

        public BookCommandSetting(string shortCutKey, string mouseGesture, bool isShowMessage = true)
        {
            ShortCutKey = shortCutKey;
            MouseGesture = mouseGesture;
            IsShowMessage = isShowMessage;
        }

        public BookCommandSetting Clone()
        {
            return new BookCommandSetting(ShortCutKey, MouseGesture, IsShowMessage);
        }
    }



    //
    [DataContract]
    public class BookCommandMemento
    {
        [DataMember]
        Dictionary<BookCommandType, BookCommandSetting> _Settings;

        public BookCommandSetting this[BookCommandType type]
        {
            set { _Settings[type] = value; }
            get { return _Settings[type]; }
        }

        public void Add(BookCommandType key, BookCommandSetting value)
        {
            _Settings.Add(key, value);
        }


        private void Constructor()
        {
            _Settings = new Dictionary<BookCommandType, BookCommandSetting>();
        }

        public BookCommandMemento()
        {
            Constructor();
            Validate();
        }

        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }

        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            Validate();
        }

        public BookCommandMemento(bool isDefault)
        {
            Constructor();
            if (isDefault) Validate();
        }

        private void Validate()
        {
            AddWeak(BookCommandType.LoadAs, new BookCommandSetting("Ctrl+O", null, false));
            AddWeak(BookCommandType.PrevPage, new BookCommandSetting("Right,RightClick", "R", false));
            AddWeak(BookCommandType.NextPage, new BookCommandSetting("Left,LeftClick", "L", false));
            AddWeak(BookCommandType.FirstPage, new BookCommandSetting("Ctrl+Right", "UR"));
            AddWeak(BookCommandType.LastPage, new BookCommandSetting("Ctrl+Left", "UL"));
            AddWeak(BookCommandType.PrevFolder, new BookCommandSetting("Up", "LU", false));
            AddWeak(BookCommandType.NextFolder, new BookCommandSetting("Down", "LD", false));
            AddWeak(BookCommandType.ToggleFullScreen, new BookCommandSetting("F12", "U", false));
            AddWeak(BookCommandType.CancelFullScreen, new BookCommandSetting("Escape", null, false));
            AddWeak(BookCommandType.ToggleSlideShow, new BookCommandSetting("F5", null));
            AddWeak(BookCommandType.TogglePageMode, new BookCommandSetting("LeftButton+WheelUp", null));
            AddWeak(BookCommandType.ToggleStretchMode, new BookCommandSetting("LeftButton+WheelDown", null));
            AddWeak(BookCommandType.SetPageMode1, new BookCommandSetting("Ctrl+1", null));
            AddWeak(BookCommandType.SetPageMode2, new BookCommandSetting("Ctrl+2", null));
            AddWeak(BookCommandType.ViewScrollUp, new BookCommandSetting("WheelUp", null, false));
            AddWeak(BookCommandType.ViewScrollDown, new BookCommandSetting("WheelDown", null, false));
            AddWeak(BookCommandType.ViewScaleUp, new BookCommandSetting("RightButton+WheelUp", null, false));
            AddWeak(BookCommandType.ViewScaleDown, new BookCommandSetting("RightButton+WheelDown", null, false));
            AddWeak(BookCommandType.ViewRotateLeft, new BookCommandSetting(null, null, false));
            AddWeak(BookCommandType.ViewRotateRight, new BookCommandSetting(null, null, false));

            foreach (BookCommandType type in Enum.GetValues(typeof(BookCommandType)))
            {
                if (!_Settings.ContainsKey(type))
                {
                    _Settings.Add(type, new BookCommandSetting(null, null));
                }
            }
        }

        private void AddWeak(BookCommandType key, BookCommandSetting value)
        {
            if (!_Settings.ContainsKey(key))
            {
                _Settings.Add(key, value);
            }
        }

        /*
        public void CopyTo(BookCommandMemento target)
        {
            target.Clear();
            foreach (var pair in this)
            {
                target.Add(pair.Key, pair.Value.Clone());
            }
        }
        */
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
