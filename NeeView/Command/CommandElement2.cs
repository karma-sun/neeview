using NeeView.Effects;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Data;

namespace NeeView
{
    [DataContract]
    public abstract class CommandElement2
    {
        private string _menuText;


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

        // NOTE: 直接ParameterSourceを見て判定する
        //public bool HasParameter => ParameterSource != null;


        // フラグバインディング 
        public virtual System.Windows.Data.Binding CreateIsCheckedBinding()
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
                //this.InitializePropertyDefaultValues();
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
    public class LoadAsCommand : CommandElement2
    {
        public LoadAsCommand()
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

    public class ReLoadCommand : CommandElement2
    {
        public ReLoadCommand()
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

    public class UnloadCommand : CommandElement2
    {
        public UnloadCommand()
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


        public class OpenApplicationCommand : CommandElement2
        {
            public OpenApplicationCommand()
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

        public class OpenFilePlaceCommand : CommandElement2
        {
            public OpenFilePlaceCommand()
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

        public class ExportCommand : CommandElement2
        {
            public ExportCommand()
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

        public class ExportImageCommand : CommandElement2
        {
            public ExportImageCommand()
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
    }

    public class PrintCommand : CommandElement2
    {
        public PrintCommand()
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

    public class DeleteFileCommand : CommandElement2
    {
        public DeleteFileCommand()
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

    public class DeleteBookCommand : CommandElement2
    {
        public DeleteBookCommand()
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

    public class CopyFileCommand : CommandElement2
    {
        public CopyFileCommand()
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

    public class CopyImageCommand : CommandElement2
    {
        public CopyImageCommand()
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

    public class PasteCommand : CommandElement2
    {
        public PasteCommand()
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

    public class ClearHistoryCommand : CommandElement2
    {
        public ClearHistoryCommand()
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
    public class ClearHistoryInPlaceCommand : CommandElement2
    {
        public ClearHistoryInPlaceCommand()
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

    public class ToggleStretchModeCommand : CommandElement2
    {
        public ToggleStretchModeCommand()
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

    public class ToggleStretchModeReverseCommand : CommandElement2
    {
        public ToggleStretchModeReverseCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchModeReverse;
            this.Note = Properties.Resources.CommandToggleStretchModeReverseNote;
            this.ShortCutKey = "LeftButton+WheelUp";
            this.IsShowMessage = true;

            // CommandType.ToggleStretchMode
            ParameterSource = new CommandParameterSource(share.ParameterSource);
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

    public class SetStretchModeNoneCommand : CommandElement2
    {
        public SetStretchModeNoneCommand()
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

    public class SetStretchModeUniformCommand : CommandElement2
    {
        public SetStretchModeUniformCommand()
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

    public class SetStretchModeUniformToFillCommand : CommandElement2
    {
        public SetStretchModeUniformToFillCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniformToFill;
            this.Note = Properties.Resources.CommandSetStretchModeUniformToFillNote;
            this.IsShowMessage = true;

            // CommandType.SetStretchModeUniform
            this.ParameterSource = new CommandParameterSource(share.ParameterSource);
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

    public class SetStretchModeUniformToSizeCommand : CommandElement2
    {
        public SetStretchModeUniformToSizeCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniformToSize;
            this.Note = Properties.Resources.CommandSetStretchModeUniformToSizeNote;
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // SetStretchModeUniform
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


    public class SetStretchModeUniformToVerticalCommand : CommandElement2
    {
        public SetStretchModeUniformToVerticalCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniformToVertical;
            this.Note = Properties.Resources.CommandSetStretchModeUniformToVerticalNote;
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // SetStretchModeUniform
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

    public class SetStretchModeUniformToHorizontalCommand : CommandElement2
    {
        public SetStretchModeUniformToHorizontalCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeUniformToHorizontal;
            this.Note = Properties.Resources.CommandSetStretchModeUniformToHorizontalNote;
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // SetStretchModeUniform
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


    public class ToggleStretchAllowEnlargeCommand : CommandElement2
    {
        public ToggleStretchAllowEnlargeCommand()
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


    public class ToggleStretchAllowReduceCommand : CommandElement2
    {
        public ToggleStretchAllowReduceCommand()
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


    public class ToggleIsEnabledNearestNeighborCommand : CommandElement2
    {
        public ToggleIsEnabledNearestNeighborCommand()
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


    public class ToggleBackgroundCommand : CommandElement2
    {
        public ToggleBackgroundCommand()
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


    public class SetBackgroundBlackCommand : CommandElement2
    {
        public SetBackgroundBlackCommand()
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


