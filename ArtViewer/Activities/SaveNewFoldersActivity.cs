using Android.Views;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using ArtViewer.Network.DeviantArt;
using Google.Android.Material.TextField;
using System.Text;
namespace ArtViewer.Activities;



/// <summary>
/// Activity for saving new folders that you can then view in the app.
/// </summary>
[Activity(Label = "SaveNewFoldersActivity")]
public class SaveNewFoldersActivity : AppCompatActivity
{
    //Quick references to some of the views
    private CheckBox checkBox;
    private TextInputEditText folderNameInput;
    private Switch randomizationSwitch;
    private Button submitBtn;


    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_save_new_folders);

        SetupToolbar();
        SetupAllEventHandlers();
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




    private void SetupAllEventHandlers()
    {
        folderNameInput = FindViewById<TextInputEditText>(Resource.Id.folder_name_input);


        this.checkBox = FindViewById<CheckBox>(Resource.Id.should_use_full_gallery);
        this.checkBox.CheckedChange += OnCheckboxToggled;


        randomizationSwitch = FindViewById<Switch>(Resource.Id.randomization_switch);
        randomizationSwitch.CheckedChange += (sender, e) =>
        {
            if (randomizationSwitch.Checked)
                randomizationSwitch.Text = GetString(Resource.String.switch_text_when_on);
            else
                randomizationSwitch.Text = GetString(Resource.String.switch_text_when_off);
        };


        submitBtn = FindViewById<Button>(Resource.Id.submit_btn);
        submitBtn.Click += HandleSubmit;
    }



    /// <summary>
    /// Event Handler for when user toggles the checkbox for "use all images in gallery/collection"
    /// </summary>
    private void OnCheckboxToggled(Object? sender, EventArgs e)
    {
        TextInputLayout inputContainer = FindViewById<TextInputLayout>(Resource.Id.folder_name_input_container);

        if (this.checkBox.Checked)
        {
            folderNameInput.Enabled = false;
            folderNameInput.Text = "";
            inputContainer.Hint = GetString(Resource.String.disabled_folder_input_label);
        }
        else
        {
            folderNameInput.Enabled = true;
            inputContainer.Hint = GetString(Resource.String.folder_input_label);
        }
    }



    private async void HandleSubmit(object? sender, EventArgs e)
    {
        this.DeactivateSubmitBtn();

        TextInputEditText usernameInput = FindViewById<TextInputEditText>(Resource.Id.username_input);
        TextInputEditText customFolderLabelInput = FindViewById<TextInputEditText>(Resource.Id.custom_folder_name_input);
        RadioButton galleryRadioBtn = FindViewById<RadioButton>(Resource.Id.gallery_radio_btn);
        RadioButton collectionRadioBtn = FindViewById<RadioButton>(Resource.Id.collection_radio_btn);


        if (DataIsMissing(usernameInput, galleryRadioBtn, collectionRadioBtn, out string errorMsg))
        {
            AndroidX.AppCompat.App.AlertDialog.Builder dialog = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            dialog.SetTitle("Missing Data");
            dialog.SetMessage(errorMsg);
            dialog.SetPositiveButton("Ok", (sender, e) => { });
            dialog.Show();
            ActivateSubmitBtn();
            return;
        }


        //Gather all the data
        bool isFolder = !this.checkBox.Checked;
        string actualFolderName = (isFolder ? folderNameInput.Text.Trim() : null);
        string username = usernameInput.Text.Trim();
        string customFolderLabel = customFolderLabelInput.Text.Trim();
        StorageLocation location = (galleryRadioBtn.Selected ? StorageLocation.GALLERY : StorageLocation.COLLECTIONS);
        bool shouldRandomize = randomizationSwitch.Checked;


        await SaveFolder(location, username, actualFolderName, customFolderLabel, shouldRandomize, isFolder);
        ActivateSubmitBtn();
    }



    private bool DataIsMissing(TextInputEditText usernameInput, RadioButton galleryRadioBtn, RadioButton collectionRadioBtn, out string errorMsg)
    {
        StringBuilder builder = new StringBuilder("Please make sure you filled everything out.\n");
        bool thereIsAnError = false;

        if((folderNameInput.Text == null || folderNameInput.Text == "") && !this.checkBox.Checked)
        {
            builder.Append("-Provide a folder name or check the box for \"use full gallery/collection\"\n");
            thereIsAnError = true;
        }


        if(usernameInput.Text == null || usernameInput.Text == "")
        {
            builder.Append("-Provide the username of the gallery/collection owner\n");
            thereIsAnError = true;
        }


        if(!galleryRadioBtn.Checked && !collectionRadioBtn.Checked)
        {
            builder.Append("-You must select either Gallery or Collection\n");
            thereIsAnError = true;
        }


        errorMsg = (thereIsAnError ? builder.ToString() : "");
        return thereIsAnError;
    }



    private void DeactivateSubmitBtn()
    {
        submitBtn.Enabled = false;
        submitBtn.Text = GetString(Resource.String.submit_btn_text_inactive);
    }



    private void ActivateSubmitBtn()
    {
        submitBtn.Enabled = true;
        submitBtn.Text = GetString(Resource.String.submit_btn_text_active);
    }



    private async Task SaveFolder(StorageLocation location, string username, string actualFolderName, string customFolderLabel, bool shouldRandomize, bool isFolder)
    {
        string message;
        try
        {
            FolderQueryService service = new FolderQueryService();
            await service.SaveFolder(location, username, actualFolderName, customFolderLabel, shouldRandomize, !isFolder);
            message = "Folder saved successfully";
        }
        catch(SQLite.SQLiteException e)
        {
            Console.WriteLine(e.GetType() + " " + e.Message);
            message = "Something went wrong. Unable to save folder";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetType() + " " + e.Message);
            message = "Something went wrong. Unable to save folder";
        }


        DisplayToastOnUiThread(message, ToastLength.Long);
    }




    /// <summary>
    /// Convienent way to make a Toast popup excecute on the UI thread.
    /// </summary>
    private void DisplayToastOnUiThread(string message, ToastLength duration = ToastLength.Long)
    {
        RunOnUiThread(() =>
        {
            Toast.MakeText(this, message, duration).Show();
        });
    }
}