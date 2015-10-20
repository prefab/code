using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prefab
{
    public class LayerException : Exception
    {

        public readonly LayerWrapper Layer;

        public LayerException(LayerWrapper layer, Exception exception) : base(exception.Message, exception)
        {
            Layer = layer;
        }
    }
}