    public class SetBackgroundWhiteCommand : CommandElement2
    {
        public SetBackgroundWhiteCommand()
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


    public class SetBackgroundAutoCommand : CommandElement2
    {
        public SetBackgroundAutoCommand()
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


    public class SetBackgroundCheckCommand : CommandElement2
    {
        public SetBackgroundCheckCommand()
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


    public class SetBackgroundCheckDarkCommand : CommandElement2
    {
        public SetBackgroundCheckDarkCommand()
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


    public class SetBackgroundCustomCommand : CommandElement2
    {
        public SetBackgroundCustomCommand()
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


    public class ToggleTopmostCommand : CommandElement2
    {
        public ToggleTopmostCommand()
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

    public class ToggleHideMenuCommand : CommandElement2
    {
        public ToggleHideMenuCommand()
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


    public class ToggleHidePageSliderCommand : CommandElement2
    {
        public ToggleHidePageSliderCommand()
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


    public class ToggleHidePanelCommand : CommandElement2
    {
        public ToggleHidePanelCommand()
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


    public class ToggleVisibleTitleBarCommand : CommandElement2
    {
        public ToggleVisibleTitleBarCommand()
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


    public class ToggleVisibleAddressBarCommand : CommandElement2
    {
        public ToggleVisibleAddressBarCommand()
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


    public class ToggleVisibleSideBarCommand : CommandElement2
    {
        public ToggleVisibleSideBarCommand()
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


    public class ToggleVisibleFileInfoCommand : CommandElement2
    {
        public ToggleVisibleFileInfoCommand()
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


    public class ToggleVisibleEffectInfoCommand : CommandElement2
    {
        public ToggleVisibleEffectInfoCommand()
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


    public class ToggleVisibleBookshelfCommand : CommandElement2
    {
        public ToggleVisibleBookshelfCommand()
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


    public class ToggleVisibleBookmarkListCommand : CommandElement2
    {
        public ToggleVisibleBookmarkListCommand()
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


    public class ToggleVisiblePagemarkListCommand : CommandElement2
    {
        public ToggleVisiblePagemarkListCommand()
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

    public class ToggleVisibleHistoryListCommand : CommandElement2
    {
        public ToggleVisibleHistoryListCommand()
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


    public class ToggleVisiblePageListCommand : CommandElement2
    {
        public ToggleVisiblePageListCommand()
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


    public class ToggleVisibleFoldersTreeCommand : CommandElement2
    {
        public ToggleVisibleFoldersTreeCommand()
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


    public class FocusFolderSearchBoxCommand : CommandElement2
    {
        public FocusFolderSearchBoxCommand()
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


    public class FocusBookmarkListCommand : CommandElement2
    {
        public FocusBookmarkListCommand()
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


    public class FocusMainViewCommand : CommandElement2
    {
        public FocusMainViewCommand()
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


    public class TogglePageListPlacementCommand : CommandElement2
    {
        public TogglePageListPlacementCommand()
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


    public class ToggleVisibleThumbnailListCommand : CommandElement2
    {
        public ToggleVisibleThumbnailListCommand()
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


    public class ToggleHideThumbnailListCommand : CommandElement2
    {
        public ToggleHideThumbnailListCommand()
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


    public class ToggleFullScreenCommand : CommandElement2
    {
        public ToggleFullScreenCommand()
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


    public class SetFullScreenCommand : CommandElement2
    {
        public SetFullScreenCommand()
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


    public class CancelFullScreenCommand : CommandElement2
    {
        public CancelFullScreenCommand()
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


    public class ToggleWindowMinimizeCommand : CommandElement2
    {
        public ToggleWindowMinimizeCommand()
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


    public class ToggleWindowMaximizeCommand : CommandElement2
    {
        public ToggleWindowMaximizeCommand()
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


    public class ShowHiddenPanelsCommand : CommandElement2
    {
        public ShowHiddenPanelsCommand()
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


    public class ToggleSlideShowCommand : CommandElement2
    {
        public ToggleSlideShowCommand()
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


    public class ViewScrollUpCommand : CommandElement2
    {
        public ViewScrollUpCommand()
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


    public class ViewScrollDownCommand : CommandElement2
    {
        public ViewScrollDownCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollDown;
            this.Note = Properties.Resources.CommandViewScrollDownNote;
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // ViewScrollUp
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.ScrollDown((ViewScrollCommandParameter)param);
        }
    }


    public class ViewScrollLeftCommand : CommandElement2
    {
        public ViewScrollLeftCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollLeft;
            this.Note = Properties.Resources.CommandViewScrollLeftNote;
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // ViewScrollUp
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.ScrollLeft((ViewScrollCommandParameter)param);
        }
    }


