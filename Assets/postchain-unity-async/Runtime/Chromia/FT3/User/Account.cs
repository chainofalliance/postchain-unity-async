using System.Collections.Generic;
using Chromia.Postchain.Client;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;

using Cysharp.Threading.Tasks;

namespace Chromia.Postchain.Ft3
{
    public enum AuthType
    {
        None,
        SingleSig,
        MultiSig
    }

    public enum FlagsType
    {
        None,
        Account,
        Transfer
    }

    public class Flags
    {
        public List<FlagsType> FlagList;

        public Flags(List<FlagsType> flags)
        {
            this.FlagList = flags;
        }

        public bool HasFlag(FlagsType flag)
        {
            return this.FlagList.Contains(flag);
        }

        public bool IsValid(FlagsType flag)
        {
            return flag == FlagsType.Account || flag == FlagsType.Transfer;
        }

        public object[] ToGTV()
        {
            var validFlags = new List<string>();
            foreach (var flag in this.FlagList)
            {
                if (IsValid(flag))
                {
                    validFlags.Add(Util.FlagTypeToString(flag));
                }
            }

            return validFlags.ToArray();
        }
    }

    public interface AuthDescriptor : GtvSerializable
    {
        string ID
        {
            get;
        }
        List<byte[]> Signers
        {
            get;
        }
        IAuthdescriptorRule Rule
        {
            get;
        }
        byte[] Hash();
    }

    public interface GtvSerializable
    {
        object[] ToGTV();
    }

    public class Account
    {
        public readonly string Id;
        public List<AuthDescriptor> AuthDescriptor;
        public List<AssetBalance> Assets;
        public RateLimit RateLimit;
        public readonly BlockchainSession Session;

        public Account(string id, AuthDescriptor[] authDescriptor, BlockchainSession session)
        {
            this.Id = id;
            this.AuthDescriptor = authDescriptor.ToList();
            this.Session = session;

            this.Assets = new List<AssetBalance>();
        }

        public string GetID()
        {
            return Id;
        }

        public Blockchain GetBlockchain()
        {
            return this.Session.Blockchain;
        }

        public static async UniTask<PostchainResponse<Account[]>> GetByParticipantId(string id, BlockchainSession session)
        {
            var res = await session.Query<string[]>("ft3.get_accounts_by_participant_id", new (string, object)[] { ("id", id) });

            if (res.Error)
                return PostchainResponse<Account[]>.ErrorResponse(res.ErrorMessage);

            return await Account.GetByIds(res.Content, session);
        }

        public static async UniTask<PostchainResponse<Account[]>> GetByAuthDescriptorId(string id, BlockchainSession session)
        {
            var res = await session.Query<string[]>("ft3.get_accounts_by_auth_descriptor_id", new (string, object)[] { ("descriptor_id", id) });

            if (res.Error)
                return PostchainResponse<Account[]>.ErrorResponse(res.ErrorMessage);

            return await Account.GetByIds(res.Content, session);
        }

        public static async UniTask<PostchainResponse<Account>> Register(AuthDescriptor authDescriptor, BlockchainSession session)
        {
            var res = await session.Call(AccountDevOperations.Register(authDescriptor));
            if (res.Error)
                return PostchainResponse<Account>.ErrorResponse(res.ErrorMessage);

            var account = new Account(
                 Util.ByteArrayToString(authDescriptor.Hash()),
                 new AuthDescriptor[] { authDescriptor },
                 session);
            await account.Sync();

            return PostchainResponse<Account>.SuccessResponse(account);
        }

        public static byte[] RawTransactionRegister(User user, AuthDescriptor authDescriptor, Blockchain blockchain)
        {
            var signers = new List<byte[]>();
            signers.AddRange(user.AuthDescriptor.Signers);
            signers.AddRange(authDescriptor.Signers);

            return blockchain.TransactionBuilder()
                .Add(AccountDevOperations.Register(user.AuthDescriptor))
                .Add(AccountOperations.AddAuthDescriptor(user.AuthDescriptor.ID, user.AuthDescriptor.ID, authDescriptor))
                .Build(signers.ToArray())
                .Sign(user.KeyPair)
                .Raw();
        }

        public static byte[] RawTransactionAddAuthDescriptor(string accountId, User user, AuthDescriptor authDescriptor, Blockchain blockchain)
        {
            var signers = new List<byte[]>();
            signers.AddRange(user.AuthDescriptor.Signers);
            signers.AddRange(authDescriptor.Signers);

            return blockchain.TransactionBuilder()
                .Add(AccountOperations.AddAuthDescriptor(user.AuthDescriptor.ID, user.AuthDescriptor.ID, authDescriptor))
                .Build(signers.ToArray())
                .Sign(user.KeyPair)
                .Raw();
        }

        public static async UniTask<PostchainResponse<Account[]>> GetByIds(string[] ids, BlockchainSession session)
        {
            var accounts = new List<Account>();
            foreach (var id in ids)
            {
                var res = await Account.GetById(id, session);

                if (!res.Error)
                    accounts.Add(res.Content);
                else
                    UnityEngine.Debug.LogWarning(res.ErrorMessage);
            }

            return PostchainResponse<Account[]>.SuccessResponse(accounts.ToArray());
        }

        public static async UniTask<PostchainResponse<Account>> GetById(string id, BlockchainSession session)
        {
            var res = await session.Query<string>("ft3.get_account_by_id", new (string, object)[] { ("id", id) });

            if (res.Error || String.IsNullOrEmpty(res.Content))
            {
                return PostchainResponse<Account>.ErrorResponse(res.ErrorMessage);
            }

            var account = new Account(res.Content, new AuthDescriptor[] { }, session);

            await account.Sync();
            return PostchainResponse<Account>.SuccessResponse(account);
        }

