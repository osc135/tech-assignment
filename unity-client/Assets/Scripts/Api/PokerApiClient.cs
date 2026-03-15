using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using HijackPoker.Models;

namespace HijackPoker.Api
{
    /// <summary>
    /// REST client for the holdem-processor API.
    ///
    /// Endpoints:
    ///   GET  /health          — Service health check
    ///   POST /process         — Advance one hand step
    ///   GET  /table/{tableId} — Fetch current table state
    ///
    /// Uses UnityWebRequest wrapped in async/await via TaskCompletionSource.
    /// Requires Newtonsoft JSON (com.unity.nuget.newtonsoft-json).
    /// </summary>
    public class PokerApiClient : MonoBehaviour, IPokerApi
    {
        [Header("API Configuration")]
        [SerializeField] private string baseUrl = "http://localhost:3030";
        [SerializeField] private float timeoutSeconds = 10f;

        /// <summary>
        /// Check if the holdem-processor is running.
        /// GET /health → HealthResponse
        /// </summary>
        public async Task<HealthResponse> GetHealthAsync()
        {
            return await SendGetRequest<HealthResponse>("/health");
        }

        /// <summary>
        /// Advance the hand by one state machine step.
        /// POST /process { "tableId": tableId } → ProcessResponse
        /// </summary>
        public async Task<ProcessResponse> ProcessStepAsync(int tableId)
        {
            return await SendPostRequest<ProcessResponse>("/process", new { tableId });
        }

        /// <summary>
        /// Fetch the full table state (game + players).
        /// GET /table/{tableId} → TableResponse
        /// </summary>
        public async Task<TableResponse> GetTableStateAsync(int tableId)
        {
            return await SendGetRequest<TableResponse>($"/table/{tableId}");
        }

        // ── HTTP helpers ─────────────────────────────────────────────

        /// <summary>
        /// Send a GET request and deserialize the JSON response.
        /// </summary>
        private async Task<T> SendGetRequest<T>(string path)
        {
            string url = baseUrl.TrimEnd('/') + path;

            using var request = UnityWebRequest.Get(url);
            request.timeout = (int)timeoutSeconds;
            request.SetRequestHeader("Accept", "application/json");

            await SendRequest(request);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"GET {path} failed: {request.error}");
                return default;
            }

            string json = request.downloadHandler.text;
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Send a POST request with a JSON body and deserialize the response.
        /// </summary>
        private async Task<T> SendPostRequest<T>(string path, object body)
        {
            string url = baseUrl.TrimEnd('/') + path;
            string jsonBody = JsonConvert.SerializeObject(body);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

            using var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = (int)timeoutSeconds;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            await SendRequest(request);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"POST {path} failed: {request.error}");
                return default;
            }

            string json = request.downloadHandler.text;
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Await a UnityWebRequest using TaskCompletionSource.
        /// This bridges Unity's coroutine-based web requests with async/await.
        /// </summary>
        private static Task SendRequest(UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }
    }
}
