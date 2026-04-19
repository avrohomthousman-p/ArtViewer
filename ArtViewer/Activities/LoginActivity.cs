using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.AppCompat.App;
using ArtViewer.Network.DeviantArt;
using ArtViewer.Utils;
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
    private TextView? loginStatusView = null;
    private Button? loginBtn = null;


    private enum LoginState
    {
        LoggedOut, LoginInProgress, LoginSuccess, LoginFailure
    }



    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_login);
        this.banner = FindViewById<TextView>(Resource.Id.banner);
        this.loginStatusView = FindViewById<TextView>(Resource.Id.login_status_view);
        this.loginBtn = FindViewById<Button>(Resource.Id.login_btn);

        if (NetworkUtils.ShouldRequireNewLogin())
        {
            ApplyLoginState(LoginState.LoggedOut);
        }
        else
        {
            RefreshSession();
        }


        AndroidX.AppCompat.Widget.Toolbar? toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(toolbar);


        loginBtn.Click += (sender, args) => LaunchWebLogin();
    }



    private async Task RefreshSession()
    {
        ApplyLoginState(LoginState.LoginInProgress);

        try
        {
            await NetworkUtils.RefreshAccessToken();
            await Task.Delay(800).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ApplyLoginState(LoginState.LoginFailure);
            Console.WriteLine("Error: " + ex.Message);
            MakeToastPopup("Unable to refresh your session");
            return;
        }


        ApplyLoginState(LoginState.LoginSuccess);

        await Task.Delay(2000).ConfigureAwait(true);

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


        SharedPrefsRepository prefs = new SharedPrefsRepository();
        prefs.PkceCodeVerifier = codeVerifier;


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
    /// Updates the UI to have the correct display for where the user
    /// is holding in the login process.
    /// </summary>
    /// <param name="loginState">The user's login status</param>
    private void ApplyLoginState(LoginState loginState)
    {
        switch (loginState)
        {
            case LoginState.LoggedOut:
                this.banner.Text = GetString(Resource.String.banner_text_reauthenticate);
                Remove(this.loginStatusView);
                Show(this.loginBtn);
                break;
            case LoginState.LoginInProgress:
                Hide(this.banner);
                this.loginStatusView.Text = GetString(Resource.String.banner_text_refreshing_access);
                Show(this.loginStatusView);
                Remove(this.loginBtn);
                break;
            case LoginState.LoginSuccess:
                this.banner.Text = GetString(Resource.String.welcome_message);
                Show(this.banner);
                this.loginStatusView.Text = GetString(Resource.String.successful_login_text);
                this.loginStatusView.SetTextColor(Color.Green);
                break;
            case LoginState.LoginFailure:
                this.banner.Text = GetString(Resource.String.failed_login_text);
                this.banner.SetTextColor(Color.Red);
                Show(this.banner);
                Remove(this.loginStatusView);
                Show(this.loginBtn);
                break;
        }
    }



    private void Show(View view) => view.Visibility = ViewStates.Visible;
    private void Hide(View view) => view.Visibility = ViewStates.Invisible;
    private void Remove(View view) => view.Visibility = ViewStates.Gone;




    /// <summary>
    /// Convienent method for making a text popup on the UI thread
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="duration">The amount of time it should stay on the screen for</param>
    private void MakeToastPopup(string message, ToastLength duration = ToastLength.Short)
    {
        RunOnUiThread(() => Toast.MakeText(this, message, duration)?.Show());
    }
}