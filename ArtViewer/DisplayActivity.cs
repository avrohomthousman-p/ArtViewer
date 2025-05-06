namespace ArtViewer;

[Activity(Label = "@string/app_name")]
public class DisplayActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_display);
        this.Window.AddFlags(Android.Views.WindowManagerFlags.KeepScreenOn);

        //TODO: setup a recycler view to display pics
    }
}
