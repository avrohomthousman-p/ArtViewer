using Android.Content;
using Android.Views;
using ArtViewer.Database;
using Google.Android.Material.Snackbar;


namespace ArtViewer.Activities
{
    /// <summary>
    /// Custom AlertDialog popup designed to edit user folders
    /// </summary>
    public class EditFolderDialogBox : GenericDialogBox
    {
        protected TextView originalFolderNameTextView;


        public override string Title { get; set; } = "Edit Folder Label";



        public EditFolderDialogBox(Activity activity, Folder folder, TextView originalFolderNameTextView) : base(activity, folder)
        {
            this.originalFolderNameTextView = originalFolderNameTextView;
        }



        protected override View BuildMainContents()
        {
            View view = base.BuildMainContents();



            this.randomizationSwitch.Checked = this.folder.ShouldRandomize;
            if (randomizationSwitch.Checked)
                randomizationSwitch.Text = this.activity.GetString(Resource.String.switch_text_when_on);
            else
                randomizationSwitch.Text = this.activity.GetString(Resource.String.switch_text_when_off);



            return view;
        }



        protected override void SetupEventHandlers(AndroidX.AppCompat.App.AlertDialog dialogBox)
        {
            base.SetupEventHandlers(dialogBox);

            this.saveBtn.Click += (sender, e) => { SaveButton_HandleClick(dialogBox); };
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
            string newName = this.folderLabelInputField.Text;



            if (!this.ChangesMade())
            {
                dialogBox.Dismiss();
                return;
            }
            else if (newName == null || newName.Length <= 3)
            {
                Snackbar.Make(rootView, "Must provide a folder name at least 4 letters long", Snackbar.LengthShort).Show();
                return;
            }



            bool success = SaveChangesToFolder(newName);
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



        public override bool ChangesMade()
        {
            string originalName = this.originalFolderNameTextView.Text;
            string newName = this.folderLabelInputField.Text;

            return base.ChangesMade() || originalName != newName;
        }



        /// <summary>
        /// Updates the folder label and randomization setting in the database.
        /// </summary>
        /// <returns>True if the folder was updated successfully and false otherwise</returns>
        protected bool SaveChangesToFolder(string newName)
        {
            try
            {
                this.folder.CustomName = newName;
                this.folder.ShouldRandomize = this.randomizationSwitch.Checked;
                StandardDBQueries.UpdateFolder(this.folder);
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
