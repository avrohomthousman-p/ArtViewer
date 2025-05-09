using ArtViewer.Database;
using ArtViewer.Network.Deviantart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArtViewer.Network.DeviantArt
{
    /// <summary>
    /// Queries the DeviantArt API to get folder/collection information
    /// </summary>
    internal class FolderQueryService
    {

        /// <summary>
        /// Fetches the desired folder from the DeviantArt API and saves it to the database for future use.
        /// </summary>
        /// <exception cref="FolderNotFoundException">If the folder could not be found</exception>
        public async Task SaveFolder(StorageLocation location, string username, string folderName, bool shouldRandomize, bool allPics=false)
        {
            string url = BuildUrl(location, username);
            using JsonDocument response = await NetworkUtils.RunGetRequest(url);
            JsonElement root = response.RootElement;


            CheckResponseForErrors(root);
            JsonElement foldersArray = root.GetProperty("results");


            Folder folder;
            if (allPics)
            {
                folder = CreateFolderForAllImages(foldersArray);
            }
            else
            {
                folder = FindFolderInResults(foldersArray, folderName);
            }


            folder.StoredIn = location;
            folder.Username = username;
            folder.ShouldRandomize = shouldRandomize;
            
            StandardDBQueries.CreateFolder(folder);
        }



        /// <summary>
        /// Builds the url to fetch all folders from a specific user
        /// </summary>
        /// <param name="location">the location of the desired folder (collections or gallery)</param>
        /// <param name="username">the username of the folder's owner</param>
        private string BuildUrl(StorageLocation location, string username)
        {
            string baseUrl = "https://www.deviantart.com/api/v1/oauth2/{0}/folders?access_token={1}&username={2}&" +
                "calculate_size=true&ext_preload=false&filter_empty_folder=true&limit=50";


            return string.Format(baseUrl, location.AsText(), NetworkUtils.GetAccessToken(), username);
        }



        private void CheckResponseForErrors(JsonElement root)
        {
            bool hasStatus = root.TryGetProperty("status", out JsonElement status);
            if (hasStatus && status.ToString() == "error")
            {
                var errorMsg = root.GetProperty("message");
                Console.WriteLine("Connection Failure: " + errorMsg.ToString());
                throw new HttpRequestException("Connection Failure: " + errorMsg.ToString());
            }
        }



        /// <summary>
        /// Finds the folder in the results array with the correct name
        /// </summary>
        private Folder FindFolderInResults(JsonElement foldersArray, string folderName)
        {
            JsonElement currentItem;
            int folderCount = foldersArray.GetArrayLength();
            

            for(int i = 0; i < folderCount; i++)
            {
                currentItem = foldersArray[i];
                if (FoldersMatch(currentItem, folderName))
                {
                    Folder output = new Folder();
                    output.FolderId = currentItem.GetProperty("folderid").ToString();
                    output.TotalImages = currentItem.GetProperty("size").GetInt32();
                    return output;
                }
            }


            throw new FolderNotFoundException($"Could not find folder named {folderName}");
        }



        /// <summary>
        /// Checks if the JSON folder data matches the folder name we are looking for.
        /// </summary>
        private static bool FoldersMatch(JsonElement folderFound, string desiredFolderName)
        {
            return folderFound.GetProperty("name").ToString().ToLower() == desiredFolderName.Trim().ToLower();
        }




        /// <summary>
        /// Creates a folder for all the images in this users collection/gallery
        /// </summary>
        private Folder CreateFolderForAllImages(JsonElement results)
        {
            //TODO: run through all results and add the total images
            return null;
        }
    }



    /// <summary>
    /// Thrown when the DeviantArt folder cannot be found
    /// </summary>
    internal class FolderNotFoundException : Exception
    {
        public FolderNotFoundException() { }

        public FolderNotFoundException(string message) : base(message) { }

        public FolderNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
