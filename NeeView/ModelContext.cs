// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Model 共通コンテキスト (static)
    /// </summary>
    public static class ModelContext
    {
        public static JobEngine JobEngine { get; set; }

        public static SusieContext SusieContext { get; set; }
        public static Susie.Susie Susie => SusieContext.Susie;

        public static BookHistory BookHistory { get; set; }

        public static ArchiverManager ArchiverManager { get; set; }
        public static BitmapLoaderManager BitmapLoaderManager { get; set; }

        public static CommandTable CommandTable { get; set; }


        // 初期化
        public static void Initialize()
        {
            JobEngine = new JobEngine();
            JobEngine.Start();

            BookHistory = new BookHistory();

            ArchiverManager = new ArchiverManager();
            BitmapLoaderManager = new BitmapLoaderManager();

            CommandTable = new CommandTable();

            SusieContext = new SusieContext();
        }


        // 終了処理
        public static void Terminate()
        {
            JobEngine.Dispose();
        }
    }


}

