using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public enum InfoMessageType
    {
        Notify,
        BookName,
        Command,
        Gesture,
        Loading,
        ViewTransform,
    }

    // 通知表示の種類
    public enum ShowMessageStyle
    {
        [AliasName]
        None,

        [AliasName]
        Normal,

        [AliasName]
        Tiny,
    }

    /// <summary>
    /// 通知表示管理
    /// </summary>
    public class InfoMessage
    {
        static InfoMessage() => Current = new InfoMessage();
        public static InfoMessage Current { get; }

        private InfoMessage()
        {
        }

        private ShowMessageStyle GetShowMessageStyle(InfoMessageType type)
        {
            switch (type)
            {
                default:
                case InfoMessageType.Notify:
                    return Config.Current.Notice.NoticeShowMessageStyle;
                case InfoMessageType.BookName:
                    return Config.Current.Notice.BookNameShowMessageStyle;
                case InfoMessageType.Command:
                    return Config.Current.Notice.CommandShowMessageStyle;
                case InfoMessageType.Gesture:
                    return Config.Current.Notice.GestureShowMessageStyle;
                case InfoMessageType.Loading:
                    return Config.Current.Notice.NowLoadingShowMessageStyle;
                case InfoMessageType.ViewTransform:
                    return Config.Current.Notice.ViewTransformShowMessageStyle;
            }
        }

        public NormalInfoMessage NormalInfoMessage { get; } = new NormalInfoMessage();

        public TinyInfoMessage TinyInfoMessage { get; } = new TinyInfoMessage();

        private void SetMessage(ShowMessageStyle style, string message, string tinyMessage = null, double dispTime = 1.0, BookMementoType bookmarkType = BookMementoType.None)
        {
            switch (style)
            {
                case ShowMessageStyle.Normal:
                    this.NormalInfoMessage.SetMessage(message, dispTime, bookmarkType);
                    break;
                case ShowMessageStyle.Tiny:
                    this.TinyInfoMessage.SetMessage(tinyMessage ?? message);
                    break;
            }
        }

        public void SetMessage(InfoMessageType type, string message, string tinyMessage = null, double dispTime = 1.0, BookMementoType bookmarkType = BookMementoType.None)
        {
            SetMessage(GetShowMessageStyle(type), message, tinyMessage, dispTime, bookmarkType);
        }

        public void ClearMessage(ShowMessageStyle style)
        {
            SetMessage(style, "");
        }

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(ShowMessageStyle.Normal)]
            public ShowMessageStyle NoticeShowMessageStyle { get; set; }

            [DataMember, DefaultValue(ShowMessageStyle.Normal)]
            public ShowMessageStyle BookNameShowMessageStyle { get; set; }

            [DataMember, DefaultValue(ShowMessageStyle.Normal)]
            public ShowMessageStyle CommandShowMessageStyle { get; set; }

            [DataMember, DefaultValue(ShowMessageStyle.Normal)]
            public ShowMessageStyle GestureShowMessageStyle { get; set; }

            [DataMember, DefaultValue(ShowMessageStyle.Normal)]
            public ShowMessageStyle NowLoadingShowMessageStyle { get; set; }

            [DataMember, DefaultValue(ShowMessageStyle.None)]
            public ShowMessageStyle ViewTransformShowMessageStyle { get; set; }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                config.Notice.NoticeShowMessageStyle = NoticeShowMessageStyle;
                config.Notice.BookNameShowMessageStyle = BookNameShowMessageStyle;
                config.Notice.CommandShowMessageStyle = CommandShowMessageStyle;
                config.Notice.GestureShowMessageStyle = GestureShowMessageStyle;
                config.Notice.NowLoadingShowMessageStyle = NowLoadingShowMessageStyle;
                config.Notice.ViewTransformShowMessageStyle = ViewTransformShowMessageStyle;
            }
        }

        #endregion

    }
}
