using Cryptography.ECDSA;
using System;
using System.Collections;

using Cysharp.Threading.Tasks;

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
        public bool sent { get; private set; } = false;

        private Gtx _gtxObject;
        private Uri _baseUri;
        private string _brid;

        private Uri txUri => new Uri(_baseUri, "tx/" + this._brid);
        private Uri txStatusUri => new Uri(_baseUri, "tx/" + this._brid + "/" + GetTxRID() + "/status");

        internal PostchainTransaction(Gtx gtx, Uri baseUri, string brid)
        {
            this._gtxObject = gtx;
            this._baseUri = baseUri;
            this._brid = brid;
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
        public async UniTask<PostchainResponse<string>> Post()
        {
            if (this.sent)
            {
                return PostchainResponse<string>.ErrorResponse("Tried to send tx twice");
            }
            else
            {
                var payload = String.Format(@"{{""tx"": ""{0}""}}", Serialize());

                this.sent = true;
                return await PostchainRequest.Post<string>(txUri, payload);
            }
        }

        ///<summary>
        ///Commit the transaction and send it to the blockchain and waits for its confirmation.
        ///Fails if transaction as already been sent.
        ///</summary>
        ///<param name = "callback">Action that gets called once the transaction has been confirmed.</param>
        ///<returns>Unity coroutine enumerator.</returns>
        public async UniTask<PostchainResponse<string>> PostAndWait()
        {
            var response = await Post();
            var waitResponse = await WaitConfirmation();

            return waitResponse.Error ? waitResponse : response;
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

        private async UniTask<PostchainResponse<string>> WaitConfirmation()
        {
            var request = await PostchainRequest.Get<TxStatusResponse>(txStatusUri);

            var ret = request.Content;
            switch (ret.status)
            {
                case "confirmed":
                    {
                        return PostchainResponse<string>.SuccessResponse();
                    }
                case "rejected":
                case "unknown":
                    {
                        return PostchainResponse<string>.ErrorResponse(ret.rejectReason);
                    }
                case "waiting":
                    {
                        await UniTask.Delay(TimeSpan.FromMilliseconds(511), ignoreTimeScale: false);
                        return await WaitConfirmation();
                    }
                case "exception":
                    {
                        return PostchainResponse<string>.ErrorResponse("HTTP Exception: " + ret.rejectReason);
                    }
                default:
                    {
                        return PostchainResponse<string>.ErrorResponse($"Got unexpected response from server ({ret.status}): {ret.rejectReason}");
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