    public class ViewScrollRightCommand : CommandElement2
    {
        public ViewScrollRightCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollRight;
            this.Note = Properties.Resources.CommandViewScrollRightNote;
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // ViewScrollUp
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.ScrollRight((ViewScrollCommandParameter)param);
        }
    }

    public class ViewScaleUpCommand : CommandElement2
    {
        public ViewScaleUpCommand()
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

    public class ViewScaleDownCommand : CommandElement2
    {
        public ViewScaleDownCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScaleDown;
            this.Note = Properties.Resources.CommandViewScaleDownNote;
            this.ShortCutKey = "RightButton+WheelDown";
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // ViewScaleUp
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            var parameter = (ViewScaleCommandParameter)param;
            DragTransformControl.Current.ScaleDown(parameter.Scale / 100.0, parameter.IsSnapDefaultScale, ContentCanvas.Current.MainContentScale);
        }
    }


    public class ViewRotateLeftCommand : CommandElement2
    {
        public ViewRotateLeftCommand()
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


    public class ViewRotateRightCommand : CommandElement2
    {
        public ViewRotateRightCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewRotateRight;
            this.Note = Properties.Resources.CommandViewRotateRightNote;
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // ViewRotateLeft
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.ViewRotateRight((ViewRotateCommandParameter)param);
        }
    }


    public class ToggleIsAutoRotateLeftCommand : CommandElement2
    {
        public ToggleIsAutoRotateLeftCommand()
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


    public class ToggleIsAutoRotateRightCommand : CommandElement2
    {
        public ToggleIsAutoRotateRightCommand()
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


    public class ToggleViewFlipHorizontalCommand : CommandElement2
    {
        public ToggleViewFlipHorizontalCommand()
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


    public class ViewFlipHorizontalOnCommand : CommandElement2
    {
        public ViewFlipHorizontalOnCommand()
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


    public class ViewFlipHorizontalOffCommand : CommandElement2
    {
        public ViewFlipHorizontalOffCommand()
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


    public class ToggleViewFlipVerticalCommand : CommandElement2
    {
        public ToggleViewFlipVerticalCommand()
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


    public class ViewFlipVerticalOnCommand : CommandElement2
    {
        public ViewFlipVerticalOnCommand()
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


    public class ViewFlipVerticalOffCommand : CommandElement2
    {
        public ViewFlipVerticalOffCommand()
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


    public class ViewResetCommand : CommandElement2
    {
        public ViewResetCommand()
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


    public class PrevPageCommand : CommandElement2
    {
        public PrevPageCommand()
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


    public class NextPageCommand : CommandElement2
    {
        public NextPageCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextPage;
            this.Note = Properties.Resources.CommandNextPageNote;
            this.ShortCutKey = "Left,LeftClick";
            this.TouchGesture = "TouchL1,TouchL2";
            this.MouseGesture = "L";
            this.IsShowMessage = false;
            this.PairPartner = CommandType.PrevPage;

            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // PrevPage
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.NextPage();
        }
    }


    public class PrevOnePageCommand : CommandElement2
    {
        public PrevOnePageCommand()
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


    public class NextOnePageCommand : CommandElement2
    {
        public NextOnePageCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextOnePage;
            this.Note = Properties.Resources.CommandNextOnePageNote;
            this.MouseGesture = "RL";
            this.IsShowMessage = false;
            this.PairPartner = CommandType.PrevOnePage;

            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // PrevOnePage
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.NextOnePage();
        }
    }


    public class PrevScrollPageCommand : CommandElement2
    {
        public PrevScrollPageCommand()
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


    public class NextScrollPageCommand : CommandElement2
    {
        public NextScrollPageCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextScrollPage;
            this.Note = Properties.Resources.CommandNextScrollPageNote;
            this.ShortCutKey = "WheelDown";
            this.IsShowMessage = false;
            this.PairPartner = CommandType.PrevScrollPage;

            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // PrevScrollPage
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.NextScrollPage();
        }
    }


    public class JumpPageCommand : CommandElement2
    {
        public JumpPageCommand()
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


    public class PrevSizePageCommand : CommandElement2
    {
        public PrevSizePageCommand()
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


    public class NextSizePageCommand : CommandElement2
    {
        public NextSizePageCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextSizePage;
            this.Note = Properties.Resources.CommandNextSizePageNote;
            this.IsShowMessage = false;
            this.PairPartner = CommandType.PrevSizePage;

            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // PrevSizePage
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.NextSizePage(((MoveSizePageCommandParameter)param).Size);
        }
    }


    public class PrevFolderPageCommand : CommandElement2
    {
        public PrevFolderPageCommand()
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


