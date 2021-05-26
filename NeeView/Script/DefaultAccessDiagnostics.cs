using System;

namespace NeeView
{
    /// <summary>
    /// 標準の例外処理セット
    /// </summary>
    public class DefaultAccessDiagnostics : IAccessDiagnostics
    {
        public T Throw<T>(Exception ex)
        {
            throw ex;
        }

        public object Throw(Exception ex, Type type)
        {
            throw ex;
        }

        public void Throw(Exception ex)
        {
            throw ex;
        }
    }

}
