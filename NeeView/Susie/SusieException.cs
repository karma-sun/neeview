using System;

namespace NeeView.Susie
{
    [Serializable]
    public class SusieException : ApplicationException
    {
        public SusieException(string msg, SusiePlugin spi) : base($"[{System.IO.Path.GetFileName(spi.FileName)}] {msg}")
        {
        }
    }
}
