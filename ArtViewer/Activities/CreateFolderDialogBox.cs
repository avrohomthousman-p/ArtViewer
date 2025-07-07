using Android.Views;
using ArtViewer.Database;
using Google.Android.Material.Snackbar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtViewer.Activities
{
    public class CreateFolderDialogBox : GenericDialogBox
    {
        public override string Title { get; set; } = "Create Folder";



        public CreateFolderDialogBox(Activity activity, Folder folder) : base(activity, folder) { }


        protected override void SetupEventHandlers(AndroidX.AppCompat.App.AlertDialog dialogBox)
        {
            base.SetupEventHandlers(dialogBox);

            this.saveBtn.Click += async (sender, e) => { await SaveFolder(dialogBox); };
        }



        protected async Task SaveFolder(AndroidX.AppCompat.App.AlertDialog dialogBox)
        {
            View rootView = this.activity.FindViewById(Android.Resource.Id.Content);
            string customLabel = this.folderLabelInputField.Text;


            if (customLabel == null || customLabel.Trim().Length <= 3)
            {
                Snackbar.Make(rootView, "Must provide a folder name at least 4 letters long", Snackbar.LengthShort).Show();
                return;
            }


            customLabel = customLabel.Trim();


            try
            {
                this.folder.ShouldRandomize = this.randomizationSwitch.Checked;
                this.folder.CustomName = customLabel;
                await StandardDBQueries.CreateOrUpdateFolder(this.folder);
                dialogBox.Dismiss();
                Toast.MakeText(this.activity, "Folder saved successfully", ToastLength.Short).Show();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.GetType() + " " + e.Message);
                Snackbar.Make(rootView, "Something went wrong. Unable to save folder.", Snackbar.LengthShort).Show();
                dialogBox.Dismiss();
            }
        }
    }
}
