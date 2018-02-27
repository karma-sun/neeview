using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 開発用：ログ管理
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name"></param>
        public static TraceSource CreateLogger(string name)
        {
            var _source = new TraceSource(name, SourceLevels.All);

#if DEBUG && TRACE
            _source.Listeners.Clear();

            var fileName = $"{name}.log";
            if (File.Exists(fileName)) File.Delete(fileName); // new

            TextWriterTraceListener listener = new TextWriterTraceListener($"{name}.log", $"{name}.Listner");
            //listener.TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId;
            _source.Listeners.Add(listener);
#endif

            // 自動フラッシュ
            Trace.AutoFlush = true;

            return _source;
        }
    }

}
