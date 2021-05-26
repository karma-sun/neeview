using System;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// スクリプトの実行時例外。
    /// Obsoleteなプロパティの呼び出し等、仕様上の例外をCLRからエンジンに投げるときに使用する。
    /// </summary>
    public class ScriptException : Exception
    {
        public ScriptException()
        {
        }

        public ScriptException(string message) : base(message)
        {
        }

        public ScriptException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ScriptException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}
