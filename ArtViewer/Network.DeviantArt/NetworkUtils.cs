using Android.Util;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ArtViewer.Network.Deviantart
{
    public static class NetworkUtils
    {
        public static async Task<JsonDocument> RunGetRequest(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonDocument.Parse(result);
                }
                catch (HttpRequestException e)
                {
                    Log.Error("Connection Error", $"Request error: {e.Message}");
                    return CreateErrorJson(e.Message);
                }
            }
        }



        public static async Task<JsonDocument> RunPostRequest(string url, Dictionary<string, string> arguments)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var postData = new FormUrlEncodedContent(arguments);
                    HttpResponseMessage response = await client.PostAsync(url, postData);
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return JsonDocument.Parse(result);
                }
                catch (HttpRequestException e)
                {
                    Log.Error("Connection Error", $"Request error: {e.Message}");
                    return CreateErrorJson(e.Message);
                }
            }
        }



        /// <summary>
        /// Creates a JSON document with an error message. Used as a response when an HTTP call fails.
        /// </summary>
        private static JsonDocument CreateErrorJson(string message)
        {
            var errorObject = new Dictionary<string, object>
            {
                ["status"] = "error",
                ["message"] = message
            };

            var json = JsonSerializer.Serialize(errorObject);
            return JsonDocument.Parse(json);
        }

    }
}