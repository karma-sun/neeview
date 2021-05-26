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
        private JavascriptEngine _engine;

        public ScriptAccessDiagnostics(JavascriptEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

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
            var message = _engine.CreateMessageWithLocation(ex.Message);

            switch (Config.Current.Script.ErrorLevel)
            {
                case ScriptErrorLevel.Info:
                    ConsoleWindowManager.Current.InforMessage(message, false);
                    break;

                case ScriptErrorLevel.Warning:
                    ConsoleWindowManager.Current.WarningMessage(message, _engine.IsToastEnable);
                    break;

                case ScriptErrorLevel.Error:
                default:
                    throw new ScriptException(ex.Message, ex);
            }
        }

    }
}
