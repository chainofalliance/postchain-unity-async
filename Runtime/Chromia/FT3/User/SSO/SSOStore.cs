using System.Collections.Generic;
using System;

namespace Chromia.Postchain.Ft3
{
    public interface SSOStore
    {
        KeyPair TmpKeyPair
        {
            get;
        }
        byte[] TmpPrivKey
        {
            get; set;
        }
        string TmpTx
        {
            get; set;
        }
        List<SavedSSOAccount> GetAccounts();
        void AddAccount(string accountId, string privKey);
        void RemoveAccount(string accountId);
        void Load();
        void Save();
        void ClearTmp();
    }

    [Serializable]
    public class SavedSSOData
    {
        public string __tmpTX = null;
        public string __ssoTmpPrivKey = null;
        public List<SavedSSOAccount> __accounts = new List<SavedSSOAccount>();

        public void AddAccountOrPrivKey(string accountId, string privKey)
        {
            var result = __accounts.Find((elem) => elem.__ssoAccountId.Equals(accountId));
            if (result != null)
            {
                result.__ssoPrivKey = privKey;
            }
            else
            {
                __accounts.Add(new SavedSSOAccount(accountId, privKey));
            }
        }

        public void RemoveAccount(string accountId)
        {
            var result = __accounts.Find((elem) => elem.__ssoAccountId.Equals(accountId));
            __accounts.Remove(result);
        }
    }

    [Serializable]
    public class SavedSSOAccount
    {
        public string __ssoAccountId;
        public string __ssoPrivKey;

        public SavedSSOAccount(string accountId, string privKey)
        {
            __ssoAccountId = accountId;
            __ssoPrivKey = privKey;
        }
    }
}