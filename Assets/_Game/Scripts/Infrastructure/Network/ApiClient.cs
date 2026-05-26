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

        public IEnumerator PostJson(string path, string jsonBody, Action<string> onSuccess, Action<string> onError)
        {
            using var request = new UnityWebRequest($"{BaseUrl}{path}", "POST");
            byte[] body = Encoding.UTF8.GetBytes(jsonBody ?? "{}");
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyAuth(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.text);
            else
                onError?.Invoke(request.error);
        }

        public IEnumerator PutJson(string path, string jsonBody, Action<string> onSuccess, Action<string> onError)
        {
            using var request = new UnityWebRequest($"{BaseUrl}{path}", "PUT");
            byte[] body = Encoding.UTF8.GetBytes(jsonBody ?? "{}");
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyAuth(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.text);
            else
                onError?.Invoke(request.error);
        }

        public IEnumerator Get(string path, Action<string> onSuccess, Action<string> onError)
        {
            using var request = UnityWebRequest.Get($"{BaseUrl}{path}");
            ApplyAuth(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.text);
            else
                onError?.Invoke(request.error);
        }

        private void ApplyAuth(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(AuthToken))
                request.SetRequestHeader("Authorization", $"Bearer {AuthToken}");
        }
    }
}
