using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{


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

