using Android.Content;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using ArtViewer.Network.DeviantArt;
using Java.Util.Prefs;

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
        DataScheme = "ArtViewer",
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
            var uri = intent.DataString;

            if (uri != null && uri.StartsWith("artviewer://oauth2redirect"))
            {
                //TODO: extract data we need

                ExchangeCodeForToken();
            }
        }



        private async void ExchangeCodeForToken()
        {
            //TODO: run another query for the access token
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
            FindViewById<LinearLayout>(Resource.Id.browse_my_folders_container).Click +=
                (sender, e) =>
                {
                    Intent intent = new Intent(this, typeof(ManageFoldersActivity));
                    StartActivity(intent);
                };


            FindViewById<LinearLayout>(Resource.Id.add_new_folders_container).Click +=
                (sender, e) =>
                {
                    Intent intent = new Intent(this, typeof(SearchNewFoldersActivity));
                    StartActivity(intent);
                };
        }
    }
}