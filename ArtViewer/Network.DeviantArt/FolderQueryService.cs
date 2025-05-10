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
        public async Task SaveFolder(StorageLocation location, string username, string actualFolderName, string customFolderLabel, bool shouldRandomize, bool allPics=false)
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
                folder = FindFolderInResults(foldersArray, actualFolderName, username);
            }


            folder.StoredIn = location;
            folder.Username = username;
            folder.ShouldRandomize = shouldRandomize;
            folder.CustomName = GetCustomLabel(location, username, actualFolderName, customFolderLabel, allPics);


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
            if (hasStatus && status.GetString() == "error")
            {
                string errorMsg = root.GetProperty("error_description").GetString();
                Console.WriteLine("Connection Failure: " + errorMsg);
                throw new HttpRequestException("Connection Failure: " + errorMsg);
            }
        }



        /// <summary>
        /// Finds the folder in the results array with the correct name
        /// </summary>
        private Folder FindFolderInResults(JsonElement foldersArray, string folderName, string username)
        {
            JsonElement currentItem;
            int folderCount = foldersArray.GetArrayLength();


            CheckIfUserExists(foldersArray, username);


            for (int i = 0; i < folderCount; i++)
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
        /// If you query the API for a username that does not exist, it returns a single default
        /// folder that is empty and named featured. So if that is the response we got, we need
        /// to throw a UserNotFoundException.
        /// </summary>
        /// <exception cref="UserNotFoundException"></exception>
        private void CheckIfUserExists(JsonElement foldersArray, string username)
        {
            if(foldersArray.GetArrayLength() != 1)
            {
                return;
            }

            if (foldersArray[0].GetProperty("name").GetString() != "Featured")
            {
                return;
            }

            if(foldersArray[0].GetProperty("size").GetInt32() != 0)
            {
                return;
            }


            throw new UserNotFoundException($"Could not find any user named {username}");
        }



        /// <summary>
        /// Checks if the JSON folder data matches the folder name we are looking for.
        /// </summary>
        private static bool FoldersMatch(JsonElement folderFound, string desiredFolderName)
        {
            return folderFound.GetProperty("name").ToString().ToLower() == desiredFolderName.ToLower();
        }




        /// <summary>
        /// Creates a folder for all the images in this users collection/gallery
        /// </summary>
        private Folder CreateFolderForAllImages(JsonElement results)
        {
            int imageCount = 0;
            for(int i = 0; i < results.GetArrayLength(); i++)
            {
                imageCount += results.GetProperty("size").GetInt32();
            }

            Folder folder = new Folder();
            //TODO: save custom name
            folder.FolderId = "All";
            folder.TotalImages = imageCount;
            return folder;
        }



        /// <summary>
        /// Gets the appropriate label to use for the saved folder. If the user entered one, use that
        /// one. Otherwise use the actual folder name for folders, and the username for full gallery
        /// or collection.
        /// </summary>
        private string GetCustomLabel(StorageLocation location, string username, string actualFolderName, string customLabel, bool allPics)
        {
            if(customLabel != null && customLabel != "")
            {
                return customLabel;
            }


            if (allPics)
            {
                return username + "'s " + location.AsText();
            }
            else
            {
                return actualFolderName;
            }
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



    /// <summary>
    /// Thrown when the DeviantArt user does not exist
    /// </summary>
    internal class UserNotFoundException : Exception
    {
        public UserNotFoundException() { }

        public UserNotFoundException(string message) : base(message) { }

        public UserNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
