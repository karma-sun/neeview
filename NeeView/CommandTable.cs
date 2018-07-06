using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Resources;

// TODO: コマンド引数にコマンドパラメータを渡せないだろうか。（現状メニュー呼び出しであることを示すタグが指定されることが有る)

namespace NeeView
{
    public enum InputSceme
    {
        TypeA, // 標準
        TypeB, // ホイールでページ送り
        TypeC, // クリックでページ送り
    };

    public class CommandChangedEventArgs : EventArgs
    {
        /// <summary>
        /// キーバインド反映を保留
        /// </summary>
        public bool OnHold;

        public CommandChangedEventArgs(bool onHold)
        {
            this.OnHold = onHold;
        }
    }

    /// <summary>
    /// コマンド設定テーブル
    /// </summary>
    public class CommandTable : BindableBase, IEnumerable<KeyValuePair<CommandType, CommandElement>>
    {
        // SystemObject
        public static CommandTable Current { get; private set; }

        #region Fields

        private static Memento s_defaultMemento;

        private Dictionary<CommandType, CommandElement> _elements;
        private Models _models;
        private BookHub _book;
        private bool _isReversePageMove = true;
        private bool _isReversePageMoveWheel;

        #endregion

        #region Constructors

        // コンストラクタ
        public CommandTable()
        {
            if (Current != null) throw new InvalidOperationException();
            Current = this;

            InitializeCommandTable();
        }

        #endregion

        #region Events

        /// <summary>
        /// コマンドテーブルが変更された
        /// </summary>
        public event EventHandler<CommandChangedEventArgs> Changed;

        #endregion

        #region Properties

        // インテグザ
        public CommandElement this[CommandType key]
        {
            get
            {
                if (!_elements.ContainsKey(key)) throw new ArgumentOutOfRangeException(key.ToString());
                return _elements[key];
            }
            set { _elements[key] = value; }
        }


