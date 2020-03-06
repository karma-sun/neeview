using NeeView.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    [Flags]
    public enum CommandOption
    {
        None = 0,
        ByMenu = 0x0001,
    }

    /// <summary>
    /// コマンド設定
    /// </summary>
    public class CommandElement
    {
        // コマンドのグループ
        public string Group { get; set; }

        // コマンド表示名
        public string Text { get; set; }

        // メニュー表示名
        private string _menuText;
        public string MenuText
        {
            get { return _menuText ?? Text; }
            set { _menuText = value; }
        }

        // ショートカットキー
        public string ShortCutKey { get; set; }

        // タッチ
        public string TouchGesture { get; set; }

        // マウスジェスチャー
        public string MouseGesture { get; set; }

        // コマンド実行時の通知フラグ
        public bool IsShowMessage { get; set; }

        // コマンド本体
        public Action<int, CommandOption> Execute { get; set; }

        // コマンド実行時表示デリゲート
        public Func<int, string> ExecuteMessage { get; set; }

        // コマンド実行可能判定
        public Func<bool> CanExecute { get; set; }

        // フラグバインディング
        public Func<System.Windows.Data.Binding> CreateIsCheckedBinding { get; set; }

        // ペアコマンド
        public CommandType PairPartner { get; set; }


        // トグル候補
        [Obsolete]
        public bool IsToggled { get; set; }

        // コマンド説明
        public string Note { get; set; }

        //
        private static Regex _tipsRegex = new Regex(@"<[\w/]+>", RegexOptions.Compiled);


        // コマンドパラメータ標準
        public CommandParameter DefaultParameter { get; set; }

        // コマンドパラメータ
        private CommandParameter _parameterRaw;

        // コマンドパラメータ(Raw)
        public CommandParameter ParameterRaw
        {
            get { return GetParameter(true); }
        }

        // コマンドパラメータ
        // 共有解決したパラメータ。通常はこちらを使用します
        public CommandParameter Parameter
        {
            get { return GetParameter(false); }
            set { SetParameter(value); }
        }

        // コマンドパラメータ存在？
        public bool HasParameter => DefaultParameter != null;

        /// <summary>
        /// パラメータ取得
        /// </summary>
        /// <param name="isRaw"></param>
        /// <returns></returns>
        public CommandParameter GetParameter(bool isRaw)
        {
            if (_parameterRaw == null && DefaultParameter != null)
            {
                _parameterRaw = (CommandParameter)DefaultParameter.Clone();
            }

            if (isRaw)
            {
                return _parameterRaw;
            }
            else
            {
                return _parameterRaw?.Entity();
            }
        }

        /// <summary>
        /// パラメータ設定
        /// </summary>
        /// <param name="value"></param>
        private void SetParameter(CommandParameter value)
        {
            if (!DefaultParameter.IsReadOnly())
            {
                _parameterRaw = value;
            }
        }

        // constructor
        public CommandElement()
        {
            ExecuteMessage = e => Text;
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

            // no used
            [Obsolete, DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsToggled { get; set; }

            private void Constructor()
            {
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
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

            if (HasParameter && !DefaultParameter.IsReadOnly())
            {
                var type = DefaultParameter.GetType();
                var original = Json.Serialize(DefaultParameter, type);
                var current = Json.Serialize(Parameter, type);
                if (original != current)
                {
                    memento.Parameter = current;
                }
            }

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            ShortCutKey = memento.ShortCutKey?.TrimStart(',');
            TouchGesture = memento.TouchGesture ?? this.TouchGesture; // compatible before ver.24
            MouseGesture = memento.MouseGesture;
            IsShowMessage = memento.IsShowMessage;

#pragma warning disable CS0612
            IsToggled = memento.IsToggled;
#pragma warning restore CS0612

            if (HasParameter)
            {
                Parameter = memento.Parameter != null
                    ? (CommandParameter)Json.Deserialize(memento.Parameter, DefaultParameter.GetType())
                    : null;
            }
        }

        #endregion
    }
}
