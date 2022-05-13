using System.Collections.Generic;
using Chromia.Postchain.Client;
using Chromia.Postchain.Ft3;
using UnityEngine;

using Cysharp.Threading.Tasks;

namespace Chromia.Postchain.Fs
{
    public class FileChain : MonoBehaviour
    {
        public static List<FileChain> FileChains = new List<FileChain>();

        public static FileChain GetFileChain(string brid)
        {
            return FileChains.Find(elem => elem.Brid.Equals(brid));
        }

        public static FileChain Create(string nodeApiUrl, string brid, GameObject container)
        {
            FileChain fileChain = GetFileChain(brid);

            if (fileChain == null)
            {
                GameObject fileChainGO = new GameObject();
                fileChainGO.name = "FileChain" + brid;
                if (container != null) fileChainGO.transform.SetParent(container.transform);

                fileChain = fileChainGO.AddComponent<FileChain>();
                BlockchainClient connection = fileChainGO.AddComponent<BlockchainClient>();

                connection.Setup(
                    brid,
                    nodeApiUrl
                );

                fileChain.Client = connection;
                fileChain.Brid = brid;

                FileChains.Add(fileChain);
            }

            return fileChain;
        }

        public BlockchainClient Client;
        public string Brid;

        // TODO: Check error message. If it's not a duplicate chunk, throw error.
        // Error message is today not returned by the client.
        public UniTask<PostchainResponse<string>> StoreChunkData(User user, byte[] data)
        {
            //[OK] Chunk already stored possible
            var tx = this.Client.NewTransaction(user.AuthDescriptor.Signers.ToArray());
            tx.AddOperation("fs.add_chunk_data", Util.ByteArrayToString(data));
            var nop = AccountOperations.Nop();
            tx.AddOperation(nop.Name, nop.Args);
            tx.Sign(user.KeyPair.PrivKey, user.KeyPair.PubKey);

            return tx.PostAndWait();
        }

        public UniTask<PostchainResponse<bool>> ChunkHashExists(string hash)
        {
            return this.Client.Query<bool>("fs.chunk_hash_exists",
                new (string, object)[] { ("hash", hash) });
        }

        public UniTask<PostchainResponse<string>> GetChunkDataByHash(string hash)
        {
            return this.Client.Query<string>("fs.get_chunk",
                new (string, object)[] { ("hash", hash) });
        }
    }
}