using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    public class PageSelectDialogResultEventArgs : EventArgs
    {
        public PageSelectDialogResultEventArgs(bool result)
        {
            Result = result;
        }

        public bool Result { get; set; }
    }

    public class PageSelectDialogViewModel : BindableBase, INotifyDataErrorInfo
    {
        #region INotifyDataErrorInfo Support
        private Dictionary<string, string> _errors = new Dictionary<string, string>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private bool SetError(string propertyName, string message)
        {
            if (message == null)
            {
                return ResetError(propertyName);
            }

            if (_errors.ContainsKey(propertyName) && _errors[propertyName] == message)
            {
                return false;
            }

            _errors[propertyName] = message;
            RaiseErrorsChanged(propertyName);
            return true;
        }

        private bool ResetError(string propertyName)
        {
            if (!_errors.ContainsKey(propertyName))
            {
                return false;
            }

            _errors.Remove(propertyName);
            RaiseErrorsChanged(propertyName);
            return true;
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            var errors = new List<string>();

            if (_errors.ContainsKey(propertyName))
            {
                errors.Add(_errors[propertyName]);
            }

            return errors;
        }

        public bool HasErrors
        {
            get { return _errors.Count > 0; }
        }
        #endregion

        private PageSelecteDialogModel _model;

        public PageSelectDialogViewModel(PageSelecteDialogModel model)
        {
            _model = model;
            _inputValue = _model.GetValue();
        }

        public event EventHandler<PageSelectDialogResultEventArgs> ChangeResult;

        public string Caption => _model.Caption;
        public string Label => _model.Label;

        private string _inputValue;
        public string InputValue
        {
            get { return _inputValue; }
            set
            {
                if (SetProperty(ref _inputValue, value))
                {
                    if (SetError(nameof(InputValue), _model.CanParse(value) ? null : "Error"))
                    {
                        DecideCommand.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        private RelayCommand _decideCommand;
        public RelayCommand DecideCommand
        {
            get
            {
                return _decideCommand = _decideCommand ?? new RelayCommand(Execute, CanExcute);

                bool CanExcute()
                {
                    return !HasErrors;
                }

                void Execute()
                {
                    if (_model.SetValue(_inputValue))
                    {
                        ChangeResult?.Invoke(this, new PageSelectDialogResultEventArgs(true));
                    }
                }
            }
        }

        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand = _cancelCommand ?? new RelayCommand(Execute);

                void Execute()
                {
                    ChangeResult?.Invoke(this, new PageSelectDialogResultEventArgs(false));
                }
            }
        }

        public void AddValue(int delta)
        {
            InputValue = _model.AddValue(InputValue, delta);
        }
    }
}
