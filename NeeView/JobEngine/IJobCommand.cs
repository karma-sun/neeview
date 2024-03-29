﻿using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// JobCommand
    /// </summary>
    public interface IJobCommand
    {
        // メイン処理
        void Execute(CancellationToken token);
    }
}
