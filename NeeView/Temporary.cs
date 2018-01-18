// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace NeeView
{
    public static class Temporary
    {
        // static constructor
        static Temporary()
        {
            var assembly = Assembly.GetExecutingAssembly();

            //AssemblyCompanyの取得
            var asmcmp = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute));
            //AssemblyProductの取得
            var asmprd = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));

            //ProcessIDの取得
            var processId = Process.GetCurrentProcess().Id;
            //Process名の取得
            var processName = Process.GetCurrentProcess().ProcessName;

            TempDirectoryBaseName = processName; //  asmprd.Product;
            TempDirectory = Path.Combine(Path.GetTempPath(), TempDirectoryBaseName) + processId.ToString();
            TempDownloadDirectory = Path.Combine(Temporary.TempDirectory, "(一時フォルダー)");
            TempSystemDirectory = Path.Combine(Temporary.TempDirectory, "System");
        }

        // アプリのテンポラリフォルダー(BaseName)
        public static string TempDirectoryBaseName { get; private set; }

        // アプリのテンポラリフォルダー
        public static string TempDirectory { get; private set; }

        // アプリのダウンロードテンポラリフォルダー
        public static string TempDownloadDirectory { get; private set; }

        // アプリのシステムテンポラリフォルダー
        public static string TempSystemDirectory { get; private set; }


        // テンポラリファイル名用のカウンタ
        public static int _Count = 0;

        // 排他制御用オブジェクト
        public static object _Lock = new object();

        /// <summary>
        /// テンポラリファイル名をカウンタ付きで生成
        /// マルチスレッド用
        /// </summary>
        /// <param name="prefix">ファイル名前置詞</param>
        /// <param name="ext">ファイル拡張子。例：".txt"</param>
        /// <returns>ユニークなテンポラリファイル名</returns>
        public static string CreateCountedTempFileName(string prefix, string ext)
        {
            lock (_Lock)
            {
                _Count = (_Count + 1) % 10000;
                return CreateTempFileName(string.Format("{0}{1:0000}{2}", prefix, _Count, ext));
            }
        }

        /// <summary>
        /// テンポラリファイル名を生成
        /// 既に存在するときはファイル名に数値を付けて重複を避けます
        /// </summary>
        /// <param name="name">希望するファイル名</param>
        /// <returns>テンポラリファイル名</returns>
        public static string CreateTempFileName(string name)
        {
            // 専用フォルダー作成
            Directory.CreateDirectory(TempDirectory);

            // 名前の修正
            var validName = LoosePath.ValidFileName(name);

            // ファイル名作成
            string tempFileName = Path.Combine(TempDirectory, validName);
            int count = 1;
            while (File.Exists(tempFileName) || Directory.Exists(tempFileName))
            {
                tempFileName = Path.Combine(TempDirectory, Path.GetFileNameWithoutExtension(validName) + $"-{count++}" + Path.GetExtension(validName));
            }

            return tempFileName;
        }


        /// <summary>
        /// アプリのテンポラリフォルダーを削除
        /// アプリ終了時に呼ばれます
        /// </summary>
        public static void RemoveTempFolder()
        {
            try
            {
                var name = Process.GetCurrentProcess().ProcessName;
                var processes = Process.GetProcessesByName(name);

                // 最後のプロセスであればすべてのテンポラリを削除する
                if (processes.Length <= 1)
                {
                    foreach (var path in Directory.GetDirectories(Path.GetDirectoryName(TempDirectory), TempDirectoryBaseName + "*"))
                    {
                        Directory.Delete(path, true);
                    }
                }
                // 自プロセスのテンポラリのみ削除する
                else
                {
                    Directory.Delete(TempDirectory, true);
                }
            }
            catch
            {
                // 例外スルー
            }
        }
    }
}
