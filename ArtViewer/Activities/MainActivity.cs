using Android.Content;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using ArtViewer.Network.Deviantart;
using Java.Util.Prefs;
using System.Text.Json;

namespace ArtViewer.Activities
{

    /// <summary>
    /// Lets the user decide what he/she wants to do. Browse saved folders, edit saved folders,
    /// or find new folders to save.
    /// </summary>
    [Activity(Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "artviewer",
        DataHost = "oauth2redirect"
    )]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            //StartBackgroundJobs();


            base.OnCreate(savedInstanceState);
            HandleRedirect(base.Intent);

            SetContentView(Resource.Layout.activity_main);


            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);


            SetupClickListeners();
        }



        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            HandleRedirect(intent);
        }



        private void HandleRedirect(Intent intent)
        {
            string uri = intent.DataString;

            if (uri != null && uri.StartsWith("artviewer://oauth2redirect"))
            {
                //var code = uri.GetQueryParameter("code");
                //var state = uri.GetQueryParameter("state");
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine(uri);
                //TODO: analyze incoming data and extract what we need

                //Console.WriteLine($"Authorization Code: {code}");
                //ExchangeCodeForToken(code);
            }
        }



        //TODO: extract to network folder
        private async void ExchangeCodeForToken(string code)
        {
            ISharedPreferences prefs = Application.Context.GetSharedPreferences("MyPrefs", FileCreationMode.Private);
            string codeVerifier = prefs.GetString("pkce_code_verifier", "");

            var values = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", "YOUR_CLIENT_ID" },
                { "redirect_uri", "artviewer://oauth2redirect" },
                { "code", code },
                { "code_verifier", codeVerifier }
            };

            var content = new FormUrlEncodedContent(values);
            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync("https://www.deviantart.com/oauth2/token", content);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Token Response: " + json);

            // Parse and store access_token
            JsonDocument doc = JsonDocument.Parse(json);
            //TODO: extract access token

            //var accessToken = tokenObj["access_token"];
            //await SecureStorage.SetAsync("access_token", accessToken);
        }




        /// <summary>
        /// Runs background threads to establish connects to the database and the DeviantArt API.
        /// These connections are not needed by this activity, but will be needed by other activities.
        /// They are fetched here only to keep those other activitys moving quickly.
        /// </summary>
        private void StartBackgroundJobs()
        {
            Task.Run(() =>
            {
                try
                {
                    NetworkUtils.GetAccessToken();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.GetType() + " " + e.Message);
                    MakeToastPopup("Unable to connect to DeviantArt .....", ToastLength.Long);
                }
            });


            Task.Run(() => DatabaseConnection.GetConnection());
        }



        /// <summary>
        /// Convienent method for making a text popup on the UI thread
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="duration">The amount of time it should stay on the screen for</param>
        private void MakeToastPopup(string message, ToastLength duration = ToastLength.Short)
        {
            RunOnUiThread(() => Toast.MakeText(this, message, duration).Show());
        }



        /// <summary>
        /// Sets all the click listeners for the buttons in the layout
        /// </summary>
        private void SetupClickListeners()
        {
            FindViewById<Button>(Resource.Id.browse_my_folders_btn).Click +=
                (sender, e) =>
                {
                    Intent intent = new Intent(this, typeof(ManageFoldersActivity));
                    StartActivity(intent);
                };


            FindViewById<Button>(Resource.Id.add_new_folders_btn).Click +=
                (sender, e) =>
                {
                    Intent intent = new Intent(this, typeof(SearchNewFoldersActivity));
                    StartActivity(intent);
                };
        }
    }
}