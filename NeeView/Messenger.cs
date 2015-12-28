using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public object Parameter { get; set; }
        public bool? Result { get; set; }
        
        public MessageEventArgs()
        {
        }

        public MessageEventArgs(string message)
        {
            Message = message;
        }
    }

    public delegate void MessageEventHandler(object sender, MessageEventArgs e);
    
    public class Messenger
    {
        public static event MessageEventHandler MessageEventHandler;

        private static Dictionary<string, MessageEventHandler> _Handles = new Dictionary<string, MessageEventHandler>();

        public static void Send(object sender, MessageEventArgs message)
        {
            MessageEventHandler?.Invoke(sender, message);
        }

        public static void Initialize()
        {
            MessageEventHandler += Sender;
        }

        private static void Sender(object sender, MessageEventArgs e)
        {
            MessageEventHandler handle;
            if (_Handles.TryGetValue(e.Message, out handle))
            {
                handle(sender, e);
            }
        }

        public static void AddReciever(string key, MessageEventHandler handle)
        {
            _Handles[key] = handle;
        }
    }


    public class MessageBoxParams
    {
        public string MessageBoxText;
        public string Caption;
        public MessageBoxButton Button;
        public MessageBoxImage Icon;
    }
}
