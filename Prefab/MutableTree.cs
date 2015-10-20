using Newtonsoft.Json.Linq;
using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prefab
{
    public class MutableTree : IBoundingBox
    {
        private List<MutableTree> _children;
        private Dictionary<string, object> _tags;


        public Dictionary<string, object> Tags
        {
            get { return _tags; }
        }


        public List<MutableTree> GetChildren()
        {
            return _children;
        
        
        }

        public List<MutableTree> get_children()
        {
            return GetChildren();
        }

        public static MutableTree FromBoundingBox(IBoundingBox bb, Dictionary<string, object> tags)
        {
            MutableTree tree = new MutableTree();
            tree.Top = bb.Top;
            tree.Left = bb.Left;
            tree.Width = bb.Width;
            tree.Height = bb.Height;

            if (tags != null)
                tree._tags = new Dictionary<string, object>(tags);
            else
                tree._tags = new Dictionary<string, object>();

            tree._children = new List<MutableTree>();

            return tree;
        }

        public bool ContainsAttribute(string key)
        {
            return _tags.ContainsKey(key);
        }

        public IEnumerable<KeyValuePair<string, object>> GetAttributes()
        {
            return _tags;
        }

        public int top
        {
            get { return Top; }
            set { Height = value; }
        }

        public int left
        {
            get { return Left; }
            set { Left = value; }
        }

        public int width
        {
            get { return Width; }
            set { Width = value; }
        }

        public int height
        {
            get { return Height; }
            set { Height = value; }
        }

        public int Top
        {
            get;
            set;
        }

        public int Left
        {
            get;
            set;
        }

        public int Height
        {
            get;
            set;
        }

        public int Width
        {
            get;
            set;
        }

        public object this[string attributeName]
        {
            get
            {
                object value = null;
                _tags.TryGetValue(attributeName, out value);
                return value;
            }
            set
            {
                _tags[attributeName] = value;
            }
        }

        public static string ToJson(MutableTree tree)
        {
            JObject jobject = new JObject();
            ToJsonHelper(tree, jobject);

            return jobject.ToString();
        }

        private static void ToJsonHelper(MutableTree currnode, JObject jo)
        {
            foreach (string tagname in currnode._tags.Keys)
            {
                jo.Add(tagname, currnode._tags[tagname].ToString());
            }

            jo.Add("top", currnode.Top);
            jo.Add("left", currnode.Left);
            jo.Add("width", currnode.Width);
            jo.Add("height", currnode.Height);

            JArray array = new JArray();

            foreach (MutableTree child in currnode._children)
            {
                JObject childjo = new JObject();
                ToJsonHelper(child, childjo);
                array.Add(childjo);
            }

            jo.Add("children", array);
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

        public static IEnumerable<MutableTree> GetSiblings(MutableTree node, MutableTree root)
        {
            MutableTree parent = GetParent(node, root);
            List<MutableTree> siblings = new List<MutableTree>();
            if (parent != null)
            {
                foreach (MutableTree child in parent.GetChildren())
                {
                    if (child != node)
                        siblings.Add(node);
                }
            }

            return siblings;
        }


        public static MutableTree FromTree(Tree tree)
        {
            
            var tags = new Dictionary<string, object>();
            foreach (var pair in tree.GetTags())
            {
                tags[pair.Key] = pair.Value;
            }

            MutableTree node = MutableTree.FromBoundingBox(tree, tags);
            
            foreach (Tree child in tree.GetChildren())
                node._children.Add(FromTree(child));

            return node;
        }
    }
}