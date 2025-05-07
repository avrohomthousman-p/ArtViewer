using ArtViewer.Network.Deviantart;
using SQLite;
using System.ComponentModel.DataAnnotations;


namespace ArtViewer.Database
{
    /// <summary>
    /// Model class for storing information about a gallery/collection that the user has saved and
    /// images should be loaded from.
    /// </summary>
    public class Folder
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; private set; }


        [SQLite.MaxLength(255)]
        public string FolderId { get; set; }


        [NotNull]
        public int TotalImages { get; set; } = Network.Deviantart.QueryThreadManager.MAX_IMAGES;


        [SQLite.MaxLength(20)]
        [NotNull]
        public string CollectionType { get; set; }


        [SQLite.MaxLength(50)]
        [NotNull]
        public string Username { get; set; }


        [NotNull]
        public bool ShouldRandomize { get; set; } = true;



        private const string BASE_URL = "https://www.deviantart.com/api/v1/oauth2/{0}/{1}?access_token={2}&username={3}&mature_content=true&limit={4}&offset={5}";


        public Folder() { }


        public Folder(string folderId, int totalImages, string collectionType, string username, bool shouldRandomize)
        {
            FolderId = folderId;
            TotalImages = totalImages;
            CollectionType = collectionType;
            Username = username;
            ShouldRandomize = shouldRandomize;
        }



        public string BuildUrl(int queryLimit, int offset)
        {
            return string.Format(BASE_URL, this.CollectionType, this.FolderId, NetworkUtils.GetAccessToken(), this.Username, queryLimit, offset);
        }
    }
}