using Android.OS;
using Android.Util;
using Org.Apache.Http.Authentication;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ArtViewer.Network.DeviantArt
{
    public static class NetworkUtils
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static string accessToken = null;



        /// <summary>
        /// Gets the access token for the DeviantArt API.
        /// </summary>
        public static string GetAccessToken()
        {
            if (accessToken != null)
            {
                return accessToken;
            }


            semaphore.Wait();

            if (accessToken != null)
            {
                return accessToken;
            }


            try
            {
                GenerateAccessToken();
            }
            catch (Exception ex)
            {
                semaphore.Release();
                Console.WriteLine(ex.GetType() + " " + ex.Message);
                throw new Exception("Failed to retrieve access token", ex);
            }



            semaphore.Release();
            return accessToken;
        }



        /// <summary>
        /// Connects to the DeviantArt API and retrieves and saves an access token.
        /// </summary>
        private static void GenerateAccessToken()
        {
            if (accessToken != null)
            {
                return;
            }

            const string url = "https://www.deviantart.com/oauth2/token";
            Dictionary<string, string> postData = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", Secrets.client_id },
                    { "client_secret", Secrets.client_secret }
                };



            JsonDocument response = RunPostRequest(url, postData).Result;

            if (response.RootElement.TryGetProperty("access_token", out JsonElement accessTokenElement))
            {
                accessToken = accessTokenElement.ToString();
            }
            else
            {
                Log.Error("Connection Error", "Failed to retrieve access token.");
                throw new Exception("Failed to retrieve access token.");
            }
        }




        public static async Task<JsonDocument> RunGetRequest(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
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
                ["error_description"] = message
            };

            var json = JsonSerializer.Serialize(errorObject);
            return JsonDocument.Parse(json);
        }
    }
}