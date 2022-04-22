using Cryptography.ECDSA;
using System;
using System.Collections;


namespace Chromia.Postchain.Client.Unity
{
    public class PostchainTransaction
    {
        ///<summary>
        ///Status response class used for WaitConfirmation.
        ///</summary>
        private class TxStatusResponse
        {
            public string status = "";
            public string rejectReason = "";
        }

        ///<summary>
        ///Indicates wether the transaction has been sent already.
        ///</summary>
        public bool sent {get; private set;} = false;

        private Gtx _gtxObject;
        private string _baseUrl;
        private string _brid;
        private bool _error;
        private Action<string> _onError;

        internal PostchainTransaction(Gtx gtx, string baseUrl, string brid, Action<string> onError)
        {
            this._gtxObject = gtx;
            this._baseUrl = baseUrl;
            this._brid = brid;
            this._onError = onError;
        }

        ///<summary>
        ///Add an operation to the Transaction.
        ///</summary>
        ///<param name = "name">Name of the operation.</param>
        ///<param name = "args">Array of object parameters. For example {"Hamburg", 42}</param>
        public void AddOperation(string name, params object[] args)
        {
            this._gtxObject.AddOperationToGtx(name, args);
        }

        ///<summary>
        ///Commit the transaction and send it to the blockchain asynchronously.
        ///Fails if transaction as already been sent.
        ///</summary>
        ///<returns>Unity coroutine enumerator.</returns>
        public IEnumerator Post()
        {
            if (this.sent)
            {
                this._onError("Tried to send tx twice");
            }
            else
            {
                var payload = String.Format(@"{{""tx"": ""{0}""}}", Serialize());

                var request = new PostchainRequestRaw(this._baseUrl, "tx/" + this._brid);
                yield return request.Post(payload);

                this.sent = true;
                this._error = request.error;
                if (this._error)
                {
                    this._onError(request.errorMessage);
                }
            }
        }

        ///<summary>
        ///Commit the transaction and send it to the blockchain and waits for its confirmation.
        ///Fails if transaction as already been sent.
        ///</summary>
        ///<param name = "callback">Action that gets called once the transaction has been confirmed.</param>
        ///<returns>Unity coroutine enumerator.</returns>
        public IEnumerator PostAndWait(Action callback)
        {
            yield return Post();
            yield return WaitConfirmation();

            if (!this._error)
            {
                callback();
            }
        }

        ///<summary>
        ///Signs the transaction with the given keypair.
        ///</summary>
        ///<param name = "privKey">Private key of the keypair.</param>
        ///<param name = "pubKey">Public key of the keypair. If null, a public key will be generated from the given private key.</param>
        public void Sign(byte[] privKey, byte[] pubKey)
        {
            byte[] pub = pubKey;
            if (pubKey == null)
            {
                pub = Secp256K1Manager.GetPublicKey(privKey, true);
            }
            this._gtxObject.Sign(privKey, pub);
        }

        ///<summary>
        ///Serializes the transaction as a hex string.
        ///</summary>
        ///<returns>Encoded transaction as hex string.</returns>
        public string Serialize()
        {
            return this._gtxObject.Serialize();
        }

        ///<summary>
        ///Serializes the transaction as a buffer.
        ///</summary>
        ///<returns>Encoded transaction.</returns>
        public byte[] Encode()
        {
            return this._gtxObject.Encode();
        }

        private IEnumerator WaitConfirmation()
        {
            var request = new PostchainRequest<TxStatusResponse>(this._baseUrl, "tx/" + this._brid + "/" + GetTxRID() + "/status");
            yield return request.Get();

            var ret = request.parsedContent;
            this._error = true;
            switch (ret.status)
            {
                case "confirmed":
                    {
                        this._error = false;
                        break;
                    }
                case "rejected":
                case "unknown":
                    {
                        this._onError(ret.rejectReason);
                        break;
                    }
                case "waiting":
                    {
                        yield return new UnityEngine.WaitForSeconds(0.511f);
                        yield return WaitConfirmation();
                        break;
                    }
                case "exception":
                    {
                        this._onError("HTTP Exception: " + ret.rejectReason);
                        break;
                    }
                default:
                    {
                        this._onError("Got unexpected response from server: " + ret.status);
                        break;
                    }
            }
        }

        private string GetTxRID()
        {
            return PostchainUtil.ByteArrayToString(this.GetBufferToSign());
        }

        private byte[] GetBufferToSign()
        {
            return this._gtxObject.GetBufferToSign();
        }

        private void AddSignature(byte[] pubKey, byte[] signature)
        {
            this._gtxObject.AddSignature(pubKey, signature);
        }
    }
}