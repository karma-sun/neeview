using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class CommandArgs
    {
        public static CommandArgs Empty { get; } = new CommandArgs(null, CommandOption.None);

        public CommandArgs(object[] args, CommandOption options)
        {
            this.Args = args ?? CommandElement.EmptyArgs;
            this.Options = options;
        }

        public object[] Args { get; private set; }
        public CommandOption Options { get; private set; }
    }

    public class CommandContext
    {
        public CommandContext(CommandParameter parameter, object[] args, CommandOption options)
        {
            this.Parameter = parameter;
            this.Args = args ?? CommandElement.EmptyArgs;
            this.Options = options;
        }

        public CommandContext(CommandParameter parameter, CommandArgs args) : this(parameter, args.Args, args.Options)
        {
        }

        public CommandParameter Parameter { get; private set; }
        public object[] Args { get; private set; }
        public CommandOption Options { get; private set; }
    }


    public enum CommandGroup
    {
        [AliasName] None,
        [AliasName] Bookmark,
        [AliasName] BookMove,
        [AliasName] BookOrder,
        [AliasName] Effect,
        [AliasName] File,
        [AliasName] FilmStrip,
        [AliasName] ImageScale,
        [AliasName] Move,
        [AliasName] Other,
        [AliasName] Playlist,
        [AliasName] PageOrder,
        [AliasName] PageSetting,
        [AliasName] Panel,
        [AliasName] Script,
        [AliasName] Video,
        [AliasName] ViewManipulation,
        [AliasName] Window,
    }

    [DataContract]
    public abstract class CommandElement
    {
        public static CommandElement None { get; } = new NoneCommand();

        public static object[] EmptyArgs { get; } = new object[] { };

        private string _menuText;
        private string _shortCutKey = "";
        private string _touchGesture = "";
        private string _mouseGesture = "";

        private static Regex _trimCommand = new Regex(@"Command$", RegexOptions.Compiled);

        public CommandElement() : this(null)
        {
        }

        public CommandElement(string name)
        {
            NameSource = new CommandNameSource(name ?? _trimCommand.Replace(this.GetType().Name, ""));

            Text = GetResourceText(null, null, true);
            Menu = GetResourceText(nameof(Menu));
            Remarks = GetResourceText(nameof(Remarks));
        }

        private string GetResourceKey(string property, string postfix = null)
        {
            var period = (property is null) ? "" : ".";
            return "@" + this.GetType().Name + period + property + postfix;
        }

        private string GetResourceText(string property, string postfix = null, bool isRequired = false)
        {
            var resourceKey = GetResourceKey(property, postfix);
            var resourceValue = ResourceService.GetResourceString(resourceKey, true);

#if DEBUG
            if (isRequired && resourceValue is null)
            {
                Debug.WriteLine($"Error: CommandName not found: {resourceKey}");
                return resourceKey;
            }
#endif

            return resourceValue;
        }


        // コマンドの並び優先度
        public int Order { get; set; }

        // コマンド名ソース
        public CommandNameSource NameSource { get; private set; }

        // コマンド名
        public string Name => NameSource.FullName;

        // コマンドのグループ
        public string Group { get; set; }

        // コマンド表示名
        public string Text { get; set; }

        public string LongText => Group + "/" + Text;

        public string Menu
        {
            get { return _menuText ?? Text; }
            set { _menuText = value; }
        }

        // コマンド説明
        public string Remarks { get; set; }


        /// <summary>
        /// 入力情報が変更されたフラグ。
        /// コマンドバインディングの更新判定に使用される。
        /// </summary>
        public bool IsInputGestureDarty { get; set; }

        // ショートカットキー
        public string ShortCutKey
        {
            get { return _shortCutKey; }
            set
            {
                if (_shortCutKey != value)
                {
                    _shortCutKey = value;
                    IsInputGestureDarty = true;
                }
            }
        }

        // タッチ
        public string TouchGesture
        {
            get { return _touchGesture; }
            set
            {
                if (_touchGesture != value)
                {
                    _touchGesture = value;
                    IsInputGestureDarty = true;
                }
            }
        }

        // マウスジェスチャー
        public string MouseGesture
        {
            get { return _mouseGesture; }
            set
            {
                if (_mouseGesture != value)
                {
                    _mouseGesture = value;
                    IsInputGestureDarty = true;
                }
            }
        }

        // コマンド実行時の通知フラグ
        public bool IsShowMessage { get; set; }

        // ペアコマンド
        // TODO: CommandElementを直接指定
        public string PairPartner { get; set; }

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
            ParameterSource = share.ParameterSource;
            return this;
        }


        // フラグバインディング 
        public virtual Binding CreateIsCheckedBinding()
        {
            return null;
        }

        // コマンド実行時表示デリゲート
        public virtual string ExecuteMessage(object sender, CommandContext e)
        {
            return Text;
        }

        public string ExecuteMessage(object sender, CommandArgs args)
        {
            return ExecuteMessage(sender, new CommandContext(this.Parameter, args));
        }

        // コマンド実行可能判定
        public virtual bool CanExecute(object sender, CommandContext e)
        {
            return true;
        }

        public bool CanExecute(object sender, CommandArgs args)
        {
            return CanExecute(sender, new CommandContext(this.Parameter, args));
        }

        // コマンド実行
        public abstract void Execute(object sender, CommandContext args);

        public void Execute(object sender, CommandArgs args)
        {
            Execute(sender, new CommandContext(this.Parameter, args));
        }

        // 一時コマンドパラメーター作成
        public CommandParameter CreateOverwriteCommandParameter(IDictionary<string, object> args)
        {
            if (this.Parameter == null) return null;

            var clone = (CommandParameter)this.Parameter.Clone();
            if (args == null) return clone;

            var map = new PropertyMap(clone, $"nv.Command.{this.Name}.Parameter");
            foreach (var arg in args)
            {
                map[arg.Key] = arg.Value;
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

        // 検索用文字列を取得
        public string GetSearchText()
        {
            return string.Join(",", new string[] { this.Group, this.Text, this.Menu, this.Remarks, this.ShortCutKey, this.MouseGesture, new MouseGestureSequence(this.MouseGesture).ToDispString(), this.TouchGesture });
        }

        // コマンドの複製
        public CommandElement CloneCommand(CommandNameSource name)
        {
            var type = this.GetType();

            var clone = (CommandElement)Activator.CreateInstance(type);

            var memento = CreateMementoV2();
            clone.RestoreV2(memento);
            clone.Order = this.Order;
            clone.ClearGestures();

            clone.NameSource = name;

            if (name.Number != 0)
            {
                clone.Text = clone.Text + " " + name.Number.ToString();
                clone.Menu = clone.Menu + " " + name.Number.ToString();
            }

            return clone;
        }


        private void ClearGestures()
        {
            this.ShortCutKey = "";
            this.TouchGesture = "";
            this.MouseGesture = "";
        }


        public bool IsCloneCommand()
        {
            return NameSource.IsClone;
        }

        public bool CanClone()
        {
            return this.ParameterSource != null;
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
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public Memento Clone()
            {
                return (Memento)MemberwiseClone();
            }
        }

        #endregion

        #region MementoV2

        /// <summary>
        /// 設定V2用
        /// </summary>
        public class MementoV2 : ICloneable
        {
            public string ShortCutKey { get; set; }
            public string TouchGesture { get; set; }
            public string MouseGesture { get; set; }
            public bool IsShowMessage { get; set; }
            public CommandParameter Parameter { get; set; }


            public object Clone()
            {
                var clone = (MementoV2)MemberwiseClone();
                clone.Parameter = (CommandParameter)this.Parameter?.Clone();
                return clone;
            }

            // ショートカットキーの補正
            public void ValidateShortCutKey()
            {
                if (string.IsNullOrWhiteSpace(ShortCutKey)) return;

                var gestures = ShortCutKey.Split(',').Select(e => InputGestureConverter.ConvertFromString(e)).Where(e => e != null).ToList();
                var validShortCutKey = string.Join(",", gestures.Select(e => InputGestureConverter.ConvertToString(e)));
                if (validShortCutKey != ShortCutKey)
                {
                    Debug.WriteLine($"ValidateShortCutKey: {ShortCutKey} => {validShortCutKey}");
                    ShortCutKey = validShortCutKey;
                }
            }
        }

        public MementoV2 CreateMementoV2()
        {
            var memento = new MementoV2();

            memento.ShortCutKey = ShortCutKey ?? "";
            memento.TouchGesture = TouchGesture ?? "";
            memento.MouseGesture = MouseGesture ?? "";
            memento.IsShowMessage = IsShowMessage;
            ////memento.Parameter = (CommandParameter)ParameterSource?.GetRaw()?.Clone();
            memento.Parameter = (CommandParameter)Parameter?.Clone();

            Debug.Assert(Parameter == null || JsonCommandParameterConverter.KnownTypes.Contains(Parameter.GetType()));

            return memento;
        }

        public void RestoreV2(MementoV2 memento)
        {
            if (memento == null) return;

            ShortCutKey = memento.ShortCutKey?.TrimStart(',');
            TouchGesture = memento.TouchGesture;
            MouseGesture = memento.MouseGesture;
            IsShowMessage = memento.IsShowMessage;
            ParameterSource?.Set(memento.Parameter);
        }

        #endregion

    }
}

