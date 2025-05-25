using SQLite;
using Android.Content;

namespace ArtViewer.Database
{
    /// <summary>
    /// Singleton class for getting a database connection.
    /// </summary>
    public static class DatabaseConnection
    {
        private static SQLiteConnection database = null;
        private static readonly object locker = new object();
        


        public static SQLiteConnection GetConnection()
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
                    database = new SQLiteConnection(dbPath);
                }
            }


            return database;
        }



        private static string BuildDBPath()
        {
            var basePath = Android.App.Application.Context.FilesDir.AbsolutePath;
            return Path.Combine(basePath, DatabaseSeeder.DB_FILE_NAME);
        }
    }
}
