using NeeLaboratory.ComponentModel;
using NeeView.Data;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView.Setting
{
    public class CommandParameterViewModel : BindableBase
    {
        private Dictionary<CommandType, CommandElement.Memento> _sources;
        private CommandType _key;

        private CommandParameter _defaultParameter;
        private PropertyDocument _propertyDocument;


        //
        public PropertyDocument PropertyDocument
        {
            get { return _propertyDocument; }
            set { if (_propertyDocument != value) { _propertyDocument = value; RaisePropertyChanged(); } }
        }

        public string Note { get; private set; }



        public CommandParameterViewModel(CommandTable.Memento memento, CommandType key)
        {
            _sources = memento.Elements;
            _key = key;

            _defaultParameter = CommandTable.Current[key].DefaultParameter;

            if (_defaultParameter is ShareCommandParameter share)
            {
                _key = share.CommandType;
                _defaultParameter = CommandTable.Current[_key].DefaultParameter;

                this.Note = string.Format(Properties.Resources.ParamCommandShare, _key.ToDispString());
            }

            if (_defaultParameter == null)
            {
                return;
            }


            var parameter = _sources[_key].Parameter != null
                ? (CommandParameter)Json.Deserialize(_sources[_key].Parameter, _defaultParameter.GetType())
                : _defaultParameter.Clone();

            _propertyDocument = new PropertyDocument(parameter);
        }

        public void Flush()
        {
            if (_propertyDocument != null)
            {
                _sources[_key].Parameter = Json.Serialize(_propertyDocument.Source, _propertyDocument.Source.GetType());
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
