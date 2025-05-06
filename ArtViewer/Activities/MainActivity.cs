using Android.Content;
using ArtViewer.Database;
using ArtViewer.Network.Deviantart;

namespace ArtViewer.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            //Start establishing a connection to the API and database as soon as possible to avoid slowdowns
            Task.Run(() => NetworkUtils.GetAccessToken());
            Task.Run(() => DatabaseConnection.GetConnection());


            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);



            //TODO: setup button click listeners for all buttons

            FindViewById<Button>(Resource.Id.browse_my_folders_btn).Click +=
                (sender, e) =>
                {
                    Intent intent = new Intent(this, typeof(DisplayActivity));
                    StartActivity(intent);
                };
        }
    }
}