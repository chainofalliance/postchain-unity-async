using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Chromia.Postchain.Client
{
    public class PostchainResponse<T>
    {
        public bool Error => _error;
        public string ErrorMessage => _errorMessage;
        public T Content => JsonConvert.DeserializeObject<T>(_rawContent);
        public string RawContent => _rawContent;

        private bool _error;
        private string _errorMessage;
        private string _rawContent;

        public static PostchainResponse<T> SuccessResponse()
        {
            return new PostchainResponse<T>()
            {
                _error = false,
                _errorMessage = null,
                _rawContent = null
            };
        }

        public static PostchainResponse<T> ErrorResponse(string errorMessage)
        {
            return new PostchainResponse<T>()
            {
                _error = true,
                _errorMessage = errorMessage,
                _rawContent = null
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