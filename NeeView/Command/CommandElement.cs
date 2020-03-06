using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    [Flags]
    public enum CommandOption
    {
        None = 0,
        ByMenu = 0x0001,
    }


    [DataContract]
    public abstract class CommandElement
    {
        private string _menuText;

        public CommandElement(CommandType commandType)
        {
            CommandType = commandType;
        }

        public CommandType CommandType { get; private set; }

        // コマンドのグループ
        public string Group { get; set; }

        // コマンド表示名
        public string Text { get; set; }

        public string MenuText
        {
            get { return _menuText ?? Text; }
            set { _menuText = value; }
        }

        // コマンド説明
        public string Note { get; set; }


        // ショートカットキー
        public string ShortCutKey { get; set; }

        // タッチ
        public string TouchGesture { get; set; }

        // マウスジェスチャー
        public string MouseGesture { get; set; }

        // コマンド実行時の通知フラグ
        public bool IsShowMessage { get; set; }

        // ペアコマンド
        public CommandType PairPartner { get; set; }

        public CommandParameterSource ParameterSource { get; set; }

        public CommandParameter Parameter
        {
            get => ParameterSource?.Get();
            set => ParameterSource?.Set(value);
        }

        public CommandElement Share { get; private set; }

        public CommandElement SetShare(CommandElement share)
        {
            Share = share;
            ParameterSource = new CommandParameterSource(share.ParameterSource);
            return this;
        }


        // フラグバインディング 
        public virtual Binding CreateIsCheckedBinding()
        {
            return null;
        }

        // コマンド実行時表示デリゲート
        public virtual string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return Text;
        }

        public string ExecuteMessage(CommandOption option = CommandOption.None)
        {
            return ExecuteMessage(this.Parameter, option);
        }

        // コマンド実行可能判定
        public virtual bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return true;
        }

        public bool CanExecute(CommandOption option = CommandOption.None)
        {
            return CanExecute(this.Parameter, option);
        }

        // コマンド実行
        public abstract void Execute(CommandParameter param, CommandOption option = CommandOption.None);

        public void Execute(CommandOption option = CommandOption.None)
        {
            Execute(this.Parameter, option);
        }

        public CommandParameter CreateOverwriteCommandParameter(IDictionary<string, object> args)
        {
            return CreateOverwriteCommandParameter(this.Parameter, args);
        }

        public static CommandParameter CreateOverwriteCommandParameter(CommandParameter source, IDictionary<string, object> args)
        {
            if (source == null) return null;

            var clone = (CommandParameter)source.Clone();
            if (args == null) return clone;

            var type = source.GetType();

            foreach (var arg in args)
            {
                var property = type.GetProperty(arg.Key);
                if (property == null) throw new ArgumentException($"Property '{arg.Key}' is not supported.");

                try
                {
                    property.SetValue(clone, Convert.ChangeType(arg.Value, property.PropertyType));
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Property '{arg.Key}' value is invalid. {ex.Message}", ex);
                }
            }

            return clone;
        }

        // ショートカットキー を InputGestureのコレクションに変換
        public List<InputGesture> GetInputGestureCollection()
        {
            var list = new List<InputGesture>();
            if (!string.IsNullOrWhiteSpace(ShortCutKey))
            {
                foreach (var key in ShortCutKey.Split(','))
                {
                    InputGesture inputGesture = InputGestureConverter.ConvertFromString(key);
                    if (inputGesture != null)
                    {
                        list.Add(inputGesture);
                    }
                }
            }

            return list;
        }

        // タッチコレクション取得
        public List<TouchGesture> GetTouchGestureCollection()
        {
            var list = new List<TouchGesture>();
            if (!string.IsNullOrWhiteSpace(this.TouchGesture))
            {
                foreach (var key in this.TouchGesture.Split(','))
                {
                    if (Enum.TryParse(key, out TouchGesture gesture))
                    {
                        list.Add(gesture);
                    }
                }
            }

            return list;
        }

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public string ShortCutKey { get; set; }
            [DataMember]
            public string TouchGesture { get; set; }
            [DataMember]
            public string MouseGesture { get; set; }
            [DataMember]
            public bool IsShowMessage { get; set; }


            [DataMember(Order = 15, EmitDefaultValue = false)]
            public string Parameter { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public Memento Clone()
            {
                return (Memento)MemberwiseClone();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.ShortCutKey = ShortCutKey ?? "";
            memento.TouchGesture = TouchGesture ?? "";
            memento.MouseGesture = MouseGesture ?? "";
            memento.IsShowMessage = IsShowMessage;

            memento.Parameter = ParameterSource?.Store();

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            ShortCutKey = memento.ShortCutKey?.TrimStart(',');
            TouchGesture = memento.TouchGesture ?? this.TouchGesture; // compatible before ver.24
            MouseGesture = memento.MouseGesture;
            IsShowMessage = memento.IsShowMessage;

            ParameterSource?.Restore(memento.Parameter);
        }

        #endregion
    }
}


namespace NeeView
{

    public class NoneCommand : CommandElement
    {
        public NoneCommand() : base(CommandType.None)
        {
            this.Group = "(none)";
            this.Text = "(none)";
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return;
        }
    }


