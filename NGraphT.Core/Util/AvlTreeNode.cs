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

using System.Diagnostics;

namespace NGraphT.Core.Util;

/// <summary>
/// Container holding the values stored in the tree.
/// </summary>
/// <typeparam name="TElement"> a tree node value type.</typeparam>
public sealed class AvlTreeNode<TElement>
{
    /// <summary>
    /// Next node in the tree according to the in order traversal.
    /// </summary>
    private AvlTreeNode<TElement>? _successor;

    /// <summary>
    /// Previous node in the tree according to the in order traversal.
    /// </summary>
    private AvlTreeNode<TElement>? _predecessor;

    /// <summary>
    /// Size of the subtree rooted at this node.
    /// </summary>
    private int _subtreeSize;

    /// <summary>
    /// Constructs a new node with the <c>value</c> stored in it.
    /// </summary>
    /// <param name="value"> a value to store in this node.</param>
    internal AvlTreeNode(TElement value)
    {
        Value = value;
        Reset();
    }

    /// <summary>
    /// Returns a value stored in this node.
    /// </summary>
    /// <returns>a value stored in this node.</returns>
    public TElement Value { get; }

    /// <summary>
    /// Returns a root of the tree this node is stored in.
    /// </summary>
    /// <returns>a root of the tree this node is stored in.</returns>
    public AvlTreeNode<TElement>? Root
    {
        get
        {
            var current = this;
            while (current.Parent != null)
            {
                current = current.Parent;
            }

            return current.Left;
        }
    }

    /// <summary>
    /// Returns a minimum node stored in the subtree rooted at this node.
    /// </summary>
    /// <returns>a minimum node stored in the subtree rooted at this node.</returns>
    public AvlTreeNode<TElement>? SubtreeMin { get; internal set; }

    /// <summary>
    /// Returns a maximum node stored in the subtree rooted at this node.
    /// </summary>
    /// <returns>a maximum node stored in the subtree rooted at this node.</returns>
    public AvlTreeNode<TElement>? SubtreeMax { get; internal set; }

    /// <summary>
    /// Returns a minimum node stored in the tree.
    /// </summary>
    /// <returns>a minimum node stored in the tree.</returns>
    public AvlTreeNode<TElement>? TreeMin => Root?.SubtreeMin;

    /// <summary>
    /// Returns a maximum node stored in the tree.
    /// </summary>
    /// <returns>a maximum node stored in the tree.</returns>
    public AvlTreeNode<TElement>? TreeMax => Root?.SubtreeMax;

    /// <summary>
    /// Returns a parent of this node.
    /// </summary>
    /// <returns>a parent of this node.</returns>
    public AvlTreeNode<TElement>? Parent { get; internal set; }

    /// <summary>
    /// Returns a left child of this node.
    /// </summary>
    /// <returns>a left child of this node.</returns>
    public AvlTreeNode<TElement>? Left { get; internal set; }

    /// <summary>
    /// Returns a right child of this node.
    /// </summary>
    /// <returns>a right child of this node.</returns>
    public AvlTreeNode<TElement>? Right { get; internal set; }

    /// <summary>
    /// Returns a successor of this node according to the tree in order traversal, or
    /// <c>null</c> if this node is a maximum node in the tree.
    /// </summary>
    /// <returns>successor of this node, or <c>null</c> if this node in a maximum node in the
    ///         tree.</returns>
    public AvlTreeNode<TElement>? Successor
    {
        get => _successor;
        set
        {
            _successor = value;
            if (value != null)
            {
                value._predecessor = this;
            }
        }
    }

    /// <summary>
    /// Returns a predecessor of this node according to the tree in order traversal, or
    /// <c>null</c> if this node is a minimum node in the tree.
    /// </summary>
    /// <returns>predecessor of this node, or <c>null</c> if this node in a minimum node in the
    ///         tree.</returns>
    public AvlTreeNode<TElement>? Predecessor
    {
        get => _predecessor;
        set
        {
            _predecessor = value;
            if (value != null)
            {
                value._successor = this;
            }
        }
    }

    /// <summary>
    /// Returns a height of this node.
    /// </summary>
    /// <returns>a height of this node.</returns>
    internal int Height { get; set; }

    /// <summary>
    /// Returns a subtree size of the tree rooted at this node.
    /// </summary>
    /// <returns>a subtree size of the tree rooted at this node.</returns>
    internal int SubtreeSize => _subtreeSize;

    /// <summary>
    /// Returns a height of the right subtree.
    /// </summary>
    /// <returns>a height of the right subtree.</returns>
    internal int RightHeight => Right?.Height ?? 0;

    /// <summary>
    /// Returns a height of the left subtree.
    /// </summary>
    /// <returns>a height of the right subtree.</returns>
    internal int LeftHeight => Left?.Height ?? 0;

    /// <summary>
    /// Returns a size of the left subtree.
    /// </summary>
    /// <returns>a size of the left subtree.</returns>
    internal int LeftSubtreeSize => Left?._subtreeSize ?? 0;

    /// <summary>
    /// Returns a size of the right subtree.
    /// </summary>
    /// <returns>a size of the right subtree.</returns>
    internal int RightSubtreeSize => Right?._subtreeSize ?? 0;

