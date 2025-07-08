using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using Bumptech.Glide;

namespace ArtViewer.Activities;

[Activity(Label = "Refresh Folders")]
public class RefreshFoldersActivity : AppCompatActivity
{
    private Folder[] folders = new Folder[0];
    private bool[] selected = new bool[0];
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
        await LoadFoldersFromDB();
        RemoveTemporaryLoadingView();
        BuildMainContent();
    }



    /// <summary>
    /// Initiates the DB query to fetch all folders from the database, and saves the results to
    /// the local class variable.
    /// </summary>
    private async Task LoadFoldersFromDB()
    {
        try
        {
            var loadedFolders = await StandardDBQueries.GetAllFolders();
            this.folders = loadedFolders.ToArray();
            this.selected = Enumerable.Repeat(true, this.folders.Length).ToArray();
        }
        catch(Exception e)
        {
            Console.WriteLine(e.GetType() + " " + e.Message);
            Toast.MakeText(this, "Error: unable to load your folders", ToastLength.Short).Show();
        }
    }



    /// <summary>
    /// Builds and inserts a view to display each folder.
    /// </summary>
    /// <param name="folders"></param>
    private void BuildMainContent()
    {
        if (this.folders.Length == 0)
        {
            TextView noFolders = DefaultDisplayForNoFolders();
            return;
        }


        LinearLayout parentView = FindViewById<LinearLayout>(Resource.Id.folders_container);


        TextView introView = BuildIntroView();
        parentView.AddView(introView);


        LayoutInflater inflater = LayoutInflater.From(this);

        Folder folder;
        for (int i = 0; i < this.folders.Length; i++)
        {
            folder = this.folders[i];

            View folderContainer = inflater.Inflate(Resource.Layout.display_candidate_for_refresh, parentView, false);


            TextView folderName = folderContainer.FindViewById<TextView>(Resource.Id.folder_name);
            folderName.Text = folder.CustomName;


            ImageView thumbnailImageView = folderContainer.FindViewById<ImageView>(Resource.Id.folder_icon);
            Glide.With(this)
                  .Load(folder.ThumbnailUrl)
                  .Into(thumbnailImageView);



            int index = i;
            CheckBox checkBox = folderContainer.FindViewById<CheckBox>(Resource.Id.should_refresh_checkbox);
            checkBox.CheckedChange += (sender, e) => this.selected[index] = !this.selected[index];



            parentView.AddView(folderContainer);
        }


        parentView.AddView(BuiltSubmitButton());
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



    /// <summary>
    /// Builds an intro text view with simple instructions.
    /// </summary>
    private TextView BuildIntroView()
    {
        return new TextView(this)
        {
            Text = "Check off the folders you want to refresh",
            TextSize = 16,
            Gravity = GravityFlags.Center,
            LayoutParameters = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.WrapContent
                )
            { TopMargin = 20 },
        };
    }



    /// <summary>
    /// Build button that initaiates the folder refresh.
    /// </summary>
    private Button BuiltSubmitButton()
    {
        Button submitBtn = new Button(this)
        {
            Text = "Start Refresh"
        };

        submitBtn.SetBackgroundResource(Resource.Drawable.rounded_button);
        submitBtn.SetTextColor(Android.Graphics.Color.White);


        var layoutParams = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.WrapContent,
            ViewGroup.LayoutParams.WrapContent
        );
        layoutParams.SetMargins(20, 20, 20, 30);
        submitBtn.LayoutParameters = layoutParams;
        submitBtn.SetPadding(30, 20, 30, 20);



        submitBtn.Click += (sender, e) =>
        {
            RefreshFolders();
        };


        return submitBtn;
    }



    /// <summary>
    /// Runs the refresh folders job in the background, and prevents users from doing anything until its done.
    /// </summary>
    private async Task RefreshFolders()
    {
        //Block user input while the process runs
        AndroidX.AppCompat.App.AlertDialog dialog = BuildDoNotCloseAppDialog();
        dialog.Show();


        for(int i = 0; i < this.folders.Length; i++)
        {
            if (!this.selected[i])
                continue;


            //TODO: launch API query to load this folder and save results to the DB
        }


        //TODO: on complete send user back to folders activity
    }



    /// <summary>
    /// Builds a non closable popup that should be shown while the refresh process runs, so that 
    /// users cant input anything until the process finishes.
    /// 
    /// Note: the dialog is built and returned but not shown. You must call dialog.Show() on the
    /// returned object.
    /// </summary>
    /// <returns>A reference to the still hidden dialog.</returns>
    private AndroidX.AppCompat.App.AlertDialog BuildDoNotCloseAppDialog()
    {
        AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        builder.SetTitle("Refreshing...");

        builder.SetMessage("Your folders are being refreshed. This may take a moment. Please do NOT " +
            "exit the app or press the back button.");

        builder.SetCancelable(false);

        return builder.Create();
    }
}