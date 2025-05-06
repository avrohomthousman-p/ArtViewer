using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;


namespace ArtViewer.Activities
{
    internal class ImageAdapter : RecyclerView.Adapter

    {
        private Context context;
        private List<string> imageUrls;



        public ImageAdapter(Context context, List<string> imageUrls)
        {
            this.context = context;
            this.imageUrls = imageUrls;
        }



        public override int ItemCount => imageUrls.Count;



        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ImageViewHolder viewHolder = holder as ImageViewHolder;


            Glide.With(context)
             .Load(imageUrls[position])
             .Into(viewHolder.ImageView);
        }



        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.image_item, parent, false);
            return new ImageViewHolder(itemView);
        }
    }
}
