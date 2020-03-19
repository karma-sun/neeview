using NeeLaboratory.ComponentModel;
using NeeView.Data;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView.Setting
{
    public class CommandParameterViewModel : BindableBase
    {
        private Dictionary<string, CommandElement.MementoV2> _sources;
        private string _key;

        private CommandParameter _defaultParameter;
        private PropertyDocument _propertyDocument;


        public PropertyDocument PropertyDocument
        {
            get { return _propertyDocument; }
            set { if (_propertyDocument != value) { _propertyDocument = value; RaisePropertyChanged(); } }
        }

        public string Note { get; private set; }



        public CommandParameterViewModel(CommandTable.CommandCollection memento, string key)
        {
            _sources = memento.Items;
            _key = key;

            if (CommandTable.Current.GetElement(_key).Share != null)
            {
                _key = CommandTable.Current.GetElement(_key).Share.Name;
                this.Note = string.Format(Properties.Resources.ParamCommandShare, CommandTable.Current.GetElement(_key).Text);
            }

            _defaultParameter = CommandTable.Current.GetElement(_key).ParameterSource?.GetDefault();
            if (_defaultParameter == null)
            {
                return;
            }

            ////var parameter = _sources[_key].Parameter != null
            ////    ? (CommandParameter)Json.Deserialize(_sources[_key].Parameter, _defaultParameter.GetType())
            ////    : _defaultParameter.Clone();
            var parameter = (CommandParameter)(_sources[_key].Parameter ?? _defaultParameter)?.Clone();

            _propertyDocument = new PropertyDocument(parameter);
        }

        public void Flush()
        {
            if (_propertyDocument != null)
            {
                ////_sources[_key].Parameter = Json.Serialize(_propertyDocument.Source, _propertyDocument.Source.GetType());
                _sources[_key].Parameter = (CommandParameter)_propertyDocument.Source;
            }
        }

        public void Reset()
        {
            if (_propertyDocument != null)
            {
                _propertyDocument.Set(_defaultParameter);
                RaisePropertyChanged(nameof(PropertyDocument));
            }
        }
    }
}