    public class NextFolderPageCommand : CommandElement2
    {
        public NextFolderPageCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextFolderPage;
            this.Note = Properties.Resources.CommandNextFolderPageNote;
            this.IsShowMessage = true;
            this.PairPartner = CommandType.PrevFolderPage;

            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // PrevFolderPage
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


    public class FirstPageCommand : CommandElement2
    {
        public FirstPageCommand()
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


    public class LastPageCommand : CommandElement2
    {
        public LastPageCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandLastPage;
            this.Note = Properties.Resources.CommandLastPageNote;
            this.ShortCutKey = "Ctrl+Left";
            this.MouseGesture = "UL";
            this.IsShowMessage = true;
            this.PairPartner = CommandType.FirstPage;

            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // FirstPage
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.LastPage();
        }
    }


    public class PrevFolderCommand : CommandElement2
    {
        public PrevFolderCommand()
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


    public class NextFolderCommand : CommandElement2
    {
        public NextFolderCommand()
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


    public class PrevHistoryCommand : CommandElement2
    {
        public PrevHistoryCommand()
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


    public class NextHistoryCommand : CommandElement2
    {
        public NextHistoryCommand()
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


    public class PrevBookHistoryCommand : CommandElement2
    {
        public PrevBookHistoryCommand()
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


    public class NextBookHistoryCommand : CommandElement2
    {
        public NextBookHistoryCommand()
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


    public class MoveToParentBookCommand : CommandElement2
    {
        public MoveToParentBookCommand()
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


    public class MoveToChildBookCommand : CommandElement2
    {
        public MoveToChildBookCommand()
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


    public class ToggleMediaPlayCommand : CommandElement2
    {
        public ToggleMediaPlayCommand()
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


    public class ToggleFolderOrderCommand : CommandElement2
    {
        public ToggleFolderOrderCommand()
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


    public class SetFolderOrderByFileNameACommand : CommandElement2
    {
        public SetFolderOrderByFileNameACommand()
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


    public class SetFolderOrderByFileNameDCommand : CommandElement2
    {
        public SetFolderOrderByFileNameDCommand()
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


    public class SetFolderOrderByPathACommand : CommandElement2
    {
        public SetFolderOrderByPathACommand()
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


    public class SetFolderOrderByPathDCommand : CommandElement2
    {
        public SetFolderOrderByPathDCommand()
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


    public class SetFolderOrderByFileTypeACommand : CommandElement2
    {
        public SetFolderOrderByFileTypeACommand()
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


    public class SetFolderOrderByFileTypeDCommand : CommandElement2
    {
        public SetFolderOrderByFileTypeDCommand()
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


    public class SetFolderOrderByTimeStampACommand : CommandElement2
    {
        public SetFolderOrderByTimeStampACommand()
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


    public class SetFolderOrderByTimeStampDCommand : CommandElement2
    {
        public SetFolderOrderByTimeStampDCommand()
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


    public class SetFolderOrderByEntryTimeACommand : CommandElement2
    {
        public SetFolderOrderByEntryTimeACommand()
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


    public class SetFolderOrderByEntryTimeDCommand : CommandElement2
    {
        public SetFolderOrderByEntryTimeDCommand()
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
    public class SetFolderOrderBySizeACommand : CommandElement2
    {
        public SetFolderOrderBySizeACommand()
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


    public class SetFolderOrderBySizeDCommand : CommandElement2
    {
        public SetFolderOrderBySizeDCommand()
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


    public class SetFolderOrderByRandomCommand : CommandElement2
    {
        public SetFolderOrderByRandomCommand()
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


    public class TogglePageModeCommand : CommandElement2
    {
        public TogglePageModeCommand()
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


    public class SetPageMode1Command : CommandElement2
    {
        public SetPageMode1Command()
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


    public class SetPageMode2Command : CommandElement2
    {
        public SetPageMode2Command()
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


    public class ToggleBookReadOrderCommand : CommandElement2
    {
        public ToggleBookReadOrderCommand()
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


    public class SetBookReadOrderRightCommand : CommandElement2
    {
        public SetBookReadOrderRightCommand()
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


    public class SetBookReadOrderLeftCommand : CommandElement2
    {
        public SetBookReadOrderLeftCommand()
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


    public class ToggleIsSupportedDividePageCommand : CommandElement2
    {
        public ToggleIsSupportedDividePageCommand()
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


    public class ToggleIsSupportedWidePageCommand : CommandElement2
    {
        public ToggleIsSupportedWidePageCommand()
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


    public class ToggleIsSupportedSingleFirstPageCommand : CommandElement2
    {
        public ToggleIsSupportedSingleFirstPageCommand()
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


