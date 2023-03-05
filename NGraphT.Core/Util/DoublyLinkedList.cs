using NGraphT.Core.Util;
using System.Diagnostics;

/*
 * (C) Copyright 2018-2021, by Timofey Chudakov and Contributors.
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
/// {@code DoublyLinkedList} implements a doubly linked <see cref="System.Collections.IList"/> data structure, that exposes its
/// <see cref="IListNode{TNode}"/> where the data is stored in.
/// <para>
/// An element holding {@code ListNode} can be removed or added to a {@code DoublyLinkedList} in
/// constant time O(1). Other methods that operate on {@code ListNodes} directly also have constant
/// runtime. This is also the case for methods that operate on the first(head) and last(tail) node or
/// element. Random access methods have a runtime O(n) that is linearly dependent on the size of the
/// {@code DoublyLinkedList}.
/// </para>
/// <para>
/// A {@code DoublyLinkedList} supports {@code null} elements but does not support
/// {@code null ListNodes}. This class is not thread safe and needs to be synchronized externally if
/// modified by concurrent threads.
/// </para>
/// <para>
/// The iterators over this list have a <i>fail-fast</i> behavior meaning that they throw a
/// <see cref="ConcurrentModificationException"/> after they detect a structural modification of the list,
/// that they're not responsible for.
/// </para>
/// <para>
/// This class is similar to <see cref="System.Collections.Generic.LinkedList"/>. The general difference is that the {@code ListNodes}
/// of this {@code List} are accessible and can be removed or added directly. To ensure the integrity
/// of the {@code List} nodes of this List have a reference to the List they belong to. This
/// increases the memory occupied by this list implementation compared to {@code LinkedList} for the
/// same elements. Instances of {@code LinkedList.Node} have three references each (the element, next
/// and previous), instances of {@code DoublyLinkedList.ListNode} have four (the element, next,
/// previous and the list).
/// </para>
/// </summary>
/// @param <TEdge> the list element type
/// <remarks>Author: Timofey Chudakov.</remarks>
/// <remarks>Author: Hannes Wellmann.</remarks>
public class DoublyLinkedList<TEdge> : AbstractSequentialList<TEdge>, LinkedList<TEdge>
{
    /// <summary>
    /// The first element of the list, {@code null} if this list is empty. </summary>
    private ListNodeImpl<TEdge> _head = null;

    private int _size;

    private ListNodeImpl<TEdge> Tail()
    {
        return _head.prev;
    }

    /// <inheritdoc/>
    public override bool Empty
    {
        get
        {
            return _head == null;
        }
    }

    /// <inheritdoc/>
    public override int Size()
    {
        return _size;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        if (!Empty)
        {
            var node = _head;
            do
            {
                ListNodeImpl<TEdge> next    = node.next;
                var                removed = RemoveListNode(node); // clears all links of removed node
                Debug.Assert(removed);
                node = next;
            } while (node != _head);

            _head = null;
            Debug.Assert(_size == 0);
        }
    }

    // internal modification methods

    /// <summary>
    /// Adds the given <see cref="IListNode{TNode}"/> to this {@code List}.
    /// <para>
    /// Sets the {@code list} reference of {@code node} to this list, increases this lists
    /// {@code size} and {@code modcount} by one.
    /// </para>
    /// </summary>
    /// <param name="node"> the node to add to this list.</param>
    /// <exception cref="ArgumentException"> if {@code node} is already contained in this or another
    ///         {@code DoublyLinkedList} </exception>
    private void AddListNode(ListNodeImpl<TEdge> node)
    {
        // call this before any modification of this list is done
        if (node.List != null)
        {
            var list = (node.List == this) ? "this" : "other";
            throw new ArgumentException("Node <" + node + "> already contained in " + list + " list");
        }

        node.List = this;
        _size++;
        modCount++;
    }

    /// <summary>
    /// Atomically moves all <see cref="IListNode{TNode}"/> from {@code list} to this list as if each
    /// node was removed with <see cref="removeListNode(ListNodeImpl)"/> from {@code list} and
    /// subsequently added to this list by <see cref="addListNode(ListNodeImpl)"/>.
    /// </summary>
    private void MoveAllListNodes(DoublyLinkedList<TEdge> list)
    {
        // call this before any modification of this list is done

        for (ListNodeIteratorImpl it = new DoublyLinkedList.ListNodeIteratorImpl(list, 0); it.MoveNext();)
        {
            var node = it.NextNode();
            Debug.Assert(node.List == list);
            node.List = this;
        }

        _size      += list._size;
        list._size =  0;
        modCount++;
        list.modCount++;
    }

    /// <summary>
    /// Removes the given <see cref="IListNode{TNode}"/> from this {@code List}, if it is contained in this
    /// {@code List}.
    /// <para>
    /// If {@code node} is contained in this list, sets the {@code list}, {@code next} and
    /// {@code prev} reference of {@code node} to {@code null} decreases this list's {@code size} and
    /// increases the {@code modcount} by one.
    /// </para>
    /// </summary>
    /// <param name="node"> the node to remove from this list.</param>
    /// <returns>true if {@code node} was removed from this list, else false.</returns>
    private bool RemoveListNode(ListNodeImpl<TEdge> node)
    {
        // call this before any modification of this list is done
        if (node.List == this)
        {
            node.List = null;
            node.next = null;
            node.prev = null;

            _size--;
            modCount++;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Establishes the links between the given <see cref="ListNodeImpl nodes"/> in such a way that the
    /// {@code predecessor} is linked before the {@code successor}.
    /// </summary>
    /// <param name="predecessor"> the first node linked before the other.</param>
    /// <param name="successor"> the second node linked after the other.</param>
    private void Link(ListNodeImpl<TEdge> predecessor, ListNodeImpl<TEdge> successor)
    {
        predecessor.next = successor;
        successor.prev   = predecessor;
    }

    /// <summary>
    /// Insert non null {@code node} before non null {@code successor} into the list. </summary>
    private void LinkBefore(ListNodeImpl<TEdge> node, ListNodeImpl<TEdge> successor)
    {
        AddListNode(node);
        Link(successor.prev, node);
        Link(node,           successor);
    }

    /// <summary>
    /// Insert non null {@code node} as last node into the list. </summary>
    private void LinkLast(ListNodeImpl<TEdge> node)
    {
        if (Empty)
        {
            // node will be the first and only one
            AddListNode(node);
            Link(node, node); // self link
            _head = node;
        }
        else
        {
            LinkBefore(node, _head);
        }
    }

    /// <summary>
    /// Insert non null {@code list} before node at {@code index} into the list. </summary>
    private void LinkListIntoThisBefore(int index, DoublyLinkedList<TEdge> list)
    {
        var previousSize = _size;
        MoveAllListNodes(list);

        // link list's node into this list
        if (previousSize == 0)
        {
            _head = list._head; // head and tail already linked together
        }
        else
        {
            var refNode = (index == previousSize) ? _head : GetNodeAt(index);

            var listTail = list.Tail();
            Link(refNode.prev, list._head); // changes list.tail()
            Link(listTail,     refNode);

            if (index == 0)
            {
                _head = list._head;
            }
        }

        // clear list but do not call list.clear(), since their nodes are still used
        list._head = null;
    }

    /// <summary>
    /// Remove the non null {@code node} from the list. </summary>
    private bool Unlink(ListNodeImpl<TEdge> node)
    {
        ListNodeImpl<TEdge> prev = node.prev;
        ListNodeImpl<TEdge> next = node.next;
        if (RemoveListNode(node))
        {
            // clears prev and next of node
            if (_size == 0)
            {
                _head = null;
            }
            else
            {
                // list is circular, don't have to worry about null values
                Link(prev, next);

                if (_head == node)
                {
                    _head = next;
                }
            }

            return true;
        }

        return false;
    }

    // ----------------------------------------------------------------------------
    // public modification and access methods

    // ListNode methods:
    // Base methods to access, add and remove nodes to/from this list.
    // Used by all public methods if possible

    /// <summary>
    /// Inserts the specified <see cref="IListNode{TNode}"/> at the specified position in this list.
    /// <para>
    /// This method has a linear runtime complexity O(n) that depends linearly on the distance of the
    /// index to the nearest end. Adding {@code node} as first or last takes only constant time O(1).
    /// </para>
    /// </summary>
    /// <param name="index"> index at which the specified {@code node} is to be inserted.</param>
    /// <param name="node"> the node to add.</param>
    /// <exception cref="IndexOutOfBoundsException"> if the index is out of range
    ///         ({@code index < 0 || index > size()}) </exception>
    /// <exception cref="ArgumentException"> if {@code node} is already part of this or another
    ///         {@code DoublyLinkedList} </exception>
    /// <exception cref="NullReferenceException"> if {@code node} is {@code null} </exception>
    public virtual void AddNode(int index, IListNode<TEdge> node)
    {
        var nodeImpl = (ListNodeImpl<TEdge>)node;
        if (index == _size)
        {
            // also true if this is empty
            LinkLast(nodeImpl);
        }
        else
        {
            var successor = index == 0 ? _head : GetNodeAt(index);
            LinkBefore(nodeImpl, successor);
            if (_head == successor)
            {
                _head = nodeImpl;
            }
        }
    }

    /// <summary>
    /// Inserts the specified <see cref="IListNode{TNode}"/> at the front of this list.
    /// <para>
    /// This method has constant runtime complexity O(1).
    /// </para>
    /// </summary>
    /// <param name="node"> the node to add.</param>
    /// <exception cref="ArgumentException"> if {@code node} is already part of this or another
    ///         {@code DoublyLinkedList} </exception>
    /// <exception cref="NullReferenceException"> if {@code node} is {@code null} </exception>
    public virtual void AddNodeFirst(IListNode<TEdge> node)
    {
        AddNode(0, node);
    }

    /// <summary>
    /// Inserts the specified <see cref="IListNode{TNode}"/> at the end of this list.
    /// <para>
    /// This method has constant runtime complexity O(1).
    /// </para>
    /// </summary>
    /// <param name="node"> the node to add.</param>
    /// <exception cref="ArgumentException"> if {@code node} is already part of this or another
    ///         {@code DoublyLinkedList} </exception>
    /// <exception cref="NullReferenceException"> if {@code node} is {@code null} </exception>
    public virtual void AddNodeLast(IListNode<TEdge> node)
    {
        AddNode(_size, node);
    }

    /// <summary>
    /// Inserts the specified <see cref="IListNode{TNode}"/> before the specified {@code successor} in this
    /// list.
    /// <para>
    /// This method has constant runtime complexity O(1).
    /// </para>
    /// </summary>
    /// <param name="node"> the node to add.</param>
    /// <param name="successor"> {@code ListNode} before which the {@code node} is inserted.</param>
    /// <exception cref="ArgumentException"> if {@code node} is already contained in this or another
    ///         {@code DoublyLinkedList} or {@code successor} is not contained in this list.</exception>
    /// <exception cref="NullReferenceException"> if {@code successor} or {@code node} is {@code null} </exception>
    public virtual void AddNodeBefore(IListNode<TEdge> node, IListNode<TEdge> successor)
    {
        var successorImpl = (ListNodeImpl<TEdge>)successor;
        var nodeImpl      = (ListNodeImpl<TEdge>)node;

        if (successorImpl.List != this)
        {
            throw new ArgumentException("Node <" + successorImpl + "> not in this list");
        }

        LinkBefore(nodeImpl, successorImpl);
        if (_head == successorImpl)
        {
            _head = nodeImpl;
        }
    }

    /// <summary>
    /// Returns the first <see cref="IListNode{TNode}"/> of this list.
    /// <para>
    /// This method has constant runtime complexity O(1).
    /// </para>
    /// </summary>
    /// <returns>the first {@code ListNode} of this list.</returns>
    /// <exception cref="NoSuchElementException"> if this list is empty.</exception>
    public virtual IListNode<TEdge> FirstNode
    {
        get
        {
            if (Empty)
            {
                throw new NoSuchElementException();
            }

            return _head;
        }
    }

    /// <summary>
    /// Returns the last <see cref="IListNode{TNode}"/> of this list.
    /// <para>
    /// This method has constant runtime complexity O(1).
    /// </para>
    /// </summary>
    /// <returns>the last {@code ListNode} of this list.</returns>
    /// <exception cref="NoSuchElementException"> if this list is empty.</exception>
    public virtual IListNode<TEdge> LastNode
    {
        get
        {
            if (Empty)
            {
                throw new NoSuchElementException();
            }

            return Tail();
        }
    }

    /// <summary>
    /// Returns the <see cref="IListNode{TNode}"/> at the specified position in this list.
    /// <para>
    /// This method has linear runtime complexity O(n).
    /// </para>
    /// </summary>
    /// <param name="index"> index of the {@code ListNode} to return.</param>
    /// <returns>the {@code ListNode} at the specified position in this list.</returns>
    /// <exception cref="IndexOutOfBoundsException"> if the index is out of range
    ///         ({@code index < 0 || index >= size()}) </exception>
    public virtual IListNode<TEdge> GetNode(int index)
    {
        return GetNodeAt(index);
    }

    /// <summary>
    /// Returns the <see cref="ListNodeImpl node"/> at the specified position in this list.
    /// </summary>
    /// <param name="index"> index of the {@code ListNodeImpl} to return.</param>
    /// <returns>the {@code ListNode} at the specified position in this list.</returns>
    /// <exception cref="IndexOutOfBoundsException"> if the index is out of range
    ///         ({@code index < 0 || index >= size()}) </exception>
    private ListNodeImpl<TEdge> GetNodeAt(int index)
    {
        if (index < 0 || _size <= index)
        {
            throw new IndexOutOfRangeException("Index: " + index);
        }

        ListNodeImpl<TEdge> node;
        if (index < _size / 2)
        {
            node = _head;
            for (var i = 0; i < index; i++)
            {
                node = node.next;
            }
        }
        else
        {
            node = Tail();
            for (var i = _size - 1; index < i; i--)
            {
                node = node.prev;
            }
        }

        return node;
    }

    /// <summary>
    /// Returns the index of the specified <see cref="IListNode{TNode}"/> in this list, or -1 if this list
    /// does not contain the {@code node}.
    /// <para>
    /// More formally, returns the index {@code i} such that {@code node == getNode(i)}, or -1 if
    /// there is no such index. Because a {@code ListNode} is contained in at most one list exactly
    /// once, the returned index (if not -1) is the only occurrence of that {@code node}.
    /// </para>
    /// <para>
    /// This method has linear runtime complexity O(n) to find {@code node} but returns in constant
    /// time O(1) if {@code node} is not <see cref="containsNode(IListNode{TNode}) contained"/> in this list.
    /// </para>
    /// </summary>
    /// <param name="node"> the node to search for.</param>
    /// <returns>the index of the specified {@code node} in this list, or -1 if this list does not
    ///         contain {@code node}</returns>
    /// <exception cref="NullReferenceException"> if {@code node} is {@code null} </exception>
    public virtual int IndexOfNode(IListNode<TEdge> node)
    {
        if (!ContainsNode(node))
        {
            return -1;
        }

        var current = _head;
        for (var i = 0; i < _size; i++)
        {
            if (current == node)
            {
                return i;
            }

            current = current.next;
        }

        // should never happen:
        throw new InvalidOperationException("Node contained in list not found: " + node);
    }

    /// <summary>
    /// Returns true if this {@code DoublyLinkedList} contains the specified <see cref="IListNode{TNode}"/>.
    /// <para>
    /// This method has constant runtime complexity O(1).
    /// </para>
    /// </summary>
    /// <param name="node"> the node whose presence in this {@code DoublyLinkedList} is to be tested.</param>
    /// <returns>true if this {@code DoublyLinkedList} contains the <see cref="IListNode{TNode}"/></returns>
    /// <exception cref="NullReferenceException"> if {@code node} is {@code null} </exception>
    public virtual bool ContainsNode(IListNode<TEdge> node)
    {
        return ((ListNodeImpl<TEdge>)node).List == this;
    }

    /// <summary>
    /// Removes the <see cref="IListNode{TNode}"/> from this list. Returns true if {@code node} was in this
    /// list and is now removed. If {@code node} is not contained in this list, the list is left
    /// unchanged.
    /// <para>
    /// This method has constant runtime complexity O(1).
    /// </para>
    /// </summary>
    /// <param name="node"> the node to remove from this list.</param>
    /// <returns>true if node was removed from this list.</returns>
    /// <exception cref="NullReferenceException"> if {@code node} is {@code null} </exception>
    public virtual bool RemoveNode(IListNode<TEdge> node)
    {
        return Unlink((ListNodeImpl<TEdge>)node);
    }

    /// <summary>
    /// Returns the first <see cref="IListNode{TNode}"/> holding the specified {@code element} in this list.
    /// More formally, returns the first {@code ListNode} such that
    /// {@code Objects.equals(element, node.getValue())}, or {@code null} if there is no such node.
    /// <para>
    /// This method has linear runtime complexity O(n).
    /// </para>
    /// </summary>
    /// <param name="element"> the element whose {@code ListNode} is to return.</param>
    /// <returns>the first {@code ListNode} holding the {@code element} or null if no node was found.</returns>
    public virtual IListNode<TEdge> NodeOf(object element)
    {
        return SearchNode(() => _head, n => n.next, element).First;
    }

    /// <summary>
    /// Returns the last <see cref="IListNode{TNode}"/> holding the specified {@code element} in this list.
    /// More formally, returns the last {@code ListNode} such that
    /// {@code Objects.equals(element, node.getValue())}, or {@code null} if there is no such node.
    /// <para>
    /// This method has linear runtime complexity O(n).
    /// </para>
    /// </summary>
    /// <param name="element"> the element whose {@code ListNode} is to return.</param>
    /// <returns>the last {@code ListNode} holding the {@code element} or null if no node was found.</returns>
    public virtual IListNode<TEdge> LastNodeOf(object element)
    {
        return SearchNode(this.tail, n => n.prev, element).First;
    }

    /// <summary>
    /// Returns a <see cref="Pair"/> of the first encountered <see cref="IListNode{TNode}"/> in this list, whose
    /// {@code value} is equal to the given {@code element}, and its index. Or if this list does not
    /// contain such node a Pair of {@code null} and {@code -1};
    /// <para>
    /// The search starts at the node supplied by {@code first} and advances in the direction induced
    /// by the specified {@code next} operator.
    /// </para>
    /// </summary>
    /// <param name="first"> supplier of the first node to check if this list is not empty.</param>
    /// <param name="next"> {@code Function} to get from the current node the next node to check.</param>
    /// <param name="element"> the element for that the first node with equal value is searched.</param>
    /// <returns>a <see cref="Pair"/> of the first encountered {@code ListNode} holding a {@code value}
    ///         equal to {@code element} and its index, or if no such node was found a
    ///         {@code Pair.of(null, -1)}</returns>
    private Pair<ListNodeImpl<TEdge>, int> SearchNode(
        Func<ListNodeImpl<TEdge>>                      first,
        Func<ListNodeImpl<TEdge>, ListNodeImpl<TEdge>> next,
        object                                                element
    )
    {
        if (!Empty)
        {
            var                 index     = 0;
            var firstNode = first();
            var node      = firstNode;
            do
            {
                if (Equals(node.value, element))
                {
                    return Pair.of(node, index);
                }

                index++;
                node = next(node);
            } while (node != firstNode);
        }

        return Pair.of(null, -1);
    }

    /// <summary>
    /// Inserts the specified element at the front of this list. Returns the <see cref="IListNode{TNode}"/>
    /// allocated to store the {@code value}. The returned {@code ListNode} is the new head of the
    /// list.
    /// <para>
    /// This method is equivalent to <see cref="addFirst(Object)"/> but returns the allocated
    /// {@code ListNode}.
    /// </para>
    /// </summary>
    /// <param name="element"> the element to add.</param>
    /// <returns>the {@code ListNode} allocated to store the {@code value}</returns>
    public virtual IListNode<TEdge> AddElementFirst(TEdge element)
    {
        IListNode<TEdge> node = new ListNodeImpl<TEdge>(element);
        AddNode(0, node);
        return node;
    }

    /// <summary>
    /// Inserts the specified element at the end of this list. Returns the <see cref="IListNode{TNode}"/> allocated
    /// to store the {@code value}. The returned {@code ListNode} is the new tail of the list.
    /// <para>
    /// This method is equivalent to <see cref="addLast(Object)"/> but returns the allocated
    /// {@code ListNode}.
    /// </para>
    /// </summary>
    /// <param name="element"> the element to add.</param>
    /// <returns>the {@code ListNode} allocated to store the {@code value}</returns>
    public virtual IListNode<TEdge> AddElementLast(TEdge element)
    {
        IListNode<TEdge> node = new ListNodeImpl<TEdge>(element);
        AddNode(_size, node);
        return node;
    }

    /// <summary>
    /// Inserts the specified element before the specified <see cref="IListNode{TNode}"/> in this list.
    /// Returns the {@code ListNode} allocated to store the {@code value}.
    /// </summary>
    /// <param name="successor"> {@code ListNode} before which the node holding {@code value} is inserted.</param>
    /// <param name="element"> the element to add.</param>
    /// <returns>the {@code ListNode} allocated to store the {@code value}</returns>
    /// <exception cref="ArgumentException"> if {@code successor} is not contained in this list.</exception>
    /// <exception cref="NullReferenceException"> if {@code successor} is {@code null} </exception>
    public virtual IListNode<TEdge> AddElementBeforeNode(IListNode<TEdge> successor, TEdge element)
    {
        IListNode<TEdge> node = new ListNodeImpl<TEdge>(element);
        AddNodeBefore(node, successor);
        return node;
    }

    // List methods (shortcut for most commonly used methods to avoid iterator creation)

    /// <inheritdoc/>
    public override void Add(int index, TEdge element)
    {
        if (index == _size)
        {
            // also true if this is empty
            AddElementLast(element);
        }
        else
        {
            AddElementBeforeNode(GetNode(index), element);
        }
    }

    /// <inheritdoc/>
    public override TEdge Get(int index)
    {
        return GetNodeAt(index).value;
    }

    /// <inheritdoc/>
    public override TEdge Remove(int index)
    {
        var node = GetNode(index);
        RemoveNode(node);
        return node.Value;
    }

    // Deque methods

    /// <inheritdoc/>
    public override void AddFirst(TEdge edge)
    {
        AddElementFirst(edge);
    }

    /// <inheritdoc/>
    public override void AddLast(TEdge edge)
    {
        AddElementLast(edge);
    }

    /// <inheritdoc/>
    public override bool OfferFirst(TEdge edge)
    {
        AddElementFirst(edge);
        return true;
    }

    /// <inheritdoc/>
    public override bool OfferLast(TEdge edge)
    {
        AddElementLast(edge);
        return true;
    }

    /// <inheritdoc/>
    public override TEdge RemoveFirst()
    {
        if (Empty)
        {
            throw new NoSuchElementException();
        }

        IListNode<TEdge> node = _head;
        RemoveNode(node); // changes head
        return node.Value;
    }

    /// <inheritdoc/>
    public override TEdge RemoveLast()
    {
        if (Empty)
        {
            throw new NoSuchElementException();
        }

        IListNode<TEdge> node = Tail();
        RemoveNode(node); // changes tail
        return node.Value;
    }

    /// <inheritdoc/>
    public override TEdge PollFirst()
    {
        if (Empty)
        {
            return default(TEdge);
        }

        IListNode<TEdge> node = _head;
        RemoveNode(node); // changes head
        return node.Value;
    }

    /// <inheritdoc/>
    public override TEdge PollLast()
    {
        if (Empty)
        {
            return default(TEdge);
        }

        IListNode<TEdge> node = Tail();
        RemoveNode(node); // changes tail()
        return node.Value;
    }

    /// <inheritdoc/>
    public override TEdge First
    {
        get
        {
            return FirstNode.Value;
        }
    }

    /// <inheritdoc/>
    public override TEdge Last
    {
        get
        {
            return LastNode.Value;
        }
    }

    /// <inheritdoc/>
    public override TEdge PeekFirst()
    {
        return Empty ? default(TEdge) : First;
    }

    /// <inheritdoc/>
    public override TEdge PeekLast()
    {
        return Empty ? default(TEdge) : Last;
    }

    /// <inheritdoc/>
    public override bool RemoveFirstOccurrence(object o)
    {
        var node = NodeOf(o);
        if (node != null)
        {
            RemoveNode(node);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public override bool RemoveLastOccurrence(object o)
    {
        var node = LastNodeOf(o);
        if (node != null)
        {
            RemoveNode(node);
            return true;
        }

        return false;
    }

    // Queue methods

    /// <inheritdoc/>
    public override bool Offer(TEdge edge)
    {
        return OfferLast(edge);
    }

    /// <inheritdoc/>
    public override TEdge Remove()
    {
        return RemoveFirst();
    }

    /// <inheritdoc/>
    public override TEdge Poll()
    {
        return PollFirst();
    }

    /// <inheritdoc/>
    public override TEdge Element()
    {
        return First;
    }

    /// <inheritdoc/>
    public override TEdge Peek()
    {
        return PeekFirst();
    }

    // Stack methods

    /// <inheritdoc/>
    public override void Push(TEdge edge)
    {
        AddFirst(edge);
    }

    /// <inheritdoc/>
    public override TEdge Pop()
    {
        return RemoveFirst();
    }

    // special bulk methods

    /// <summary>
    /// Inverts the list. For instance, calling this method on the list $(a,b,c,\dots,x,y,z)$ will
    /// result in the list $(z,y,x,\dots,c,b,a)$. This method does only pointer manipulation, meaning
    /// that all the list nodes allocated for the previously added elements are valid after this
    /// method finishes.
    /// </summary>
    public virtual void Invert()
    {
        if (_size < 2)
        {
            return;
        }

        var newHead = Tail();
        var current = _head;
        do
        {
            ListNodeImpl<TEdge> next = current.next;

            current.next = current.prev;
            current.prev = next;

            current = next;
        } while (current != _head);

        _head = newHead;
        ++modCount;
    }

    /// <summary>
    /// Moves all <see cref="IListNode{TNode}"/> of the given {@code sourceList} to this list and inserts
    /// them all before the node previously at the given position. All the {@code nodes} of
    /// {@code movedList} are moved to this list. When this method terminates this list contains all
    /// nodes of {@code movedList} and {@code movedList} is empty.
    /// </summary>
    /// <param name="index"> index of the first element of {@code list} in this {@code list} after it was
    ///        added.</param>
    /// <param name="movedList"> the {@code DoublyLinkedList} to move to this one.</param>
    /// <exception cref="NullReferenceException"> if {@code movedList} is {@code null} </exception>
    public virtual void MoveFrom(int index, DoublyLinkedList<TEdge> movedList)
    {
        LinkListIntoThisBefore(index, movedList);
    }

    /// <summary>
    /// Appends the {@code movedList} to the end of this list. All the elements from
    /// {@code movedList} are transferred to this list, i.TEdge. the {@code list} is empty after calling
    /// this method.
    /// </summary>
    /// <param name="movedList"> the {@code DoublyLinkedList} to append to this one.</param>
    /// <exception cref="NullReferenceException"> if {@code movedList} is {@code null} </exception>
    public virtual void Append(DoublyLinkedList<TEdge> movedList)
    {
        MoveFrom(_size, movedList);
    }

    /// <summary>
    /// Prepends the {@code movedList} to the beginning of this list. All the elements from
    /// {@code movedList} are transferred to this list, i.TEdge. the {@code movedList} is empty after
    /// calling this method.
    /// </summary>
    /// <param name="movedList"> the {@code DoublyLinkedList} to prepend to this one.</param>
    /// <exception cref="NullReferenceException"> if {@code movedList} is {@code null} </exception>
    public virtual void Prepend(DoublyLinkedList<TEdge> movedList)
    {
        MoveFrom(0, movedList);
    }

    // ----------------------------------------------------------------------------
    // (List)Iterators

    /// <summary>
    /// Returns a <see cref="INodeIterator{TEdge}"/> that starts at the first <see cref="IListNode{TNode}"/> of this list that is
    /// equal to the specified {@code firstElement}, iterates in forward direction over the end of
    /// this list until the first node.
    /// <para>
    /// The first call to <see cref="INodeIterator{TEdge}.nextNode()"/> returns the first {@code node} that holds a
    /// value such that {@code Objects.equals(node.getValue, firstElement)} returns {@code true}. The
    /// returned {@code NodeIterator} iterates in forward direction returning the respective next
    /// element in subsequent calls to {@code next(Node)}. The returned iterator ignores the actual
    /// bounds of this {@code DoublyLinkedList} and iterates until the node before the first one is
    /// reached. Its <see cref="INodeIterator{TEdge}.hasNext() hasNext()"/> returns {@code false} if the next node
    /// would be the first one.
    /// </para>
    /// </summary>
    /// <param name="firstElement"> the element equal to the first {@code next()} </param>
    /// <returns>a circular {@code NodeIterator} iterating forward from {@code firstElement}</returns>
    public virtual INodeIterator<TEdge> CircularIterator(TEdge firstElement)
    {
        var startNode = (ListNodeImpl<TEdge>)NodeOf(firstElement);
        if (startNode == null)
        {
            throw new NoSuchElementException();
        }

        return new ListNodeIteratorImpl(this, 0, startNode);
    }

    /// <summary>
    /// Returns a <see cref="INodeIterator{TEdge}"/> that starts at the first <see cref="IListNode{TNode}"/> of this list that is
    /// equal to the specified {@code firstElement}, iterates in reverse direction over the end of
    /// this list until the first node.
    /// <para>
    /// The first call to <see cref="INodeIterator{TEdge}.nextNode()"/> returns the first {@code node} that holds a
    /// value such that {@code Objects.equals(node.getValue, firstElement)} returns {@code true}. The
    /// returned {@code NodeIterator} iterates in reverse direction returning the respective previous
    /// element in subsequent calls to {@code next(Node)}. The returned iterator ignores the actual
    /// bounds of this {@code DoublyLinkedList} and iterates until the node before the first one is
    /// reached. Its <see cref="INodeIterator{TEdge}.hasNext() hasNext()"/> returns {@code false} if the next node
    /// would be the first one.
    /// </para>
    /// </summary>
    /// <param name="firstElement"> the element equal to the first {@code next()} </param>
    /// <returns>a circular {@code NodeIterator} iterating backwards from {@code firstElement}</returns>
    public virtual INodeIterator<TEdge> ReverseCircularIterator(TEdge firstElement)
    {
        var startNode = (ListNodeImpl<TEdge>)NodeOf(firstElement);
        if (startNode == null)
        {
            throw new NoSuchElementException();
        }

        return ReverseIterator(new ListNodeIteratorImpl(this, _size, startNode.next));
    }

    /// <inheritdoc/>
    public override INodeIterator<TEdge> DescendingIterator()
    {
        return ReverseIterator(ListIterator(_size));
    }

    /// <inheritdoc/>
    public override INodeIterator<TEdge> Iterator()
    {
        return ListIterator();
    }

    /// <inheritdoc/>
    public override IListNodeIterator<TEdge> ListIterator()
    {
        return ListIterator(0);
    }

    /// <inheritdoc/>
    public override IListNodeIterator<TEdge> ListIterator(int index)
    {
        return new ListNodeIteratorImpl(this, index);
    }

    /// <summary>
    /// Returns a <see cref="IListNodeIterator{TEdge}"/> over the elements in this list (in proper sequence)
    /// starting with the first <see cref="IListNode{TNode}"/> whose value is equal to the specified
    /// {@code element}.
    /// </summary>
    /// <param name="element"> the first element to be returned from the list iterator (by a call to the
    ///        {@code next} method) </param>
    /// <returns>a list iterator over the elements in this list (in proper sequence)</returns>
    /// <exception cref="NoSuchElementException"> if {@code element} is not in the list.</exception>
    public virtual IListNodeIterator<TEdge> ListIterator(TEdge element)
    {
        Pair<ListNodeImpl<TEdge>, int> startPair  = SearchNode(() => _head, n => n.next, element);
        ListNodeImpl<TEdge>            startNode  = startPair.First;
        int                            startIndex = startPair.Second;
        if (startNode == null)
        {
            throw new NoSuchElementException();
        }

        return new ListNodeIteratorImpl(this, startIndex, startNode);
    }

    /// <summary>
    /// An extension of the <see cref="System.Collections.IEnumerator"/> interface for <see cref="DoublyLinkedList DoublyLinkedLists"/>
    /// exposing their <see cref="IListNode{TNode}"/>.
    /// </summary>
    /// <typeparam name="TEdge"> the list element type.</typeparam>
    public interface INodeIterator<TEdge> : IEnumerator<TEdge>
    {
        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        override TEdge Next()
        {
            return nextNode().getValue();
        }

        /// <summary>
        /// Returns the next <see cref="IListNode{TNode}"/> in the list and advances the cursor position.
        /// </summary>
        /// <returns>the next {@code ListNode}</returns>
        /// <seealso cref="ListIterator.next()"/>
        IListNode<TEdge> NextNode();
    }

    /// <summary>
    /// An extension of the <see cref="System.Collections.IEnumerator"/> interface for {@link DoublyLinkedList
    /// DoublyLinkedLists} exposing their <see cref="IListNode{TNode}"/>.
    /// </summary>
    /// <typeparam name="TEdge"> the list element type.</typeparam>
    public interface IListNodeIterator<TEdge> : IEnumerator<TEdge>, INodeIterator<TEdge>
    {
        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        override TEdge Next()
        {
            return nextNode().getValue();
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        override TEdge Previous()
        {
            return previousNode().getValue();
        }

        /// <summary>
        /// Returns the previous <see cref="IListNode{TNode}"/> in the list and moves the cursor position
        /// backwards.
        /// </summary>
        /// <returns>the previous {@code ListNode}</returns>
        /// <seealso cref="ListIterator.previous()"/>
        IListNode<TEdge> PreviousNode();
    }

    /// <summary>
    /// An implementation of the <see cref="DoublyLinkedList.ListNodeIterator"/> interface.
    /// </summary>
    private class ListNodeIteratorImpl : IListNodeIterator<TEdge>
    {
        private readonly DoublyLinkedList<TEdge> _outerInstance;

        /// <summary>
        /// Index in this list of the ListNode returned next. </summary>
        internal int NextIndex;

        /// <summary>
        /// ListNode this iterator will return next. Null if this list is empty. </summary>
        internal ListNodeImpl<TEdge> Next;

        /// <summary>
        /// ListNode this iterator returned last. </summary>
        internal ListNodeImpl<TEdge> Last = null;

        /// <summary>
        /// The number of modifications the list have had at the moment when this iterator was
        /// created
        /// </summary>
        internal int ExpectedModCount = modCount;

        internal ListNodeIteratorImpl(DoublyLinkedList<TEdge> outerInstance, int startIndex)
        {
            _outerInstance = outerInstance;
            this.nextIndex      = startIndex;
            if (startIndex == outerInstance._size)
            {
                Next = outerInstance.Empty ? null : outerInstance._head;
            }
            else
            {
                Next = outerInstance.GetNodeAt(startIndex);
            }
        }

        internal ListNodeIteratorImpl(
            DoublyLinkedList<TEdge> outerInstance,
            int                     startIndex,
            ListNodeImpl<TEdge>     startNode
        )
        {
            _outerInstance = outerInstance;
            this.nextIndex      = startIndex;
            Next           = startNode;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool HasNext()
        {
            return nextIndex < outerInstance._size;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool HasPrevious()
        {
            return nextIndex > 0;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override int NextIndex()
        {
            return nextIndex;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override int PreviousIndex()
        {
            return nextIndex - 1;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual ListNodeImpl<TEdge> NextNode()
        {
            CheckForComodification();
            if (!HasNext())
            {
                throw new NoSuchElementException();
            }

            Last = Next;
            Next = Next.next;
            nextIndex++;
            return Last;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual IListNode<TEdge> PreviousNode()
        {
            CheckForComodification();
            if (!HasPrevious())
            {
                throw new NoSuchElementException();
            }

            Last = Next = Next.prev;
            nextIndex--;
            return Last;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override void Add(TEdge edge)
        {
            CheckForComodification();

            if (nextIndex == outerInstance._size)
            {
                outerInstance.AddElementLast(edge); // sets head to new node of TEdge if was empty
                if (outerInstance._size == 1)
                {
                    // was empty
                    Next = outerInstance._head; // jump over head threshold, so cursor is at the end
                }
            }
            else
            {
                outerInstance.AddElementBeforeNode(Next, edge);
            }

            Last = null;
            nextIndex++;
            ExpectedModCount++;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override void Set(TEdge edge)
        {
            if (Last == null)
            {
                throw new InvalidOperationException();
            }

            CheckForComodification();
            // replace node returned last with a new node holding TEdge

            IListNode<TEdge> nextNode = Last.next;
            bool             wasLast  = Last == outerInstance.Tail();
            outerInstance.RemoveNode(Last);
            if (wasLast)
            {
                // or the sole node
                Last = (ListNodeImpl<TEdge>)outerInstance.AddElementLast(edge);
            }
            else
            {
                Last = (ListNodeImpl<TEdge>)outerInstance.AddElementBeforeNode(nextNode, edge);
            }

            ExpectedModCount += 2; // because of unlink and add
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override void Remove()
        {
            if (Last == null)
            {
                throw new InvalidOperationException();
            }

            CheckForComodification();

            ListNodeImpl<TEdge> lastsNext = Last.next;
            outerInstance.RemoveNode(Last);
            if (Next == Last)
            {
                // previousNode() called before
                // removed element after cursor (which would have been next)
                Next = lastsNext;
            }
            else
            {
                // nextNode() called before
                // removed element before cursor (next is unaffected but the index decreases)
                nextIndex--;
            }

            Last = null;
            ExpectedModCount++;
        }

        /// <summary>
        /// Verifies that the list structure hasn't been changed since the iteration started
        /// </summary>
        internal virtual void CheckForComodification()
        {
            if (ExpectedModCount != modCount)
            {
                throw new ConcurrentModificationException();
            }
        }
    }

    /// <summary>
    /// Returns a <see cref="INodeIterator{TEdge}"/> that iterates in reverse order, assuming the cursor of the
    /// specified <see cref="IListNodeIterator{TEdge}"/> is behind the tail of the list.
    /// </summary>
    private static INodeIterator<TEdge> ReverseIterator<TEdge>(IListNodeIterator<TEdge> listIterator)
    {
        return new NodeIteratorAnonymousInnerClass(listIterator);
    }

    private class NodeIteratorAnonymousInnerClass : INodeIterator<TEdge>
    {
        private DoublyLinkedList.ListNodeIterator<TEdge> _listIterator;

        public NodeIteratorAnonymousInnerClass(DoublyLinkedList.ListNodeIterator<TEdge> listIterator)
        {
            _listIterator = listIterator;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public bool HasNext()
        {
            return _listIterator.hasPrevious();
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public IListNode<TEdge> NextNode()
        {
            return _listIterator.PreviousNode();
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public void Remove()
        {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
            _listIterator.remove();
        }
    }

    /// <summary>
    /// Container for the elements stored in a <see cref="DoublyLinkedList"/>.
    /// <para>
    /// A <see cref="IListNode{TNode}"/> is either contained exactly once in exactly one {@code DoublyLinkedList}
    /// or contained in no {@code DoublyLinkedList}.
    /// </para>
    /// </summary>
    /// <typeparam name="TNode"> the type of the element stored in this node.</typeparam>
    public interface IListNode<TNode>
    {
        /// <summary>
        /// Returns the immutable value this {@code ListNode} contains.
        /// </summary>
        /// <returns>the value this list node contains.</returns>
        TNode Value { get; }

        /// <summary>
        /// Returns the next node in the list structure with respect to this node
        /// </summary>
        /// <returns>the next node in the list structure with respect to this node.</returns>
        IListNode<TNode> Next { get; }

        /// <summary>
        /// Returns the previous node in the list structure with respect to this node
        /// </summary>
        /// <returns>the previous node in the list structure with respect to this node.</returns>
        IListNode<TNode> Prev { get; }
    }

    /// <summary>
    /// The default <see cref="IListNode{TNode}"/> implementation that enables checks and enforcement of a single
    /// container list policy.
    /// </summary>
    private class ListNodeImpl<TNode> : IListNode<TNode>
    {
        internal readonly TNode                   Value;
        internal          DoublyLinkedList<TNode> List = null;
        internal          ListNodeImpl<TNode>     Next = null;
        internal          ListNodeImpl<TNode>     Prev = null;

        /// <summary>
        /// Creates new list node
        /// </summary>
        /// <param name="value"> the value this list node stores.</param>
        internal ListNodeImpl(TNode value)
        {
            this.value = value;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override string ToString()
        {
            if (List == null)
            {
                return " - " + value + " - "; // not in a list
            }
            else
            {
                return prev.value + " -> " + value + " -> " + next.value;
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual TNode Value
        {
            get
            {
                return value;
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual ListNodeImpl<TNode> Next
        {
            get
            {
                return next;
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual ListNodeImpl<TNode> Prev
        {
            get
            {
                return prev;
            }
        }
    }
}
