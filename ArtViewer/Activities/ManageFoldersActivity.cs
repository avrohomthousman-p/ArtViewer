using Android.Content;
using Android.Views;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using Google.Android.Material.Snackbar;

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


        // Enable back button
        SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        SupportActionBar.SetDisplayShowHomeEnabled(true);
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
            View folderDisplayView = inflater.Inflate(Resource.Layout.single_folder_display, parentLayout, false);


            TextView folderName = folderDisplayView.FindViewById<TextView>(Resource.Id.folder_name);
            folderName.Text = folder.CustomName;


            //Set the event handler on click of the whole card
            folderDisplayView.Click += (sender, e) => { StartDisplayActivity(folder); };


            ImageButton deleteBtn = folderDisplayView.FindViewById<ImageButton>(Resource.Id.delete_btn);
            deleteBtn.Click += (sender, e) => ShowDeletionWarningDialog(folder, folderDisplayView, parentLayout);


            ImageButton editBtn = folderDisplayView.FindViewById<ImageButton>(Resource.Id.edit_btn);
            editBtn.Click += (sender, e) => ShowFolderRenameDialog(folder, folderName);


            parentLayout.AddView(folderDisplayView);
        }
    }




    private void StartDisplayActivity(Folder folder)
    {
        Intent intent = new Intent(this, typeof(DisplayActivity));
        intent.PutExtra(DisplayActivity.FOLDER_ID_KEY, folder.ID);
        StartActivity(intent);
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



    /// <summary>
    /// Displays a popup with an EditText allowing the user to enter a new name to use as the folder label.
    /// </summary>
    /// <param name="originalFolderNameDisplay">The TextView from the activity displaying the (old) folder name</param>
    private void ShowFolderRenameDialog(Folder folder, TextView originalFolderNameDisplay)
    {
        LinearLayout inputField = BuildTextEntryForEditPopup(folder);
        AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        builder.SetTitle("Edit Folder Label");
        builder.SetView(inputField);
        builder.SetNegativeButton("Cancel", (sender, e) => { });

        //Use null to avoid the default event handler (which closes on click)
        builder.SetPositiveButton("Save", (IDialogInterfaceOnClickListener)null);


        AndroidX.AppCompat.App.AlertDialog popup = builder.Create();
        popup.Show();


        //Provide a custom click handler that only closes if the data is valid
        var saveBtn = popup.GetButton((int)DialogButtonType.Positive);
        saveBtn.Click += (sender, e) => { ApplyNewFolderName(folder, originalFolderNameDisplay, popup, inputField); };
    }



    /// <summary>
    /// Builds the TextView for the popup that changes the folder name
    /// </summary>
    private LinearLayout BuildTextEntryForEditPopup(Folder folder)
    {
        EditText input = new EditText(this)
        {
            Text = folder.CustomName
        };

        input.SetBackgroundResource(Resource.Drawable.rounded_edit_text);
        input.SetPadding(15, 20, 0, 20);
        input.SetSingleLine(true);


        //Setup Margins
        LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent
        );

        layoutParams.SetMargins(20, 0, 20, 0);
        input.LayoutParameters = layoutParams;


        //For the margins to work, the input needs to be inside a layout
        LinearLayout container = new LinearLayout(this)
        {
            Orientation = Orientation.Horizontal
        };

        container.AddView(input);

        return container;
    }



    /// <summary>
    /// If the user provided name is valid, updates the folder in the database and closes the popup.
    /// </summary>
    /// <param name="folder">The folder whose label is being edited</param>
    /// <param name="originalFolderNameDisplay">The TextView from the activity displaying the (old) folder name</param>
    /// <param name="popup">A reference to the popup itself</param>
    /// <param name="inputContainer">A reference to the layout that the popup's TextView input field is in</param>
    private void ApplyNewFolderName(Folder folder, TextView originalFolderNameDisplay, AndroidX.AppCompat.App.AlertDialog popup, LinearLayout inputContainer)
    {
        View rootView = FindViewById(Android.Resource.Id.Content);
        TextView inputFeild = (TextView)inputContainer.GetChildAt(0);
        string newName = inputFeild.Text;

        if (newName == null || newName.Length <= 3)
        {
            Snackbar.Make(rootView, "Must provide a folder name at least 4 letters long", Snackbar.LengthShort).Show();
            return;
        }


        try
        {
            folder.CustomName = newName;
            StandardDBQueries.UpdateFolder(folder);
            originalFolderNameDisplay.Text = newName;
        }
        catch(Exception e)
        {
            Console.WriteLine(e.GetType() + " " + e.Message);
            Snackbar.Make(rootView, "Something went wrong. Could not save your change", Snackbar.LengthShort).Show();
            popup.Dismiss();
        }


        popup.Dismiss();
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
}