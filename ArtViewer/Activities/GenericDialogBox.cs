using Android.Content;
using Android.Views;
using AndroidX.ConstraintLayout.Widget;
using ArtViewer.Database;

namespace ArtViewer.Activities
{
    /// <summary>
    /// Object that builds a AlertDialog for use in saving or editing folders.
    /// </summary>
    public class GenericDialogBox
    {
        protected readonly AndroidX.AppCompat.App.AlertDialog.Builder builder;
        protected readonly Activity activity;
        protected readonly Folder folder;
        protected EditText folderLabelInputField;
        protected Button cancelBtn;
        protected Button saveBtn;
        protected Switch randomizationSwitch;


        public virtual string Title { get; set; }


        public GenericDialogBox(Activity activity, Folder folder)
        {
            this.activity = activity;
            this.folder = folder;

            this.builder = new AndroidX.AppCompat.App.AlertDialog.Builder(activity);
            this.builder.SetView(BuildMainContents());
        }



        /// <summary>
        /// Builds the main view to be displayed in the dialog box.
        /// </summary>
        protected virtual View BuildMainContents()
        {
            LayoutInflater inflater = LayoutInflater.From(this.activity);
            ConstraintLayout parentView = (ConstraintLayout)inflater.Inflate(Resource.Layout.edit_folder_dialog_box, null);

            this.folderLabelInputField = parentView.FindViewById<EditText>(Resource.Id.folder_label_input);
            this.cancelBtn = parentView.FindViewById<Button>(Resource.Id.cancel_btn);
            this.saveBtn = parentView.FindViewById<Button>(Resource.Id.save_btn);
            this.randomizationSwitch = parentView.FindViewById<Switch>(Resource.Id.randomization_switch);

            return parentView;
        }




        /// <summary>
        /// Displays the dialog box
        /// </summary>
        /// <returns>A reference to the instance of the running dialog box</returns>
        public virtual AndroidX.AppCompat.App.AlertDialog ShowDialogBox()
        {
            this.builder.SetTitle(this.Title); //do this here son any changes made after the constructor are not ignored

            AndroidX.AppCompat.App.AlertDialog dialogBox = this.builder.Create();
            dialogBox.Show();

            SetupEventHandlers(dialogBox);

            return dialogBox;
        }



        /// <summary>
        /// Event handlers for AndroidX.AppCompat.App.AlertDialog need to be set after the dialog is showing.
        /// Override this method to set proper event handlers on any of your views.
        /// </summary>
        /// <param name="dialogBox">A reference to the dialog box being displayed</param>
        protected virtual void SetupEventHandlers(AndroidX.AppCompat.App.AlertDialog dialogBox)
        {
            this.randomizationSwitch.CheckedChange += (sender, e) =>
            {
                if (randomizationSwitch.Checked)
                    randomizationSwitch.Text = this.activity.GetString(Resource.String.switch_text_when_on);
                else
                    randomizationSwitch.Text = this.activity.GetString(Resource.String.switch_text_when_off);
            };



            this.cancelBtn.Click += (sender, e) => { dialogBox.Dismiss(); };
        }
    }
}
