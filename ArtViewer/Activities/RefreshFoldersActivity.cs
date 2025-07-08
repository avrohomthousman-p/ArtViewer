using Android.Views;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using Bumptech.Glide;

namespace ArtViewer.Activities;

[Activity(Label = "Refresh Folders")]
public class RefreshFoldersActivity : AppCompatActivity
{
    private TextView tempView = null;


    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.display_item_in_scrollview);


        AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(toolbar);


        SetupInfoWarningView();
        InsertTemporaryLoadingView();

        PopulateScrollView();
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



    /// <summary>
    /// Builds a temporary view displaying the text "Loading...", and inserts it into the 
    /// activity. It should be removed before real data is inserted.
    /// </summary>
    private void InsertTemporaryLoadingView()
    {
        this.tempView = new TextView(this)
        {
            Text = "Loading...",
            TextSize = 24,
            TextAlignment = TextAlignment.Center
        };

        tempView.Typeface = Android.Graphics.Typeface.DefaultBold;
        tempView.SetPadding(20, 20, 20, 20);

        // Set layout parameters with margins (for a LinearLayout)
        var layoutParams = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent
        );


        layoutParams.SetMargins(20, 20, 20, 20);
        tempView.LayoutParameters = layoutParams;

        var layout = FindViewById<LinearLayout>(Resource.Id.folders_container);
        layout.AddView(tempView);
    }




    /// <summary>
    /// Remove the temporary view that says "Loading..." from the page so real data
    /// can be inserted.
    /// </summary>
    private void RemoveTemporaryLoadingView()
    {
        if (tempView != null)
        {
            var layout = FindViewById<LinearLayout>(Resource.Id.folders_container);
            layout.RemoveView(tempView);
        }
    }



    /// <summary>
    /// Fills the page with the appropriate content.
    /// </summary>
    private async Task PopulateScrollView()
    {
        var folders = await LoadFoldersFromDB();
        RemoveTemporaryLoadingView();
        BuildMainContent(folders);
    }



    /// <summary>
    /// Initiates the DB query to fetch all folders from the database.
    /// </summary>
    private async Task<IEnumerable<Folder>> LoadFoldersFromDB()
    {
        try
        {
            var folders = await StandardDBQueries.GetAllFolders();
            return folders;
        }
        catch(Exception e)
        {
            Console.WriteLine(e.GetType() + " " + e.Message);
            Toast.MakeText(this, "Error: unable to load your folders", ToastLength.Short).Show();
            return Enumerable.Empty<Folder>();
        }
    }



    /// <summary>
    /// Builds and inserts a view to display each folder.
    /// </summary>
    /// <param name="folders"></param>
    private void BuildMainContent(IEnumerable<Folder> folders)
    {
        if (folders.Count() == 0)
        {
            TextView noFolders = DefaultDisplayForNoFolders();
            return;
        }


        LinearLayout parentView = FindViewById<LinearLayout>(Resource.Id.folders_container);


        //TODO: add intro view with "check the folders you want to refresh"


        LayoutInflater inflater = LayoutInflater.From(this);

        foreach (Folder folder in folders)
        {
            View folderContainer = inflater.Inflate(Resource.Layout.display_candidate_for_refresh, parentView, false);


            TextView folderName = folderContainer.FindViewById<TextView>(Resource.Id.folder_name);
            folderName.Text = folder.CustomName;


            ImageView thumbnailImageView = folderContainer.FindViewById<ImageView>(Resource.Id.folder_icon);
            Glide.With(this)
                  .Load(folder.ThumbnailUrl)
                  .Into(thumbnailImageView);


            //TODO: handle checked and unchecked


            parentView.AddView(folderContainer);
        }
    }



    /// <summary>
    /// Builds a TextView that displays the default message for when there are no folders.
    /// </summary>
    private TextView DefaultDisplayForNoFolders()
    {
        return new TextView(this)
        {
            Text = "No folders found",
            TextSize = Resources.GetDimension(Resource.Dimension.large_text),
            Gravity = GravityFlags.Center,
            LayoutParameters = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.WrapContent
                )
            { TopMargin = 20 },
        };
    }
}