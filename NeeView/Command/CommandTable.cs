﻿using NeeLaboratory.ComponentModel;
using NeeView.Data;
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
    /// 廃棄されたコマンドの情報
    /// </summary>
    public class ObsoleteCommandItem
    {
        public ObsoleteCommandItem(string obsolete, string alternative, int version)
        {
            Obsolete = obsolete;
            Alternative = alternative;
            Version = version;
        }

        public string Obsolete { get; }
        public string Alternative { get; }
        public int Version { get; }
    }


    /// <summary>
    /// コマンド設定テーブル
    /// </summary>
    public partial class CommandTable : BindableBase, IDictionary<string, CommandElement>, IDisposable
    {
        static CommandTable() => Current = new CommandTable();
        public static CommandTable Current { get; }


        private Dictionary<string, CommandElement> _elements;
        private bool _disposedValue;


        private CommandTable()
        {
            InitializeCommandTable();

            this.ScriptManager = new ScriptManager(this);

            Changed += CommandTable_Changed;

            ApplicationDisposer.Current.Add(this);
        }


        /// <summary>
        /// コマンドテーブルが変更された
        /// </summary>
        public event EventHandler<CommandChangedEventArgs> Changed;


        public ScriptManager ScriptManager { get; private set; }

        public CommandCollection DefaultMemento { get; private set; }

        public int ChangeCount { get; private set; }

        public Dictionary<string, CommandElement> Elements => _elements;

        public Dictionary<string, ObsoleteCommandItem> ObsoleteCommands { get; private set; }

        #region IDictionary Support

        public ICollection<string> Keys => ((IDictionary<string, CommandElement>)_elements).Keys;

        public ICollection<CommandElement> Values => ((IDictionary<string, CommandElement>)_elements).Values;

        public int Count => ((ICollection<KeyValuePair<string, CommandElement>>)_elements).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, CommandElement>>)_elements).IsReadOnly;

        public CommandElement this[string key]
        {
            get => ((IDictionary<string, CommandElement>)_elements)[key];
            set => ((IDictionary<string, CommandElement>)_elements)[key] = value;
        }

        public void Add(string key, CommandElement value)
        {
            ((IDictionary<string, CommandElement>)_elements).Add(key, value);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, CommandElement>)_elements).Remove(key);
        }

        public void Add(KeyValuePair<string, CommandElement> item)
        {
            ((ICollection<KeyValuePair<string, CommandElement>>)_elements).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, CommandElement>>)_elements).Clear();
        }

        public bool Contains(KeyValuePair<string, CommandElement> item)
        {
            return ((ICollection<KeyValuePair<string, CommandElement>>)_elements).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return key != null && _elements.ContainsKey(key);
        }

        public bool TryGetValue(string key, out CommandElement value)
        {
            return ((IDictionary<string, CommandElement>)_elements).TryGetValue(key, out value);
        }

        public void CopyTo(KeyValuePair<string, CommandElement>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, CommandElement>>)_elements).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, CommandElement> item)
        {
            return ((ICollection<KeyValuePair<string, CommandElement>>)_elements).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, CommandElement>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, CommandElement>>)_elements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_elements).GetEnumerator();
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
                new LoadAsCommand(),
                new ReLoadCommand(),
                new UnloadCommand(),
                new OpenExternalAppCommand(),
                new OpenExplorerCommand(),
                new ExportImageAsCommand(),
                new ExportImageCommand(),
                new PrintCommand(),
                new DeleteFileCommand(),
                new DeleteBookCommand(),
                new CopyFileCommand(),
                new CopyImageCommand(),
                new PasteCommand(),

                new ClearHistoryCommand(),
                new ClearHistoryInPlaceCommand(),
                new RemoveUnlinkedHistoryCommand(),

                new ToggleStretchModeCommand(),
                new ToggleStretchModeReverseCommand(),
                new SetStretchModeNoneCommand(),
                new SetStretchModeUniformCommand(),
                new SetStretchModeUniformToFillCommand(),
                new SetStretchModeUniformToSizeCommand(),
                new SetStretchModeUniformToVerticalCommand(),
                new SetStretchModeUniformToHorizontalCommand(),
                new ToggleStretchAllowScaleUpCommand(),
                new ToggleStretchAllowScaleDownCommand(),
                new ToggleNearestNeighborCommand(),
                new ToggleBackgroundCommand(),
                new SetBackgroundBlackCommand(),
                new SetBackgroundWhiteCommand(),
                new SetBackgroundAutoCommand(),
                new SetBackgroundCheckCommand(),
                new SetBackgroundCheckDarkCommand(),
                new SetBackgroundCustomCommand(),
                new ToggleTopmostCommand(),
                new ToggleVisibleAddressBarCommand(),
                new ToggleHideMenuCommand(),
                new ToggleVisibleSideBarCommand(),
                new ToggleHidePanelCommand(),
                new ToggleVisiblePageSliderCommand(),
                new ToggleHidePageSliderCommand(),

                new ToggleVisibleBookshelfCommand(),
                new ToggleVisiblePageListCommand(),
                new ToggleVisibleBookmarkListCommand(),
                new ToggleVisiblePlaylistCommand(),
                new ToggleVisibleHistoryListCommand(),
                new ToggleVisibleFileInfoCommand(),
                new ToggleVisibleNavigatorCommand(),
                new ToggleVisibleEffectInfoCommand(),
                new ToggleVisibleFoldersTreeCommand(),
                new FocusFolderSearchBoxCommand(),
                new FocusBookmarkListCommand(),
                new FocusMainViewCommand(),
                new ToggleVisibleThumbnailListCommand(),
                new ToggleHideThumbnailListCommand(),
                new ToggleMainViewFloatingCommand(),

                new ToggleFullScreenCommand(),
                new SetFullScreenCommand(),
                new CancelFullScreenCommand(),
                new ToggleWindowMinimizeCommand(),
                new ToggleWindowMaximizeCommand(),
                new ShowHiddenPanelsCommand(),

                new ToggleSlideShowCommand(),
                new ToggleHoverScrollCommand(),

                new ViewScrollNTypeUpCommand(),
                new ViewScrollNTypeDownCommand(),
                new ViewScrollUpCommand(),
                new ViewScrollDownCommand(),
                new ViewScrollLeftCommand(),
                new ViewScrollRightCommand(),
                new ViewScaleUpCommand(),
                new ViewScaleDownCommand(),
                new ViewRotateLeftCommand(),
                new ViewRotateRightCommand(),
                new ToggleIsAutoRotateLeftCommand(),
                new ToggleIsAutoRotateRightCommand(),

                new ToggleViewFlipHorizontalCommand(),
                new ViewFlipHorizontalOnCommand(),
                new ViewFlipHorizontalOffCommand(),

                new ToggleViewFlipVerticalCommand(),
                new ViewFlipVerticalOnCommand(),
                new ViewFlipVerticalOffCommand(),
                new ViewResetCommand(),

                new PrevPageCommand(),
                new NextPageCommand(),
                new PrevOnePageCommand(),
                new NextOnePageCommand(),

                new PrevScrollPageCommand(),
                new NextScrollPageCommand(),
                new JumpPageCommand(),
                new JumpRandomPageCommand(),
                new PrevSizePageCommand(),
                new NextSizePageCommand(),
                new PrevFolderPageCommand(),
                new NextFolderPageCommand(),
                new FirstPageCommand(),
                new LastPageCommand(),
                new PrevHistoryPageCommand(),
                new NextHistoryPageCommand(),

                new PrevBookCommand(),
                new NextBookCommand(),
                new RandomBookCommand(),
                new PrevHistoryCommand(),
                new NextHistoryCommand(),

                new PrevBookHistoryCommand(),
                new NextBookHistoryCommand(),
                new MoveToParentBookCommand(),
                new MoveToChildBookCommand(),

                new ToggleMediaPlayCommand(),
                new ToggleBookOrderCommand(),
                new SetBookOrderByFileNameACommand(),
                new SetBookOrderByFileNameDCommand(),
                new SetBookOrderByPathACommand(),
                new SetBookOrderByPathDCommand(),
                new SetBookOrderByFileTypeACommand(),
                new SetBookOrderByFileTypeDCommand(),
                new SetBookOrderByTimeStampACommand(),
                new SetBookOrderByTimeStampDCommand(),
                new SetBookOrderByEntryTimeACommand(),
                new SetBookOrderByEntryTimeDCommand(),
                new SetBookOrderBySizeACommand(),
                new SetBookOrderBySizeDCommand(),
                new SetBookOrderByRandomCommand(),
                new TogglePageModeCommand(),
                new SetPageModeOneCommand(),
                new SetPageModeTwoCommand(),
                new ToggleBookReadOrderCommand(),
                new SetBookReadOrderRightCommand(),
                new SetBookReadOrderLeftCommand(),
                new ToggleIsSupportedDividePageCommand(),
                new ToggleIsSupportedWidePageCommand(),
                new ToggleIsSupportedSingleFirstPageCommand(),
                new ToggleIsSupportedSingleLastPageCommand(),
                new ToggleIsRecursiveFolderCommand(),
                new ToggleSortModeCommand(),
                new SetSortModeFileNameCommand(),
                new SetSortModeFileNameDescendingCommand(),
                new SetSortModeTimeStampCommand(),
                new SetSortModeTimeStampDescendingCommand(),
                new SetSortModeSizeCommand(),
                new SetSortModeSizeDescendingCommand(),
                new SetSortModeEntryCommand(),
                new SetSortModeEntryDescendingCommand(),
                new SetSortModeRandomCommand(),
                new SetDefaultPageSettingCommand(),

                new ToggleBookmarkCommand(),
                new TogglePlaylistItemCommand(),
                new PrevPlaylistItemCommand(),
                new NextPlaylistItemCommand(),
                new PrevPlaylistItemInBookCommand(),
                new NextPlaylistItemInBookCommand(),

                new ToggleCustomSizeCommand(),

                new ToggleResizeFilterCommand(),
                new ToggleGridCommand(),
                new ToggleEffectCommand(),

                new ToggleIsLoupeCommand(),
                new LoupeOnCommand(),
                new LoupeOffCommand(),
                new LoupeScaleUpCommand(),
                new LoupeScaleDownCommand(),
                new OpenOptionsWindowCommand(),
                new OpenSettingFilesFolderCommand(),
                new OpenScriptsFolderCommand(),
                new OpenVersionWindowCommand(),
                new CloseApplicationCommand(),

                new TogglePermitFileCommand(),

                new HelpCommandListCommand(),
                new HelpScriptCommand(),
                new HelpMainMenuCommand(),
                new HelpSearchOptionCommand(),
                new OpenContextMenuCommand(),

                new ExportBackupCommand(),
                new ImportBackupCommand(),
                new ReloadSettingCommand(),
                new SaveSettingCommand(),
                new TouchEmulateCommand(),

                new FocusPrevAppCommand(),
                new FocusNextAppCommand(),

                new StretchWindowCommand(),

                new OpenConsoleCommand(),

                new CancelScriptCommand()
            };

            // command list order
            foreach (var item in list.GroupBy(e => e.Group).SelectMany(e => e).Select((e, index) => (e, index)))
            {
                item.e.Order = item.index;
            }

            // to dictionary
            _elements = list.ToDictionary(e => e.Name);

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
            _elements["NextPlaylistItemInBook"].SetShare(_elements["PrevPlaylistItemInBook"]);
            _elements["ViewScrollNTypeDown"].SetShare(_elements["ViewScrollNTypeUp"]);
            _elements["ViewScrollDown"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScrollLeft"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScrollRight"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScaleDown"].SetShare(_elements["ViewScaleUp"]);
            _elements["ViewRotateRight"].SetShare(_elements["ViewRotateLeft"]);

            // TODO: pair...

            // デフォルト設定として記憶
            DefaultMemento = CreateCommandCollectionMemento();

            // 廃棄されたコマンドの情報
            var obsoleteCommands = new List<ObsoleteCommandItem>()
            {
                new ObsoleteCommandItem("ToggleVisibleTitleBar", null, 39),
                new ObsoleteCommandItem("ToggleVisiblePagemarkList", "ToggleVisiblePlaylist", 39),
                new ObsoleteCommandItem("TogglePagemark", "TogglePlaylistMark", 39),
                new ObsoleteCommandItem("PrevPagemark", "PrevPlaylistItem", 39),
                new ObsoleteCommandItem("NextPagemark", "NextPlaylistItem", 39),
                new ObsoleteCommandItem("PrevPagemarkInBook", "PrevPlaylistItemInBook", 39),
                new ObsoleteCommandItem("NextPagemarkInBook", "NextPlaylistItemInBook", 39),
            };
            ObsoleteCommands = obsoleteCommands.ToDictionary(e => e.Obsolete);
        }

        #endregion

        #region Methods

        public CommandElement GetElement(string key)
        {
            if (TryGetValue(key, out CommandElement command))
            {
                return command;
            }
            else
            {
                return CommandElement.None;
            }
        }

        public CommandElement CreateCloneCommand(CommandElement source)
        {
            var cloneCommand = CloneCommand(source);

            Changed?.Invoke(this, new CommandChangedEventArgs(false));

            return cloneCommand;
        }

        public void RemoveCloneCommand(CommandElement command)
        {
            if (command.IsCloneCommand())
            {
                _elements.Remove(command.Name);

                Changed?.Invoke(this, new CommandChangedEventArgs(false));
            }
        }

        private CommandElement CloneCommand(CommandElement source)
        {
            var cloneCommandName = CraeteUniqueCommandName(source.NameSource);
            return CloneCommand(source, cloneCommandName);
        }

        private CommandElement CloneCommand(CommandElement source, CommandNameSource name)
        {
            var cloneCommand = source.CloneCommand(name);
            _elements.Add(cloneCommand.Name, cloneCommand);
            ValudateOrder();
            return cloneCommand;
        }

        private CommandNameSource CraeteUniqueCommandName(CommandNameSource name)
        {
            if (!_elements.ContainsKey(name.FullName))
            {
                return name;
            }

            for (int id = 2; ; id++)
            {
                var newName = new CommandNameSource(name.Name, id);
                if (!_elements.ContainsKey(newName.FullName))
                {
                    return newName;
                }
            }
        }

        private void ValudateOrder()
        {
            var sorted = _elements.Values
                .OrderBy(e => e.Order)
                .GroupBy(e => e.GetType())
                .Select(group => group.OrderBy(e => e.NameSource))
                .SelectMany(e => e)
                .ToList();

            foreach (var item in sorted.Select((e, i) => (e, i)))
            {
                item.e.Order = item.i;
            }
        }

        /// <summary>
        /// テーブル更新イベントを発行
        /// </summary>
        public void RaiseChanged()
        {
            Changed?.Invoke(this, new CommandChangedEventArgs(false));
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
        public static CommandCollection CreateDefaultMemento(InputSceme type)
        {
            var memento = CommandTable.Current.DefaultMemento.Clone();

            // Type.M
            switch (type)
            {
                case InputSceme.TypeA: // default
                    break;

                case InputSceme.TypeB: // wheel page, right click contextmenu
                    memento["NextScrollPage"].ShortCutKey = null;
                    memento["PrevScrollPage"].ShortCutKey = null;
                    memento["NextPage"].ShortCutKey = "Left,WheelDown";
                    memento["PrevPage"].ShortCutKey = "Right,WheelUp";
                    memento["OpenContextMenu"].ShortCutKey = "RightClick";
                    break;

                case InputSceme.TypeC: // click page
                    memento["NextScrollPage"].ShortCutKey = null;
                    memento["PrevScrollPage"].ShortCutKey = null;
                    memento["NextPage"].ShortCutKey = "Left,LeftClick";
                    memento["PrevPage"].ShortCutKey = "Right,RightClick";
                    memento["ViewScrollUp"].ShortCutKey = "WheelUp";
                    memento["ViewScrollDown"].ShortCutKey = "WheelDown";
                    break;
            }

            return memento;
        }

        public bool TryExecute(object sender, string commandName, object[] args, CommandOption option)
        {
            if (TryGetValue(commandName, out CommandElement command))
            {
                var arguments = new CommandArgs(args, option);
                if (command.CanExecute(sender, arguments))
                {
                    command.Execute(sender, arguments);
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
                AppDispatcher.Invoke(() => Changed?.Invoke(this, new CommandChangedEventArgs(false)));
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
            Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "CommandList.html");

            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.WriteLine(HtmlHelpUtility.CraeteHeader("NeeView Command List"));
                writer.WriteLine($"<body><h1>{Properties.Resources.HelpCommandList_Title}</h1>");
                writer.WriteLine($"<p>{Properties.Resources.HelpCommandList_Message}</p>");
                writer.WriteLine("<table class=\"table-slim table-topless\">");
                writer.WriteLine($"<tr><th>{Properties.Resources.Word_Group}</th><th>{Properties.Resources.Word_Command}</th><th>{Properties.Resources.Word_Shortcut}</th><th>{Properties.Resources.Word_Gesture}</th><th>{Properties.Resources.Word_Touch}</th><th>{Properties.Resources.Word_Summary}</th></tr>");
                foreach (var command in _elements.Values.OrderBy(e => e.Order))
                {
                    writer.WriteLine($"<tr><td>{command.Group}</td><td>{command.Text}</td><td>{command.ShortCutKey}</td><td>{new MouseGestureSequence(command.MouseGesture).ToDispString()}</td><td>{command.TouchGesture}</td><td>{command.Remarks}</td></tr>");
                }
                writer.WriteLine("</table>");
                writer.WriteLine("</body>");

                writer.WriteLine(HtmlHelpUtility.CreateFooter());
            }

            ExternalProcess.Start(fileName);
        }

        #endregion

        #region Scripts

        /// <summary>
        /// スクリプトコマンド更新要求
        /// </summary>
        /// <param name="commands">新しいスクリプトコマンド群</param>
        /// <param name="isReplace">登録済コマンドも置き換える</param>
        public void SetScriptCommands(IEnumerable<ScriptCommand> commands, bool isReplace)
        {
            var newers = (commands ?? new List<ScriptCommand>())
                .ToList();

            var oldies = _elements.Values
                .OfType<ScriptCommand>()
                .ToList();

            // 入れ替えの場合は既存の設定をすべて削除
            if (isReplace)
            {
                foreach (var command in oldies)
                {
                    _elements.Remove(command.Name);
                }
                oldies = new List<ScriptCommand>();
            }

            // 存在しないものは削除
            var newPaths = newers.Select(e => e.Path).ToList();
            var exceps = oldies.Where(e => !newPaths.Contains(e.Path)).ToList();
            foreach (var command in exceps)
            {
                _elements.Remove(command.Name);
            }

            // 既存のものは情報更新
            var updates = oldies.Except(exceps).ToList();
            foreach (var command in updates)
            {
                command.UpdateDocument(false);
            }

            // 新規のものは追加
            var overwritesPaths = updates.Select(e => e.Path).Distinct().ToList();
            var news = newers.Where(e => !overwritesPaths.Contains(e.Path)).ToList();
            foreach (var command in news)
            {
                _elements.Add(command.Name, command);
            }

            // re order
            var scripts = _elements.Values.OfType<ScriptCommand>().OrderBy(e => e.NameSource.Name).ThenBy(e =>e.NameSource.Number);
            var offset = _elements.Count;
            foreach (var item in scripts.Select((e, i) => (e, i)))
            {
                item.e.Order = offset + item.i;
            }

            Debug.Assert(_elements.Values.GroupBy(e => e.Order).All(e => e.Count() == 1));
            Changed?.Invoke(this, new CommandChangedEventArgs(false));
        }

        #endregion Scripts

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.ScriptManager.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region Memento

        [DataContract]
        public class Memento
        {
            public static Dictionary<string, string> RenameMap_37_0_0 = new Dictionary<string, string>()
            {
                ["OpenApplication"] = "OpenExternalApp",
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
                ["ReloadUserSetting"] = "ReloadSetting",
                ["OpenSettingWindow"] = "OpenOptionsWindow",
                ["ToggleStretchAllowEnlarge"] = "ToggleStretchAllowScaleUp",
                ["ToggleStretchAllowReduce"] = "ToggleStretchAllowScaleDown",
            };

            public static Dictionary<string, string> RenameMap_38_0_0 = new Dictionary<string, string>()
            {
                ["TogglePermitFileCommand"] = "TogglePermitFile",
                ["FocusPrevAppCommand"] = "FocusPrevApp",
                ["FocusNextAppCommand"] = "FocusNextApp",
            };

            public static Dictionary<string, string> RenameMap_39_0_0 = new Dictionary<string, string>()
            {
                ["ToggleVisiblePagemarkList"] = "ToggleVisiblePlaylist",
                ["TogglePagemark"] = "TogglePlaylistMark",
                ["PrevPagemark"] = "PrevPlaylistItem",
                ["NextPagemark"] = "NextPlaylistItem",
                ["PrevPagemarkInBook"] = "PrevPlaylistItemInBook",
                ["NextPagemarkInBook"] = "NextPlaylistItemInBook",
            };

            public int _Version { get; set; } = Environment.ProductVersionNumber;

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
                Elements = Elements ?? new Dictionary<string, CommandElement.Memento>();

                // before 32.0
                if (_Version < Environment.GenerateProductVersionNumber(32, 0, 0))
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
                if (_Version <= Environment.GenerateProductVersionNumber(33, 2, 0))
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
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
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
                if (_Version < Environment.GenerateProductVersionNumber(35, 0, 0))
                {
                    // ストレッチコマンドパラメータ継承
                    if (Elements.TryGetValue("SetStretchModeInside", out var element))
                    {
                        Elements["SetStretchModeUniform"].Parameter = element.Parameter;
                    }
                }

                // before 37.0
                if (_Version < Environment.GenerateProductVersionNumber(37, 0, 0))
                {
                    foreach (var pair in RenameMap_37_0_0)
                    {
                        Rename(pair.Key, pair.Value);
                    }

                    if (Elements["PrevHistory"].ShortCutKey == "Back")
                    {
                        Elements["PrevHistory"].ShortCutKey = "";
                    }
                    if (Elements["NextHistory"].ShortCutKey == "Shift+Back")
                    {
                        Elements["NextHistory"].ShortCutKey = "";
                    }
                }

                if (_Version < Environment.GenerateProductVersionNumber(38, 0, 0))
                {
                    foreach (var pair in RenameMap_38_0_0)
                    {
                        Rename(pair.Key, pair.Value);
                    }
                }

                if (_Version < Environment.GenerateProductVersionNumber(39, 0, 0))
                {
                    foreach (var pair in RenameMap_39_0_0)
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
                        Elements.Remove(oldName);
                    }
                }
            }

            public void RestoreConfig(Config config)
            {
                config.Command.IsReversePageMove = IsReversePageMove;
                config.Command.IsReversePageMoveWheel = IsReversePageMoveWheel;
                config.Script.IsScriptFolderEnabled = IsScriptFolderEnabled;
                config.Script.ScriptFolder = ScriptFolder;
            }

            public Memento Clone()
            {
                var memento = (Memento)this.MemberwiseClone();
                memento.Elements = this.Elements.ToDictionary(e => e.Key, e => e.Value.Clone());
                return memento;
            }

            public CommandCollection CreateCommandCollection()
            {
                // TODO: とてもしっくりこないアクセスなのでいい感じに修正する
                var elements = CommandTable.Current._elements;

                var collection = new CommandCollection();
                foreach (var pair in Elements)
                {
                    if (elements.ContainsKey(pair.Key))
                    {
                        var parameterType = elements[pair.Key].ParameterSource?.GetDefault().GetType();
                        var value = CreateCommandMementoV2(pair.Value, parameterType);
                        collection.Add(pair.Key, value);
                    }
                    else
                    {
                        Debug.WriteLine($"Warning: No such command '{pair.Key}'");
                        collection.Add(pair.Key, CreateCommandMementoV2(pair.Value, null));
                    }
                }

                return collection;


                CommandElement.MementoV2 CreateCommandMementoV2(CommandElement.Memento mementoV1, Type parameterType)
                {
                    var mementoV2 = new CommandElement.MementoV2();
                    mementoV2.ShortCutKey = mementoV1.ShortCutKey;
                    mementoV2.TouchGesture = mementoV1.TouchGesture;
                    mementoV2.MouseGesture = mementoV1.MouseGesture;
                    mementoV2.IsShowMessage = mementoV1.IsShowMessage;

                    if (parameterType != null && !string.IsNullOrWhiteSpace(mementoV1.Parameter))
                    {
                        mementoV2.Parameter = (CommandParameter)Json.Deserialize(mementoV1.Parameter, parameterType);
                    }

                    return mementoV2;
                }
            }
        }

        #endregion

        #region Memento CommandCollection

        public CommandCollection CreateCommandCollectionMemento()
        {
            var collection = new CommandCollection();
            foreach (var item in _elements)
            {
                collection.Add(item.Key, item.Value.CreateMementoV2());
            }
            return collection;
        }

        public void RestoreCommandCollection(CommandCollection collection)
        {
            if (collection == null) return;

            this.ScriptManager.UpdateScriptCommands(isForce: false, isReplace: false);

            foreach (var pair in collection)
            {
                if (_elements.ContainsKey(pair.Key))
                {
                    _elements[pair.Key].RestoreV2(pair.Value);
                }
                else
                {
                    var cloneName = CommandNameSource.Parse(pair.Key);
                    if (cloneName.IsClone)
                    {
                        if (_elements.TryGetValue(cloneName.Name, out var source))
                        {
                            var command = CloneCommand(source, cloneName);
                            Debug.Assert(command.Name == pair.Key);
                            command.RestoreV2(pair.Value);
                        }
                        else
                        {
                            Debug.WriteLine($"Warning: No such clone source command '{cloneName.Name}'");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Warning: No such command '{pair.Key}'");
                    }
                }
            }

            Changed?.Invoke(this, new CommandChangedEventArgs(false));
        }

        #endregion
    }

    /// <summary>
    /// 設定V2ではこのデータを保存する
    /// </summary>
    public class CommandCollection : Dictionary<string, CommandElement.MementoV2>
    {
        public CommandCollection Clone()
        {
            var clone = new CommandCollection();
            foreach (var item in this)
            {
                clone.Add(item.Key, (CommandElement.MementoV2)item.Value.Clone());
            }
            return clone;
        }

    }
}
