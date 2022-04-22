using Newtonsoft.Json;
using System.Collections;

namespace Chromia.Postchain.Client.Unity
{
    public class PostchainQuery<T>
    {
        public bool error = false;
        public string errorMessage = "";
        public T content;

        private string _urlBase;
        private string _brid;

        public PostchainQuery(string urlBase, string brid)
        {
            this._urlBase = urlBase;
            this._brid = brid;
        }

        public IEnumerator Query(string queryName, (string name, object content)[] queryObject)
        {
            var queryDict = PostchainUtil.QueryToDict(queryName, queryObject);
            string queryString = JsonConvert.SerializeObject(queryDict);

            var request = new PostchainRequest<T>(this._urlBase, "query/" + this._brid);
            yield return request.Post(queryString);

            this.error = request.error;
            this.errorMessage = request.errorMessage;

            if (!this.error)
            {
                this.content = request.parsedContent;
            }
        }
    }
}