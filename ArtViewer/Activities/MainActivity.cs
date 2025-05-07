using Android.Content;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using ArtViewer.Network.Deviantart;

namespace ArtViewer.Activities
{

    [Activity(Label = "@string/app_name", MainLauncher = true)]
    /// <summary>
    /// Lets the user decide what he/she wants to do. Browse saved folders, edit saved folders,
    /// or find new folders to save.
    /// </summary>
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            //Start establishing a connection to the API and database as soon as possible to avoid slowdowns
            Task.Run(() => NetworkUtils.GetAccessToken());
            Task.Run(() => DatabaseConnection.GetConnection());


            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);


            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);



            //TODO: setup button click listeners for all buttons

            FindViewById<Button>(Resource.Id.browse_my_folders_btn).Click +=
                (sender, e) =>
                {
                    Intent intent = new Intent(this, typeof(DisplayActivity));
                    StartActivity(intent);
                };


            FindViewById<Button>(Resource.Id.add_new_folders_btn).Click +=
                (sender, e) =>
                {
                    Intent intent = new Intent(this, typeof(SaveNewFoldersActivity));
                    StartActivity(intent);
                };
        }
    }
}