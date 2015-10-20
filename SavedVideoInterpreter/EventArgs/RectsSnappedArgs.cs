using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;

namespace SavedVideoInterpreter
{
    public class RectsSnappedArgs : EventArgs
    {
        public IEnumerable<IBoundingBox> Snapped { get; private set; }

        public RectsSnappedArgs(IEnumerable<IBoundingBox> rects)
        {
            Snapped = rects;
        }
    }
}
