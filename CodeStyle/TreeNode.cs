using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStyle {
    public class TreeNode<T> {

        public T Value {
            get; private set;
        }

        public TreeNode<T> parent;
        public List<TreeNode<T>> children;
        
        public TreeNode(T value, TreeNode<T> parent = null) {
            this.Value = value;
            this.parent = parent;
            this.children = new List<TreeNode<T>>();
            if (parent != null) this.parent.AddChild(this);
        }

        public void SetParent(TreeNode<T> node) {
            this.parent = node;
        }

        public void AddChild(TreeNode<T> node) {
            if (node == null) throw new ArgumentNullException();
            this.children.Add(node);
            node.SetParent(this);
        }

        public TreeNode<T> GetChild(int index) {
            if (index < 0 || index >= this.children.Count) throw new ArgumentOutOfRangeException();
            return this.children[index];
        }

        public void RemoveChild(int index) {
            if (index < 0 || index >= this.children.Count) throw new ArgumentOutOfRangeException();
            this.children.RemoveAt(index);
        }
        
        public void RemoveChild(TreeNode<T> node) {
            if (node == null) throw new NullReferenceException();
            this.children.Remove(node);
        }

        public int GetChildrenCount() => this.children.Count;
    }
}
