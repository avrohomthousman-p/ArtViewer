using Android.Drm;
using ArtViewer.Activities;
using ArtViewer.Database;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ArtViewer.Network.Deviantart
{

    /// <summary>
    /// Class that starts threads to connect to the DeviantArt API and retrieve the image URLs.
    /// </summary>
    internal class QueryThreadManager
    {
        //fetch only 250 imagaes, but it not necessarily the first 250 in the gallery
        public const int MAX_IMAGES = 250;
        public const int MAX_QUERY_LIMIT = 24;


        private readonly ConcurrentBag<string> urls = new ConcurrentBag<string>();
        private readonly TaskCompletionSource queriesCompleted = new TaskCompletionSource();



        
        public QueryThreadManager()
        {
            InitAsync();
        }



        /// <summary>
        /// Runs all the asyncronous initialization needed for this class
        /// </summary>
        private async void InitAsync()
        {
            Folder folder = await StandardDBQueries.GetFolder();
            List<string> queries = PlanQueries(folder);
            await StartAllQueries(queries);
        }



        /// <summary>
        /// Generates a list of queries that will fetch the all images from the API, from beginning
        /// to end (or the MAX_IMAGES limit is reached).
        /// </summary>
        private List<string> PlanQueries(Folder folder)
        {
            List<string> queries = new List<string>();
            int offset = 0;
            while (offset < MAX_IMAGES)
            {
                int queryLimit = MAX_QUERY_LIMIT;
                string url = NetworkUtils.BuildGenericFolderUrl(folder.CollectionType, folder.FolderId, folder.Username, MAX_QUERY_LIMIT, offset);

                queries.Add(url);
                offset += MAX_QUERY_LIMIT;
            }

            return queries;
        }



        private async Task StartAllQueries(List<string> urls)
        {

            await Parallel.ForEachAsync(urls, async (url, ct) =>
            {
                try
                {
                    await GetImageUrls(url);
                }
                catch (Exception e)
                {
                    //We don't want the whole app to crash if some of the queries fail
                    Console.WriteLine("Failed to retrieve images: " + e.Message);
                }
            });


            this.queriesCompleted.SetResult();
        }



        /// <summary>
        /// Runs a query to get images from the DeviantArt API. Since the API limits the amount of data
        /// that can be retrieved in a single call, we have to make multiple calls to this function, each
        /// with an offset determaning which images this call should retrieve.
        /// </summary>
        private async Task GetImageUrls(string url)
        {
            using JsonDocument folder = await NetworkUtils.RunGetRequest(url);
            JsonElement root = folder.RootElement;


            //This will throw an exception if something went wrong with the API call
            CheckQueryResponseForErrors(root);


            SaveQueryResults(root);
        }



        /// <summary>
        /// Checks if something went wrong with the API call. If so, throw an exception.
        /// </summary>
        /// <param name="root">Root element of the JSON response</param>
        private void CheckQueryResponseForErrors(JsonElement root)
        {
            bool hasError = root.TryGetProperty("error", out JsonElement error);
            if (hasError)
            {
                Console.WriteLine("Connection Failure: " + error.ToString());
                throw new HttpRequestException("Connection Failure: " + error.ToString());
            }

            //Sometimes the API will use the key status instead of error
            bool hasStatus = root.TryGetProperty("status", out JsonElement status);
            if (hasStatus && status.ToString() == "error")
            {
                var errorMsg = root.GetProperty("message");
                Console.WriteLine("Connection Failure: " + errorMsg.ToString());
                throw new HttpRequestException("Connection Failure: " + errorMsg.ToString());
            }
        }




        /// <summary>
        /// Extracts the image urls from the returned data and saves them to the urls list.
        /// </summary>

        /// <param name="root">the root element of the API response</param>
        private void SaveQueryResults(JsonElement root)
        {
            var pics = root.GetProperty("results");
            int imagesRecieved = pics.GetArrayLength();
            int imageCount = pics.GetArrayLength();


            for (int i = 0; i < imageCount; i++)
            {
                ExtractImage(i);
            }



            void ExtractImage(int imageIndex)
            {
                var imageData = pics[imageIndex];
                if (imageData.TryGetProperty("content", out JsonElement content))
                {
                    if (content.TryGetProperty("src", out JsonElement src))
                    {
                        urls.Add(src.ToString());
                    }
                }
            }
        }



        /// <summary>
        /// Compiles the results from the threads into a single list of random order.
        /// 
        /// Note: this function will block until all the data is loaded.
        /// </summary>
        public async Task<List<string>> GetResults()
        {
            await queriesCompleted.Task;

            List<string> results = new List<string>(this.urls);
            ShuffleList<string>(results);
            return results;
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