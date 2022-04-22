using Chromia.Postchain.Client.Unity;
using System.Collections;
using System;

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

        public IEnumerator Post()
        {
            yield return this._tx.Post();
        }

        public IEnumerator PostAndWait(Action onSuccess)
        {
            yield return this._tx.PostAndWait(onSuccess);
        }

        public byte[] Raw()
        {
            return this._tx.Encode();
        }
    }
}