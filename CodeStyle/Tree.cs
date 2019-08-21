using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollectionUtils;

namespace CodeStyle {
    public class Tree<T> {
        public TreeNode<T> root;

        public Tree(TreeNode<T> root) {
            //this.root = root;// ?? throw new ArgumentNullException();
            SetRoot(root);
        }
        
        public static void Print(TreeNode<T> root) {
            Console.WriteLine(root.Value);
            foreach (var c in root.children) Print(c);
        }
        
        public static int GetNodeCount(TreeNode<T> t) {
            if (Extensions.IsNullOrEmpty(t.children)) return 0;
            int sum = 0;
            foreach (var c in t.children) {
                sum += t.GetChildrenCount();
            }
            return sum;
        }

        public void SetRoot(TreeNode<T> newRoot) {
            this.root = newRoot;
        } 

        public static void ClearTree(TreeNode<T> root) {
            if (root.children.IsEmpty()) {
                root = null;
                return;
            }
            foreach (var c in root.children) ClearTree(c);
            root.children.Clear();
        }

        public static TreeNode<T> Find(TreeNode<T> root, T key) {
            if (root == null) throw new NullReferenceException();
            if (root.children.IsEmpty()) {
                return root.Value.Equals(key) ? root : null;
            }
            if (root.Value.Equals(key)) return root;
            foreach (var c in root.children) {
                TreeNode<T> x = Find(c, key);
                if (x != null) return x;
            }
            return null;
        }

        ~Tree() {
            ClearTree(this.root);
            this.root = null;
        }
    }
}
