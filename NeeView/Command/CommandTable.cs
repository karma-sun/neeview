using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public class CommandTable : BindableBase, IEnumerable<KeyValuePair<string, CommandElement>>
    {
        static CommandTable() => Current = new CommandTable();
        public static CommandTable Current { get; }


        #region Fields

        private Dictionary<string, CommandElement> _elements;
        private bool _isReversePageMove = true;
        private bool _isReversePageMoveWheel;

        #endregion

        #region Constructors

        // コンストラクタ
        private CommandTable()
        {
            InitializeCommandTable();
            CreateDefaultScriptFolder();

            Changed += CommandTable_Changed;
        }

        #endregion

        #region Events

        /// <summary>
        /// コマンドテーブルが変更された
        /// </summary>
        public event EventHandler<CommandChangedEventArgs> Changed;

        #endregion

        #region Properties

        public Memento DefaultMemento { get; private set; }

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

        public int ChangeCount { get; private set; }

        #endregion

        #region IEnumerable Support

        // Enumerator
        public IEnumerator<KeyValuePair<string, CommandElement>> GetEnumerator()
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

        #region Methods: Initialize

        /// <summary>
        /// コマンドテーブル初期化
        /// </summary>
        private void InitializeCommandTable()
        {
            var list = new List<CommandElement>()
            {
                new LoadAsCommand("LoadAs"),
                new ReLoadCommand("ReLoad"),
                new UnloadCommand("Unload"),
                new OpenExternalAppCommand("OpenExternalApp"),
                new OpenExplorerCommand("OpenExplorer"),
                new ExportImageAsCommand("ExportImageAs"),
                new ExportImageCommand("ExportImage"),
                new PrintCommand("Print"),
                new DeleteFileCommand("DeleteFile"),
                new DeleteBookCommand("DeleteBook"),
                new CopyFileCommand("CopyFile"),
                new CopyImageCommand("CopyImage"),
                new PasteCommand("Paste"),

                new ClearHistoryCommand("ClearHistory"),
                new ClearHistoryInPlaceCommand("ClearHistoryInPlace"),

                new ToggleStretchModeCommand("ToggleStretchMode"),
                new ToggleStretchModeReverseCommand("ToggleStretchModeReverse"),
                new SetStretchModeNoneCommand("SetStretchModeNone"),
                new SetStretchModeUniformCommand("SetStretchModeUniform"),
                new SetStretchModeUniformToFillCommand("SetStretchModeUniformToFill"),
                new SetStretchModeUniformToSizeCommand("SetStretchModeUniformToSize"),
                new SetStretchModeUniformToVerticalCommand("SetStretchModeUniformToVertical"),
                new SetStretchModeUniformToHorizontalCommand("SetStretchModeUniformToHorizontal"),
                new ToggleStretchAllowEnlargeCommand("ToggleStretchAllowEnlarge"),
                new ToggleStretchAllowReduceCommand("ToggleStretchAllowReduce"),
                new ToggleNearestNeighborCommand("ToggleNearestNeighbor"),
                new ToggleBackgroundCommand("ToggleBackground"),
                new SetBackgroundBlackCommand("SetBackgroundBlack"),
                new SetBackgroundWhiteCommand("SetBackgroundWhite"),
                new SetBackgroundAutoCommand("SetBackgroundAuto"),
                new SetBackgroundCheckCommand("SetBackgroundCheck"),
                new SetBackgroundCheckDarkCommand("SetBackgroundCheckDark"),
                new SetBackgroundCustomCommand("SetBackgroundCustom"),
                new ToggleTopmostCommand("ToggleTopmost"),
                new ToggleHideMenuCommand("ToggleHideMenu"),
                new ToggleHidePageSliderCommand("ToggleHidePageSlider"),
                new ToggleHidePanelCommand("ToggleHidePanel"),

                new ToggleVisibleTitleBarCommand("ToggleVisibleTitleBar"),
                new ToggleVisibleAddressBarCommand("ToggleVisibleAddressBar"),
                new ToggleVisibleSideBarCommand("ToggleVisibleSideBar"),
                new ToggleVisibleFileInfoCommand("ToggleVisibleFileInfo"),
                new ToggleVisibleEffectInfoCommand("ToggleVisibleEffectInfo"),
                new ToggleVisibleBookshelfCommand("ToggleVisibleBookshelf"),
                new ToggleVisibleBookmarkListCommand("ToggleVisibleBookmarkList"),
                new ToggleVisiblePagemarkListCommand("ToggleVisiblePagemarkList"),
                new ToggleVisibleHistoryListCommand("ToggleVisibleHistoryList"),
                new ToggleVisiblePageListCommand("ToggleVisiblePageList"),
                new ToggleVisibleFoldersTreeCommand("ToggleVisibleFoldersTree"),
                new FocusFolderSearchBoxCommand("FocusFolderSearchBox"),
                new FocusBookmarkListCommand("FocusBookmarkList"),
                new FocusMainViewCommand("FocusMainView"),
                new TogglePageListPlacementCommand("TogglePageListPlacement"),
                new ToggleVisibleThumbnailListCommand("ToggleVisibleThumbnailList"),
                new ToggleHideThumbnailListCommand("ToggleHideThumbnailList"),

                new ToggleFullScreenCommand("ToggleFullScreen"),
                new SetFullScreenCommand("SetFullScreen"),
                new CancelFullScreenCommand("CancelFullScreen"),
                new ToggleWindowMinimizeCommand("ToggleWindowMinimize"),
                new ToggleWindowMaximizeCommand("ToggleWindowMaximize"),
                new ShowHiddenPanelsCommand("ShowHiddenPanels"),

                new ToggleSlideShowCommand("ToggleSlideShow"),
                new ViewScrollUpCommand("ViewScrollUp"),
                new ViewScrollDownCommand("ViewScrollDown"),
                new ViewScrollLeftCommand("ViewScrollLeft"),
                new ViewScrollRightCommand("ViewScrollRight"),
                new ViewScaleUpCommand("ViewScaleUp"),
                new ViewScaleDownCommand("ViewScaleDown"),
                new ViewRotateLeftCommand("ViewRotateLeft"),
                new ViewRotateRightCommand("ViewRotateRight"),
                new ToggleIsAutoRotateLeftCommand("ToggleIsAutoRotateLeft"),
                new ToggleIsAutoRotateRightCommand("ToggleIsAutoRotateRight"),

                new ToggleViewFlipHorizontalCommand("ToggleViewFlipHorizontal"),
                new ViewFlipHorizontalOnCommand("ViewFlipHorizontalOn"),
                new ViewFlipHorizontalOffCommand("ViewFlipHorizontalOff"),

                new ToggleViewFlipVerticalCommand("ToggleViewFlipVertical"),
                new ViewFlipVerticalOnCommand("ViewFlipVerticalOn"),
                new ViewFlipVerticalOffCommand("ViewFlipVerticalOff"),
                new ViewResetCommand("ViewReset"),
                new PrevPageCommand("PrevPage"),
                new NextPageCommand("NextPage"),
                new PrevOnePageCommand("PrevOnePage"),
                new NextOnePageCommand("NextOnePage"),

                new PrevScrollPageCommand("PrevScrollPage"),
                new NextScrollPageCommand("NextScrollPage"),
                new JumpPageCommand("JumpPage"),
                new PrevSizePageCommand("PrevSizePage"),
                new NextSizePageCommand("NextSizePage"),

                new PrevFolderPageCommand("PrevFolderPage"),
                new NextFolderPageCommand("NextFolderPage"),
                new FirstPageCommand("FirstPage"),
                new LastPageCommand("LastPage"),
                new PrevBookCommand("PrevBook"),
                new NextBookCommand("NextBook"),
                new PrevHistoryCommand("PrevHistory"),
                new NextHistoryCommand("NextHistory"),

                new PrevBookHistoryCommand("PrevBookHistory"),
                new NextBookHistoryCommand("NextBookHistory"),
                new MoveToParentBookCommand("MoveToParentBook"),
                new MoveToChildBookCommand("MoveToChildBook"),

                new ToggleMediaPlayCommand("ToggleMediaPlay"),
                new ToggleBookOrderCommand("ToggleBookOrder"),
                new SetBookOrderByFileNameACommand("SetBookOrderByFileNameA"),
                new SetBookOrderByFileNameDCommand("SetBookOrderByFileNameD"),
                new SetBookOrderByPathACommand("SetBookOrderByPathA"),
                new SetBookOrderByPathDCommand("SetBookOrderByPathD"),
                new SetBookOrderByFileTypeACommand("SetBookOrderByFileTypeA"),
                new SetBookOrderByFileTypeDCommand("SetBookOrderByFileTypeD"),
                new SetBookOrderByTimeStampACommand("SetBookOrderByTimeStampA"),
                new SetBookOrderByTimeStampDCommand("SetBookOrderByTimeStampD"),
                new SetBookOrderByEntryTimeACommand("SetBookOrderByEntryTimeA"),
                new SetBookOrderByEntryTimeDCommand("SetBookOrderByEntryTimeD"),
                new SetBookOrderBySizeACommand("SetBookOrderBySizeA"),
                new SetBookOrderBySizeDCommand("SetBookOrderBySizeD"),
                new SetBookOrderByRandomCommand("SetBookOrderByRandom"),
                new TogglePageModeCommand("TogglePageMode"),
                new SetPageModeOneCommand("SetPageModeOne"),
                new SetPageModeTwoCommand("SetPageModeTwo"),
                new ToggleBookReadOrderCommand("ToggleBookReadOrder"),
                new SetBookReadOrderRightCommand("SetBookReadOrderRight"),
                new SetBookReadOrderLeftCommand("SetBookReadOrderLeft"),
                new ToggleIsSupportedDividePageCommand("ToggleIsSupportedDividePage"),
                new ToggleIsSupportedWidePageCommand("ToggleIsSupportedWidePage"),
                new ToggleIsSupportedSingleFirstPageCommand("ToggleIsSupportedSingleFirstPage"),
                new ToggleIsSupportedSingleLastPageCommand("ToggleIsSupportedSingleLastPage"),
                new ToggleIsRecursiveFolderCommand("ToggleIsRecursiveFolder"),
                new ToggleSortModeCommand("ToggleSortMode"),
                new SetSortModeFileNameCommand("SetSortModeFileName"),
                new SetSortModeFileNameDescendingCommand("SetSortModeFileNameDescending"),
                new SetSortModeTimeStampCommand("SetSortModeTimeStamp"),
                new SetSortModeTimeStampDescendingCommand("SetSortModeTimeStampDescending"),
                new SetSortModeSizeCommand("SetSortModeSize"),
                new SetSortModeSizeDescendingCommand("SetSortModeSizeDescending"),
                new SetSortModeRandomCommand("SetSortModeRandom"),
                new SetDefaultPageSettingCommand("SetDefaultPageSetting"),

                new ToggleBookmarkCommand("ToggleBookmark"),
                new TogglePagemarkCommand("TogglePagemark"),
                new PrevPagemarkCommand("PrevPagemark"),
                new NextPagemarkCommand("NextPagemark"),
                new PrevPagemarkInBookCommand("PrevPagemarkInBook"),
                new NextPagemarkInBookCommand("NextPagemarkInBook"),

                new ToggleCustomSizeCommand("ToggleCustomSize"),

                new ToggleResizeFilterCommand("ToggleResizeFilter"),
                new ToggleGridCommand("ToggleGrid"),
                new ToggleEffectCommand("ToggleEffect"),

                new ToggleIsLoupeCommand("ToggleIsLoupe"),
                new LoupeOnCommand("LoupeOn"),
                new LoupeOffCommand("LoupeOff"),
                new LoupeScaleUpCommand("LoupeScaleUp"),
                new LoupeScaleDownCommand("LoupeScaleDown"),
                new OpenSettingWindowCommand("OpenSettingWindow"),
                new OpenSettingFilesFolderCommand("OpenSettingFilesFolder"),
                new OpenScriptsFolderCommand("OpenScriptsFolder"),
                new OpenVersionWindowCommand("OpenVersionWindow"),
                new CloseApplicationCommand("CloseApplication"),

                new TogglePermitFileCommandCommand("TogglePermitFileCommand"),

                new HelpCommandListCommand("HelpCommandList"),
                new HelpScriptCommand("HelpScript"),
                new HelpMainMenuCommand("HelpMainMenu"),
                new HelpSearchOptionCommand("HelpSearchOption"),
                new OpenContextMenuCommand("OpenContextMenu"),

                new ExportBackupCommand("ExportBackup"),
                new ImportBackupCommand("ImportBackup"),
                new ReloadUserSettingCommand("ReloadUserSetting"),
                new TouchEmulateCommand("TouchEmulate"),

                new OpenConsoleCommand("OpenConsole"),
            };

            // グループで並び替えてから辞書化
            _elements = list
                .GroupBy(e => e.Group).SelectMany(e => e)
                .ToDictionary(e => e.Name);

            // share
            _elements["NextPage"].SetShare(_elements["PrevPage"]);
            _elements["NextOnePage"].SetShare(_elements["PrevOnePage"]);
            _elements["NextScrollPage"].SetShare(_elements["PrevScrollPage"]);
            _elements["NextSizePage"].SetShare(_elements["PrevSizePage"]);
            _elements["NextFolderPage"].SetShare(_elements["PrevFolderPage"]);
            _elements["LastPage"].SetShare(_elements["FirstPage"]);
            _elements["ToggleStretchModeReverse"].SetShare(_elements["ToggleStretchMode"]);
            _elements["SetStretchModeUniformToFill"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToSize"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToVertical"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToHorizontal"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["NextPagemarkInBook"].SetShare(_elements["PrevPagemarkInBook"]);
            _elements["ViewScrollDown"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScrollLeft"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScrollRight"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScaleDown"].SetShare(_elements["ViewScaleUp"]);
            _elements["ViewRotateRight"].SetShare(_elements["ViewRotateLeft"]);

            // TODO: pair...

            // デフォルト設定として記憶
            DefaultMemento = CreateMemento();
        }

        #endregion

        #region Methods

        // NODE: 応急処置
        public IEnumerable<string> Keys => _elements.Keys;

        // NODE: 応急処置
        public bool ContainsKey(string key)
        {
            return key != null && _elements.ContainsKey(key);
        }

        public bool TryGetValue(string key, out CommandElement command)
        {
            return _elements.TryGetValue(key, out command);
        }

        public CommandElement GetElement(string key)
        {
            if (TryGetValue(key, out CommandElement command))
            {
                return command;
            }
            else
            {
                throw new ArgumentOutOfRangeException(key);
            }
        }

        private void CommandTable_Changed(object sender, CommandChangedEventArgs e)
        {
            ChangeCount++;
            ClearInputGestureDarty();
        }


        /// <summary>
        /// 初期設定生成
        /// </summary>
        /// <param name="type">入力スキーム</param>
        /// <returns></returns>
        public static Memento CreateDefaultMemento(InputSceme type)
        {
            var memento = CommandTable.Current.DefaultMemento.Clone();

            // Type.M
            switch (type)
            {
                case InputSceme.TypeA: // default
                    break;

                case InputSceme.TypeB: // wheel page, right click contextmenu
                    memento.Elements["NextScrollPage"].ShortCutKey = null;
                    memento.Elements["PrevScrollPage"].ShortCutKey = null;
                    memento.Elements["NextPage"].ShortCutKey = "Left,WheelDown";
                    memento.Elements["PrevPage"].ShortCutKey = "Right,WheelUp";
                    memento.Elements["OpenContextMenu"].ShortCutKey = "RightClick";
                    break;

                case InputSceme.TypeC: // click page
                    memento.Elements["NextScrollPage"].ShortCutKey = null;
                    memento.Elements["PrevScrollPage"].ShortCutKey = null;
                    memento.Elements["NextPage"].ShortCutKey = "Left,LeftClick";
                    memento.Elements["PrevPage"].ShortCutKey = "Right,RightClick";
                    memento.Elements["ViewScrollUp"].ShortCutKey = "WheelUp";
                    memento.Elements["ViewScrollDown"].ShortCutKey = "WheelDown";
                    break;
            }

            return memento;
        }

        // .. あまりかわらん
        public T Parameter<T>(string commandName) where T : class
        {
            return _elements[commandName].Parameter as T;
        }


        public bool TryExecute(string commandName, object[] args, CommandOption option)
        {
            if (TryGetValue(commandName, out CommandElement command))
            {
                args = args ?? CommandElement.EmptyArgs;
                if (command.CanExecute(args, option))
                {
                    command.Execute(args, option);
                }
            }

            return false;
        }

        /// <summary>
        /// 入力ジェスチャーが変更されていたらテーブル更新イベントを発行する
        /// </summary>
        public void FlushInputGesture()
        {
            if (_elements.Values.Any(e => e.IsInputGestureDarty))
            {
                Changed?.Invoke(this, new CommandChangedEventArgs(false));
            }
        }

        /// <summary>
        /// 入力ジェスチャー変更フラグをクリア
        /// </summary>
        public void ClearInputGestureDarty()
        {
            foreach (var command in _elements.Values)
            {
                command.IsInputGestureDarty = false;
            }
        }


        // ショートカット重複チェック
        public List<string> GetOverlapShortCut(string shortcut)
        {
            var overlaps = _elements
                .Where(e => !string.IsNullOrEmpty(e.Value.ShortCutKey) && e.Value.ShortCutKey.Split(',').Contains(shortcut))
                .Select(e => e.Key)
                .ToList();

            return overlaps;
        }

        // マウスジェスチャー重複チェック
        public List<string> GetOverlapMouseGesture(string gesture)
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
                if (command.Group == "(none)") continue;

                if (!groups.ContainsKey(command.Group))
                {
                    groups.Add(command.Group, new List<CommandElement>());
                }

                groups[command.Group].Add(command);
            }

            // 
            Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "CommandList.html");

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



        // スクリプト用リファレンス
        public void OpenScriptHelp()
        {
            // グループ分け
            var groups = new Dictionary<string, List<CommandElement>>();
            foreach (var command in _elements.Values)
            {
                if (command.Group == "(none)") continue;

                if (!groups.ContainsKey(command.Group))
                {
                    groups.Add(command.Group, new List<CommandElement>());
                }

                groups[command.Group].Add(command);
            }

            Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "CommandList.html");

            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.WriteLine(HtmlHelpUtility.CraeteHeader("NeeView Script Manual"));
                writer.WriteLine($"<body>");

                WriteResource(writer, "/Resources/ja-JP/ScriptManual.html");

                var executeMethodArgTypes = new Type[] { typeof(CommandParameter), typeof(object[]), typeof(CommandOption) };

                // グループごとに出力
                foreach (var pair in groups)
                {
                    writer.WriteLine($"<h3>{pair.Key}</h3>");
                    writer.WriteLine("<table>");

                    writer.WriteLine($"<th>{Properties.Resources.WordCommand}<th>{Properties.Resources.WordCommandName}<th>{Properties.Resources.WordArgument}<th>{Properties.Resources.WordCommandParameter}<th>{Properties.Resources.WordDescription}<tr>");
                    foreach (var command in pair.Value)
                    {
                        string argument = "";
                        {
                            var type = command.GetType();
                            var info = type.GetMethod(nameof(command.Execute), executeMethodArgTypes);
                            var attribute = (MethodArgumentAttribute)Attribute.GetCustomAttributes(info, typeof(MethodArgumentAttribute)).FirstOrDefault();
                            if (attribute != null)
                            {
                                var tokens = ResourceService.GetString(attribute.Note).Split('|');
                                int index = 0;
                                argument += "<dl>";
                                while (index < tokens.Length )
                                {
                                    var dt = tokens.ElementAtOrDefault(index++);
                                    var dd = tokens.ElementAtOrDefault(index++);
                                    argument += $"<dt>{dt}</dt><dd>{dd}</dd>";
                                }
                                argument += "</dl>";
                            }
                        }

                        string properties = "";
                        if (command.Parameter != null)
                        {
                            var type = command.Parameter.GetType();
                            var title = "";
                            var enums = "";

                            if (command.Share != null)
                            {
                                properties = "<p style=\"color:red\">" + string.Format(Properties.Resources.ParamCommandShare, command.Share.Name) + "</p>";
                            }

                            foreach (PropertyInfo info in type.GetProperties())
                            {
                                var attribute = (PropertyMemberAttribute)Attribute.GetCustomAttributes(info, typeof(PropertyMemberAttribute)).FirstOrDefault();
                                if (attribute != null)
                                {
                                    if (attribute.Title != null)
                                    {
                                        title = ResourceService.GetString(attribute.Title) + " / ";
                                    }

                                    if (info.PropertyType.IsEnum)
                                    {
                                        enums = string.Join(" / ", info.PropertyType.VisibledAliasNameDictionary().Select(e => $"{Convert.ToInt32(e.Key)}: {e.Value}")) + "<br/>";
                                    }

                                    var text = title + ResourceService.GetString(attribute.Name).TrimEnd(Properties.Resources.WordPeriod.ToArray()) + Properties.Resources.WordPeriod + (attribute.Tips != null ? " " + ResourceService.GetString(attribute.Tips) : "");

                                    properties = properties + $"<dt><b>{info.Name}</b>: {TypeToString(info.PropertyType)}</dt><dd>{enums + text}<dd/>";
                                }
                            }
                            if (!string.IsNullOrEmpty(properties))
                            {
                                properties = "<dl>" + properties + "</dl>";
                            }
                        }

                        writer.WriteLine($"<td>{command.Text}<td><b>{command.Name}</b><td>{argument}<td>{properties}<td>{command.Note}<tr>");
                    }
                    writer.WriteLine("</table>");
                }

                WriteResource(writer, "/Resources/ja-JP/ScriptManualExample.html");

                writer.WriteLine("</body>");

                writer.WriteLine(HtmlHelpUtility.CreateFooter());
            }

            System.Diagnostics.Process.Start(fileName);

            void WriteResource(StreamWriter writer, string resourcPath)
            {
                Uri fileUri = new Uri(resourcPath, UriKind.Relative);
                StreamResourceInfo info = System.Windows.Application.GetResourceStream(fileUri);
                using (StreamReader sr = new StreamReader(info.Stream))
                {
                    writer.WriteLine(sr.ReadToEnd());
                }
            }
        }



        private string TypeToString(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.Double:
                    return "double";
                case TypeCode.String:
                    return "string";
            }

            return "???";
        }


        #endregion


        #region Scripts

        private readonly string _defaultScriptFolderName = "Scripts";
        private bool _isScriptFolderEnabled;
        private string _scriptFolder;
        private bool _isScriptFolderDarty = true;

        public string GetDefaultScriptFolder()
        {
            if (Config.Current.IsZipLikePackage)
            {
                return Path.Combine(Config.Current.LocalApplicationDataPath, _defaultScriptFolderName);
            }
            else
            {
                return Path.Combine(Config.Current.GetMyDocumentPath(false), _defaultScriptFolderName);
            }
        }

        public void CreateDefaultScriptFolder()
        {
            var path = GetDefaultScriptFolder();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);

                try
                {
                    // サンプルスクリプトを生成
                    var filename = "Sample.nvjs";
                    var source = Path.Combine(Config.Current.AssemblyLocation, _defaultScriptFolderName, filename);
                    File.Copy(source, Path.Combine(path, filename));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        [PropertyMember("@ParamIsScriptFolderEnabled")]
        public bool IsScriptFolderEnabled
        {
            get { return _isScriptFolderEnabled; }
            set
            {
                if (SetProperty(ref _isScriptFolderEnabled, value))
                {

                    UpdateScriptCommand(true);
                    Changed?.Invoke(this, new CommandChangedEventArgs(false));
                }
            }
        }

        [PropertyPath("@ParamScriptFolder", Tips = "@ParamScriptFolderTips", FileDialogType = Windows.Controls.FileDialogType.Directory)]
        public string ScriptFolder
        {
            get { return _scriptFolder ?? GetDefaultScriptFolder(); }
            set
            {
                var path = value?.Trim();
                if (string.IsNullOrEmpty(path) || path == GetDefaultScriptFolder())
                {
                    path = null;
                }

                if (SetProperty(ref _scriptFolder, path))
                {
                    UpdateScriptCommand(true);
                    Changed?.Invoke(this, new CommandChangedEventArgs(false));
                }
            }
        }

        public bool UpdateScriptCommand(bool isForce = false)
        {
            if (!_isScriptFolderDarty && !isForce) return false;
            _isScriptFolderDarty = false;

            if (!_isScriptFolderEnabled)
            {
                ClearScriptCommand();
                return true;
            }

            var oldies = _elements.Keys
                .Where(e => e.StartsWith(ScriptCommand.Prefix))
                .ToList();

            var newers = new List<string>();

            try
            {
                newers = Directory.GetFiles(ScriptFolder, "*" + ScriptCommand.Extension)
                    .Select(e => ScriptCommand.Prefix + Path.GetFileNameWithoutExtension(e))
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            foreach (var name in oldies.Except(newers))
            {
                _elements.Remove(name);
            }

            foreach (var name in newers.Except(oldies))
            {
                _elements.Add(name, new ScriptCommand(name));
            }

            return true;
        }

        public void ClearScriptCommand()
        {
            var oldies = _elements.Keys
                .Where(e => e.StartsWith(ScriptCommand.Prefix))
                .ToList();

            foreach (var name in oldies)
            {
                _elements.Remove(name);
            }

            _isScriptFolderDarty = true;
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            public static Dictionary<string, string> RenameMap_37_0_0 = new Dictionary<string, string>()
            {
                ["OpenApplicaion"] = "OpenExternalApp",
                ["OpenFilePlace"] = "OpenExplorer",
                ["Export"] = "ExportImageAs",
                ["PrevFolder"] = "PrevBook",
                ["NextFolder"] = "NextBook",
                ["SetPageMode1"] = "SetPageModeOne",
                ["SetPageMode2"] = "SetPageModeTwo",
                ["ToggleIsEnabledNearestNeighbor"] = "ToggleNearestNeighbor",
                ["ToggleFolderOrder"] = "ToggleBookOrder",
                ["SetFolderOrderByFileNameA"] = "SetBookOrderByFileNameA",
                ["SetFolderOrderByFileNameD"] = "SetBookOrderByFileNameD",
                ["SetFolderOrderByPathA"] = "SetBookOrderByPathA",
                ["SetFolderOrderByPathD"] = "SetBookOrderByPathD",
                ["SetFolderOrderByFileTypeA"] = "SetBookOrderByFileTypeA",
                ["SetFolderOrderByFileTypeD"] = "SetBookOrderByFileTypeD",
                ["SetFolderOrderByTimeStampA"] = "SetBookOrderByTimeStampA",
                ["SetFolderOrderByTimeStampD"] = "SetBookOrderByTimeStampD",
                ["SetFolderOrderByEntryTimeA"] = "SetBookOrderByEntryTimeA",
                ["SetFolderOrderByEntryTimeD"] = "SetBookOrderByEntryTimeD",
                ["SetFolderOrderBySizeA"] = "SetBookOrderBySizeA",
                ["SetFolderOrderBySizeD"] = "SetBookOrderBySizeD",
                ["SetFolderOrderByRandom"] = "SetBookOrderByRandom",
            };

            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember, DefaultValue(true)]
            public bool IsReversePageMove { get; set; }

            [DataMember]
            public bool IsReversePageMoveWheel { get; set; }

            [DataMember(Name = "ElementsV2")]
            public Dictionary<string, CommandElement.Memento> Elements { get; set; } = new Dictionary<string, CommandElement.Memento>();

            [DataMember]
            public bool IsScriptFolderEnabled { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public string ScriptFolder { get; set; }

            [Obsolete, DataMember(Name = "Elements", EmitDefaultValue = false)]
            private Dictionary<CommandType, CommandElement.Memento> _elementsV1;


            [OnSerializing]
            private void OnSerializing(StreamingContext context)
            {
            }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
#pragma warning disable CS0612
                if (_elementsV1 != null)
                {
                    Elements = _elementsV1.ToDictionary(e => e.Key.ToString(), e => e.Value);
                    _elementsV1 = null;
                }
#pragma warning restore CS0612

                Elements = Elements ?? new Dictionary<string, CommandElement.Memento>();

                // before ver.29
                if (_Version < Config.GenerateProductVersionNumber(1, 29, 0))
                {
                    // ver.29以前はデフォルトOFF
                    IsReversePageMove = false;
                }

                // before 32.0
                if (_Version < Config.GenerateProductVersionNumber(32, 0, 0))
                {
                    // 新しいコマンドに設定を引き継ぐ
                    if (Elements.TryGetValue("ToggleVisibleFolderSearchBox", out CommandElement.Memento toggleVisibleFolderSearchBox))
                    {
                        Elements["FocusFolderSearchBox"] = toggleVisibleFolderSearchBox;
                    }

                    if (Elements.TryGetValue("ToggleVisibleBookmarkList", out CommandElement.Memento toggleVisibleBookmarkList))
                    {
                        Elements["FocusBookmarkList"] = toggleVisibleBookmarkList;
                    }

                    if (Elements.TryGetValue("ToggleVisibleFolderList", out CommandElement.Memento toggleVisibleFolderList))
                    {
                        Elements["ToggleVisibleBookshelf"] = toggleVisibleFolderList;
                    }
                }

                // before 33.2
                if (_Version <= Config.GenerateProductVersionNumber(33, 2, 0))
                {
                    // change shortcut "Escape" to "Esc"
                    foreach (var element in Elements.Values)
                    {
                        if (element.ShortCutKey != null && element.ShortCutKey.Contains("Escape"))
                        {
                            var keys = element.ShortCutKey
                                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(e => e.Replace("Escape", "Esc"))
                                .Distinct();

                            element.ShortCutKey = string.Join(",", keys);
                        }
                    }
                }

                // before 34.0
                if (_Version < Config.GenerateProductVersionNumber(34, 0, 0))
                {
                    // 自動回転のショートカットキーをなるべく継承
                    if (Elements.TryGetValue("ToggleIsAutoRotate", out var element))
                    {
                        var commandName = element.Parameter is null ? "ToggleIsAutoRotateRight" : "ToggleIsAutoRotateLeft";
                        Elements[commandName] = element.Clone();
                        Elements[commandName].IsShowMessage = true;
                        Elements[commandName].Parameter = null;
                    }
                }

                // before 35.0
                if (_Version < Config.GenerateProductVersionNumber(35, 0, 0))
                {
                    // ストレッチコマンドパラメータ継承
                    if (Elements.TryGetValue("SetStretchModeInside", out var element))
                    {
                        Elements["SetStretchModeUniform"].Parameter = element.Parameter;
                    }
                }

                // before 37.0
                if (_Version < Config.GenerateProductVersionNumber(37, 0, 0))
                {
                    foreach(var pair in RenameMap_37_0_0)
                    {
                        Rename(pair.Key, pair.Value);
                    }
                }

                // コマンド名変更
                void Rename(string oldName, string newName)
                {
                    if (Elements.TryGetValue(oldName, out var element))
                    {
                        Elements[newName] = element;
                        // NOTE: 未登録コマンドは適用されないので古いコマンドの削除の必要はない
                    }
                }
            }

            public Memento Clone()
            {
                var memento = (Memento)this.MemberwiseClone();
                memento.Elements = this.Elements.ToDictionary(e => e.Key, e => e.Value.Clone());
                return memento;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            foreach (var pair in _elements)
            {
                memento.Elements.Add(pair.Key, pair.Value.CreateMemento());
            }

            memento.IsReversePageMove = this.IsReversePageMove;
            memento.IsReversePageMoveWheel = this.IsReversePageMoveWheel;
            memento.IsScriptFolderEnabled = _isScriptFolderEnabled;
            memento.ScriptFolder = _scriptFolder;

            return memento;
        }

        public void Restore(Memento memento, bool onHold)
        {
            RestoreInner(memento);
            Changed?.Invoke(this, new CommandChangedEventArgs(onHold));
        }

        private void RestoreInner(Memento memento)
        {
            if (memento == null) return;

            this.IsReversePageMove = memento.IsReversePageMove;
            this.IsReversePageMoveWheel = memento.IsReversePageMoveWheel;
            _isScriptFolderEnabled = memento.IsScriptFolderEnabled;
            _scriptFolder = memento.ScriptFolder;

            UpdateScriptCommand();

            foreach (var pair in memento.Elements)
            {
                if (_elements.ContainsKey(pair.Key))
                {
                    _elements[pair.Key].Restore(pair.Value);
                }
                else
                {
                    Debug.WriteLine($"Warning: No such command '{pair.Key}'");
                }
            }
        }

        #endregion
    }
}
