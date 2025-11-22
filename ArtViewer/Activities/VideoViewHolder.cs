using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace ArtViewer.Activities
{
    public class VideoViewHolder : RecyclerView.ViewHolder
    {
        public TextView Title { get; private set; }
        public VideoView Video { get; private set; }



        public VideoViewHolder(View itemView) : base(itemView)
        {
            this.Title = itemView.FindViewById<TextView>(Resource.Id.title);
            this.Video = itemView.FindViewById<VideoView>(Resource.Id.video_view);
        }
    }
}
