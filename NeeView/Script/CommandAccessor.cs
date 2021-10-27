using System.Collections.Generic;
using System.Collections.Immutable;

namespace NeeView
{
    /// <summary>
    /// コマンドアクセス
    /// </summary>
    public class CommandAccessor : ICommandAccessor
    {
        private CommandElement _command;
        private ImmutableDictionary<string, object> _patch = ImmutableDictionary<string, object>.Empty;
        private IAccessDiagnostics _accessDiagnostics;


        public CommandAccessor(CommandElement command, IAccessDiagnostics accessDiagnostics)
        {
            _command = command;
            _accessDiagnostics = accessDiagnostics ?? throw new System.ArgumentNullException(nameof(accessDiagnostics));
            Parameter = _command.Parameter != null ? new PropertyMap(_command.Parameter, _accessDiagnostics, $"nv.Command.{_command.Name}.Parameter") : null;
        }


        [WordNodeMember]
        public bool IsShowMessage
        {
            get { return _command.IsShowMessage; }
            set { _command.IsShowMessage = value; }
        }

        [WordNodeMember]
        public string ShortCutKey
        {
            get { return _command.ShortCutKey; }
            set { _command.ShortCutKey = value; }
        }

        [WordNodeMember]
        public string TouchGesture
        {
            get { return _command.TouchGesture; }
            set { _command.TouchGesture = value; }
        }

        [WordNodeMember]
        public string MouseGesture
        {
            get { return _command.MouseGesture; }
            set { _command.MouseGesture = value.Replace("←", "L").Replace("↑", "U").Replace("→", "R").Replace("↓", "L").Replace("Click", "C"); }
        }

        [WordNodeMember(IsAutoCollect = false)]
        public PropertyMap Parameter { get; }


        [WordNodeMember]
        public bool Execute(params object[] args)
        {
            var parameter = _command.CreateOverwriteCommandParameter(_patch, _accessDiagnostics);
            var context = new CommandContext(parameter, args, CommandOption.None);
            if (_command.CanExecute(this, context))
            {
                AppDispatcher.Invoke(() => _command.Execute(this, context));
                return true;
            }
            else
            {
                return false;
            }
        }

        [WordNodeMember]
        public CommandAccessor Patch(IDictionary<string, object> patch)
        {
            return Clone().AddPatch(patch);
        }


        internal CommandAccessor Clone()
        {
            return (CommandAccessor)this.MemberwiseClone();
        }

        internal CommandAccessor AddPatch(IDictionary<string, object> patch)
        {
            _patch = _patch.AddRange(patch);
            return this;
        }

        internal WordNode CreateWordNode(string commandName)
        {
            var node = WordNodeHelper.CreateClassWordNode(commandName, this.GetType());

            if (Parameter != null)
            {
                node.Children.Add(Parameter.CreateWordNode(nameof(Parameter)));
            }

            return node;
        }

    }
}
