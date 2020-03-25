using NeeView.Windows.Property;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    public class SoundPlayerService
    {
        public static SoundPlayerService Current { get; } = new SoundPlayerService();

        private DateTime _lastTime;

#if false
        private string _seCannotMove;
        [PropertyPath("@ParamSeCannotMove", Filter ="Wave|*.wav")]
        public string SeCannotMove
        {
            get { return _seCannotMove; }
            set { _seCannotMove = string.IsNullOrWhiteSpace(value) ? null : value; }
        }
#endif

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

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.SeCannotMove = Config.Current.Book.TerminalSound;
            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            ////this.SeCannotMove = memento.SeCannotMove;
        }

#endregion

    }

}
