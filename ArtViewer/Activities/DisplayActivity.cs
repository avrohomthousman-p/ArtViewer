using AndroidX.RecyclerView.Widget;
using ArtViewer.Network.Deviantart;

namespace ArtViewer.Activities;

[Activity(Label = "@string/app_name")]
public class DisplayActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(_Microsoft.Android.Resource.Designer.ResourceConstant.Layout.activity_display);
        Window.AddFlags(Android.Views.WindowManagerFlags.KeepScreenOn);


        SetupRecyclerView();
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
}
