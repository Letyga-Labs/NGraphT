// (C) Copyright 2003-2023, by Barak Naveh and Contributors.
//
// NGraphT : a free .NET graph-theory library.
// It is a third-party port of the JGraphT library and it
// strictly inherits all legal conditions of its origin:
// licenses, authorship rights, restrictions and permissions.
//
// See the CONTRIBUTORS.md file distributed with this work for additional
// information regarding copyright ownership.
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// http://www.eclipse.org/legal/epl-2.0, or the
// GNU Lesser General Public License v2.1 or later
// which is available at
// http://www.gnu.org/licenses/old-licenses/lgpl-2.1-standalone.html.
//
// SPDX-License-Identifier: EPL-2.0 OR LGPL-2.1-or-later

using System.Collections;
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
///
/// <para>
/// AVL tree is a self-balancing binary tree data structure. In an AVL tree, the heights of two child
/// subtrees differ by at most one. This ensures that the height of the tree is $\mathcal{O}(\log n)$
/// where $n$ is the number of elements in the tree. Also this tree doesn't support key comparisons,
/// it does define an element order. As a result, this tree can be used to query vertex
/// successor/predecessor.
/// </para>
///
/// <para>
/// Subtree query means that the result is being computed only on the subtree nodes. This tree
/// supports the following operations:
/// <list type="bullet">
///     <item>Min/max insertion and deletion in $\mathcal{O}(\log n)$ time</item>
///     <item>Subtree min/max queries in $\mathcal{O}(1)$ time</item>
///     <item>Node successor/predecessor queries in $\mathcal{O}(1)$ time</item>
///     <item>Tree split in $\mathcal{O}(\log n)$ time</item>
///     <item>Tree merge in $\mathcal{O}(\log n)$ time</item>
/// </list>
/// </para>
///
/// <para>
/// This implementation gives users access to the tree nodes which hold the inserted elements. The
/// user is able to store the tree nodes references but isn't able to modify them.
/// </para>
/// </summary>
///
/// <typeparam name="TElement">the key data type.</typeparam>
///
/// <remarks>Author: Timofey Chudakov.</remarks>
public sealed class AvlTree<TElement> : IEnumerable<TElement>
{
    /// <summary>
    /// An auxiliary vertex which's always present in a tree and doesn't contain any data.
    /// </summary>
    private readonly AvlTreeNode<TElement> _virtualRoot = new(default!);

    /// <summary>
    /// Modification tracker.
    /// </summary>
    private int _modCount;

    /// <summary>
    /// Constructs an empty tree.
    /// </summary>
    public AvlTree()
    {
    }

    /// <summary>
    /// Constructor for internal usage.
    /// </summary>
    /// <param name="root"> the root of the newly create tree.</param>
    private AvlTree(AvlTreeNode<TElement> root)
    {
        MakeRoot(root);
    }

    /// <summary>
    /// Returns the root of this tree or null if this tree is empty.
    /// </summary>
    /// <returns>the root of this tree or null if this tree is empty.</returns>
    public AvlTreeNode<TElement>? Root => _virtualRoot.Left;

    /// <summary>
    /// Returns the minimum node in this tree or null if the tree is empty.
    /// </summary>
    /// <returns>the minimum node in this tree or null if the tree is empty.</returns>
    public AvlTreeNode<TElement>? Min => Root?.SubtreeMin;

    /// <summary>
    /// Returns the maximum node in this tree or null if the tree is empty.
    /// </summary>
    /// <returns>the maximum node in this tree or null if the tree is empty.</returns>
    public AvlTreeNode<TElement>? Max => Root?.SubtreeMax;

    /// <summary>
    /// Check if this tree is empty.
    /// </summary>
    /// <returns><c>true</c> if this tree is empty, <c>false</c> otherwise.</returns>
    public bool Empty => Root == null;

    /// <summary>
    /// Returns the size of this tree.
    /// </summary>
    /// <returns>the size of this tree.</returns>
    public int Size => _virtualRoot.Left?.SubtreeSize ?? 0;

    /// <summary>
    /// Adds <c>value</c> as a maximum element to this tree. The running time of this method is
    /// $\mathcal{O}(\log n)$.
    /// </summary>
    /// <param name="value"> a value to add as a tree max.</param>
    /// <returns>a tree node holding the <c>value</c>.</returns>
    public AvlTreeNode<TElement> AddMax(TElement value)
    {
        var newMax = new AvlTreeNode<TElement>(value);
        AddMaxNode(newMax);
        return newMax;
    }