    public class ToggleIsSupportedSingleLastPageCommand : CommandElement2
    {
        public ToggleIsSupportedSingleLastPageCommand()
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


    public class ToggleIsRecursiveFolderCommand : CommandElement2
    {
        public ToggleIsRecursiveFolderCommand()
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


    public class ToggleSortModeCommand : CommandElement2
    {
        public ToggleSortModeCommand()
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


    public class SetSortModeFileNameCommand : CommandElement2
    {
        public SetSortModeFileNameCommand()
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


    public class SetSortModeFileNameDescendingCommand : CommandElement2
    {
        public SetSortModeFileNameDescendingCommand()
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


    public class SetSortModeTimeStampCommand : CommandElement2
    {
        public SetSortModeTimeStampCommand()
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


    public class SetSortModeTimeStampDescendingCommand : CommandElement2
    {
        public SetSortModeTimeStampDescendingCommand()
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


    public class SetSortModeSizeCommand : CommandElement2
    {
        public SetSortModeSizeCommand()
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


    public class SetSortModeSizeDescendingCommand : CommandElement2
    {
        public SetSortModeSizeDescendingCommand()
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


    public class SetSortModeRandomCommand : CommandElement2
    {
        public SetSortModeRandomCommand()
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

    public class SetDefaultPageSettingCommand : CommandElement2
    {
        public SetDefaultPageSettingCommand()
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


    public class ToggleBookmarkCommand : CommandElement2
    {
        public ToggleBookmarkCommand()
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


    public class TogglePagemarkCommand : CommandElement2
    {
        public TogglePagemarkCommand()
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


    public class PrevPagemarkCommand : CommandElement2
    {
        public PrevPagemarkCommand()
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


    public class NextPagemarkCommand : CommandElement2
    {
        public NextPagemarkCommand()
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


    public class PrevPagemarkInBookCommand : CommandElement2
    {
        public PrevPagemarkInBookCommand()
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


    public class NextPagemarkInBookCommand : CommandElement2
    {
        public NextPagemarkInBookCommand(CommandElement2 share)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandNextPagemarkInBook;
            this.Note = Properties.Resources.CommandNextPagemarkInBookNote;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(share.ParameterSource); // PrevPagemarkInBook
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


    public class ToggleCustomSizeCommand : CommandElement2
    {
        public ToggleCustomSizeCommand()
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


    public class ToggleResizeFilterCommand : CommandElement2
    {
        public ToggleResizeFilterCommand()
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


    public class ToggleGridCommand : CommandElement2
    {
        public ToggleGridCommand()
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


    public class ToggleEffectCommand : CommandElement2
    {
        public ToggleEffectCommand()
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


    public class ToggleIsLoupeCommand : CommandElement2
    {
        public ToggleIsLoupeCommand()
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


    public class LoupeOnCommand : CommandElement2
    {
        public LoupeOnCommand()
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


    public class LoupeOffCommand : CommandElement2
    {
        public LoupeOffCommand()
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


    public class LoupeScaleUpCommand : CommandElement2
    {
        public LoupeScaleUpCommand()
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


    public class LoupeScaleDownCommand : CommandElement2
    {
        public LoupeScaleDownCommand()
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


    public class OpenSettingWindowCommand : CommandElement2
    {
        public OpenSettingWindowCommand()
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


    public class OpenSettingFilesFolderCommand : CommandElement2
    {
        public OpenSettingFilesFolderCommand()
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


    public class OpenVersionWindowCommand : CommandElement2
    {
        public OpenVersionWindowCommand()
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


    public class CloseApplicationCommand : CommandElement2
    {
        public CloseApplicationCommand()
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


    public class TogglePermitFileCommandCommand : CommandElement2
    {
        public TogglePermitFileCommandCommand()
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


    public class HelpCommandListCommand : CommandElement2
    {
        public HelpCommandListCommand()
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


    public class HelpMainMenuCommand : CommandElement2
    {
        public HelpMainMenuCommand()
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


    public class HelpSearchOptionCommand : CommandElement2
    {
        public HelpSearchOptionCommand()
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


    public class OpenContextMenuCommand : CommandElement2
    {
        public OpenContextMenuCommand()
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


    public class ExportBackupCommand : CommandElement2
    {
        public ExportBackupCommand()
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


    public class ImportBackupCommand : CommandElement2
    {
        public ImportBackupCommand()
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


    public class ReloadUserSettingCommand : CommandElement2
    {
        public ReloadUserSettingCommand()
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


    public class TouchEmulateCommand : CommandElement2
    {
        public TouchEmulateCommand()
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
