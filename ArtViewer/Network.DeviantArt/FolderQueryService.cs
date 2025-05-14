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
        /// Fetches the desired gallery/collection from the DeviantArt API and saves it to the database for future use.
        /// </summary>
        public async Task SaveFullGalleryOrCollection(StorageLocation location, string username, bool shouldRandomize)
        {
            JsonElement foldersArray = await FetchAllUsersFolders(username, location);


            Folder folder = CreateFolderForAllImages(foldersArray);
            folder.StoredIn = location;
            folder.Username = username;
            folder.ShouldRandomize = shouldRandomize;
            folder.CustomName = username + "'s " + location.AsText();


            StandardDBQueries.CreateFolder(folder);
        }



        /// <summary>
        /// Queries the API for all folders owned by the specified user and returns them as an array
        /// of UNSAVED folder instances. 
        /// 
        /// Along with every folder, this function returns a url to the thumbnail for that folder, or null
        /// if no thumbnail exists.
        /// 
        /// Note: the actual name of the folder is put inside the CustomName field.
        /// </summary>
        /// <param name="location">The location of the folders, gallery or collection</param>
        /// <param name="username">The username of the DeviantArt user who owns the folder</param>
        /// <returns>An array of unsaved folder instances containing each folder the user has, and its thumbnail image</returns>
        public async Task<Tuple<Folder, string?>[]> GetAllFoldersOwnedByUser(StorageLocation location, string username)
        {
            JsonElement foldersArray = await FetchAllUsersFolders(username, location);
            int folderCount = foldersArray.GetArrayLength();


            List<Tuple<Folder, string?>> folders = new List<Tuple<Folder, string?>>();
            Folder currentFolder;
            JsonElement currentJsonItem;
            for(int i = 0; i < folderCount; i++)
            {
                currentJsonItem = foldersArray[i];

                currentFolder = new Folder();
                currentFolder.FolderId = currentJsonItem.GetProperty("folderid").GetString();
                currentFolder.TotalImages = currentJsonItem.GetProperty("size").GetInt32();
                currentFolder.Username = username;
                currentFolder.StoredIn = location;
                currentFolder.CustomName = currentJsonItem.GetProperty("name").GetString();

                string? thumbnail = ExtractFolderThumbnail(currentJsonItem);

                folders.Add(Tuple.Create(currentFolder, thumbnail));
            }


            return folders.ToArray();
        }



        /// <summary>
        /// Queries the API and gets all the folders owned by the specified DeviantArt user.
        /// </summary>
        /// <param name="username">The name of the DeviantArt user whose folders you want to see</param>
        /// <param name="location">The location those folders can be found in, Gallery or Collection</param>
        private async Task<JsonElement> FetchAllUsersFolders(string username, StorageLocation location)
        {
            string url = BuildUrl(location, username);
            JsonDocument response = await NetworkUtils.RunGetRequest(url);
            JsonElement root = response.RootElement;


            CheckResponseForErrors(root);
            return root.GetProperty("results");
        }



        /// <summary>
        /// Builds the url to fetch all folders from a specific user
        /// </summary>
        /// <param name="location">the location of the desired folder (collections or gallery)</param>
        /// <param name="username">the username of the folder's owner</param>
        private string BuildUrl(StorageLocation location, string username)
        {
            string baseUrl = "https://www.deviantart.com/api/v1/oauth2/{0}/folders?access_token={1}&username={2}&" +
                "calculate_size=true&ext_preload=false&filter_empty_folder=true&limit=50&mature_content=true";


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
        /// Creates a folder for all the images in this users collection/gallery
        /// </summary>
        private Folder CreateFolderForAllImages(JsonElement foldersArray)
        {
            int imageCount = 0;
            for (int i = 0; i < foldersArray.GetArrayLength(); i++)
            {
                imageCount += foldersArray[i].GetProperty("size").GetInt32();
            }

            Folder folder = new Folder();
            folder.FolderId = "all";
            folder.TotalImages = imageCount;
            return folder;
        }



        /// <summary>
        /// Extracts the thumbnail of the DeviantArt folder provided
        /// </summary>
        /// <param name="json">The JSON data for the folder</param>
        /// <returns>An image url for the DeviantArt folder thumbnail</returns>
        private string? ExtractFolderThumbnail(JsonElement json)
        {
            JsonElement thumbnailObj = json.GetProperty("thumb");
            if(thumbnailObj.ValueKind != JsonValueKind.Null)
            {
                if (thumbnailObj.TryGetProperty("thumbs", out JsonElement imageList))
                {
                    return imageList[0].GetProperty("src").GetString();
                }
            }


            return null;
        }
    }
}
