using AndroidX.Lifecycle;
using ArtViewer.Database;
using ArtViewer.Network.Deviantart;
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
        private List<string> urls = null;



        /// <summary>
        /// Gets the image urls if they are cached, otherwise gets them from the ImageQueryService 
        /// </summary>
        /// <param name="folder">The folder whose images are to be displayed</param>
        /// <returns>The image urls to be displayed</returns>
        internal async Task<List<string>> GetImageUrlsAsync(Folder folder)
        {
            if (urls == null)
            {
                ImageQueryService service = new ImageQueryService();
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
