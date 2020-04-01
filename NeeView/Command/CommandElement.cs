using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    [DataContract]
    public abstract class CommandElement
    {
        public static CommandElement None { get; } = new NoneCommand();

        public static object[] EmptyArgs { get; } = new object[] { };

        private string _menuText;
        private string _shortCutKey;
        private string _touchGesture;
        private string _mouseGesture;


        public CommandElement(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        // コマンドのグループ
        public string Group { get; set; }

        // コマンド表示名
        public string Text { get; set; }

        public string LongText => Group + "/" + Text;

        public string MenuText
        {
            get { return _menuText ?? Text; }
            set { _menuText = value; }
        }

        // コマンド説明
        public string Note { get; set; }


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
            ParameterSource = new CommandParameterSource(share.ParameterSource);
            return this;
        }


        // フラグバインディング 
        public virtual Binding CreateIsCheckedBinding()
        {
            return null;
        }

        // コマンド実行時表示デリゲート
        public virtual string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return Text;
        }

        public string ExecuteMessage(object[] args, CommandOption option)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            return ExecuteMessage(this.Parameter, args, option);
        }

        // コマンド実行可能判定
        public virtual bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return true;
        }

        public bool CanExecute(object[] args, CommandOption option)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            return CanExecute(this.Parameter, args, option);
        }

        // コマンド実行
        public abstract void Execute(CommandParameter param, object[] args, CommandOption option);

        public void Execute(object[] args, CommandOption option)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            Execute(this.Parameter, args, option);
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

            var map = new PropertyMap(clone);
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
            return string.Join(",", new string[] { this.Group, this.Text, this.MenuText, this.Note, this.ShortCutKey, this.MouseGesture, new MouseGestureSequence(this.MouseGesture).ToDispString(), this.TouchGesture });
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
                clone.Parameter = (CommandParameter)this.Parameter.Clone();
                return clone;
            }
        }

        public MementoV2 CreateMementoV2()
        {
            var memento = new MementoV2();

            memento.ShortCutKey = ShortCutKey ?? string.Empty;
            memento.TouchGesture = TouchGesture ?? string.Empty;
            memento.MouseGesture = MouseGesture ?? string.Empty;
            memento.IsShowMessage = IsShowMessage;
            memento.Parameter = (CommandParameter)ParameterSource?.GetRaw()?.Clone();

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

