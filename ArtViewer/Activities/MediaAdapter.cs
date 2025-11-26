using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using ArtViewer.Network.DeviantArt;
using Bumptech.Glide;


namespace ArtViewer.Activities
{
    enum ItemType { IMAGE = 0, VIDEO = 1 };


    internal class MediaAdapter : RecyclerView.Adapter
    {
        private Context context;
        private List<MediaItem> artData;
        private VideoViewHolder currentVideoHolder;



        public MediaAdapter(Context context, List<MediaItem> urls)
        {
            this.context = context;
            this.artData = urls;
        }



        public override int ItemCount => artData.Count;



        public override int GetItemViewType(int position)
        {
            var item = this.artData[position];
            return (int)(item.IsImage ? ItemType.IMAGE : ItemType.VIDEO);
        }



        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view;

            if (viewType == (int)ItemType.IMAGE)
            {
                view = LayoutInflater.From(parent.Context)
                    .Inflate(Resource.Layout.image_item, parent, false);

                return new ImageViewHolder(view);
            }
            else
            {
                view = LayoutInflater.From(parent.Context)
                    .Inflate(Resource.Layout.video_item, parent, false);

                return new VideoViewHolder(view, parent.Context);
            }
        }



        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            string title = artData[position].Title;

            if (artData[position].IsImage)
            {
                ImageViewHolder viewHolder = holder as ImageViewHolder;

                viewHolder.Title.Text = artData[position].Title;


                Glide.With(context)
                    .Load(artData[position].Url)
                    .Placeholder(Resource.Drawable.ic_loading)
                    .Into(viewHolder.ImageView);
            }
            else
            {
                VideoViewHolder viewHolder = holder as VideoViewHolder;
                var videoHolder = holder as VideoViewHolder;

                this.UpdateCurrentVideo(viewHolder);

                viewHolder.Title.Text = artData[position].Title;
                viewHolder.PrepareVideo(artData[position].Url);
            }
        }



        /// <summary>
        /// Updates the variable referencing the current video, and pauses the previous video.
        /// </summary>
        /// <param name="holder"></param>
        private void UpdateCurrentVideo(VideoViewHolder holder)
        {
            if (this.currentVideoHolder != null && this.currentVideoHolder != holder)
            {
                this.currentVideoHolder.Player.Pause();
            }
            this.currentVideoHolder = holder;
        }



        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            if (holder is VideoViewHolder videoHolder)
            {
                videoHolder.Player.Stop();
                videoHolder.Player.Release();
            }
            base.OnViewRecycled(holder);
        }



        public override void OnViewDetachedFromWindow(Java.Lang.Object holder)
        {
            if (holder is VideoViewHolder videoHolder)
            {
                videoHolder.Player.Pause();
            }
            base.OnViewDetachedFromWindow(holder);
        }



        public override void OnViewAttachedToWindow(Java.Lang.Object holder)
        {
            if (holder is VideoViewHolder videoHolder)
            {
                videoHolder.Player.Play();
            }
            base.OnViewAttachedToWindow(holder);
        }
    }
}