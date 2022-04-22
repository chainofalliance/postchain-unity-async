using System.Collections;
using System;

namespace Chromia.Postchain.Ft3
{
    public class Postchain
    {
        private readonly string _url;

        public Postchain(string url)
        {
            this._url = url;
        }

        public IEnumerator Blockchain(string id, Action<Blockchain> onSuccess, Action<string> onError)
        {
            var directoryService = new DirectoryServiceBase(
                new ChainConnectionInfo[] { new ChainConnectionInfo(id, _url) }
            );

            yield return Ft3.Blockchain.Initialize(id, directoryService, onSuccess, onError);
        }
    }
}
