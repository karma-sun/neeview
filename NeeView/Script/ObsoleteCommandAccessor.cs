using System;
using System.Collections.Generic;

namespace NeeView
{
    [Obsolete]
    public class ObsoleteCommandAccessor : ICommandAccessor, IHasObsoleteMessage
    {
        private string _name;
        private IAccessDiagnostics _accessDiagnostics;

        public ObsoleteCommandAccessor(string commandName, string replaceName, IAccessDiagnostics accessDiagnostics)
        {
            _name = commandName;
            _accessDiagnostics = accessDiagnostics;
            ObsoleteMessage = $"nv.Command.{_name} is obsolete." + (replaceName != null ? $" Use {replaceName} instead." : "");
        }


        [Obsolete]
        public string ObsoleteMessage { get; } 

        [Obsolete]
        public bool IsShowMessage
        {
            get => _accessDiagnostics.Throw<bool>(new NotSupportedException(ObsoleteMessage));
            set => _accessDiagnostics.Throw(new NotSupportedException(ObsoleteMessage));
        }
        
        [Obsolete]
        public string MouseGesture
        {
            get => _accessDiagnostics.Throw<string>(new NotSupportedException(ObsoleteMessage));
            set => _accessDiagnostics.Throw(new NotSupportedException(ObsoleteMessage));
        }

        [Obsolete]
        public PropertyMap Parameter => _accessDiagnostics.Throw<PropertyMap>(new NotSupportedException(ObsoleteMessage));

        [Obsolete]
        public string ShortCutKey
        {
            get => _accessDiagnostics.Throw<string>(new NotSupportedException(ObsoleteMessage));
            set => _accessDiagnostics.Throw(new NotSupportedException(ObsoleteMessage));
        }
        
        [Obsolete]
        public string TouchGesture
        {
            get => _accessDiagnostics.Throw<string>(new NotSupportedException(ObsoleteMessage));
            set => _accessDiagnostics.Throw(new NotSupportedException(ObsoleteMessage));
        }


        [Obsolete]
        public bool Execute(params object[] args)
        {
            return _accessDiagnostics.Throw<bool>(new NotSupportedException(ObsoleteMessage));
        }

        [Obsolete]
        public CommandAccessor Patch(IDictionary<string, object> patch)
        {
            return _accessDiagnostics.Throw<CommandAccessor>(new NotSupportedException(ObsoleteMessage));
        }
    }
}
