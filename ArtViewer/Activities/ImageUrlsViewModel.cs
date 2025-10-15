using AndroidX.Lifecycle;
using ArtViewer.Database;
using ArtViewer.Network.DeviantArt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtViewer.Activities
{
    /// <summary>
    /// Stores the image urls such that they dont get deleted when the screen is rotated
    /// </summary>
    internal class ImageUrlsViewModel : ViewModel
    {
        private List<MediaItem> urls = null;



        /// <summary>
        /// Gets the image/video urls if they are cached, otherwise gets them from the ImageQueryService 
        /// </summary>
        /// <param name="folder">The folder whose items are to be displayed</param>
        /// <returns>The urls to be displayed</returns>
        internal async Task<List<MediaItem>> GetMediaUrlsAsync(Folder folder)
        {
            if (urls == null)
            {
                MediaQueryService service = new MediaQueryService();
                this.urls = await service.LoadAllImages(folder);
                return this.urls;
            }
            else
            {
                return this.urls;
            }
        }
    }
}
