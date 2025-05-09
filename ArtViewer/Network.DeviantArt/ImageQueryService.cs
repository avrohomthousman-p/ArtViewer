﻿using Android.Drm;
using ArtViewer.Activities;
using ArtViewer.Database;
using ArtViewer.Network.DeviantArt;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ArtViewer.Network.Deviantart
{

    /// <summary>
    /// Class that starts threads to connect to the DeviantArt API and retrieve the image URLs.
    /// </summary>
    internal class ImageQueryService
    {
        //fetch only 250 imagaes, but it not necessarily the first 250 in the gallery
        public const int MAX_IMAGES = 250;
        public const int MAX_QUERY_LIMIT = 24;


        private UrlStore urls;
        private readonly TaskCompletionSource queriesCompleted = new TaskCompletionSource();



        
        public ImageQueryService()
        {
            Folder folder = StandardDBQueries.GetFolder();
            this.urls = new UrlStore(folder.ShouldRandomize);

            List<QueryTarget> queries = PlanQueries(folder);
            StartAllQueries(folder, queries);
        }



        /// <summary>
        /// Determans what queries need to be made inorder to fetch all the images in the input array.
        /// </summary>
        /// <returns>A list with a query data object for each query that needs to be done</returns>
        private List<QueryTarget> PlanQueries(Folder folder)
        {
            //We only need to download random images if there are more than MAX_IMAGES. Otherwise, just take all
            if (folder.ShouldRandomize && folder.TotalImages > MAX_IMAGES)
            {
                int[] galleryIndexes = PickImagesToLoad(folder);
                return PlanNonConsecutiveQueries(galleryIndexes);
            }
            else
            {
                return PlanConsecutiveQueries(folder);
            }
        }



        /// <summary>
        /// Picks the images from the gallery that will be loaded. They must be randomly selected
        /// so that all parts of the gallery are equally represented.
        /// </summary>
        private int[] PickImagesToLoad(Folder folder)
        {
            //If we have less images than the max, just load all of them
            if (folder.TotalImages <= MAX_IMAGES)
            {
                return Enumerable.Range(0, folder.TotalImages).ToArray();
            }


            Random random = new Random();
            HashSet<int> selectedImages = new HashSet<int>();
            while (selectedImages.Count < MAX_IMAGES)
            {
                int randomNumber = random.Next(0, folder.TotalImages);
                selectedImages.Add(randomNumber);
            }


            return selectedImages.Order().ToArray();
        }



        /// <summary>
        /// Generates the queries needed to fetch specific images from the API. Use this if you 
        /// don't want all images, you just want to pick specific ones and skip the rest.
        /// </summary>
        /// <param name="galleryIndexes">The indexes of the images needed from the API</param>
        private List<QueryTarget> PlanNonConsecutiveQueries(int[] galleryIndexes)
        {
            List<QueryTarget> queries = new List<QueryTarget>();

            int offset;
            int queryLimit;
            List<int> imagesToKeep;


            //For each image we want to load, ensure it is included in a query
            for (int i = 0; i < galleryIndexes.Length; i++)
            {
                imagesToKeep = new List<int>();


                //start the next query from this image
                offset = galleryIndexes[i];
                imagesToKeep.Add(0); //keep the first image in the query


                //check if there are any more images that are close by enough that we can include them in the same query
                bool HasNextImage() => i + 1 < galleryIndexes.Length;
                bool NextImageFitsInQuery() => HasNextImage() && galleryIndexes[i + 1] < offset + MAX_QUERY_LIMIT;

                while (NextImageFitsInQuery())
                {
                    i++;
                    imagesToKeep.Add(galleryIndexes[i] - offset);
                }


                queryLimit = galleryIndexes[i] - offset + 1;

                queries.Add(new QueryTarget(offset, queryLimit, imagesToKeep));
            }


            return queries;
        }



        /// <summary>
        /// Generates a list of queries that will fetch the all images from the API, from beginning
        /// to end (or the MAX_IMAGES limit is reached). Use this if you don't want to skip any 
        /// images.
        /// </summary>
        private List<QueryTarget> PlanConsecutiveQueries(Folder folder)
        {
            List<QueryTarget> queries = new List<QueryTarget>();

            int numImagesToFetch = Math.Min(MAX_IMAGES, folder.TotalImages);
            int offset = 0;
            while (offset < numImagesToFetch)
            {
                int queryLimit = (offset + MAX_QUERY_LIMIT > numImagesToFetch ? numImagesToFetch - offset : MAX_QUERY_LIMIT);
                queries.Add(new QueryTarget(offset, queryLimit, null));
                offset += MAX_QUERY_LIMIT;
            }

            return queries;
        }



        private async Task StartAllQueries(Folder folderModel, List<QueryTarget> queries)
        {

            await Parallel.ForEachAsync(queries, async (queryData, ct) =>
            {
                try
                {
                    await GetImageUrls(folderModel, queryData);
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
        private async Task GetImageUrls(Folder folderModel, QueryTarget queryData)
        {
            string url = folderModel.BuildUrl(queryData.queryLimit, queryData.offset);
            using JsonDocument jsonFolder = await NetworkUtils.RunGetRequest(url);
            JsonElement root = jsonFolder.RootElement;


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

            return urls.GetUrls();
        }
    }
}