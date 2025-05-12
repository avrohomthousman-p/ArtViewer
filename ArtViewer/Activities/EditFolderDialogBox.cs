using Android.Content;
using Android.Views;
using ArtViewer.Database;
using Google.Android.Material.Snackbar;


namespace ArtViewer.Activities
{
    /// <summary>
    /// Custom AlertDialog popup designed to edit user folders
    /// </summary>
    public class EditFolderDialogBox
    {
        protected readonly AndroidX.AppCompat.App.AlertDialog.Builder builder;
        protected readonly Activity activity;
        protected readonly Folder folder;

        protected EditText popupInputField;
        protected TextView originalFolderNameTextView;


        public string Title { get; set; } = "Edit Folder Label";



        public EditFolderDialogBox(Activity activity, TextView originalFolderNameTextView, Folder folder)
        {
            this.activity = activity;
            this.originalFolderNameTextView = originalFolderNameTextView;
            this.folder = folder;

            this.builder = new AndroidX.AppCompat.App.AlertDialog.Builder(activity);
            this.builder.SetTitle(this.Title);
            this.builder.SetView(BuildMainContents());
            this.builder.SetNegativeButton("Cancel", (sender, e) => { });

            //Use null to avoid the default event handler (which closes on click)
            builder.SetPositiveButton("Save", (IDialogInterfaceOnClickListener)null);
        }



        /// <summary>
        /// Builds the main view to be displayed in the dialog.
        /// </summary>
        protected View BuildMainContents()
        {
            this.popupInputField = new EditText(this.activity)
            {
                Text = this.folder.CustomName
            };

            this.popupInputField.SetBackgroundResource(Resource.Drawable.rounded_edit_text);
            this.popupInputField.SetPadding(15, 20, 0, 20);
            this.popupInputField.SetSingleLine(true);


            //Setup Margins
            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent
            );

            layoutParams.SetMargins(20, 0, 20, 0);
            this.popupInputField.LayoutParameters = layoutParams;


            //For the margins to work, the input needs to be inside a layout
            LinearLayout container = new LinearLayout(this.activity)
            {
                Orientation = Orientation.Horizontal
            };

            container.AddView(this.popupInputField);

            return container;
        }



        /// <summary>
        /// Displays the dialog box
        /// </summary>
        /// <returns>A reference to the instance of the running dialog box</returns>
        public AndroidX.AppCompat.App.AlertDialog ShowDialogBox()
        {
            AndroidX.AppCompat.App.AlertDialog dialogBox = this.builder.Create();
            dialogBox.Show();


            //Provide a custom click handler that only closes if the data is valid (can only be done while the dialog is visible)
            Button saveBtn = dialogBox.GetButton((int)DialogButtonType.Positive);
            saveBtn.Click += (sender, e) => { SaveButton_HandleClick(dialogBox); };


            return dialogBox;
        }



        /// <summary>
        /// Handles the click event of the save button by updating the name of the folder (and
        /// the name displayed on the activity) or displaying an error, whichever is appropriate.
        /// </summary>
        /// <param name="dialogBox">A reference to the dialog box being displayed</param>
        protected void SaveButton_HandleClick(AndroidX.AppCompat.App.AlertDialog dialogBox)
        {
            View rootView = this.activity.FindViewById(Android.Resource.Id.Content);
            string originalName = this.originalFolderNameTextView.Text;
            string newName = this.popupInputField.Text;



            if (originalName == newName)
            {
                dialogBox.Dismiss();
                return;
            }
            else if (newName == null || newName.Length <= 3)
            {
                Snackbar.Make(rootView, "Must provide a folder name at least 4 letters long", Snackbar.LengthShort).Show();
                return;
            }



            bool success = ApplyNewFolderName(newName);
            if (success)
            {
                this.originalFolderNameTextView.Text = newName;
            }
            else
            {
                Snackbar.Make(rootView, "Something went wrong. Could not save your change", Snackbar.LengthShort).Show();
            }

            dialogBox.Dismiss();
        }



        /// <summary>
        /// Updates the folder label in the database.
        /// </summary>
        /// <returns>True if the folder was updated successfully and false otherwise</returns>
        protected bool ApplyNewFolderName(string newName)
        {
            try
            {
                this.folder.CustomName = newName;
                StandardDBQueries.UpdateFolder(folder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType() + " " + e.Message);
                return false;
            }


            return true;
        }
    }
}
