using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtViewer.Database
{
    /// <summary>
    /// Class for tracking different kinds of changes that were done or need to be done
    /// to a folder.
    /// </summary>
    public enum ChangeType
    {
        NO_CHANGE, UPDATE, DELETE
    }
}
