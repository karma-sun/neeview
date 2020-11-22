using NeeView.Windows.Property;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    public class SoundPlayerService
    {
        static SoundPlayerService() => Current = new SoundPlayerService();
        public static SoundPlayerService Current { get; }


        private DateTime _lastTime;


        public void PlaySeCannotMove()
        {
            if (Config.Current.Book.TerminalSound == null) return;

            PlaySe(Config.Current.Book.TerminalSound);
        }

        private void PlaySe(string path)
        {
            try
            {
                if ((DateTime.Now - _lastTime).TotalMilliseconds > 100)
                {
                    using (var player = new System.Media.SoundPlayer(path))
                    {
                        player.Play();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            _lastTime = DateTime.Now;
        }

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public string SeCannotMove { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Book.TerminalSound = SeCannotMove;
            }
        }

        #endregion

    }

}
