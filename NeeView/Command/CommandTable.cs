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
    public class CommandTable : BindableBase, IEnumerable<KeyValuePair<string, CommandElement>>
    {
        static CommandTable() => Current = new CommandTable();
        public static CommandTable Current { get; }

        #region Fields

        private static Memento s_defaultMemento;

        private Dictionary<string, CommandElement> _elements;
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
        public CommandElement this[string key]
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

        #region Methods

        // NODE: 応急処置
        public IEnumerable<string> Keys => _elements.Keys;

        // NODE: 応急処置
        public bool ContainsKey(string key)
        {
            return _elements.ContainsKey(key);
        }

        public bool TryGetValue(string key, out CommandElement command)
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
        public T Parameter<T>(string commandType) where T : class
        {
            return _elements[commandType].Parameter as T;
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

        #endregion

        #region Methods: Initialize

        /// <summary>
        /// コマンドテーブル初期化
        /// </summary>
        private void InitializeCommandTable()
        {
            // コマンドの設定定義
            _elements = new Dictionary<string, CommandElement>();

            _elements["OpenSettingWindow"] = new OpenSettingWindowCommand();
            _elements["OpenSettingFilesFolder"] = new OpenSettingFilesFolderCommand();
            _elements["OpenVersionWindow"] = new OpenVersionWindowCommand();
            _elements["CloseApplication"] = new CloseApplicationCommand();
            _elements["LoadAs"] = new LoadAsCommand();
            _elements["ReLoad"] = new ReLoadCommand();
            _elements["Unload"] = new UnloadCommand();
            _elements["OpenApplication"] = new OpenApplicationCommand();
            _elements["OpenFilePlace"] = new OpenFilePlaceCommand();
            _elements["Export"] = new ExportCommand();
            _elements["ExportImage"] = new ExportImageCommand();
            _elements["Print"] = new PrintCommand();
            _elements["DeleteFile"] = new DeleteFileCommand();
            _elements["DeleteBook"] = new DeleteBookCommand();
            _elements["CopyFile"] = new CopyFileCommand();
            _elements["CopyImage"] = new CopyImageCommand();
            _elements["Paste"] = new PasteCommand();
            _elements["OpenContextMenu"] = new OpenContextMenuCommand();
            _elements["ClearHistory"] = new ClearHistoryCommand();
            _elements["ClearHistoryInPlace"] = new ClearHistoryInPlaceCommand();
            _elements["PrevPage"] = new PrevPageCommand();
            _elements["NextPage"] = new NextPageCommand().SetShare(_elements["PrevPage"]);
            _elements["PrevOnePage"] = new PrevOnePageCommand();
            _elements["NextOnePage"] = new NextOnePageCommand().SetShare(_elements["PrevOnePage"]);
            _elements["PrevScrollPage"] = new PrevScrollPageCommand();
            _elements["NextScrollPage"] = new NextScrollPageCommand().SetShare(_elements["PrevScrollPage"]);
            _elements["JumpPage"] = new JumpPageCommand();
            _elements["PrevSizePage"] = new PrevSizePageCommand();
            _elements["NextSizePage"] = new NextSizePageCommand().SetShare(_elements["PrevSizePage"]);
            _elements["PrevFolderPage"] = new PrevFolderPageCommand();
            _elements["NextFolderPage"] = new NextFolderPageCommand().SetShare(_elements["PrevFolderPage"]);
            _elements["FirstPage"] = new FirstPageCommand();
            _elements["LastPage"] = new LastPageCommand().SetShare(_elements["FirstPage"]);
            _elements["ToggleMediaPlay"] = new ToggleMediaPlayCommand();
            _elements["PrevFolder"] = new PrevFolderCommand();
            _elements["NextFolder"] = new NextFolderCommand();
            _elements["PrevHistory"] = new PrevHistoryCommand();
            _elements["NextHistory"] = new NextHistoryCommand();
            _elements["PrevBookHistory"] = new PrevBookHistoryCommand();
            _elements["NextBookHistory"] = new NextBookHistoryCommand();
            _elements["MoveToParentBook"] = new MoveToParentBookCommand();
            _elements["MoveToChildBook"] = new MoveToChildBookCommand();
            _elements["ToggleFolderOrder"] = new ToggleFolderOrderCommand();
            _elements["SetFolderOrderByFileNameA"] = new SetFolderOrderByFileNameACommand();
            _elements["SetFolderOrderByFileNameD"] = new SetFolderOrderByFileNameDCommand();
            _elements["SetFolderOrderByPathA"] = new SetFolderOrderByPathACommand();
            _elements["SetFolderOrderByPathD"] = new SetFolderOrderByPathDCommand();
            _elements["SetFolderOrderByFileTypeA"] = new SetFolderOrderByFileTypeACommand();
            _elements["SetFolderOrderByFileTypeD"] = new SetFolderOrderByFileTypeDCommand();
            _elements["SetFolderOrderByTimeStampA"] = new SetFolderOrderByTimeStampACommand();
            _elements["SetFolderOrderByTimeStampD"] = new SetFolderOrderByTimeStampDCommand();
            _elements["SetFolderOrderByEntryTimeA"] = new SetFolderOrderByEntryTimeACommand();
            _elements["SetFolderOrderByEntryTimeD"] = new SetFolderOrderByEntryTimeDCommand();
            _elements["SetFolderOrderBySizeA"] = new SetFolderOrderBySizeACommand();
            _elements["SetFolderOrderBySizeD"] = new SetFolderOrderBySizeDCommand();
            _elements["SetFolderOrderByRandom"] = new SetFolderOrderByRandomCommand();
            _elements["ToggleTopmost"] = new ToggleTopmostCommand();
            _elements["ToggleHideMenu"] = new ToggleHideMenuCommand();
            _elements["ToggleHidePageSlider"] = new ToggleHidePageSliderCommand();
            _elements["ToggleHidePanel"] = new ToggleHidePanelCommand();
            _elements["ToggleVisibleTitleBar"] = new ToggleVisibleTitleBarCommand();
            _elements["ToggleVisibleAddressBar"] = new ToggleVisibleAddressBarCommand();
            _elements["ToggleVisibleSideBar"] = new ToggleVisibleSideBarCommand();
            _elements["ToggleVisibleFileInfo"] = new ToggleVisibleFileInfoCommand();
            _elements["ToggleVisibleEffectInfo"] = new ToggleVisibleEffectInfoCommand();
            _elements["ToggleVisibleBookshelf"] = new ToggleVisibleBookshelfCommand();
            _elements["ToggleVisibleBookmarkList"] = new ToggleVisibleBookmarkListCommand();
            _elements["ToggleVisiblePagemarkList"] = new ToggleVisiblePagemarkListCommand();
            _elements["ToggleVisibleHistoryList"] = new ToggleVisibleHistoryListCommand();
            _elements["ToggleVisiblePageList"] = new ToggleVisiblePageListCommand();
            _elements["ToggleVisibleFoldersTree"] = new ToggleVisibleFoldersTreeCommand();
            _elements["FocusFolderSearchBox"] = new FocusFolderSearchBoxCommand();
            _elements["FocusBookmarkList"] = new FocusBookmarkListCommand();
            _elements["FocusMainView"] = new FocusMainViewCommand();
            _elements["TogglePageListPlacement"] = new TogglePageListPlacementCommand();
            _elements["ToggleVisibleThumbnailList"] = new ToggleVisibleThumbnailListCommand();
            _elements["ToggleHideThumbnailList"] = new ToggleHideThumbnailListCommand();
            _elements["ToggleFullScreen"] = new ToggleFullScreenCommand();
            _elements["SetFullScreen"] = new SetFullScreenCommand();
            _elements["CancelFullScreen"] = new CancelFullScreenCommand();
            _elements["ToggleWindowMinimize"] = new ToggleWindowMinimizeCommand();
            _elements["ToggleWindowMaximize"] = new ToggleWindowMaximizeCommand();
            _elements["ShowHiddenPanels"] = new ShowHiddenPanelsCommand();
            _elements["ToggleSlideShow"] = new ToggleSlideShowCommand();
            _elements["ToggleStretchMode"] = new ToggleStretchModeCommand();
            _elements["ToggleStretchModeReverse"] = new ToggleStretchModeReverseCommand().SetShare(_elements["ToggleStretchMode"]);
            _elements["SetStretchModeNone"] = new SetStretchModeNoneCommand();
            _elements["SetStretchModeUniform"] = new SetStretchModeUniformCommand();
            _elements["SetStretchModeUniformToFill"] = new SetStretchModeUniformToFillCommand().SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToSize"] = new SetStretchModeUniformToSizeCommand().SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToVertical"] = new SetStretchModeUniformToVerticalCommand().SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToHorizontal"] = new SetStretchModeUniformToHorizontalCommand().SetShare(_elements["SetStretchModeUniform"]);
            _elements["ToggleStretchAllowEnlarge"] = new ToggleStretchAllowEnlargeCommand();
            _elements["ToggleStretchAllowReduce"] = new ToggleStretchAllowReduceCommand();
            _elements["ToggleIsEnabledNearestNeighbor"] = new ToggleIsEnabledNearestNeighborCommand();
            _elements["ToggleBackground"] = new ToggleBackgroundCommand();
            _elements["SetBackgroundBlack"] = new SetBackgroundBlackCommand();
            _elements["SetBackgroundWhite"] = new SetBackgroundWhiteCommand();
            _elements["SetBackgroundAuto"] = new SetBackgroundAutoCommand();
            _elements["SetBackgroundCheck"] = new SetBackgroundCheckCommand();
            _elements["SetBackgroundCheckDark"] = new SetBackgroundCheckDarkCommand();
            _elements["SetBackgroundCustom"] = new SetBackgroundCustomCommand();
            _elements["TogglePageMode"] = new TogglePageModeCommand();
            _elements["SetPageMode1"] = new SetPageMode1Command();
            _elements["SetPageMode2"] = new SetPageMode2Command();
            _elements["ToggleBookReadOrder"] = new ToggleBookReadOrderCommand();
            _elements["SetBookReadOrderRight"] = new SetBookReadOrderRightCommand();
            _elements["SetBookReadOrderLeft"] = new SetBookReadOrderLeftCommand();
            _elements["ToggleIsSupportedDividePage"] = new ToggleIsSupportedDividePageCommand();
            _elements["ToggleIsSupportedWidePage"] = new ToggleIsSupportedWidePageCommand();
            _elements["ToggleIsSupportedSingleFirstPage"] = new ToggleIsSupportedSingleFirstPageCommand();
            _elements["ToggleIsSupportedSingleLastPage"] = new ToggleIsSupportedSingleLastPageCommand();
            _elements["ToggleIsRecursiveFolder"] = new ToggleIsRecursiveFolderCommand();
            _elements["ToggleSortMode"] = new ToggleSortModeCommand();
            _elements["SetSortModeFileName"] = new SetSortModeFileNameCommand();
            _elements["SetSortModeFileNameDescending"] = new SetSortModeFileNameDescendingCommand();
            _elements["SetSortModeTimeStamp"] = new SetSortModeTimeStampCommand();
            _elements["SetSortModeTimeStampDescending"] = new SetSortModeTimeStampDescendingCommand();
            _elements["SetSortModeSize"] = new SetSortModeSizeCommand();
            _elements["SetSortModeSizeDescending"] = new SetSortModeSizeDescendingCommand();
            _elements["SetSortModeRandom"] = new SetSortModeRandomCommand();
            _elements["SetDefaultPageSetting"] = new SetDefaultPageSettingCommand();
            _elements["ToggleBookmark"] = new ToggleBookmarkCommand();
            _elements["TogglePagemark"] = new TogglePagemarkCommand();
            _elements["PrevPagemark"] = new PrevPagemarkCommand();
            _elements["NextPagemark"] = new NextPagemarkCommand();
            _elements["PrevPagemarkInBook"] = new PrevPagemarkInBookCommand();
            _elements["NextPagemarkInBook"] = new NextPagemarkInBookCommand().SetShare(_elements["PrevPagemarkInBook"]);
            _elements["ViewScrollUp"] = new ViewScrollUpCommand();
            _elements["ViewScrollDown"] = new ViewScrollDownCommand().SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScrollLeft"] = new ViewScrollLeftCommand().SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScrollRight"] = new ViewScrollRightCommand().SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScaleUp"] = new ViewScaleUpCommand();
            _elements["ViewScaleDown"] = new ViewScaleDownCommand().SetShare(_elements["ViewScaleUp"]);
            _elements["ViewRotateLeft"] = new ViewRotateLeftCommand();
            _elements["ViewRotateRight"] = new ViewRotateRightCommand().SetShare(_elements["ViewRotateLeft"]);
            _elements["ToggleIsAutoRotateLeft"] = new ToggleIsAutoRotateLeftCommand();
            _elements["ToggleIsAutoRotateRight"] = new ToggleIsAutoRotateRightCommand();
            _elements["ToggleViewFlipHorizontal"] = new ToggleViewFlipHorizontalCommand();
            _elements["ViewFlipHorizontalOn"] = new ViewFlipHorizontalOnCommand();
            _elements["ViewFlipHorizontalOff"] = new ViewFlipHorizontalOffCommand();
            _elements["ToggleViewFlipVertical"] = new ToggleViewFlipVerticalCommand();
            _elements["ViewFlipVerticalOn"] = new ViewFlipVerticalOnCommand();
            _elements["ViewFlipVerticalOff"] = new ViewFlipVerticalOffCommand();
            _elements["ViewReset"] = new ViewResetCommand();
            _elements["ToggleCustomSize"] = new ToggleCustomSizeCommand();
            _elements["ToggleResizeFilter"] = new ToggleResizeFilterCommand();
            _elements["ToggleGrid"] = new ToggleGridCommand();
            _elements["ToggleEffect"] = new ToggleEffectCommand();
            _elements["ToggleIsLoupe"] = new ToggleIsLoupeCommand();
            _elements["LoupeOn"] = new LoupeOnCommand();
            _elements["LoupeOff"] = new LoupeOffCommand();
            _elements["LoupeScaleUp"] = new LoupeScaleUpCommand();
            _elements["LoupeScaleDown"] = new LoupeScaleDownCommand();
            _elements["TogglePermitFileCommand"] = new TogglePermitFileCommandCommand();
            _elements["HelpCommandList"] = new HelpCommandListCommand();
            _elements["HelpMainMenu"] = new HelpMainMenuCommand();
            _elements["HelpSearchOption"] = new HelpSearchOptionCommand();
            _elements["ExportBackup"] = new ExportBackupCommand();
            _elements["ImportBackup"] = new ImportBackupCommand();
            _elements["ReloadUserSetting"] = new ReloadUserSettingCommand();
            _elements["TouchEmulate"] = new TouchEmulateCommand();

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

            [DataMember, DefaultValue(true)]
            public bool IsReversePageMove { get; set; }

            [DataMember]
            public bool IsReversePageMoveWheel { get; set; }

            [DataMember(Name = "ElementsV2")]
            public Dictionary<string, CommandElement.Memento> Elements { get; set; } = new Dictionary<string, CommandElement.Memento>();


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

                Elements = Elements ?? new Dictionary<string, CommandElement.Memento>();

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

                // before 34.0
                if (_Version < Config.GenerateProductVersionNumber(34, 0, 0))
                {
                    // 自動回転のショートカットキーをなるべく継承
                    if (Elements.TryGetValue("ToggleIsAutoRotate", out var element))
                    {
                        var commandType = element.Parameter is null ? "ToggleIsAutoRotateRight" : "ToggleIsAutoRotateLeft";
                        Elements[commandType] = element.Clone();
                        Elements[commandType].IsShowMessage = true;
                        Elements[commandType].Parameter = null;
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

#pragma warning restore CS0612

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

        public Memento CreateMemento()
        {
            var memento = new Memento();

            foreach (var pair in _elements)
            {
                memento.Elements.Add(pair.Key, pair.Value.CreateMemento());
            }

            memento.IsReversePageMove = this.IsReversePageMove;
            memento.IsReversePageMoveWheel = this.IsReversePageMoveWheel;

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
