using SQLite;
using Android.Content;

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
                    string dbPath = BuildDBPath();
                    DatabaseSeeder.EnsureDBFileExists(dbPath);
                    database = new SQLiteAsyncConnection(dbPath);
                }
            }


            return database;
        }



        private static string BuildDBPath()
        {
            var basePath = Android.App.Application.Context.FilesDir.AbsolutePath;
            return Path.Combine(basePath, DatabaseSeeder.SAMPLE_DB_FILE_NAME);
        }
    }
}
