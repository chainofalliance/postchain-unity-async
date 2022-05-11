using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

namespace Chromia.Postchain.Client
{
    public class PostchainResponse<T>
    {
        public bool Error => _error;
        public string ErrorMessage => _errorMessage;
        public string RawContent => _rawContent;
        public T Content
        {
            get
            {
                if (_content == null)
                {
                    if (String.IsNullOrEmpty(_rawContent))
                    {
                        return default(T);
                    }

                    _content = JsonConvert.DeserializeObject<T>(_rawContent);
                }

                return _content;
            }
        }

        private bool _error;
        private string _errorMessage;
        private string _rawContent;
        private T _content;

        public static PostchainResponse<T> SuccessResponse()
        {
            return new PostchainResponse<T>()
            {
                _error = false,
                _errorMessage = null,
                _rawContent = null,
                _content = default(T)
            };
        }

        public static PostchainResponse<T> SuccessResponse(T content)
        {
            return new PostchainResponse<T>()
            {
                _error = false,
                _errorMessage = null,
                _rawContent = null,
                _content = content
            };
        }

        public static PostchainResponse<T> ErrorResponse(string errorMessage)
        {
            return new PostchainResponse<T>()
            {
                _error = true,
                _errorMessage = errorMessage,
                _rawContent = null,
                _content = default(T)
            };
        }

        private PostchainResponse() { }

        public PostchainResponse(UnityWebRequest request)
        {
            CheckError(request);
            _rawContent = request.downloadHandler.text;
        }

        private void CheckError(UnityWebRequest request)
        {
            this._error = request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.DataProcessingError
                || request.result == UnityWebRequest.Result.ProtocolError;
            this._errorMessage = request.error;
        }
    }
}