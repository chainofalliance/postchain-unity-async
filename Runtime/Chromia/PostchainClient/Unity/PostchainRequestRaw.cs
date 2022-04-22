using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

namespace Chromia.Postchain.Client.Unity
{
    public class PostchainRequestRaw
    {
        public bool error = false;
        public string errorMessage = "";
        public string content;

        protected Uri _uri;

        public PostchainRequestRaw(Uri uri)
        {
            this._uri = uri;
        }

        public PostchainRequestRaw(string urlBase, string path)
        {
            this._uri = new Uri(new Uri(urlBase), path);
        }

        public virtual IEnumerator Get()
        {
            var request = new UnityWebRequest(this._uri, "GET");
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (!CheckError(request))
            {
                this.content = request.downloadHandler.text;
            }
        }

        public virtual IEnumerator Post(string payload)
        {
            var request = new UnityWebRequest(this._uri, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            var uploader = new UploadHandlerRaw(bodyRaw);

            uploader.contentType = "application/json";

            request.uploadHandler = uploader;
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (!CheckError(request))
            {
                this.content = request.downloadHandler.text;
            }
        }

        private bool CheckError(UnityWebRequest request)
        {
            this.error = request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.DataProcessingError
                || request.result == UnityWebRequest.Result.ProtocolError;
            this.errorMessage = request.error;

            return this.error;
        }
    }
}