    /// <summary>
    /// Returns <c>true</c> if this node is unbalanced and the left child's height is greater,
    /// <c>false</c> otherwise.
    /// </summary>
    /// <returns><c>true</c> if this node is unbalanced and the left child's height is greater,
    ///         <c>false</c> otherwise.</returns>
    internal bool IsLeftDoubleHeavy => LeftHeight > RightHeight + 1;

    /// <summary>
    /// Returns <c>true</c> if this node is unbalanced and the right child's height is greater,
    /// <c>false</c> otherwise.
    /// </summary>
    /// <returns><c>true</c> if this node is unbalanced and the right child's height is greater,
    ///          <c>false</c> otherwise..</returns>
    internal bool IsRightDoubleHeavy => RightHeight > LeftHeight + 1;

    /// <summary>
    /// Returns <c>true</c> if the height of the left child is greater than the height of the
    /// right child.
    /// </summary>
    /// <returns><c>true</c> if the height of the left child is greater than the height of the
    ///         right child.</returns>
    internal bool IsLeftHeavy => LeftHeight > RightHeight;

    /// <summary>
    /// Returns <c>true</c> if the height of the right child is greater than the height of the
    /// left child.
    /// </summary>
    /// <returns><c>true</c> if the height of the right child is greater than the height of the
    ///         left child.</returns>
    internal bool IsRightHeavy => RightHeight > LeftHeight;

    /// <summary>
    /// Returns <c>true</c> if this node is a left child of its parent, <c>false</c> otherwise.
    /// </summary>
    /// <returns><c>true</c> if this node is a left child of its parent, <c>false</c> otherwise.</returns>
    internal bool IsLeftChild => this == Parent?.Left;

    /// <summary>
    /// Returns <c>true</c> if this node is a right child of its parent, <c>false</c> otherwise.
    /// </summary>
    /// <returns><c>true</c> if this node is a right child of its parent, <c>false</c> otherwise.</returns>
    internal bool IsRightChild => this == Parent?.Right;

    /// <inheritdoc/>
    public override string ToString()
    {
        return
            $"{{{Value}}}: [parent = {(Parent == null ? "null" : Parent.Value)}, left = {(Left == null ? "null" : Left.Value)}, right = {(Right == null ? "null" : Right.Value)}], [subtreeMin = {(SubtreeMin == null ? "null" : SubtreeMin.Value)}, subtreeMax = {(SubtreeMax == null ? "null" : SubtreeMax.Value)}], [predecessor = {(_predecessor == null ? "null" : _predecessor.Value)}, successor = {(_successor == null ? "null" : _successor.Value)}], [height = {Height:D}, subtreeSize = {_subtreeSize:D}]";
    }

    /// <summary>
    /// Sets the left child reference of this node to <c>node</c>. If the <c>node</c> is not
    /// <c>null</c>, updates its parent reference as well.
    /// </summary>
    /// <param name="node"> a new left child.</param>
    internal void SetLeftChild(AvlTreeNode<TElement>? node)
    {
        Left = node;
        if (node != null)
        {
            node.Parent = this;
            Predecessor = node.SubtreeMax;
            SubtreeMin  = node.SubtreeMin;
        }
        else
        {
            SubtreeMin   = this;
            _predecessor = null;
        }
    }

    /// <summary>
    /// Sets the right child reference of this node to <c>node</c>. If the <c>node</c> is not
    /// <c>null</c>, updates its parent reference as well.
    /// </summary>
    /// <param name="node"> a new right child.</param>
    internal void SetRightChild(AvlTreeNode<TElement>? node)
    {
        Right = node;
        if (node != null)
        {
            node.Parent = this;
            Successor   = node.SubtreeMin;
            SubtreeMax  = node.SubtreeMax;
        }
        else
        {
            _successor = null;
            SubtreeMax = this;
        }
    }

    /// <summary>
    /// Substitutes the <c>prevChild</c> with the <c>newChild</c>. If the <c>newChild</c> is
    /// not <c>null</c>, updates its parent reference as well.
    /// </summary>
    /// <param name="prevChild"> either left or right child of this node.</param>
    /// <param name="newChild"> a new child of this node.</param>
    internal void SubstituteChild(AvlTreeNode<TElement> prevChild, AvlTreeNode<TElement>? newChild)
    {
        Debug.Assert(Left == prevChild || Right == prevChild);
        Debug.Assert(!(Left == prevChild && Right == prevChild));
        if (Left == prevChild)
        {
            SetLeftChild(newChild);
        }
        else
        {
            SetRightChild(newChild);
        }
    }

    /// <summary>
    /// Updates the height and subtree size of this node according to the values of the left and
    /// right children.
    /// </summary>
    internal void UpdateHeightAndSubtreeSize()
    {
        Height       = Math.Max(LeftHeight, RightHeight) + 1;
        _subtreeSize = LeftSubtreeSize + RightSubtreeSize + 1;
    }

    /// <summary>
    /// Resets this node to the default state.
    /// </summary>
    internal void Reset()
    {
        Height       = 1;
        _subtreeSize = 1;
        SubtreeMin   = this;
        SubtreeMax   = this;
        Left         = Right = Parent = _predecessor = _successor = null;
    }
}
