using System;
using System.Collections.Generic;

namespace NeeView
{
    [Obsolete]
    public class ObsoleteCommandAccessor : ICommandAccessor, IHasObsoleteMessage
    {
        private string _name;


        public ObsoleteCommandAccessor(string commandName)
        {
            _name = commandName;
        }


        [Obsolete]
        public string ObsoleteMessage => $"Script: nv.Command.{_name} is obsolete.";

        [Obsolete]
        public bool IsShowMessage
        {
            get => throw new NotSupportedException(ObsoleteMessage);
            set => throw new NotSupportedException(ObsoleteMessage);
        }
        
        [Obsolete]
        public string MouseGesture
        {
            get => throw new NotSupportedException(ObsoleteMessage);
            set => throw new NotSupportedException(ObsoleteMessage);
        }

        [Obsolete]
        public PropertyMap Parameter => throw new NotSupportedException(ObsoleteMessage);

        [Obsolete]
        public string ShortCutKey
        {
            get => throw new NotSupportedException(ObsoleteMessage);
            set => throw new NotSupportedException(ObsoleteMessage);
        }
        
        [Obsolete]
        public string TouchGesture
        {
            get => throw new NotSupportedException(ObsoleteMessage);
            set => throw new NotSupportedException(ObsoleteMessage);
        }


        [Obsolete]
        public bool Execute(params object[] args)
        {
            throw new NotSupportedException(ObsoleteMessage);
        }

        [Obsolete]
        public CommandAccessor Patch(IDictionary<string, object> patch)
        {
            throw new NotSupportedException(ObsoleteMessage);
        }
    }
}
