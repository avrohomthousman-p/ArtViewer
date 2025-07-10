using Android.Content;
using Android.Graphics;
using AndroidX.AppCompat.App;
using ArtViewer.Network.DeviantArt;
using Java.Util.Prefs;

namespace ArtViewer.Activities;



/// <summary>
/// Startup activity that redirects the user to a browser to log in to DeviantArt.
/// DeviantArt will redirect the user back to the App and provide us with an access
/// token.
/// </summary>
[Activity(MainLauncher = true, Label = "Art Viewer")]
public class LoginActivity : AppCompatActivity
{
    public const int CLIENT_ID = 48967;
    private TextView? banner = null;
    private Button? loginBtn = null;



    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_login);
        this.banner = FindViewById<TextView>(Resource.Id.banner);
        this.loginBtn = FindViewById<Button>(Resource.Id.login_btn);

        if (NetworkUtils.ShouldRequireNewLogin())
        {
            this.banner.Text = GetString(Resource.String.banner_text_reauthenticate);
            this.loginBtn.Visibility = Android.Views.ViewStates.Visible;
        }
        else
        {
            this.banner.Text = GetString(Resource.String.banner_text_refreshing_access);
            this.loginBtn.Visibility = Android.Views.ViewStates.Gone;

            RefreshSession();
        }


        AndroidX.AppCompat.Widget.Toolbar? toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(toolbar);


        loginBtn.Click += (sender, args) => LaunchWebLogin();
    }



    private async Task RefreshSession()
    {
        try
        {
            await NetworkUtils.RefreshAccessToken();
        }
        catch (Exception ex)
        {
            this.banner.Text = GetString(Resource.String.banner_text_refresh_failed);
            this.banner.SetTextColor(Color.Red);
            this.loginBtn.Visibility = Android.Views.ViewStates.Visible;
            Console.WriteLine("Error: " + ex.Message);
            MakeToastPopup("Unable to refresh your session");
            return;
        }


        this.banner.Text = GetString(Resource.String.successful_login_text);
        this.banner.SetTextColor(Color.Green);


        await Task.Delay(2500).ConfigureAwait(true);


        var intent = new Intent(this, typeof(MainActivity));
        StartActivity(intent);
    }



    /// <summary>
    /// Launches the browser so the user can login.
    /// </summary>
    private void LaunchWebLogin()
    {

        string codeVerifier = PkceUtil.GenerateCodeVerifier();
        string codeChallenge = PkceUtil.GenerateCodeChallenge(codeVerifier);


        ISharedPreferences prefs = Application.Context.GetSharedPreferences("MyPrefs", FileCreationMode.Private);
        ISharedPreferencesEditor editor = prefs.Edit();
        editor.PutString(PkceUtil.PKCE_CODE_VERIFIER_KEY, codeVerifier);
        editor.Apply();


        string authUrl = "https://www.deviantart.com/oauth2/authorize" +
                 "?response_type=code" +
                 "&client_id=" + LoginActivity.CLIENT_ID +
                 "&redirect_uri=artviewer://oauth2redirect" +
                 "&scope=browse" +
                 $"&code_challenge={codeChallenge}" +
                 "&code_challenge_method=S256";


        var uri = Android.Net.Uri.Parse(authUrl);
        var intent = new Intent(Intent.ActionView, uri);
        StartActivity(intent);
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