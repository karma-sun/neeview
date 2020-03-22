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
    //
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
        [AliasName("@EnumShowMessageStyleNone")]
        None,

        [AliasName("@EnumShowMessageStyleNormal")]
        Normal,

        [AliasName("@EnumShowMessageStyleTiny")]
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

        [PropertyMember("@ParamInfoMessageNoticeShowMessageStyle")]
        public ShowMessageStyle NoticeShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyMember("@ParamInfoBookNameShowMessageStyle")]
        public ShowMessageStyle BookNameShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyMember("@ParamInfoMessageCommandShowMessageStyle")]
        public ShowMessageStyle CommandShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyMember("@ParamInfoMessageGestureShowMessageStyle")]
        public ShowMessageStyle GestureShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyMember("@ParamInfoMessageNowLoadingShowMessageStyle")]
        public ShowMessageStyle NowLoadingShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyMember("@ParamInfoMessageViewTransformShowMessageStyle")]
        public ShowMessageStyle ViewTransformShowMessageStyle { get; set; } = ShowMessageStyle.None;


        //
        private ShowMessageStyle GetShowMessageStyle(InfoMessageType type)
        {
            switch (type)
            {
                default:
                case InfoMessageType.Notify:
                    return NoticeShowMessageStyle;
                case InfoMessageType.BookName:
                    return BookNameShowMessageStyle;
                case InfoMessageType.Command:
                    return CommandShowMessageStyle;
                case InfoMessageType.Gesture:
                    return GestureShowMessageStyle;
                case InfoMessageType.Loading:
                    return NowLoadingShowMessageStyle;
                case InfoMessageType.ViewTransform:
                    return ViewTransformShowMessageStyle;
            }
        }




        //
        public NormalInfoMessage NormalInfoMessage { get; } = new NormalInfoMessage();

        //
        public TinyInfoMessage TinyInfoMessage { get; } = new TinyInfoMessage();

        //
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

        //
        public void SetMessage(InfoMessageType type, string message, string tinyMessage = null, double dispTime = 1.0, BookMementoType bookmarkType = BookMementoType.None)
        {
            SetMessage(GetShowMessageStyle(type), message, tinyMessage, dispTime, bookmarkType);
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
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.NoticeShowMessageStyle = this.NoticeShowMessageStyle;
            memento.BookNameShowMessageStyle = this.BookNameShowMessageStyle;
            memento.CommandShowMessageStyle = this.CommandShowMessageStyle;
            memento.GestureShowMessageStyle = this.GestureShowMessageStyle;
            memento.NowLoadingShowMessageStyle = this.NowLoadingShowMessageStyle;
            memento.ViewTransformShowMessageStyle = this.ViewTransformShowMessageStyle;

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.NoticeShowMessageStyle = memento.NoticeShowMessageStyle;
            this.BookNameShowMessageStyle = memento.BookNameShowMessageStyle;
            this.CommandShowMessageStyle = memento.CommandShowMessageStyle;
            this.GestureShowMessageStyle = memento.GestureShowMessageStyle;
            this.NowLoadingShowMessageStyle = memento.NowLoadingShowMessageStyle;
            this.ViewTransformShowMessageStyle = memento.ViewTransformShowMessageStyle;
        }
        #endregion

    }
}
