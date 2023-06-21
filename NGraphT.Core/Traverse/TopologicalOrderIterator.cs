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

using System.Diagnostics.CodeAnalysis;
using NGraphT.Core.DotNetUtil;
using NGraphT.Core.Util;

namespace NGraphT.Core.Traverse;

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
///
/// <para>
/// The iterator crosses components but does not track them, it only tracks visited vertices. The
/// iterator will detect (at some point) if the graph is not a directed acyclic graph and throw a
/// <see cref="NotDirectedAcyclicGraphException"/>.
/// </para>
///
/// <para>
/// For this iterator to work correctly the graph must not be modified during iteration. Currently
/// there are no means to ensure that, nor to fail-fast. The results of such modifications are
/// undefined.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Marden Neubert.</remarks>
/// <remarks>Author: Dimitrios Michail.</remarks>
public sealed class TopologicalOrderIterator<TVertex, TEdge> : AbstractGraphIterator<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private readonly IQueue<TVertex>          _queue;
    private readonly Dictionary<TVertex, int> _inDegreeMap = new();

    private int      _remainingVertices;
    private TVertex? _current;

    /// <summary>
    /// Construct a topological order iterator.
    ///
    /// <para>
    /// Traversal will start at one of the graph's <i>sources</i>. See the definition of source at
    /// <a href="http://mathworld.wolfram.com/Source.html">
    /// http://mathworld.wolfram.com/Source.html</a>. In case of partial order, tie-breaking is
    /// arbitrary.
    /// </para>
    ///
    /// </summary>
    /// <param name="graph"> the directed graph to be iterated.</param>
    public TopologicalOrderIterator(IGraph<TVertex, TEdge> graph)
        : this(graph, comparer: null)
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
    /// </para>
    ///
    /// </summary>
    /// <param name="graph"> the directed graph to be iterated.</param>
    /// <param name="comparer"> comparator in order to break ties in case of partial order.</param>
    public TopologicalOrderIterator(IGraph<TVertex, TEdge> graph, IComparer<TVertex>? comparer)
        : base(graph)
    {
        // TODO: GraphTests.RequireDirected(graph);
        if (comparer == null)
        {
            _queue = new QueueAdapter<TVertex>(new Queue<TVertex>());
        }
        else
        {
            _queue = new PriorityQueueAdapter<TVertex>(new PriorityQueue<TVertex, TVertex>(comparer));
        }

        // count in-degrees
        foreach (var v in graph.VertexSet())
        {
            var degree = 0;
            foreach (var edge in graph.IncomingEdgesOf(v))
            {
                var u = Graphs.GetOppositeVertex(graph, edge, v);
                if (v.Equals(u))
                {
                    throw new NotDirectedAcyclicGraphException("The given graph grpah is not acyclic", nameof(graph));
                }

                degree++;
            }

            _inDegreeMap[v] = degree;
            if (degree == 0)
            {
                _queue.Enqueue(v);
            }
        }

        // record vertices count
        _remainingVertices = graph.VertexSet().Count;
    }

    /// <summary>
    /// <inheritdoc/>
    ///
    /// Always returns true since the iterator does not care about components.
    /// </summary>
    public override bool CrossComponentTraversal
    {
        get => true;
        set
        {
            if (!value)
            {
                throw new ArgumentException("Iterator is always cross-component", nameof(value));
            }
        }
    }

    /// <summary>
    /// Returns the current vertex in the ordering.
    /// </summary>
    /// <returns>the current vertex in the ordering.</returns>
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override TVertex Current => _current ?? throw new NoSuchElementException();

    public override bool MoveNext()
    {
        var previous = _current;
        if (previous != null && NListeners != 0)
        {
            FireVertexFinished(CreateVertexTraversalEvent(previous));
        }

        _current = Advance();
        if (_current != null && NListeners != 0)
        {
            FireVertexTraversed(CreateVertexTraversalEvent(_current));
        }

        return _current != null;
    }

    private TVertex? Advance()
    {
        if (_queue.TryDequeue(out var result))
        {
            foreach (var edge in Graph.OutgoingEdgesOf(result))
            {
                var other = Graphs.GetOppositeVertex(Graph, edge, result);

                var inDegree = _inDegreeMap[other];
                if (inDegree == 1)
                {
                    _queue.Enqueue(other);
                }

                if (inDegree > 0)
                {
                    _inDegreeMap[other]--;
                }
            }

            _remainingVertices--;
        }
        else
        {
            // Still expecting some vertices, but no vertex has zero degree.
            if (_remainingVertices > 0)
            {
#pragma warning disable MA0015
                throw new NotDirectedAcyclicGraphException();
#pragma warning restore MA0015
            }
        }

        return result;
    }
}
