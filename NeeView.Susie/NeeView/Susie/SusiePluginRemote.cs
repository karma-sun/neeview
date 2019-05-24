using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NeeView.Susie
{
    public static class SusiePluginRemote
    {
        public static string CreateServerName(Process process)
        {
            return $"nv{process.Id}.rpc";
        }
    }
}
