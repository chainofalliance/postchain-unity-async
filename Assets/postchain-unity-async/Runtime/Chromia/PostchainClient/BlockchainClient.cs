using System;
using System.Collections;
using Newtonsoft.Json;

using Chromia.Postchain.Client.Unity;

using UnityEngine;
using Cysharp.Threading.Tasks;


namespace Chromia.Postchain.Client
{
    public class BlockchainClient : MonoBehaviour
    {
        public string BlockchainRID { get { return _blockchainRID; } }
        public string BaseUrl { get { return _baseURL; } }

        [SerializeField] private string _blockchainRID;
        [SerializeField] private int _chainId;
        [SerializeField] private string _baseURL;

        private Uri baseUri => new Uri(this._baseURL);
        private Uri queryUri => new Uri(baseUri, "query/" + this._blockchainRID);
        private Uri initBridUri => new Uri(baseUri, "brid/iid_" + this._chainId);

        /// <inheritdoc />
        void Start()
        {
            if (String.IsNullOrEmpty(this._blockchainRID))
            {
                InitializeBRIDFromChainID();
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
        ///<returns>New PostchainTransaction object.</returns>
        public PostchainTransaction NewTransaction(byte[][] signers)
        {
            Gtx newGtx = new Gtx(this._blockchainRID);

            foreach (byte[] signer in signers)
            {
                newGtx.AddSignerToGtx(signer);
            }

            return new PostchainTransaction(newGtx, baseUri, this._blockchainRID);
        }

        ///<summary>
        ///Queries data async from the blockchain.
        ///</summary>
        ///<param name = "queryName">Name of the query in RELL.</param>
        ///<param name = "queryObject">Parameters of the query.</param>
        ///<returns>UniTask that resolves to a PostchainResponse.</returns>
        public UniTask<PostchainResponse<T>> Query<T>(string queryName, (string name, object content)[] queryObject)
        {
            var queryDict = PostchainUtil.QueryToDict(queryName, queryObject);
            string queryString = JsonConvert.SerializeObject(queryDict);

            return PostchainRequest.Post<T>(queryUri, queryString);
        }

        private async void InitializeBRIDFromChainID()
        {
            var response = await PostchainRequest.Get<string>(initBridUri);

            if (response.Error)
            {
                Debug.LogError("InitializeBRIDFromChainID: " + response.ErrorMessage);
            }
            else
            {
                this._blockchainRID = response.RawContent;
            }
        }
    }
}