    /// <summary>
    /// Adds the <c>newMax</c> as a maximum node to this tree.
    /// </summary>
    /// <param name="newMax"> a node to add as a tree max.</param>
    public void AddMaxNode(AvlTreeNode<TElement>? newMax)
    {
        ArgumentNullException.ThrowIfNull(newMax);

        RegisterModification();
        if (Empty)
        {
            _virtualRoot.Left = newMax;
            newMax.Parent     = _virtualRoot;
        }
        else
        {
            var max = Max;
            max!.SetRightChild(newMax);
            Balance(max);
        }
    }

    /// <summary>
    /// Adds the <c>value</c> as a minimum element to this tree.
    /// </summary>
    /// <param name="value"> a value to add as a tree min.</param>
    /// <returns>a tree node holding the <c>value</c>.</returns>
    public AvlTreeNode<TElement> AddMin(TElement value)
    {
        var newMin = new AvlTreeNode<TElement>(value);
        AddMinNode(newMin);
        return newMin;
    }

    /// <summary>
    /// Adds the <c>newMin</c> as a minimum node to this tree.
    /// </summary>
    /// <param name="newMin"> a node to add as a tree min.</param>
    public void AddMinNode(AvlTreeNode<TElement>? newMin)
    {
        ArgumentNullException.ThrowIfNull(newMin);

        RegisterModification();
        if (Empty)
        {
            _virtualRoot.Left = newMin;
            newMin.Parent     = _virtualRoot;
        }
        else
        {
            var min = Min;
            min!.SetLeftChild(newMin);
            Balance(min);
        }
    }

    /// <summary>
    /// Splits the tree into two parts.
    /// <para>
    /// The first part contains the nodes which are smaller than or equal to the <c>node</c>. The
    /// first part stays in this tree. The second part contains the nodes which are strictly greater
    /// than the <c>node</c>. The second part is returned as a tree.
    /// </para>
    /// </summary>
    ///
    /// <param name="node"> a separating node.</param>
    ///
    /// <returns>a tree containing the nodes which are strictly greater than the <c>node</c>.</returns>
    public AvlTree<TElement> SplitAfter(AvlTreeNode<TElement> node)
    {
        ArgumentNullException.ThrowIfNull(node);
        RegisterModification();

        var parent   = node.Parent!;
        var nextMove = node.IsLeftChild;
        var left     = node.Left;
        var right    = node.Right;

        parent.SubstituteChild(node, null);

        node.Reset();

        if (left != null)
        {
            left.Parent = null;
        }

        if (right != null)
        {
            right.Parent = null;
        }

        if (left == null)
        {
            left = node;
        }
        else
        {
            // insert node as a left subtree max
            var t = left;
            while (t.Right != null)
            {
                t = t.Right;
            }

            t.SetRightChild(node);

            while (t != left)
            {
                var p = t.Parent;
                p!.SubstituteChild(t, BalanceNode(t));
                t = p;
            }

            left = BalanceNode(left);
        }

        return Split(left, right, parent, nextMove);
    }

    /// <summary>
    /// Splits the tree into two parts.
    /// <para>
    /// The first part contains the nodes which are smaller than the <c>node</c>. The first part
    /// stays in this tree. The second part contains the nodes which are greater than or equal to the
    /// <c>node</c>. The second part is returned as a tree.
    /// </para>
    /// </summary>
    ///
    /// <param name="node"> a separating node.</param>
    /// <returns>a tree containing the nodes which are greater than or equal to the <c>node</c>.</returns>
    public AvlTree<TElement> SplitBefore(AvlTreeNode<TElement> node)
    {
        RegisterModification();

        var predecessor = Predecessor(node);
        if (predecessor == null)
        {
            // node is a minimum node
            var tree = new AvlTree<TElement>();
            Swap(tree);
            return tree;
        }

        return SplitAfter(predecessor);
    }

    /// <summary>
    /// Append the nodes in the <c>tree</c> after the nodes in this tree.
    /// <para>
    /// The result of this operation is stored in this tree.
    /// </para>
    /// </summary>
    /// <param name="tree"> a tree to append.</param>
    public void MergeAfter(AvlTree<TElement> tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
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

        var junctionNode = tree.RemoveMin()!;
        var treeRoot     = tree.Root;
        tree.Clear();

        MakeRoot(Merge(junctionNode, Root, treeRoot));
    }

