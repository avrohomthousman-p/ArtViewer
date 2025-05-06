using Android.Content;

namespace ArtViewer
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);



            //TODO: setup button click listeners for all buttons

            FindViewById<Button>(Resource.Id.browse_my_folders_btn).Click +=
                (Object sender, EventArgs e) =>
                {
                    Intent intent = new Intent(this, typeof(DisplayActivity));
                    StartActivity(intent);
                };
        }
    }
}