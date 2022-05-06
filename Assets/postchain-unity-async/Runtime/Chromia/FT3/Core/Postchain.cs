using Cysharp.Threading.Tasks;

namespace Chromia.Postchain.Ft3
{
    public class Postchain
    {
        private readonly string _url;

        public Postchain(string url)
        {
            this._url = url;
        }

        public UniTask<Blockchain> Blockchain(string id)
        {
            var directoryService = new DirectoryServiceBase(
                new ChainConnectionInfo[] { new ChainConnectionInfo(id, _url) }
            );

            return Ft3.Blockchain.Initialize(id, directoryService);
        }
    }
}
