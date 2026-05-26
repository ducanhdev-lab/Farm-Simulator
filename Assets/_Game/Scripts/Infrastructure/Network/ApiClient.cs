using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace IslandHarvest.Game
{
    public class ApiClient
    {
        public string BaseUrl { get; set; } = "http://localhost:3000";
        public string AuthToken { get; set; }

        public IEnumerator PostJson(string path, string jsonBody, Action<ApiResult> onComplete) =>
            SendJson(UnityWebRequest.kHttpVerbPOST, path, jsonBody, onComplete);

        public IEnumerator PutJson(string path, string jsonBody, Action<ApiResult> onComplete) =>
            SendJson(UnityWebRequest.kHttpVerbPUT, path, jsonBody, onComplete);

        public IEnumerator Get(string path, Action<ApiResult> onComplete) =>
            SendGet(path, onComplete);

        private IEnumerator SendJson(string method, string path, string jsonBody, Action<ApiResult> onComplete)
        {
            using var request = new UnityWebRequest($"{BaseUrl}{path}", method);
            byte[] body = Encoding.UTF8.GetBytes(jsonBody ?? "{}");
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyAuth(request);

            yield return request.SendWebRequest();

            long status = request.responseCode;
            if (request.result == UnityWebRequest.Result.Success)
                onComplete?.Invoke(ApiResult.Ok(status, request.downloadHandler.text));
            else
                onComplete?.Invoke(ApiResult.Fail(status, request.error));
        }

        private IEnumerator SendGet(string path, Action<ApiResult> onComplete)
        {
            using var request = UnityWebRequest.Get($"{BaseUrl}{path}");
            ApplyAuth(request);

            yield return request.SendWebRequest();

            long status = request.responseCode;
            if (request.result == UnityWebRequest.Result.Success)
                onComplete?.Invoke(ApiResult.Ok(status, request.downloadHandler.text));
            else
                onComplete?.Invoke(ApiResult.Fail(status, request.error));
        }

        private void ApplyAuth(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(AuthToken))
                request.SetRequestHeader("Authorization", $"Bearer {AuthToken}");
        }
    }
}
