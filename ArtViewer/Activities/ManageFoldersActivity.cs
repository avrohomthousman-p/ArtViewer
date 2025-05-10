using Android.Views;
using AndroidX.AppCompat.App;
using ArtViewer.Database;

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
            View folderDisplay = inflater.Inflate(Resource.Layout.single_folder_display, parentLayout, false);


            TextView folderName = folderDisplay.FindViewById<TextView>(Resource.Id.folder_name);
            folderName.Text = folder.CustomName;


            //TODO: set the event handler on click of the whole card


            ImageButton deleteBtn = folderDisplay.FindViewById<ImageButton>(Resource.Id.delete_btn);
            deleteBtn.Click += (sender, e) => ShowDeletionWarningDialog(folder, folderDisplay, parentLayout);


            ImageButton editBtn = folderDisplay.FindViewById<ImageButton>(Resource.Id.edit_btn);
            deleteBtn.Click += EditFolder;


            parentLayout.AddView(folderDisplay);
        }
    }



    /// <summary>
    /// Show popup asking user to confirm that they want to delete the folder.
    /// </summary>
    /// <param name="folder">The folder to be deleted</param>
    /// <param name="targetView">The view that represents that folder and must be removed</param>
    /// <param name="parentLayout">The layout containing all the folder views</param>
    private void ShowDeletionWarningDialog(Folder folder, View targetView, LinearLayout parentLayout)
    {
        AndroidX.AppCompat.App.AlertDialog.Builder dialog = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        dialog.SetTitle("Delete " + folder.CustomName);
        dialog.SetMessage("Are you sure you want to delete this folder?");
        dialog.SetNegativeButton("Cancel", (sender, e) => { });
        dialog.SetPositiveButton("Delete", (sender, e) => DeleteFolder(folder, targetView, parentLayout));
        dialog.Show();
        
        
    }




    private void DeleteFolder(Folder folder, View targetView, LinearLayout parentLayout)
    {
        //If this function slows things down, move it to a background thread

        string toastText;
        try
        {
            StandardDBQueries.DeleteFolder(folder);
            parentLayout.RemoveView(targetView);
            toastText = "Folder deleted successfully";
        }
        catch (SQLite.SQLiteException e)
        {
            Console.WriteLine(e.GetType() + " " + e.Message);
            toastText = "Something went wrong while attempting to delete the folder.";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetType() + " " + e.Message);
            toastText = "Something went wrong";
        }


        Toast.MakeText(this, toastText, ToastLength.Short).Show();
    }




    private void EditFolder(Object sender, EventArgs e)
    {
        //TODO: start activity where you can edit the folder
    }
}