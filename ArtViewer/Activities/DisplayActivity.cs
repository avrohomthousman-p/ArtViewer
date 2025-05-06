using AndroidX.RecyclerView.Widget;

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
        List<string> imageUrls = new List<string>();
        imageUrls.Add("https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/cc64ef64-3d04-4b32-b650-ec8aac8c6d5b/dji89sc-c" +
            "b686366-ac3b-41cb-a6cb-7c1732510e14.png/v1/fill/w_1192,h_670,q_70,strp/when_angels_play_by_dawgit_dji89sc-pre.jpg?" +
            "token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXN" +
            "zIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9NzIwIiwicGF0aCI6IlwvZlwvY" +
            "2M2NGVmNjQtM2QwNC00YjMyLWI2NTAtZWM4YWFjOGM2ZDViXC9kamk4OXNjLWNiNjg2MzY2LWFjM2ItNDFjYi1hNmNiLTdjMTczMjUxMGUxNC5wbmc" +
            "iLCJ3aWR0aCI6Ijw9MTI4MCJ9XV0sImF1ZCI6WyJ1cm46c2VydmljZTppbWFnZS5vcGVyYXRpb25zIl19.4_ePocHkNMWbBdwlAWNcdH1uaB1ZfakE" +
            "SJy13EJ6EHY");


        imageUrls.Add("https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/cc64ef64-3d04-4b32-b650-ec8aac8c6d5b/dbur6i6-7" +
            "20155fc-14e9-4584-9ba1-90956760d323.png/v1/fit/w_828,h_1472,q_70,strp/apo7x_16550228_2ss_by_dawgit_dbur6i6-414w-2x" +
            ".jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIi" +
            "wiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9MTgyMSIsInBhdGgiOiJc" +
            "L2ZcL2NjNjRlZjY0LTNkMDQtNGIzMi1iNjUwLWVjOGFhYzhjNmQ1YlwvZGJ1cjZpNi03MjAxNTVmYy0xNGU5LTQ1ODQtOWJhMS05MDk1Njc2MGQzMj" +
            "MucG5nIiwid2lkdGgiOiI8PTEwMjQifV1dLCJhdWQiOlsidXJuOnNlcnZpY2U6aW1hZ2Uub3BlcmF0aW9ucyJdfQ.3KGefJaMs3oR6fP4-wrk2Purq" +
            "s4rhH4gmHnQfQ5DEBc");



        imageUrls.Add("https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/20566ef6-fc61-452b-a5f2-326f8442dbd3/djoukg5-0" +
            "634e660-05c9-45b1-ace0-e4aba82af9a1.jpg/v1/fill/w_975,h_819,q_70,strp/uf_chain_pong_2354___love_me_a_julia_by_west" +
            "oz64_djoukg5-pre.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZD" +
            "QxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9MTA3" +
            "NSIsInBhdGgiOiJcL2ZcLzIwNTY2ZWY2LWZjNjEtNDUyYi1hNWYyLTMyNmY4NDQyZGJkM1wvZGpvdWtnNS0wNjM0ZTY2MC0wNWM5LTQ1YjEtYWNlMC" +
            "1lNGFiYTgyYWY5YTEuanBnIiwid2lkdGgiOiI8PTEyODAifV1dLCJhdWQiOlsidXJuOnNlcnZpY2U6aW1hZ2Uub3BlcmF0aW9ucyJdfQ.Xoj3jhAAL" +
            "7t8PeoOakR0BddT_tAaS-LlJD_prrzAC4g");


        //Recycler view creation
        RecyclerView recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
        recyclerView.SetLayoutManager(new LinearLayoutManager(this));
        recyclerView.SetAdapter(new ImageAdapter(this, imageUrls));


        //Make the images snap to the center of the screen on scroll
        var snapHelper = new PagerSnapHelper();
        snapHelper.AttachToRecyclerView(recyclerView);
    }
}
