namespace NeeLaboratory
{
    public class Chunk
    {
        public Chunk(int id, byte[] data)
        {
            Id = id;
            Data = data;
        }

        public int Id { get; set; }
        public byte[] Data { get; set; }

        public int Length => Data != null ? Data.Length : 0;
    }
}
