using System.Diagnostics;
using System.Text;

/*
 * (C) Copyright 2020-2021, by Timofey Chudakov and Contributors.
 *
 * JGraphT : a free Java graph-theory library
 *
 * See the CONTRIBUTORS.md file distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Eclipse Public License 2.0 which is available at
 * http://www.eclipse.org/legal/epl-2.0, or the
 * GNU Lesser General Public License v2.1 or later
 * which is available at
 * http://www.gnu.org/licenses/old-licenses/lgpl-2.1-standalone.html.
 *
 * SPDX-License-Identifier: EPL-2.0 OR LGPL-2.1-or-later
 */
namespace NGraphT.Core.Util;

/// <summary>
/// Implementation of the <a href="https://en.wikipedia.org/wiki/AVL_tree">AVL tree</a> data
/// structure. <b>Note:</b> this tree doesn't use key comparisons, so this tree can't be used as a
/// binary search tree. This implies that the same key can be added to this tree multiple times.
/// <para>
/// AVL tree is a self-balancing binary tree data structure. In an AVL tree, the heights of two child
/// subtrees differ by at most one. This ensures that the height of the tree is $\mathcal{O}(\log n)$
/// where $n$ is the number of elements in the tree. Also this tree doesn't support key comparisons,
/// it does define an element order. As a result, this tree can be used to query node
/// successor/predecessor.
/// </para>
/// <para>
/// Subtree query means that the result is being computed only on the subtree nodes. This tree
/// supports the following operations:
/// <ul>
/// <li>Min/max insertion and deletion in $\mathcal{O}(\log n)$ time</li>
/// <li>Subtree min/max queries in $\mathcal{O}(1)$ time</li>
/// <li>Node successor/predecessor queries in $\mathcal{O}(1)$ time</li>
/// <li>Tree split in $\mathcal{O}(\log n)$ time</li>
/// <li>Tree merge in $\mathcal{O}(\log n)$ time</li>
/// </ul>
/// </para>
/// <para>
/// This implementation gives users access to the tree nodes which hold the inserted elements. The
/// user is able to store the tree nodes references but isn't able to modify them.
///
/// </para>
/// </summary>
/// @param <T> the key data type
/// <remarks>Author: Timofey Chudakov.</remarks>
public class AvlTree<T> : IEnumerable<T>
{
    /// <summary>
    /// An auxiliary node which's always present in a tree and doesn't contain any data.
    /// </summary>
    private TreeNode<T> _virtualRoot = new TreeNode<T>(default(T));

    /// <summary>
    /// Modification tracker
    /// </summary>
    private int _modCount = 0;

    /// <summary>
    /// Constructs an empty tree
    /// </summary>
    public AvlTree()
    {
    }

    /// <summary>
    /// Constructor for internal usage
    /// </summary>
    /// <param name="root"> the root of the newly create tree.</param>
    private AvlTree(TreeNode<T> root)
    {
        MakeRoot(root);
    }

    /// <summary>
    /// Adds {@code value} as a maximum element to this tree. The running time of this method is
    /// $\mathcal{O}(\log n)$
    /// </summary>
    /// <param name="value"> a value to add as a tree max.</param>
    /// <returns>a tree node holding the {@code value}</returns>
    public virtual TreeNode<T> AddMax(T value)
    {
        TreeNode<T> newMax = new TreeNode<T>(value);
        AddMaxNode(newMax);
        return newMax;
    }

    /// <summary>
    /// Adds the {@code newMax} as a maximum node to this tree.
    /// </summary>
    /// <param name="newMax"> a node to add as a tree max.</param>
    public virtual void AddMaxNode(TreeNode<T> newMax)
    {
        RegisterModification();

        if (Empty)
        {
            _virtualRoot.left = newMax;
            newMax.parent     = _virtualRoot;
        }
        else
        {
            TreeNode<T> max = Max;
            max.SetRightChild(newMax);
            Balance(max);
        }
    }

    /// <summary>
    /// Adds the {@code value} as a minimum element to this tree
    /// </summary>
    /// <param name="value"> a value to add as a tree min.</param>
    /// <returns>a tree node holding the {@code value}</returns>
    public virtual TreeNode<T> AddMin(T value)
    {
        TreeNode<T> newMin = new TreeNode<T>(value);
        AddMinNode(newMin);
        return newMin;
    }

