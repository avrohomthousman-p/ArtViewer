namespace ArtViewer.Activities;

using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using ArtViewer.Network.DeviantArt;
using Bumptech.Glide;
using static Android.Icu.Text.Transliterator;
using static AndroidX.RecyclerView.Widget.RecyclerView;


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
            Tuple<Folder, string?>[] folders = await GetFoldersToDisplay();
            AddElementsToScrollView(folders);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetType() + " " + e.Message);
            Toast.MakeText(this, "Something went wrong. Could not load the user's folders.", ToastLength.Long);
        }
    }



    /// <summary>
    /// Uses the FolderQueryService to fetch all the folders that need to be displayed.
    /// </summary>
    private async Task<Tuple<Folder, string?>[]> GetFoldersToDisplay()
    {
        string username = Intent.GetStringExtra(USERNAME_KEY);
        StorageLocation location = (StorageLocation)Intent.GetIntExtra(LOCATION_KEY, 0);
        FolderQueryService service = new FolderQueryService();
        return await service.GetAllFoldersOwnedByUser(location, username);
    }



    /// <summary>
    /// For each folder, adds a view to the activity displaying that folder.
    /// </summary>
    /// <param name="folders">All the folders that should be displayed, along with optional thumbnail images</param>
    private void AddElementsToScrollView(Tuple<Folder, string?>[] folders)
    {
        LayoutInflater inflater = LayoutInflater.From(this);
        LinearLayout parentView = FindViewById<LinearLayout>(Resource.Id.folders_container);
        parentView.AddView(BuildIntroDisplay());


        foreach (Tuple<Folder, string?> item in folders)
        {
            View newChild = BuildViewForSingleFolder(item, parentView, inflater);
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
        view.TextSize = Resources.GetDimension(Resource.Dimension.medium_text);
        view.Typeface = Typeface.DefaultBold;
        view.Gravity = GravityFlags.Center;
        view.SetPadding(32, 24, 32, 24);
        return view;
    }



    /// <summary>
    /// Builds a view for to display the specified folder
    /// </summary>
    private View BuildViewForSingleFolder(Tuple<Folder, string?> folder, LinearLayout parentView, LayoutInflater inflater)
    {
        View view = inflater.Inflate(Resource.Layout.display_deviantart_folder, parentView, false);

        TextView folderNameDisplay = view.FindViewById<TextView>(Resource.Id.folder_name);
        folderNameDisplay.Text = folder.Item1.CustomName;



        if (folder.Item2 != null)
        {
            ImageView thumbnail = view.FindViewById<ImageView>(Resource.Id.folder_icon);
            Glide.With(this)
                 .Load(folder.Item2)
                 .Into(thumbnail);
        }




        view.Click += (sender, e) =>
        {
            CreateFolderDialogBox boxBuilder = new CreateFolderDialogBox(this, folder.Item1);
            boxBuilder.ShowDialogBox();
        };

        return view;
    }
}