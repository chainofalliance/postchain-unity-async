using Chromia.Postchain.Client.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Chromia.Postchain.Ft3
{
    public class Postchain
    {
        private readonly string _url;

        public Postchain(string url)
        {
            this._url = url;
        }

        public UniTask<Blockchain> Blockchain(string brid)
        {
            var directoryService = new DirectoryServiceBase(
                new ChainConnectionInfo[] { new ChainConnectionInfo(brid, _url) }
            );

            return Ft3.Blockchain.Initialize(brid, directoryService);
        }

        public async UniTask<Blockchain> Blockchain(int chainId)
        {
            var response = await UnityWebRequest.Get(
                PostchainRequest.ToUri(_url, "brid/iid_" + chainId)).SendWebRequest();
            var brid = response.downloadHandler.text;

            if (System.String.IsNullOrEmpty(brid))
                throw new System.Exception("InitializeBRIDFromChainID: brid is null or empty");

            var directoryService = new DirectoryServiceBase(
                new ChainConnectionInfo[] { new ChainConnectionInfo(brid, _url) }
            );

            return await Ft3.Blockchain.Initialize(brid, directoryService);
        }
    }
}
