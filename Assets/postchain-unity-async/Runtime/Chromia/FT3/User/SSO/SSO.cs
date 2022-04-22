using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;

using Chromia.Postchain.Client.Unity;
using Chromia.Postchain.Client;

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
            {
                Store = new SSOStoreLocalStorage();
            }
            else
            {
                Store = store;
            }
        }

        public static string VaultUrl
        {
            get => _vaultUrl;
            set { _vaultUrl = value; }
        }

        private IEnumerator GetAccountAndUserByStoredIds(Action<List<(Account, User)>> onSuccess, Action<string> onError)
        {
            List<(Account, User)> aus = new List<(Account, User)>();
            List<SavedSSOAccount> accounts = this.Store.GetAccounts();

            foreach (var acc in accounts)
            {
                yield return GetAccountAndUserByStoredId(acc, ((Account, User) au) => aus.Add(au), onError);
            }

            onSuccess(aus);
        }

        private IEnumerator GetAccountAndUserByStoredId(SavedSSOAccount savedAccount, Action<(Account, User)> onSuccess, Action<string> onError)
        {
            var keyPair = new KeyPair(savedAccount.__ssoPrivKey);
            var authDescriptor = new SingleSignatureAuthDescriptor(keyPair.PubKey, new FlagsType[] { FlagsType.Transfer });
            var user = new User(keyPair, authDescriptor);

            Account account = null;
            yield return this.Blockchain.NewSession(user).GetAccountById(savedAccount.__ssoAccountId,
                (Account _account) => account = _account, onError);

            if (account != null)
            {
                yield return account.IsAuthDescriptorValid(user.AuthDescriptor.ID,
                    (bool isValid) =>
                    {
                        if (isValid) onSuccess((account, user));
                    }, onError // delete authdescriptor from localstorage?
                );
            }
        }

        public IEnumerator AutoLogin(Action<List<(Account, User)>> onSuccess, Action<string> onError)
        {
            yield return GetAccountAndUserByStoredIds(onSuccess, onError);
        }

        public void InitiateLogin(string successUrl, string cancelUrl)
        {
            KeyPair keyPair = new KeyPair();
            this.Store.TmpPrivKey = keyPair.PrivKey;
            this.Store.Save();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(
                "{0}/?route=/authorize&dappId={1}&pubkey={2}&successAction={3}&cancelAction={4}&version=0.1",
                SSO._vaultUrl, this.Blockchain.Id, Util.ByteArrayToString(keyPair.PubKey), new Uri(successUrl), new Uri(cancelUrl)
            );
            UnityEngine.Application.OpenURL(sb.ToString());
        }

        public IEnumerator FinalizeLogin(string tx, Action<(Account, User)> onSuccess, Action<string> onError)
        {
            var keyPair = this.Store.TmpKeyPair;
            this.Store.ClearTmp();

            if (keyPair == null) throw new Exception("Error loading public key");

            var authDescriptor = new SingleSignatureAuthDescriptor(keyPair.PubKey, new FlagsType[] { FlagsType.Transfer });
            var user = new User(keyPair, authDescriptor);

            var gtx = PostchainUtil.DeserializeGTX(Util.HexStringToBuffer(tx));
            gtx.Sign(keyPair.PrivKey, keyPair.PubKey);

            var connection = this.Blockchain.Connection;
            var postchainTransaction = new PostchainTransaction(gtx, connection.BaseUrl, connection.BlockchainRID, onError);

            bool isSuccess = false;
            yield return postchainTransaction.PostAndWait(() => isSuccess = true);

            if (isSuccess)
            {
                var accountID = GetAccountId(gtx);
                this.Store.AddAccount(accountID, Util.ByteArrayToString(keyPair.PrivKey));
                this.Store.Save();

                yield return this.Blockchain.NewSession(user).GetAccountById(accountID,
                    (Account _account) => onSuccess((_account, user)), onError);
            }
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

        public IEnumerator Logout<T>((Account, User) au, Action onSuccess, Action<string> onError)
        {
            yield return au.Item1.DeleteAuthDescriptor(au.Item2.AuthDescriptor, () =>
            {
                this.Store.ClearTmp();
                this.Store.Save();
                onSuccess();
            }, onError);
        }

        public static Dictionary<string, string> GetParams(string uri)
        {
            var matches = Regex.Matches(uri, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
            return matches.Cast<Match>().ToDictionary(
                m => Uri.UnescapeDataString(m.Groups[2].Value),
                m => Uri.UnescapeDataString(m.Groups[3].Value)
            );
        }

        // For Webgl
        public IEnumerator PendingSSO(Action<(Account, User)> onSuccess, Action<string> onError)
        {
            var url = UnityEngine.Application.absoluteURL;
            var pairs = GetParams(url);

            if (pairs.ContainsKey("rawTx"))
            {
                var raw = pairs["rawTx"];
                yield return FinalizeLogin(raw, onSuccess, onError);
            }
        }
    }
}
