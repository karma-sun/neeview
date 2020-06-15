using System;
using System.Diagnostics;

namespace NeeView
{
    public static class BookSettingPolicyConfigExtensions
    {
        public static BookSettingConfig Mix(this BookSettingPolicyConfig self, BookSettingConfig def, BookSettingConfig current, BookSettingConfig restore, bool isDefaultRecursive)
        {
            Debug.Assert(def != null);
            Debug.Assert(current != null);
            Debug.Assert(current.Page == null);

            BookSettingConfig param = new BookSettingConfig();

            var policyMap = new BookSettingPolicyConfigMap(self);

            var paramMap = new BookSettingConfigMap(param);
            var defMap = new BookSettingConfigMap(def);
            var currentMap = new BookSettingConfigMap(current);
            var restoretMap = new BookSettingConfigMap(restore);

            foreach (BookSettingKey key in Enum.GetValues(typeof(BookSettingKey)))
            {
                switch (policyMap[key])
                {
                    case BookSettingSelectMode.Default:
                        paramMap[key] = defMap[key];
                        break;

                    case BookSettingSelectMode.Continue:
                        paramMap[key] = currentMap[key];
                        break;

                    case BookSettingSelectMode.RestoreOrDefault:
                    case BookSettingSelectMode.RestoreOrDefaultReset:
                        paramMap[key] = restore != null ? restoretMap[key] : defMap[key];
                        break;

                    case BookSettingSelectMode.RestoreOrContinue:
                        paramMap[key] = restore != null ? restoretMap[key] : currentMap[key];
                        break;
                }
            }

            if (isDefaultRecursive && restore == null)
            {
                param.IsRecursiveFolder = true;
            }

            return param;
        }

    }
}
