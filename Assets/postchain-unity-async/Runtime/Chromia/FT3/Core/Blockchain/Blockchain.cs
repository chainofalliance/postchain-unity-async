using Chromia.Postchain.Client;
using System.Collections.Generic;
using UnityEngine;
using System;

using Cysharp.Threading.Tasks;

namespace Chromia.Postchain.Ft3
{
    public class Blockchain
    {
        public readonly string Id;
        public readonly BlockchainInfo Info;
        public readonly BlockchainClient Connection;
        private readonly DirectoryService _directoryService;

        public Blockchain(string id, BlockchainInfo info, BlockchainClient connection, DirectoryService directoryService)
        {
            this.Id = id;
            this.Info = info;
            this.Connection = connection;
            this._directoryService = directoryService;
        }

        public async static UniTask<Blockchain> Initialize(string blockchainRID, DirectoryService directoryService)
        {
            var chainConnectionInfo = directoryService.GetChainConnectionInfo(blockchainRID);

            if (chainConnectionInfo == null)
                throw new Exception("Cannot find details for chain with RID: " + blockchainRID);

            GameObject goConnection = new GameObject();
            goConnection.name = "Blockchain_" + blockchainRID;
            BlockchainClient connection = goConnection.AddComponent<BlockchainClient>();

            connection.Setup(
                blockchainRID,
                chainConnectionInfo.Url
            );

            var info = await BlockchainInfo.GetInfo(connection);

            if (info.Error)
                throw new Exception(info.ErrorMessage);

            return new Blockchain(blockchainRID, info.Content, connection, directoryService);
        }

        public BlockchainSession NewSession(User user)
        {
            return new BlockchainSession(user, this);
        }

        public UniTask<PostchainResponse<Account[]>> GetAccountsByParticipantId(string id, User user)
        {
            return Account.GetByParticipantId(id, this.NewSession(user));
        }

        public UniTask<PostchainResponse<Account[]>> GetAccountsByAuthDescriptorId(string id, User user)
        {
            return Account.GetByAuthDescriptorId(id, this.NewSession(user));
        }

        public UniTask<PostchainResponse<Account>> RegisterAccount(AuthDescriptor authDescriptor, User user)
        {
            return Account.Register(authDescriptor, this.NewSession(user));
        }

        public UniTask<PostchainResponse<Asset[]>> GetAssetsByName(string name)
        {
            return Asset.GetByName(name, this);
        }

        public UniTask<PostchainResponse<Asset>> GetAssetById(string id)
        {
            return Asset.GetById(id, this);
        }

        public UniTask<PostchainResponse<Asset[]>> GetAllAssets()
        {
            return Asset.GetAssets(this);
        }

        public UniTask<PostchainResponse<string>> LinkChain(string chainId)
        {
            return this.TransactionBuilder()
                .Add(Operation.Op("ft3.xc.link_chain", chainId))
                .Build(new byte[][] { })
                .PostAndWait();
        }

        public UniTask<PostchainResponse<bool>> IsLinkedWithChain(string chainId)
        {
            return this.Query<bool>("ft3.xc.is_linked_with_chain", new (string, object)[] { ("chain_rid", chainId) });
        }

        public UniTask<PostchainResponse<string[]>> GetLinkedChainsIds()
        {
            return this.Query<string[]>("ft3.xc.get_linked_chains", null);
        }

        public async UniTask<List<Blockchain>> GetLinkedChains()
        {
            List<Blockchain> blockchains = new List<Blockchain>();

            var res = await this.GetLinkedChainsIds();
            if (res.Error)
                Debug.LogWarning(res.ErrorMessage);

            foreach (var id in res.Content)
            {
                var bc = await Blockchain.Initialize(id, this._directoryService);
                blockchains.Add(bc);
            }

            return blockchains;
        }

        public UniTask<PostchainResponse<T>> Query<T>(string queryName, (string name, object content)[] queryObject)
        {
            return this.Connection.Query<T>(queryName, queryObject);
        }

        public UniTask<PostchainResponse<string>> Call(Operation operation, User user)
        {
            return TransactionBuilder()
                .Add(operation)
                .Add(AccountOperations.Nop())
                .Build(user.AuthDescriptor.Signers.ToArray())
                .Sign(user.KeyPair)
                .PostAndWait();
        }

        public TransactionBuilder TransactionBuilder()
        {
            return new TransactionBuilder(this);
        }
    }
}