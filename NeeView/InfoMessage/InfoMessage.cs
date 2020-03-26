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

#if false
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
#endif

        //
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
                config.Notice.NoticeShowMessageStyle = NoticeShowMessageStyle;
                config.Notice.BookNameShowMessageStyle = BookNameShowMessageStyle;
                config.Notice.CommandShowMessageStyle = CommandShowMessageStyle;
                config.Notice.GestureShowMessageStyle = GestureShowMessageStyle;
                config.Notice.NowLoadingShowMessageStyle = NowLoadingShowMessageStyle;
                config.Notice.ViewTransformShowMessageStyle = ViewTransformShowMessageStyle;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.NoticeShowMessageStyle = Config.Current.Notice.NoticeShowMessageStyle;
            memento.BookNameShowMessageStyle = Config.Current.Notice.BookNameShowMessageStyle;
            memento.CommandShowMessageStyle = Config.Current.Notice.CommandShowMessageStyle;
            memento.GestureShowMessageStyle = Config.Current.Notice.GestureShowMessageStyle;
            memento.NowLoadingShowMessageStyle = Config.Current.Notice.NowLoadingShowMessageStyle;
            memento.ViewTransformShowMessageStyle = Config.Current.Notice.ViewTransformShowMessageStyle;

            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            ////this.NoticeShowMessageStyle = memento.NoticeShowMessageStyle;
            ////this.BookNameShowMessageStyle = memento.BookNameShowMessageStyle;
            ////this.CommandShowMessageStyle = memento.CommandShowMessageStyle;
            ////this.GestureShowMessageStyle = memento.GestureShowMessageStyle;
            ////this.NowLoadingShowMessageStyle = memento.NowLoadingShowMessageStyle;
            ////this.ViewTransformShowMessageStyle = memento.ViewTransformShowMessageStyle;
        }
#endregion

    }
}
