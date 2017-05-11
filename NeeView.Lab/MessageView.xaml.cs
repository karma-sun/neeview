using Microsoft.Expression.Interactivity.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace NeeView.Lab
{
    /// <summary>
    /// MessageView.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageView : UserControl
    {
        private MessageViewModel _vm;
        public MessageViewModel VM => _vm;

        public MessageView()
        {
            InitializeComponent();

            _vm = new MessageViewModel();
            this.DataContext = _vm;


            var trigger2 = Interaction.GetTriggers(this.Sample);
        }


    }

    public class AutoFadeBehavior : Behavior<FrameworkElement>
    {


        public bool Trigger
        {
            get { return (bool)GetValue(TriggerProperty); }
            set { SetValue(TriggerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Trigger.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TriggerProperty =
            DependencyProperty.Register("Trigger", typeof(bool), typeof(AutoFadeBehavior), new PropertyMetadata(false, TriggerChanged));

        private static void TriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoFadeBehavior behaviour && behaviour.Trigger)
            {
                behaviour.AutoFade();
            }
        }

        protected override void OnAttached()
        {
            // nop.
        }

        protected override void OnDetaching()
        {
            // nop.
        }

        //
        private void AutoFade()
        {
            AutoFade(AssociatedObject, 1.0, 1.0);
        }

        //
        public static void AutoFade(UIElement element, double beginSec, double fadeSec)
        {
            // 既存のアニメーションを削除
            element.ApplyAnimationClock(UIElement.OpacityProperty, null);

            // 不透明度を1.0にする
            element.Opacity = 1.0;

            // 不透明度を0.0にするアニメを開始
            var ani = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(fadeSec));
            ani.BeginTime = TimeSpan.FromSeconds(beginSec);
            element.BeginAnimation(UIElement.OpacityProperty, ani);
        }
    }

    /// <summary>
    /// これが現実的？
    /// </summary>
    public class AutoFadeTriggerAction : TriggerAction<FrameworkElement>
    {
        public TimeSpan BeginTime
        {
            get { return (TimeSpan)GetValue(BeginTimeProperty); }
            set { SetValue(BeginTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BeginTimeProperty =
            DependencyProperty.Register("BeginTime", typeof(TimeSpan), typeof(AutoFadeTriggerAction), new PropertyMetadata(TimeSpan.FromSeconds(1.0)));


        public Duration FadeTime
        {
            get { return (Duration)GetValue(FadeTimeProperty); }
            set { SetValue(FadeTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FadeTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FadeTimeProperty =
            DependencyProperty.Register("FadeTime", typeof(Duration), typeof(AutoFadeTriggerAction), new PropertyMetadata(new Duration(TimeSpan.FromSeconds(1.0))));


        //
        protected override void Invoke(object parameter)
        {
            AutoFade(AssociatedObject, BeginTime, FadeTime);
        }


        /// <summary>
        /// UI要素を自動的にフェイドアウトさせる
        /// </summary>
        /// <param name="element">UI要素</param>
        /// <param name="beginTime">フェイド開始時間(秒)</param>
        /// <param name="fadeTime">フェイドアウト時間(秒)</param>
        public void AutoFade(UIElement element, TimeSpan beginTime, Duration fadeTime)
        {
            // 既存のアニメーションを削除
            element.ApplyAnimationClock(UIElement.OpacityProperty, null);

            // 不透明度を1.0にする
            element.Opacity = 1.0;

            // 不透明度を0.0にするアニメを開始
            var ani = new DoubleAnimation(1, 0, fadeTime) { BeginTime = beginTime };
            element.BeginAnimation(UIElement.OpacityProperty, ani);
        }

    }

    public class IsNotNullOrWhiteSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                return !string.IsNullOrWhiteSpace(s);
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //
    public class MessageViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

        /// <summary>
        /// Message property.
        /// </summary>
        public string Message
        {
            get { return _Message; }
            //set { if (_Message != value) { _Message = value; RaisePropertyChanged(); } }
            set { _Message = value; RaisePropertyChanged(); }
        }

        private string _Message;


        /// <summary>
        /// BeginTime property.
        /// </summary>
        public TimeSpan BeginTime
        {
            get { return _BeginTime; }
            set { if (_BeginTime != value) { _BeginTime = value; RaisePropertyChanged(); } }
        }

        private TimeSpan _BeginTime = TimeSpan.FromSeconds(3.0);
    }
}