    /// <summary>
    /// Adds the {@code newMin} as a minimum node to this tree
    /// </summary>
    /// <param name="newMin"> a node to add as a tree min.</param>
    public virtual void AddMinNode(TreeNode<T> newMin)
    {
        RegisterModification();
        if (Empty)
        {
            _virtualRoot.left = newMin;
            newMin.parent     = _virtualRoot;
        }
        else
        {
            TreeNode<T> min = Min;
            min.SetLeftChild(newMin);
            Balance(min);
        }
    }

    /// <summary>
    /// Splits the tree into two parts.
    /// <para>
    /// The first part contains the nodes which are smaller than or equal to the {@code node}. The
    /// first part stays in this tree. The second part contains the nodes which are strictly greater
    /// than the {@code node}. The second part is returned as a tree.
    ///
    /// </para>
    /// </summary>
    /// <param name="node"> a separating node.</param>
    /// <returns>a tree containing the nodes which are strictly greater than the {@code node}</returns>
    public virtual AvlTree<T> SplitAfter(TreeNode<T> node)
    {
        RegisterModification();

        TreeNode<T> parent   = node.parent;
        bool        nextMove = node.isLeftChild();
        TreeNode<T> left     = node.left;
        TreeNode<T> right    = node.right;

        node.parent.SubstituteChild(node, null);

        node.Reset();

        if (left != null)
        {
            left.parent = null;
        }

        if (right != null)
        {
            right.parent = null;
        }

        if (left == null)
        {
            left = node;
        }
        else
        {
            // insert node as a left subtree max
            TreeNode<T> t = left;
            while (t.right != null)
            {
                t = t.right;
            }

            t.SetRightChild(node);

            while (t != left)
            {
                TreeNode<T> p = t.parent;
                p.SubstituteChild(t, BalanceNode(t));
                t = p;
            }

            left = BalanceNode(left);
        }

        return Split(left, right, parent, nextMove);
    }

    /// <summary>
    /// Splits the tree into two parts.
    /// <para>
    /// The first part contains the nodes which are smaller than the {@code node}. The first part
    /// stays in this tree. The second part contains the nodes which are greater than or equal to the
    /// {@code node}. The second part is returned as a tree.
    ///
    /// </para>
    /// </summary>
    /// <param name="node"> a separating node.</param>
    /// <returns>a tree containing the nodes which are greater than or equal to the {@code node}</returns>
    public virtual AvlTree<T> SplitBefore(TreeNode<T> node)
    {
        RegisterModification();

        TreeNode<T> predecessor = Predecessor(node);
        if (predecessor == null)
        {
            // node is a minimum node
            AvlTree<T> tree = new AvlTree<T>();
            Swap(tree);
            return tree;
        }

        return SplitAfter(predecessor);
    }

    /// <summary>
    /// Append the nodes in the {@code tree} after the nodes in this tree.
    /// <para>
    /// The result of this operation is stored in this tree.
    ///
    /// </para>
    /// </summary>
    /// <param name="tree"> a tree to append.</param>
    public virtual void MergeAfter(AvlTree<T> tree)
    {
        RegisterModification();
        if (tree.Empty)
        {
            return;
        }
        else if (tree.Size == 1)
        {
            AddMaxNode(tree.RemoveMin());
            return;
        }

        TreeNode<T> junctionNode = tree.RemoveMin();
        TreeNode<T> treeRoot     = tree.Root;
        tree.Clear();

        MakeRoot(Merge(junctionNode, Root, treeRoot));
    }

    /// <summary>
    /// Prepends the nodes in the {@code tree} before the nodes in this tree.
    /// <para>
    /// The result of this operation is stored in this tree.
    ///
    /// </para>
    /// </summary>
    /// <param name="tree"> a tree to prepend.</param>
    public virtual void MergeBefore(AvlTree<T> tree)
    {
        RegisterModification();

        tree.MergeAfter(this);

        Swap(tree);
    }

