using Android.Views;
using AndroidX.AppCompat.App;
using ArtViewer.Database;
using Google.Android.Material.TextField;
using System.Text;
namespace ArtViewer.Activities;

[Activity(Label = "SaveNewFoldersActivity")]
public class SaveNewFoldersActivity : AppCompatActivity
{
    //Quick references to some of the views
    private CheckBox checkBox;
    private TextInputEditText folderNameInput;
    private Switch randomizationSwitch;


    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_save_new_folders);

        SetupToolbar();


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


        Button submitBtn = FindViewById<Button>(Resource.Id.submit_btn);
        submitBtn.Click += SaveFolder;
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



    private void SaveFolder(object? sender, EventArgs e)
    {
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
            return;
        }


        //Gather all the data
        bool isFolder = !this.checkBox.Checked;
        string folderName = (isFolder ? folderNameInput.Text : null);
        string username = usernameInput.Text;
        ImageSource storedIn = (galleryRadioBtn.Selected ? ImageSource.GALLERY : ImageSource.COLLECTIONS);



        //TODO: run API request
        //TODO: run DB request
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
}