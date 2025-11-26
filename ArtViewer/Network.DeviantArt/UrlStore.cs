using ArtViewer.Network.DeviantArt;
using System.Collections.Concurrent;

namespace ArtViewer.Network.DeviantArt
{
    /// <summary>
    /// Stores urls (and other data) and, if set to not randomize, also indexes needed for sorting.
    /// </summary>
    internal class UrlStore
    {
        public bool shouldRandomize;
        private readonly ConcurrentBag<MediaItem> gatheredArt = null;


        internal UrlStore(bool shouldRandomize)
        {
            this.shouldRandomize = shouldRandomize;

            this.gatheredArt = new ConcurrentBag<MediaItem>();
        }



        /// <summary>
        /// Add an image url to the storage.
        /// </summary>
        /// <param name="url">the image url to be stored</param>
        /// <param name="index">the image index in the gallery (for sorting). Ignored if randomize is on</param>
        /// <exception cref="ArgumentException">If null index is provided when randomizing is off</exception>
        public void Add(string url, string title, int? index = null, bool isImage = true)
        {
            if (!this.shouldRandomize && index == null)
            {
                throw new ArgumentException("Image index cannot be null when sorting is on.");
            }
            if (!this.shouldRandomize && (index < 0 || index >= MediaQueryService.MAX_MEDIA_ITEMS))
            {
                throw new ArgumentOutOfRangeException($"Image index {index} is out of bounds for range 0-{MediaQueryService.MAX_MEDIA_ITEMS}");
            }


            MediaItem item = new MediaItem(url, title);
            item.IsImage = isImage;
            if (index != null)
            {
                item.Index = (int)index;
            }


            this.gatheredArt.Add(item);
        }



        /// <summary>
        /// Compiles a list of all the images collected so far, sorts them or randomizes them as
        /// decided by class variable this.shouldRandomize, and returns the result.
        /// 
        /// Note: this function assumes all images have already been gathered and no threads are still
        /// getting more. If you want to wait for other threads you need to do so external to this
        /// function.
        /// </summary>
        /// <returns>A list of image data, sorted or randomized depending on your settings</returns>
        public List<MediaItem> GetUrls()
        {
            if (this.shouldRandomize)
            {
                List<MediaItem> results = new List<MediaItem>(this.gatheredArt);
                ShuffleList(results);
                return results;
            }
            else
            {
                return this.gatheredArt.OrderBy(image => image.Index).ToList();
            }
        }



        /// <summary>
        /// Randomize the order of the list so the images are displayed in a different order each time.
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            Random rng = new Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
