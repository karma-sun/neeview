using System;
using System.Collections.Generic;

namespace NeeView
{
    public class DialogService
    {
        private Dictionary<string, IShowDialog> _map = new Dictionary<string, IShowDialog>();


        public void Register(string key, IShowDialog dialogService)
        {
            _map[key] = dialogService;
        }


        public bool? ShowDialog(string key, object parameter)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (parameter is null) throw new ArgumentNullException(nameof(parameter));

            var dialog = _map[key];
            var dialogResult = dialog.ShowDialog(parameter);

            return dialogResult;
        }
    }

    public interface IShowDialog
    {
        bool? ShowDialog(object parameter);
    }

}
