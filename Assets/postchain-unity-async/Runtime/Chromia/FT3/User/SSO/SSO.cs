
using System.Collections.Generic;
using System.Text;
using System;

using Chromia.Postchain.Client.Unity;
using Chromia.Postchain.Client;

using Cysharp.Threading.Tasks;

namespace Chromia.Postchain.Ft3
{
    public class SSO
    {
        public readonly Blockchain Blockchain;
        public readonly SSOStore Store;
        private static string _vaultUrl = "https://vault-testnet.chromia.com";

        public SSO(Blockchain blockchain, SSOStore store = null)
        {
            this.Blockchain = blockchain;

            if (store == null)
                // Instantiate default standalone store
                Store = new SSOStandaloneStore();
            else
                Store = store;
        }

        public static string VaultUrl
        {
            get => _vaultUrl;
            set { _vaultUrl = value; }
        }

        private async UniTask<UserAccount[]> GetAccountAndUserByStoredIds()
        {
            List<UserAccount> userAccounts = new List<UserAccount>();
            List<SSOAccount> accounts = this.Store.Accounts;

            foreach (var acc in accounts)
            {
                var userAccount = await GetAccountAndUserByStoredId(acc);

                if (userAccount.Error)
                    UnityEngine.Debug.LogWarning(userAccount.ErrorMessage);
                else
                    userAccounts.Add(userAccount.Content);
            }

            return userAccounts.ToArray();
        }

        private async UniTask<PostchainResponse<UserAccount>> GetAccountAndUserByStoredId(SSOAccount sSOAccount)
        {
            var keyPair = new KeyPair(sSOAccount.PrivKey);
            var authDescriptor = new SingleSignatureAuthDescriptor(keyPair.PubKey, new FlagsType[] { FlagsType.Transfer });
            var user = new User(keyPair, authDescriptor);

            var res = await this.Blockchain.NewSession(user).GetAccountById(sSOAccount.AccountId);

            if (res.Error)
                return PostchainResponse<UserAccount>.ErrorResponse(res.ErrorMessage);

            var resValid = await res.Content.IsAuthDescriptorValid(user.AuthDescriptor.ID);

            var errorMessage = "";

            if (resValid.Error)
                errorMessage = resValid.ErrorMessage;
            else if (resValid.Content == false)
                errorMessage = "Authdescriptor is not valid";
            else
                return PostchainResponse<UserAccount>.SuccessResponse(new UserAccount(user, res.Content));

            return PostchainResponse<UserAccount>.ErrorResponse(errorMessage);
        }

        public UniTask<UserAccount[]> AutoLogin()
        {
            return GetAccountAndUserByStoredIds();
        }

        public void InitiateLogin(string successUrl, string cancelUrl)
        {
            KeyPair keyPair = new KeyPair();
            this.Store.DataLoad.TmpPrivKey = Util.ByteArrayToString(keyPair.PrivKey);
            this.Store.Save();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(
                "{0}/?route=/authorize&dappId={1}&pubkey={2}&successAction={3}&cancelAction={4}&version=0.1",
                SSO._vaultUrl, this.Blockchain.Id, Util.ByteArrayToString(keyPair.PubKey), new Uri(successUrl), new Uri(cancelUrl)
            );
            UnityEngine.Application.OpenURL(sb.ToString());
        }

        public async UniTask<PostchainResponse<UserAccount>> FinalizeLogin(string tx)
        {
            var keyPair = this.Store.TmpKeyPair;
            this.Store.ClearTmp();

            if (keyPair == null) throw new Exception("Error loading public key");

            var authDescriptor = new SingleSignatureAuthDescriptor(keyPair.PubKey, new FlagsType[] { FlagsType.Transfer });
            var user = new User(keyPair, authDescriptor);

            var gtx = PostchainUtil.DeserializeGTX(Util.HexStringToBuffer(tx));
            gtx.Sign(keyPair.PrivKey, keyPair.PubKey);

            var connection = this.Blockchain.Connection;
            var postchainTransaction = new PostchainTransaction(gtx, new Uri(connection.BaseUrl), connection.BlockchainRID);

            var res = await postchainTransaction.PostAndWait();
            var errorMessage = "";

            if (!res.Error)
            {
                var accountID = GetAccountId(gtx);
                this.Store.AddAccountOrPrivKey(
                    accountID,
                    Util.ByteArrayToString(keyPair.PrivKey)
                );
                this.Store.Save();

                var resAccount = await this.Blockchain.NewSession(user).GetAccountById(accountID);

                if (!resAccount.Error)
                    return PostchainResponse<UserAccount>.SuccessResponse(new UserAccount(user, resAccount.Content));
                else
                    errorMessage = resAccount.ErrorMessage;
            }
            else
            {
                errorMessage = res.ErrorMessage;
            }

            return PostchainResponse<UserAccount>.ErrorResponse(errorMessage);
        }

        private string GetAccountId(Gtx gtx)
        {
            var ops = gtx.Operations;
            if (ops.Count == 1)
            {
                return ops[0].Args[0].String;
            }
            else if (ops.Count == 2)
            {
                return ops[1].Args[0].String;
            }
            else
            {
                throw new Exception("Invalid sso transaction");
            }
        }

        public async UniTask<PostchainResponse<string>> Logout((Account, User) au)
        {
            var res = await au.Item1.DeleteAuthDescriptor(au.Item2.AuthDescriptor);

            if (!res.Error)
            {
                this.Store.ClearTmp();
                this.Store.Save();
            }

            return res;
        }
    }

    public struct UserAccount
    {
        public User User;
        public Account Account;

        public UserAccount(User user, Account account)
        {
            User = user;
            Account = account;
        }
    }
}
