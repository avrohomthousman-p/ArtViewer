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
        public const int PAGE_SIZE = 50;



        /// <summary>
        /// Fetches the desired gallery/collection from the DeviantArt API and saves it to the database for future use.
        /// </summary>
        public async Task SaveFullGalleryOrCollection(StorageLocation location, string username, bool shouldRandomize)
        {
            Folder[] folders = await GetAllUserFolders(location, username);


            Folder folder = CreateFolderForAllImages(folders);
            folder.StoredIn = location;
            folder.Username = username;
            folder.ShouldRandomize = shouldRandomize;
            folder.CustomName = username + "'s " + location.AsText();


            await StandardDBQueries.CreateFolder(folder);
        }



        /// <summary>
        /// Queries the API for all folders owned by the specified user and returns them as an array
        /// of UNSAVED folder instances.
        /// 
        /// Note: the actual name of the folder is put inside the CustomName field.
        /// </summary>
        /// <param name="location">The location of the folders, gallery or collection</param>
        /// <param name="username">The username of the DeviantArt user who owns the folder</param>
        /// <returns>An array of unsaved folder instances containing each folder the user has</returns>
        public async Task<Folder[]> GetAllUserFolders(StorageLocation location, string username)
        {
            List<Folder> folders = new List<Folder>();

            string url;
            JsonElement foldersArray;
            int page = 0;
            bool hasMore = true;
            while (hasMore)
            {
                url = BuildUrl(location, username, page * PAGE_SIZE);
                JsonDocument response = await NetworkUtils.RunGetRequest(url);
                JsonElement root = response.RootElement;

                CheckResponseForErrors(root);
                foldersArray = root.GetProperty("results");

                folders.AddRange(ConvertJsonArrayToFolders(foldersArray, location, username));

                hasMore = root.GetProperty("has_more").GetBoolean();
                page++;
            }


            return folders.ToArray();
        }



        /// <summary>
        /// Queries the API for a specific "page" of 50 folders owned by the specified DeviantArt user.
        /// 
        /// Tha page value is used as an offset in the API query to skip n folder folders where n = PAGE_SIZE * page.
        /// So, with PAGE_SIZE == 50, page 2 means skip the first 100 folders, and get the next 50.
        /// </summary>
        /// <param name="location">The location of the folders, gallery or collection</param>
        /// <param name="username">The username of the DeviantArt user who owns the folder</param>
        /// <param name="page">Which PAGE_SIZE folders should be fetched.</param>
        /// <returns>An array of unsaved folder instances containing each folder the user has in this page</returns>
        public async Task<Folder[]> GetPageOfUserFolders(StorageLocation location, string username, int page)
        {
            JsonElement foldersArray = await FetchPageOfUserFolders(location, username, page);

            List<Folder> folders = ConvertJsonArrayToFolders(foldersArray, location, username);

            return folders.ToArray();
        }



        /// <summary>
        /// Converts a JSON array of data for each folder, into a list of folder objects.
        /// </summary>
        /// <param name="foldersArray">The json data from the API response</param>
        /// <param name="location">The location of the folders, gallery or collection</param>
        /// <param name="username">The username of the DeviantArt user who owns the folder</param>
        /// <returns>A list of unsaved folder objects with the API response data.</returns>
        private List<Folder> ConvertJsonArrayToFolders(JsonElement foldersArray, StorageLocation location, string username)
        {
            int folderCount = foldersArray.GetArrayLength();
            List<Folder> folders = new List<Folder>();


            Folder currentFolder;
            JsonElement currentJsonItem;
            int folderSize;
            for (int i = 0; i < folderCount; i++)
            {
                currentJsonItem = foldersArray[i];
                folderSize = currentJsonItem.GetProperty("size").GetInt32();
                if (folderSize == 0)
                {
                    continue;
                }

                currentFolder = new Folder();
                currentFolder.FolderId = currentJsonItem.GetProperty("folderid").GetString();
                currentFolder.TotalImages = folderSize;
                currentFolder.Username = username;
                currentFolder.StoredIn = location;
                currentFolder.CustomName = currentJsonItem.GetProperty("name").GetString();
                currentFolder.ThumbnailUrl = ExtractFolderThumbnail(currentJsonItem);

                folders.Add(currentFolder);
            }


            return folders;
        }



        /// <summary>
        /// Queries the API and gets all the folders owned by the specified DeviantArt user within the specified page.
        /// </summary>
        /// <param name="location">The location those folders can be found in, Gallery or Collection</param>
        /// <param name="username">The name of the DeviantArt user whose folders you want to see</param>
        /// <param name="page"></param>
        private async Task<JsonElement> FetchPageOfUserFolders(StorageLocation location, string username, int page)
        {
            string url = BuildUrl(location, username, page * PAGE_SIZE);
            JsonDocument response = await NetworkUtils.RunGetRequest(url);
            JsonElement root = response.RootElement;


            CheckResponseForErrors(root);
            return root.GetProperty("results");
        }



        /// <summary>
        /// Builds the url to fetch folders from a specific user.
        /// </summary>
        /// <param name="location">The location of the desired folder (collections or gallery)</param>
        /// <param name="username">The username of the folder's owner</param>
        /// <param name="offset">The number of folders to skip</param>
        private string BuildUrl(StorageLocation location, string username, int offset=0)
        {
            string baseUrl = "https://www.deviantart.com/api/v1/oauth2/{0}/folders?access_token={1}&username={2}&" +
                "offset={3}&calculate_size=true&ext_preload=false&filter_empty_folder=true&limit=50&mature_content=true";


            return string.Format(baseUrl, location.AsText(), NetworkUtils.GetAccessToken(), username, offset);
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
        private Folder CreateFolderForAllImages(Folder[] folders)
        {
            int imageCount = 0;
            for (int i = 0; i < folders.Length; i++)
            {
                imageCount += folders[i].TotalImages;
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
