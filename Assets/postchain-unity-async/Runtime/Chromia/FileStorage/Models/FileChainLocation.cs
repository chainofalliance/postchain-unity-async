namespace Chromia.Postchain.Fs
{
    public struct ChunkLocation
    {
        public int Idx { set; get; }
        public string Hash { set; get; }
        public string Brid { set; get; }
        public string Location { set; get; }
    }
}