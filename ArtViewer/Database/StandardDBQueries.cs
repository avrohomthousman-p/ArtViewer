using Android.Renderscripts;
using SQLite;

namespace ArtViewer.Database
{
    /// <summary>
    /// Class that stores database queries.
    /// </summary>
    internal static class StandardDBQueries
    {
        private static SQLiteAsyncConnection database = DatabaseConnection.GetConnection();



        public static async Task CreateFolder(Folder folder)
        {
            await database.InsertAsync(folder);
        }



        /// <summary>
        /// Updates the folder if it exists, otherwise creates a new one.
        /// </summary>
        public static async Task CreateOrUpdateFolder(Folder folder)
        {
            if(folder.ID > 0) //already in the DB
            {
                await UpdateFolderByPK(folder);
                return;
            }


            Folder internalFolder = await GetMatchingFolder(folder);

            if (internalFolder == null)
            {
                await CreateFolder(folder);
            }
            else
            {
                //Copy data from external folder to internal folder
                if(folder.CustomName != null && folder.CustomName != "")
                {
                    internalFolder.CustomName = folder.CustomName;
                }

                internalFolder.TotalImages = folder.TotalImages;
                internalFolder.ThumbnailUrl = folder.ThumbnailUrl;
                internalFolder.ShouldRandomize = folder.ShouldRandomize;

                await UpdateFolderByPK(internalFolder);
            }
        }




        /// <summary>
        /// Updates feilds in a folder that is already in the database. Use this when you have a reference
        /// to a folder that is from the database and has a PK, but you have local chages to it that you 
        /// want to save to the database.
        /// </summary>
        /// <param name="folder"></param>
        public static async Task UpdateFolderByPK(Folder folder)
        {
            await database.UpdateAsync(folder);
        }




        public static async Task DeleteFolder(Folder folder)
        {
            await database.DeleteAsync(folder);
        }



        /// <summary>
        /// Gets a folder from the database that matches the provided external folder, or null if no matching
        /// folder is found. The folders are said to match if they both represent the same deviantArt folder:
        /// i.e. they have the same folder ID (as assigned by the DeviantArt API), or in the case of a full 
        /// collection/gallery (which dont get assigned ID's), they are owned by the same user and have a 
        /// matching location (both are gallery or both are collection).
        /// 
        /// Do not pass in an external folder if that folder instance is from the database (if it has a PK).  
        /// If your folder has a pk, use the GetFolderByID function instead.
        /// </summary>
        /// <param name="externalFolder"></param>
        /// <returns>The internal (from database) folder that represents the same DeviantArt folder as the folder provided</returns>
        public static async Task<Folder> GetMatchingFolder(Folder externalFolder)
        {
            Folder internalFolder;

            if (externalFolder.FolderId == "all") //represents full gallery or full collection
            {
                internalFolder = await database.Table<Folder>()
                                                .Where(item => item.Username == externalFolder.Username
                                                                && item.StoredIn == externalFolder.StoredIn)
                                                .FirstOrDefaultAsync();
            }
            else
            {
                internalFolder = await database.Table<Folder>()
                                                .Where(item => item.FolderId == externalFolder.FolderId)
                                                .FirstOrDefaultAsync();
            }


            return internalFolder;
        }



        /// <summary>
        /// Gets the folder with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the folder.</param>
        /// <returns>The matching Folder object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the folder with the specified ID does not exist.</exception>
        public static async Task<Folder> GetFolderByID(int id)
        {
            return await database.Table<Folder>().FirstAsync(item => item.ID == id);
        }



        public static async Task<IEnumerable<Folder>> GetAllFolders()
        {
            return await database.Table<Folder>()
                                    .OrderBy(folder => folder.CustomName)
                                    .ToListAsync();
        }
    }
}
