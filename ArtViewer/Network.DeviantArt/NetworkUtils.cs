using Android.OS;
using Android.Util;
using Android.Content;
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
        /// Connects to the cloud worker and gets an access token for the DeviantArt API from it.
        /// </summary>
        private static void GenerateAccessToken(bool isRetry=false)
        {
            if (accessToken != null)
            {
                return;
            }

            RegisterApp();

            string url = "https://verification-server-morning-thunder-6fdd.avrohomthousman.workers.dev/accessToken?appID="
                                    + SecurePreferences.DecryptAppID();

            JsonDocument response = RunGetRequest(url).Result;


            if (response.RootElement.TryGetProperty("access_token", out JsonElement accessTokenElement))
            {
                accessToken = accessTokenElement.ToString();
            }
            else
            {
                //Corner case. If we have a saved appID, but it no longer exists on the cloud worker
                if (response.RootElement.TryGetProperty("error", out JsonElement errorMsg))
                {
                    if (!isRetry && errorMsg.ToString() == "Access Denied")
                    {
                        SecurePreferences.DeleteStoredAppID();
                        GenerateAccessToken(true);
                        return;
                    }
                }


                Log.Error("Connection Error", "Failed to retrieve access token.");
                throw new Exception("Failed to retrieve access token.");
            }
        }



        /// <summary>
        /// Connects to a cloud worker to register this app if it isnt already registered. This gets 
        /// the AppID that is used to connect to the could worker again and fetch an API access token
        /// for DeviantArt.
        /// </summary>
        private static void RegisterApp()
        {
            if (SecurePreferences.AppIDExists())
            {
                return;
            }


            const string url = "https://verification-server-morning-thunder-6fdd.avrohomthousman.workers.dev/register";
            JsonDocument response = RunGetRequest(url).Result;


            if (response.RootElement.TryGetProperty("appID", out JsonElement appID))
            {
                SecurePreferences.EncryptAppID(appID.ToString());
            }
            else
            {
                Log.Error("Connection Error", "Failed to register app.");
                throw new Exception("Failed to register app.");
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