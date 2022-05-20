using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System;

using System.Runtime.InteropServices;

namespace Chromia.Postchain.Ft3
{
    public class SSOWebGLStore : SSOStore
    {
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

        private string storageKey = "SSO";

        public SSOWebGLStore(string storageKey = null)
        {
            if (!String.IsNullOrEmpty(storageKey))
                this.storageKey = storageKey;

            Load();
        }

        public override void Load()
        {
            string result = null;
            if (SSOStoreWebgl.HasKeyInLocalStorage(storageKey) == 1)
            {
                result = SSOStoreWebgl.LoadFromLocalStorage(storageKey);
            }

            if (!String.IsNullOrEmpty(result))
            {
                DataLoad = JsonConvert.DeserializeObject<SSOLoadOut>(result);
            }
        }

        public override void Save()
        {
            string data = JsonConvert.SerializeObject(DataLoad, Formatting.Indented);
            SSOStoreWebgl.SaveToLocalStorage(storageKey, data);
        }


        public static Dictionary<string, string> GetParams(string uri)
        {
            var matches = Regex.Matches(uri, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
            return matches.Cast<Match>().ToDictionary(
                m => Uri.UnescapeDataString(m.Groups[2].Value),
                m => Uri.UnescapeDataString(m.Groups[3].Value)
            );
        }

        public static string ExtractRawTx()
        {
            var url = UnityEngine.Application.absoluteURL;
            var pairs = GetParams(url);

            if (pairs.ContainsKey("rawTx"))
                return pairs["rawTx"];

            return null;
        }
    }
}