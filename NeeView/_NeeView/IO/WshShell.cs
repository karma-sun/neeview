// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


namespace NeeView.IO
{
    /// <summary>
    /// Windows の Shell アクセスインターフェイス
    /// </summary>
    public class WshShell
    {
        public static WshShell Current { get; private set; } = new WshShell();

        #region Fields

        private IWshRuntimeLibrary.WshShell _shell = new IWshRuntimeLibrary.WshShell();

        #endregion

        #region Properties

        public IWshRuntimeLibrary.WshShell Shell => _shell;

        #endregion

        #region Methods

        ~WshShell()
        {
            if (_shell != null)
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(_shell);
            }
        }

        #endregion
    }
}