    /// <summary>
    /// Prepends the nodes in the <c>tree</c> before the nodes in this tree.
    /// <para>
    /// The result of this operation is stored in this tree.
    /// </para>
    ///
    /// </summary>
    /// <param name="tree"> a tree to prepend.</param>
    public void MergeBefore(AvlTree<TElement> tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
        RegisterModification();
        tree.MergeAfter(this);
        Swap(tree);
    }

    /// <summary>
    /// Removes the minimum node in this tree. Returns <c>null</c> if this tree is empty.
    /// </summary>
    /// <returns>the removed node or <c>null</c> if this tree is empty.</returns>
    public AvlTreeNode<TElement>? RemoveMin()
    {
        RegisterModification();

        if (Empty)
        {
            return null;
        }

        var min = Min!;
        // min.parent != null
        if (min.Parent == _virtualRoot)
        {
            MakeRoot(min.Right);
        }
        else
        {
            min.Parent!.SetLeftChild(min.Right);
        }

        Balance(min.Parent);

        return min;
    }

    /// <summary>
    /// Removes the maximum node in this tree. Returns <c>null</c> if this tree is empty.
    /// </summary>
    /// <returns>the removed node or <c>null</c> if this tree is empty.</returns>
    public AvlTreeNode<TElement>? RemoveMax()
    {
        RegisterModification();
        if (Empty)
        {
            return null;
        }

        var max = Max!;
        if (max.Parent == _virtualRoot)
        {
            MakeRoot(max.Left);
        }
        else
        {
            max.Parent!.SetRightChild(max.Left);
        }

        Balance(max.Parent);
        return max;
    }

    /// <summary>
    /// Returns the node following the <c>node</c> in the order defined by this tree. Returns null
    /// if the <c>node</c> is the maximum node in the tree.
    /// </summary>
    /// <param name="node"> a node to compute successor of.</param>
    /// <returns>the successor of the <c>node</c>.</returns>
    public AvlTreeNode<TElement>? Successor(AvlTreeNode<TElement> node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.Successor;
    }

    /// <summary>
    /// Returns the node, which is before the <c>node</c> in the order defined by this tree. Returns
    /// null if the <c>node</c> is the minimum node in the tree.
    /// </summary>
    /// <param name="node"> a node to compute predecessor of.</param>
    /// <returns>the predecessor of the <c>node</c>.</returns>
    public AvlTreeNode<TElement>? Predecessor(AvlTreeNode<TElement> node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.Predecessor;
    }

