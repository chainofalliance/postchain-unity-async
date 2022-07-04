using System;
using UnityEngine.Networking;

using Cysharp.Threading.Tasks;

namespace Chromia.Postchain.Client.Unity
{
    public class PostchainRequest
    {
        public static async UniTask<PostchainResponse<T>> Get<T>(string baseUrl, string path)
        {
            return await Get<T>(ToUri(baseUrl, path));
        }

        public static async UniTask<PostchainResponse<T>> Get<T>(Uri uri)
        {
            var response = await UnityWebRequest.Get(uri).SendWebRequest();

            return new PostchainResponse<T>(response);
        }

        public static async UniTask<PostchainResponse<T>> Post<T>(string baseUrl, string path, string payload)
        {
            return await Post<T>(ToUri(baseUrl, path), payload);
        }

        public static async UniTask<PostchainResponse<T>> Post<T>(Uri uri, string payload)
        {
            using (var request = new UnityWebRequest(uri, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
                var uploader = new UploadHandlerRaw(bodyRaw);

                uploader.contentType = "application/json";

                request.uploadHandler = uploader;
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                var response = await request.SendWebRequest();

                return new PostchainResponse<T>(response);
            };
        }

        public static Uri ToUri(string baseUrl, string path)
        {
            return new Uri(new Uri(baseUrl), path);
        }
    }
}