    public class LoadAsCommand : CommandElement
    {
        public LoadAsCommand() : base(CommandType.LoadAs)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandLoadAs;
            this.MenuText = Properties.Resources.CommandLoadAsMenu;
            this.Note = Properties.Resources.CommandLoadAsNote;
            this.ShortCutKey = "Ctrl+O";
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.LoadAs();
        }
    }

    public class ReLoadCommand : CommandElement
    {
        public ReLoadCommand() : base(CommandType.ReLoad)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandReLoad;
            this.Note = Properties.Resources.CommandReLoadNote;
            this.MouseGesture = "UD";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookHub.Current.CanReload();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHub.Current.RequestReLoad();
        }
    }

    public class UnloadCommand : CommandElement
    {
        public UnloadCommand() : base(CommandType.Unload)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandUnload;
            this.MenuText = Properties.Resources.CommandUnloadMenu;
            this.Note = Properties.Resources.CommandUnloadNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookHub.Current.CanUnload();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHub.Current.RequestUnload(true);
        }
    }

    public class OpenApplicationCommand : CommandElement
    {
        public OpenApplicationCommand() : base(CommandType.OpenApplication)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandOpenApplication;
            this.Note = Properties.Resources.CommandOpenApplicationNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.OpenApplication();
        }
    }

    public class OpenFilePlaceCommand : CommandElement
    {
        public OpenFilePlaceCommand() : base(CommandType.OpenFilePlace)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandOpenFilePlace;
            this.Note = Properties.Resources.CommandOpenFilePlaceNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.OpenFilePlace();
        }
    }

    public class ExportCommand : CommandElement
    {
        public ExportCommand() : base(CommandType.Export)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandExportImageDialog;
            this.MenuText = Properties.Resources.CommandExportImageDialogMenu;
            this.Note = Properties.Resources.CommandExportImageDialogNote;
            this.ShortCutKey = "Ctrl+S";
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ExportImageDialogCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookOperation.Current.CanExport();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.ExportDialog((ExportImageDialogCommandParameter)param);
        }
    }

    public class ExportImageCommand : CommandElement
    {
        public ExportImageCommand() : base(CommandType.ExportImage)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandExportImage;
            this.MenuText = Properties.Resources.CommandExportImageMenu;
            this.Note = Properties.Resources.CommandExportImageNote;
            this.ShortCutKey = "Shift+Ctrl+S";
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ExportImageCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookOperation.Current.CanExport();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.Export((ExportImageCommandParameter)param);
        }
    }

    public class PrintCommand : CommandElement
    {
        public PrintCommand() : base(CommandType.Print)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandPrint;
            this.MenuText = Properties.Resources.CommandPrintMenu;
            this.Note = Properties.Resources.CommandPrintNote;
            this.ShortCutKey = "Ctrl+P";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return ContentCanvas.Current.CanPrint();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            // TODO: Viewを直接呼び出さないようにする
            MainWindow.Current.Print();
        }
    }

    public class DeleteFileCommand : CommandElement
    {
        public DeleteFileCommand() : base(CommandType.DeleteFile)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandDeleteFile;
            this.MenuText = Properties.Resources.CommandDeleteFileMenu;
            this.Note = Properties.Resources.CommandDeleteFileNote;
            this.ShortCutKey = "Delete";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookOperation.Current.CanDeleteFile();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            var async = BookOperation.Current.DeleteFileAsync();
        }
    }

    public class DeleteBookCommand : CommandElement
    {
        public DeleteBookCommand() : base(CommandType.DeleteBook)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandDeleteBook;
            this.MenuText = Properties.Resources.CommandDeleteBookMenu;
            this.Note = Properties.Resources.CommandDeleteBookNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookOperation.Current.CanDeleteBook();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.DeleteBook();
        }
    }

    public class CopyFileCommand : CommandElement
    {
        public CopyFileCommand() : base(CommandType.CopyFile)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandCopyFile;
            this.MenuText = Properties.Resources.CommandCopyFileMenu;
            this.Note = Properties.Resources.CommandCopyFileNote;
            this.ShortCutKey = "Ctrl+C";
            this.IsShowMessage = true;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.CopyToClipboard();
        }
    }

    public class CopyImageCommand : CommandElement
    {
        public CopyImageCommand() : base(CommandType.CopyImage)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandCopyImage;
            this.MenuText = Properties.Resources.CommandCopyImageMenu;
            this.Note = Properties.Resources.CommandCopyImageNote;
            this.ShortCutKey = "Ctrl+Shift+C";
            this.IsShowMessage = true;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvas.Current.CanCopyImageToClipboard();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.CopyImageToClipboard();
        }
    }

    public class PasteCommand : CommandElement
    {
        public PasteCommand() : base(CommandType.Paste)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandPaste;
            this.MenuText = Properties.Resources.CommandPasteMenu;
            this.Note = Properties.Resources.CommandPasteNote;
            this.ShortCutKey = "Ctrl+V";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentDropManager.Current.CanLoadFromClipboard();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentDropManager.Current.LoadFromClipboard();
        }
    }

    public class ClearHistoryCommand : CommandElement
    {
        public ClearHistoryCommand() : base(CommandType.ClearHistory)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandClearHistory;
            this.Note = Properties.Resources.CommandClearHistoryNote;
            this.IsShowMessage = true;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHistoryCollection.Current.Clear();
        }
    }
    public class ClearHistoryInPlaceCommand : CommandElement
    {
        public ClearHistoryInPlaceCommand() : base(CommandType.ClearHistoryInPlace)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandClearHistoryInPlace;
            this.Note = Properties.Resources.CommandClearHistoryInPlaceNote;
            this.IsShowMessage = true;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookshelfFolderList.Current.Place != null;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.ClearHistory();
        }
    }

    public class ToggleStretchModeCommand : CommandElement
    {
        public ToggleStretchModeCommand() : base(CommandType.ToggleStretchMode)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchMode;
            this.Note = Properties.Resources.CommandToggleStretchModeNote;
            this.ShortCutKey = "LeftButton+WheelDown";
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter() { IsLoop = true });
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvas.Current.GetToggleStretchMode((ToggleStretchModeCommandParameter)param).ToAliasName();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.StretchMode = ContentCanvas.Current.GetToggleStretchMode((ToggleStretchModeCommandParameter)param);
        }
    }

    public class ToggleStretchModeReverseCommand : CommandElement
    {
        public ToggleStretchModeReverseCommand() : base(CommandType.ToggleStretchModeReverse)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchModeReverse;
            this.Note = Properties.Resources.CommandToggleStretchModeReverseNote;
            this.ShortCutKey = "LeftButton+WheelUp";
            this.IsShowMessage = true;

            // CommandType.ToggleStretchMode
            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter() { IsLoop = true });
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvas.Current.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)param).ToAliasName();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.StretchMode = ContentCanvas.Current.GetToggleStretchModeReverse((ToggleStretchModeCommandParameter)param);
        }
    }

    public class SetStretchModeNoneCommand : CommandElement
    {
        public SetStretchModeNoneCommand() : base(CommandType.SetStretchModeNone)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeNone;
            this.Note = Properties.Resources.CommandSetStretchModeNoneNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.None);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.StretchMode = PageStretchMode.None;
        }
    }

    public class SetStretchModeUniformCommand : CommandElement
    {
        public SetStretchModeUniformCommand() : base(CommandType.SetStretchModeUniform)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniform;
            this.Note = Properties.Resources.CommandSetStretchModeUniformNote;
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new StretchModeCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.Uniform);
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return this.Text + (ContentCanvas.Current.TestStretchMode(PageStretchMode.Uniform, ((StretchModeCommandParameter)param).IsToggle) ? "" : " OFF");
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.SetStretchMode(PageStretchMode.Uniform, ((StretchModeCommandParameter)param).IsToggle);
        }
    }

    public class SetStretchModeUniformToFillCommand : CommandElement
    {
        public SetStretchModeUniformToFillCommand() : base(CommandType.SetStretchModeUniformToFill)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniformToFill;
            this.Note = Properties.Resources.CommandSetStretchModeUniformToFillNote;
            this.IsShowMessage = true;

            // CommandType.SetStretchModeUniform
            this.ParameterSource = new CommandParameterSource(new StretchModeCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.UniformToFill);
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return this.Text + (ContentCanvas.Current.TestStretchMode(PageStretchMode.UniformToFill, ((StretchModeCommandParameter)param).IsToggle) ? "" : " OFF");
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.SetStretchMode(PageStretchMode.UniformToFill, ((StretchModeCommandParameter)param).IsToggle);
        }
    }

    public class SetStretchModeUniformToSizeCommand : CommandElement
    {
        public SetStretchModeUniformToSizeCommand() : base(CommandType.SetStretchModeUniformToSize)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniformToSize;
            this.Note = Properties.Resources.CommandSetStretchModeUniformToSizeNote;
            this.IsShowMessage = true;

            // SetStretchModeUniform
            this.ParameterSource = new CommandParameterSource(new StretchModeCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.UniformToSize);
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return this.Text + (ContentCanvas.Current.TestStretchMode(PageStretchMode.UniformToSize, ((StretchModeCommandParameter)param).IsToggle) ? "" : " OFF");
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.SetStretchMode(PageStretchMode.UniformToSize, ((StretchModeCommandParameter)param).IsToggle);
        }
    }


    public class SetStretchModeUniformToVerticalCommand : CommandElement
    {
        public SetStretchModeUniformToVerticalCommand() : base(CommandType.SetStretchModeUniformToVertical)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniformToVertical;
            this.Note = Properties.Resources.CommandSetStretchModeUniformToVerticalNote;
            this.IsShowMessage = true;

            // SetStretchModeUniform
            this.ParameterSource = new CommandParameterSource(new StretchModeCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.UniformToVertical);
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return this.Text + (ContentCanvas.Current.TestStretchMode(PageStretchMode.UniformToVertical, ((StretchModeCommandParameter)param).IsToggle) ? "" : " OFF");
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.SetStretchMode(PageStretchMode.UniformToVertical, ((StretchModeCommandParameter)param).IsToggle);
        }
    }

    public class SetStretchModeUniformToHorizontalCommand : CommandElement
    {
        public SetStretchModeUniformToHorizontalCommand() : base(CommandType.SetStretchModeUniformToHorizontal)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniformToHorizontal;
            this.Note = Properties.Resources.CommandSetStretchModeUniformToHorizontalNote;
            this.IsShowMessage = true;

            // SetStretchModeUniform
            this.ParameterSource = new CommandParameterSource(new StretchModeCommandParameter());
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.UniformToHorizontal);
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return this.Text + (ContentCanvas.Current.TestStretchMode(PageStretchMode.UniformToHorizontal, ((StretchModeCommandParameter)param).IsToggle) ? "" : " OFF");
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.SetStretchMode(PageStretchMode.UniformToHorizontal, ((StretchModeCommandParameter)param).IsToggle);
        }
    }


    public class ToggleStretchAllowEnlargeCommand : CommandElement
    {
        public ToggleStretchAllowEnlargeCommand() : base(CommandType.ToggleStretchAllowEnlarge)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchAllowEnlarge;
            this.Note = Properties.Resources.CommandToggleStretchAllowEnlargeNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.Current.AllowEnlarge)) { Source = ContentCanvas.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return this.Text + (ContentCanvas.Current.AllowEnlarge ? " OFF" : " ");
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.AllowEnlarge = !ContentCanvas.Current.AllowEnlarge;
        }
    }


    public class ToggleStretchAllowReduceCommand : CommandElement
    {
        public ToggleStretchAllowReduceCommand() : base(CommandType.ToggleStretchAllowReduce)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchAllowReduce;
            this.Note = Properties.Resources.CommandToggleStretchAllowReduceNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.Current.AllowReduce)) { Source = ContentCanvas.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return this.Text + (ContentCanvas.Current.AllowReduce ? " OFF" : "");
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.AllowReduce = !ContentCanvas.Current.AllowReduce;
        }
    }


    public class ToggleIsEnabledNearestNeighborCommand : CommandElement
    {
        public ToggleIsEnabledNearestNeighborCommand() : base(CommandType.ToggleIsEnabledNearestNeighbor)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleIsEnabledNearestNeighbor;
            this.MenuText = Properties.Resources.CommandToggleIsEnabledNearestNeighborMenu;
            this.Note = Properties.Resources.CommandToggleIsEnabledNearestNeighborNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.Current.IsEnabledNearestNeighbor)) { Source = ContentCanvas.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvas.Current.IsEnabledNearestNeighbor ? Properties.Resources.CommandToggleIsEnabledNearestNeighborOff : Properties.Resources.CommandToggleIsEnabledNearestNeighborOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.IsEnabledNearestNeighbor = !ContentCanvas.Current.IsEnabledNearestNeighbor;
        }
    }


    public class ToggleBackgroundCommand : CommandElement
    {
        public ToggleBackgroundCommand() : base(CommandType.ToggleBackground)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleBackground;
            this.Note = Properties.Resources.CommandToggleBackgroundNote;
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvasBrush.Current.Background.GetToggle().ToAliasName();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvasBrush.Current.Background = ContentCanvasBrush.Current.Background.GetToggle();
        }
    }


    public class SetBackgroundBlackCommand : CommandElement
    {
        public SetBackgroundBlackCommand() : base(CommandType.SetBackgroundBlack)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundBlack;
            this.Note = Properties.Resources.CommandSetBackgroundBlackNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundStyle.Black);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvasBrush.Current.Background = BackgroundStyle.Black;
        }
    }


    public class SetBackgroundWhiteCommand : CommandElement
    {
        public SetBackgroundWhiteCommand() : base(CommandType.SetBackgroundWhite)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundWhite;
            this.Note = Properties.Resources.CommandSetBackgroundWhiteNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundStyle.White);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvasBrush.Current.Background = BackgroundStyle.White;
        }
    }


    public class SetBackgroundAutoCommand : CommandElement
    {
        public SetBackgroundAutoCommand() : base(CommandType.SetBackgroundAuto)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundAuto;
            this.Note = Properties.Resources.CommandSetBackgroundAutoNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundStyle.Auto);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvasBrush.Current.Background = BackgroundStyle.Auto;
        }
    }


    public class SetBackgroundCheckCommand : CommandElement
    {
        public SetBackgroundCheckCommand() : base(CommandType.SetBackgroundCheck)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundCheck;
            this.Note = Properties.Resources.CommandSetBackgroundCheckNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundStyle.Check);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvasBrush.Current.Background = BackgroundStyle.Check;
        }
    }


    public class SetBackgroundCheckDarkCommand : CommandElement
    {
        public SetBackgroundCheckDarkCommand() : base(CommandType.SetBackgroundCheckDark)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundCheckDark;
            this.Note = Properties.Resources.CommandSetBackgroundCheckDarkNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundStyle.CheckDark);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvasBrush.Current.Background = BackgroundStyle.CheckDark;
        }
    }


    public class SetBackgroundCustomCommand : CommandElement
    {
        public SetBackgroundCustomCommand() : base(CommandType.SetBackgroundCustom)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundCustom;
            this.Note = Properties.Resources.CommandSetBackgroundCustomNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundStyle.Custom);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvasBrush.Current.Background = BackgroundStyle.Custom;
        }
    }


    public class ToggleTopmostCommand : CommandElement
    {
        public ToggleTopmostCommand() : base(CommandType.ToggleTopmost)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleTopmost;
            this.MenuText = Properties.Resources.CommandToggleTopmostMenu;
            this.Note = Properties.Resources.CommandToggleTopmostNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(WindowShape.IsTopmost)) { Source = WindowShape.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return WindowShape.Current.IsTopmost ? Properties.Resources.CommandToggleTopmostOff : Properties.Resources.CommandToggleTopmostOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            WindowShape.Current.ToggleTopmost();
        }
    }

    public class ToggleHideMenuCommand : CommandElement
    {
        public ToggleHideMenuCommand() : base(CommandType.ToggleHideMenu)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleHideMenu;
            this.MenuText = Properties.Resources.CommandToggleHideMenuMenu;
            this.Note = Properties.Resources.CommandToggleHideMenuNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MainWindowModel.Current.IsHideMenu)) { Source = MainWindowModel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return MainWindowModel.Current.IsHideMenu ? Properties.Resources.CommandToggleHideMenuOff : Properties.Resources.CommandToggleHideMenuOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.ToggleHideMenu();
        }
    }


    public class ToggleHidePageSliderCommand : CommandElement
    {
        public ToggleHidePageSliderCommand() : base(CommandType.ToggleHidePageSlider)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleHidePageSlider;
            this.MenuText = Properties.Resources.CommandToggleHidePageSliderMenu;
            this.Note = Properties.Resources.CommandToggleHidePageSliderNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MainWindowModel.Current.IsHidePageSlider)) { Source = MainWindowModel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return MainWindowModel.Current.IsHidePageSlider ? Properties.Resources.CommandToggleHidePageSliderOff : Properties.Resources.CommandToggleHidePageSliderOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.ToggleHidePageSlider();
        }
    }


    public class ToggleHidePanelCommand : CommandElement
    {
        public ToggleHidePanelCommand() : base(CommandType.ToggleHidePanel)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleHidePanel;
            this.MenuText = Properties.Resources.CommandToggleHidePanelMenu;
            this.Note = Properties.Resources.CommandToggleHidePanelNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MainWindowModel.Current.IsHidePanel)) { Source = MainWindowModel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return MainWindowModel.Current.IsHidePanel ? Properties.Resources.CommandToggleHidePanelOff : Properties.Resources.CommandToggleHidePanelOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.ToggleHidePanel();
        }
    }


    public class ToggleVisibleTitleBarCommand : CommandElement
    {
        public ToggleVisibleTitleBarCommand() : base(CommandType.ToggleVisibleTitleBar)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleVisibleTitleBar;
            this.MenuText = Properties.Resources.CommandToggleVisibleTitleBarMenu;
            this.Note = Properties.Resources.CommandToggleVisibleTitleBarNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(WindowShape.IsCaptionVisible)) { Source = WindowShape.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return WindowShape.Current.IsCaptionVisible ? Properties.Resources.CommandToggleVisibleTitleBarOff : Properties.Resources.CommandToggleVisibleTitleBarOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            WindowShape.Current.ToggleCaptionVisible();
        }
    }


    public class ToggleVisibleAddressBarCommand : CommandElement
    {
        public ToggleVisibleAddressBarCommand() : base(CommandType.ToggleVisibleAddressBar)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleVisibleAddressBar;
            this.MenuText = Properties.Resources.CommandToggleVisibleAddressBarMenu;
            this.Note = Properties.Resources.CommandToggleVisibleAddressBarNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MainWindowModel.Current.IsVisibleAddressBar)) { Source = MainWindowModel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return MainWindowModel.Current.IsVisibleAddressBar ? Properties.Resources.CommandToggleVisibleAddressBarOff : Properties.Resources.CommandToggleVisibleAddressBarOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.ToggleVisibleAddressBar();
        }
    }


    public class ToggleVisibleSideBarCommand : CommandElement
    {
        public ToggleVisibleSideBarCommand() : base(CommandType.ToggleVisibleSideBar)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleVisibleSideBar;
            this.MenuText = Properties.Resources.CommandToggleVisibleSideBarMenu;
            this.Note = Properties.Resources.CommandToggleVisibleSideBarNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsSideBarVisible)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SidePanel.Current.IsSideBarVisible ? Properties.Resources.CommandToggleVisibleSideBarOff : Properties.Resources.CommandToggleVisibleSideBarOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.IsSideBarVisible = !SidePanel.Current.IsSideBarVisible;
        }
    }


    public class ToggleVisibleFileInfoCommand : CommandElement
    {
        public ToggleVisibleFileInfoCommand() : base(CommandType.ToggleVisibleFileInfo)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleFileInfo;
            this.MenuText = Properties.Resources.CommandToggleVisibleFileInfoMenu;
            this.Note = Properties.Resources.CommandToggleVisibleFileInfoNote;
            this.ShortCutKey = "I";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsVisibleFileInfo)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SidePanel.Current.IsVisibleFileInfo ? Properties.Resources.CommandToggleVisibleFileInfoOff : Properties.Resources.CommandToggleVisibleFileInfoOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.ToggleVisibleFileInfo(option.HasFlag(CommandOption.ByMenu));
        }
    }


    public class ToggleVisibleEffectInfoCommand : CommandElement
    {
        public ToggleVisibleEffectInfoCommand() : base(CommandType.ToggleVisibleEffectInfo)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleEffectInfo;
            this.MenuText = Properties.Resources.CommandToggleVisibleEffectInfoMenu;
            this.Note = Properties.Resources.CommandToggleVisibleEffectInfoNote;
            this.ShortCutKey = "E";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsVisibleEffectInfo)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SidePanel.Current.IsVisibleEffectInfo ? Properties.Resources.CommandToggleVisibleEffectInfoOff : Properties.Resources.CommandToggleVisibleEffectInfoOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.ToggleVisibleEffectInfo(option.HasFlag(CommandOption.ByMenu));
        }
    }


    public class ToggleVisibleBookshelfCommand : CommandElement
    {
        public ToggleVisibleBookshelfCommand() : base(CommandType.ToggleVisibleBookshelf)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleBookshelf;
            this.MenuText = Properties.Resources.CommandToggleVisibleBookshelfMenu;
            this.Note = Properties.Resources.CommandToggleVisibleBookshelfNote;
            this.ShortCutKey = "B";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsVisibleFolderList)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SidePanel.Current.IsVisibleFolderList ? Properties.Resources.CommandToggleVisibleBookshelfOff : Properties.Resources.CommandToggleVisibleBookshelfOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.ToggleVisibleFolderList(option.HasFlag(CommandOption.ByMenu));
        }
    }


    public class ToggleVisibleBookmarkListCommand : CommandElement
    {
        public ToggleVisibleBookmarkListCommand() : base(CommandType.ToggleVisibleBookmarkList)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleBookmarkList;
            this.MenuText = Properties.Resources.CommandToggleVisibleBookmarkListMenu;
            this.Note = Properties.Resources.CommandToggleVisibleBookmarkListNote;
            this.ShortCutKey = "D";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsVisibleBookmarkList)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SidePanel.Current.IsVisibleBookmarkList ? Properties.Resources.CommandToggleVisibleBookmarkListOff : Properties.Resources.CommandToggleVisibleBookmarkListOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.ToggleVisibleBookmarkList(option.HasFlag(CommandOption.ByMenu));
        }
    }


    public class ToggleVisiblePagemarkListCommand : CommandElement
    {
        public ToggleVisiblePagemarkListCommand() : base(CommandType.ToggleVisiblePagemarkList)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisiblePagemarkList;
            this.MenuText = Properties.Resources.CommandToggleVisiblePagemarkListMenu;
            this.Note = Properties.Resources.CommandToggleVisiblePagemarkListNote;
            this.ShortCutKey = "M";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsVisiblePagemarkList)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SidePanel.Current.IsVisiblePagemarkList ? Properties.Resources.CommandToggleVisiblePagemarkListOff : Properties.Resources.CommandToggleVisiblePagemarkListOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.ToggleVisiblePagemarkList(option.HasFlag(CommandOption.ByMenu));
        }
    }

    public class ToggleVisibleHistoryListCommand : CommandElement
    {
        public ToggleVisibleHistoryListCommand() : base(CommandType.ToggleVisibleHistoryList)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleHistoryList;
            this.MenuText = Properties.Resources.CommandToggleVisibleHistoryListMenu;
            this.Note = Properties.Resources.CommandToggleVisibleHistoryListNote;
            this.ShortCutKey = "H";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsVisibleHistoryList)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SidePanel.Current.IsVisibleHistoryList ? Properties.Resources.CommandToggleVisibleHistoryListOff : Properties.Resources.CommandToggleVisibleHistoryListOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.ToggleVisibleHistoryList(option.HasFlag(CommandOption.ByMenu));
        }
    }


    public class ToggleVisiblePageListCommand : CommandElement
    {
        public ToggleVisiblePageListCommand() : base(CommandType.ToggleVisiblePageList)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisiblePageList;
            this.MenuText = Properties.Resources.CommandToggleVisiblePageListMenu;
            this.Note = Properties.Resources.CommandToggleVisiblePageListNote;
            this.ShortCutKey = "P";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsVisiblePageList)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SidePanel.Current.IsVisiblePageList ? Properties.Resources.CommandToggleVisiblePageListOff : Properties.Resources.CommandToggleVisiblePageListOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.ToggleVisiblePageList(option.HasFlag(CommandOption.ByMenu));
        }
    }


    public class ToggleVisibleFoldersTreeCommand : CommandElement
    {
        public ToggleVisibleFoldersTreeCommand() : base(CommandType.ToggleVisibleFoldersTree)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisibleFoldersTree;
            this.MenuText = Properties.Resources.CommandToggleVisibleFoldersTreeMenu;
            this.Note = Properties.Resources.CommandToggleVisibleFoldersTreeNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookshelfFolderList.Current.IsFolderTreeVisible)) { Source = BookshelfFolderList.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SidePanel.Current.IsVisibleFolderTree ? Properties.Resources.CommandToggleVisibleFoldersTreeOff : Properties.Resources.CommandToggleVisibleFoldersTreeOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.ToggleVisibleFolderTree(option.HasFlag(CommandOption.ByMenu));
        }
    }


    public class FocusFolderSearchBoxCommand : CommandElement
    {
        public FocusFolderSearchBoxCommand() : base(CommandType.FocusFolderSearchBox)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandFocusFolderSearchBox;
            this.MenuText = Properties.Resources.CommandFocusFolderSearchBoxMenu;
            this.Note = Properties.Resources.CommandFocusFolderSearchBoxNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.FocusFolderSearchBox(option.HasFlag(CommandOption.ByMenu));
        }
    }


    public class FocusBookmarkListCommand : CommandElement
    {
        public FocusBookmarkListCommand() : base(CommandType.FocusBookmarkList)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandFocusBookmarkList;
            this.MenuText = Properties.Resources.CommandFocusBookmarkListMenu;
            this.Note = Properties.Resources.CommandFocusBookmarkListNote;
            this.IsShowMessage = false;
        }
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.FocusBookmarkList(option.HasFlag(CommandOption.ByMenu));
        }
    }


    public class FocusMainViewCommand : CommandElement
    {
        public FocusMainViewCommand() : base(CommandType.FocusMainView)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandFocusMainView;
            this.MenuText = Properties.Resources.CommandFocusMainViewMenu;
            this.Note = Properties.Resources.CommandFocusMainViewNote;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new FocusMainViewCommandParameter() { NeedClosePanels = false });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.FocusMainView((FocusMainViewCommandParameter)param, option.HasFlag(CommandOption.ByMenu));
        }
    }


    public class TogglePageListPlacementCommand : CommandElement
    {
        public TogglePageListPlacementCommand() : base(CommandType.TogglePageListPlacement)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandTogglePageListPlacement;
            this.MenuText = Properties.Resources.CommandTogglePageListPlacementMenu;
            this.Note = Properties.Resources.CommandTogglePageListPlacementNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PageListPlacementService.Current.IsPlacedInBookshelf)) { Source = PageListPlacementService.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return PageListPlacementService.Current.IsPlacedInBookshelf ? Properties.Resources.CommandTogglePageListPlacementPanel : Properties.Resources.CommandTogglePageListPlacementBookshelf;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            PageListPlacementService.Current.IsPlacedInBookshelf = !PageListPlacementService.Current.IsPlacedInBookshelf;
        }
    }


    public class ToggleVisibleThumbnailListCommand : CommandElement
    {
        public ToggleVisibleThumbnailListCommand() : base(CommandType.ToggleVisibleThumbnailList)
        {
            this.Group = Properties.Resources.CommandGroupFilmStrip;
            this.Text = Properties.Resources.CommandToggleVisibleThumbnailList;
            this.MenuText = Properties.Resources.CommandToggleVisibleThumbnailListMenu;
            this.Note = Properties.Resources.CommandToggleVisibleThumbnailListNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ThumbnailList.Current.IsEnableThumbnailList)) { Source = ThumbnailList.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ThumbnailList.Current.IsVisible ? Properties.Resources.CommandToggleVisibleThumbnailListOff : Properties.Resources.CommandToggleVisibleThumbnailListOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ThumbnailList.Current.ToggleVisibleThumbnailList(option.HasFlag(CommandOption.ByMenu));
        }
    }


    public class ToggleHideThumbnailListCommand : CommandElement
    {
        public ToggleHideThumbnailListCommand() : base(CommandType.ToggleHideThumbnailList)
        {
            this.Group = Properties.Resources.CommandGroupFilmStrip;
            this.Text = Properties.Resources.CommandToggleHideThumbnailList;
            this.MenuText = Properties.Resources.CommandToggleHideThumbnailListMenu;
            this.Note = Properties.Resources.CommandToggleHideThumbnailListNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ThumbnailList.Current.IsHideThumbnailList)) { Source = ThumbnailList.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ThumbnailList.Current.IsHideThumbnailList ? Properties.Resources.CommandToggleHideThumbnailListOff : Properties.Resources.CommandToggleHideThumbnailListOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ThumbnailList.Current.IsEnableThumbnailList;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ThumbnailList.Current.ToggleHideThumbnailList();
        }
    }


    public class ToggleFullScreenCommand : CommandElement
    {
        public ToggleFullScreenCommand() : base(CommandType.ToggleFullScreen)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleFullScreen;
            this.MenuText = Properties.Resources.CommandToggleFullScreenMenu;
            this.Note = Properties.Resources.CommandToggleFullScreenNote;
            this.ShortCutKey = "F11";
            this.MouseGesture = "U";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(WindowShape.Current.IsFullScreen)) { Source = WindowShape.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return WindowShape.Current.IsFullScreen ? Properties.Resources.CommandToggleFullScreenOff : Properties.Resources.CommandToggleFullScreenOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            WindowShape.Current.ToggleFullScreen();
        }
    }


    public class SetFullScreenCommand : CommandElement
    {
        public SetFullScreenCommand() : base(CommandType.SetFullScreen)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandSetFullScreen;
            this.Note = Properties.Resources.CommandSetFullScreenNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            WindowShape.Current.SetFullScreen(true);
        }
    }


    public class CancelFullScreenCommand : CommandElement
    {
        public CancelFullScreenCommand() : base(CommandType.CancelFullScreen)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandCancelFullScreen;
            this.Note = Properties.Resources.CommandCancelFullScreenNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            WindowShape.Current.SetFullScreen(false);
        }
    }


    public class ToggleWindowMinimizeCommand : CommandElement
    {
        public ToggleWindowMinimizeCommand() : base(CommandType.ToggleWindowMinimize)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleWindowMinimize;
            this.MenuText = Properties.Resources.CommandToggleWindowMinimizeMenu;
            this.Note = Properties.Resources.CommandToggleWindowMinimizeNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindow.Current.MainWindow_Minimize();
        }
    }


    public class ToggleWindowMaximizeCommand : CommandElement
    {
        public ToggleWindowMaximizeCommand() : base(CommandType.ToggleWindowMaximize)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleWindowMaximize;
            this.MenuText = Properties.Resources.CommandToggleWindowMaximizeMenu;
            this.Note = Properties.Resources.CommandToggleWindowMaximizeNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindow.Current.MainWindow_Maximize();
        }
    }


    public class ShowHiddenPanelsCommand : CommandElement
    {
        public ShowHiddenPanelsCommand() : base(CommandType.ShowHiddenPanels)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandShowHiddenPanels;
            this.MenuText = Properties.Resources.CommandShowHiddenPanelsMenu;
            this.Note = Properties.Resources.CommandShowHiddenPanelsNote;
            this.TouchGesture = "TouchCenter";
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.EnterVisibleLocked();
        }
    }


    public class ToggleSlideShowCommand : CommandElement
    {
        public ToggleSlideShowCommand() : base(CommandType.ToggleSlideShow)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleSlideShow;
            this.MenuText = Properties.Resources.CommandToggleSlideShowMenu;
            this.Note = Properties.Resources.CommandToggleSlideShowNote;
            this.ShortCutKey = "F5";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SlideShow.IsPlayingSlideShow)) { Source = SlideShow.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SlideShow.Current.IsPlayingSlideShow ? Properties.Resources.CommandToggleSlideShowOff : Properties.Resources.CommandToggleSlideShowOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SlideShow.Current.ToggleSlideShow();
        }
    }


    public class ViewScrollUpCommand : CommandElement
    {
        public ViewScrollUpCommand() : base(CommandType.ViewScrollUp)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollUp;
            this.Note = Properties.Resources.CommandViewScrollUpNote;
            this.IsShowMessage = false;
            
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter() { Scroll = 25, AllowCrossScroll = true });
        }
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.ScrollUp((ViewScrollCommandParameter)param);
        }
    }


    public class ViewScrollDownCommand : CommandElement
    {
        public ViewScrollDownCommand() : base(CommandType.ViewScrollDown)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollDown;
            this.Note = Properties.Resources.CommandViewScrollDownNote;
            this.IsShowMessage = false;
            
            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter() { Scroll = 25, AllowCrossScroll = true });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.ScrollDown((ViewScrollCommandParameter)param);
        }
    }


    public class ViewScrollLeftCommand : CommandElement
    {
        public ViewScrollLeftCommand() : base(CommandType.ViewScrollLeft)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollLeft;
            this.Note = Properties.Resources.CommandViewScrollLeftNote;
            this.IsShowMessage = false;
            
            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter() { Scroll = 25, AllowCrossScroll = true });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.ScrollLeft((ViewScrollCommandParameter)param);
        }
    }


    public class ViewScrollRightCommand : CommandElement
    {
        public ViewScrollRightCommand() : base(CommandType.ViewScrollRight)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollRight;
            this.Note = Properties.Resources.CommandViewScrollRightNote;
            this.IsShowMessage = false;

            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter() { Scroll = 25, AllowCrossScroll = true });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.ScrollRight((ViewScrollCommandParameter)param);
        }
    }

    public class ViewScaleUpCommand : CommandElement
    {
        public ViewScaleUpCommand() : base(CommandType.ViewScaleUp)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScaleUp;
            this.Note = Properties.Resources.CommandViewScaleUpNote;
            this.ShortCutKey = "RightButton+WheelUp";
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter() { Scale = 20, IsSnapDefaultScale = true });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            var parameter = (ViewScaleCommandParameter)param;
            DragTransformControl.Current.ScaleUp(parameter.Scale / 100.0, parameter.IsSnapDefaultScale, ContentCanvas.Current.MainContentScale);
        }
    }

    public class ViewScaleDownCommand : CommandElement
    {
        public ViewScaleDownCommand() : base(CommandType.ViewScaleDown)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScaleDown;
            this.Note = Properties.Resources.CommandViewScaleDownNote;
            this.ShortCutKey = "RightButton+WheelDown";
            this.IsShowMessage = false;

            // ViewScaleUp
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter() { Scale = 20, IsSnapDefaultScale = true });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            var parameter = (ViewScaleCommandParameter)param;
            DragTransformControl.Current.ScaleDown(parameter.Scale / 100.0, parameter.IsSnapDefaultScale, ContentCanvas.Current.MainContentScale);
        }
    }


    public class ViewRotateLeftCommand : CommandElement
    {
        public ViewRotateLeftCommand() : base(CommandType.ViewRotateLeft)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewRotateLeft;
            this.Note = Properties.Resources.CommandViewRotateLeftNote;
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewRotateCommandParameter() { Angle = 45 });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.ViewRotateLeft((ViewRotateCommandParameter)param);
        }
    }


    public class ViewRotateRightCommand : CommandElement
    {
        public ViewRotateRightCommand() : base(CommandType.ViewRotateRight)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewRotateRight;
            this.Note = Properties.Resources.CommandViewRotateRightNote;
            this.IsShowMessage = false;
            
            // ViewRotateLeft
            this.ParameterSource = new CommandParameterSource(new ViewRotateCommandParameter() { Angle = 45 });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.ViewRotateRight((ViewRotateCommandParameter)param);
        }
    }


    public class ToggleIsAutoRotateLeftCommand : CommandElement
    {
        public ToggleIsAutoRotateLeftCommand() : base(CommandType.ToggleIsAutoRotateLeft)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleIsAutoRotateLeft;
            this.MenuText = Properties.Resources.CommandToggleIsAutoRotateLeftMenu;
            this.Note = Properties.Resources.CommandToggleIsAutoRotateLeftNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.IsAutoRotateLeft)) { Source = ContentCanvas.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvas.Current.IsAutoRotateLeft ? Properties.Resources.CommandToggleIsAutoRotateLeftOff : Properties.Resources.CommandToggleIsAutoRotateLeftOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.IsAutoRotateLeft = !ContentCanvas.Current.IsAutoRotateLeft;
        }
    }


    public class ToggleIsAutoRotateRightCommand : CommandElement
    {
        public ToggleIsAutoRotateRightCommand() : base(CommandType.ToggleIsAutoRotateRight)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleIsAutoRotateRight;
            this.MenuText = Properties.Resources.CommandToggleIsAutoRotateRightMenu;
            this.Note = Properties.Resources.CommandToggleIsAutoRotateRightNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.IsAutoRotateRight)) { Source = ContentCanvas.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvas.Current.IsAutoRotateRight ? Properties.Resources.CommandToggleIsAutoRotateRightOff : Properties.Resources.CommandToggleIsAutoRotateRightOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.IsAutoRotateRight = !ContentCanvas.Current.IsAutoRotateRight;
        }
    }


    public class ToggleViewFlipHorizontalCommand : CommandElement
    {
        public ToggleViewFlipHorizontalCommand() : base(CommandType.ToggleViewFlipHorizontal)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleViewFlipHorizontal;
            this.Note = Properties.Resources.CommandToggleViewFlipHorizontalNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(DragTransform.IsFlipHorizontal)) { Source = DragTransform.Current, Mode = BindingMode.OneWay };
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.ToggleFlipHorizontal();
        }
    }


    public class ViewFlipHorizontalOnCommand : CommandElement
    {
        public ViewFlipHorizontalOnCommand() : base(CommandType.ViewFlipHorizontalOn)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewFlipHorizontalOn;
            this.Note = Properties.Resources.CommandViewFlipHorizontalOnNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.FlipHorizontal(true);
        }
    }


    public class ViewFlipHorizontalOffCommand : CommandElement
    {
        public ViewFlipHorizontalOffCommand() : base(CommandType.ViewFlipHorizontalOff)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewFlipHorizontalOff;
            this.Note = Properties.Resources.CommandViewFlipHorizontalOffNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.FlipHorizontal(false);
        }
    }


    public class ToggleViewFlipVerticalCommand : CommandElement
    {
        public ToggleViewFlipVerticalCommand() : base(CommandType.ToggleViewFlipVertical)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleViewFlipVertical;
            this.Note = Properties.Resources.CommandToggleViewFlipVerticalNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(DragTransform.IsFlipVertical)) { Source = DragTransform.Current, Mode = BindingMode.OneWay };
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.ToggleFlipVertical();
        }
    }


    public class ViewFlipVerticalOnCommand : CommandElement
    {
        public ViewFlipVerticalOnCommand() : base(CommandType.ViewFlipVerticalOn)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewFlipVerticalOn;
            this.Note = Properties.Resources.CommandViewFlipVerticalOnNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.FlipVertical(true);
        }
    }


    public class ViewFlipVerticalOffCommand : CommandElement
    {
        public ViewFlipVerticalOffCommand() : base(CommandType.ViewFlipVerticalOff)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewFlipVerticalOff;
            this.Note = Properties.Resources.CommandViewFlipVerticalOffNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.FlipVertical(false);
        }
    }


    public class ViewResetCommand : CommandElement
    {
        public ViewResetCommand() : base(CommandType.ViewReset)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewReset;
            this.Note = Properties.Resources.CommandViewResetNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.ResetTransform(true);
        }
    }


    public class PrevPageCommand : CommandElement
    {
        public PrevPageCommand() : base(CommandType.PrevPage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevPage;
            this.Note = Properties.Resources.CommandPrevPageNote;
            this.ShortCutKey = "Right,RightClick";
            this.TouchGesture = "TouchR1,TouchR2";
            this.MouseGesture = "R";
            this.IsShowMessage = false;
            this.PairPartner = CommandType.NextPage;

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.PrevPage();
        }
    }


    public class NextPageCommand : CommandElement
    {
        public NextPageCommand() : base(CommandType.NextPage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextPage;
            this.Note = Properties.Resources.CommandNextPageNote;
            this.ShortCutKey = "Left,LeftClick";
            this.TouchGesture = "TouchL1,TouchL2";
            this.MouseGesture = "L";
            this.IsShowMessage = false;
            this.PairPartner = CommandType.PrevPage;

            // PrevPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.NextPage();
        }
    }


    public class PrevOnePageCommand : CommandElement
    {
        public PrevOnePageCommand() : base(CommandType.PrevOnePage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevOnePage;
            this.Note = Properties.Resources.CommandPrevOnePageNote;
            this.MouseGesture = "LR";
            this.IsShowMessage = false;
            this.PairPartner = CommandType.NextOnePage;

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.PrevOnePage();
        }
    }


    public class NextOnePageCommand : CommandElement
    {
        public NextOnePageCommand() : base(CommandType.NextOnePage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextOnePage;
            this.Note = Properties.Resources.CommandNextOnePageNote;
            this.MouseGesture = "RL";
            this.IsShowMessage = false;
            this.PairPartner = CommandType.PrevOnePage;

            // PrevOnePage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.NextOnePage();
        }
    }


    public class PrevScrollPageCommand : CommandElement
    {
        public PrevScrollPageCommand() : base(CommandType.PrevScrollPage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevScrollPage;
            this.Note = Properties.Resources.CommandPrevScrollPageNote;
            this.ShortCutKey = "WheelUp";
            this.IsShowMessage = false;
            this.PairPartner = CommandType.NextScrollPage;

            this.ParameterSource = new CommandParameterSource(new ScrollPageCommandParameter() { IsNScroll = true, IsAnimation = true, Margin = 50, Scroll = 100 });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.PrevScrollPage();
        }
    }


    public class NextScrollPageCommand : CommandElement
    {
        public NextScrollPageCommand() : base(CommandType.NextScrollPage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextScrollPage;
            this.Note = Properties.Resources.CommandNextScrollPageNote;
            this.ShortCutKey = "WheelDown";
            this.IsShowMessage = false;
            this.PairPartner = CommandType.PrevScrollPage;

            // PrevScrollPage
            this.ParameterSource = new CommandParameterSource(new ScrollPageCommandParameter() { IsNScroll = true, IsAnimation = true, Margin = 50, Scroll = 100 });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.NextScrollPage();
        }
    }


    public class JumpPageCommand : CommandElement
    {
        public JumpPageCommand() : base(CommandType.JumpPage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandJumpPage;
            this.Note = Properties.Resources.CommandJumpPageNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.JumpPage();
        }
    }


    public class PrevSizePageCommand : CommandElement
    {
        public PrevSizePageCommand() : base(CommandType.PrevSizePage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevSizePage;
            this.Note = Properties.Resources.CommandPrevSizePageNote;
            this.IsShowMessage = false;
            this.PairPartner = CommandType.NextSizePage;

            this.ParameterSource = new CommandParameterSource(new MoveSizePageCommandParameter() { Size = 10 });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.PrevSizePage(((MoveSizePageCommandParameter)param).Size);
        }
    }


    public class NextSizePageCommand : CommandElement
    {
        public NextSizePageCommand() : base(CommandType.NextSizePage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextSizePage;
            this.Note = Properties.Resources.CommandNextSizePageNote;
            this.IsShowMessage = false;
            this.PairPartner = CommandType.PrevSizePage;

            // PrevSizePage
            this.ParameterSource = new CommandParameterSource(new MoveSizePageCommandParameter() { Size = 10 });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.NextSizePage(((MoveSizePageCommandParameter)param).Size);
        }
    }


    public class PrevFolderPageCommand : CommandElement
    {
        public PrevFolderPageCommand() : base(CommandType.PrevFolderPage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevFolderPage;
            this.Note = Properties.Resources.CommandPrevFolderPageNote;
            this.IsShowMessage = true;
            this.PairPartner = CommandType.NextFolderPage;

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return null;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.PrevFolderPage(this.IsShowMessage);
        }
    }


    public class NextFolderPageCommand : CommandElement
    {
        public NextFolderPageCommand() : base(CommandType.NextFolderPage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextFolderPage;
            this.Note = Properties.Resources.CommandNextFolderPageNote;
            this.IsShowMessage = true;
            this.PairPartner = CommandType.PrevFolderPage;

            // PrevFolderPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return null;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.NextFolderPage(this.IsShowMessage);
        }
    }


    public class FirstPageCommand : CommandElement
    {
        public FirstPageCommand() : base(CommandType.FirstPage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandFirstPage;
            this.Note = Properties.Resources.CommandFirstPageNote;
            this.ShortCutKey = "Ctrl+Right";
            this.MouseGesture = "UR";
            this.IsShowMessage = true;
            this.PairPartner = CommandType.LastPage;

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.FirstPage();
        }
    }


    public class LastPageCommand : CommandElement
    {
        public LastPageCommand() : base(CommandType.LastPage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandLastPage;
            this.Note = Properties.Resources.CommandLastPageNote;
            this.ShortCutKey = "Ctrl+Left";
            this.MouseGesture = "UL";
            this.IsShowMessage = true;
            this.PairPartner = CommandType.FirstPage;

            // FirstPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.LastPage();
        }
    }


    public class PrevFolderCommand : CommandElement
    {
        public PrevFolderCommand() : base(CommandType.PrevFolder)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevFolder;
            this.Note = Properties.Resources.CommandPrevFolderNote;
            this.ShortCutKey = "Up";
            this.MouseGesture = "LU";
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            var async = BookshelfFolderList.Current.PrevFolder();
        }
    }


    public class NextFolderCommand : CommandElement
    {
        public NextFolderCommand() : base(CommandType.NextFolder)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextFolder;
            this.Note = Properties.Resources.CommandNextFolderNote;
            this.ShortCutKey = "Down";
            this.MouseGesture = "LD";
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            var async = BookshelfFolderList.Current.NextFolder();
        }
    }


    public class PrevHistoryCommand : CommandElement
    {
        public PrevHistoryCommand() : base(CommandType.PrevHistory)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevHistory;
            this.Note = Properties.Resources.CommandPrevHistoryNote;
            this.ShortCutKey = "Back";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookHistoryCommand.Current.CanPrevHistory();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHistoryCommand.Current.PrevHistory();
        }
    }


    public class NextHistoryCommand : CommandElement
    {
        public NextHistoryCommand() : base(CommandType.NextHistory)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextHistory;
            this.Note = Properties.Resources.CommandNextHistoryNote;
            this.ShortCutKey = "Shift+Back";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookHistoryCommand.Current.CanNextHistory();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHistoryCommand.Current.NextHistory();
        }
    }


    public class PrevBookHistoryCommand : CommandElement
    {
        public PrevBookHistoryCommand() : base(CommandType.PrevBookHistory)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevBookHistory;
            this.Note = Properties.Resources.CommandPrevBookHistoryNote;
            this.ShortCutKey = "Alt+Left";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookHubHistory.Current.CanMoveToPrevious();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHubHistory.Current.MoveToPrevious();
        }
    }


    public class NextBookHistoryCommand : CommandElement
    {
        public NextBookHistoryCommand() : base(CommandType.NextBookHistory)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextBookHistory;
            this.Note = Properties.Resources.CommandNextBookHistoryNote;
            this.ShortCutKey = "Alt+Right";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookHubHistory.Current.CanMoveToNext();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHubHistory.Current.MoveToNext();
        }
    }


    public class MoveToParentBookCommand : CommandElement
    {
        public MoveToParentBookCommand() : base(CommandType.MoveToParentBook)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandMoveToParentBook;
            this.Note = Properties.Resources.CommandMoveToParentBookNote;
            this.ShortCutKey = "Alt+Up";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookHub.Current.CanLoadParent();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHub.Current.RequestLoadParent();
        }
    }


    public class MoveToChildBookCommand : CommandElement
    {
        public MoveToChildBookCommand() : base(CommandType.MoveToChildBook)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandMoveToChildBook;
            this.Note = Properties.Resources.CommandMoveToChildBookNote;
            this.ShortCutKey = "Alt+Down";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.CanMoveToChildBook();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.MoveToChildBook();
        }
    }


    public class ToggleMediaPlayCommand : CommandElement
    {
        public ToggleMediaPlayCommand() : base(CommandType.ToggleMediaPlay)
        {
            this.Group = Properties.Resources.CommandGroupVideo;
            this.Text = Properties.Resources.CommandToggleMediaPlay;
            this.Note = Properties.Resources.CommandToggleMediaPlayNote;
        }
        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.IsMediaPlaying() ? Properties.Resources.WordStop : Properties.Resources.WordPlay;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.Book != null && BookOperation.Current.Book.IsMedia;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.ToggleMediaPlay();
        }
    }


    public class ToggleFolderOrderCommand : CommandElement
    {
        public ToggleFolderOrderCommand() : base(CommandType.ToggleFolderOrder)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandToggleFolderOrder;
            this.Note = Properties.Resources.CommandToggleFolderOrderNote;
            this.IsShowMessage = true;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.ToggleFolderOrder();
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookshelfFolderList.Current.GetNextFolderOrder().ToAliasName();
        }
    }


    public class SetFolderOrderByFileNameACommand : CommandElement
    {
        public SetFolderOrderByFileNameACommand() : base(CommandType.SetFolderOrderByFileNameA)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByFileNameA;
            this.Note = Properties.Resources.CommandSetFolderOrderByFileNameANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileName);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileName);
        }
    }


    public class SetFolderOrderByFileNameDCommand : CommandElement
    {
        public SetFolderOrderByFileNameDCommand() : base(CommandType.SetFolderOrderByFileNameD)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByFileNameD;
            this.Note = Properties.Resources.CommandSetFolderOrderByFileNameDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileNameDescending);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileNameDescending);
        }
    }


    public class SetFolderOrderByPathACommand : CommandElement
    {
        public SetFolderOrderByPathACommand() : base(CommandType.SetFolderOrderByPathA)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByPathA;
            this.Note = Properties.Resources.CommandSetFolderOrderByPathANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.Path);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Path);
        }
    }


    public class SetFolderOrderByPathDCommand : CommandElement
    {
        public SetFolderOrderByPathDCommand() : base(CommandType.SetFolderOrderByPathD)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByPathD;
            this.Note = Properties.Resources.CommandSetFolderOrderByPathDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.PathDescending);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.PathDescending);
        }
    }


    public class SetFolderOrderByFileTypeACommand : CommandElement
    {
        public SetFolderOrderByFileTypeACommand() : base(CommandType.SetFolderOrderByFileTypeA)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByFileTypeA;
            this.Note = Properties.Resources.CommandSetFolderOrderByFileTypeANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileType);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileType);
        }
    }


    public class SetFolderOrderByFileTypeDCommand : CommandElement
    {
        public SetFolderOrderByFileTypeDCommand() : base(CommandType.SetFolderOrderByFileTypeD)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByFileTypeD;
            this.Note = Properties.Resources.CommandSetFolderOrderByFileTypeDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileTypeDescending);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileTypeDescending);
        }
    }


    public class SetFolderOrderByTimeStampACommand : CommandElement
    {
        public SetFolderOrderByTimeStampACommand() : base(CommandType.SetFolderOrderByTimeStampA)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByTimeStampA;
            this.Note = Properties.Resources.CommandSetFolderOrderByTimeStampANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.TimeStamp);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.TimeStamp);
        }
    }


    public class SetFolderOrderByTimeStampDCommand : CommandElement
    {
        public SetFolderOrderByTimeStampDCommand() : base(CommandType.SetFolderOrderByTimeStampD)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByTimeStampD;
            this.Note = Properties.Resources.CommandSetFolderOrderByTimeStampDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.TimeStampDescending);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.TimeStampDescending);
        }
    }


    public class SetFolderOrderByEntryTimeACommand : CommandElement
    {
        public SetFolderOrderByEntryTimeACommand() : base(CommandType.SetFolderOrderByEntryTimeA)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByEntryTimeA;
            this.Note = Properties.Resources.CommandSetFolderOrderByEntryTimeANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.EntryTime);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.EntryTime);
        }
    }


    public class SetFolderOrderByEntryTimeDCommand : CommandElement
    {
        public SetFolderOrderByEntryTimeDCommand() : base(CommandType.SetFolderOrderByEntryTimeD)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByEntryTimeD;
            this.Note = Properties.Resources.CommandSetFolderOrderByEntryTimeDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.EntryTimeDescending);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.EntryTimeDescending);
        }
    }
    public class SetFolderOrderBySizeACommand : CommandElement
    {
        public SetFolderOrderBySizeACommand() : base(CommandType.SetFolderOrderBySizeA)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderBySizeA;
            this.Note = Properties.Resources.CommandSetFolderOrderBySizeANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.Size);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Size);
        }
    }


    public class SetFolderOrderBySizeDCommand : CommandElement
    {
        public SetFolderOrderBySizeDCommand() : base(CommandType.SetFolderOrderBySizeD)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderBySizeD;
            this.Note = Properties.Resources.CommandSetFolderOrderBySizeDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.SizeDescending);
        }
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.SizeDescending);
        }
    }


    public class SetFolderOrderByRandomCommand : CommandElement
    {
        public SetFolderOrderByRandomCommand() : base(CommandType.SetFolderOrderByRandom)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByRandom;
            this.Note = Properties.Resources.CommandSetFolderOrderByRandomNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.Random);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Random);
        }
    }


    public class TogglePageModeCommand : CommandElement
    {
        public TogglePageModeCommand() : base(CommandType.TogglePageMode)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandTogglePageMode;
            this.Note = Properties.Resources.CommandTogglePageModeNote;
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.LatestSetting.PageMode.GetToggle().ToAliasName();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.TogglePageMode();
        }
    }


    public class SetPageMode1Command : CommandElement
    {
        public SetPageMode1Command() : base(CommandType.SetPageMode1)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetPageMode1;
            this.Note = Properties.Resources.CommandSetPageMode1Note;
            this.ShortCutKey = "Ctrl+1";
            this.MouseGesture = "RU";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageMode(PageMode.SinglePage);
        }
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetPageMode(PageMode.SinglePage);
        }
    }


    public class SetPageMode2Command : CommandElement
    {
        public SetPageMode2Command() : base(CommandType.SetPageMode2)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetPageMode2;
            this.Note = Properties.Resources.CommandSetPageMode2Note;
            this.ShortCutKey = "Ctrl+2";
            this.MouseGesture = "RD";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageMode(PageMode.WidePage);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetPageMode(PageMode.WidePage);
        }
    }


    public class ToggleBookReadOrderCommand : CommandElement
    {
        public ToggleBookReadOrderCommand() : base(CommandType.ToggleBookReadOrder)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleBookReadOrder;
            this.Note = Properties.Resources.CommandToggleBookReadOrderNote;
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.LatestSetting.BookReadOrder.GetToggle().ToAliasName();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.ToggleBookReadOrder();
        }
    }


    public class SetBookReadOrderRightCommand : CommandElement
    {
        public SetBookReadOrderRightCommand() : base(CommandType.SetBookReadOrderRight)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetBookReadOrderRight;
            this.Note = Properties.Resources.CommandSetBookReadOrderRightNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BookReadOrder(PageReadOrder.RightToLeft);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetBookReadOrder(PageReadOrder.RightToLeft);
        }
    }


    public class SetBookReadOrderLeftCommand : CommandElement
    {
        public SetBookReadOrderLeftCommand() : base(CommandType.SetBookReadOrderLeft)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetBookReadOrderLeft;
            this.Note = Properties.Resources.CommandSetBookReadOrderLeftNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BookReadOrder(PageReadOrder.LeftToRight);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetBookReadOrder(PageReadOrder.LeftToRight);
        }
    }


    public class ToggleIsSupportedDividePageCommand : CommandElement
    {
        public ToggleIsSupportedDividePageCommand() : base(CommandType.ToggleIsSupportedDividePage)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedDividePage;
            this.Note = Properties.Resources.CommandToggleIsSupportedDividePageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedDividePage));
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedDividePage ? Properties.Resources.CommandToggleIsSupportedDividePageOff : Properties.Resources.CommandToggleIsSupportedDividePageOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.CanPageModeSubSetting(PageMode.SinglePage);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.ToggleIsSupportedDividePage();
        }
    }


    public class ToggleIsSupportedWidePageCommand : CommandElement
    {
        public ToggleIsSupportedWidePageCommand() : base(CommandType.ToggleIsSupportedWidePage)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedWidePage;
            this.Note = Properties.Resources.CommandToggleIsSupportedWidePageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedWidePage));
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedWidePage ? Properties.Resources.CommandToggleIsSupportedWidePageOff : Properties.Resources.CommandToggleIsSupportedWidePageOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.CanPageModeSubSetting(PageMode.WidePage);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.ToggleIsSupportedWidePage();
        }
    }


    public class ToggleIsSupportedSingleFirstPageCommand : CommandElement
    {
        public ToggleIsSupportedSingleFirstPageCommand() : base(CommandType.ToggleIsSupportedSingleFirstPage)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedSingleFirstPage;
            this.Note = Properties.Resources.CommandToggleIsSupportedSingleFirstPageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedSingleFirstPage));
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedSingleFirstPage ? Properties.Resources.CommandToggleIsSupportedSingleFirstPageOff : Properties.Resources.CommandToggleIsSupportedSingleFirstPageOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.CanPageModeSubSetting(PageMode.WidePage);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.ToggleIsSupportedSingleFirstPage();
        }
    }


    public class ToggleIsSupportedSingleLastPageCommand : CommandElement
    {
        public ToggleIsSupportedSingleLastPageCommand() : base(CommandType.ToggleIsSupportedSingleLastPage)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsSupportedSingleLastPage;
            this.Note = Properties.Resources.CommandToggleIsSupportedSingleLastPageNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsSupportedSingleLastPage));
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.LatestSetting.IsSupportedSingleLastPage ? Properties.Resources.CommandToggleIsSupportedSingleLastPageOff : Properties.Resources.CommandToggleIsSupportedSingleLastPageOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.CanPageModeSubSetting(PageMode.WidePage);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.ToggleIsSupportedSingleLastPage();
        }
    }


    public class ToggleIsRecursiveFolderCommand : CommandElement
    {
        public ToggleIsRecursiveFolderCommand() : base(CommandType.ToggleIsRecursiveFolder)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandToggleIsRecursiveFolder;
            this.Note = Properties.Resources.CommandToggleIsRecursiveFolderNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettingPresenter.Current.LatestSetting.IsRecursiveFolder));
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.LatestSetting.IsRecursiveFolder ? Properties.Resources.CommandToggleIsRecursiveFolderOff : Properties.Resources.CommandToggleIsRecursiveFolderOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.ToggleIsRecursiveFolder();
        }
    }


    public class ToggleSortModeCommand : CommandElement
    {
        public ToggleSortModeCommand() : base(CommandType.ToggleSortMode)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandToggleSortMode;
            this.Note = Properties.Resources.CommandToggleSortModeNote;
            this.IsShowMessage = true;
        }
        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookSettingPresenter.Current.LatestSetting.SortMode.GetToggle().ToAliasName();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.ToggleSortMode();
        }
    }


    public class SetSortModeFileNameCommand : CommandElement
    {
        public SetSortModeFileNameCommand() : base(CommandType.SetSortModeFileName)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeFileName;
            this.Note = Properties.Resources.CommandSetSortModeFileNameNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.FileName);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.FileName);
        }
    }


    public class SetSortModeFileNameDescendingCommand : CommandElement
    {
        public SetSortModeFileNameDescendingCommand() : base(CommandType.SetSortModeFileNameDescending)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeFileNameDescending;
            this.Note = Properties.Resources.CommandSetSortModeFileNameDescendingNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.FileNameDescending);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.FileNameDescending);
        }
    }


    public class SetSortModeTimeStampCommand : CommandElement
    {
        public SetSortModeTimeStampCommand() : base(CommandType.SetSortModeTimeStamp)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeTimeStamp;
            this.Note = Properties.Resources.CommandSetSortModeTimeStampNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.TimeStamp);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.TimeStamp);
        }
    }


    public class SetSortModeTimeStampDescendingCommand : CommandElement
    {
        public SetSortModeTimeStampDescendingCommand() : base(CommandType.SetSortModeTimeStampDescending)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeTimeStampDescending;
            this.Note = Properties.Resources.CommandSetSortModeTimeStampDescendingNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.TimeStampDescending);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.TimeStampDescending);
        }
    }


    public class SetSortModeSizeCommand : CommandElement
    {
        public SetSortModeSizeCommand() : base(CommandType.SetSortModeSize)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeSize;
            this.Note = Properties.Resources.CommandSetSortModeSizeNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.Size);
        }
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.Size);
        }
    }


    public class SetSortModeSizeDescendingCommand : CommandElement
    {
        public SetSortModeSizeDescendingCommand() : base(CommandType.SetSortModeSizeDescending)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeSizeDescending;
            this.Note = Properties.Resources.CommandSetSortModeSizeDescendingNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.SizeDescending);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.SizeDescending);
        }
    }


    public class SetSortModeRandomCommand : CommandElement
    {
        public SetSortModeRandomCommand() : base(CommandType.SetSortModeRandom)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeRandom;
            this.Note = Properties.Resources.CommandSetSortModeRandomNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.Random);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.Random);
        }
    }

    public class SetDefaultPageSettingCommand : CommandElement
    {
        public SetDefaultPageSettingCommand() : base(CommandType.SetDefaultPageSetting)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetDefaultPageSetting;
            this.Note = Properties.Resources.CommandSetDefaultPageSettingNote;
            this.IsShowMessage = true;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetDefaultPageSetting();
        }
    }


    public class ToggleBookmarkCommand : CommandElement
    {
        public ToggleBookmarkCommand() : base(CommandType.ToggleBookmark)
        {
            this.Group = Properties.Resources.CommandGroupBookmark;
            this.Text = Properties.Resources.CommandToggleBookmark;
            this.MenuText = Properties.Resources.CommandToggleBookmarkMenu;
            this.Note = Properties.Resources.CommandToggleBookmarkNote;
            this.ShortCutKey = "Ctrl+D";
            this.IsShowMessage = true;
        }
        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.IsBookmark)) { Source = BookOperation.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.IsBookmark ? Properties.Resources.CommandToggleBookmarkOff : Properties.Resources.CommandToggleBookmarkOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.CanBookmark();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.ToggleBookmark();
        }
    }


    public class TogglePagemarkCommand : CommandElement
    {
        public TogglePagemarkCommand() : base(CommandType.TogglePagemark)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandTogglePagemark;
            this.MenuText = Properties.Resources.CommandTogglePagemarkMenu;
            this.Note = Properties.Resources.CommandTogglePagemarkNote;
            this.ShortCutKey = "Ctrl+M";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.IsPagemark)) { Source = BookOperation.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.IsMarked() ? Properties.Resources.CommandTogglePagemarkOff : Properties.Resources.CommandTogglePagemarkOn;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.CanPagemark();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.TogglePagemark();
        }
    }


    public class PrevPagemarkCommand : CommandElement
    {
        public PrevPagemarkCommand() : base(CommandType.PrevPagemark)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandPrevPagemark;
            this.Note = Properties.Resources.CommandPrevPagemarkNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            PagemarkList.Current.PrevPagemark();
        }
    }


    public class NextPagemarkCommand : CommandElement
    {
        public NextPagemarkCommand() : base(CommandType.NextPagemark)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandNextPagemark;
            this.Note = Properties.Resources.CommandNextPagemarkNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            PagemarkList.Current.NextPagemark();
        }
    }


    public class PrevPagemarkInBookCommand : CommandElement
    {
        public PrevPagemarkInBookCommand() : base(CommandType.PrevPagemarkInBook)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandPrevPagemarkInBook;
            this.Note = Properties.Resources.CommandPrevPagemarkInBookNote;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new MovePagemarkCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            // TODO: parameterを引数で渡す
            return BookOperation.Current.CanPrevPagemarkInPlace((MovePagemarkCommandParameter)param);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.PrevPagemarkInPlace((MovePagemarkCommandParameter)param);
        }
    }


    public class NextPagemarkInBookCommand : CommandElement
    {
        public NextPagemarkInBookCommand() : base(CommandType.NextPagemarkInBook)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandNextPagemarkInBook;
            this.Note = Properties.Resources.CommandNextPagemarkInBookNote;
            this.IsShowMessage = false;

            // PrevPagemarkInBook
            this.ParameterSource = new CommandParameterSource(new MovePagemarkCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            // TODO: parameterを引数で渡す
            return BookOperation.Current.CanNextPagemarkInPlace((MovePagemarkCommandParameter)param);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.NextPagemarkInPlace((MovePagemarkCommandParameter)param);
        }
    }


    public class ToggleCustomSizeCommand : CommandElement
    {
        public ToggleCustomSizeCommand() : base(CommandType.ToggleCustomSize)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleCustomSize;
            this.MenuText = Properties.Resources.CommandToggleCustomSizeMenu;
            this.Note = Properties.Resources.CommandToggleCustomSizeNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PictureProfile.Current.CustomSize.IsEnabled)) { Mode = BindingMode.OneWay, Source = PictureProfile.Current.CustomSize };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return PictureProfile.Current.CustomSize.IsEnabled ? Properties.Resources.CommandToggleCustomSizeOff : Properties.Resources.CommandToggleCustomSizeOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            PictureProfile.Current.CustomSize.IsEnabled = !PictureProfile.Current.CustomSize.IsEnabled;
        }
    }


    public class ToggleResizeFilterCommand : CommandElement
    {
        public ToggleResizeFilterCommand() : base(CommandType.ToggleResizeFilter)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleResizeFilter;
            this.MenuText = Properties.Resources.CommandToggleResizeFilterMenu;
            this.Note = Properties.Resources.CommandToggleResizeFilterNote;
            this.ShortCutKey = "Ctrl+R";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PictureProfile.Current.IsResizeFilterEnabled)) { Mode = BindingMode.OneWay, Source = PictureProfile.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return PictureProfile.Current.IsResizeFilterEnabled ? Properties.Resources.CommandToggleResizeFilterOff : Properties.Resources.CommandToggleResizeFilterOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            PictureProfile.Current.IsResizeFilterEnabled = !PictureProfile.Current.IsResizeFilterEnabled;
        }
    }


    public class ToggleGridCommand : CommandElement
    {
        public ToggleGridCommand() : base(CommandType.ToggleGrid)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleGrid;
            this.MenuText = Properties.Resources.CommandToggleGridMenu;
            this.Note = Properties.Resources.CommandToggleGridNote;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ContentCanvas.Current.GridLine.IsEnabled)) { Mode = BindingMode.OneWay, Source = ContentCanvas.Current.GridLine };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvas.Current.GridLine.IsEnabled ? Properties.Resources.CommandToggleGridOff : Properties.Resources.CommandToggleGridOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.GridLine.IsEnabled = !ContentCanvas.Current.GridLine.IsEnabled;
        }
    }


    public class ToggleEffectCommand : CommandElement
    {
        public ToggleEffectCommand() : base(CommandType.ToggleEffect)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandToggleEffect;
            this.MenuText = Properties.Resources.CommandToggleEffectMenu;
            this.Note = Properties.Resources.CommandToggleEffectNote;
            this.ShortCutKey = "Ctrl+E";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(ImageEffect.Current.IsEnabled)) { Mode = BindingMode.OneWay, Source = ImageEffect.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ImageEffect.Current.IsEnabled ? Properties.Resources.CommandToggleEffectOff : Properties.Resources.CommandToggleEffectOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ImageEffect.Current.IsEnabled = !ImageEffect.Current.IsEnabled;
        }
    }


    public class ToggleIsLoupeCommand : CommandElement
    {
        public ToggleIsLoupeCommand() : base(CommandType.ToggleIsLoupe)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleIsLoupe;
            this.MenuText = Properties.Resources.CommandToggleIsLoupeMenu;
            this.Note = Properties.Resources.CommandToggleIsLoupeNote;
            this.IsShowMessage = false;
        }
        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MouseInput.Current.IsLoupeMode)) { Mode = BindingMode.OneWay, Source = MouseInput.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return MouseInput.Current.IsLoupeMode ? Properties.Resources.CommandToggleIsLoupeOff : Properties.Resources.CommandToggleIsLoupeOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MouseInput.Current.IsLoupeMode = !MouseInput.Current.IsLoupeMode;
        }
    }


    public class LoupeOnCommand : CommandElement
    {
        public LoupeOnCommand() : base(CommandType.LoupeOn)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeOn;
            this.Note = Properties.Resources.CommandLoupeOnNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MouseInput.Current.IsLoupeMode = true;
        }
    }


    public class LoupeOffCommand : CommandElement
    {
        public LoupeOffCommand() : base(CommandType.LoupeOff)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeOff;
            this.Note = Properties.Resources.CommandLoupeOffNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MouseInput.Current.IsLoupeMode = false;
        }
    }


    public class LoupeScaleUpCommand : CommandElement
    {
        public LoupeScaleUpCommand() : base(CommandType.LoupeScaleUp)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeScaleUp;
            this.Note = Properties.Resources.CommandLoupeScaleUpNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return MouseInput.Current.IsLoupeMode;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MouseInput.Current.Loupe.LoupeZoomIn();
        }
    }


    public class LoupeScaleDownCommand : CommandElement
    {
        public LoupeScaleDownCommand() : base(CommandType.LoupeScaleDown)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeScaleDown;
            this.Note = Properties.Resources.CommandLoupeScaleDownNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return MouseInput.Current.IsLoupeMode;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MouseInput.Current.Loupe.LoupeZoomOut();
        }
    }


    public class OpenSettingWindowCommand : CommandElement
    {
        public OpenSettingWindowCommand() : base(CommandType.OpenSettingWindow)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenSettingWindow;
            this.MenuText = Properties.Resources.CommandOpenSettingWindowMenu;
            this.Note = Properties.Resources.CommandOpenSettingWindowNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.OpenSettingWindow();
        }
    }


    public class OpenSettingFilesFolderCommand : CommandElement
    {
        public OpenSettingFilesFolderCommand() : base(CommandType.OpenSettingFilesFolder)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenSettingFilesFolder;
            this.Note = Properties.Resources.CommandOpenSettingFilesFolderNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.OpenSettingFilesFolder();
        }
    }


    public class OpenVersionWindowCommand : CommandElement
    {
        public OpenVersionWindowCommand() : base(CommandType.OpenVersionWindow)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenVersionWindow;
            this.MenuText = Properties.Resources.CommandOpenVersionWindowMenu;
            this.Note = Properties.Resources.CommandOpenVersionWindowNote;
            this.IsShowMessage = false;
        }
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.OpenVersionWindow();
        }
    }


    public class CloseApplicationCommand : CommandElement
    {
        public CloseApplicationCommand() : base(CommandType.CloseApplication)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandCloseApplication;
            this.MenuText = Properties.Resources.CommandCloseApplicationMenu;
            this.Note = Properties.Resources.CommandCloseApplicationNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindow.Current.Close();
        }
    }


    public class TogglePermitFileCommandCommand : CommandElement
    {
        public TogglePermitFileCommandCommand() : base(CommandType.TogglePermitFileCommand)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandTogglePermitFileCommand;
            this.MenuText = Properties.Resources.CommandTogglePermitFileCommandMenu;
            this.Note = Properties.Resources.CommandTogglePermitFileCommandNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(FileIOProfile.Current.IsEnabled)) { Source = FileIOProfile.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return FileIOProfile.Current.IsEnabled ? Properties.Resources.CommandTogglePermitFileCommandOff : Properties.Resources.CommandTogglePermitFileCommandOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            FileIOProfile.Current.IsEnabled = !FileIOProfile.Current.IsEnabled;
        }
    }


    public class HelpCommandListCommand : CommandElement
    {
        public HelpCommandListCommand() : base(CommandType.HelpCommandList)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandHelpCommandList;
            this.MenuText = Properties.Resources.CommandHelpCommandListMenu;
            this.Note = Properties.Resources.CommandHelpCommandListNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            CommandTable.Current.OpenCommandListHelp();
        }
    }


    public class HelpMainMenuCommand : CommandElement
    {
        public HelpMainMenuCommand() : base(CommandType.HelpMainMenu)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandHelpMainMenu;
            this.MenuText = Properties.Resources.CommandHelpMainMenuMenu;
            this.Note = Properties.Resources.CommandHelpMainMenuNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MenuBar.Current.OpenMainMenuHelp();
        }
    }


    public class HelpSearchOptionCommand : CommandElement
    {
        public HelpSearchOptionCommand() : base(CommandType.HelpSearchOption)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandHelpSearchOption;
            this.MenuText = Properties.Resources.CommandHelpSearchOptionMenu;
            this.Note = Properties.Resources.CommandHelpSearchOptionNote;
            this.IsShowMessage = false;
        }
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MenuBar.Current.OpenSearchOptionHelp();
        }
    }


    public class OpenContextMenuCommand : CommandElement
    {
        public OpenContextMenuCommand() : base(CommandType.OpenContextMenu)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenContextMenu;
            this.Note = Properties.Resources.CommandOpenContextMenuNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindow.Current.OpenContextMenu();
        }
    }


    public class ExportBackupCommand : CommandElement
    {
        public ExportBackupCommand() : base(CommandType.ExportBackup)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandExportBackup;
            this.MenuText = Properties.Resources.CommandExportBackupMenu;
            this.Note = Properties.Resources.CommandExportBackupNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SaveDataBackup.Current.ExportBackup();
        }
    }


    public class ImportBackupCommand : CommandElement
    {
        public ImportBackupCommand() : base(CommandType.ImportBackup)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandImportBackup;
            this.MenuText = Properties.Resources.CommandImportBackupMenu;
            this.Note = Properties.Resources.CommandImportBackupNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SaveDataBackup.Current.ImportBackup();
        }
    }


    public class ReloadUserSettingCommand : CommandElement
    {
        public ReloadUserSettingCommand() : base(CommandType.ReloadUserSetting)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandReloadUserSetting;
            this.Note = Properties.Resources.CommandReloadUserSettingNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SaveData.Current.LoadUserSetting();
        }
    }


    public class TouchEmulateCommand : CommandElement
    {
        public TouchEmulateCommand() : base(CommandType.TouchEmulate)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandTouchEmulate;
            this.Note = Properties.Resources.CommandTouchEmulateNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            TouchInput.Current.Emulator.Execute();
        }
    }
}
