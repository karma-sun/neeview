// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// プロセス間通信
    /// </summary>
    public static class IpcRemote
    {
        private const string PortName = "IpcNeeView";
        private const string ObjURI = "Command";

        private static IpcServer s_server;

        /// <summary>
        /// IPCサーバ起動
        /// </summary>
        public static void BootServer(int id)
        {
            try
            {
                s_server = new IpcServer();
                s_server.Boot(id);
            }
            catch { }
        }

        /// <summary>
        /// IPCサーバ停止
        /// </summary>
        public static void Shutdown()
        {
            if (s_server != null)
            {
                s_server.Shutdown();
                s_server = null;
            }
        }

        /// <summary>
        /// IPCクライアントからの命令：LoadAs
        /// </summary>
        /// <param name="path"></param>
        public static void LoadAs(int id, string path)
        {
            var client = new IpcClient();
            client.LoadAs(id, path);
        }


        /// <summary>
        /// IPCリモートオブジェクト
        /// </summary>
        private class IpcRemoteObject : MarshalByRefObject
        {
            /// <summary>
            /// 自動的に切断されるのを回避する
            /// </summary>
            public override object InitializeLifetimeService()
            {
                return null;
            }

            //
            public void LoadAs(string path)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // ウィンドウをアクティブにする (準備)
                        // 最小化されているならば解除する
                        var window = Application.Current.MainWindow;
                        if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;

                        // ウィンドウをアクティブにする (準備)
                        // 一瞬TOPMOSTにする
                        var temp = window.Topmost;
                        window.Topmost = true;
                        window.Topmost = temp;

                        // パスの指定があれば開く
                        if (path != null)
                        {
                            CommandTable.Current[CommandType.LoadAs].Execute(this, path);
                        }

                        // ウィンドウをアクティブにする (実行)
                        window.Activate();
                    }
                    catch { }
                });
            }
        }


        /// <summary>
        /// IPCサーバー
        /// </summary>
        private class IpcServer
        {
            public IpcRemoteObject RemoteObject { get; set; }

            private IpcServerChannel _channel;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public void Boot(int id)
            {
                // サーバーチャンネルの生成
                _channel = new IpcServerChannel(PortName + id.ToString());

                // チャンネルを登録
                ChannelServices.RegisterChannel(_channel, true);

                // リモートオブジェクトを生成して公開
                RemoteObject = new IpcRemoteObject();
                RemotingServices.Marshal(RemoteObject, ObjURI, typeof(IpcRemoteObject));
            }


            /// <summary>
            /// 停止
            /// </summary>
            public void Shutdown()
            {
                if (_channel != null)
                {
                    ChannelServices.UnregisterChannel(_channel);
                    _channel = null;
                }
            }
        }


        /// <summary>
        /// IPCクライアント
        /// </summary>
        private class IpcClient
        {
            public IpcRemoteObject RemoteObject { get; set; }

            //
            public bool IsConnected => RemoteObject != null;

            //
            public IpcRemoteObject Connect(int id)
            {
                if (IsConnected) return RemoteObject;

                try
                {
                    // クライアントチャンネルの生成
                    IpcClientChannel channel = new IpcClientChannel();

                    // チャンネルを登録
                    ChannelServices.RegisterChannel(channel, true);

                    // リモートオブジェクトを取得
                    RemoteObject = Activator.GetObject(typeof(IpcRemoteObject), $"ipc://{PortName + id.ToString()}/{ObjURI}") as IpcRemoteObject;

                    return RemoteObject;
                }
                catch
                {
                    return null;
                }
            }

            //
            public void LoadAs(int id, string path)
            {
                var remote = Connect(id);
                remote.LoadAs(path);
            }
        }
    }
}
