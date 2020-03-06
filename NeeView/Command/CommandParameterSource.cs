using NeeView.Data;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class CommandParameterSource
    {
        public CommandParameterSource _share;
        private CommandParameter _defaultParameter;
        private CommandParameter _parameter;

        public CommandParameterSource()
        {
        }

        public CommandParameterSource(CommandParameter defaultParameter)
        {
            _defaultParameter = defaultParameter;
        }

        public CommandParameterSource(CommandParameterSource share)
        {
            _share = share;
        }

        public CommandParameter Get()
        {
            if (_share != null)
            {
                return _share.Get();
            }
            else
            {
                return _parameter ?? _defaultParameter;
            }
        }

        public void Set(CommandParameter value)
        {
            if (_share != null)
            {
                _share.Set(value);
            }
            else
            {
                if (_defaultParameter == null) return;
                _parameter = _defaultParameter.MemberwiseEquals(value) ? null : value;
            }
        }

        public string Store()
        {
            if (_defaultParameter != null && _parameter != null)
            {
                return Json.Serialize(_parameter, _defaultParameter.GetType());
            }
            else
            {
                return null;
            }
        }

        public void Restore(string json)
        {
            if (_defaultParameter != null && !string.IsNullOrWhiteSpace(json))
            {
                _parameter = (CommandParameter)Json.Deserialize(json, _defaultParameter.GetType());
            }
            else
            {
                _parameter = null;
            }
        }
    }
}
