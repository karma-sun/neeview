using System;

namespace NeeView
{
    /// <summary>
    /// 例外処理のラップ
    /// </summary>
    public interface IAccessDiagnostics
    {
        T Throw<T>(Exception ex);

        object Throw(Exception ex, Type type);

        void Throw(Exception ex);
    }

}
