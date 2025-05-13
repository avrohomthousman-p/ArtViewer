namespace ArtViewer.Activities;

using Android.Graphics;
using Android.Views;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using ArtViewer.Network.DeviantArt;


/// <summary>
/// Activity for picking the folders you actually want to save
/// </summary>
[Activity(Label = "SaveNewFoldersActivity")]
public class PickDesiredFoldersActivity : AppCompatActivity
{
    //Keys used to pass these data points to this activity
    public const string USERNAME_KEY = "username";
    public const string LOCATION_KEY = "location";



    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.display_item_in_scrollview);
        SetupToolbar();


        PopulateView();
    }



    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        if (item.ItemId == Android.Resource.Id.Home)
        {
            OnBackPressed();
            return true;
        }
        return base.OnOptionsItemSelected(item);
    }



    private void SetupToolbar()
    {
        AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(toolbar);


        // Enable back button
        SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        SupportActionBar.SetDisplayShowHomeEnabled(true);
    }



    /// <summary>
    /// Ensures that the ScrollView is properly populated with the correct data
    /// </summary>
    private async Task PopulateView()
    {
        try
        {
            Folder[] folders = await GetFoldersToDisplay();
            AddElementsToScrollView(folders);
        }
        catch(Exception e)
        {
            Console.WriteLine(e.GetType() + " " + e.Message);
            Toast.MakeText(this, "Something went wrong. Could not load the user's folders.", ToastLength.Long);
        }
    }



    /// <summary>
    /// Uses the FolderQueryService to fetch all the folders that need to be displayed.
    /// </summary>
    /// <returns></returns>
    private async Task<Folder[]> GetFoldersToDisplay()
    {
        string username = Intent.GetStringExtra(USERNAME_KEY);
        StorageLocation location = (StorageLocation)Intent.GetIntExtra(LOCATION_KEY, 0);
        FolderQueryService service = new FolderQueryService();
        return await service.GetAllFoldersOwnedByUser(location, username);
    }



    /// <summary>
    /// For each folder, adds a view to the activity displaying that folder.
    /// </summary>
    /// <param name="folders">All the folders that should be displayed</param>
    private void AddElementsToScrollView(Folder[] folders)
    {
        LinearLayout parentView = FindViewById<LinearLayout>(Resource.Id.folders_container);
        parentView.AddView(BuildIntroDisplay());


        foreach (Folder item in folders)
        {
            View newChild = BuildViewForSingleFolder(item);
            parentView.AddView(newChild);
        }
    }



    /// <summary>
    /// Builds a TextView to be displayed at the top of the activity to display an introduction
    /// to the activity.
    /// </summary>
    private TextView BuildIntroDisplay()
    {
        TextView view = new TextView(this);
        view.Text = "Tap on the folder you want to save";
        view.TextSize = Resources.GetDimension(Resource.Dimension.large_text);
        view.Typeface = Typeface.DefaultBold;
        view.Gravity = GravityFlags.Center;
        view.SetPadding(32, 24, 32, 24);
        return view;
    }



    /// <summary>
    /// Builds a view for to display the specified folder
    /// </summary>
    private View BuildViewForSingleFolder(Folder folder)
    {
        TextView view = new TextView(this);
        view.Text = folder.CustomName;
        view.Typeface = Typeface.DefaultBold;
        view.Gravity = GravityFlags.CenterVertical;
        view.SetPadding(32, 24, 32, 24);
        view.SetBackgroundColor(Color.ParseColor("#F8EDC4"));


        view.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent
        );
        ((LinearLayout.LayoutParams)view.LayoutParameters).Gravity = GravityFlags.CenterHorizontal;
        var marginParams = new ViewGroup.MarginLayoutParams(view.LayoutParameters);
        marginParams.SetMargins(20, 20, 20, 20);
        view.LayoutParameters = new LinearLayout.LayoutParams(marginParams);



        view.Click += (sender, e) => {
            CreateFolderDialogBox boxBuilder = new CreateFolderDialogBox(this, folder);
            boxBuilder.ShowDialogBox();
        };

        return view;
    }
}