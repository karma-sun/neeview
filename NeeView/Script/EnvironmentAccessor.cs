namespace NeeView
{
    public class EnvironmentAccessor
    {
        [WordNodeMember]
        public string NeeViewPath
        {
            get
            {
                return NeeView.Environment.AssemblyLocation;
            }
        }

        [WordNodeMember]
        public string UserSettingFilePath
        {
            get
            {
                return SaveData.Current.UserSettingFilePath;
            }
        }


        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());

            return node;
        }
    }
}
