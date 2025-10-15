using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtViewer.Network.DeviantArt
{
    /// <summary>
    /// Data class that stores imformation about an image/video that will be needed to display it.
    /// </summary>
    class MediaItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public int Index { get; set; } = -1;
        public bool IsImage { get; set; } = true;


        public MediaItem(string url, string title, bool isImage = true, int index = -1)
        {
            this.Url = url;
            this.Title = title;
            this.IsImage = isImage;
            this.Index = index;
        }
    }
}
