using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.Media3.Common;
using AndroidX.Media3.ExoPlayer;
using AndroidX.Media3.UI;
using AndroidX.RecyclerView.Widget;
using static Android.Icu.Text.Transliterator;

namespace ArtViewer.Activities
{
    public class VideoViewHolder : RecyclerView.ViewHolder
    {
        public PlayerView PlayerView { get; private set; }
        public TextView Title { get; private set; }
        public IExoPlayer Player { get; private set; }

        public VideoViewHolder(View itemView, Context context) : base(itemView)
        {
            Title = itemView.FindViewById<TextView>(Resource.Id.title);
            PlayerView = itemView.FindViewById<PlayerView>(Resource.Id.video_view);

            Player = new ExoPlayerBuilder(context).Build();
            PlayerView.Player = Player;
        }



        public void PrepareVideo(string url)
        {
            var mediaItem = AndroidX.Media3.Common.MediaItem.FromUri(Android.Net.Uri.Parse(url));
            Player.SetMediaItem(mediaItem);
            Player.Prepare();
            Player.PlayWhenReady = true;
        }
    }
}