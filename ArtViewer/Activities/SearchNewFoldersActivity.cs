using Android.Content;
using Android.Views;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using ArtViewer.Network.DeviantArt;
using Google.Android.Material.TextField;
using System.Text;
namespace ArtViewer.Activities;



/// <summary>
/// Activity for searching for new folders that you can then view in the app.
/// </summary>
[Activity]
public class SearchNewFoldersActivity : AppCompatActivity
{
    //Quick references to some of the views
    private CheckBox checkBox;
    private Switch randomizationSwitch;
    private Button submitBtn;


    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_search_new_folders);

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
        this.checkBox = FindViewById<CheckBox>(Resource.Id.should_use_full_gallery);
        this.checkBox.CheckedChange += OnCheckboxToggled;


        this.randomizationSwitch = FindViewById<Switch>(Resource.Id.randomization_switch);
        this.randomizationSwitch.CheckedChange += (sender, e) =>
        {
            if (randomizationSwitch.Checked)
                randomizationSwitch.Text = GetString(Resource.String.switch_text_when_on);
            else
                randomizationSwitch.Text = GetString(Resource.String.switch_text_when_off);
        };


        this.submitBtn = FindViewById<Button>(Resource.Id.submit_btn);
        this.submitBtn.Click += HandleSubmit;
    }



    private void SetSubmitButtonText()
    {
        if (this.checkBox.Checked)
        {
            this.submitBtn.Text = GetString(Resource.String.submit_btn_text_full_gallery);
        }
        else
        {
            this.submitBtn.Text = GetString(Resource.String.submit_btn_text_individual_folders);
        }
    }



    private void SetRandomizationSwitchVisibility()
    {
        if (this.checkBox.Checked)
        {
            this.randomizationSwitch.Visibility = ViewStates.Visible;
        }
        else
        {
            this.randomizationSwitch.Visibility = ViewStates.Invisible;
        }
    }



    /// <summary>
    /// Event Handler for when user toggles the checkbox for "use all images in gallery/collection"
    /// </summary>
    private void OnCheckboxToggled(Object? sender, EventArgs e)
    {
        SetRandomizationSwitchVisibility();
        SetSubmitButtonText();
    }



    private async void HandleSubmit(object? sender, EventArgs e)
    {
        this.DeactivateSubmitBtn();

        TextInputEditText usernameInput = FindViewById<TextInputEditText>(Resource.Id.username_input);
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
        string username = usernameInput.Text.Trim();
        StorageLocation location = (galleryRadioBtn.Checked ? StorageLocation.GALLERY : StorageLocation.COLLECTIONS);
        bool shouldRandomize = randomizationSwitch.Checked;


        if (isFolder)
        {
            //Start an activity where the user can pick the folder(s) they want
            Intent intent = new Intent(this, typeof(PickDesiredFoldersActivity));
            intent.PutExtra(PickDesiredFoldersActivity.USERNAME_KEY, username);
            intent.PutExtra(PickDesiredFoldersActivity.LOCATION_KEY, (int)location);
            StartActivity(intent);
        }
        else
        {
            await SaveFullGalleryOrCollection(location, username, shouldRandomize);
        }

        ActivateSubmitBtn();
    }



    private bool DataIsMissing(TextInputEditText usernameInput, RadioButton galleryRadioBtn, RadioButton collectionRadioBtn, out string errorMsg)
    {
        StringBuilder builder = new StringBuilder("Please make sure you filled everything out.\n");
        bool thereIsAnError = false;


        if (usernameInput.Text == null || usernameInput.Text == "")
        {
            builder.Append("-Provide the username of the gallery/collection owner\n");
            thereIsAnError = true;
        }


        if (!galleryRadioBtn.Checked && !collectionRadioBtn.Checked)
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
        this.submitBtn.Enabled = true;
        SetSubmitButtonText();
    }



    /// <summary>
    /// Saves a full gallery or collection to the database
    /// </summary>
    private async Task SaveFullGalleryOrCollection(StorageLocation location, string username, bool shouldRandomize)
    {
        string message;
        try
        {
            FolderQueryService service = new FolderQueryService();
            await service.SaveFullGalleryOrCollection(location, username, shouldRandomize);
            message = "Folder saved successfully";
        }
        catch (SQLite.SQLiteException e)
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