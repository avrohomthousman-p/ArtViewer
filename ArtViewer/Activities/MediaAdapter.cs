using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using ArtViewer.Network.DeviantArt;


namespace ArtViewer.Activities
{
    enum ItemType { IMAGE = 0, VIDEO = 1 };


    internal class MediaAdapter : RecyclerView.Adapter
    {
        private Context context;
        private List<MediaItem> artData;



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

                return new VideoViewHolder(view);
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

                viewHolder.Title.Text = artData[position].Title;


                var uri = Android.Net.Uri.Parse(artData[position].Url);

                viewHolder.Video.SetVideoURI(uri);
                viewHolder.Video.SetMediaController(new MediaController(context));
                viewHolder.Video.RequestFocus();
                //viewHolder.Video.Start();

                viewHolder.Video.Prepared += (s, e) =>
                {
                    viewHolder.Video.Start();
                };
            }
        }
    }
}