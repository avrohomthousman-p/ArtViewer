namespace ArtViewer.Activities;

using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using ArtViewer.Database;
using ArtViewer.Network.DeviantArt;
using Bumptech.Glide;
using static Android.Icu.Text.Transliterator;
using static AndroidX.RecyclerView.Widget.RecyclerView;


/// <summary>
/// Activity for picking the folders you actually want to save
/// </summary>
[Activity(Label = "@string/pick_folders_activity_name")]
public class PickDesiredFoldersActivity : AppCompatActivity
{
    //Keys used to pass these data points to this activity
    public const string USERNAME_KEY = "username";
    public const string LOCATION_KEY = "location";

    private int nextPage = 0;
    private bool hasNextPage = true;
    private TextView tempView; //class reference to temp view that says "loading..." for easy removal when data is loaded



    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.display_item_in_scrollview);
        SetupToolbar();


        PopulateView();
    }



    public override bool OnCreateOptionsMenu(IMenu? menu)
    {
        MenuInflater.Inflate(Resource.Menu.pick_folders_activity_menu, menu);
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
        else if (item.ItemId == Resource.Id.action_see_my_folders)
        {
            Intent intent = new Intent(this, typeof(ManageFoldersActivity));
            StartActivity(intent);
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
    /// Runs an API query and displayes the folders in the scroll view.
    /// </summary>
    private async Task PopulateView()
    {
        InsertTemporaryView();
        string username = Intent.GetStringExtra(USERNAME_KEY);
        StorageLocation location = (StorageLocation)Intent.GetIntExtra(LOCATION_KEY, 0);


        try
        {
            Folder[] folders = await GetFoldersToDisplay(location, username);
            AddElementsToScrollView(folders, location, username);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetType() + " " + e.Message);


            LinearLayout parentView = FindViewById<LinearLayout>(Resource.Id.folders_container);
            bool thereAreNoFoldersOnDisplay = parentView.ChildCount <= 1; //Allow the "loading" TextView


            if (thereAreNoFoldersOnDisplay)
            {
                InsertNoFoldersFoundDisplay(location, username);
            }
            else
            {
                Toast.MakeText(this, "Error: unable to load more folders", ToastLength.Short).Show();
            }
        }
    }




    /// <summary>
    /// Puts a temporary view on the screen that says "Loading..." for while the API queries load.
    /// </summary>
    private void InsertTemporaryView()
    {
        LinearLayout parentView = FindViewById<LinearLayout>(Resource.Id.folders_container);

        TextView tempView = new TextView(this)
        {
            Text = "Loading...",
            TextSize = Resources.GetDimension(Resource.Dimension.large_text),
            Typeface = Typeface.DefaultBold,
            Gravity = GravityFlags.Center,
        };
        tempView.SetPadding(32, 24, 32, 24);

        this.tempView = tempView;
        parentView.AddView(tempView);
    }



    /// <summary>
    /// Uses the FolderQueryService to fetch all the folders that need to be displayed.
    /// <param name="location">The storage location of the desired folders (gallery or collection)</param>
    /// <param name="username">The DeviantArt username of the owner of the folder(s)</param>
    /// </summary>
    private async Task<Folder[]> GetFoldersToDisplay(StorageLocation location, string username)
    {
        FolderQueryService service = new FolderQueryService();
        Tuple<Folder[], bool> results = await service.GetPageOfUserFolders(location, username, this.nextPage);

        this.nextPage++;
        this.hasNextPage = results.Item2;

        return results.Item1;
    }



    /// <summary>
    /// For each folder, adds a view to the activity displaying that folder.
    /// </summary>
    /// <param name="folders">All the folders that should be displayed</param>
    /// <param name="location">The storage location of the folders provided (gallery or collection)</param>
    /// <param name="username">The DeviantArt username of the owner of the provided folders</param>
    /// 
    private void AddElementsToScrollView(Folder[] folders, StorageLocation location, string username)
    {
        LinearLayout parentView = FindViewById<LinearLayout>(Resource.Id.folders_container);
        bool thereAreNoFoldersOnDisplay = parentView.ChildCount <= 1; //Allow the "loading" TextView


        if (folders.Length == 0 && thereAreNoFoldersOnDisplay)
        {
            InsertNoFoldersFoundDisplay(location, username);
            return;
        }


        LayoutInflater inflater = LayoutInflater.From(this);

        parentView.RemoveView(this.tempView);


        if (thereAreNoFoldersOnDisplay)
        {
            parentView.AddView(BuildIntroDisplay());
        }
        

        foreach (Folder item in folders)
        {
            View newChild = BuildViewForSingleFolder(item, parentView, inflater);
            parentView.AddView(newChild);
        }


        if (this.hasNextPage)
        {
            parentView.AddView(BuildButtonToLoadMoreFolders());
        }
    }




    /// <summary>
    /// Populates the ScrollView with a message saying that no folders were found, and suggestions
    /// for correcting the user input.
    /// </summary>
    /// <param name="location">The storage location the user entered for the search (gallery or collection)</param>
    /// <param name="username">The username the user entered for the folder search</param>
    private void InsertNoFoldersFoundDisplay(StorageLocation location, string username)
    {
        LinearLayout parentView = FindViewById<LinearLayout>(Resource.Id.folders_container);
        parentView.RemoveAllViews();

        TextView intro = new TextView(this)
        {
            Text = "No Folders Found",
            TextSize = Resources.GetDimension(Resource.Dimension.medium_text),
            Typeface = Typeface.DefaultBold,
            Gravity = GravityFlags.Center,
        };
        intro.SetPadding(32, 24, 32, 24);
        parentView.AddView(intro);



        TextView suggestion = new TextView(this)
        {
            Text = $"Double check that you are connected to the internet and that you entered the correct information:" +
                            $"\n\tusername: {username}\n\tstorage location: {location.AsText()}",
            Gravity = GravityFlags.Center,
        };
        suggestion.SetPadding(32, 24, 32, 24);
        parentView.AddView(suggestion);
    }




    /// <summary>
    /// Builds a TextView to be displayed at the top of the activity to display an introduction
    /// to the activity.
    /// </summary>
    private TextView BuildIntroDisplay()
    {
        TextView view = new TextView(this);
        view.Text = "Save any folders you like. They will appear on the \"Browse My Folders\" screen.";
        view.TextSize = Resources.GetDimension(Resource.Dimension.medium_text);
        view.Typeface = Typeface.DefaultBold;
        view.Gravity = GravityFlags.Center;
        view.SetPadding(32, 24, 32, 24);
        return view;
    }



    /// <summary>
    /// Builds a view for to display the specified folder
    /// </summary>
    private View BuildViewForSingleFolder(Folder folder, LinearLayout parentView, LayoutInflater inflater)
    {
        View view = inflater.Inflate(Resource.Layout.display_deviantart_folder, parentView, false);

        TextView folderNameDisplay = view.FindViewById<TextView>(Resource.Id.folder_name);
        folderNameDisplay.Text = folder.CustomName + $" ({folder.TotalImages} images)";



        if (folder.ThumbnailUrl != null)
        {
            ImageView thumbnail = view.FindViewById<ImageView>(Resource.Id.folder_icon);
            Glide.With(this)
                 .Load(folder.ThumbnailUrl)
                 .Into(thumbnail);
        }




        ImageButton saveBtn = view.FindViewById<ImageButton>(Resource.Id.save_btn);
        saveBtn.Click += (sender, e) =>
        {
            CreateFolderDialogBox boxBuilder = new CreateFolderDialogBox(this, folder);
            boxBuilder.ShowDialogBox();
        };

        return view;
    }



    private Button BuildButtonToLoadMoreFolders()
    {
        Button button = new Button(this);
        button.Text = "Load More...";


        //Styling
        var typedValue = new TypedValue();
        Theme.ResolveAttribute(Android.Resource.Attribute.ColorPrimary, typedValue, true);
        var primaryColor = new Color(typedValue.Data);
        button.SetBackgroundColor(primaryColor);
        button.SetTextColor(Android.Graphics.Color.White);


        //Layout setup
        var layoutParams = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.WrapContent,
            ViewGroup.LayoutParams.WrapContent
        );

        layoutParams.SetMargins(20, 10, 20, 20);
        button.LayoutParameters = layoutParams;

        button.SetPadding(24, 16, 24, 16);



        //Event handling
        button.Click += async (sender, e) => {
            if (button.Parent is ViewGroup parentLayout)
            {
                parentLayout.RemoveView(button);
            }

            await PopulateView();
        };



        return button;
    }
}