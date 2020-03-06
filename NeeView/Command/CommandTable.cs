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
        static CommandTable() => Current = new CommandTable();
        public static CommandTable Current { get; }

        #region Fields

        private static Memento s_defaultMemento;

        private Dictionary<CommandType, CommandElement> _elements;
        private bool _isReversePageMove = true;
        private bool _isReversePageMoveWheel;

        #endregion

        #region Constructors

        // コンストラクタ
        private CommandTable()
        {
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

        #endregion

        #region Methods: Initialize

        /// <summary>
        /// コマンドテーブル初期化
        /// </summary>
        private void InitializeCommandTable()
        {
            // コマンドの設定定義
            _elements = new Dictionary<CommandType, CommandElement>();

            _elements[CommandType.None] = new NoneCommand();
            _elements[CommandType.OpenSettingWindow] = new OpenSettingWindowCommand();
            _elements[CommandType.OpenSettingFilesFolder] = new OpenSettingFilesFolderCommand();
            _elements[CommandType.OpenVersionWindow] = new OpenVersionWindowCommand();
            _elements[CommandType.CloseApplication] = new CloseApplicationCommand();
            _elements[CommandType.LoadAs] = new LoadAsCommand();
            _elements[CommandType.ReLoad] = new ReLoadCommand();
            _elements[CommandType.Unload] = new UnloadCommand();
            _elements[CommandType.OpenApplication] = new OpenApplicationCommand();
            _elements[CommandType.OpenFilePlace] = new OpenFilePlaceCommand();
            _elements[CommandType.Export] = new ExportCommand();
            _elements[CommandType.ExportImage] = new ExportImageCommand();
            _elements[CommandType.Print] = new PrintCommand();
            _elements[CommandType.DeleteFile] = new DeleteFileCommand();
            _elements[CommandType.DeleteBook] = new DeleteBookCommand();
            _elements[CommandType.CopyFile] = new CopyFileCommand();
            _elements[CommandType.CopyImage] = new CopyImageCommand();
            _elements[CommandType.Paste] = new PasteCommand();
            _elements[CommandType.OpenContextMenu] = new OpenContextMenuCommand();
            _elements[CommandType.ClearHistory] = new ClearHistoryCommand();
            _elements[CommandType.ClearHistoryInPlace] = new ClearHistoryInPlaceCommand();
            _elements[CommandType.PrevPage] = new PrevPageCommand();
            _elements[CommandType.NextPage] = new NextPageCommand().SetShare(_elements[CommandType.PrevPage]);
            _elements[CommandType.PrevOnePage] = new PrevOnePageCommand();
            _elements[CommandType.NextOnePage] = new NextOnePageCommand().SetShare(_elements[CommandType.PrevOnePage]);
            _elements[CommandType.PrevScrollPage] = new PrevScrollPageCommand();
            _elements[CommandType.NextScrollPage] = new NextScrollPageCommand().SetShare(_elements[CommandType.PrevScrollPage]);
            _elements[CommandType.JumpPage] = new JumpPageCommand();
            _elements[CommandType.PrevSizePage] = new PrevSizePageCommand();
            _elements[CommandType.NextSizePage] = new NextSizePageCommand().SetShare(_elements[CommandType.PrevSizePage]);
            _elements[CommandType.PrevFolderPage] = new PrevFolderPageCommand();
            _elements[CommandType.NextFolderPage] = new NextFolderPageCommand().SetShare(_elements[CommandType.PrevFolderPage]);
            _elements[CommandType.FirstPage] = new FirstPageCommand();
            _elements[CommandType.LastPage] = new LastPageCommand().SetShare(_elements[CommandType.FirstPage]);
            _elements[CommandType.ToggleMediaPlay] = new ToggleMediaPlayCommand();
            _elements[CommandType.PrevFolder] = new PrevFolderCommand();
            _elements[CommandType.NextFolder] = new NextFolderCommand();
            _elements[CommandType.PrevHistory] = new PrevHistoryCommand();
            _elements[CommandType.NextHistory] = new NextHistoryCommand();
            _elements[CommandType.PrevBookHistory] = new PrevBookHistoryCommand();
            _elements[CommandType.NextBookHistory] = new NextBookHistoryCommand();
            _elements[CommandType.MoveToParentBook] = new MoveToParentBookCommand();
            _elements[CommandType.MoveToChildBook] = new MoveToChildBookCommand();
            _elements[CommandType.ToggleFolderOrder] = new ToggleFolderOrderCommand();
            _elements[CommandType.SetFolderOrderByFileNameA] = new SetFolderOrderByFileNameACommand();
            _elements[CommandType.SetFolderOrderByFileNameD] = new SetFolderOrderByFileNameDCommand();
            _elements[CommandType.SetFolderOrderByPathA] = new SetFolderOrderByPathACommand();
            _elements[CommandType.SetFolderOrderByPathD] = new SetFolderOrderByPathDCommand();
            _elements[CommandType.SetFolderOrderByFileTypeA] = new SetFolderOrderByFileTypeACommand();
            _elements[CommandType.SetFolderOrderByFileTypeD] = new SetFolderOrderByFileTypeDCommand();
            _elements[CommandType.SetFolderOrderByTimeStampA] = new SetFolderOrderByTimeStampACommand();
            _elements[CommandType.SetFolderOrderByTimeStampD] = new SetFolderOrderByTimeStampDCommand();
            _elements[CommandType.SetFolderOrderByEntryTimeA] = new SetFolderOrderByEntryTimeACommand();
            _elements[CommandType.SetFolderOrderByEntryTimeD] = new SetFolderOrderByEntryTimeDCommand();
            _elements[CommandType.SetFolderOrderBySizeA] = new SetFolderOrderBySizeACommand();
            _elements[CommandType.SetFolderOrderBySizeD] = new SetFolderOrderBySizeDCommand();
            _elements[CommandType.SetFolderOrderByRandom] = new SetFolderOrderByRandomCommand();
            _elements[CommandType.ToggleTopmost] = new ToggleTopmostCommand();
            _elements[CommandType.ToggleHideMenu] = new ToggleHideMenuCommand();
            _elements[CommandType.ToggleHidePageSlider] = new ToggleHidePageSliderCommand();
            _elements[CommandType.ToggleHidePanel] = new ToggleHidePanelCommand();
            _elements[CommandType.ToggleVisibleTitleBar] = new ToggleVisibleTitleBarCommand();
            _elements[CommandType.ToggleVisibleAddressBar] = new ToggleVisibleAddressBarCommand();
            _elements[CommandType.ToggleVisibleSideBar] = new ToggleVisibleSideBarCommand();
            _elements[CommandType.ToggleVisibleFileInfo] = new ToggleVisibleFileInfoCommand();
            _elements[CommandType.ToggleVisibleEffectInfo] = new ToggleVisibleEffectInfoCommand();
            _elements[CommandType.ToggleVisibleBookshelf] = new ToggleVisibleBookshelfCommand();
            _elements[CommandType.ToggleVisibleBookmarkList] = new ToggleVisibleBookmarkListCommand();
            _elements[CommandType.ToggleVisiblePagemarkList] = new ToggleVisiblePagemarkListCommand();
            _elements[CommandType.ToggleVisibleHistoryList] = new ToggleVisibleHistoryListCommand();
            _elements[CommandType.ToggleVisiblePageList] = new ToggleVisiblePageListCommand();
            _elements[CommandType.ToggleVisibleFoldersTree] = new ToggleVisibleFoldersTreeCommand();
            _elements[CommandType.FocusFolderSearchBox] = new FocusFolderSearchBoxCommand();
            _elements[CommandType.FocusBookmarkList] = new FocusBookmarkListCommand();
            _elements[CommandType.FocusMainView] = new FocusMainViewCommand();
            _elements[CommandType.TogglePageListPlacement] = new TogglePageListPlacementCommand();
            _elements[CommandType.ToggleVisibleThumbnailList] = new ToggleVisibleThumbnailListCommand();
            _elements[CommandType.ToggleHideThumbnailList] = new ToggleHideThumbnailListCommand();
            _elements[CommandType.ToggleFullScreen] = new ToggleFullScreenCommand();
            _elements[CommandType.SetFullScreen] = new SetFullScreenCommand();
            _elements[CommandType.CancelFullScreen] = new CancelFullScreenCommand();
            _elements[CommandType.ToggleWindowMinimize] = new ToggleWindowMinimizeCommand();
            _elements[CommandType.ToggleWindowMaximize] = new ToggleWindowMaximizeCommand();
            _elements[CommandType.ShowHiddenPanels] = new ShowHiddenPanelsCommand();
            _elements[CommandType.ToggleSlideShow] = new ToggleSlideShowCommand();
            _elements[CommandType.ToggleStretchMode] = new ToggleStretchModeCommand();
            _elements[CommandType.ToggleStretchModeReverse] = new ToggleStretchModeReverseCommand().SetShare(_elements[CommandType.ToggleStretchMode]);
            _elements[CommandType.SetStretchModeNone] = new SetStretchModeNoneCommand();
            _elements[CommandType.SetStretchModeUniform] = new SetStretchModeUniformCommand();
            _elements[CommandType.SetStretchModeUniformToFill] = new SetStretchModeUniformToFillCommand().SetShare(_elements[CommandType.SetStretchModeUniform]);
            _elements[CommandType.SetStretchModeUniformToSize] = new SetStretchModeUniformToSizeCommand().SetShare(_elements[CommandType.SetStretchModeUniform]);
            _elements[CommandType.SetStretchModeUniformToVertical] = new SetStretchModeUniformToVerticalCommand().SetShare(_elements[CommandType.SetStretchModeUniform]);
            _elements[CommandType.SetStretchModeUniformToHorizontal] = new SetStretchModeUniformToHorizontalCommand().SetShare(_elements[CommandType.SetStretchModeUniform]);
            _elements[CommandType.ToggleStretchAllowEnlarge] = new ToggleStretchAllowEnlargeCommand();
            _elements[CommandType.ToggleStretchAllowReduce] = new ToggleStretchAllowReduceCommand();
            _elements[CommandType.ToggleIsEnabledNearestNeighbor] = new ToggleIsEnabledNearestNeighborCommand();
            _elements[CommandType.ToggleBackground] = new ToggleBackgroundCommand();
            _elements[CommandType.SetBackgroundBlack] = new SetBackgroundBlackCommand();
            _elements[CommandType.SetBackgroundWhite] = new SetBackgroundWhiteCommand();
            _elements[CommandType.SetBackgroundAuto] = new SetBackgroundAutoCommand();
            _elements[CommandType.SetBackgroundCheck] = new SetBackgroundCheckCommand();
            _elements[CommandType.SetBackgroundCheckDark] = new SetBackgroundCheckDarkCommand();
            _elements[CommandType.SetBackgroundCustom] = new SetBackgroundCustomCommand();
            _elements[CommandType.TogglePageMode] = new TogglePageModeCommand();
            _elements[CommandType.SetPageMode1] = new SetPageMode1Command();
            _elements[CommandType.SetPageMode2] = new SetPageMode2Command();
            _elements[CommandType.ToggleBookReadOrder] = new ToggleBookReadOrderCommand();
            _elements[CommandType.SetBookReadOrderRight] = new SetBookReadOrderRightCommand();
            _elements[CommandType.SetBookReadOrderLeft] = new SetBookReadOrderLeftCommand();
            _elements[CommandType.ToggleIsSupportedDividePage] = new ToggleIsSupportedDividePageCommand();
            _elements[CommandType.ToggleIsSupportedWidePage] = new ToggleIsSupportedWidePageCommand();
            _elements[CommandType.ToggleIsSupportedSingleFirstPage] = new ToggleIsSupportedSingleFirstPageCommand();
            _elements[CommandType.ToggleIsSupportedSingleLastPage] = new ToggleIsSupportedSingleLastPageCommand();
            _elements[CommandType.ToggleIsRecursiveFolder] = new ToggleIsRecursiveFolderCommand();
            _elements[CommandType.ToggleSortMode] = new ToggleSortModeCommand();
            _elements[CommandType.SetSortModeFileName] = new SetSortModeFileNameCommand();
            _elements[CommandType.SetSortModeFileNameDescending] = new SetSortModeFileNameDescendingCommand();
            _elements[CommandType.SetSortModeTimeStamp] = new SetSortModeTimeStampCommand();
            _elements[CommandType.SetSortModeTimeStampDescending] = new SetSortModeTimeStampDescendingCommand();
            _elements[CommandType.SetSortModeSize] = new SetSortModeSizeCommand();
            _elements[CommandType.SetSortModeSizeDescending] = new SetSortModeSizeDescendingCommand();
            _elements[CommandType.SetSortModeRandom] = new SetSortModeRandomCommand();
            _elements[CommandType.SetDefaultPageSetting] = new SetDefaultPageSettingCommand();
            _elements[CommandType.ToggleBookmark] = new ToggleBookmarkCommand();
            _elements[CommandType.TogglePagemark] = new TogglePagemarkCommand();
            _elements[CommandType.PrevPagemark] = new PrevPagemarkCommand();
            _elements[CommandType.NextPagemark] = new NextPagemarkCommand();
            _elements[CommandType.PrevPagemarkInBook] = new PrevPagemarkInBookCommand();
            _elements[CommandType.NextPagemarkInBook] = new NextPagemarkInBookCommand().SetShare(_elements[CommandType.PrevPagemarkInBook]);
            _elements[CommandType.ViewScrollUp] = new ViewScrollUpCommand();
            _elements[CommandType.ViewScrollDown] = new ViewScrollDownCommand().SetShare(_elements[CommandType.ViewScrollUp]);
            _elements[CommandType.ViewScrollLeft] = new ViewScrollLeftCommand().SetShare(_elements[CommandType.ViewScrollUp]);
            _elements[CommandType.ViewScrollRight] = new ViewScrollRightCommand().SetShare(_elements[CommandType.ViewScrollUp]);
            _elements[CommandType.ViewScaleUp] = new ViewScaleUpCommand();
            _elements[CommandType.ViewScaleDown] = new ViewScaleDownCommand().SetShare(_elements[CommandType.ViewScaleUp]);
            _elements[CommandType.ViewRotateLeft] = new ViewRotateLeftCommand();
            _elements[CommandType.ViewRotateRight] = new ViewRotateRightCommand().SetShare(_elements[CommandType.ViewRotateLeft]);
            _elements[CommandType.ToggleIsAutoRotateLeft] = new ToggleIsAutoRotateLeftCommand();
            _elements[CommandType.ToggleIsAutoRotateRight] = new ToggleIsAutoRotateRightCommand();
            _elements[CommandType.ToggleViewFlipHorizontal] = new ToggleViewFlipHorizontalCommand();
            _elements[CommandType.ViewFlipHorizontalOn] = new ViewFlipHorizontalOnCommand();
            _elements[CommandType.ViewFlipHorizontalOff] = new ViewFlipHorizontalOffCommand();
            _elements[CommandType.ToggleViewFlipVertical] = new ToggleViewFlipVerticalCommand();
            _elements[CommandType.ViewFlipVerticalOn] = new ViewFlipVerticalOnCommand();
            _elements[CommandType.ViewFlipVerticalOff] = new ViewFlipVerticalOffCommand();
            _elements[CommandType.ViewReset] = new ViewResetCommand();
            _elements[CommandType.ToggleCustomSize] = new ToggleCustomSizeCommand();
            _elements[CommandType.ToggleResizeFilter] = new ToggleResizeFilterCommand();
            _elements[CommandType.ToggleGrid] = new ToggleGridCommand();
            _elements[CommandType.ToggleEffect] = new ToggleEffectCommand();
            _elements[CommandType.ToggleIsLoupe] = new ToggleIsLoupeCommand();
            _elements[CommandType.LoupeOn] = new LoupeOnCommand();
            _elements[CommandType.LoupeOff] = new LoupeOffCommand();
            _elements[CommandType.LoupeScaleUp] = new LoupeScaleUpCommand();
            _elements[CommandType.LoupeScaleDown] = new LoupeScaleDownCommand();
            _elements[CommandType.TogglePermitFileCommand] = new TogglePermitFileCommandCommand();
            _elements[CommandType.HelpCommandList] = new HelpCommandListCommand();
            _elements[CommandType.HelpMainMenu] = new HelpMainMenuCommand();
            _elements[CommandType.HelpSearchOption] = new HelpSearchOptionCommand();
            _elements[CommandType.ExportBackup] = new ExportBackupCommand();
            _elements[CommandType.ImportBackup] = new ImportBackupCommand();
            _elements[CommandType.ReloadUserSetting] = new ReloadUserSettingCommand();
            _elements[CommandType.TouchEmulate] = new TouchEmulateCommand();

            // 無効な命令にダミー設定
            foreach (var ignore in CommandTypeExtensions.IgnoreCommandTypes)
            {
                _elements[ignore] = new NoneCommand();
            }

            // 検証
            VerifyCommandTable();

            // デフォルト設定として記憶
            s_defaultMemento = CreateMemento();
        }

        [Conditional("DEBUG")]
        private void VerifyCommandTable()
        {
            var undefinedCollection = Enum.GetValues(typeof(CommandType))
                .Cast<CommandType>()
                .Where(e => !_elements.ContainsKey(e));

            if (undefinedCollection.Any())
            {
                foreach (var undefined in undefinedCollection)
                {
                    Debug.WriteLine($"Error: CommandTable[{undefined}] undefined.");
                }
                throw new InvalidOperationException("CommandTable is invalid.");
            }
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

                if (_elementsV2 != null)
                {
                    if (_Version < Config.GenerateProductVersionNumber(32, 0, 0))
                    {
                        // 新しいコマンドに設定を引き継ぐ
                        if (_elementsV2.TryGetValue("ToggleVisibleFolderSearchBox", out CommandElement.Memento toggleVisibleFolderSearchBox))
                        {
                            _elementsV2[CommandType.FocusFolderSearchBox.ToString()] = toggleVisibleFolderSearchBox;
                        }

                        if (_elementsV2.TryGetValue("ToggleVisibleBookmarkList", out CommandElement.Memento toggleVisibleBookmarkList))
                        {
                            _elementsV2[CommandType.FocusBookmarkList.ToString()] = toggleVisibleBookmarkList;
                        }

                        if (_elementsV2.TryGetValue("ToggleVisibleFolderList", out CommandElement.Memento toggleVisibleFolderList))
                        {
                            _elementsV2[CommandType.ToggleVisibleBookshelf.ToString()] = toggleVisibleFolderList;
                        }
                    }

                    foreach (var element in _elementsV2)
                    {
                        if (Enum.TryParse(element.Key, out CommandType key))
                        {
                            Elements[key] = element.Value;
                        }
                    }
                    _elementsV2 = null;
                }

                // before 34.0
                if (_Version < Config.GenerateProductVersionNumber(34, 0, 0))
                {
                    // 自動回転のショートカットキーをなるべく継承
                    if (Elements.TryGetValue(CommandType.ToggleIsAutoRotate, out var element))
                    {
                        CommandType commandType = element.Parameter is null ? CommandType.ToggleIsAutoRotateRight : CommandType.ToggleIsAutoRotateLeft;
                        Elements[commandType] = element.Clone();
                        Elements[commandType].IsShowMessage = true;
                        Elements[commandType].Parameter = null;
                    }
                }

                // before 35.0
                if (_Version < Config.GenerateProductVersionNumber(35, 0, 0))
                {
                    // ストレッチコマンドパラメータ継承
                    if (Elements.TryGetValue(CommandType.SetStretchModeInside, out var element))
                    {
                        Elements[CommandType.SetStretchModeUniform].Parameter = element.Parameter;
                    }
                }

#pragma warning restore CS0612

                // remove obsolete
                foreach (var key in CommandTypeExtensions.IgnoreCommandTypes)
                {
                    Elements.Remove(key);
                }

                // change shortcut "Escape" to "Esc"
                if (_Version <= Config.GenerateProductVersionNumber(33, 2, 0))
                {
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