    /// <summary>
    /// Removes the minimum node in this tree. Returns {@code null} if this tree is empty
    /// </summary>
    /// <returns>the removed node or {@code null} if this tree is empty.</returns>
    public virtual TreeNode<T> RemoveMin()
    {
        RegisterModification();

        if (Empty)
        {
            return null;
        }

        TreeNode<T> min = Min;
        // min.parent != null
        if (min.parent == _virtualRoot)
        {
            MakeRoot(min.right);
        }
        else
        {
            min.parent.SetLeftChild(min.right);
        }

        Balance(min.parent);

        return min;
    }

    /// <summary>
    /// Removes the maximum node in this tree. Returns {@code null} if this tree is empty
    /// </summary>
    /// <returns>the removed node or {@code null} if this tree is empty.</returns>
    public virtual TreeNode<T> RemoveMax()
    {
        RegisterModification();
        if (Empty)
        {
            return null;
        }

        TreeNode<T> max = Max;
        if (max.parent == _virtualRoot)
        {
            MakeRoot(max.left);
        }
        else
        {
            max.parent.SetRightChild(max.left);
        }

        Balance(max.parent);
        return max;
    }

    /// <summary>
    /// Returns the root of this tree or null if this tree is empty.
    /// </summary>
    /// <returns>the root of this tree or null if this tree is empty.</returns>
    public virtual TreeNode<T> Root
    {
        get
        {
            return _virtualRoot.left;
        }
    }

    /// <summary>
    /// Returns the node following the {@code node} in the order defined by this tree. Returns null
    /// if the {@code node} is the maximum node in the tree.
    /// </summary>
    /// <param name="node"> a node to compute successor of.</param>
    /// <returns>the successor of the {@code node}</returns>
    public virtual TreeNode<T> Successor(TreeNode<T> node)
    {
        return node.successor;
    }

    /// <summary>
    /// Returns the node, which is before the {@code node} in the order defined by this tree. Returns
    /// null if the {@code node} is the minimum node in the tree.
    /// </summary>
    /// <param name="node"> a node to compute predecessor of.</param>
    /// <returns>the predecessor of the {@code node}</returns>
    public virtual TreeNode<T> Predecessor(TreeNode<T> node)
    {
        return node.predecessor;
    }

    /// <summary>
    /// Returns the minimum node in this tree or null if the tree is empty.
    /// </summary>
    /// <returns>the minimum node in this tree or null if the tree is empty.</returns>
    public virtual TreeNode<T> Min
    {
        get
        {
            return Root == null ? null : Root.SubtreeMin;
        }
    }

    /// <summary>
    /// Returns the maximum node in this tree or null if the tree is empty.
    /// </summary>
    /// <returns>the maximum node in this tree or null if the tree is empty.</returns>
    public virtual TreeNode<T> Max
    {
        get
        {
            return Root == null ? null : Root.SubtreeMax;
        }
    }

    /// <summary>
    /// Check if this tree is empty
    /// </summary>
    /// <returns>{@code true} if this tree is empty, {@code false otherwise}</returns>
    public virtual bool Empty
    {
        get
        {
            return Root == null;
        }
    }

    /// <summary>
    /// Removes all nodes from this tree.
    /// <para>
    /// <b>Note:</b> the memory allocated for the tree structure won't be deallocated until there are
    /// active external referenced to the nodes of this tree.
    /// </para>
    /// </summary>
    public virtual void Clear()
    {
        RegisterModification();

        _virtualRoot.left = null;
    }

    /// <summary>
    /// Returns the size of this tree
    /// </summary>
    /// <returns>the size of this tree.</returns>
    public virtual int Size
    {
        get
        {
            return _virtualRoot.left == null ? 0 : _virtualRoot.left.subtreeSize;
        }
    }

    /// <summary>
    /// Makes the {@code node} the root of this tree
    /// </summary>
    /// <param name="node"> a new root of this tree.</param>
    private void MakeRoot(TreeNode<T> node)
    {
        _virtualRoot.left = node;
        if (node != null)
        {
            node.subtreeMax.successor   = null;
            node.subtreeMin.predecessor = null;
            node.parent                 = _virtualRoot;
        }
    }

