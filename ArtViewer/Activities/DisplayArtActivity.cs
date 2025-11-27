using AndroidX.RecyclerView.Widget;
using ArtViewer.Network.DeviantArt;
using AndroidX.AppCompat.App;
using Android.Views;
using ArtViewer.Database;
using Android.Content;
using AndroidX.Lifecycle;
namespace ArtViewer.Activities;



/// <summary>
/// Activity for displaying the images in a user's folder.
/// </summary>
[Activity]
public class DisplayArtActivity : AppCompatActivity
{
    public const string FOLDER_ID_KEY = "folderId";

    private AndroidX.AppCompat.Widget.Toolbar toolbar;


    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_art_display);


        this.toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(this.toolbar);


        SetupRecyclerView();


        Window.AddFlags(Android.Views.WindowManagerFlags.KeepScreenOn);



        // Enable back button
        SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        SupportActionBar.SetDisplayShowHomeEnabled(true);
    }



    private async Task SetupRecyclerView()
    {
        List<MediaItem> imageUrls;


        try
        {
            int folderId = Intent.GetIntExtra(FOLDER_ID_KEY, -1);
            if(folderId == -1)
            {
                throw new KeyNotFoundException("Could not find folder");
            }
            Folder folder = await StandardDBQueries.GetFolderByID(folderId);

            SetActivityTitle(folder.CustomName);


            //Urls are stored in the ImageUrlsViewModel to ensure they dont get reset on device rotation
            MediaUrlsViewModel viewModel = new ViewModelProvider(this).Get(Java.Lang.Class.FromType(typeof(MediaUrlsViewModel))) as MediaUrlsViewModel;
            imageUrls = await viewModel.GetMediaUrlsAsync(folder);
        }
        catch(Exception e)
        {
            Console.WriteLine(e.GetType() + " " + e.Message);
            Toast.MakeText(this, "Unable to load images", ToastLength.Short).Show();
            return;
        }



        //Recycler view creation
        RecyclerView recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
        recyclerView.SetLayoutManager(new LinearLayoutManager(this));
        recyclerView.SetAdapter(new MediaAdapter(this, imageUrls));


        //Make the images snap to the center of the screen on scroll
        var snapHelper = new PagerSnapHelper();
        snapHelper.AttachToRecyclerView(recyclerView);
    }



    /// <summary>
    /// Sets the activity title on the UI thread.
    /// </summary>
    /// <param name="title">The new title you want for the activity</param>
    private void SetActivityTitle(string title)
    {
        RunOnUiThread(() => { this.toolbar.Title = title; });
    }



    public override bool OnCreateOptionsMenu(IMenu? menu)
    {
        MenuInflater.Inflate(Resource.Menu.art_display_activity_menu, menu);
        return base.OnCreateOptionsMenu(menu);
    }



    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        if (item.ItemId == Android.Resource.Id.Home)
        {
            OnBackPressed();
            return true;
        }
        else if (item.ItemId == Resource.Id.action_home)
        {
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
            return true;
        }
        else if (item.ItemId == Resource.Id.action_search_for_folders)
        {
            Intent intent = new Intent(this, typeof(SearchNewFoldersActivity));
            StartActivity(intent);
            return true;
        }

        return base.OnOptionsItemSelected(item);
    }
}
