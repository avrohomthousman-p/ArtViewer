using Android.Content;
using AndroidX.AppCompat.App;
using ArtViewer.Network.DeviantArt;
using Java.Util.Prefs;

namespace ArtViewer.Activities;



/// <summary>
/// Startup activity that redirects the user to a browser to log in to DeviantArt.
/// DeviantArt will redirect the user back to the App and provide us with an access
/// token.
/// </summary>
[Activity(MainLauncher = true, Label = "Login Page")]
public class LoginActivity : AppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_login);

        AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(toolbar);


        //TODO: add option for logging in with personal token

        Button loginBtn = FindViewById<Button>(Resource.Id.login_btn);
        loginBtn.Click += (sender, args) => LaunchWebLogin();
    }



    /// <summary>
    /// Luanches the browser so the user can login.
    /// </summary>
    private void LaunchWebLogin()
    {

        string codeVerifier = PkceUtil.GenerateCodeVerifier();
        string codeChallenge = PkceUtil.GenerateCodeChallenge(codeVerifier);


        ISharedPreferences prefs = Application.Context.GetSharedPreferences("MyPrefs", FileCreationMode.Private);
        ISharedPreferencesEditor editor = prefs.Edit();
        editor.PutString("pkce_code_verifier", codeVerifier);
        editor.Apply();


        string authUrl = "https://www.deviantart.com/oauth2/authorize" +
                 "?response_type=token" +
                 "&client_id=" + Secrets.client_id +
                 "&redirect_uri=artviewer://oauth2redirect" +
                 "&scope=browse user.profile" +
                 "&state=some_random_state" +
                 $"&code_challenge={codeChallenge}" +
                 $"&code_challenge_method=S256";
                 //"";


        var uri = Android.Net.Uri.Parse(authUrl);
        var intent = new Intent(Intent.ActionView, uri);
        StartActivity(intent);
    }
}