    /// <summary>
    /// Traverses the tree up until the virtual root and splits it into two parts.
    /// <para>
    /// The algorithm is described in <i>Donald TEdge. Knuth. The art of computer programming. Second
    /// Edition. Volume 3 / Sorting and Searching, p. 474</i>.
    ///
    /// </para>
    /// </summary>
    /// <param name="left"> a left subtree.</param>
    /// <param name="right"> a right subtree.</param>
    /// <param name="p"> next parent node.</param>
    /// <param name="leftMove"> {@code true} if we're moving from the left child, {@code false} otherwise.</param>
    /// <returns>the resulting right tree.</returns>
    private AvlTree<T> Split(TreeNode<T> left, TreeNode<T> right, TreeNode<T> p, bool leftMove)
    {
        while (p != _virtualRoot)
        {
            bool        nextMove = p.isLeftChild();
            TreeNode<T> nextP    = p.parent;

            p.parent.SubstituteChild(p, null);
            p.parent = null;

            if (leftMove)
            {
                right = Merge(p, right, p.right);
            }
            else
            {
                left = Merge(p, p.left, left);
            }

            p        = nextP;
            leftMove = nextMove;
        }

        MakeRoot(left);

        return new AvlTree<T>(right);
    }

    /// <summary>
    /// Merges the {@code left} and {@code right} subtrees using the {@code junctionNode}.
    /// <para>
    /// The algorithm is described in <i>Donald TEdge. Knuth. The art of computer programming. Second
    /// Edition. Volume 3 / Sorting and Searching, p. 474</i>.
    ///
    /// </para>
    /// </summary>
    /// <param name="junctionNode"> a node between left and right subtrees.</param>
    /// <param name="left"> a left subtree.</param>
    /// <param name="right"> a right subtree.</param>
    /// <returns>the root of the resulting tree.</returns>
    private TreeNode<T> Merge(TreeNode<T> junctionNode, TreeNode<T> left, TreeNode<T> right)
    {
        if (left == null && right == null)
        {
            junctionNode.Reset();
            return junctionNode;
        }
        else if (left == null)
        {
            right.SetLeftChild(Merge(junctionNode, left, right.left));
            return BalanceNode(right);
        }
        else if (right == null)
        {
            left.SetRightChild(Merge(junctionNode, left.right, right));
            return BalanceNode(left);
        }
        else if (left.Height > right.Height + 1)
        {
            left.SetRightChild(Merge(junctionNode, left.right, right));
            return BalanceNode(left);
        }
        else if (right.Height > left.Height + 1)
        {
            right.SetLeftChild(Merge(junctionNode, left, right.left));
            return BalanceNode(right);
        }
        else
        {
            junctionNode.SetLeftChild(left);
            junctionNode.SetRightChild(right);
            return BalanceNode(junctionNode);
        }
    }

    /// <summary>
    /// Swaps the contents of this tree and the {@code tree}
    /// </summary>
    /// <param name="tree"> a tree to swap content of.</param>
    private void Swap(AvlTree<T> tree)
    {
        TreeNode<T> t = _virtualRoot.left;
        MakeRoot(tree._virtualRoot.left);
        tree.MakeRoot(t);
    }

    /// <summary>
    /// Performs a right node rotation.
    /// </summary>
    /// <param name="node"> a node to rotate.</param>
    /// <returns>a new parent of the {@code node}</returns>
    private TreeNode<T> RotateRight(TreeNode<T> node)
    {
        TreeNode<T> left = node.left;
        left.parent = null;

        node.SetLeftChild(left.right);
        left.SetRightChild(node);

        node.UpdateHeightAndSubtreeSize();
        left.UpdateHeightAndSubtreeSize();

        return left;
    }

    /// <summary>
    /// Performs a left node rotation.
    /// </summary>
    /// <param name="node"> a node to rotate.</param>
    /// <returns>a new parent of the {@code node}</returns>
    private TreeNode<T> RotateLeft(TreeNode<T> node)
    {
        TreeNode<T> right = node.right;
        right.parent = null;

        node.SetRightChild(right.left);

        right.SetLeftChild(node);

        node.UpdateHeightAndSubtreeSize();
        right.UpdateHeightAndSubtreeSize();

        return right;
    }

