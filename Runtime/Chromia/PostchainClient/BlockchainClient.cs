using System;
using System.Collections;

using Chromia.Postchain.Client.Unity;

using UnityEngine;

namespace Chromia.Postchain.Client
{
    public class BlockchainClient : MonoBehaviour
    {
        public string BlockchainRID { get { return _blockchainRID; } }
        public string BaseUrl { get { return _baseURL; } }

        [SerializeField] private string _blockchainRID;
        [SerializeField] private int _chainId;
        [SerializeField] private string _baseURL;

        /// <inheritdoc />
        void Start()
        {
            if (String.IsNullOrEmpty(this._blockchainRID))
            {
                StartCoroutine(InitializeBRIDFromChainID());
            }
        }

        ///<summary>
        ///Sets parameter to connect to blockchain.
        ///</summary>
        ///<param name = "blockchainRID">The blockchain RID of the dapp.</param>
        ///<param name = "baseURL">Location of the blockchain.</param>
        public void Setup(string blockchainRID, string baseURL)
        {
            this._blockchainRID = blockchainRID;
            this._baseURL = baseURL;
        }

        ///<summary>
        ///Create a new Transaction.
        ///</summary>
        ///<param name = "signers">Array of signers. Can be empty and set later.</param>
        ///<param name = "onError">Action that gets called in case of any error with the transaction.</param>
        ///<returns>New PostchainTransaction object.</returns>
        public PostchainTransaction NewTransaction(byte[][] signers, Action<string> onError)
        {
            Gtx newGtx = new Gtx(this._blockchainRID);

            foreach (byte[] signer in signers)
            {
                newGtx.AddSignerToGtx(signer);
            }

            return new PostchainTransaction(newGtx, this._baseURL, this._blockchainRID, onError);
        }

        ///<summary>
        ///Queries data from the blockchain.
        ///</summary>
        ///<param name = "queryName">Name of the query in RELL.</param>
        ///<param name = "queryObject">Parameters of the query.</param>
        ///<param name = "onSuccess">Action that gets called when the query succeeds. Passes return value as parameter.</param>
        ///<param name = "onError">Action that gets called if any error occures while querying from blockchain. Passes error message as parameter.</param>
        ///<returns>Unity coroutine enumerator.</returns>
        public IEnumerator Query<T>(string queryName, (string name, object content)[] queryObject, Action<T> onSuccess, Action<string> onError)
        {
            var request = new PostchainQuery<T>(this._baseURL, this._blockchainRID);

            yield return request.Query(queryName, queryObject);

            if (request.error)
            {
                onError(request.errorMessage);
            }
            else
            {
                onSuccess(request.content);
            }
        }

        private IEnumerator InitializeBRIDFromChainID()
        {
            var request = new PostchainRequestRaw(this._baseURL, "brid/iid_" + this._chainId);

            yield return request.Get();

            if (request.error)
            {
                Debug.LogError("InitializeBRIDFromChainID: " + request.errorMessage);
            }
            else
            {
                this._blockchainRID = request.content;
            }
        }
    }
}
