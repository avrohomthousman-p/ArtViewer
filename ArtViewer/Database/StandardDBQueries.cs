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
        private static SQLiteAsyncConnection database = DatabaseConnection.GetConnection();


        //TODO: replace these temp functions with real queries

        private static async Task CreateSampleData()
        {
            await database.CreateTableAsync<Folder>();

            int count = await database.Table<Folder>().CountAsync();

            if (count == 0)
            {
                Folder sample = new Folder("89DB8DF6-9027-4CD2-965F-27CE55CCEFA9", 98, "gallery", "dissunder", true);
                database.InsertAsync(sample);
            }
        }


        public static async Task<Folder> GetFolder()
        {
            await CreateSampleData();
            return await database.Table<Folder>().FirstAsync();
        }
    }
}
