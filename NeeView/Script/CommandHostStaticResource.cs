using System;
using System.Collections.Generic;

namespace NeeView
{
    public class CommandHostStaticResource
    {
        private static Lazy<CommandHostStaticResource> _instance = new Lazy<CommandHostStaticResource>();
        public static CommandHostStaticResource Current => _instance.Value;


        private ScriptAccessDiagnostics _accessDiagnostics;
        private ConfigMap _configMap;
        private CommandAccessorMap _commandAccessMap;


        public CommandHostStaticResource()
        {
            _accessDiagnostics = new ScriptAccessDiagnostics();
            _configMap = new ConfigMap(_accessDiagnostics);

            CommandTable.Current.Changed += (s, e) => UpdateCommandAccessMap();
            UpdateCommandAccessMap();
        }

        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();
        public ScriptAccessDiagnostics AccessDiagnostics => _accessDiagnostics;
        public ConfigMap ConfigMap => _configMap;
        public CommandAccessorMap CommandAccessMap => _commandAccessMap;

        private void UpdateCommandAccessMap()
        {
            _commandAccessMap = new CommandAccessorMap(CommandTable.Current, _accessDiagnostics);
        }
    }
}
