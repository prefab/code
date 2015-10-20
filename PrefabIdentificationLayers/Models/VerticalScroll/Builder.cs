using Prefab;
using PrefabIdentificationLayers.Prototypes;
using PrefabIdentificationLayers.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrefabIdentificationLayers.Models.VerticalScroll
{
    class Builder //: PtypeBuilder
    {

        //public Ptype.Mutable BuildPrototype(IBuildPrototypeArgs args)
        //{
        //    //HierarchicalPtypeBuildArgs hargs = args as HierarchicalPtypeBuildArgs;
        //    //IEnumerable<Bitmap> positives = hargs.Examples.Positives;
        //    //IEnumerable<Bitmap> negatives = hargs.Examples.Negatives;

        //    //IEnumerable<Tree> existing = hargs.ChildOccurrences.First();

        //    //if (existing.Count() != 3)
        //    //    return null;

        //    //IEnumerable<Tree> sortedByTop = existing.OrderBy((o) => o.TopOffset);

        //    //Tree top = sortedByTop.ElementAt(0);
        //    //Tree bottom = sortedByTop.ElementAt(2);
        //    //Tree thumb = sortedByTop.ElementAt(1);


        //    //Bitmap topTrack = null;
        //    //Bitmap bottomTrack = null;
        //    //foreach (Bitmap pos in positives)
        //    //{
        //    //    Bitmap toppattern = VerticalPatternMatcher.ShortestPattern(pos, 0, top.Height, thumb.TopOffset - 1, pos.Width);

        //    //    if (topTrack == null)
        //    //        topTrack = toppattern;

        //    //    else if (!topTrack.Equals(toppattern))
        //    //        return null;


        //    //    Bitmap bottompattern = VerticalPatternMatcher.ShortestPattern(pos, 0, thumb.TopOffset + thumb.Height, bottom.TopOffset - 1, pos.Width);

        //    //    if (bottomTrack == null)
        //    //        bottomTrack = bottompattern;
        //    //    else if (!bottomTrack.Equals(bottompattern))
        //    //        return null;
        //    //}


        //    //Dictionary<string, Bitmap> features = new Dictionary<string, Bitmap>();
        //    //Dictionary<string, Region> regions = new Dictionary<string, Region>();
        //    //regions["top"] = new Region("vertical", topTrack);
        //    //regions["bottom"] = new Region("vertical", bottomTrack);

        //    //Dictionary<string, Guid> children = new Dictionary<string, Guid>();
        //    //children["top"] = top.Prototype.Guid;
        //    //children["bottom"] = bottom.Prototype.Guid;
        //    //children["thumb"] = thumb.Prototype.Guid;

        //    //return new Ptype.Hierarchical.HierarchicalMutable(features, regions, children);
        //}



    }
}
