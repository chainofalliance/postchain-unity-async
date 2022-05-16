using System.Collections.Generic;
using Chromia.Postchain.Client;
using System.Linq;
using System.IO;
using System;

namespace Chromia.Postchain.Fs
{
    public class FsFile
    {
        public static FsFile FromLocalFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
            var data = File.ReadAllBytes(path);

            return new FsFile(data);
        }

        public static FsFile FromData(byte[] data)
        {
            return new FsFile(data);
        }

        public static FsFile FromChunks(List<ChunkIndex> chunks)
        {
            IEnumerable<ChunkIndex> ordered = chunks.OrderBy(chunk => chunk.Index);

            return new FsFile(
                ordered.SelectMany((elem) => elem.Data).ToArray()
            );
        }

        private static List<byte[]> SliceIntoChunks(byte[] data)
        {
            var nrOfChunks = Math.Ceiling((decimal)data.Length / FsFile.BYTES);
            var chunks = new List<byte[]>();

            for (int i = 0; i < nrOfChunks; i++)
            {
                var offset = i * FsFile.BYTES;
                var chunkSize = FsFile.BYTES;

                if ((data.Length - offset) < FsFile.BYTES) chunkSize = data.Length - offset;
                var subset = new ArraySegment<byte>(data, offset, chunkSize);
                chunks.Add(subset.ToArray());
            }

            return chunks;
        }

        private static int BYTES = 1000000;


        public readonly byte[] Hash;
        public readonly List<byte[]> Chunks;
        public readonly int Size;
        public readonly byte[] Data;

        private FsFile(byte[] data)
        {
            this.Hash = PostchainUtil.Sha256(data);
            this.Data = data;
            this.Chunks = FsFile.SliceIntoChunks(data);
            this.Size = data.Length;
        }

        public byte[] GetChunk(int index)
        {
            return this.Chunks[index];
        }

        public decimal NumberOfChunks()
        {
            return Math.Ceiling((decimal)this.Size / FsFile.BYTES);
        }
    }
}