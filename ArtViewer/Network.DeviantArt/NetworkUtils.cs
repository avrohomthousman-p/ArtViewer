using Android.Content;
using Android.OS;
using Android.Util;
using ArtViewer.Activities;
using ArtViewer.Utils;
using System.Net.Http.Headers;
using System.Text.Json;



namespace ArtViewer.Network.DeviantArt
{
    public static class NetworkUtils
    {
        private static string? accessToken = null;



        /// <summary>
        /// Gets the access token for the DeviantArt API if there is one.
        /// Ensure you call GenerateAccessToken before calling this.
        /// </summary>
        /// <returns>The current DeviantArt access token.</returns>
        /// <exception cref="Exception">
        /// Thrown when no access token has been generated.
        /// </exception>
        public static string GetAccessToken()
        {
            if (NetworkUtils.accessToken == null)
            {
                throw new Exception("No access token for DeviantArt.");
            }

            return NetworkUtils.accessToken;
        }



        /// <summary>
        /// Gets an access token from the DeviantArt API using the authorization code
        /// they provided after a successfull login.
        /// <param name="authorizationCode">The authorization code given by the API after logging in</param>
        /// </summary>
        public static async Task GenerateAccessToken(string authorizationCode)
        {
            if (NetworkUtils.accessToken != null)
            {
                return;
            }


            SharedPrefsRepository prefs = new SharedPrefsRepository();
            string? codeVerifier = prefs.PkceCodeVerifier;


            var requestData = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", LoginActivity.CLIENT_ID.ToString() },
                { "redirect_uri", "artviewer://oauth2redirect" },
                { "code", authorizationCode },
                { "code_verifier", codeVerifier }
            };

            var response = await RunPostRequest("https://www.deviantart.com/oauth2/token", requestData);


            if (response.RootElement.TryGetProperty("status", out JsonElement statusElement))
            {
                if (statusElement.GetString() == "error")
                {
                    string? message = response.RootElement.GetProperty("error_description").GetString();
                    throw new Exception(message);
                }
            }


            NetworkUtils.accessToken = response.RootElement.GetProperty("access_token").GetString();
            string? refreshToken = response.RootElement.GetProperty("refresh_token").GetString();
            DateTime exprirationDate = DateTime.UtcNow.AddMonths(3);


            prefs.RefreshToken = refreshToken;
            prefs.RefreshTokenExpirationDate = exprirationDate;
        }



        /// <summary>
        /// Determans if the user needs to log in again becuase we cannot use the refresh token
        /// to get an access token.
        /// </summary>
        /// <returns>True if we need the user to reauthenticate and false otherwise</returns>
        public static bool ShouldRequireNewLogin()
        {
            SharedPrefsRepository prefs = new SharedPrefsRepository();
            DateTime? expirationDate = prefs.RefreshTokenExpirationDate;


            if(expirationDate == null)
                return true;


            return DateTime.UtcNow >= expirationDate;
        }



        /// <summary>
        /// Gets an access token using the refresh token saved in shared preferences, so
        /// the user doesnt need to login manually. Do not call this function unless you
        /// have confirmed that the refresh token is not expired by calling the method
        /// ShouldRequireNewLogin.
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no saved refresh token</exception>
        /// <exception cref="Exception"></exception>
        public static async Task RefreshAccessToken()
        {
            SharedPrefsRepository prefs = new SharedPrefsRepository();
            string? refreshToken = prefs.RefreshToken;

            if (string.IsNullOrEmpty(refreshToken))
                throw new InvalidOperationException("No refresh token stored.");


            const string URL = "https://www.deviantart.com/oauth2/token";
            var requestData = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", LoginActivity.CLIENT_ID.ToString() },
                { "refresh_token", refreshToken }
            };


            JsonDocument response = await RunPostRequest(URL, requestData);


            if (response.RootElement.TryGetProperty("status", out JsonElement statusElement))
            {
                if (statusElement.GetString() == "error")
                {
                    string? message = response.RootElement.GetProperty("error_description").GetString();
                    throw new Exception(message);
                }
            }


            NetworkUtils.accessToken = response.RootElement.GetProperty("access_token").GetString();

            //Update the refresh token
            prefs.RefreshToken = response.RootElement.GetProperty("refresh_token").GetString();
            prefs.RefreshTokenExpirationDate = DateTime.UtcNow.AddMonths(3);
        }




        /// <summary>
        /// Logs the user out of DeviantArt for this session only. Also deletes the access token and refresh token.
        /// </summary>
        public static async Task LogOutOfDeviantArt()
        {
            SharedPrefsRepository prefs = new SharedPrefsRepository();
            
            string? token = prefs.RefreshToken ?? NetworkUtils.accessToken;


            if (String.IsNullOrEmpty(token))
                return; //Already logged out


            string url = "https://www.deviantart.com/oauth2/revoke";
            var requestData = new Dictionary<string, string>
            {
                { "revoke_refresh_only", "true" },
                { "token", token }
            };

            await RunPostRequest(url, requestData);

            NetworkUtils.accessToken = null;
            prefs.RefreshToken = null;
            prefs.RefreshTokenExpirationDate = null;
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



        /// <summary>
        /// This functions runs a regular get request except that it includes a header with
        /// the DeviantArt access token from the GetAccessToken function.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>The response data from the request</returns>
        public static async Task<JsonDocument> RunAuthorizedGetRequest(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", NetworkUtils.GetAccessToken());
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



        public static async Task<JsonDocument> RunPostRequest(string url, Dictionary<string, string> postData)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var payload = new FormUrlEncodedContent(postData);
                    HttpResponseMessage response = await client.PostAsync(url, payload);
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