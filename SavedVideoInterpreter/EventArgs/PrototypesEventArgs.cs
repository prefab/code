using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SavedVideoInterpreter
{
    public class PrototypesEventArgs : EventArgs
    {
        public PrototypesEventArgs(IEnumerable<string> ptypes)
        {
            Prototypes = ptypes;
        }

        public IEnumerable<string> Prototypes
        {
            get;
            private set;
        }
    }
}
