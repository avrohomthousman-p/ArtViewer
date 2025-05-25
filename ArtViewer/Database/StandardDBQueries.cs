using Android.Renderscripts;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtViewer.Database
{
    /// <summary>
    /// Class that stores database queries.
    /// </summary>
    internal static class StandardDBQueries
    {
        private static SQLiteConnection database = DatabaseConnection.GetConnection();



        public static void CreateFolder(Folder folder)
        {
            database.Insert(folder);
        }



        public static void UpdateFolder(Folder folder)
        {
            database.Update(folder);
        }



        public static void DeleteFolder(Folder folder)
        {
            database.Delete(folder);
        }



        /// <summary>
        /// Gets the folder with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the folder.</param>
        /// <returns>The matching Folder object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the folder with the specified ID does not exist.</exception>
        public static Folder GetFolderByID(int id)
        {
            return database.Table<Folder>().First(item => item.ID == id);
        }



        public static IEnumerable<Folder> GetAllFolders()
        {
            return database.Table<Folder>();
        }
    }
}
