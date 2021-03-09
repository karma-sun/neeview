using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class FileInformationRecord : BindableBase
    {
        private object _value;


        public FileInformationRecord(InformationGroup group, string key, object value)
        {
            Group = group;
            Key = key;
            Value = value;
        }


        public InformationGroup Group { get; private set; }
        
        public string Key { get; private set; }

        public object Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
    }
}
