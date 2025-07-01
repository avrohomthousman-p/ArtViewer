using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtViewer.Database
{
    /// <summary>
    /// Ensures that the database is seeded properly. If there is an existing database (like when the app
    /// is being re-installed or opened but not for the first time), that one will be used. If not, (like
    /// when the app is being opened for the first time) the sample database from the Assets folder is copied
    /// over to the proper location so it can be used as the database.
    /// </summary>
    internal static class DatabaseSeeder
    {
        internal const string SAMPLE_DB_FILE_NAME = "prepopulated.db";



        /// <summary>
        /// Copies the sample database file from the assets folder to the specified database file location
        /// only if there isn't one there already.
        /// </summary>
        /// <param name="liveDbPath">The correct filepath for the database file to be stored.</param>
        internal static void EnsureDBFileExists(string liveDbPath)
        {
            if (!File.Exists(liveDbPath))
            {
                using (var assetStream = Android.App.Application.Context.Assets.Open(SAMPLE_DB_FILE_NAME))
                using (var destStream = File.Create(liveDbPath))
                {
                    assetStream.CopyTo(destStream);
                }
            }
        }
    }
}
