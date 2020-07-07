namespace NeeView
{
    /// <summary>
    /// コマンドパラメーター用プロパティマップソース
    /// </summary>
    /// <remarks>
    /// 注意：パラメータークローンに対する操作になるため、複数のインスタンスで操作すると衝突する
    /// </remarks>
    public class CommandParameterPropertyMapSource
    {
        private CommandElement _command;
        private CommandParameter _parameter;

        public CommandParameterPropertyMapSource(CommandElement command)
        {
            _command = command;
            _parameter = (CommandParameter)_command.Parameter?.Clone();

            if (_parameter != null)
            {
                PropertyMap = new PropertyMap(_parameter);
                PropertyMap.PropertyChanged += PropertyMap_PropertyChanged;
            }
        }

        public PropertyMap PropertyMap { get; private set; }

        private void PropertyMap_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _command.ParameterSource.Set(_parameter, true);
        }
    }

}
