using Newtonsoft.Json;
using UnityEngine;
using System;

namespace Chromia.Postchain.Ft3
{
    public class SSOStandaloneStore : SSOStore
    {
        private const string STORAGEKEY = "SSO";
        // not really a .dat file
        private const string FILENAME = STORAGEKEY + ".dat";

        public SSOStandaloneStore()
        {
            Load();
        }

        public override void Load()
        {
            string result = null;
            FileManager.LoadFromFile(FILENAME, out result);

            if (!String.IsNullOrEmpty(result))
            {
                DataLoad = JsonConvert.DeserializeObject<SSOLoadOut>(result);
            }
        }

        public override void Save()
        {
            string data = JsonConvert.SerializeObject(DataLoad, Formatting.Indented);
            FileManager.WriteToFile(FILENAME, data);
        }


        public static void SaveTmpTX(string name)
        {
            if (Application.isBatchMode)
            {
                string[] args = System.Environment.GetCommandLineArgs();
                SSOStore store = new SSOStandaloneStore();
                store.Load();

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith(name))
                    {
                        store.DataLoad.TmpTx = args[i];
                        store.Save();
                        break;
                    }
                }
                Application.Quit();
            }
        }
    }
}