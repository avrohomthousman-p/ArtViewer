using Android.OS;
using Android.Util;
using ImageSearch;
using Org.Apache.Http.Authentication;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ArtViewer.Network.Deviantart
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
                GenerateAccessToken().Wait();
            }
            catch (Exception ex)
            {
                semaphore.Release();
                throw new Exception("Failed to retrieve access token", ex);
            }



            semaphore.Release();
            return accessToken;
        }



        /// <summary>
        /// Connects to the DeviantArt API and retrieves and saves an access token.
        /// </summary>
        private static async Task GenerateAccessToken()
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



            JsonDocument response = await RunPostRequest(url, postData);

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




        /// <summary>
        /// Builds the url for getting the images in a users gallery.
        /// </summary>
        /// <param name="username">the username of the gallery owner</param>
        /// <param name="queryLimit">the number of images to fetch in the query</param>
        /// <param name="offset">the position in the gallery/collection where the query should start</param>
        /// <returns>a url that will connect to the DeviantArt API and get images from the users gallery</returns>
        public static string BuildFullGalleryUrl(string username, int queryLimit, int offset)
        {
            string usersFullGalleryUrl = "https://www.deviantart.com/api/v1/oauth2/gallery/all?access_token=" +
                                "{0}&username={1}&mature_content=true&limit={2}&offset={3}";


            return string.Format(usersFullGalleryUrl, GetAccessToken(), username, queryLimit, offset);
        }




        /// <summary>
        /// Builds the url for getting the images from a specific folder.
        /// </summary>
        /// <param name="storageType">where the folder is located. "collection" or "gallery"</param>
        /// <param name="folderId">the id the folder was given by DeviantArt</param>
        /// <param name="username">the username of the folder owner</param>
        /// <param name="queryLimit">the number of images to fetch in the query</param>
        /// <param name="offset">the position in the gallery/collection where the query should start</param>
        /// <returns>a url that will connect to the DeviantArt API and get images from a specific folder</returns>
        public static string BuildGenericFolderUrl(string storageType, string folderId, string username, int queryLimit, int offset)
        {
            string userFolderUrl = "https://www.deviantart.com/api/v1/oauth2/{0}/{1}?access_token=" +
                                "{2}&username={3}&mature_content=true&limit={4}&offset={5}";


            return string.Format(userFolderUrl, storageType, folderId, GetAccessToken(), username, queryLimit, offset);
        }
    }
}