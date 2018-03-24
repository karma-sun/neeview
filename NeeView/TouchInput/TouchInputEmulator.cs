using System;
using System.Windows.Input;

namespace NeeView
{
    public class TouchInputEmulator : TouchInputBase
    {
        public TouchInputEmulator(TouchInputContext context) : base(context)
        {
        }

        public void Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var point = Mouse.GetPosition(_context.Sender);
            ExecuteTouchGesture(sender, e, point);
        }

        public override void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void OnStylusMove(object sender, StylusEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void OnStylusUp(object sender, StylusEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
