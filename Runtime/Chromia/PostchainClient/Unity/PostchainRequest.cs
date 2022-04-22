using System;
using Newtonsoft.Json;
using System.Collections;

namespace Chromia.Postchain.Client.Unity
{
    public class PostchainRequest<T> : PostchainRequestRaw
    {
        public T parsedContent;

        public PostchainRequest(Uri uri): base(uri)
        {}

        public PostchainRequest(string urlBase, string path): base(urlBase, path)
        {}

        public override IEnumerator Get()
        {
            yield return base.Get();

            if (!error)
            {
                this.parsedContent = JsonConvert.DeserializeObject<T>(this.content);
            }
        }

        public override IEnumerator Post(string payload)
        {
            yield return base.Post(payload);

            if (!error)
            {
                this.parsedContent = JsonConvert.DeserializeObject<T>(this.content);
            }
        }
    }
}