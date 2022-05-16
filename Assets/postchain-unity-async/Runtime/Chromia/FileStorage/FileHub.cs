using System.Collections.Generic;
using Chromia.Postchain.Ft3;
using System.Collections;
using Chromia.Postchain;
using UnityEngine;
using System;

using Cysharp.Threading.Tasks;

namespace Chromia.Postchain.Fs
{
    public class FileHub : MonoBehaviour
    {
        private static UniTask<Client.PostchainResponse<string>> GetChunkDataByHash(
            FileChain fileChain, string hash)
        {
            return fileChain.GetChunkDataByHash(hash);
        }

        [SerializeField] private GameObject fileChainContainer;

        public Blockchain Blockchain { get; private set; }

        public async UniTask<Blockchain> Establish(string nodeUrl, string brid)
        {
            Chromia.Postchain.Ft3.Postchain postchain = new Chromia.Postchain.Ft3.Postchain(nodeUrl);
            Blockchain = await postchain.Blockchain(brid);
            return Blockchain;
        }

        public async UniTask<Blockchain> Establish(string nodeUrl, int chainId)
        {
            Chromia.Postchain.Ft3.Postchain postchain = new Chromia.Postchain.Ft3.Postchain(nodeUrl);
            Blockchain = await postchain.Blockchain(chainId);
            return Blockchain;
        }

        /**
        * Stores a file. Contacts the Filehub and allocates a chunk, and then persists the data in the correct filechain.
        */
        public async UniTask StoreFile(User user, FsFile file)
        {
            //[OK] File already stored possible needs to be catched
            var res = await ExecuteOperation(
                user,
                Operation.Op("fs.allocate_file", user.AuthDescriptor.ID, file.Hash, file.Size)
            );

            if (!res.Error)
            {
                Debug.Log("file allocated");
                await this.StoreChunks(user, file);
            }
            else
            {
                Debug.LogWarning(res.ErrorMessage);
            }
        }
        /**
        * Retrieves a file by its hash.
        *
        * @param passphrase optional options for retrieving file.
        */
        public async UniTask<Client.PostchainResponse<FsFile>> GetFile(byte[] hash)
        {
            ChunkLocation[] fileChainLocations = await GetChunkLocations(hash);

            var chunkIndexes = new List<ChunkIndex>();
            foreach (var chunkLocation in fileChainLocations)
            {
                Debug.LogFormat("Getting chunk {0} from filechain {1}", chunkLocation.Hash, chunkLocation.Location);
                var fileChain = this.InitFileChainClient(chunkLocation.Location, chunkLocation.Brid);

                var chunkIndex = await this.GetChunk(fileChain, chunkLocation);

                if (!chunkIndex.Error)
                    chunkIndexes.Add(chunkIndex.Content);
                else
                    Debug.LogWarning("GetFile: " + chunkIndex.ErrorMessage);
            }

            if (chunkIndexes.Count > 0)
            {
                var file = FsFile.FromChunks(chunkIndexes);
                return Client.PostchainResponse<FsFile>.SuccessResponse(file);
            }
            else
            {
                return Client.PostchainResponse<FsFile>.ErrorResponse("GetFile: No ChunkIndexes found");
            }
        }

        private FileChain InitFileChainClient(string url, string brid)
        {
            Debug.Log("Initializing filechain client with brid " + brid);
            return FileChain.Create(url, brid, this.fileChainContainer);
        }

        private async UniTask StoreChunks(User user, FsFile file)
        {
            Debug.LogFormat("Storing nr of chunks: {0}", file.NumberOfChunks());

            var allocateTasks = new List<UniTask>();
            for (int i = 0; i < file.NumberOfChunks(); i++)
            {
                allocateTasks.Add(
                     this.AllocateChunk(user, file.GetChunk(i), file.Hash, i)
                );
            }

            await UniTask.WhenAll(allocateTasks);

            ChunkLocation[] filechainLocations = await this.GetChunkLocations(file.Hash);
            var persistTasks = new List<UniTask>();
            foreach (var chunkLocation in filechainLocations)
            {
                Debug.LogFormat("Storing chunk {0} in filechain {1}", chunkLocation.Hash, chunkLocation.Location);
                var fileChain = this.InitFileChainClient(chunkLocation.Location, chunkLocation.Brid);

                persistTasks.Add(
                    this.PersistChunkDataInFilechain(user, fileChain, file.GetChunk(chunkLocation.Idx))
                );
            }

            await UniTask.WhenAll(persistTasks);
        }

        private UniTask<Client.PostchainResponse<string>> PersistChunkDataInFilechain(
            User user, FileChain fileChain, byte[] data)
        {
            return fileChain.StoreChunkData(user, data);
        }

        private async UniTask<ChunkLocation[]> GetChunkLocations(byte[] hash)
        {
            var res = await this.ExecuteQuery<ChunkLocation[]>("fs.get_chunk_locations",
                new (string, object)[] { ("file_hash", Util.ByteArrayToString(hash)) });

            Debug.LogFormat("Got number of chunks: {0}", res.Content.Length);
            if (res.Content.Length < 1) throw new Exception("Did not receive enough active & online Filechains");

            return res.Content;
        }

        private async UniTask<Client.PostchainResponse<ChunkIndex>> GetChunk(
            FileChain fileChain, ChunkLocation chunkLocation)
        {
            var res = await FileHub.GetChunkDataByHash(fileChain, chunkLocation.Hash);

            if (!res.Error)
            {
                var idx = new ChunkIndex(Util.HexStringToBuffer(res.Content), chunkLocation.Idx);
                return Client.PostchainResponse<ChunkIndex>.SuccessResponse(idx);
            }

            return Client.PostchainResponse<ChunkIndex>.ErrorResponse(res.ErrorMessage);
        }

        private UniTask<Client.PostchainResponse<string>> AllocateChunk(
            User user, byte[] chunk, byte[] fileHash, int index)
        {
            var hash = Client.PostchainUtil.Sha256(chunk);

            var op = Operation.Op(
                "fs.allocate_chunk",
                user.AuthDescriptor.ID,
                fileHash,
                hash,
                chunk.Length,
                index
            );
            /* Ok error should be catched */
            return ExecuteOperation(user, op);
        }

        /**
        * Executes a operation towards the Filehub.
        *
        * @param user to sign the operation.
        * @param operation to perform.
        * @param on success callback.
        * @param on error callback.
        */
        private UniTask<Client.PostchainResponse<string>> ExecuteOperation(User user, Operation operation, bool addNop = false)
        {
            var builder = this.Blockchain.TransactionBuilder().Add(operation);

            if (addNop) builder.Add(AccountOperations.Nop());

            return builder.BuildAndSign(user).PostAndWait();
        }

        /**
        * Queries the Filehub for data.
        *
        * @param query the identifier of the query.
        * @param data to provide in the query.
        * @param on success callback.
        * @param on error callback.
        */
        private UniTask<Client.PostchainResponse<T>> ExecuteQuery<T>(string queryName, (string name, object content)[] queryObject)
        {
            return this.Blockchain.Query<T>(queryName, queryObject);
        }
    }
}