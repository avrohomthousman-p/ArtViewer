using ArtViewer.Network.Deviantart;
using System.Collections.Concurrent;

namespace ArtViewer.Network.DeviantArt
{
    /// <summary>
    /// Stores urls and, if set to not randomize, also indexes needed for sorting.
    /// </summary>
    internal class UrlStore
    {
        public bool shouldRandomize;
        private readonly ConcurrentBag<string> nonSortable = null;
        private readonly ConcurrentBag<Tuple<int, string>> sortable = null;


        internal UrlStore(bool shouldRandomize)
        {
            this.shouldRandomize = shouldRandomize;

            if (shouldRandomize)
            {
                nonSortable = new ConcurrentBag<string>();
            }
            else
            {
                sortable = new ConcurrentBag<Tuple<int, string>>();
            }
        }



        /// <summary>
        /// Add an image url to the storage.
        /// </summary>
        /// <param name="url">the image url to be stored</param>
        /// <param name="index">the image index in the gallery (for sorting). Ignored if randomize is on</param>
        /// <exception cref="ArgumentException">If null index is provided when randomizing is off</exception>
        public void Add(string url, int? index = null)
        {
            if (this.shouldRandomize)
            {
                this.nonSortable.Add(url);
            }
            else
            {
                if (index == null)
                {
                    throw new ArgumentException("Image index cannot be null when sorting is on.");
                }
                if (index < 0 || index >= QueryThreadManager.MAX_IMAGES)
                {
                    throw new ArgumentOutOfRangeException($"Image index {index} is out of bounds for range 0-{QueryThreadManager.MAX_IMAGES}");
                }

                sortable.Add(Tuple.Create((int)index, url));
            }
        }



        /// <summary>
        /// Compiles a list of all the images collected so far, sorts them or randomizes them as
        /// decided by class variable this.shouldRandomize, and returns the result.
        /// 
        /// Note: this function assumes all images have already been gathered and no threads are still
        /// getting more. If you want to wait for other threads you need to do so external to this
        /// function.
        /// </summary>
        /// <returns>A list of image urls, sorted or randomized depending on your settings</returns>
        public List<string> GetUrls()
        {
            if (this.shouldRandomize)
            {
                List<string> results = new List<string>(this.nonSortable);
                ShuffleList(results);
                return results;
            }
            else
            {
                return this.sortable.OrderBy(data => data.Item1).Select(data => data.Item2).ToList();
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