        public async UniTask<PostchainResponse<string>> AddAuthDescriptor(AuthDescriptor authDescriptor)
        {
            var res = await this.Session.Call(AccountOperations.AddAuthDescriptor(
                this.Id,
                this.Session.User.AuthDescriptor.ID,
                authDescriptor)
            );

            if (!res.Error)
                this.AuthDescriptor.Add(authDescriptor);

            return res;
        }

        public UniTask<PostchainResponse<bool>> IsAuthDescriptorValid(string id)
        {
            return Session.Query<bool>("ft3.is_auth_descriptor_valid",
                new (string, object)[] { ("account_id", this.Id), ("auth_descriptor_id", Util.HexStringToBuffer(id)) }
            );
        }

        public async UniTask<PostchainResponse<string>> DeleteAllAuthDescriptorsExclude(AuthDescriptor authDescriptor)
        {
            var res = await this.Session.Call(AccountOperations.DeleteAllAuthDescriptorsExclude(
                this.Id,
                authDescriptor.ID)
            );

            if (!res.Error)
            {
                this.AuthDescriptor.Clear();
                this.AuthDescriptor.Add(authDescriptor);
            }

            return res;
        }

        public async UniTask<PostchainResponse<string>> DeleteAuthDescriptor(AuthDescriptor authDescriptor)
        {
            var res = await this.Session.Call(AccountOperations.DeleteAuthDescriptor(
               this.Id,
               this.Session.User.AuthDescriptor.ID,
               authDescriptor.ID)
           );

            if (!res.Error)
                this.AuthDescriptor.Remove(authDescriptor);

            return res;
        }

        public async UniTask Sync()
        {
            var syncAssetsTask = SyncAssets();
            var syncDescTask = SyncAuthDescriptors();
            var syncLimitTask = SyncRateLimit();

            await UniTask.WhenAll(syncAssetsTask, syncDescTask, syncLimitTask);
        }

        private async UniTask SyncAssets()
        {
            var res = await AssetBalance.GetByAccountId(this.Id, this.Session.Blockchain);

            if (res.Error)
                UnityEngine.Debug.LogWarning(res.ErrorMessage);
            else
                this.Assets = res.Content.ToList();
        }

        private async UniTask SyncAuthDescriptors()
        {
            var res = await this.Session.Query<AuthDescriptorFactory.AuthDescriptorQuery[]>("ft3.get_account_auth_descriptors",
                new (string, object)[] { ("id", this.Id) });


            if (res.Error)
                UnityEngine.Debug.LogWarning(res.ErrorMessage);
            else
            {
                var authDescriptorFactory = new AuthDescriptorFactory();
                List<AuthDescriptor> authList = new List<AuthDescriptor>();

                foreach (var authDescriptor in res.Content)
                {
                    authList.Add(
                        authDescriptorFactory.Create(
                            Util.StringToAuthType((string)authDescriptor.type),
                            Util.HexStringToBuffer((string)authDescriptor.args)
                        )
                    );
                }

                this.AuthDescriptor = authList;
            }
        }

        private async UniTask SyncRateLimit()
        {
            var res = await RateLimit.GetByAccountRateLimit(this.Id, this.Session.Blockchain);

            if (!res.Error)
                this.RateLimit = res.Content;
        }

        public AssetBalance GetAssetById(string id)
        {
            return this.Assets.Find(assetBalance => assetBalance.Asset.Id.Equals(id));
        }

        public async UniTask<PostchainResponse<string>> TransferInputsToOutputs(object[] inputs, object[] outputs)
        {
            var res = await this.Session.Call(AccountOperations.Transfer(inputs, outputs));

            await this.SyncAssets();

            return res;
        }

        public UniTask<PostchainResponse<string>> Transfer(string accountId, string assetId, long amount)
        {
            var input = new object[] {
                this.Id,
                assetId,
                this.Session.User.AuthDescriptor.ID,
                amount,
                new object[] {}
            };

            var output = new object[] {
                accountId,
                assetId,
                amount,
                new object[] {}
            };

            return this.TransferInputsToOutputs(
                new object[] { input },
                new object[] { output }
            );
        }

        public UniTask<PostchainResponse<string>> BurnTokens(string assetId, long amount)
        {
            var input = new object[]{
                this.Id,
                assetId,
                this.Session.User.AuthDescriptor.Hash(),
                amount,
                new object[] {}
            };

            return this.TransferInputsToOutputs(
                new object[] { input },
                new object[] { }
            );
        }

        public async UniTask<PostchainResponse<string>> XcTransfer(string destinationChainId, string destinationAccountId,
            string assetId, long amount)
        {
            var res = await this.Session.Call(this.XcTransferOp(
                destinationChainId, destinationAccountId, assetId, amount)
            );

            if (!res.Error)
                await this.SyncAssets();

            return res;
        }

        public Operation XcTransferOp(string destinationChainId, string destinationAccountId, string assetId, long amount)
        {
            var source = new object[] {
                this.Id,
                assetId,
                this.Session.User.AuthDescriptor.ID,
                amount,
                new object[]{}
            };

            var target = new object[] {
                destinationAccountId,
                new object[]{}
            };

            var hops = new string[] {
                destinationChainId
            };

            return AccountOperations.XcTransfer(source, target, hops);
        }
    }
}