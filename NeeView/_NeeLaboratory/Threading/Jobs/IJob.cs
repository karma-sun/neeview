// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Jobs
{
    /// <summary>
    /// Jobインターフェイス
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Jobの実行処理
        /// </summary>
        Task ExecuteAsync();
    }
}
