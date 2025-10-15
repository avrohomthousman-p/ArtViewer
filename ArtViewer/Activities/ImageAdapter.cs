using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using ArtViewer.Network.DeviantArt;
using Bumptech.Glide;


namespace ArtViewer.Activities
{
    internal class ImageAdapter : RecyclerView.Adapter

    {
        private Context context;
        private List<MediaItem> displayData;



        public ImageAdapter(Context context, List<MediaItem> displayData)
        {
            this.context = context;
            this.displayData = displayData;
        }



        public override int ItemCount => displayData.Count;



        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ImageViewHolder viewHolder = holder as ImageViewHolder;


            Glide.With(context)
             .Load(displayData[position].Url)
             .Placeholder(Resource.Drawable.ic_loading)
             .Into(viewHolder.ImageView);
        }



        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.image_item, parent, false);
            return new ImageViewHolder(itemView);
        }
    }
}
