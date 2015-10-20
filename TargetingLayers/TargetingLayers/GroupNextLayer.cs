using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetingLayers
{
    [Serializable]
    public class GroupNextLayer : Layer
    {

        private const int MAX_SPACE_DIST = 12;
        private const int MAX_ROW_DIST = 12;

        public void AfterInterpret(Tree tree)
        {
            
        }

        public IEnumerable<string> AnnotationLibraries()
        {
            return new List<string>();
        }

        public void Close()
        {
            
        }

        public void Init(Dictionary<string, object> parameters)
        {
            
        }

        public void Interpret(InterpretArgs args)
        {
            InterpretHelper(args, args.Tree);
        }



        //For each element, see if it's in an existing row.
        // If not, create a row.
        // Sort the rows.
        //Sort the cols.
        public static List<Tree> SortNodesInReadingOrder(IEnumerable<Tree> siblings)
        {
            List<Tree> sorted = new List<Tree>();
            List<List<Tree>> rows = NodesByRows(siblings);

            foreach (var row in rows)
            {
                foreach (Tree node in row)
                {
                    sorted.Add(node);
                }
            }

            return sorted;
        }

        private static int CompareByHeightBiggerFirst(IBoundingBox a, IBoundingBox b)
        {
            return b.Height - a.Height;
        }

        private static List<List<Tree>> NodesByRows(IEnumerable<Tree> text)
        {
            List<List<Tree>> rows = new List<List<Tree>>();
            List<Tree> txtSortedByHeight = new List<Tree>(text);
            txtSortedByHeight.Sort(CompareByHeightBiggerFirst);

            foreach (Tree txt in txtSortedByHeight)
            {
                bool added = false;
                foreach (List<Tree> row in rows)
                {
                    if (BoundingBox.IsAlignedHorizontally(row[0], txt))
                    {
                        row.Add(txt);
                        added = true;
                    }
                }

                if (!added)
                {
                    List<Tree> row = new List<Tree>();
                    row.Add(txt);
                    rows.Add(row);
                }
            }

            foreach (var row in rows)
            {
                row.Sort(BoundingBox.CompareByLeft);
            }

            rows.Sort(CompareRows);

            return rows; 
        }

        private static int CompareByRight(IBoundingBox a, IBoundingBox b)
        {
            return (a.Left + a.Width) - (b.Left + b.Width);
        }

        //private static List<GroupedText> GetGroups(IEnumerable<Tree> text)
        //{

            
        //    List<GroupedText> groups = new List<GroupedText>();
        //    if (text.Count() > 0)
        //    {
                
        //        groups.Add(new GroupedText(text.First()));
        //        text = text.Skip(1);

        //        foreach (Tree txt in text)
        //        {
        //            GroupedText nearest = GetClosest(txt, groups);
        //            int right = nearest.Boundary.Left + nearest.Boundary.Width;
        //            int bottom = nearest.Boundary.Top + nearest.Boundary.Height;

        //            if ((txt.Left < right + MAX_SPACE_DIST) && (txt.Top < bottom + MAX_ROW_DIST))
        //                nearest.Add(txt);
        //            else
        //            {
        //                GroupedText next = new GroupedText(txt);
        //                groups.Add(next);
        //            }
        //        }
        //    }
        //    foreach (GroupedText group in groups)
        //        group.Group.Sort(BoundingBox.CompareByTopThenLeft);

        //    groups.Sort( CompareGroupBoundingBoxTopTheLeft );

        //    return groups;
        //}

        private static int CompareGroupBoundingBoxTopTheLeft(GroupedText g1, GroupedText g2)
        {
            return BoundingBox.CompareByTopThenLeft(g1.Boundary, g2.Boundary);
        }


        private static int CompareRows(List<Tree> l1, List<Tree> l2)
        {
            return l1[0].Top - l2[0].Top;
        }

        private class GroupedText
        {
            public List<IBoundingBox> Group;
            public IBoundingBox Boundary;


            public GroupedText(IBoundingBox first)
            {
                Group = new List<IBoundingBox>();
                Boundary = first;
                Group.Add(Boundary);
            }

            public void Add(IBoundingBox box)
            {
                Group.Add(box);
                Boundary = BoundingBox.Union(box, Boundary);
            }
        }

        public void InterpretHelper(InterpretArgs args, Tree currNode)
        {
            IEnumerable<Tree> textChildren = currNode.GetChildren().Where(c => c.ContainsTag("is_text") && (bool)c["is_text"]);


            List<Tree> readingOrder = SortNodesInReadingOrder(textChildren);

            for (int i = 0; i < readingOrder.Count - 1; i++)
            {
                Tree curr = readingOrder[i];
                Tree next = readingOrder[i + 1];
                
                args.Tag(curr, "group_next", GroupNext(curr, next));
            }

            //recurse
            foreach (Tree child in currNode.GetChildren())
                InterpretHelper(args, child);
        }

        private static GroupedText GetClosest(IBoundingBox txt, List<GroupedText> groups)
        {
            double mindist = double.MaxValue;
            GroupedText closest = null;

            foreach (GroupedText set in groups)
            {
                double xdist = 0;
                double ydist = 0;
                double dist = 0;

                if (!BoundingBox.IsAlignedVertically(txt, set.Boundary))
                {
                    if (txt.Left < set.Boundary.Left)
                        xdist = set.Boundary.Left - (txt.Left + txt.Width);
                    else
                        xdist = txt.Left - (set.Boundary.Left + set.Boundary.Width);
                }
                if (!BoundingBox.IsAlignedHorizontally(txt, set.Boundary))
                {
                    if (txt.Top < set.Boundary.Top)
                        ydist = set.Boundary.Top - (txt.Top + txt.Height);
                    else
                        ydist = txt.Top - (set.Boundary.Top + set.Boundary.Height);
                }

                dist = Math.Sqrt(xdist * xdist + ydist * ydist);

                if(dist < mindist)
                {
                    mindist = dist;
                    closest = set;
                }
            }

            return closest;
        }



        //private bool IsToTheRightAndInLine(Tree possibleLeft, Tree possibleRight)
        //{
        //    bool isInLine = BoundingBox.IsAlignedHorizontally(possibleLeft, possibleRight);
        //    return isInLine && possibleLeft.Left + possibleLeft.Width < possibleRight.Left;
        //}

        private bool GroupNext(Tree curr, Tree next)
        {
            int leftDist;
            int topDist;

            if (BoundingBox.IsAlignedVertically(curr, next))
                leftDist = 0;
            else if (curr.Left < next.Left)
                leftDist = next.Left - (curr.Left + curr.Width);
            else
                leftDist = curr.Left - (next.Left + next.Width);

            if (BoundingBox.IsAlignedHorizontally(curr, next))
                topDist = 0;
            else if (curr.Top < next.Top)
            {
                topDist = next.Top - (curr.Top + curr.Height);
            }
            else
            {
                topDist = curr.Top - (next.Top + next.Height);
            }

            bool closeXDist = leftDist < MAX_SPACE_DIST;
            bool closeYDist = topDist < MAX_ROW_DIST;

            //int yDist = next.Top - curr.Top;
            //bool horizontallyAligned = BoundingBox.IsAlignedHorizontally(curr, next);
            return closeXDist && closeYDist;
        }

        public string Name
        {
            get { return "group_next"; }
        }

        public void ProcessAnnotations(AnnotationArgs args)
        {
            
        }
    }
}
