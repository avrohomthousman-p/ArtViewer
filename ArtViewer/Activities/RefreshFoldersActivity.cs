using Android.Views;
using AndroidX.AppCompat.App;

namespace ArtViewer.Activities;

[Activity(Label = "Refresh Folders")]
public class RefreshFoldersActivity : AppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.display_item_in_scrollview);


        AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(toolbar);


        SetupInfoWarningView();
    }



    /// <summary>
    /// Builds a TextView that the user can tap onto get an explenation of what this activity does.
    /// </summary>
    private void SetupInfoWarningView()
    {
        TextView warningText = new TextView(this)
        {
            Text = "!! Important: Please Tap To Read !!",
            TextSize = 16,
            TextAlignment = TextAlignment.Center
        };


        warningText.SetTextColor(Android.Graphics.Color.Red);
        warningText.Typeface = Android.Graphics.Typeface.DefaultBold;
        warningText.SetPadding(20, 20, 20, 20);
        warningText.Click += (sender, e) =>
        {
            LaunchInfoPopup();
        };



        var layoutParams = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent
        );


        layoutParams.SetMargins(20, 20, 20, 20);
        warningText.LayoutParameters = layoutParams;



        LinearLayout parentView = FindViewById<LinearLayout>(Resource.Id.folders_container);
        parentView.AddView(warningText);
    }



    /// <summary>
    /// Launches an info popup that explains what the happens when a refresh is done.
    /// </summary>
    private void LaunchInfoPopup()
    {
        AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        builder.SetTitle("Warning");

        LayoutInflater inflater = LayoutInflater.From(this);
        View dialogView = inflater.Inflate(Resource.Layout.refresh_folder_dialog_content, null);
        builder.SetView(dialogView);

        builder.SetPositiveButton("OK", (senderAlert, args) => {});
        builder.SetCancelable(true);

        AndroidX.AppCompat.App.AlertDialog dialog = builder.Create();
        dialog.Show();
    }



    private async Task LoadFoldersFromDB()
    {
        try
        {
            //TODO: query folders and insert them onto the page
        }
        catch(Exception e)
        {
            //TODO: log the error and make a 
        }
    }
}