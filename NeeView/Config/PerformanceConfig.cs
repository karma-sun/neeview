﻿using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    [DataContract]
    public class PerformanceConfig : BindableBase
    {
        private int _cacheMemorySize = 100;
        private int _jobWorkerSize = 2;
        private Size _maximumSize = new Size(4096, 4096);


        /// <summary>
        /// キャッシュメモリサイズ (MB)
        /// </summary>
        [PropertyMember("@ParamCacheMemorySize", Tips = "@ParamCacheMemorySizeTips")]
        public int CacheMemorySize
        {
            // 64bit,32bit共用のため、設定時、取得時に最大メモリ制限をしている
            get { return Math.Min(_cacheMemorySize, GetMaxCacheMemorySize()); }
            set { SetProperty(ref _cacheMemorySize, Math.Min(value, GetMaxCacheMemorySize())); }
        }

        /// <summary>
        /// 先読みページ数
        /// </summary>
        [PropertyMember("@ParamPreLoadSize", Tips = "@ParamPreLoadSizeTips")]
        public int PreLoadSize { get; set; } = 2;

        /// <summary>
        /// JobWorker数
        /// </summary>
        [PropertyMember("@ParamJobEngineWorkerSize", Tips = "@ParamJobEngineWorkerSizeTips")]
        public int JobWorkerSize
        {
            get { return _jobWorkerSize; }
            set { SetProperty(ref _jobWorkerSize, MathUtility.Clamp(value, 1, GetMaxJobWorkerSzie())); }
        }

        // 画像処理の最大サイズ
        // リサイズフィルターで使用される。
        // IsLimitSourceSize フラグがONのときには、読み込みサイズにもこの制限が適用される
        [PropertyMember("@ParamPictureProfileMaximumSize", Tips = "@ParamPictureProfileMaximumSizeTips")]
        public Size MaximumSize
        {
            get { return _maximumSize; }
            set { SetProperty(ref _maximumSize, new Size(Math.Max(value.Width, 1024), Math.Max(value.Height, 1024))); }
        }

        // 読み込みデータのサイズ制限適用フラグ
        [PropertyMember("@ParamPictureProfileIsLimitSourceSize", Tips = "@ParamPictureProfileIsLimitSourceSizeTips")]
        public bool IsLimitSourceSize { get; set; }

        // ページ読み込み中表示
        [PropertyMember("@ParamBookIsLoadingPageVisible", Tips = "@ParamBookIsLoadingPageVisibleTips")]
        public bool IsLoadingPageVisible { get; set; } = true;

        // 事前展開サイズ上限(MB)
        [PropertyMember("@ParamSevenZipArchiverPreExtractSolidSize", Tips = "@ParamSevenZipArchiverPreExtractSolidSizeTips")]
        public int PreExtractSolidSize { get; set; } = 1000;

        // 事前展開先をメモリにする
        [PropertyMember("@ParamSevenZipArchiverIsPreExtractToMemory", Tips = "@ParamSevenZipArchiverIsPreExtractToMemoryTips")]
        public bool IsPreExtractToMemory { get; set; }

        [PropertyMember("@ParamThumbnailBookCapacity", Tips = "@ParamThumbnailBookCapacityTips")]
        public int ThumbnailBookCapacity { get; set; } = 200;

        [PropertyMember("@ParamThumbnailPageCapacity", Tips = "@ParamThumbnailPageCapacityTips")]
        public int ThumbnailPageCapacity { get; set; } = 100;



        /// <summary>
        /// 最大キャッシュメモリサイズ計算
        /// </summary>
        public int GetMaxCacheMemorySize()
        {
            int max = (int)(Environment.GetTotalPhysicalMemory() / 1024 / 1024);

            // -2GB or half size
            max = Math.Max(max - 2 * 1024, max / 2);

            // if 32bit, limit 2GB
            if (!Environment.IsX64)
            {
                max = Math.Min(max, 2 * 1024);
            }

            return max;
        }

        public int GetMaxJobWorkerSzie()
        {
            return Math.Max(4, System.Environment.ProcessorCount);
        }
    }

}