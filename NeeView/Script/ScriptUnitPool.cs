using System;
using System.Collections.Generic;

namespace NeeView
{
    public class ScriptUnitPool
    {
        private List<ScriptUnit> _units = new List<ScriptUnit>();
        private object _lock = new object();

        public ScriptUnit Run(object sender, string script)
        {
            var unit = new ScriptUnit(this);
            Add(unit);
            unit.Execute(sender, script);
            return unit;
        }

        public void Add(ScriptUnit unit)
        {
            if (unit is null) throw new ArgumentNullException();

            lock (_lock)
            {
                _units.Add(unit);
            }
        }

        public void Remove(ScriptUnit unit)
        {
            lock (_lock)
            {
                _units.Remove(unit);
            }
        }

        public void CancelAll()
        {
            lock (_lock)
            {
                foreach (var item in _units)
                {
                    item.Cancel();
                }
            }
        }

    }
}
