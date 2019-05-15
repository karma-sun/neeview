namespace NeeView
{
    /// <summary>
    /// 32bit and 64bit Susie setting
    /// TODO: .spiのみ対応するため、このフィルターは不要
    /// </summary>
    public class SusieContextMementoCollection
    {
        static SusieContextMementoCollection() => Current = new SusieContextMementoCollection();
        public static SusieContextMementoCollection Current { get; }

        private SusieContextMementoCollection()
        {
        }

        public SusieContext.Memento SusieContextX86 { get; set; }

        public class Memento
        {
            public SusieContext.Memento SusieContextX86 { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.SusieContextX86 = this.SusieContextX86;
            memento.SusieContextX86 = SusieContext.Current.CreateMemento();

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.SusieContextX86 = memento.SusieContextX86;
            SusieContext.Current.Restore(memento.SusieContextX86);
        }
    }
}
