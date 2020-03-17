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

        private string _seCannotMove;
        [PropertyPath("@ParamSeCannotMove", Filter ="Wave|*.wav")]
        public string SeCannotMove
        {
            get { return _seCannotMove; }
            set { _seCannotMove = string.IsNullOrWhiteSpace(value) ? null : value; }
        }

        public void PlaySeCannotMove()
        {
            if (SeCannotMove == null) return;

            PlaySe(SeCannotMove);
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
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.SeCannotMove = this.SeCannotMove;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.SeCannotMove = memento.SeCannotMove;
        }

        #endregion

    }

}
