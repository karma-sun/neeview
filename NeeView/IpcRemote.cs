// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
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
        const string PortName = "IpcNeeView";
        const string ObjURI = "Command";

        private static IpcServer _Server;

        /// <summary>
        /// IPCサーバ起動
        /// </summary>
        public static void BootServer(int id)
        {
            try
            {
                _Server = new IpcServer();
                _Server.Boot(id);
            }
            catch { }
        }

        /// <summary>
        /// IPCクライアントからの命令：LoadAs
        /// </summary>
        /// <param name="path"></param>
        public static void LoadAs(int id, string path)
        {
            try
            {
                var client = new IpcClient();
                client.LoadAs(id, path);
            }
            catch { }
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
                        var window = Application.Current.MainWindow;

                        // 最小化されているならば解除する
                        if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;

                        // ウィンドウをアクティブにする
                        window.Activate();

                        // パスの指定があれば開く
                        if (path != null)
                        {
                            ModelContext.CommandTable[CommandType.LoadAs].Execute(path);
                        }
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

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public void Boot(int id)
            {
                // サーバーチャンネルの生成
                IpcServerChannel channel = new IpcServerChannel(PortName + id.ToString());

                // チャンネルを登録
                ChannelServices.RegisterChannel(channel, true);

                // リモートオブジェクトを生成して公開
                RemoteObject = new IpcRemoteObject();
                RemotingServices.Marshal(RemoteObject, ObjURI, typeof(IpcRemoteObject));
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
