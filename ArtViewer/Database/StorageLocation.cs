using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtViewer.Database
{
    /// <summary>
    /// Stores the ways a user can store thier pictures.
    /// </summary>
    public enum StorageLocation
    {
        GALLERY, COLLECTIONS
    }


    public static class StorageLocationExtension
    {

        /// <summary>
        /// Converts the enum to text so it can be embedded in a URL.
        /// </summary>
        public static string AsText(this StorageLocation imageSource)
        {
            switch (imageSource)
            {
                case StorageLocation.GALLERY:
                    return "gallery";
                case StorageLocation.COLLECTIONS:
                    return "collections";
                default:
                    throw new ArgumentException($"{imageSource} is not a valid value for ImageSource enum");
            }
        }
    }
}
