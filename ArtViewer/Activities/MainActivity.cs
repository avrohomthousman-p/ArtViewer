using Android.Content;
using ArtViewer.Network.Deviantart;

namespace ArtViewer.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            //Start establishing a connection to the API as soon as possible to avoid slowdowns
            //FIXME: modify this code to catch a connection error and make a popup on the front end
            Task newTask = Task.Run(() => NetworkUtils.GetAccessToken());


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