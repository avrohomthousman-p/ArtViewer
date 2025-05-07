using AndroidX.RecyclerView.Widget;
using ArtViewer.Network.Deviantart;
using AndroidX.AppCompat.App;
using Android.Views;
namespace ArtViewer.Activities;



[Activity(Label = "@string/app_name")]
/// <summary>
/// Activity for displaying the images in a user's folder.
/// </summary>
public class DisplayActivity : AppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_display);


        SetupRecyclerView();


        Window.AddFlags(Android.Views.WindowManagerFlags.KeepScreenOn);


        AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(toolbar);


        // Enable back button
        SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        SupportActionBar.SetDisplayShowHomeEnabled(true);
    }



    private async Task SetupRecyclerView()
    {
        //Use sample images until the API queries are set up
        QueryThreadManager threadManager = new QueryThreadManager();
        List<string> imageUrls = await threadManager.GetResults();


        //Recycler view creation
        RecyclerView recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
        recyclerView.SetLayoutManager(new LinearLayoutManager(this));
        recyclerView.SetAdapter(new ImageAdapter(this, imageUrls));


        //Make the images snap to the center of the screen on scroll
        var snapHelper = new PagerSnapHelper();
        snapHelper.AttachToRecyclerView(recyclerView);
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
