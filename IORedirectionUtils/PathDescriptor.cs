using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabUtils
{
    public class PathDescriptor
    {

        //public static string GetSubtreeBasedDescriptor(Tree node)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    bool recurse = false;
        //    if (node.ContainsAttribute("type"))
        //    {
        //        switch (node["type"].ToString())
        //        {
        //            case "ptype":
        //                sb.Append(node["ptype_id"].ToString());
        //                break;

        //            case "content":
        //                sb.Append("content[@pixel_hash=" + node["pixel_hash"] + "]");
        //                break;

        //            case "frame":
        //                //todo: use more attributes
        //                sb.Append("frame");
        //                break;
        //            case "feature":
        //                sb.Append("feature[@id=" + node["feature_id"] +"]");
        //                break;
        //            default:
        //                recurse = true;
        //                break;
        //        }


        //    }
        //    else
        //        recurse = true;


        //    if(recurse)
        //    {
        //        var children = node.GetChildren();
        //        foreach (Tree child in children)
        //            sb.Append("/" + GetSubtreeBasedDescriptor(child));
        //    }

        //    return sb.ToString();
        //}

        
        public static string GetPath(Tree node, Tree root)
        {
            StringBuilder sb = new StringBuilder();
            GetPathHelper(node, root, sb);
            return sb.ToString();
        }

        private static void GetPathHelper(Tree node, Tree root, StringBuilder sb)
        {
            if (node == null)
                return;

            Tree parent = GetParent(node, root);
            GetLocalPath(node, parent, sb);

            if (parent != null)
            {
                sb.Append("/");
                GetPathHelper(parent, root, sb);
            }
        }

        public static Tree GetParent(Tree node, Tree root)
        {
            if (root == null || node == null)
                return null;

            if (root == node)
                return null;

            if (root.GetChildren().Contains(node))
                return root;


            foreach (Tree child in root.GetChildren())
            {
                Tree found = GetParent(node, child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static void GetLocalPath(Tree node, Tree parent, StringBuilder sb)
        {
            if (node.HasTag("type"))
            {
                switch (node["type"].ToString())
                {
                    case "ptype":
                        sb.Append(node["ptype_id"].ToString());
                        break;

                    case "frame":
                        sb.Append("frame");
                        break;

                    case "content":
                        sb.Append("content[@pixel_hash=" + node["pixel_hash"] + "]");

                        break;
                    
                    default:
                        sb.Append(node["type"]);
                        break;

                }
            }

            int index = GetNodeIndex(node, parent);
            sb.Append("[" + index + "]");

        }

        private static int GetNodeIndex(Tree node, Tree parent)
        {
            int index = 0;

            string pixelhash = null;
            string type = "none";
            if(node.HasTag("type"))
                type = node["type"].ToString();

            if(node.HasTag("type") && node["type"].Equals("content"))
                pixelhash = node["pixel_hash"].ToString();

            if(parent != null){
                var children = parent.GetChildren();
                List<Tree> sorted = new List<Tree>(children);  
                sorted.Sort(BoundingBox.CompareByTopThenLeft);
                foreach(Tree sibling in sorted){
                    if(sibling == node)
                        return index;
                    else{
                        string sibtype = "none";
                        if(sibling.HasTag("type"))
                            sibtype = sibling["type"].ToString();

                        if(sibtype.Equals("ptype") && type.Equals("ptype")){
                            if(sibling["ptype_id"].Equals(node["ptype_id"]))
                                index++;
                        }else if(sibtype.Equals(type)){
                            index++;
                        }
                    }
                }
            }

            return index;
        }






        public static string GetPath(MutableTree node, MutableTree root)
        {
            StringBuilder sb = new StringBuilder();
            GetPathHelper(node, root, sb);
            return sb.ToString();
        }

        private static void GetPathHelper(MutableTree node, MutableTree root, StringBuilder sb)
        {
            if (node == null)
                return;

            MutableTree parent = GetParent(node, root);
            GetLocalPath(node, parent, sb);

            if (parent != null)
            {
                sb.Append("/");
                GetPathHelper(parent, root, sb);
            }
        }

        public static MutableTree GetParent(MutableTree node, MutableTree root)
        {
            if (root == null || node == null)
                return null;

            if (root == node)
                return null;

            if (root.GetChildren().Contains(node))
                return root;


            foreach (MutableTree child in root.GetChildren())
            {
                MutableTree found = GetParent(node, child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static void GetLocalPath(MutableTree node, MutableTree parent, StringBuilder sb)
        {
            if (node.ContainsAttribute("type"))
            {
                switch (node["type"].ToString())
                {
                    case "ptype":
                        sb.Append(node["ptype_id"].ToString());
                        break;

                    case "frame":
                        sb.Append("frame");
                        break;

                    case "content":
                        sb.Append("content[@pixel_hash=" + node["pixel_hash"] + "]");

                        break;

                    default:
                        sb.Append(node["type"]);
                        break;

                }
            }

            int index = GetNodeIndex(node, parent);
            sb.Append("[" + index + "]");

        }

        private static int GetNodeIndex(MutableTree node, MutableTree parent)
        {
            int index = 0;

            string pixelhash = null;
            string type = "none";
            if (node.ContainsAttribute("type"))
                type = node["type"].ToString();

            if (node.ContainsAttribute("type") && node["type"].Equals("content"))
                pixelhash = node["pixel_hash"].ToString();

            if (parent != null)
            {
                var children = parent.GetChildren();
                children.Sort(BoundingBox.CompareByTopThenLeft);
                foreach (MutableTree sibling in children)
                {
                    if (sibling == node)
                        return index;
                    else
                    {
                        string sibtype = "none";
                        if (sibling.ContainsAttribute("type"))
                            sibtype = sibling["type"].ToString();

                        if (sibtype.Equals("ptype") && type.Equals("ptype"))
                        {
                            if (sibling["ptype_id"].Equals(node["ptype_id"]))
                                index++;
                        }
                        else if (sibtype.Equals(type))
                        {
                            index++;
                        }
                    }
                }
            }

            return index;
        }
    }
}

