namespace NeeView
{
    /// <summary>
    /// 32bit and 64bit Susie setting
    /// </summary>
    public class SusieContextMementoCollection
    {
        static SusieContextMementoCollection() => Current = new SusieContextMementoCollection();
        public static SusieContextMementoCollection Current { get; }

        private SusieContextMementoCollection()
        {
        }

        public SusieContext.Memento SusieContextX86 { get; set; }
        public SusieContext.Memento SusieContextX64 { get; set; }

        public class Memento
        {
            public SusieContext.Memento SusieContextX86 { get; set; }
            public SusieContext.Memento SusieContextX64 { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.SusieContextX86 = this.SusieContextX86;
            memento.SusieContextX64 = this.SusieContextX64;

            if (SusieContext.Is64bitPlugin)
            {
                memento.SusieContextX64 = SusieContext.Current.CreateMemento();
            }
            else
            {
                memento.SusieContextX86 = SusieContext.Current.CreateMemento();
            }

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.SusieContextX86 = memento.SusieContextX86;
            this.SusieContextX64 = memento.SusieContextX64;

            if (SusieContext.Is64bitPlugin)
            {
                SusieContext.Current.Restore(memento.SusieContextX64);
            }
            else
            {
                SusieContext.Current.Restore(memento.SusieContextX86);
            }
        }
    }
}
