/*
 * (C) Copyright 2004-2021, by Marden Neubert and Contributors.
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

namespace NGraphT.Core.Traverse;

using Core;
using Util;

/// <summary>
/// A topological ordering iterator for a directed acyclic graph.
///
/// <para>
/// A topological order is a permutation <c>p</c> of the vertices of a graph such that an edge
/// <c>(i,j)</c> implies that <c>i</c> appears before <c>j</c> in <c>p</c>.
/// For more information see
/// <a href="https://en.wikipedia.org/wiki/Topological_sorting">wikipedia</a> or
/// <a href="http://mathworld.wolfram.com/TopologicalSort.html">wolfram</a>.
/// </para>
/// <para>
/// The iterator crosses components but does not track them, it only tracks visited vertices. The
/// iterator will detect (at some point) if the graph is not a directed acyclic graph and throw a
/// <seealso cref="NotDirectedAcyclicGraphException"/>.
/// </para>
/// <para>
/// For this iterator to work correctly the graph must not be modified during iteration. Currently
/// there are no means to ensure that, nor to fail-fast. The results of such modifications are
/// undefined.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Marden Neubert.</remarks>
/// <remarks>Author: Dimitrios Michail.</remarks>
public class TopologicalOrderIterator<TNode, TEdge> : AbstractGraphIterator<TNode, TEdge>
{
    private LinkedList<TNode>                     _queue;
    private IDictionary<TNode, ModifiableInteger> _inDegreeMap;
    private int                                   _remainingVertices;
    private TNode                                 _cur;

    /// <summary>
    /// Construct a topological order iterator.
    ///
    /// <para>
    /// Traversal will start at one of the graph's <i>sources</i>. See the definition of source at
    /// <a href="http://mathworld.wolfram.com/Source.html">
    /// http://mathworld.wolfram.com/Source.html</a>. In case of partial order, tie-breaking is
    /// arbitrary.
    ///
    /// </para>
    /// </summary>
    /// <param name="graph"> the directed graph to be iterated.</param>
    public TopologicalOrderIterator(IGraph<TNode, TEdge> graph)
        : this(graph, (IComparer<TNode>)null)
    {
    }

    /// <summary>
    /// Construct a topological order iterator.
    ///
    /// <para>
    /// Traversal will start at one of the graph's <i>sources</i>. See the definition of source at
    /// <a href="http://mathworld.wolfram.com/Source.html">
    /// http://mathworld.wolfram.com/Source.html</a>. In case of partial order, a comparator is used
    /// to break ties.
    ///
    /// </para>
    /// </summary>
    /// <param name="graph"> the directed graph to be iterated.</param>
    /// <param name="comparator"> comparator in order to break ties in case of partial order.</param>
    public TopologicalOrderIterator(IGraph<TNode, TEdge> graph, IComparer<TNode> comparator)
        : base(graph)
    {
        GraphTests.RequireDirected(graph);

        // create queue
        if (comparator == null)
        {
            _queue = new LinkedList<TNode>();
        }
        else
        {
            _queue = new PriorityQueue<TNode>(comparator);
        }

        // count in-degrees
        _inDegreeMap = new Dictionary<TNode, ModifiableInteger>();
        foreach (var node in graph.VertexSet())
        {
            var d = 0;
            foreach (var edge in graph.IncomingEdgesOf(node))
            {
                var u = Graphs.GetOppositeVertex(graph, edge, node);
                if (node.Equals(u))
                {
                    throw new NotDirectedAcyclicGraphException();
                }

                d++;
            }

            _inDegreeMap[node] = new ModifiableInteger(d);
            if (d == 0)
            {
                _queue.AddLast(node);
            }
        }

        // record vertices count
        _remainingVertices = graph.VertexSet().Count;
    }

    /// <summary>
    /// {@inheritDoc}
    /// 
    /// Always returns true since the iterator does not care about components.
    /// </summary>
    public override bool CrossComponentTraversal
    {
        get
        {
            return true;
        }
        set
        {
            if (!value)
            {
                throw new ArgumentException("Iterator is always cross-component");
            }
        }
    }


    public override bool HasNext()
    {
        if (_cur != null)
        {
            return true;
        }

        _cur = Advance();
        if (_cur != null && NListeners != 0)
        {
            FireVertexTraversed(CreateVertexTraversalEvent(_cur));
        }

        return _cur != null;
    }

    public override TNode Next()
    {
        if (!HasNext())
        {
            throw new NoSuchElementException();
        }

        var result = _cur;
        _cur = default(TNode);
        if (NListeners != 0)
        {
            FireVertexFinished(CreateVertexTraversalEvent(result));
        }

        return result;
    }

    private TNode Advance()
    {
        TNode result = _queue.RemoveFirst();

        if (result != null)
        {
            foreach (TEdge edge in graph.OutgoingEdgesOf(result))
            {
                var other = Graphs.GetOppositeVertex(graph, edge, result);

                var inDegree = _inDegreeMap[other];
                if (inDegree.value > 0)
                {
                    inDegree.value--;

                    if (inDegree.value == 0)
                    {
                        _queue.AddLast(other);
                    }
                }
            }

            --_remainingVertices;
        }
        else
        {
            /*
             * Still expecting some vertices, but no vertex has zero degree.
             */
            if (_remainingVertices > 0)
            {
                throw new NotDirectedAcyclicGraphException();
            }
        }

        return result;
    }
}
