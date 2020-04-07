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

        
        public CommandParameterSource Share => _share;


        public CommandParameter GetRaw()
        {
            return _parameter;
        }

        public CommandParameter GetDefault()
        {
            if (_share != null)
            {
                return _share.GetDefault();
            }
            else
            {
                return _defaultParameter;
            }
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

        /// <summary>
        /// パラメーターの設定
        /// </summary>
        /// <param name="value">パラメーター</param>
        /// <param name="includeShare">シェアパラメーターの場合、シェア先のパラメーターを変更する。falseの場合は何もしない</param>
        public void Set(CommandParameter value, bool includeShare)
        {
            if (_share != null)
            {
                if (includeShare)
                {
                    _share.Set(value, includeShare);
                }
            }
            else
            {
                if (_defaultParameter == null || value == null || value.GetType() != _defaultParameter.GetType())
                {
                    _parameter = null;
                }
                else
                {
                    _parameter = _defaultParameter.MemberwiseEquals(value) ? null : value;
                }
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
