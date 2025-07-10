using Android.Content;
using Android.Graphics;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using ArtViewer.Network.DeviantArt;
using System.Web;

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
    public class RedirectActivity : AppCompatActivity
    {
        private TextView outputView;


        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleRedirect(base.Intent);

            SetContentView(Resource.Layout.activity_redirect);


            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);


            outputView = FindViewById<TextView>(Resource.Id.login_status_text_view);
        }



        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            this.HandleRedirect(intent);
        }



        /// <summary>
        /// Calls the Deviantart API's to get the access token and refresh token, 
        /// then redirects to the main activity.
        /// </summary>
        private async Task HandleRedirect(Intent? intent)
        {
            string? uriString = intent?.DataString;

            try
            {
                if (uriString == null || !uriString.StartsWith("artviewer://oauth2redirect"))
                {
                    Console.WriteLine("Invaid redirect url");
                    throw new Exception("Invalid redirect");
                }


                Uri uri = new Uri(uriString);
                var queryCollection = HttpUtility.ParseQueryString(uri.Query);
                string? code = queryCollection.Get("code");

                if (code == null)
                {
                    Console.WriteLine("No authorization code provided by deviantart");
                    throw new Exception("DeviantArt has denied access");
                }

                await NetworkUtils.GenerateAccessToken(code.ToString());
                this.SetSuccessMessage();

                await Task.Delay(1500);

                Intent outgoingIntent = new Intent(this, typeof(MainActivity));
                StartActivity(outgoingIntent);
                Finish();
            }
            catch (Exception ex)
            {
                SetErrorMessage();
                //TODO: offer user a way to try again???
            }
        }




        /// <summary>
        /// Updates the output view to visually indicate a successful login.
        /// </summary>
        private void SetSuccessMessage()
        {
            this.outputView.Text = Resources.GetString(Resource.String.successful_login_text);
            this.outputView.SetTextColor(Color.Green);
        }



        /// <summary>
        /// Updates the output view to visually indicate a failed login.
        /// </summary>
        private void SetErrorMessage()
        {
            this.outputView.Text = Resources.GetString(Resource.String.failed_login_text);
            this.outputView.SetTextColor(Color.Red);
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
    }
}