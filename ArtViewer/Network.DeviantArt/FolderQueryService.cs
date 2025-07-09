using ArtViewer.Database;
using ArtViewer.Network.Deviantart;
using System;
using System.Collections.Concurrent;
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
        /// Updates the specified folders with the most recent data from DeviantArt.
        /// </summary>
        /// <param name="localFolders">The folders from the local database that should be updated</param>
        /// <returns>An array of updated folders</returns>
        public async Task<Tuple<Folder, ChangeType>[]> RefreshFolders(Folder[] localFolders)
        {
            ConcurrentBag<Tuple<Folder, ChangeType>> refreshed = new ConcurrentBag<Tuple<Folder, ChangeType>>();
            var queries = PlanQueries(localFolders);

            await Parallel.ForEachAsync(queries, async (kvPair, ct) =>
            {
                Tuple<string, StorageLocation> queryData = kvPair.Key;
                List<Folder> localFoldersForThisQuery = kvPair.Value;

                Folder[] incomingFolders = await GetAllUserFolders(queryData.Item2, queryData.Item1);
                Folder incomingVersion;

                foreach (Folder localCopy in localFoldersForThisQuery)
                {
                    if (localCopy.FolderId == "all")
                    {
                        incomingVersion = CreateFolderForAllImages(incomingFolders);
                        ChangeType changeType = CopyFolderData(localCopy, incomingVersion);
                        refreshed.Add(Tuple.Create(localCopy, changeType));
                    }
                    else
                    {
                        incomingVersion = incomingFolders.Where(item => item.FolderId == localCopy.FolderId).FirstOrDefault();

                        if (incomingVersion == null)
                        {
                            refreshed.Add(Tuple.Create(localCopy, ChangeType.DELETE));
                        }
                        else
                        {
                            ChangeType changeType = CopyFolderData(localCopy, incomingVersion);
                            refreshed.Add(Tuple.Create(localCopy, changeType));
                        }
                    }
                }
            });


            return refreshed.ToArray();
        }



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
        public async Task<Tuple<Folder[], bool>> GetPageOfUserFolders(StorageLocation location, string username, int page)
        {
            Tuple<JsonElement, bool> data = await FetchPageOfUserFolders(location, username, page);

            JsonElement foldersArray = data.Item1;
            bool hasNextPage = data.Item2;


            List<Folder> folders = ConvertJsonArrayToFolders(foldersArray, location, username);

            return Tuple.Create(
                        folders.ToArray(),
                        hasNextPage
                    );
        }



        /// <summary>
        /// Compiles a list of all the queries that need to be done.
        /// </summary>
        /// <param name="foldersNeeded">The folders that need to be refreshed</param>
        /// <returns>
        /// A dictionary of the username and location we need to query, mapped to the 
        /// folders that need to be loaded from that query.
        /// </returns>
        private Dictionary<Tuple<string, StorageLocation>, List<Folder>> PlanQueries(Folder[] foldersNeeded)
        {
            var queries = new Dictionary<Tuple<string, StorageLocation>, List<Folder>>();

            Tuple<string, StorageLocation> requiredQuery;
            foreach (Folder folder in foldersNeeded)
            {
                requiredQuery = Tuple.Create(folder.Username, folder.StoredIn);
                
                if (queries.TryGetValue(requiredQuery, out var existingValue))
                {
                    existingValue.Add(folder);
                }
                else
                {
                    List<Folder> foldersNeedingQuery = new List<Folder>();
                    foldersNeedingQuery.Add(folder);
                    queries[requiredQuery] = foldersNeedingQuery;
                }
            }


            return queries;
        }



        /// <summary>
        /// Copies data from the incoming folder to the local folder, but only the feilds that 
        /// are expected to be updated by the API. Things like username or folder ID are not 
        /// changed.
        /// </summary>
        /// <param name="local">The version of the folder that came from our Database</param>
        /// <param name="incoming">The version of the folder that came from the API</param>
        /// <returns>A ChangeType telling if the local folder was modified or not</returns>
        private ChangeType CopyFolderData(Folder local, Folder incoming)
        {
            bool changesMade = false;

            if (local.ThumbnailUrl != incoming.ThumbnailUrl)
            {
                local.ThumbnailUrl = incoming.ThumbnailUrl;
                changesMade = true;
            }
            

            if (local.TotalImages != incoming.TotalImages)
            {
                local.TotalImages = incoming.TotalImages;
                changesMade = true;
            }


            return (changesMade ? ChangeType.UPDATE : ChangeType.NO_CHANGE);
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
        /// Also includes a boolean telling you if there is another page after this one.
        /// </summary>
        /// <param name="location">The location those folders can be found in, Gallery or Collection</param>
        /// <param name="username">The name of the DeviantArt user whose folders you want to see</param>
        /// <param name="page"></param>
        /// <returns>A Json array of the response data, and a boolean that is true if there is another page after this one</returns>
        private async Task<Tuple<JsonElement, bool>> FetchPageOfUserFolders(StorageLocation location, string username, int page)
        {
            string url = BuildUrl(location, username, page * PAGE_SIZE);
            JsonDocument response = await NetworkUtils.RunGetRequest(url);
            JsonElement root = response.RootElement;


            CheckResponseForErrors(root);
            return Tuple.Create(
                            root.GetProperty("results"), 
                            root.GetProperty("has_more").GetBoolean()
                        );
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
