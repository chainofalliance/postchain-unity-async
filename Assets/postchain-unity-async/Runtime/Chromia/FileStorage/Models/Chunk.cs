namespace Chromia.Postchain.Fs
{
    public interface IChunkHashIndex
    {
        string Hash { get; }
        int Index { get; }
    }

    public class ChunkHashIndex : IChunkHashIndex
    {
        private string hash;
        private int index;

        public ChunkHashIndex(string hash, int index)
        {
            this.hash = hash;
            this.index = index;
        }

        public string Hash
        {
            get { return hash; }
        }

        public int Index
        {
            get { return index; }
        }
    }

    public interface IChunkHashFilechain
    {
        byte[] Hash { get; }
        string Brid { get; }
        string Location { get; }
    }

    public class ChunkIndex
    {
        public readonly byte[] Data;
        public readonly int Index;

        public ChunkIndex(byte[] data, int index)
        {
            this.Data = data;
            this.Index = index;
        }
    }
}