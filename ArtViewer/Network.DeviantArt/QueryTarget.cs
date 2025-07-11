﻿using ArtViewer.Network.DeviantArt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtViewer.Network.DeviantArt
{

    /// <summary>
    /// Stores which images a query is supposed to fetch, and which of those the system should keep.
    /// 
    /// The imagesToKkeep should be index number(s) withing this query, not the full gallery. So
    /// index 0 is the first image returned by this query, not the first image in the gallery.
    /// 
    /// If itemsToKeep is set to null, all query results are kept.
    /// </summary>
    public class QueryTarget
    {
        public int offset;
        public int queryLimit;
        public List<int>? itemsToKeep;


        public QueryTarget()
        {
            offset = 0;
            queryLimit = ImageQueryService.MAX_QUERY_LIMIT;
            itemsToKeep = null;
        }


        public QueryTarget(int offset, int queryLimit, List<int>? itemsToKeep)
        {
            this.offset = offset;
            this.queryLimit = queryLimit;
            this.itemsToKeep = itemsToKeep;
        }



        /// <summary>
        /// Gets all the indexes of the images that should be kept. Indexes are based on the query 
        /// positions, not the image position in the gallery.
        /// </summary>
        public IEnumerable<int> GetItemsToKeep()
        {
            if (this.itemsToKeep == null)
            {
                //Keep everything
                return Enumerable.Range(0, this.queryLimit);
            }
            else
            {
                return this.itemsToKeep;
            }
        }
    }
}
