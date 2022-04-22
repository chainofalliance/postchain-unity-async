using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

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

        public static IEnumerator GetByParticipantId(string id, BlockchainSession session, Action<Account[]> onSuccess, Action<string> onError)
        {
            string[] accountIDs = null;
            yield return session.Query<string[]>("ft3.get_accounts_by_participant_id", new (string, object)[] { ("id", id) },
                (string[] _accountIDs) => accountIDs = _accountIDs, onError);

            yield return Account.GetByIds(accountIDs, session, onSuccess, onError);
        }

        public static IEnumerator GetByAuthDescriptorId(string id, BlockchainSession session, Action<Account[]> onSuccess, Action<string> onError)
        {
            string[] accountIDs = null;
            yield return session.Query<string[]>("ft3.get_accounts_by_auth_descriptor_id", new (string, object)[] { ("descriptor_id", id) },
                (string[] _accountIDs) => accountIDs = _accountIDs, onError);

            yield return Account.GetByIds(accountIDs, session, onSuccess, onError);
        }

        public static IEnumerator Register(AuthDescriptor authDescriptor, BlockchainSession session, Action<Account> onSuccess, Action<string> onError)
        {
            Account account = null;
            yield return session.Call(AccountDevOperations.Register(authDescriptor), () =>
                {
                    account = new Account(
                        Util.ByteArrayToString(authDescriptor.Hash()),
                        new AuthDescriptor[] { authDescriptor },
                        session);
                }
            , onError);

            if (account != null)
            {
                yield return account.Sync(() => onSuccess(account), onError);
            }
        }

        public static byte[] RawTransactionRegister(User user, AuthDescriptor authDescriptor, Blockchain blockchain)
        {
            var signers = new List<byte[]>();
            signers.AddRange(user.AuthDescriptor.Signers);
            signers.AddRange(authDescriptor.Signers);

            return blockchain.TransactionBuilder()
                .Add(AccountDevOperations.Register(user.AuthDescriptor))
                .Add(AccountOperations.AddAuthDescriptor(user.AuthDescriptor.ID, user.AuthDescriptor.ID, authDescriptor))
                .Build(signers.ToArray(), null)
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
                .Build(signers.ToArray(), null)
                .Sign(user.KeyPair)
                .Raw();
        }

        public static IEnumerator GetByIds(string[] ids, BlockchainSession session, Action<Account[]> onSuccess, Action<string> onError)
        {
            var accounts = new List<Account>();
            foreach (var id in ids)
            {
                yield return Account.GetById(id, session, (Account account) => accounts.Add(account), onError);
            }

            onSuccess(accounts.ToArray());
        }

        public static IEnumerator GetById(string id, BlockchainSession session, Action<Account> onSuccess, Action<string> onError)
        {
            Account account = null;
            yield return session.Query<string>("ft3.get_account_by_id", new (string, object)[] { ("id", id) },
            (string _id) =>
            {
                if (!String.IsNullOrEmpty(_id)) account = new Account(_id, new AuthDescriptor[] { }, session);
            }, onError);

            if (account != null)
            {
                yield return account.Sync(() => onSuccess(account), onError);
            }
        }

        public IEnumerator AddAuthDescriptor(AuthDescriptor authDescriptor, Action onSuccess, Action<string> onError)
        {
            yield return this.Session.Call(AccountOperations.AddAuthDescriptor(
                this.Id,
                this.Session.User.AuthDescriptor.ID,
                authDescriptor),
                () =>
                {
                    this.AuthDescriptor.Add(authDescriptor);
                    onSuccess();
                }, onError
            );
        }

        public IEnumerator IsAuthDescriptorValid(string id, Action<bool> onSuccess, Action<string> onError)
        {
            yield return Session.Query<bool>("ft3.is_auth_descriptor_valid",
                new (string, object)[] { ("account_id", this.Id), ("auth_descriptor_id", Util.HexStringToBuffer(id)) },
                onSuccess,
                onError
            );
        }

        public IEnumerator DeleteAllAuthDescriptorsExclude(AuthDescriptor authDescriptor, Action onSuccess, Action<string> onError)
        {
            yield return this.Session.Call(AccountOperations.DeleteAllAuthDescriptorsExclude(
                this.Id,
                authDescriptor.ID),
                () =>
                {
                    this.AuthDescriptor.Clear();
                    this.AuthDescriptor.Add(authDescriptor);
                    onSuccess();
                }, onError
            );
        }

        public IEnumerator DeleteAuthDescriptor(AuthDescriptor authDescriptor, Action onSuccess, Action<string> onError)
        {
            yield return this.Session.Call(AccountOperations.DeleteAuthDescriptor(
                this.Id,
                this.Session.User.AuthDescriptor.ID,
                authDescriptor.ID),
                () =>
                {
                    this.AuthDescriptor.Remove(authDescriptor);
                    onSuccess();
                }, onError
            );
        }

        public IEnumerator Sync(Action onSuccess, Action<string> onError)
        {
            yield return SyncAssets(() => { }, onError);
            yield return SyncAuthDescriptors(() => { }, onError);
            yield return SyncRateLimit(() => { }, onError);

            onSuccess();
        }

        private IEnumerator SyncAssets(Action onSuccess, Action<string> onError)
        {
            yield return AssetBalance.GetByAccountId(this.Id, this.Session.Blockchain,
                (AssetBalance[] balances) =>
                {
                    this.Assets = balances.ToList();
                    onSuccess();
                }, onError
            );
        }

        private IEnumerator SyncAuthDescriptors(Action onSuccess, Action<string> onError)
        {
            AuthDescriptorFactory.AuthDescriptorQuery[] authDescriptors = null;

            yield return this.Session.Query<AuthDescriptorFactory.AuthDescriptorQuery[]>("ft3.get_account_auth_descriptors",
                new (string, object)[] { ("id", this.Id) },
                (AuthDescriptorFactory.AuthDescriptorQuery[] authQuery) =>
                {
                    authDescriptors = authQuery;
                }, onError);

            var authDescriptorFactory = new AuthDescriptorFactory();
            List<AuthDescriptor> authList = new List<AuthDescriptor>();

            foreach (var authDescriptor in authDescriptors)
            {
                authList.Add(
                    authDescriptorFactory.Create(
                        Util.StringToAuthType((string)authDescriptor.type),
                        Util.HexStringToBuffer((string)authDescriptor.args)
                    )
                );
            }

            this.AuthDescriptor = authList;
            onSuccess();
        }

        private IEnumerator SyncRateLimit(Action onSuccess, Action<string> onError)
        {
            yield return RateLimit.GetByAccountRateLimit(this.Id, this.Session.Blockchain,
                (RateLimit rateLimit) =>
                {
                    this.RateLimit = rateLimit;
                    onSuccess();
                }, onError
            );
        }

        public AssetBalance GetAssetById(string id)
        {
            return this.Assets.Find(assetBalance => assetBalance.Asset.Id.Equals(id));
        }

        public IEnumerator TransferInputsToOutputs(object[] inputs, object[] outputs, Action onSuccess, Action<string> onError)
        {
            bool isTransferCompleted = false;

            yield return this.Session.Call(AccountOperations.Transfer(inputs, outputs),
                () => isTransferCompleted = true, onError);

            if (isTransferCompleted)
            {
                yield return this.SyncAssets(onSuccess, onError);
            }
        }

        public IEnumerator Transfer(string accountId, string assetId, long amount, Action onSuccess, Action<string> onError)
        {
            var input = new List<object>{
                this.Id,
                assetId,
                this.Session.User.AuthDescriptor.ID,
                amount,
                new object[] {}
            }.ToArray();

            var output = new List<object>{
                accountId,
                assetId,
                amount,
                new object[] {}
            }.ToArray();

            yield return this.TransferInputsToOutputs(
                new object[] { input },
                new object[] { output },
                onSuccess, onError
            );
        }

        public IEnumerator BurnTokens(string assetId, long amount, Action onSuccess, Action<string> onError)
        {
            var input = new List<object>(){
                this.Id,
                assetId,
                this.Session.User.AuthDescriptor.Hash(),
                amount,
                new object[] {}
            }.ToArray();

            yield return this.TransferInputsToOutputs(
                new List<object>() { input }.ToArray(),
                new List<object>() { }.ToArray(),
                onSuccess, onError
            );
        }

        public IEnumerator XcTransfer(string destinationChainId, string destinationAccountId,
            string assetId, long amount, Action onSuccess, Action<string> onError)
        {
            yield return this.Session.Call(this.XcTransferOp(
                destinationChainId, destinationAccountId, assetId, amount),
                () =>
                {
                    this.SyncAssets(onSuccess, onError);
                }, onError
            );
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