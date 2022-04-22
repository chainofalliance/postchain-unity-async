using Chromia.Postchain.Client;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

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

        public static IEnumerator Initialize(string blockchainRID, DirectoryService directoryService, Action<Blockchain> onSuccess, Action<string> onError)
        {
            var chainConnectionInfo = directoryService.GetChainConnectionInfo(blockchainRID);

            if (chainConnectionInfo == null)
            {
                throw new Exception("Cannot find details for chain with RID: " + blockchainRID);
            }

            GameObject goConnection = new GameObject();
            goConnection.name = "Blockchain_" + blockchainRID;
            BlockchainClient connection = goConnection.AddComponent<BlockchainClient>();

            connection.Setup(
                blockchainRID,
                chainConnectionInfo.Url
            );

            yield return BlockchainInfo.GetInfo(connection,
                (BlockchainInfo info) => onSuccess(new Blockchain(blockchainRID, info, connection, directoryService)), onError);
        }

        public BlockchainSession NewSession(User user)
        {
            return new BlockchainSession(user, this);
        }

        public IEnumerator GetAccountsByParticipantId(string id, User user, Action<Account[]> onSuccess, Action<string> onError)
        {
            yield return Account.GetByParticipantId(id, this.NewSession(user), onSuccess, onError);
        }

        public IEnumerator GetAccountsByAuthDescriptorId(string id, User user, Action<Account[]> onSuccess, Action<string> onError)
        {
            yield return Account.GetByAuthDescriptorId(id, this.NewSession(user), onSuccess, onError);
        }

        public IEnumerator RegisterAccount(AuthDescriptor authDescriptor, User user, Action<Account> onSuccess, Action<string> onError)
        {
            yield return Account.Register(authDescriptor, this.NewSession(user), onSuccess, onError);
        }

        public IEnumerator GetAssetsByName(string name, Action<Asset[]> onSuccess, Action<string> onError)
        {
            yield return Asset.GetByName(name, this, onSuccess, onError);
        }

        public IEnumerator GetAssetById(string id, Action<Asset> onSuccess, Action<string> onError)
        {
            yield return Asset.GetById(id, this, onSuccess, onError);
        }

        public IEnumerator GetAllAssets(Action<Asset[]> onSuccess, Action<string> onError)
        {
            yield return Asset.GetAssets(this, onSuccess, onError);
        }

        public IEnumerator LinkChain(string chainId, Action onSuccess, Action<string> onError)
        {
            yield return this.TransactionBuilder()
                .Add(Operation.Op("ft3.xc.link_chain", chainId))
                .Build(new byte[][] { }, onError)
                .PostAndWait(onSuccess);
        }

        public IEnumerator IsLinkedWithChain(string chainId, Action<bool> onSuccess, Action<string> onError)
        {
            yield return this.Query<int>("ft3.xc.is_linked_with_chain",
                new (string, object)[] { ("chain_rid", chainId) },
                (int is_linked) => onSuccess(is_linked == 1), onError);
        }

        public IEnumerator GetLinkedChainsIds(Action<string[]> onSuccess, Action<string> onError)
        {
            return this.Query<string[]>("ft3.xc.get_linked_chains", null, onSuccess, onError);
        }

        public IEnumerator GetLinkedChains(Action<List<Blockchain>> onSuccess, Action<string> onError)
        {
            string[] chaindIds = null;
            yield return this.GetLinkedChainsIds(
                (string[] linkedChains) => chaindIds = linkedChains, onError
            );

            List<Blockchain> blockchains = new List<Blockchain>();
            foreach (var chaindId in chaindIds)
            {
                yield return Blockchain.Initialize(chaindId, this._directoryService,
                (Blockchain blockchain) => blockchains.Add(blockchain), onError);
            }

            onSuccess(blockchains);
        }

        public IEnumerator Query<T>(string queryName, (string name, object content)[] queryObject, Action<T> onSuccess, Action<string> onError)
        {
            return this.Connection.Query<T>(queryName, queryObject, onSuccess, onError);
        }

        public IEnumerator Call(Operation operation, User user, Action onSuccess, Action<string> onError)
        {
            yield return TransactionBuilder()
                .Add(operation)
                .Add(AccountOperations.Nop())
                .Build(user.AuthDescriptor.Signers.ToArray(), onError)
                .Sign(user.KeyPair)
                .PostAndWait(onSuccess);
        }

        public TransactionBuilder TransactionBuilder()
        {
            return new TransactionBuilder(this);
        }
    }
}