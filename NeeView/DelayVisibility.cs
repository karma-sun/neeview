using NeeLaboratory.ComponentModel;
using NeeView.Windows.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class DelayVisibility : BindableBase
    {
        #region Fields

        private DelayValue<Visibility> _visibility;

        #endregion

        #region Constructors

        public DelayVisibility()
        {
            _visibility = new DelayValue<Visibility>(Visibility.Collapsed);
            _visibility.ValueChanged += (s, e) =>
            {
                Changed?.Invoke(s, e);
                RaisePropertyChanged(nameof(Visibility));
            };
        }

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Properties

        public Visibility Visibility
        {
            get { return Get(); }
            set { Set(value); }
        }

        public double DefaultDelayTime { get; set; } = 1.0;

        #endregion

        #region Methods

        public Visibility Get()
        {
            return _visibility.Value;
        }

        public void Set(Visibility visibility)
        {
            var delay = this.DefaultDelayTime * 1000;
            _visibility.SetValue(visibility, visibility == Visibility.Visible ? 0 : delay);
        }

        public void SetDelayVisibility(Visibility visibility, int ms)
        {
            _visibility.SetValue(visibility, visibility == Visibility.Visible ? 0 : ms);
        }

        public string ToDetail()
        {
            return _visibility.ToDetail();
        }

        #endregion
    }
}