        [PropertyMember("@ParamCommandIsReversePageMove", Tips = "@ParamCommandIsReversePageMoveTips")]
        public bool IsReversePageMove
        {
            get { return _isReversePageMove; }
            set { if (_isReversePageMove != value) { _isReversePageMove = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamCommandIsReversePageMoveWheel", Tips = "@ParamCommandIsReversePageMoveWheelTips")]
        public bool IsReversePageMoveWheel
        {
            get { return _isReversePageMoveWheel; }
            set { if (_isReversePageMoveWheel != value) { _isReversePageMoveWheel = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region IEnumerable Support

        // Enumerator
        public IEnumerator<KeyValuePair<CommandType, CommandElement>> GetEnumerator()
        {
            foreach (var pair in _elements)
            {
                yield return pair;
            }
        }

        // Enumerator
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Methods

        //
        public bool TryGetValue(CommandType key, out CommandElement command)
        {
            return _elements.TryGetValue(key, out command);
        }

        /// <summary>
        /// 初期設定生成
        /// </summary>
        /// <param name="type">入力スキーム</param>
        /// <returns></returns>
        public static Memento CreateDefaultMemento(InputSceme type)
        {
            var memento = s_defaultMemento.Clone();

            // Type.M
            switch (type)
            {
                case InputSceme.TypeA: // default
                    break;

                case InputSceme.TypeB: // wheel page, right click contextmenu
                    memento.Elements[CommandType.NextScrollPage].ShortCutKey = null;
                    memento.Elements[CommandType.PrevScrollPage].ShortCutKey = null;
                    memento.Elements[CommandType.NextPage].ShortCutKey = "Left,WheelDown";
                    memento.Elements[CommandType.PrevPage].ShortCutKey = "Right,WheelUp";
                    memento.Elements[CommandType.OpenContextMenu].ShortCutKey = "RightClick";
                    break;

                case InputSceme.TypeC: // click page
                    memento.Elements[CommandType.NextScrollPage].ShortCutKey = null;
                    memento.Elements[CommandType.PrevScrollPage].ShortCutKey = null;
                    memento.Elements[CommandType.NextPage].ShortCutKey = "Left,LeftClick";
                    memento.Elements[CommandType.PrevPage].ShortCutKey = "Right,RightClick";
                    memento.Elements[CommandType.ViewScrollUp].ShortCutKey = "WheelUp";
                    memento.Elements[CommandType.ViewScrollDown].ShortCutKey = "WheelDown";
                    break;
            }

            return memento;
        }


        // コマンドターゲット設定
        public void SetTarget(Models models)
        {
            _models = models;
            ////_VM = vm;
            _book = _models.BookHub;
        }

        // .. あまりかわらん
        public T Parameter<T>(CommandType commandType) where T : class
        {
            return _elements[commandType].Parameter as T;
        }


        // ショートカット重複チェック
        public List<CommandType> GetOverlapShortCut(string shortcut)
        {
            var overlaps = _elements
                .Where(e => !string.IsNullOrEmpty(e.Value.ShortCutKey) && e.Value.ShortCutKey.Split(',').Contains(shortcut))
                .Select(e => e.Key)
                .ToList();

            return overlaps;
        }

        // マウスジェスチャー重複チェック
        public List<CommandType> GetOverlapMouseGesture(string gesture)
        {
            var overlaps = _elements
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
            foreach (var command in _elements.Values)
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
                writer.WriteLine(HtmlHelpUtility.CraeteHeader("NeeView Command List"));
                writer.WriteLine($"<body><h1>{Properties.Resources.HelpCommandTitle}</h1>");

                writer.WriteLine($"<p>{Properties.Resources.HelpCommandMessage}</p>");

                // グループごとに出力
                foreach (var pair in groups)
                {
                    writer.WriteLine($"<h3>{pair.Key}</h3>");
                    writer.WriteLine("<table>");
                    writer.WriteLine($"<th>{Properties.Resources.WordCommand}<th>{Properties.Resources.WordShortcut}<th>{Properties.Resources.WordGesture}<th>{Properties.Resources.WordTouch}<th>{Properties.Resources.WordDescription}<tr>");
                    foreach (var command in pair.Value)
                    {
                        writer.WriteLine($"<td>{command.Text}<td>{command.ShortCutKey}<td>{new MouseGestureSequence(command.MouseGesture).ToDispString()}<td>{command.TouchGesture}<td>{command.Note}<tr>");
                    }
                    writer.WriteLine("</table>");
                }
                writer.WriteLine("</body>");

                writer.WriteLine(HtmlHelpUtility.CreateFooter());
            }

            System.Diagnostics.Process.Start(fileName);
        }

        #endregion

        #region Methods: Initialize

        /// <summary>
        /// コマンドテーブル初期化
        /// </summary>
        private void InitializeCommandTable()
        {
            // コマンドの設定定義
            _elements = new Dictionary<CommandType, CommandElement>();

            // None
            // 欠番
            {
                var element = new CommandElement();
                element.Group = "dummy";
                element.Text = "dummy";
                element.Execute = (s, e) => { return; };
                _elements[CommandType.None] = element;
            }


            // LoadAs
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandLoadAs;
                element.MenuText = Properties.Resources.CommandLoadAsMenu;
                element.Note = Properties.Resources.CommandLoadAsNote;
                element.ShortCutKey = "Ctrl+O";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MainWindowModel.LoadAs();
                _elements[CommandType.LoadAs] = element;
            }

            // ReLoad
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandReLoad;
                element.Note = Properties.Resources.CommandReLoadNote;
                element.MouseGesture = "UD";
                element.CanExecute = () => _book.CanReload();
                element.Execute = (s, e) => _book.ReLoad();
                element.IsShowMessage = false;
                _elements[CommandType.ReLoad] = element;
            }

            // Unload
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandUnload;
                element.MenuText = Properties.Resources.CommandUnloadMenu;
                element.Note = Properties.Resources.CommandUnloadNote;
                element.CanExecute = () => _book.CanUnload();
                element.Execute = (s, e) => _book.RequestUnload(true);
                element.IsShowMessage = false;
                _elements[CommandType.Unload] = element;
            }

            // OpenApplication
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandOpenApplication;
                element.Note = Properties.Resources.CommandOpenApplicationNote;
                element.Execute = (s, e) => _models.BookOperation.OpenApplication();
                element.CanExecute = () => _models.BookOperation.CanOpenFilePlace();
                element.IsShowMessage = false;
                _elements[CommandType.OpenApplication] = element;
            }
            // OpenFilePlace
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandOpenFilePlace;
                element.Note = Properties.Resources.CommandOpenFilePlaceNote;
                element.Execute = (s, e) => _models.BookOperation.OpenFilePlace();
                element.CanExecute = () => _models.BookOperation.CanOpenFilePlace();
                element.IsShowMessage = false;
                _elements[CommandType.OpenFilePlace] = element;
            }
            // Export
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandExport;
                element.MenuText = Properties.Resources.CommandExportMenu;
                element.Note = Properties.Resources.CommandExportNote;
                element.ShortCutKey = "Ctrl+S";
                element.Execute = (s, e) => _models.BookOperation.Export();
                element.CanExecute = () => _models.BookOperation.CanExport();
                element.IsShowMessage = false;
                _elements[CommandType.Export] = element;
            }
            // Print
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandPrint;
                element.MenuText = Properties.Resources.CommandPrintMenu;
                element.Note = Properties.Resources.CommandPrintNote;
                element.ShortCutKey = "Ctrl+P";
                //element.Execute = (s, e) => _VM.Print();
                element.CanExecute = () => _models.ContentCanvas.CanPrint();
                element.IsShowMessage = false;
                _elements[CommandType.Print] = element;
            }
            // DeleteFile
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandDeleteFile;
                element.MenuText = Properties.Resources.CommandDeleteFileMenu;
                element.Note = Properties.Resources.CommandDeleteFileNote;
                element.ShortCutKey = "Delete";
                element.Execute = (s, e) => _models.BookOperation.DeleteFile();
                element.CanExecute = () => _models.BookOperation.CanDeleteFile();
                element.IsShowMessage = false;
                _elements[CommandType.DeleteFile] = element;
            }
            // DeleteBook
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandDeleteBook;
                element.MenuText = Properties.Resources.CommandDeleteBookMenu;
                element.Note = Properties.Resources.CommandDeleteBookNote;
                element.Execute = (s, e) => _models.BookOperation.DeleteBook();
                element.CanExecute = () => _models.BookOperation.CanDeleteBook();
                element.IsShowMessage = false;
                _elements[CommandType.DeleteBook] = element;
            }
            // CopyFile
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandCopyFile;
                element.MenuText = Properties.Resources.CommandCopyFileMenu;
                element.Note = Properties.Resources.CommandCopyFileNote;
                element.ShortCutKey = "Ctrl+C";
                element.Execute = (s, e) => _models.BookOperation.CopyToClipboard();
                element.CanExecute = () => _models.BookOperation.CanOpenFilePlace();
                element.IsShowMessage = true;
                _elements[CommandType.CopyFile] = element;
            }
            // CopyImage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandCopyImage;
                element.MenuText = Properties.Resources.CommandCopyImageMenu;
                element.Note = Properties.Resources.CommandCopyImageNote;
                element.ShortCutKey = "Ctrl+Shift+C";
                element.Execute = (s, e) => _models.ContentCanvas.CopyImageToClipboard();
                element.CanExecute = () => _models.ContentCanvas.CanCopyImageToClipboard();
                element.IsShowMessage = true;
                _elements[CommandType.CopyImage] = element;
            }
            // Paste
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandPaste;
                element.MenuText = Properties.Resources.CommandPasteMenu;
                element.Note = Properties.Resources.CommandPasteNote;
                element.ShortCutKey = "Ctrl+V";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.ContentDropManager.LoadFromClipboard();
                element.CanExecute = () => _models.ContentDropManager.CanLoadFromClipboard();
                _elements[CommandType.Paste] = element;
            }


            // ClearHistory
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandClearHistory;
                element.Note = Properties.Resources.CommandClearHistoryNote;
                element.Execute = (s, e) => _models.MainWindowModel.ClearHistory();
                element.IsShowMessage = true;
                _elements[CommandType.ClearHistory] = element;
            }

            // ClearHistoryInPlace
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFile;
                element.Text = Properties.Resources.CommandClearHistoryInPlace;
                element.Note = Properties.Resources.CommandClearHistoryInPlaceNote;
                element.Execute = (s, e) => _models.FolderList.ClearHistory();
                element.IsShowMessage = true;
                _elements[CommandType.ClearHistoryInPlace] = element;
            }


            // ToggleStretchMode
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupImageScale;
                element.Text = Properties.Resources.CommandToggleStretchMode;
                element.Note = Properties.Resources.CommandToggleStretchModeNote;
                element.ShortCutKey = "LeftButton+WheelDown";
                element.Execute = (s, e) => _models.ContentCanvas.StretchMode = _models.ContentCanvas.GetToggleStretchMode((ToggleStretchModeCommandParameter)element.Parameter);
                element.ExecuteMessage = e => _models.ContentCanvas.GetToggleStretchMode((ToggleStretchModeCommandParameter)element.Parameter).ToAliasName();
                element.DefaultParameter = new ToggleStretchModeCommandParameter() { IsLoop = true };
                element.IsShowMessage = true;
                _elements[CommandType.ToggleStretchMode] = element;
            }
            // ToggleStretchModeReverse
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupImageScale;
                element.Text = Properties.Resources.CommandToggleStretchModeReverse;
                element.Note = Properties.Resources.CommandToggleStretchModeReverseNote;
                element.ShortCutKey = "LeftButton+WheelUp";
                element.Execute = (s, e) => _models.ContentCanvas.StretchMode = _models.ContentCanvas.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)element.Parameter);
                element.ExecuteMessage = e => _models.ContentCanvas.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)element.Parameter).ToAliasName();
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.ToggleStretchMode };
                element.IsShowMessage = true;
                _elements[CommandType.ToggleStretchModeReverse] = element;
            }
            // SetStretchModeNone
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupImageScale;
                element.Text = Properties.Resources.CommandSetStretchModeNone;
                element.Note = Properties.Resources.CommandSetStretchModeNoneNote;
                element.Execute = (s, e) => _models.ContentCanvas.StretchMode = PageStretchMode.None;
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.None);
                element.IsShowMessage = true;
                _elements[CommandType.SetStretchModeNone] = element;
            }
            // SetStretchModeInside
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupImageScale;
                element.Text = Properties.Resources.CommandSetStretchModeInside;
                element.Note = Properties.Resources.CommandSetStretchModeInsideNote;
                element.Execute = (s, e) => _models.ContentCanvas.SetStretchMode(PageStretchMode.Inside, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_models.ContentCanvas.TestStretchMode(PageStretchMode.Inside, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.Inside);
                element.DefaultParameter = new StretchModeCommandParameter();
                element.IsShowMessage = true;
                _elements[CommandType.SetStretchModeInside] = element;
            }
            // SetStretchModeOutside
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupImageScale;
                element.Text = Properties.Resources.CommandSetStretchModeOutside;
                element.Note = Properties.Resources.CommandSetStretchModeOutsideNote;
                element.Execute = (s, e) => _models.ContentCanvas.SetStretchMode(PageStretchMode.Outside, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_models.ContentCanvas.TestStretchMode(PageStretchMode.Outside, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.Outside);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.SetStretchModeInside };
                element.IsShowMessage = true;
                _elements[CommandType.SetStretchModeOutside] = element;
            }
            // SetStretchModeUniform
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupImageScale;
                element.Text = Properties.Resources.CommandSetStretchModeUniform;
                element.Note = Properties.Resources.CommandSetStretchModeUniformNote;
                element.Execute = (s, e) => _models.ContentCanvas.SetStretchMode(PageStretchMode.Uniform, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_models.ContentCanvas.TestStretchMode(PageStretchMode.Uniform, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.Uniform);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.SetStretchModeInside };
                element.IsShowMessage = true;
                _elements[CommandType.SetStretchModeUniform] = element;
            }
            // SetStretchModeUniformToFill
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupImageScale;
                element.Text = Properties.Resources.CommandSetStretchModeUniformToFill;
                element.Note = Properties.Resources.CommandSetStretchModeUniformToFillNote;
                element.Execute = (s, e) => _models.ContentCanvas.SetStretchMode(PageStretchMode.UniformToFill, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_models.ContentCanvas.TestStretchMode(PageStretchMode.UniformToFill, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.UniformToFill);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.SetStretchModeInside };
                element.IsShowMessage = true;
                _elements[CommandType.SetStretchModeUniformToFill] = element;
            }
            // SetStretchModeUniformToSize
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupImageScale;
                element.Text = Properties.Resources.CommandSetStretchModeUniformToSize;
                element.Note = Properties.Resources.CommandSetStretchModeUniformToSizeNote;
                element.Execute = (s, e) => _models.ContentCanvas.SetStretchMode(PageStretchMode.UniformToSize, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_models.ContentCanvas.TestStretchMode(PageStretchMode.UniformToSize, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.UniformToSize);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.SetStretchModeInside };
                element.IsShowMessage = true;
                _elements[CommandType.SetStretchModeUniformToSize] = element;
            }
            // SetStretchModeUniformToVertical
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupImageScale;
                element.Text = Properties.Resources.CommandSetStretchModeUniformToVertical;
                element.Note = Properties.Resources.CommandSetStretchModeUniformToVerticalNote;
                element.Execute = (s, e) => _models.ContentCanvas.SetStretchMode(PageStretchMode.UniformToVertical, ((StretchModeCommandParameter)element.Parameter).IsToggle);
                element.ExecuteMessage = e => element.Text + (_models.ContentCanvas.TestStretchMode(PageStretchMode.UniformToVertical, ((StretchModeCommandParameter)element.Parameter).IsToggle) ? "" : " OFF");
                element.CreateIsCheckedBinding = () => BindingGenerator.StretchMode(PageStretchMode.UniformToVertical);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.SetStretchModeInside };
                element.IsShowMessage = true;
                _elements[CommandType.SetStretchModeUniformToVertical] = element;
            }

            // ToggleIsEnabledNearestNeighbor
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupEffect;
                element.Text = Properties.Resources.CommandToggleIsEnabledNearestNeighbor;
                element.MenuText = Properties.Resources.CommandToggleIsEnabledNearestNeighborMenu;
                element.Note = Properties.Resources.CommandToggleIsEnabledNearestNeighborNote;
                element.Execute = (s, e) => _models.ContentCanvas.IsEnabledNearestNeighbor = !_models.ContentCanvas.IsEnabledNearestNeighbor;
                element.ExecuteMessage = e => _models.ContentCanvas.IsEnabledNearestNeighbor ? Properties.Resources.CommandToggleIsEnabledNearestNeighborOff : Properties.Resources.CommandToggleIsEnabledNearestNeighborOn;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.ContentCanvas.IsEnabledNearestNeighbor)) { Source = _models.ContentCanvas };
                element.IsShowMessage = true;
                _elements[CommandType.ToggleIsEnabledNearestNeighbor] = element;
            }

            // ToggleBackground
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupEffect;
                element.Text = Properties.Resources.CommandToggleBackground;
                element.Note = Properties.Resources.CommandToggleBackgroundNote;
                element.Execute = (s, e) => _models.ContentCanvasBrush.Background = _models.ContentCanvasBrush.Background.GetToggle();
                element.ExecuteMessage = e => _models.ContentCanvasBrush.Background.GetToggle().ToAliasName();
                element.IsShowMessage = true;
                _elements[CommandType.ToggleBackground] = element;
            }

            // SetBackgroundBlack
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupEffect;
                element.Text = Properties.Resources.CommandSetBackgroundBlack;
                element.Note = Properties.Resources.CommandSetBackgroundBlackNote;
                element.Execute = (s, e) => _models.ContentCanvasBrush.Background = BackgroundStyle.Black;
                element.CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.Black);
                element.IsShowMessage = true;
                _elements[CommandType.SetBackgroundBlack] = element;
            }

            // SetBackgroundWhite
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupEffect;
                element.Text = Properties.Resources.CommandSetBackgroundWhite;
                element.Note = Properties.Resources.CommandSetBackgroundWhiteNote;
                element.Execute = (s, e) => _models.ContentCanvasBrush.Background = BackgroundStyle.White;
                element.CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.White);
                element.IsShowMessage = true;
                _elements[CommandType.SetBackgroundWhite] = element;
            }

            // SetBackgroundAuto
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupEffect;
                element.Text = Properties.Resources.CommandSetBackgroundAuto;
                element.Note = Properties.Resources.CommandSetBackgroundAutoNote;
                element.Execute = (s, e) => _models.ContentCanvasBrush.Background = BackgroundStyle.Auto;
                element.CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.Auto);
                element.IsShowMessage = true;
                _elements[CommandType.SetBackgroundAuto] = element;
            }

            // SetBackgroundCheck
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupEffect;
                element.Text = Properties.Resources.CommandSetBackgroundCheck;
                element.Note = Properties.Resources.CommandSetBackgroundCheckNote;
                element.Execute = (s, e) => _models.ContentCanvasBrush.Background = BackgroundStyle.Check;
                element.CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.Check);
                element.IsShowMessage = true;
                _elements[CommandType.SetBackgroundCheck] = element;
            }

            // SetBackgroundCustom
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupEffect;
                element.Text = Properties.Resources.CommandSetBackgroundCustom;
                element.Note = Properties.Resources.CommandSetBackgroundCustomNote;
                element.Execute = (s, e) => _models.ContentCanvasBrush.Background = BackgroundStyle.Custom;
                element.CreateIsCheckedBinding = () => BindingGenerator.Background(BackgroundStyle.Custom);
                element.IsShowMessage = true;
                _elements[CommandType.SetBackgroundCustom] = element;
            }

            // ToggleTopmost
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandToggleTopmost;
                element.MenuText = Properties.Resources.CommandToggleTopmostMenu;
                element.Note = Properties.Resources.CommandToggleTopmostNote;
                element.Execute = (s, e) => WindowShape.Current.ToggleTopmost();
                element.ExecuteMessage = e => WindowShape.Current.IsTopmost ? Properties.Resources.CommandToggleTopmostOff : Properties.Resources.CommandToggleTopmostOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(WindowShape.IsTopmost)) { Source = WindowShape.Current, Mode = BindingMode.OneWay };
                element.IsShowMessage = true;
                _elements[CommandType.ToggleTopmost] = element;
            }
            // ToggleHideMenu
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandToggleHideMenu;
                element.MenuText = Properties.Resources.CommandToggleHideMenuMenu;
                element.Note = Properties.Resources.CommandToggleHideMenuNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MainWindowModel.ToggleHideMenu();
                element.ExecuteMessage = e => _models.MainWindowModel.IsHideMenu ? Properties.Resources.CommandToggleHideMenuOff : Properties.Resources.CommandToggleHideMenuOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.MainWindowModel.IsHideMenu)) { Source = _models.MainWindowModel };
                _elements[CommandType.ToggleHideMenu] = element;
            }
            // ToggleHidePageSlider
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandToggleHidePageSlider;
                element.MenuText = Properties.Resources.CommandToggleHidePageSliderMenu;
                element.Note = Properties.Resources.CommandToggleHidePageSliderNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MainWindowModel.ToggleHidePageSlider();
                element.ExecuteMessage = e => _models.MainWindowModel.IsHidePageSlider ? Properties.Resources.CommandToggleHidePageSliderOff : Properties.Resources.CommandToggleHidePageSliderOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.MainWindowModel.IsHidePageSlider)) { Source = _models.MainWindowModel };
                _elements[CommandType.ToggleHidePageSlider] = element;
            }
            // ToggleHidePanel
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandToggleHidePanel;
                element.MenuText = Properties.Resources.CommandToggleHidePanelMenu;
                element.Note = Properties.Resources.CommandToggleHidePanelNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MainWindowModel.ToggleHidePanel();
                element.ExecuteMessage = e => _models.MainWindowModel.IsHidePanel ? Properties.Resources.CommandToggleHidePanelOff : Properties.Resources.CommandToggleHidePanelOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.MainWindowModel.IsHidePanel)) { Source = _models.MainWindowModel };
                _elements[CommandType.ToggleHidePanel] = element;
            }


            // ToggleVisibleTitleBar
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandToggleVisibleTitleBar;
                element.MenuText = Properties.Resources.CommandToggleVisibleTitleBarMenu;
                element.Note = Properties.Resources.CommandToggleVisibleTitleBarNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => WindowShape.Current.ToggleCaptionVisible();
                element.ExecuteMessage = e => WindowShape.Current.IsCaptionVisible ? Properties.Resources.CommandToggleVisibleTitleBarOff : Properties.Resources.CommandToggleVisibleTitleBarOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(WindowShape.IsCaptionVisible)) { Source = WindowShape.Current, Mode = BindingMode.OneWay };
                _elements[CommandType.ToggleVisibleTitleBar] = element;
            }
            // ToggleVisibleAddressBar
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandToggleVisibleAddressBar;
                element.MenuText = Properties.Resources.CommandToggleVisibleAddressBarMenu;
                element.Note = Properties.Resources.CommandToggleVisibleAddressBarNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MainWindowModel.ToggleVisibleAddressBar();
                element.ExecuteMessage = e => _models.MainWindowModel.IsVisibleAddressBar ? Properties.Resources.CommandToggleVisibleAddressBarOff : Properties.Resources.CommandToggleVisibleAddressBarOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.MainWindowModel.IsVisibleAddressBar)) { Source = _models.MainWindowModel };
                _elements[CommandType.ToggleVisibleAddressBar] = element;
            }
            // ToggleVisibleSideBar
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandToggleVisibleSideBar;
                element.MenuText = Properties.Resources.CommandToggleVisibleSideBarMenu;
                element.Note = Properties.Resources.CommandToggleVisibleSideBarNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.SidePanel.IsSideBarVisible = !_models.SidePanel.IsSideBarVisible;
                element.ExecuteMessage = e => _models.SidePanel.IsSideBarVisible ? Properties.Resources.CommandToggleVisibleSideBarOff : Properties.Resources.CommandToggleVisibleSideBarOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(SidePanel.IsSideBarVisible)) { Source = _models.SidePanel };
                _elements[CommandType.ToggleVisibleSideBar] = element;
            }
            // ToggleVisibleFileInfo
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPanel;
                element.Text = Properties.Resources.CommandToggleVisibleFileInfo;
                element.MenuText = Properties.Resources.CommandToggleVisibleFileInfoMenu;
                element.Note = Properties.Resources.CommandToggleVisibleFileInfoNote;
                element.ShortCutKey = "I";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.SidePanel.ToggleVisibleFileInfo(e.Parameter is MenuCommandTag);
                element.ExecuteMessage = e => _models.SidePanel.IsVisibleFileInfo ? Properties.Resources.CommandToggleVisibleFileInfoOff : Properties.Resources.CommandToggleVisibleFileInfoOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(SidePanel.IsVisibleFileInfo)) { Source = _models.SidePanel };
                _elements[CommandType.ToggleVisibleFileInfo] = element;
            }
            // ToggleVisibleEffectInfo
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPanel;
                element.Text = Properties.Resources.CommandToggleVisibleEffectInfo;
                element.MenuText = Properties.Resources.CommandToggleVisibleEffectInfoMenu;
                element.Note = Properties.Resources.CommandToggleVisibleEffectInfoNote;
                element.ShortCutKey = "E";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.SidePanel.ToggleVisibleEffectInfo(e.Parameter is MenuCommandTag);
                element.ExecuteMessage = e => _models.SidePanel.IsVisibleEffectInfo ? Properties.Resources.CommandToggleVisibleEffectInfoOff : Properties.Resources.CommandToggleVisibleEffectInfoOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(SidePanel.IsVisibleEffectInfo)) { Source = _models.SidePanel };
                _elements[CommandType.ToggleVisibleEffectInfo] = element;
            }
            // ToggleVisibleFolderList
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPanel;
                element.Text = Properties.Resources.CommandToggleVisibleFolderList;
                element.MenuText = Properties.Resources.CommandToggleVisibleFolderListMenu;
                element.Note = Properties.Resources.CommandToggleVisibleFolderListNote;
                element.ShortCutKey = "F";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.SidePanel.ToggleVisibleFolderList(e.Parameter is MenuCommandTag);
                element.ExecuteMessage = e => _models.SidePanel.IsVisibleFolderList ? Properties.Resources.CommandToggleVisibleFolderListOff : Properties.Resources.CommandToggleVisibleFolderListOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(SidePanel.IsVisibleFolderList)) { Source = _models.SidePanel };
                _elements[CommandType.ToggleVisibleFolderList] = element;
            }
            // ToggleVisibleBookmarkList
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPanel;
                element.Text = Properties.Resources.CommandToggleVisibleBookmarkList;
                element.MenuText = Properties.Resources.CommandToggleVisibleBookmarkListMenu;
                element.Note = Properties.Resources.CommandToggleVisibleBookmarkListNote;
                element.ShortCutKey = "B";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.SidePanel.ToggleVisibleBookmarkList(e.Parameter is MenuCommandTag);
                element.ExecuteMessage = e => _models.SidePanel.IsVisibleBookmarkList ? Properties.Resources.CommandToggleVisibleBookmarkListOff : Properties.Resources.CommandToggleVisibleBookmarkListOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(SidePanel.IsVisibleBookmarkList)) { Source = _models.SidePanel };
                _elements[CommandType.ToggleVisibleBookmarkList] = element;
            }
            // ToggleVisiblePagemarkList
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPanel;
                element.Text = Properties.Resources.CommandToggleVisiblePagemarkList;
                element.MenuText = Properties.Resources.CommandToggleVisiblePagemarkListMenu;
                element.Note = Properties.Resources.CommandToggleVisiblePagemarkListNote;
                element.ShortCutKey = "M";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.SidePanel.ToggleVisiblePagemarkList(e.Parameter is MenuCommandTag);
                element.ExecuteMessage = e => _models.SidePanel.IsVisiblePagemarkList ? Properties.Resources.CommandToggleVisiblePagemarkListOff : Properties.Resources.CommandToggleVisiblePagemarkListOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(SidePanel.IsVisiblePagemarkList)) { Source = _models.SidePanel };
                _elements[CommandType.ToggleVisiblePagemarkList] = element;
            }
            // ToggleVisibleHistoryList
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPanel;
                element.Text = Properties.Resources.CommandToggleVisibleHistoryList;
                element.MenuText = Properties.Resources.CommandToggleVisibleHistoryListMenu;
                element.Note = Properties.Resources.CommandToggleVisibleHistoryListNote;
                element.ShortCutKey = "H";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.SidePanel.ToggleVisibleHistoryList(e.Parameter is MenuCommandTag);
                element.ExecuteMessage = e => _models.SidePanel.IsVisibleHistoryList ? Properties.Resources.CommandToggleVisibleHistoryListOff : Properties.Resources.CommandToggleVisibleHistoryListOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(SidePanel.IsVisibleHistoryList)) { Source = _models.SidePanel };
                _elements[CommandType.ToggleVisibleHistoryList] = element;
            }
            // ToggleVisiblePageList
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPanel;
                element.Text = Properties.Resources.CommandToggleVisiblePageList;
                element.MenuText = Properties.Resources.CommandToggleVisiblePageListMenu;
                element.Note = Properties.Resources.CommandToggleVisiblePageListNote;
                element.ShortCutKey = "P";
                element.IsShowMessage = false;
                element.ExecuteMessage = e => _models.SidePanel.IsVisiblePageListMenu ? Properties.Resources.CommandToggleVisiblePageListOff : Properties.Resources.CommandToggleVisiblePageListOn;
                element.Execute = (s, e) => _models.SidePanel.ToggleVisiblePageList(e.Parameter is MenuCommandTag);
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.FolderPanelModel.IsPageListVisible)) { Source = _models.FolderPanelModel, Mode = BindingMode.OneWay };
                _elements[CommandType.ToggleVisiblePageList] = element;
            }
            // ToggleVisibleFolderSearchBox
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPanel;
                element.Text = Properties.Resources.CommandToggleVisibleFolderSearchBox;
                element.MenuText = Properties.Resources.CommandToggleVisibleFolderSearchBoxMenu;
                element.Note = Properties.Resources.CommandToggleVisibleFolderSearchBoxNote;
                element.IsShowMessage = false;
                element.ExecuteMessage = e => _models.SidePanel.IsVisibleFolderSearchBox ? Properties.Resources.CommandToggleVisibleFolderSearchBoxOff : Properties.Resources.CommandToggleVisibleFolderSearchBoxOn;
                element.Execute = (s, e) => _models.SidePanel.ToggleVisibleFolderSearchBox(e.Parameter is MenuCommandTag);
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.FolderList.IsFolderSearchBoxVisible)) { Source = _models.FolderList, Mode = BindingMode.OneWay };
                _elements[CommandType.ToggleVisibleFolderSearchBox] = element;
            }
            // ToggleVisibleFoldersTree
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPanel;
                element.Text = Properties.Resources.CommandToggleVisibleFoldersTree;
                element.MenuText = Properties.Resources.CommandToggleVisibleFoldersTreeMenu;
                element.Note = Properties.Resources.CommandToggleVisibleFoldersTreeNote;
                element.IsShowMessage = false;
                element.ExecuteMessage = e => _models.SidePanel.IsVisibleFolderTree ? Properties.Resources.CommandToggleVisibleFoldersTreeOff : Properties.Resources.CommandToggleVisibleFoldersTreeOn;
                element.Execute = (s, e) => _models.SidePanel.ToggleVisibleFolderTree(e.Parameter is MenuCommandTag);
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.FolderList.IsFolderTreeVisible)) { Source = _models.FolderList, Mode = BindingMode.OneWay };
                _elements[CommandType.ToggleVisibleFoldersTree] = element;
            }

            // ToggleVisibleThumbnailList
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFilmStrip;
                element.Text = Properties.Resources.CommandToggleVisibleThumbnailList;
                element.MenuText = Properties.Resources.CommandToggleVisibleThumbnailListMenu;
                element.Note = Properties.Resources.CommandToggleVisibleThumbnailListNote;
                element.IsShowMessage = false;
                element.ExecuteMessage = e => _models.ThumbnailList.IsEnableThumbnailList ? Properties.Resources.CommandToggleVisibleThumbnailListOff : Properties.Resources.CommandToggleVisibleThumbnailListOn;
                element.Execute = (s, e) => _models.ThumbnailList.ToggleVisibleThumbnailList();
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.ThumbnailList.IsEnableThumbnailList)) { Source = _models.ThumbnailList };
                _elements[CommandType.ToggleVisibleThumbnailList] = element;
            }
            // ToggleHideThumbnailList
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupFilmStrip;
                element.Text = Properties.Resources.CommandToggleHideThumbnailList;
                element.MenuText = Properties.Resources.CommandToggleHideThumbnailListMenu;
                element.Note = Properties.Resources.CommandToggleHideThumbnailListNote;
                element.IsShowMessage = false;
                element.ExecuteMessage = e => _models.ThumbnailList.IsHideThumbnailList ? Properties.Resources.CommandToggleHideThumbnailListOff : Properties.Resources.CommandToggleHideThumbnailListOn;
                element.Execute = (s, e) => _models.ThumbnailList.ToggleHideThumbnailList();
                element.CanExecute = () => _models.ThumbnailList.IsEnableThumbnailList;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.ThumbnailList.IsHideThumbnailList)) { Source = _models.ThumbnailList };
                _elements[CommandType.ToggleHideThumbnailList] = element;
            }


            // ToggleFullScreen
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandToggleFullScreen;
                element.MenuText = Properties.Resources.CommandToggleFullScreenMenu;
                element.Note = Properties.Resources.CommandToggleFullScreenNote;
                element.ShortCutKey = "F11";
                element.MouseGesture = "U";
                element.IsShowMessage = false;
                element.Execute = (s, e) => WindowShape.Current.ToggleFullScreen();
                element.ExecuteMessage = e => WindowShape.Current.IsFullScreen ? Properties.Resources.CommandToggleFullScreenOff : Properties.Resources.CommandToggleFullScreenOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(WindowShape.Current.IsFullScreen)) { Source = WindowShape.Current, Mode = BindingMode.OneWay };
                _elements[CommandType.ToggleFullScreen] = element;
            }
            // SetFullScreen
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandSetFullScreen;
                element.Note = Properties.Resources.CommandSetFullScreenNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => WindowShape.Current.SetFullScreen(true);
                element.CanExecute = () => true;
                _elements[CommandType.SetFullScreen] = element;
            }
            // CancelFullScreen
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandCancelFullScreen;
                element.Note = Properties.Resources.CommandCancelFullScreenNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => WindowShape.Current.SetFullScreen(false);
                element.CanExecute = () => true;
                _elements[CommandType.CancelFullScreen] = element;
            }

            // ToggleWindowMinimize
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandToggleWindowMinimize;
                element.MenuText = Properties.Resources.CommandToggleWindowMinimizeMenu;
                element.Note = Properties.Resources.CommandToggleWindowMinimizeNote;
                element.IsShowMessage = false;
                element.CanExecute = () => true;
                _elements[CommandType.ToggleWindowMinimize] = element;
            }

            // ToggleWindowMaximize
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandToggleWindowMaximize;
                element.MenuText = Properties.Resources.CommandToggleWindowMaximizeMenu;
                element.Note = Properties.Resources.CommandToggleWindowMaximizeNote;
                element.IsShowMessage = false;
                element.CanExecute = () => true;
                _elements[CommandType.ToggleWindowMaximize] = element;
            }

            // ShowHiddenPanels
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupWindow;
                element.Text = Properties.Resources.CommandShowHiddenPanels;
                element.MenuText = Properties.Resources.CommandShowHiddenPanelsMenu;
                element.Note = Properties.Resources.CommandShowHiddenPanelsNote;
                element.TouchGesture = "TouchCenter";
                element.CanExecute = () => true;
                element.Execute = (s, e) => _models.MainWindowModel.EnterVisibleLocked();
                element.IsShowMessage = false;
                _elements[CommandType.ShowHiddenPanels] = element;
            }


            // ToggleSlideShow
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandToggleSlideShow;
                element.MenuText = Properties.Resources.CommandToggleSlideShowMenu;
                element.Note = Properties.Resources.CommandToggleSlideShowNote;
                element.ShortCutKey = "F5";
                element.Execute = (s, e) => _models.SlideShow.ToggleSlideShow();
                element.ExecuteMessage = e => _models.SlideShow.IsPlayingSlideShow ? Properties.Resources.CommandToggleSlideShowOff : Properties.Resources.CommandToggleSlideShowOn;
                element.CreateIsCheckedBinding = () => new Binding(nameof(SlideShow.IsPlayingSlideShow)) { Source = _models.SlideShow };
                element.IsShowMessage = true;
                _elements[CommandType.ToggleSlideShow] = element;
            }
            // ViewScrollUp
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewScrollUp;
                element.Note = Properties.Resources.CommandViewScrollUpNote;
                element.IsShowMessage = false;
                element.DefaultParameter = new ViewScrollCommandParameter() { Scroll = 25, AllowCrossScroll = true };
                element.Execute = (s, e) => _models.DragTransformControl.ScrollUp((ViewScrollCommandParameter)element.Parameter);
                _elements[CommandType.ViewScrollUp] = element;
            }
            // ViewScrollDown
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewScrollDown;
                element.Note = Properties.Resources.CommandViewScrollDownNote;
                element.IsShowMessage = false;
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.ViewScrollUp };
                element.Execute = (s, e) => _models.DragTransformControl.ScrollDown((ViewScrollCommandParameter)element.Parameter);
                _elements[CommandType.ViewScrollDown] = element;
            }
            // ViewScrollLeft
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewScrollLeft;
                element.Note = Properties.Resources.CommandViewScrollLeftNote;
                element.IsShowMessage = false;
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.ViewScrollUp };
                element.Execute = (s, e) => _models.DragTransformControl.ScrollLeft((ViewScrollCommandParameter)element.Parameter);
                _elements[CommandType.ViewScrollLeft] = element;
            }
            // ViewScrollRight
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewScrollRight;
                element.Note = Properties.Resources.CommandViewScrollRightNote;
                element.IsShowMessage = false;
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.ViewScrollUp };
                element.Execute = (s, e) => _models.DragTransformControl.ScrollRight((ViewScrollCommandParameter)element.Parameter);
                _elements[CommandType.ViewScrollRight] = element;
            }
            // ViewScaleUp
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewScaleUp;
                element.Note = Properties.Resources.CommandViewScaleUpNote;
                element.ShortCutKey = "RightButton+WheelUp";
                element.IsShowMessage = false;
                element.DefaultParameter = new ViewScaleCommandParameter() { Scale = 20, IsSnapDefaultScale = true };
                element.Execute = (s, e) => { var param = (ViewScaleCommandParameter)element.Parameter; _models.DragTransformControl.ScaleUp(param.Scale / 100.0, param.IsSnapDefaultScale, _models.ContentCanvas.MainContentScale); };
                _elements[CommandType.ViewScaleUp] = element;
            }
            // ViewScaleDown
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewScaleDown;
                element.Note = Properties.Resources.CommandViewScaleDown;
                element.ShortCutKey = "RightButton+WheelDown";
                element.IsShowMessage = false;
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.ViewScaleUp };
                element.Execute = (s, e) => { var param = (ViewScaleCommandParameter)element.Parameter; _models.DragTransformControl.ScaleDown(param.Scale / 100.0, param.IsSnapDefaultScale, _models.ContentCanvas.MainContentScale); };
                _elements[CommandType.ViewScaleDown] = element;
            }
            // ViewRotateLeft
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewRotateLeft;
                element.Note = Properties.Resources.CommandViewRotateLeftNote;
                element.IsShowMessage = false;
                element.DefaultParameter = new ViewRotateCommandParameter() { Angle = 45 };
                element.Execute = (s, e) => _models.ContentCanvas.ViewRotateLeft((ViewRotateCommandParameter)element.Parameter);
                _elements[CommandType.ViewRotateLeft] = element;
            }
            // ViewRotateRight
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewRotateRight;
                element.Note = Properties.Resources.CommandViewRotateRightNote;
                element.IsShowMessage = false;
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.ViewRotateLeft };
                element.Execute = (s, e) => _models.ContentCanvas.ViewRotateRight((ViewRotateCommandParameter)element.Parameter);
                _elements[CommandType.ViewRotateRight] = element;
            }


            // ToggleIsAutoRotate
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandToggleIsAutoRotate;
                element.MenuText = Properties.Resources.CommandToggleIsAutoRotateMenu;
                element.Note = Properties.Resources.CommandToggleIsAutoRotateNote;
                element.Execute = (s, e) => _models.ContentCanvas.ToggleAutoRotate();
                element.ExecuteMessage = e => _models.ContentCanvas.IsAutoRotate ? Properties.Resources.CommandToggleIsAutoRotateOff : Properties.Resources.CommandToggleIsAutoRotateOn;
                element.CreateIsCheckedBinding = () => new Binding(nameof(ContentCanvas.IsAutoRotate)) { Source = _models.ContentCanvas };
                element.DefaultParameter = new AutoRotateCommandParameter();
                element.IsShowMessage = true;
                _elements[CommandType.ToggleIsAutoRotate] = element;
            }

            // ToggleViewFlipHorizontal
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandToggleViewFlipHorizontal;
                element.Note = Properties.Resources.CommandToggleViewFlipHorizontalNote;
                element.IsShowMessage = false;
                element.CreateIsCheckedBinding = () => BindingGenerator.IsFlipHorizontal();
                element.Execute = (s, e) => _models.DragTransformControl.ToggleFlipHorizontal();
                _elements[CommandType.ToggleViewFlipHorizontal] = element;
            }
            // ViewFlipHorizontalOn
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewFlipHorizontalOn;
                element.Note = Properties.Resources.CommandViewFlipHorizontalOnNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.DragTransformControl.FlipHorizontal(true);
                _elements[CommandType.ViewFlipHorizontalOn] = element;
            }
            // ViewFlipHorizontalOff
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewFlipHorizontalOff;
                element.Note = Properties.Resources.CommandViewFlipHorizontalOffNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.DragTransformControl.FlipHorizontal(false);
                _elements[CommandType.ViewFlipHorizontalOff] = element;
            }


            // ToggleViewFlipVertical
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandToggleViewFlipVertical;
                element.Note = Properties.Resources.CommandToggleViewFlipVerticalNote;
                element.IsShowMessage = false;
                element.CreateIsCheckedBinding = () => BindingGenerator.IsFlipVertical();
                element.Execute = (s, e) => _models.DragTransformControl.ToggleFlipVertical();
                _elements[CommandType.ToggleViewFlipVertical] = element;
            }
            // ViewFlipVerticalOn
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewFlipVerticalOn;
                element.Note = Properties.Resources.CommandViewFlipVerticalOnNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.DragTransformControl.FlipVertical(true);
                _elements[CommandType.ViewFlipVerticalOn] = element;
            }
            // ViewFlipVerticalOff
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewFlipVerticalOff;
                element.Note = Properties.Resources.CommandViewFlipVerticalOffNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.DragTransformControl.FlipVertical(false);
                _elements[CommandType.ViewFlipVerticalOff] = element;
            }

            // ViewReset
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandViewReset;
                element.Note = Properties.Resources.CommandViewResetNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.ContentCanvas.ResetTransform(true);
                _elements[CommandType.ViewReset] = element;
            }

            // PrevPage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandPrevPage;
                element.Note = Properties.Resources.CommandPrevPageNote;
                element.ShortCutKey = "Right,RightClick";
                element.TouchGesture = "TouchR1,TouchR2";
                element.MouseGesture = "R";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.BookOperation.PrevPage();
                element.PairPartner = CommandType.NextPage;
                _elements[CommandType.PrevPage] = element;
            }
            // NextPage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandNextPage;
                element.Note = Properties.Resources.CommandNextPageNote;
                element.ShortCutKey = "Left,LeftClick";
                element.TouchGesture = "TouchL1,TouchL2";
                element.MouseGesture = "L";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.BookOperation.NextPage();
                element.PairPartner = CommandType.PrevPage;
                _elements[CommandType.NextPage] = element;
            }
            // PrevOnePage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandPrevOnePage;
                element.Note = Properties.Resources.CommandPrevOnePageNote;
                element.MouseGesture = "LR";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.BookOperation.PrevOnePage();
                element.PairPartner = CommandType.NextOnePage;
                _elements[CommandType.PrevOnePage] = element;
            }
            // NextOnePage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandNextOnePage;
                element.Note = Properties.Resources.CommandNextOnePageNote;
                element.MouseGesture = "RL";
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.BookOperation.NextOnePage();
                element.PairPartner = CommandType.PrevOnePage;
                _elements[CommandType.NextOnePage] = element;
            }


            // PrevScrollPage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandPrevScrollPage;
                element.Note = Properties.Resources.CommandPrevScrollPageNote;
                element.ShortCutKey = "WheelUp";
                element.IsShowMessage = false;
                element.DefaultParameter = new ScrollPageCommandParameter() { IsNScroll = true, IsAnimation = true, Margin = 50, Scroll = 100 };
                element.Execute = (s, e) => _models.MainWindowModel.PrevScrollPage();
                element.PairPartner = CommandType.NextScrollPage;
                _elements[CommandType.PrevScrollPage] = element;
            }
            // NextScrollPage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandNextScrollPage;
                element.Note = Properties.Resources.CommandNextScrollPageNote;
                element.ShortCutKey = "WheelDown";
                element.IsShowMessage = false;
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.PrevScrollPage };
                element.Execute = (s, e) => _models.MainWindowModel.NextScrollPage();
                element.PairPartner = CommandType.PrevScrollPage;
                _elements[CommandType.NextScrollPage] = element;
            }

            // JumpPage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandJumpPage;
                element.Note = Properties.Resources.CommandJumpPageNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.BookOperation.JumpPage();
                _elements[CommandType.JumpPage] = element;
            }


            // PrevSizePage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandPrevSizePage;
                element.Note = Properties.Resources.CommandPrevSizePageNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.BookOperation.PrevSizePage(((MoveSizePageCommandParameter)element.Parameter).Size);
                element.DefaultParameter = new MoveSizePageCommandParameter() { Size = 10 };
                element.PairPartner = CommandType.NextSizePage;
                _elements[CommandType.PrevSizePage] = element;
            }

            // NextSizePage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandNextSizePage;
                element.Note = Properties.Resources.CommandNextSizePageNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.BookOperation.NextSizePage(((MoveSizePageCommandParameter)element.Parameter).Size);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.PrevSizePage };
                element.PairPartner = CommandType.PrevSizePage;
                _elements[CommandType.NextSizePage] = element;
            }

            // FirstPage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandFirstPage;
                element.Note = Properties.Resources.CommandFirstPageNote;
                element.ShortCutKey = "Ctrl+Right";
                element.MouseGesture = "UR";
                element.Execute = (s, e) => _models.BookOperation.FirstPage();
                element.IsShowMessage = true;
                element.PairPartner = CommandType.LastPage;
                _elements[CommandType.FirstPage] = element;
            }
            // LastPage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandLastPage;
                element.Note = Properties.Resources.CommandLastPageNote;
                element.ShortCutKey = "Ctrl+Left";
                element.MouseGesture = "UL";
                element.Execute = (s, e) => _models.BookOperation.LastPage();
                element.IsShowMessage = true;
                element.PairPartner = CommandType.FirstPage;
                _elements[CommandType.LastPage] = element;
            }
            // PrevFolder
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandPrevFolder;
                element.Note = Properties.Resources.CommandPrevFolderNote;
                element.ShortCutKey = "Up";
                element.MouseGesture = "LU";
                element.IsShowMessage = false;
                element.Execute = async (s, e) => await _models.FolderList.PrevFolder();
                _elements[CommandType.PrevFolder] = element;
            }
            // NextFolder
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandNextFolder;
                element.Note = Properties.Resources.CommandNextFolderNote;
                element.ShortCutKey = "Down";
                element.MouseGesture = "LD";
                element.IsShowMessage = false;
                element.Execute = async (s, e) => await _models.FolderList.NextFolder();
                _elements[CommandType.NextFolder] = element;
            }
            // PrevHistory
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandPrevHistory;
                element.Note = Properties.Resources.CommandPrevHistoryNote;
                element.ShortCutKey = "Back";
                element.IsShowMessage = false;
                element.CanExecute = () => _models.BookHistoryCommand.CanPrevHistory();
                element.Execute = (s, e) => _models.BookHistoryCommand.PrevHistory();
                _elements[CommandType.PrevHistory] = element;
            }
            // NextHistory
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupMove;
                element.Text = Properties.Resources.CommandNextHistory;
                element.Note = Properties.Resources.CommandNextHistoryNote;
                element.ShortCutKey = "Shift+Back";
                element.IsShowMessage = false;
                element.CanExecute = () => _models.BookHistoryCommand.CanNextHistory();
                element.Execute = (s, e) => _models.BookHistoryCommand.NextHistory();
                _elements[CommandType.NextHistory] = element;
            }

            // ToggleMediaPlay
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupVideo;
                element.Text = Properties.Resources.CommandToggleMediaPlay;
                element.Note = Properties.Resources.CommandToggleMediaPlayNote;
                element.ExecuteMessage = e => _models.BookOperation.IsMediaPlaying() ? Properties.Resources.WordStop : Properties.Resources.WordPlay;
                element.CanExecute = () => _book.Book != null && _book.Book.IsMedia;
                element.Execute = (s, e) => _models.BookOperation.ToggleMediaPlay(s, e);
                _elements[CommandType.ToggleMediaPlay] = element;
            }

            // ToggleFolderOrder
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupBookOrder;
                element.Text = Properties.Resources.CommandToggleFolderOrder;
                element.Note = Properties.Resources.CommandToggleFolderOrderNote;
                element.Execute = (s, e) => _models.FolderList.ToggleFolderOrder();
                element.ExecuteMessage = e => _models.FolderList.GetFolderOrder().GetToggle().ToAliasName();
                element.IsShowMessage = true;
                _elements[CommandType.ToggleFolderOrder] = element;
            }
            // SetFolderOrderByFileNameA
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupBookOrder;
                element.Text = Properties.Resources.CommandSetFolderOrderByFileNameA;
                element.Note = Properties.Resources.CommandSetFolderOrderByFileNameANote;
                element.Execute = (s, e) => _models.FolderList.SetFolderOrder(FolderOrder.FileName);
                element.CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.FileName);
                element.IsShowMessage = true;
                _elements[CommandType.SetFolderOrderByFileNameA] = element;
            }
            // SetFolderOrderByFileNameD
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupBookOrder;
                element.Text = Properties.Resources.CommandSetFolderOrderByFileNameD;
                element.Note = Properties.Resources.CommandSetFolderOrderByFileNameDNote;
                element.Execute = (s, e) => _models.FolderList.SetFolderOrder(FolderOrder.FileNameDescending);
                element.CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.FileNameDescending);
                element.IsShowMessage = true;
                _elements[CommandType.SetFolderOrderByFileNameD] = element;
            }
            // SetFolderOrderByTimeStampA
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupBookOrder;
                element.Text = Properties.Resources.CommandSetFolderOrderByTimeStampA;
                element.Note = Properties.Resources.CommandSetFolderOrderByTimeStampANote;
                element.Execute = (s, e) => _models.FolderList.SetFolderOrder(FolderOrder.TimeStamp);
                element.CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.TimeStamp);
                element.IsShowMessage = true;
                _elements[CommandType.SetFolderOrderByTimeStampA] = element;
            }
            // SetFolderOrderByTimeStampD
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupBookOrder;
                element.Text = Properties.Resources.CommandSetFolderOrderByTimeStampD;
                element.Note = Properties.Resources.CommandSetFolderOrderByTimeStampDNote;
                element.Execute = (s, e) => _models.FolderList.SetFolderOrder(FolderOrder.TimeStampDescending);
                element.CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.TimeStampDescending);
                element.IsShowMessage = true;
                _elements[CommandType.SetFolderOrderByTimeStampD] = element;
            }
            // SetFolderOrderBySizeA
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupBookOrder;
                element.Text = Properties.Resources.CommandSetFolderOrderBySizeA;
                element.Note = Properties.Resources.CommandSetFolderOrderBySizeANote;
                element.Execute = (s, e) => _models.FolderList.SetFolderOrder(FolderOrder.Size);
                element.CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.Size);
                element.IsShowMessage = true;
                _elements[CommandType.SetFolderOrderBySizeA] = element;
            }
            // SetFolderOrderBySizeD
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupBookOrder;
                element.Text = Properties.Resources.CommandSetFolderOrderBySizeD;
                element.Note = Properties.Resources.CommandSetFolderOrderBySizeDNote;
                element.Execute = (s, e) => _models.FolderList.SetFolderOrder(FolderOrder.SizeDescending);
                element.CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.SizeDescending);
                element.IsShowMessage = true;
                _elements[CommandType.SetFolderOrderBySizeD] = element;
            }
            // SetFolderOrderByRandom
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupBookOrder;
                element.Text = Properties.Resources.CommandSetFolderOrderByRandom;
                element.Note = Properties.Resources.CommandSetFolderOrderByRandomNote;
                element.Execute = (s, e) => _models.FolderList.SetFolderOrder(FolderOrder.Random);
                element.CreateIsCheckedBinding = () => BindingGenerator.FolderOrder(FolderOrder.Random);
                element.IsShowMessage = true;
                _elements[CommandType.SetFolderOrderByRandom] = element;
            }

            // TogglePageMode
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandTogglePageMode;
                element.Note = Properties.Resources.CommandTogglePageModeNote;
                element.CanExecute = () => true;
                element.Execute = (s, e) => _models.BookSetting.TogglePageMode();
                element.ExecuteMessage = e => _models.BookSetting.BookMemento.PageMode.GetToggle().ToAliasName();
                element.IsShowMessage = true;
                _elements[CommandType.TogglePageMode] = element;
            }
            // SetPageMode1
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandSetPageMode1;
                element.Note = Properties.Resources.CommandSetPageMode1Note;
                element.ShortCutKey = "Ctrl+1";
                element.MouseGesture = "RU";
                element.Execute = (s, e) => _models.BookSetting.SetPageMode(PageMode.SinglePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.PageMode(PageMode.SinglePage);
                element.IsShowMessage = true;
                _elements[CommandType.SetPageMode1] = element;
            }
            // SetPageMode2
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandSetPageMode2;
                element.Note = Properties.Resources.CommandSetPageMode2Note;
                element.ShortCutKey = "Ctrl+2";
                element.MouseGesture = "RD";
                element.Execute = (s, e) => _models.BookSetting.SetPageMode(PageMode.WidePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.PageMode(PageMode.WidePage);
                element.IsShowMessage = true;
                _elements[CommandType.SetPageMode2] = element;
            }
            // ToggleBookReadOrder
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandToggleBookReadOrder;
                element.Note = Properties.Resources.CommandToggleBookReadOrderNote;
                element.CanExecute = () => true;
                element.Execute = (s, e) => _models.BookSetting.ToggleBookReadOrder();
                element.ExecuteMessage = e => _models.BookSetting.BookMemento.BookReadOrder.GetToggle().ToAliasName();
                element.IsShowMessage = true;
                _elements[CommandType.ToggleBookReadOrder] = element;
            }
            // SetBookReadOrderRight
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandSetBookReadOrderRight;
                element.Note = Properties.Resources.CommandSetBookReadOrderRightNote;
                element.Execute = (s, e) => _models.BookSetting.SetBookReadOrder(PageReadOrder.RightToLeft);
                element.CreateIsCheckedBinding = () => BindingGenerator.BookReadOrder(PageReadOrder.RightToLeft);
                element.IsShowMessage = true;
                _elements[CommandType.SetBookReadOrderRight] = element;
            }
            // SetBookReadOrderLeft
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandSetBookReadOrderLeft;
                element.Note = Properties.Resources.CommandSetBookReadOrderLeftNote;
                element.Execute = (s, e) => _models.BookSetting.SetBookReadOrder(PageReadOrder.LeftToRight);
                element.CreateIsCheckedBinding = () => BindingGenerator.BookReadOrder(PageReadOrder.LeftToRight);
                element.IsShowMessage = true;
                _elements[CommandType.SetBookReadOrderLeft] = element;
            }

            // ToggleIsSupportedDividePage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandToggleIsSupportedDividePage;
                element.Note = Properties.Resources.CommandToggleIsSupportedDividePageNote;
                element.Execute = (s, e) => _models.BookSetting.ToggleIsSupportedDividePage();
                element.ExecuteMessage = e => _models.BookSetting.BookMemento.IsSupportedDividePage ? Properties.Resources.CommandToggleIsSupportedDividePageOff : Properties.Resources.CommandToggleIsSupportedDividePageOn;
                element.CanExecute = () => _models.BookSetting.CanPageModeSubSetting(PageMode.SinglePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_models.BookSetting.BookMemento.IsSupportedDividePage));
                element.IsShowMessage = true;
                _elements[CommandType.ToggleIsSupportedDividePage] = element;
            }

            // ToggleIsSupportedWidePage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandToggleIsSupportedWidePage;
                element.Note = Properties.Resources.CommandToggleIsSupportedWidePageNote;
                element.Execute = (s, e) => _models.BookSetting.ToggleIsSupportedWidePage();
                element.ExecuteMessage = e => _models.BookSetting.BookMemento.IsSupportedWidePage ? Properties.Resources.CommandToggleIsSupportedWidePageOff : Properties.Resources.CommandToggleIsSupportedWidePageOn;
                element.CanExecute = () => _models.BookSetting.CanPageModeSubSetting(PageMode.WidePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_models.BookSetting.BookMemento.IsSupportedWidePage));
                element.IsShowMessage = true;
                _elements[CommandType.ToggleIsSupportedWidePage] = element;
            }
            // ToggleIsSupportedSingleFirstPage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandToggleIsSupportedSingleFirstPage;
                element.Note = Properties.Resources.CommandToggleIsSupportedSingleFirstPageNote;
                element.Execute = (s, e) => _models.BookSetting.ToggleIsSupportedSingleFirstPage();
                element.ExecuteMessage = e => _models.BookSetting.BookMemento.IsSupportedSingleFirstPage ? Properties.Resources.CommandToggleIsSupportedSingleFirstPageOff : Properties.Resources.CommandToggleIsSupportedSingleFirstPageOn;
                element.CanExecute = () => _models.BookSetting.CanPageModeSubSetting(PageMode.WidePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_models.BookSetting.BookMemento.IsSupportedSingleFirstPage));
                element.IsShowMessage = true;
                _elements[CommandType.ToggleIsSupportedSingleFirstPage] = element;
            }
            // ToggleIsSupportedSingleLastPage
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandToggleIsSupportedSingleLastPage;
                element.Note = Properties.Resources.CommandToggleIsSupportedSingleLastPageNote;
                element.Execute = (s, e) => _models.BookSetting.ToggleIsSupportedSingleLastPage();
                element.ExecuteMessage = e => _models.BookSetting.BookMemento.IsSupportedSingleLastPage ? Properties.Resources.CommandToggleIsSupportedSingleLastPageOff : Properties.Resources.CommandToggleIsSupportedSingleLastPageOn;
                element.CanExecute = () => _models.BookSetting.CanPageModeSubSetting(PageMode.WidePage);
                element.CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_models.BookSetting.BookMemento.IsSupportedSingleLastPage));
                element.IsShowMessage = true;
                _elements[CommandType.ToggleIsSupportedSingleLastPage] = element;
            }

            // ToggleIsRecursiveFolder
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandToggleIsRecursiveFolder;
                element.Note = Properties.Resources.CommandToggleIsRecursiveFolderNote;
                element.Execute = (s, e) => _models.BookSetting.ToggleIsRecursiveFolder();
                element.ExecuteMessage = e => _models.BookSetting.BookMemento.IsRecursiveFolder ? Properties.Resources.CommandToggleIsRecursiveFolderOff : Properties.Resources.CommandToggleIsRecursiveFolderOn;
                element.CreateIsCheckedBinding = () => BindingGenerator.BindingBookSetting(nameof(_models.BookSetting.BookMemento.IsRecursiveFolder));
                element.IsShowMessage = true;
                _elements[CommandType.ToggleIsRecursiveFolder] = element;
            }

            // ToggleSortMode
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageOrder;
                element.Text = Properties.Resources.CommandToggleSortMode;
                element.Note = Properties.Resources.CommandToggleSortModeNote;
                element.CanExecute = () => true;
                element.Execute = (s, e) => _models.BookSetting.ToggleSortMode();
                element.ExecuteMessage = e => _models.BookSetting.BookMemento.SortMode.GetToggle().ToAliasName();
                element.IsShowMessage = true;
                _elements[CommandType.ToggleSortMode] = element;
            }
            // SetSortModeFileName
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageOrder;
                element.Text = Properties.Resources.CommandSetSortModeFileName;
                element.Note = Properties.Resources.CommandSetSortModeFileNameNote;
                element.Execute = (s, e) => _models.BookSetting.SetSortMode(PageSortMode.FileName);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.FileName);
                element.IsShowMessage = true;
                _elements[CommandType.SetSortModeFileName] = element;
            }
            // SetSortModeFileNameDescending
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageOrder;
                element.Text = Properties.Resources.CommandSetSortModeFileNameDescending;
                element.Note = Properties.Resources.CommandSetSortModeFileNameDescendingNote;
                element.Execute = (s, e) => _models.BookSetting.SetSortMode(PageSortMode.FileNameDescending);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.FileNameDescending);
                element.IsShowMessage = true;
                _elements[CommandType.SetSortModeFileNameDescending] = element;
            }
            // SetSortModeTimeStamp
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageOrder;
                element.Text = Properties.Resources.CommandSetSortModeTimeStamp;
                element.Note = Properties.Resources.CommandSetSortModeTimeStampNote;
                element.Execute = (s, e) => _models.BookSetting.SetSortMode(PageSortMode.TimeStamp);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.TimeStamp);
                element.IsShowMessage = true;
                _elements[CommandType.SetSortModeTimeStamp] = element;
            }
            // SetSortModeTimeStampDescending
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageOrder;
                element.Text = Properties.Resources.CommandSetSortModeTimeStampDescending;
                element.Note = Properties.Resources.CommandSetSortModeTimeStampDescendingNote;
                element.Execute = (s, e) => _models.BookSetting.SetSortMode(PageSortMode.TimeStampDescending);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.TimeStampDescending);
                element.IsShowMessage = true;
                _elements[CommandType.SetSortModeTimeStampDescending] = element;
            }
            // SetSortModeSize
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageOrder;
                element.Text = Properties.Resources.CommandSetSortModeSize;
                element.Note = Properties.Resources.CommandSetSortModeSizeNote;
                element.Execute = (s, e) => _models.BookSetting.SetSortMode(PageSortMode.Size);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.Size);
                element.IsShowMessage = true;
                _elements[CommandType.SetSortModeSize] = element;
            }
            // SetSortModeSizeDescending
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageOrder;
                element.Text = Properties.Resources.CommandSetSortModeSizeDescending;
                element.Note = Properties.Resources.CommandSetSortModeSizeDescendingNote;
                element.Execute = (s, e) => _models.BookSetting.SetSortMode(PageSortMode.SizeDescending);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.SizeDescending);
                element.IsShowMessage = true;
                _elements[CommandType.SetSortModeSizeDescending] = element;
            }
            // SetSortModeRandom
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageOrder;
                element.Text = Properties.Resources.CommandSetSortModeRandom;
                element.Note = Properties.Resources.CommandSetSortModeRandomNote;
                element.Execute = (s, e) => _models.BookSetting.SetSortMode(PageSortMode.Random);
                element.CreateIsCheckedBinding = () => BindingGenerator.SortMode(PageSortMode.Random);
                element.IsShowMessage = true;
                _elements[CommandType.SetSortModeRandom] = element;
            }

            // SetDefaultPageSetting
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPageSetting;
                element.Text = Properties.Resources.CommandSetDefaultPageSetting;
                element.Note = Properties.Resources.CommandSetDefaultPageSettingNote;
                element.Execute = (s, e) => _models.BookSetting.SetDefaultPageSetting();
                element.IsShowMessage = true;
                _elements[CommandType.SetDefaultPageSetting] = element;
            }


            // ToggleBookmark
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupBookmark;
                element.Text = Properties.Resources.CommandToggleBookmark;
                element.MenuText = Properties.Resources.CommandToggleBookmarkMenu;
                element.Note = Properties.Resources.CommandToggleBookmarkNote;
                element.Execute = (s, e) => _models.BookOperation.ToggleBookmark();
                element.CanExecute = () => _models.BookOperation.CanBookmark();
                element.ExecuteMessage = e => _models.BookOperation.IsBookmark ? Properties.Resources.CommandToggleBookmarkOff : Properties.Resources.CommandToggleBookmarkOn;
                element.IsShowMessage = true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.BookOperation.IsBookmark)) { Source = _models.BookOperation, Mode = BindingMode.OneWay };
                element.ShortCutKey = "Ctrl+D";
                _elements[CommandType.ToggleBookmark] = element;
            }

            // PrevBookmark
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupBookmark;
                element.Text = Properties.Resources.CommandPrevBookmark;
                element.Note = Properties.Resources.CommandPrevBookmarkNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.BookmarkList.PrevBookmark();
                _elements[CommandType.PrevBookmark] = element;
            }
            // NextBookmark
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupBookmark;
                element.Text = Properties.Resources.CommandNextBookmark;
                element.Note = Properties.Resources.CommandNextBookmarkNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.BookmarkList.NextBookmark();
                _elements[CommandType.NextBookmark] = element;
            }

            // TogglePagemark
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPagemark;
                element.Text = Properties.Resources.CommandTogglePagemark;
                element.MenuText = Properties.Resources.CommandTogglePagemarkMenu;
                element.Note = Properties.Resources.CommandTogglePagemarkNote;
                element.Execute = (s, e) => _models.BookOperation.TogglePagemark();
                element.CanExecute = () => _models.BookOperation.CanPagemark();
                element.ExecuteMessage = e => _models.BookOperation.IsMarked() ? Properties.Resources.CommandTogglePagemarkOff : Properties.Resources.CommandTogglePagemarkOn;
                element.IsShowMessage = true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.BookOperation.IsPagemark)) { Source = _models.BookOperation, Mode = BindingMode.OneWay };
                element.ShortCutKey = "Ctrl+M";
                _elements[CommandType.TogglePagemark] = element;
            }

            // PrevPagemark
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPagemark;
                element.Text = Properties.Resources.CommandPrevPagemark;
                element.Note = Properties.Resources.CommandPrevPagemarkNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.PagemarkList.PrevPagemark();
                _elements[CommandType.PrevPagemark] = element;
            }
            // NextPagemark
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPagemark;
                element.Text = Properties.Resources.CommandNextPagemark;
                element.Note = Properties.Resources.CommandNextPagemarkNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.PagemarkList.NextPagemark();
                _elements[CommandType.NextPagemark] = element;
            }

            // PrevPagemarkInBook
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPagemark;
                element.Text = Properties.Resources.CommandPrevPagemarkInBook;
                element.Note = Properties.Resources.CommandPrevPagemarkInBookNote;
                element.IsShowMessage = false;
                element.CanExecute = () => _models.BookOperation.CanPrevPagemarkInPlace((MovePagemarkCommandParameter)element.Parameter);
                element.Execute = (s, e) => _models.BookOperation.PrevPagemarkInPlace((MovePagemarkCommandParameter)element.Parameter);
                element.DefaultParameter = new MovePagemarkCommandParameter();
                _elements[CommandType.PrevPagemarkInBook] = element;
            }
            // NextPagemarkInBook
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupPagemark;
                element.Text = Properties.Resources.CommandNextPagemarkInBook;
                element.Note = Properties.Resources.CommandNextPagemarkInBookNote;
                element.IsShowMessage = false;
                element.CanExecute = () => _models.BookOperation.CanNextPagemarkInPlace((MovePagemarkCommandParameter)element.Parameter);
                element.Execute = (s, e) => _models.BookOperation.NextPagemarkInPlace((MovePagemarkCommandParameter)element.Parameter);
                element.DefaultParameter = new ShareCommandParameter() { CommandType = CommandType.PrevPagemarkInBook };
                _elements[CommandType.NextPagemarkInBook] = element;
            }


            // ToggleCustomSize
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupImageScale;
                element.Text = Properties.Resources.CommandToggleCustomSize;
                element.MenuText = Properties.Resources.CommandToggleCustomSizeMenu;
                element.Note = Properties.Resources.CommandToggleCustomSizeNote;
                element.CanExecute = () => true;
                element.IsShowMessage = true;
                element.ExecuteMessage = e => _models.PictureProfile.CustomSize.IsEnabled ? Properties.Resources.CommandToggleCustomSizeOff : Properties.Resources.CommandToggleCustomSizeOn;
                element.Execute = (s, e) => _models.PictureProfile.CustomSize.IsEnabled = !_models.PictureProfile.CustomSize.IsEnabled;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.PictureProfile.CustomSize.IsEnabled)) { Mode = BindingMode.OneWay, Source = _models.PictureProfile.CustomSize };
                _elements[CommandType.ToggleCustomSize] = element;
            }


            // ToggleResizeFilter
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupEffect;
                element.Text = Properties.Resources.CommandToggleResizeFilter;
                element.MenuText = Properties.Resources.CommandToggleResizeFilterMenu;
                element.Note = Properties.Resources.CommandToggleResizeFilterNote;
                element.CanExecute = () => true;
                element.ShortCutKey = "Ctrl+R";
                element.IsShowMessage = true;
                element.ExecuteMessage = e => _models.PictureProfile.IsResizeFilterEnabled ? Properties.Resources.CommandToggleResizeFilterOff : Properties.Resources.CommandToggleResizeFilterOn;
                element.Execute = (s, e) => _models.PictureProfile.IsResizeFilterEnabled = !_models.PictureProfile.IsResizeFilterEnabled;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.PictureProfile.IsResizeFilterEnabled)) { Mode = BindingMode.OneWay, Source = _models.PictureProfile };
                _elements[CommandType.ToggleResizeFilter] = element;
            }

            // ToggleGrid
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupEffect;
                element.Text = Properties.Resources.CommandToggleGrid;
                element.MenuText = Properties.Resources.CommandToggleGridMenu;
                element.Note = Properties.Resources.CommandToggleGridNote;
                element.CanExecute = () => true;
                element.ExecuteMessage = e => _models.ContentCanvas.GridLine.IsEnabled ? Properties.Resources.CommandToggleGridOff : Properties.Resources.CommandToggleGridOn;
                element.Execute = (s, e) => _models.ContentCanvas.GridLine.IsEnabled = !_models.ContentCanvas.GridLine.IsEnabled;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.ContentCanvas.GridLine.IsEnabled)) { Mode = BindingMode.OneWay, Source = _models.ContentCanvas.GridLine };
                _elements[CommandType.ToggleGrid] = element;
            }

            // ToggleEffect
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupEffect;
                element.Text = Properties.Resources.CommandToggleEffect;
                element.MenuText = Properties.Resources.CommandToggleEffectMenu;
                element.Note = Properties.Resources.CommandToggleEffectNote;
                element.CanExecute = () => true;
                element.ShortCutKey = "Ctrl+E";
                element.IsShowMessage = true;
                element.ExecuteMessage = e => _models.ImageEffect.IsEnabled ? Properties.Resources.CommandToggleEffectOff : Properties.Resources.CommandToggleEffectOn;
                element.Execute = (s, e) => _models.ImageEffect.IsEnabled = !_models.ImageEffect.IsEnabled;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.ImageEffect.IsEnabled)) { Mode = BindingMode.OneWay, Source = _models.ImageEffect };
                _elements[CommandType.ToggleEffect] = element;
            }


            // ToggleIsLoupe
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandToggleIsLoupe;
                element.MenuText = Properties.Resources.CommandToggleIsLoupeMenu;
                element.Note = Properties.Resources.CommandToggleIsLoupeNote;
                element.CanExecute = () => true;
                element.IsShowMessage = false;
                element.ExecuteMessage = e => _models.MouseInput.IsLoupeMode ? Properties.Resources.CommandToggleIsLoupeOff : Properties.Resources.CommandToggleIsLoupeOn;
                element.Execute = (s, e) => _models.MouseInput.IsLoupeMode = !_models.MouseInput.IsLoupeMode;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.MouseInput.IsLoupeMode)) { Mode = BindingMode.OneWay, Source = _models.MouseInput };
                _elements[CommandType.ToggleIsLoupe] = element;
            }

            // LoupeOn
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandLoupeOn;
                element.Note = Properties.Resources.CommandLoupeOnNote;
                element.CanExecute = () => true;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MouseInput.IsLoupeMode = true;
                _elements[CommandType.LoupeOn] = element;
            }

            // LoupeOff
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandLoupeOff;
                element.Note = Properties.Resources.CommandLoupeOffNote;
                element.CanExecute = () => true;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MouseInput.IsLoupeMode = false;
                _elements[CommandType.LoupeOff] = element;
            }

            // LoupeScaleUp
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandLoupeScaleUp;
                element.Note = Properties.Resources.CommandLoupeScaleUpNote;
                element.CanExecute = () => _models.MouseInput.IsLoupeMode;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MouseInput.Loupe.LoupeZoomIn();
                _elements[CommandType.LoupeScaleUp] = element;
            }

            // LoupeScaleDown
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupViewManipulation;
                element.Text = Properties.Resources.CommandLoupeScaleDown;
                element.Note = Properties.Resources.CommandLoupeScaleDownNote;
                element.CanExecute = () => _models.MouseInput.IsLoupeMode;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MouseInput.Loupe.LoupeZoomOut();
                _elements[CommandType.LoupeScaleDown] = element;
            }

            // OpenSettingWindow
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandOpenSettingWindow;
                element.MenuText = Properties.Resources.CommandOpenSettingWindowMenu;
                element.Note = Properties.Resources.CommandOpenSettingWindowNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MainWindowModel.OpenSettingWindow();
                _elements[CommandType.OpenSettingWindow] = element;
            }
            // OpenSettingFilesFolder
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandOpenSettingFilesFolder;
                element.Note = Properties.Resources.CommandOpenSettingFilesFolderNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MainWindowModel.OpenSettingFilesFolder();
                _elements[CommandType.OpenSettingFilesFolder] = element;
            }

            // OpenVersionWindow
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandOpenVersionWindow;
                element.MenuText = Properties.Resources.CommandOpenVersionWindowMenu;
                element.Note = Properties.Resources.CommandOpenVersionWindowNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MainWindowModel.OpenVersionWindow();
                _elements[CommandType.OpenVersionWindow] = element;
            }
            // CloseApplication
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandCloseApplication;
                element.MenuText = Properties.Resources.CommandCloseApplicationMenu;
                element.Note = Properties.Resources.CommandCloseApplicationNote;
                element.IsShowMessage = false;
                element.CanExecute = () => true;
                _elements[CommandType.CloseApplication] = element;
            }


            // TogglePermitFileCommand
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandTogglePermitFileCommand;
                element.MenuText = Properties.Resources.CommandTogglePermitFileCommandMenu;
                element.Note = Properties.Resources.CommandTogglePermitFileCommandNote;
                element.IsShowMessage = true;
                element.Execute = (s, e) => _models.FileIOProfile.IsEnabled = !_models.FileIOProfile.IsEnabled;
                element.ExecuteMessage = e => _models.FileIOProfile.IsEnabled ? Properties.Resources.CommandTogglePermitFileCommandOff : Properties.Resources.CommandTogglePermitFileCommandOn;
                element.CanExecute = () => true;
                element.CreateIsCheckedBinding = () => new Binding(nameof(_models.FileIOProfile.IsEnabled)) { Source = _models.FileIOProfile, Mode = BindingMode.OneWay };
                _elements[CommandType.TogglePermitFileCommand] = element;
            }


            // HelpOnline
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandHelpOnline;
                element.Note = Properties.Resources.CommandHelpOnlineNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MainWindowModel.OpenOnlineHelp();
                element.CanExecute = () => App.Current.IsNetworkEnabled;
                _elements[CommandType.HelpOnline] = element;
            }

            // HelpCommandList
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandHelpCommandList;
                element.MenuText = Properties.Resources.CommandHelpCommandListMenu;
                element.Note = Properties.Resources.CommandHelpCommandListNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => this.OpenCommandListHelp();
                element.CanExecute = () => true;
                _elements[CommandType.HelpCommandList] = element;
            }

            // HelpMainMenu
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandHelpMainMenu;
                element.MenuText = Properties.Resources.CommandHelpMainMenuMenu;
                element.Note = Properties.Resources.CommandHelpMainMenuNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => _models.MenuBar.OpenMainMenuHelp();
                element.CanExecute = () => true;
                _elements[CommandType.HelpMainMenu] = element;
            }

            // OpenContextMenu
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandOpenContextMenu;
                element.Note = Properties.Resources.CommandOpenContextMenuNote;
                element.IsShowMessage = false;
                element.CanExecute = () => true;
                _elements[CommandType.OpenContextMenu] = element;
            }


            // ExportBackup
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandExportBackup;
                element.MenuText = Properties.Resources.CommandExportBackupMenu;
                element.Note = Properties.Resources.CommandExportBackupNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => SaveData.Current.ExportBackup();
                _elements[CommandType.ExportBackup] = element;
            }

            // ImportBackup
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandImportBackup;
                element.MenuText = Properties.Resources.CommandImportBackupMenu;
                element.Note = Properties.Resources.CommandImportBackupNote;
                element.IsShowMessage = false;
                element.Execute = (s, e) => SaveData.Current.ImportBackup();
                _elements[CommandType.ImportBackup] = element;
            }

            // TouchEmulate
            {
                var element = new CommandElement();
                element.Group = Properties.Resources.CommandGroupOther;
                element.Text = Properties.Resources.CommandTouchEmulate;
                element.Note = Properties.Resources.CommandTouchEmulateNote;
                element.Execute = (s, e) => TouchInput.Current.Emulator.Execute(s, e);
                element.IsShowMessage = false;
                _elements[CommandType.TouchEmulate] = element;
            }


            // 無効な命令にダミー設定
            foreach (var ignore in CommandTypeExtensions.IgnoreCommandTypes)
            {
                var element = new CommandElement();
                element.Group = "dummy";
                element.Text = "dummy";
                element.Execute = (s, e) => { return; };
                _elements[ignore] = element;
            }

            // 並び替え
            //_Elements = _Elements.OrderBy(e => e.Key).ToDictionary(e => e.Key, e => e.Value);

            // デフォルト設定として記憶
            s_defaultMemento = CreateMemento();
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            // V2: Enum型キーは前方互換性に難があるため、文字列化して保存する

            [Obsolete, DataMember(Name = "Elements", EmitDefaultValue = false)]
            private Dictionary<CommandType, CommandElement.Memento> _elementsV1;

            [DataMember(Name = "ElementsV2")]
            private Dictionary<string, CommandElement.Memento> _elementsV2;

            [DataMember, DefaultValue(true)]
            public bool IsReversePageMove { get; set; }

            [DataMember]
            public bool IsReversePageMoveWheel { get; set; }

            public Dictionary<CommandType, CommandElement.Memento> Elements { get; set; } = new Dictionary<CommandType, CommandElement.Memento>();


            [OnSerializing]
            private void OnSerializing(StreamingContext context)
            {
                _elementsV2 = Elements.ToDictionary(e => e.Key.ToString(), e => e.Value);
            }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                Elements = new Dictionary<CommandType, CommandElement.Memento>();

