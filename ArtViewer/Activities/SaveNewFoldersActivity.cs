namespace ArtViewer.Activities;

[Activity(Label = "SaveNewFoldersActivity")]
public class SaveNewFoldersActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_save_new_folders);
    }
}