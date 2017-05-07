// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// メッセージ
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        // 受信者識別用キー
        public string Key { get; set; }

        // メッセージ
        public object Parameter { get; set; }

        // メッセージ返信
        // 受信側で設定される
        public bool? Result { get; set; }

        // コンストラクタ
        public MessageEventArgs()
        {
        }

        // コンストラクタ
        public MessageEventArgs(string key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// メッセージハンドラ デリゲート
    /// </summary>
    /// <param name="sender">イベント発行者</param>
    /// <param name="e">メッセージ</param>
    public delegate void MessageEventHandler(object sender, MessageEventArgs e);


    /// <summary>
    /// メッセンジャー
    /// </summary>
    public static class Messenger
    {
        public static event MessageEventHandler MessageEventHandler;

        private static Dictionary<string, MessageEventHandler> s_handles = new Dictionary<string, MessageEventHandler>();

        // メッセージ送信
        public static bool? Send(object sender, MessageEventArgs message)
        {
            MessageEventHandler?.Invoke(sender, message);
            return message.Result;
        }

        // メッセージ送信(IDのみ)
        public static void Send(object sender, string key)
        {
            MessageEventHandler?.Invoke(sender, new MessageEventArgs(key));
        }

        // コンストラクタ
        static Messenger()
        {
            MessageEventHandler += Sender;
        }

        // 配送者
        private static void Sender(object sender, MessageEventArgs e)
        {
            MessageEventHandler handle;
            if (s_handles.TryGetValue(e.Key, out handle))
            {
                handle(sender, e);
            }
        }

        /// <summary>
        /// 受信者登録
        /// </summary>
        /// <param name="key">識別キー</param>
        /// <param name="handle">メッセージ処理デリゲート</param>
        public static void AddReciever(string key, MessageEventHandler handle)
        {
            s_handles[key] = handle;
        }
    }


    /// <summary>
    /// 通知表示メッセージパラメータ
    /// </summary>
    public class MessageShowParams
    {
        public const double DefaultDispTime = 1.0;

        public string Text;
        public BookMementoType BookmarkType;
        public double DispTime = DefaultDispTime;

        public MessageShowParams(string text)
        {
            Text = text;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ResetHideDelayParam
    {
        public PanelSide PanelSide { get; set; }
    }
}