    /// <summary>
    /// Performs a node balancing on the path from {@code node} up until the root
    /// </summary>
    /// <param name="node"> a node to start tree balancing from.</param>
    private void Balance(TreeNode<T> node)
    {
        Balance(node, _virtualRoot);
    }

    /// <summary>
    /// Performs a node balancing on the path from {@code node} up until the {@code stop} node
    /// </summary>
    /// <param name="node"> a node to start tree balancing from.</param>
    /// <param name="stop"> a node to stop balancing at (this node is not being balanced) </param>
    private void Balance(TreeNode<T> node, TreeNode<T> stop)
    {
        if (node == stop)
        {
            return;
        }

        TreeNode<T> p = node.parent;
        if (p == _virtualRoot)
        {
            MakeRoot(BalanceNode(node));
        }
        else
        {
            p.SubstituteChild(node, BalanceNode(node));
        }

        Balance(p, stop);
    }

    /// <summary>
    /// Checks whether the {@code node} is unbalanced. If so, balances the {@code node}
    /// </summary>
    /// <param name="node"> a node to balance.</param>
    /// <returns>a new parent of {@code node} if the balancing occurs, {@code node} otherwise.</returns>
    private TreeNode<T> BalanceNode(TreeNode<T> node)
    {
        node.UpdateHeightAndSubtreeSize();
        if (node.LeftDoubleHeavy)
        {
            if (node.left.RightHeavy)
            {
                node.SetLeftChild(RotateLeft(node.left));
            }

            RotateRight(node);
            return node.parent;
        }
        else if (node.RightDoubleHeavy)
        {
            if (node.right.LeftHeavy)
            {
                node.SetRightChild(RotateRight(node.right));
            }

            RotateLeft(node);
            return node.parent;
        }

        return node;
    }

    /// <summary>
    /// Registers a modifying operation
    /// </summary>
    private void RegisterModification()
    {
        ++_modCount;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var builder = new StringBuilder();
        for (IEnumerator<TreeNode<T>> i = NodeIterator(); i.MoveNext();)
        {
            TreeNode<T> node = i.Current;
            builder.Append(node.ToString()).Append("\n");
        }

        return builder.ToString();
    }

    /// <inheritdoc/>
    public virtual IEnumerator<T> GetEnumerator()
    {
        return new TreeValuesIterator(this);
    }

    /// <summary>
    /// Returns an iterator over the tree nodes rather than the node values. The tree are returned in
    /// the same order as the tree values.
    /// </summary>
    /// <returns>an iterator over the tree nodes.</returns>
    public virtual IEnumerator<TreeNode<T>> NodeIterator()
    {
        return new TreeNodeIterator(this);
    }

    /// <summary>
    /// Iterator over the values stored in this tree. This implementation uses the
    /// {@code TreeNodeIterator} to iterator over the values.
    /// </summary>
    private class TreeValuesIterator : IEnumerator<T>
    {
        private readonly AvlTree<T> _outerInstance;

        /// <summary>
        /// Internally used {@code TreeNodeIterator}
        /// </summary>
        internal TreeNodeIterator Iterator;

