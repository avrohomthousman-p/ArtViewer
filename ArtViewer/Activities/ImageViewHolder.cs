using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace ArtViewer.Activities
{
    public class ImageViewHolder : RecyclerView.ViewHolder
    {
        public ImageView ImageView { get; private set; }



        public ImageViewHolder(View itemView) : base(itemView)
        {
            ImageView = itemView.FindViewById<ImageView>(Resource.Id.image_view);
        }
    }

}
