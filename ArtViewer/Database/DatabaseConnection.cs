using SQLite;

namespace ArtViewer.Database
{
    /// <summary>
    /// Singleton class for getting a database connection.
    /// </summary>
    public static class DatabaseConnection
    {
        private static SQLiteAsyncConnection database = null;
        private static readonly object locker = new object();


        public static SQLiteAsyncConnection GetConnection()
        {
            if (database != null)
            {
                return database;
            }


            lock (locker)
            {
                if (database == null)
                {
                    database = new SQLiteAsyncConnection(BuildDBPath());
                }
            }


            return database;
        }


        private static string BuildDBPath()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var docFolder = Path.Combine(basePath, "Documents");

            if (!Directory.Exists(docFolder))
                Directory.CreateDirectory(docFolder);

            return Path.Combine(docFolder, "folders.db3");
        }
    }
}
