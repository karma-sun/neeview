using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NeeView
{
    /// <summary>
    /// スクリプトの開発環境。
    /// ErrorLevelに応じて例外処理を切り替える。
    /// </summary>
    public class ScriptAccessDiagnostics : IAccessDiagnostics
    {
        public T Throw<T>(Exception ex)
        {
            Throw(ex);
            return default(T);
        }

        public object Throw(Exception ex, Type type)
        {
            Throw(ex);
            return type.GetDefaultValue();
        }

        public void Throw(Exception ex)
        {
            var _engine = JavascroptEngineMap.Current.GetCurrentEngine();

            var message = _engine.CreateScriptErrorMessage(ex.Message);

            switch (Config.Current.Script.ErrorLevel)
            {
                case ScriptErrorLevel.Info:
                    ConsoleWindowManager.Current.InforMessage(message.ToString(), false);
                    break;

                case ScriptErrorLevel.Warning:
                    ConsoleWindowManager.Current.WarningMessage(message.ToString(), _engine.IsToastEnable);
                    break;

                case ScriptErrorLevel.Error:
                default:
                    throw new ScriptException(message, ex);
            }
        }

    }

}
