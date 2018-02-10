// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        Command,
        Gesture,
        Loading,
        ViewTransform,
    }

    // 通知表示の種類
    public enum ShowMessageStyle
    {
        [AliasName("表示しない")]
        None,

        [AliasName("表示する")]
        Normal,

        [AliasName("小さく表示する")]
        Tiny,
    }

    /// <summary>
    /// 
    /// </summary>
    public class InfoMessage
    {
        // System Object
        public static InfoMessage Current { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public InfoMessage()
        {
            Current = this;
        }

        [PropertyEnum("通知を", Tips = "開いたブック名の表示等の通知の表示方法を指定します")]
        public ShowMessageStyle NoticeShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyEnum("コマンド実行を", Tips = "コマンド実行メッセージの表示方法を指定します")]
        public ShowMessageStyle CommandShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyEnum("ジェスチャーを", Tips = "ジェスチャーの状態の表示方法を指定します")]
        public ShowMessageStyle GestureShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyEnum("Now Loading を", Tips = "Now Loading の表示方法を指定します")]
        public ShowMessageStyle NowLoadingShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyEnum("ビュー操作を", Tips = "ドラッグ操作によるスケール変更等の情報表示方法を指定します")]
        public ShowMessageStyle ViewTransformShowMessageStyle { get; set; } = ShowMessageStyle.None;


        //
        private ShowMessageStyle GetShowMessageStyle(InfoMessageType type)
        {
            switch (type)
            {
                default:
                case InfoMessageType.Notify:
                    return NoticeShowMessageStyle;
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
        public class Memento
        {
            [DataMember]
            public ShowMessageStyle NoticeShowMessageStyle { get; set; }

            [DataMember]
            public ShowMessageStyle CommandShowMessageStyle { get; set; }

            [DataMember]
            public ShowMessageStyle GestureShowMessageStyle { get; set; }

            [DataMember]
            public ShowMessageStyle NowLoadingShowMessageStyle { get; set; }

            [DataMember]
            public ShowMessageStyle ViewTransformShowMessageStyle { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.NoticeShowMessageStyle = this.NoticeShowMessageStyle;
            memento.CommandShowMessageStyle = this.CommandShowMessageStyle;
            memento.GestureShowMessageStyle = this.GestureShowMessageStyle;
            memento.NowLoadingShowMessageStyle = this.NowLoadingShowMessageStyle;
            memento.ViewTransformShowMessageStyle = this.ViewTransformShowMessageStyle;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.NoticeShowMessageStyle = memento.NoticeShowMessageStyle;
            this.CommandShowMessageStyle = memento.CommandShowMessageStyle;
            this.GestureShowMessageStyle = memento.GestureShowMessageStyle;
            this.NowLoadingShowMessageStyle = memento.NowLoadingShowMessageStyle;
            this.ViewTransformShowMessageStyle = memento.ViewTransformShowMessageStyle;
        }
        #endregion

    }
}
