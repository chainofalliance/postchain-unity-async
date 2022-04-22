using System.Collections.Generic;
using Newtonsoft.Json;
using System;

using System.Runtime.InteropServices;

namespace Chromia.Postchain.Ft3
{
#if UNITY_WEBGL
    public static class SSOStoreWebgl
    {
        [DllImport("__Internal")]
        public static extern void SaveToLocalStorage(string key, string value);

        [DllImport("__Internal")]
        public static extern string LoadFromLocalStorage(string key);

        [DllImport("__Internal")]
        public static extern void RemoveFromLocalStorage(string key);

        [DllImport("__Internal")]
        public static extern int HasKeyInLocalStorage(string key);

        [DllImport("__Internal")]
        public static extern void CloseWindow();
    }
#endif

    public class SSOStoreLocalStorage : SSOStore
    {
        private const string STORAGEKEY = "SSO";
        private const string FILENAME = STORAGEKEY + ".dat";
        public SavedSSOData SSOData;

        public SSOStoreLocalStorage()
        {
            Load();
        }

        public void Load()
        {
            string result = null;
#if UNITY_WEBGL
            if (SSOStoreWebgl.HasKeyInLocalStorage(STORAGEKEY) == 1)
            {
                result = SSOStoreWebgl.LoadFromLocalStorage(STORAGEKEY);
            }
#elif UNITY_STANDALONE
            FileManager.LoadFromFile(FILENAME, out result);
#endif
            if (!String.IsNullOrEmpty(result))
            {
                SSOData = JsonConvert.DeserializeObject<SavedSSOData>(result);
            }
            else
            {
                SSOData = new SavedSSOData();
            }
        }

        public void Save()
        {
            string data = JsonConvert.SerializeObject(SSOData, Formatting.Indented);
#if UNITY_WEBGL
            SSOStoreWebgl.SaveToLocalStorage(STORAGEKEY, data);
#elif UNITY_STANDALONE
            FileManager.WriteToFile(FILENAME, data);
#endif
        }

        public KeyPair TmpKeyPair
        {
            get
            {
                string privKeyString = SSOData.__ssoTmpPrivKey;
                if (privKeyString == null) return null;

                return new KeyPair(privKeyString);
            }
        }

        public string TmpTx
        {
            get
            {
                return SSOData.__tmpTX;
            }
            set
            {
                SSOData.__tmpTX = value;
            }
        }

        public byte[] TmpPrivKey
        {
            get
            {
                string privKeyString = SSOData.__ssoTmpPrivKey;
                if (privKeyString == null) return null;

                return Util.HexStringToBuffer(privKeyString);
            }
            set
            {
                SSOData.__ssoTmpPrivKey = Util.ByteArrayToString(value);
            }
        }

        public void ClearTmp()
        {
            SSOData.__ssoTmpPrivKey = null;
            SSOData.__tmpTX = null;
        }

        public void AddAccount(string accountId, string privKey)
        {
            SSOData.AddAccountOrPrivKey(accountId, privKey);
        }

        public void RemoveAccount(string accountId)
        {
            SSOData.RemoveAccount(accountId);
        }

        public List<SavedSSOAccount> GetAccounts()
        {
            return SSOData.__accounts;
        }

    }
}