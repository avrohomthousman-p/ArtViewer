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



        public static async Task UpdateFolder(Folder folder)
        {
            await database.UpdateAsync(folder);
        }



        public static async Task DeleteFolder(Folder folder)
        {
            await database.DeleteAsync(folder);
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
            return await database.Table<Folder>().ToListAsync();
        }
    }
}
