using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// スレッドIDとエンジンの対応表
    /// </summary>
    public class JavascroptEngineMap
    {
        static JavascroptEngineMap() => Current = new JavascroptEngineMap();
        public static JavascroptEngineMap Current { get; }

        private JavascroptEngineMap()
        {
        }


        private ConcurrentDictionary<int, JavascriptEngine> _map = new ConcurrentDictionary<int, JavascriptEngine>();


        public void Add(JavascriptEngine engine)
        {
            if (engine is null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            //Debug.WriteLine($"> JavascriptEngine.{id}: add");
            Debug.Assert(!_map.ContainsKey(id));
            var result = _map.TryAdd(id, engine);
            Debug.Assert(result);
        }

        public void Remove(JavascriptEngine engine)
        {
            if (engine is null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            //Debug.WriteLine($"> JavascriptEngine.{id}: remove");
            var result = _map.TryRemove(id, out var target);
            Debug.Assert(result && target == engine);
        }

        public JavascriptEngine GetCurrentEngine()
        {
            int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            //Debug.WriteLine($"> JavascriptEngine.{id}: access");
            _map.TryGetValue(id, out var engine);
            Debug.Assert(engine != null);
            return engine;
        }

    }

}
