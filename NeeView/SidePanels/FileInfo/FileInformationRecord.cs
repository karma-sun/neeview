using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class FileInformationRecord : BindableBase
    {
        private object _value;


        public FileInformationRecord(InformationKey key, object value)
        {
            Group = key.ToInformationGroup();
            Key = key;
            Value = value;
        }


        public InformationGroup Group { get; private set; }
        
        public InformationKey Key { get; private set; }

        public object Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
    }
}