        /// <summary>
        /// Constructs a new {@code TreeValuesIterator}
        /// </summary>
        public TreeValuesIterator(AvlTree<T> outerInstance)
        {
            _outerInstance = outerInstance;
            Iterator            = new TreeNodeIterator(outerInstance);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool HasNext()
        {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            return Iterator.HasNext();
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override T Next()
        {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            return Iterator.Next().Value;
        }
    }

    /// <summary>
    /// Iterator over the tree nodes. The nodes are returned according to the in order tree
    /// traversal.
    /// </summary>
    private class TreeNodeIterator : IEnumerator<TreeNode<T>>
    {
        private readonly AvlTree<T> _outerInstance;

        /// <summary>
        /// A node that is returned next or {@code null} if all nodes are traversed
        /// </summary>
        internal TreeNode<T> NextNode;

        /// <summary>
        /// Number of modifications of the tree at the time this iterator is created.
        /// </summary>
        internal readonly int ExpectedModCount;

        /// <summary>
        /// Constructs a new {@code TreeNodeIterator}
        /// </summary>
        public TreeNodeIterator(AvlTree<T> outerInstance)
        {
            _outerInstance = outerInstance;
            NextNode            = outerInstance.Min;
            ExpectedModCount    = outerInstance._modCount;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool HasNext()
        {
            CheckForComodification();
            return NextNode != null;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override TreeNode<T> Next()
        {
            if (!HasNext())
            {
                throw new NoSuchElementException();
            }

            TreeNode<T> result = NextNode;
            NextNode = outerInstance.Successor(NextNode);
            return result;
        }

        /// <summary>
        /// Checks if the tree has been modified during the iteration process
        /// </summary>
        internal virtual void CheckForComodification()
        {
            if (ExpectedModCount != outerInstance._modCount)
            {
                throw new ConcurrentModificationException();
            }
        }
    }

    /// <summary>
    /// Container holding the values stored in the tree.
    /// </summary>
    /// @param <T> a tree node value type.</param>
    public class TreeNode<T>
    {
        /// <summary>
        /// A value stored in this tree node
        /// </summary>
        internal T Value;

        /// <summary>
        /// Parent of this node
        /// </summary>
        internal TreeNode<T> Parent;

        /// <summary>
        /// Left child of this node
        /// </summary>
        internal TreeNode<T> Left;

        /// <summary>
        /// Right child of this node
        /// </summary>
        internal TreeNode<T> Right;

        /// <summary>
        /// Next node in the tree according to the in order traversal
        /// </summary>
        internal TreeNode<T> Successor;

        /// <summary>
        /// Previous node in the tree according to the in order traversal
        /// </summary>
        internal TreeNode<T> Predecessor;

        /// <summary>
        /// A minimum node in the subtree rooted at this node
        /// </summary>
        internal TreeNode<T> SubtreeMin;

        /// <summary>
        /// A maximum node in the subtree rooted at this node
        /// </summary>
        internal TreeNode<T> SubtreeMax;

        /// <summary>
        /// Height of the node
        /// </summary>
        internal int Height;

        /// <summary>
        /// Size of the subtree rooted at this node
        /// </summary>
        internal int SubtreeSize;

        /// <summary>
        /// Constructs a new node with the {@code value} stored in it
        /// </summary>
        /// <param name="value"> a value to store in this node.</param>
        internal TreeNode(T value)
        {
            this.value = value;
            Reset();
        }

        /// <summary>
        /// Returns a value stored in this node
        /// </summary>
        /// <returns>a value stored in this node.</returns>
        public virtual T Value
        {
            get
            {
                return value;
            }
        }

        /// <summary>
        /// Returns a root of the tree this node is stored in
        /// </summary>
        /// <returns>a root of the tree this node is stored in.</returns>
        public virtual TreeNode<T> Root
        {
            get
            {
                TreeNode<T> current = this;
                while (current.parent != null)
                {
                    current = current.parent;
                }

                return current.left;
            }
        }

        /// <summary>
        /// Returns a minimum node stored in the subtree rooted at this node
        /// </summary>
        /// <returns>a minimum node stored in the subtree rooted at this node.</returns>
        public virtual TreeNode<T> SubtreeMin
        {
            get
            {
                return subtreeMin;
            }
        }

        /// <summary>
        /// Returns a maximum node stored in the subtree rooted at this node
        /// </summary>
        /// <returns>a maximum node stored in the subtree rooted at this node.</returns>
        public virtual TreeNode<T> SubtreeMax
        {
            get
            {
                return subtreeMax;
            }
        }

        /// <summary>
        /// Returns a minimum node stored in the tree
        /// </summary>
        /// <returns>a minimum node stored in the tree.</returns>
        public virtual TreeNode<T> TreeMin
        {
            get
            {
                return Root.SubtreeMin;
            }
        }

        /// <summary>
        /// Returns a maximum node stored in the tree
        /// </summary>
        /// <returns>a maximum node stored in the tree.</returns>
        public virtual TreeNode<T> TreeMax
        {
            get
            {
                return Root.SubtreeMax;
            }
        }

        /// <summary>
        /// Returns a parent of this node
        /// </summary>
        /// <returns>a parent of this node.</returns>
        public virtual TreeNode<T> Parent
        {
            get
            {
                return parent;
            }
        }

        /// <summary>
        /// Returns a left child of this node
        /// </summary>
        /// <returns>a left child of this node.</returns>
        public virtual TreeNode<T> Left
        {
            get
            {
                return left;
            }
        }

        /// <summary>
        /// Returns a right child of this node
        /// </summary>
        /// <returns>a right child of this node.</returns>
        public virtual TreeNode<T> Right
        {
            get
            {
                return right;
            }
        }

        /// <summary>
        /// Returns a height of this node
        /// </summary>
        /// <returns>a height of this node.</returns>
        internal virtual int Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>
        /// Returns a subtree size of the tree rooted at this node
        /// </summary>
        /// <returns>a subtree size of the tree rooted at this node.</returns>
        internal virtual int SubtreeSize
        {
            get
            {
                return subtreeSize;
            }
        }

        /// <summary>
        /// Resets this node to the default state
        /// </summary>
        internal virtual void Reset()
        {
            this.height      = 1;
            this.subtreeSize = 1;
            this.subtreeMin  = this;
            this.subtreeMax  = this;
            this.left        = this.right = this.parent = this.predecessor = this.successor = null;
        }

        /// <summary>
        /// Returns a height of the right subtree
        /// </summary>
        /// <returns>a height of the right subtree.</returns>
        internal virtual int RightHeight
        {
            get
            {
                return right == null ? 0 : right.height;
            }
        }

        /// <summary>
        /// Returns a height of the left subtree
        /// </summary>
        /// <returns>a height of the right subtree.</returns>
        internal virtual int LeftHeight
        {
            get
            {
                return left == null ? 0 : left.height;
            }
        }

        /// <summary>
        /// Returns a size of the left subtree
        /// </summary>
        /// <returns>a size of the left subtree.</returns>
        internal virtual int LeftSubtreeSize
        {
            get
            {
                return left == null ? 0 : left.subtreeSize;
            }
        }

        /// <summary>
        /// Returns a size of the right subtree
        /// </summary>
        /// <returns>a size of the right subtree.</returns>
        internal virtual int RightSubtreeSize
        {
            get
            {
                return right == null ? 0 : right.subtreeSize;
            }
        }

        /// <summary>
        /// Updates the height and subtree size of this node according to the values of the left and
        /// right children
        /// </summary>
        internal virtual void UpdateHeightAndSubtreeSize()
        {
            height      = Math.Max(LeftHeight, RightHeight) + 1;
            subtreeSize = LeftSubtreeSize                   + RightSubtreeSize + 1;
        }

        /// <summary>
        /// Returns {@code true} if this node is unbalanced and the left child's height is greater,
        /// {@code false otherwise}
        /// </summary>
        /// <returns>{@code true} if this node is unbalanced and the left child's height is greater,
        ///         {@code false otherwise}</returns>
        internal virtual bool LeftDoubleHeavy
        {
            get
            {
                return LeftHeight > RightHeight + 1;
            }
        }

        /// <summary>
        /// Returns {@code true} if this node is unbalanced and the right child's height is greater,
        /// {@code false otherwise}
        /// </summary>
        /// <returns>{@code true} if this node is unbalanced and the right child's height is greater,
        ///         {@code false otherwise}</returns>
        internal virtual bool RightDoubleHeavy
        {
            get
            {
                return RightHeight > LeftHeight + 1;
            }
        }

        /// <summary>
        /// Returns {@code true} if the height of the left child is greater than the height of the
        /// right child
        /// </summary>
        /// <returns>{@code true} if the height of the left child is greater than the height of the
        ///         right child.</returns>
        internal virtual bool LeftHeavy
        {
            get
            {
                return LeftHeight > RightHeight;
            }
        }

        /// <summary>
        /// Returns {@code true} if the height of the right child is greater than the height of the
        /// left child
        /// </summary>
        /// <returns>{@code true} if the height of the right child is greater than the height of the
        ///         left child.</returns>
        internal virtual bool RightHeavy
        {
            get
            {
                return RightHeight > LeftHeight;
            }
        }

        /// <summary>
        /// Returns {@code true} if this node is a left child of its parent, {@code false} otherwise
        /// </summary>
        /// <returns>{@code true} if this node is a left child of its parent, {@code false} otherwise.</returns>
        internal virtual bool LeftChild
        {
            get
            {
                return this == parent.left;
            }
        }

        /// <summary>
        /// Returns {@code true} if this node is a right child of its parent, {@code false} otherwise
        /// </summary>
        /// <returns>{@code true} if this node is a right child of its parent, {@code false} otherwise.</returns>
        internal virtual bool RightChild
        {
            get
            {
                return this == parent.right;
            }
        }

        /// <summary>
        /// Returns a successor of this node according to the tree in order traversal, or
        /// {@code null} if this node is a maximum node in the tree
        /// </summary>
        /// <returns>successor of this node, or {@code} null if this node in a maximum node in the
        ///         tree.</returns>
        public virtual TreeNode<T> Successor
        {
            get
            {
                return successor;
            }
            set
            {
                successor = value;
                if (value != null)
                {
                    value.predecessor = this;
                }
            }
        }

        /// <summary>
        /// Returns a predecessor of this node according to the tree in order traversal, or
        /// {@code null} if this node is a minimum node in the tree
        /// </summary>
        /// <returns>predecessor of this node, or {@code} null if this node in a minimum node in the
        ///         tree.</returns>
        public virtual TreeNode<T> Predecessor
        {
            get
            {
                return predecessor;
            }
            set
            {
                predecessor = value;
                if (value != null)
                {
                    value.successor = this;
                }
            }
        }


        /// <summary>
        /// Sets the left child reference of this node to {@code node}. If the {@code node} is not
        /// {@code null}, updates its parent reference as well.
        /// </summary>
        /// <param name="node"> a new left child.</param>
        internal virtual void SetLeftChild(TreeNode<T> node)
        {
            left = node;
            if (node != null)
            {
                node.parent = this;
                Predecessor = node.subtreeMax;
                subtreeMin  = node.subtreeMin;
            }
            else
            {
                subtreeMin  = this;
                predecessor = null;
            }
        }

        /// <summary>
        /// Sets the right child reference of this node to {@code node}. If the {@code node} is not
        /// {@code null}, updates its parent reference as well.
        /// </summary>
        /// <param name="node"> a new right child.</param>
        internal virtual void SetRightChild(TreeNode<T> node)
        {
            right = node;
            if (node != null)
            {
                node.parent = this;
                Successor   = node.subtreeMin;
                subtreeMax  = node.subtreeMax;
            }
            else
            {
                successor  = null;
                subtreeMax = this;
            }
        }

        /// <summary>
        /// Substitutes the {@code prevChild} with the {@code newChild}. If the {@code newChild} is
        /// not {@code null}, updates its parent reference as well
        /// </summary>
        /// <param name="prevChild"> either left or right child of this node.</param>
        /// <param name="newChild"> a new child of this node.</param>
        internal virtual void SubstituteChild(TreeNode<T> prevChild, TreeNode<T> newChild)
        {
            Debug.Assert(left == prevChild || right == prevChild);
            Debug.Assert(!(left == prevChild && right == prevChild));
            if (left == prevChild)
            {
                SetLeftChild(newChild);
            }
            else
            {
                SetRightChild(newChild);
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override string ToString()
        {
            return string.Format(
                "{{{0}}}: [parent = {1}, left = {2}, right = {3}], [subtreeMin = {4}, subtreeMax = {5}], [predecessor = {6}, successor = {7}], [height = {8:D}, subtreeSize = {9:D}]",
                value,
                parent      == null ? "null" : parent.value,
                left        == null ? "null" : left.value,
                right       == null ? "null" : right.value,
                subtreeMin  == null ? "null" : subtreeMin.value,
                subtreeMax  == null ? "null" : subtreeMax.value,
                predecessor == null ? "null" : predecessor.value,
                successor   == null ? "null" : successor.value,
                height,
                subtreeSize
            );
        }
    }
}
