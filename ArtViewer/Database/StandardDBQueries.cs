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



        private static void SeedDB()
        {
            database.CreateTable<Folder>();

            int count = database.Table<Folder>().Count();

            if (count == 0)
            {
                Folder sample = new Folder("89DB8DF6-9027-4CD2-965F-27CE55CCEFA9", 98, "gallery", "dissunder", true);
                database.Insert(sample);
            }
        }


        //Temporary until I make real data
        public static Folder GetFolder()
        {
            SeedDB();
            return database.Table<Folder>().First();
        }
    }
}
