using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using System;

namespace Chromia.Postchain.Ft3
{
    public class SSOStandaloneStore : SSOStore
    {
        private const string STORAGEKEY = "SSO";
        private const string FILENAME = STORAGEKEY + ".txt";

        public SSOStandaloneStore()
        {
            Load();
        }

        public override void Load()
        {
            var fullPath = Path.Combine(Application.persistentDataPath, FILENAME);

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning("File does not exist");
                return;
            }

            try
            {
                var result = File.ReadAllText(fullPath);
                DataLoad = JsonConvert.DeserializeObject<SSOLoadOut>(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read from {fullPath} with exception {e}");
            }
        }

        public override void Save()
        {
            string data = JsonConvert.SerializeObject(DataLoad, Formatting.Indented);
            var fullPath = Path.Combine(Application.persistentDataPath, FILENAME);

            try
            {
                File.WriteAllText(fullPath, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to {fullPath} with exception {e}");
            }
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