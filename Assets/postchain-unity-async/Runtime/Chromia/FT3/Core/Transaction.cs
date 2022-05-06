using Chromia.Postchain.Client.Unity;
using Chromia.Postchain.Client;
using Cysharp.Threading.Tasks;

namespace Chromia.Postchain.Ft3
{
    public class Transaction
    {
        private readonly PostchainTransaction _tx;

        public Transaction(PostchainTransaction tx)
        {
            _tx = tx;
        }

        public Transaction Sign(KeyPair keyPair)
        {
            this._tx.Sign(keyPair.PrivKey, keyPair.PubKey);
            return this;
        }

        public UniTask<PostchainResponse<string>> PostAndWait()
        {
            return this._tx.PostAndWait();
        }

        public byte[] Raw()
        {
            return this._tx.Encode();
        }
    }
}