#pragma warning disable CS0612
                if (_elementsV1 != null)
                {
                    Elements = _elementsV1;
                    _elementsV1 = null;
                }
#pragma warning restore CS0612

                if (_elementsV2 != null)
                {
                    foreach (var element in _elementsV2)
                    {
                        if (Enum.TryParse(element.Key, out CommandType key))
                        {
                            Elements[key] = element.Value;
                        }
                    }
                    _elementsV2 = null;
                }
            }

            public Memento Clone()
            {
                var memento = (Memento)this.MemberwiseClone();
                memento.Elements = this.Elements.ToDictionary(e => e.Key, e => e.Value.Clone());
                return memento;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            foreach (var pair in _elements)
            {
                if (pair.Key.IsDisable()) continue;
                memento.Elements.Add(pair.Key, pair.Value.CreateMemento());
            }

            memento.IsReversePageMove = this.IsReversePageMove;
            memento.IsReversePageMoveWheel = this.IsReversePageMoveWheel;

            return memento;
        }

        //
        public void Restore(Memento memento, bool onHold)
        {
            RestoreInner(memento);
            Changed?.Invoke(this, new CommandChangedEventArgs(onHold));
        }

        //
        private void RestoreInner(Memento memento)
        {
            if (memento == null) return;

            foreach (var pair in memento.Elements)
            {
                if (_elements.ContainsKey(pair.Key))
                {
                    _elements[pair.Key].Restore(pair.Value);
                }
            }

            this.IsReversePageMove = memento.IsReversePageMove;
            this.IsReversePageMoveWheel = memento.IsReversePageMoveWheel;


#pragma warning disable CS0612
            // ToggleStrechModeの復元(1.14互換用)
            if (_elements[CommandType.ToggleStretchMode].IsToggled)
            {
                var flags = ((ToggleStretchModeCommandParameter)_elements[CommandType.ToggleStretchMode].Parameter).StretchModes;

                Dictionary<PageStretchMode, CommandType> _CommandTable = new Dictionary<PageStretchMode, CommandType>
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
                    flags[item.Key] = _elements[item.Value].IsToggled;
                }
            }
#pragma warning restore CS0612

            // compatible before ver.29
            if (memento._Version < Config.GenerateProductVersionNumber(1, 29, 0))
            {
                // ver.29以前はデフォルトOFF
                this.IsReversePageMove = false;
            }
        }

        #endregion
    }
}
