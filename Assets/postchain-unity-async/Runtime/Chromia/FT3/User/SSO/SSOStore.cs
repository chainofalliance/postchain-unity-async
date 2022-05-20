using System.Collections.Generic;
using System;

namespace Chromia.Postchain.Ft3
{
    [Serializable]
    public abstract class SSOStore
    {
        public SSOLoadOut DataLoad;

        public KeyPair TmpKeyPair
        {
            get
            {
                if (DataLoad.TmpPrivKey == null) return null;
                return new KeyPair(DataLoad.TmpPrivKey);
            }
        }

        public List<SSOAccount> Accounts
        {
            get { return DataLoad.Accounts; }
        }

        public SSOStore()
        {
            DataLoad = new SSOLoadOut();
        }

        public abstract void Load();
        public abstract void Save();

        public void AddAccountOrPrivKey(string accountId, string privKey)
        {
            var index = DataLoad.Accounts.FindIndex((elem) => elem.AccountId.Equals(accountId));

            if (index >= 0)
            {
                var account = DataLoad.Accounts[index];
                account.PrivKey = privKey;
            }
            else
            {
                DataLoad.Accounts.Add(new SSOAccount(accountId, privKey));
            }
        }

        public void RemoveAccount(string accountId)
        {
            var index = DataLoad.Accounts.FindIndex((elem) => elem.AccountId.Equals(accountId));

            if (index >= 0)
                DataLoad.Accounts.RemoveAt(index);
        }

        public void ClearTmp()
        {
            DataLoad.TmpPrivKey = null;
            DataLoad.TmpTx = null;
        }
    }

    [Serializable]
    public struct SSOAccount
    {
        public string AccountId;
        public string PrivKey;

        public SSOAccount(string accountId, string privKey)
        {
            AccountId = accountId;
            PrivKey = privKey;
        }
    }

    [Serializable]
    public class SSOLoadOut
    {
        public List<SSOAccount> Accounts = null;
        public string TmpTx = null;
        public string TmpPrivKey = null;

        public SSOLoadOut()
        {
            Accounts = new List<SSOAccount>();
        }
    }
}