    /// <summary>
    /// Removes all nodes from this tree.
    /// <para>
    /// <b>Note:</b> the memory allocated for the tree structure won't be deallocated until there are
    /// active external referenced to the nodes of this tree.
    /// </para>
    /// </summary>
    public void Clear()
    {
        RegisterModification();
        _virtualRoot.Left = null;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var       builder      = new StringBuilder();
        using var nodeIterator = NodeIterator();
        for (var i = nodeIterator; i.MoveNext();)
        {
            var node = i.Current;
            builder.Append(node).Append('\n');
        }

        return builder.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<TElement> GetEnumerator()
    {
        return new TreeValuesIterator(this);
    }

    /// <summary>
    /// Returns an iterator over the tree nodes rather than the node values. The tree are returned in
    /// the same order as the tree values.
    /// </summary>
    /// <returns>an iterator over the tree nodes.</returns>
    public IEnumerator<AvlTreeNode<TElement>> NodeIterator()
    {
        return new TreeNodeIterator(this);
    }

    /// <summary>
    /// Makes the <c>node</c> the root of this tree.
    /// </summary>
    /// <param name="node"> a new root of this tree.</param>
    private void MakeRoot(AvlTreeNode<TElement>? node)
    {
        _virtualRoot.Left = node;
        if (node != null)
        {
            node.SubtreeMax!.Successor   = null;
            node.SubtreeMin!.Predecessor = null;
            node.Parent                  = _virtualRoot;
        }
    }

    /// <summary>
    /// Traverses the tree up until the  root and splits it into two parts.
    /// <para>
    /// The algorithm is described in <i>Donald E. Knuth. The art of computer programming. Second
    /// Edition. Volume 3 / Sorting and Searching, p. 474</i>.
    /// </para>
    /// </summary>
    ///
    /// <param name="left"> a left subtree.</param>
    /// <param name="right"> a right subtree.</param>
    /// <param name="p"> next parent node.</param>
    /// <param name="leftMove"> <c>true</c> if we're moving from the left child, <c>false</c> otherwise.</param>
    ///
    /// <returns>the resulting right tree.</returns>
    private AvlTree<TElement> Split(
        AvlTreeNode<TElement>? left,
        AvlTreeNode<TElement>? right,
        AvlTreeNode<TElement>  p,
        bool                   leftMove)
    {
        while (p != _virtualRoot)
        {
            var nextMove = p.IsLeftChild;
            var nextP    = p.Parent;

            p.Parent!.SubstituteChild(p, null);
            p.Parent = null;

            if (leftMove)
            {
                right = Merge(p, right, p.Right);
            }
            else
            {
                left = Merge(p, p.Left, left);
            }

            p        = nextP!;
            leftMove = nextMove;
        }

        MakeRoot(left);

        return new AvlTree<TElement>(right!);
    }

    /// <summary>
    /// Merges the <c>left</c> and <c>right</c> subtrees using the <c>junctionNode</c>.
    /// <para>
    /// The algorithm is described in <i>Donald TEdge. Knuth. The art of computer programming. Second
    /// Edition. Volume 3 / Sorting and Searching, p. 474</i>.
    /// </para>
    /// </summary>
    ///
    /// <param name="junctionNode"> a node between left and right subtrees.</param>
    /// <param name="left"> a left subtree.</param>
    /// <param name="right"> a right subtree.</param>
    ///
    /// <returns>the root of the resulting tree.</returns>
    private AvlTreeNode<TElement> Merge(
        AvlTreeNode<TElement>  junctionNode,
        AvlTreeNode<TElement>? left,
        AvlTreeNode<TElement>? right)
    {
        if (left == null && right == null)
        {
            junctionNode.Reset();
            return junctionNode;
        }
        else if (left == null)
        {
            right!.SetLeftChild(Merge(junctionNode, left, right.Left));
            return BalanceNode(right);
        }
        else if (right == null)
        {
            left.SetRightChild(Merge(junctionNode, left.Right, right));
            return BalanceNode(left);
        }
        else if (left.Height > right.Height + 1)
        {
            left!.SetRightChild(Merge(junctionNode, left.Right, right));
            return BalanceNode(left);
        }
        else if (right.Height > left.Height + 1)
        {
            right.SetLeftChild(Merge(junctionNode, left, right.Left));
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
    /// Swaps the contents of this tree and the <c>tree</c>.
    /// </summary>
    /// <param name="tree"> a tree to swap content of.</param>
    private void Swap(AvlTree<TElement> tree)
    {
        var t = _virtualRoot.Left;
        MakeRoot(tree._virtualRoot.Left);
        tree.MakeRoot(t);
    }

    /// <summary>
    /// Performs a right node rotation.
    /// </summary>
    /// <param name="node"> a node to rotate.</param>
    /// <returns>a new parent of the <c>node</c>.</returns>
    private AvlTreeNode<TElement> RotateRight(AvlTreeNode<TElement> node)
    {
        var left = node.Left;
        left!.Parent = null;

        node.SetLeftChild(left.Right);
        left.SetRightChild(node);

        node.UpdateHeightAndSubtreeSize();
        left.UpdateHeightAndSubtreeSize();

        return left;
    }

    /// <summary>
    /// Performs a left node rotation.
    /// </summary>
    /// <param name="node"> a node to rotate.</param>
    /// <returns>a new parent of the <c>node</c>.</returns>
    private AvlTreeNode<TElement> RotateLeft(AvlTreeNode<TElement> node)
    {
        var right = node.Right;
        right!.Parent = null;

        node.SetRightChild(right.Left);

        right.SetLeftChild(node);

        node.UpdateHeightAndSubtreeSize();
        right.UpdateHeightAndSubtreeSize();

        return right;
    }

    /// <summary>
    /// Performs a node balancing on the path from <c>node</c> up until the root.
    /// </summary>
    /// <param name="node"> a node to start tree balancing from.</param>
    private void Balance(AvlTreeNode<TElement> node)
    {
        Balance(node, _virtualRoot);
    }

    /// <summary>
    /// Performs a node balancing on the path from <c>node</c> up until the <c>stop</c> node.
    /// </summary>
    /// <param name="node"> a node to start tree balancing from.</param>
    /// <param name="stop"> a node to stop balancing at (this node is not being balanced).</param>
    private void Balance(AvlTreeNode<TElement> node, AvlTreeNode<TElement> stop)
    {
        if (node == stop)
        {
            return;
        }

        var p = node.Parent;
        if (p == _virtualRoot)
        {
            MakeRoot(BalanceNode(node));
        }
        else
        {
            p!.SubstituteChild(node, BalanceNode(node));
        }

        Balance(p, stop);
    }

    /// <summary>
    /// Checks whether the <c>node</c> is unbalanced. If so, balances the <c>node</c>.
    /// </summary>
    /// <param name="node"> a node to balance.</param>
    /// <returns>a new parent of <c>node</c> if the balancing occurs, <c>node</c> otherwise.</returns>
    private AvlTreeNode<TElement> BalanceNode(AvlTreeNode<TElement> node)
    {
        node.UpdateHeightAndSubtreeSize();
        if (node.IsLeftDoubleHeavy)
        {
            if (node.Left!.IsRightHeavy)
            {
                node.SetLeftChild(RotateLeft(node.Left));
            }

            RotateRight(node);
            return node.Parent!;
        }
        else if (node.IsRightDoubleHeavy)
        {
            if (node.Right!.IsLeftHeavy)
            {
                node.SetRightChild(RotateRight(node.Right));
            }

            RotateLeft(node);
            return node.Parent!;
        }

        return node;
    }

    /// <summary>
    /// Registers a modifying operation.
    /// </summary>
    private void RegisterModification()
    {
        ++_modCount;
    }

    /// <summary>
    /// Iterator over the values stored in this tree. This implementation uses the
    /// <c>TreeNodeIterator</c> to iterator over the values.
    /// </summary>
    private sealed class TreeValuesIterator : IEnumerator<TElement>
    {
        /// <summary>
        /// Internally used <c>TreeNodeIterator</c>.
        /// </summary>
        private readonly TreeNodeIterator _nodeIterator;

        /// <summary>
        /// Constructs a new <c>TreeValuesIterator</c>.
        /// </summary>
        public TreeValuesIterator(AvlTree<TElement> tree)
        {
            _nodeIterator = new TreeNodeIterator(tree);
        }

        object? IEnumerator.Current => Current;

        public TElement Current => _nodeIterator.Current.Value;

        public bool MoveNext()
        {
            return _nodeIterator.MoveNext();
        }

        public void Reset()
        {
            _nodeIterator.Reset();
        }

        public void Dispose()
        {
            _nodeIterator.Dispose();
        }
    }

    /// <summary>
    /// Iterator over the tree nodes. The nodes are returned according to the in order tree
    /// traversal.
    /// </summary>
    private sealed class TreeNodeIterator : IEnumerator<AvlTreeNode<TElement>>
    {
        private readonly AvlTree<TElement> _tree;

        /// <summary>
        /// Number of modifications of the tree at the time this iterator is created.
        /// </summary>
        private readonly int _expectedModCount;

        /// <summary>
        /// A node that is returned next or <c>null</c> if all nodes are traversed.
        /// </summary>
        private AvlTreeNode<TElement>? _nextNode;

        private bool _firstInit;

        /// <summary>
        /// Constructs a new <c>TreeNodeIterator</c>.
        /// </summary>
        public TreeNodeIterator(AvlTree<TElement> tree)
        {
            _tree             = tree;
            _expectedModCount = tree._modCount;
        }

        object IEnumerator.Current => Current;

        public AvlTreeNode<TElement> Current => _nextNode ?? throw new NoSuchElementException();

        public bool MoveNext()
        {
            if (_firstInit)
            {
                _nextNode  = _tree.Min;
                _firstInit = false;
            }

            CheckForComodification();
            if (_nextNode == null)
            {
                return false;
            }

            _nextNode = _tree.Successor(_nextNode);
            return true;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            // do nothing
        }

        /// <summary>
        /// Checks if the tree has been modified during the iteration process.
        /// </summary>
        private void CheckForComodification()
        {
            if (_expectedModCount != _tree._modCount)
            {
                throw new InvalidOperationException("Concurrent modification detected");
            }
        }
    }
}
