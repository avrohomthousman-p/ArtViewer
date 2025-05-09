using Android.Views;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using Java.Util.Zip;

namespace ArtViewer.Activities;



/// <summary>
/// Displays all your saved folders and lets you edit/delete/view any of them.
/// </summary>
[Activity(Label = "FoldersActivity")]
public class ManageFoldersActivity : AppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_manage_folders);


        AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(toolbar);

        PopulateScrollView();
    }



    /// <summary>
    /// Inserts all the folders into the scroll view
    /// </summary>
    private void PopulateScrollView()
    {
        LayoutInflater inflater = LayoutInflater.From(this);
        LinearLayout parentLayout = FindViewById<LinearLayout>(Resource.Id.folders_container);
        var folders = StandardDBQueries.GetAllFolders();


        foreach (Folder folder in folders)
        {
            View itemView = inflater.Inflate(Resource.Layout.single_folder_display, parentLayout, false);

            //TODO: Set the view data and event handlers before adding it to the parent

            parentLayout.AddView(itemView);
        }
    }
}