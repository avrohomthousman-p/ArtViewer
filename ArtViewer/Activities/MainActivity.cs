using Android.Content;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using ArtViewer.Network.Deviantart;

namespace ArtViewer.Activities
{

    /// <summary>
    /// Lets the user decide what he/she wants to do. Browse saved folders, edit saved folders,
    /// or find new folders to save.
    /// </summary>
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            StartBackgroundJobs();


            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);


            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);


            SetupClickListeners();
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