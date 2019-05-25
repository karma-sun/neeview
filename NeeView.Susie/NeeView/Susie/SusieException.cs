using System;

namespace NeeView.Susie
{
    [Serializable]
    public class SusieException : ApplicationException
    {
        public SusieException(string msg) : base(msg)
        {
        }

        public SusieException(string msg, string spi) : base($"[{spi}] {msg}")
        {
        }
    }
}
