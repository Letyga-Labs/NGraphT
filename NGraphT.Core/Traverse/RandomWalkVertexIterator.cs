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
using System.Diagnostics.CodeAnalysis;
using NGraphT.Core.Util;

namespace NGraphT.Core.Traverse;

/// <summary>
/// A random walk iterator.
///
/// <para>
/// "Given a graph and a starting point, we select a neighbor of it at random, and move to this
/// neighbor; then we select a neighbor of this point at random, and move to it etc. The (random)
/// sequence of points selected this way is a random walk on the graph." This very simple definition,
/// together with a comprehensive survey can be found at: "Lovász, L. (1993). Random walks on graphs:
/// A survey. Combinatorics, Paul erdos is eighty, 2(1), 1-46.".
/// </para>
///
/// <para>
/// In its default variant the probability of selecting an outgoing edge is one over the (out) degree
/// of the vertex. In case the user requests a weighted walk, then the probability of each edge is
/// equal to its weight divided by the total weight of all outgoing edges. The walk can also be
/// bounded by a maximum number of hops (edges traversed). The iterator returns
/// <see cref="NoSuchElementException"/> when this bound is reached.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
[SuppressMessage("Security", "CA5394:Do not use insecure randomness")]
public sealed class RandomWalkVertexIterator<TVertex, TEdge> : IEnumerator<TVertex>
    where TVertex : class
    where TEdge : class
{
    private readonly IGraph<TVertex, TEdge>      _graph;
    private readonly Dictionary<TVertex, double> _outEdgesTotalWeight = new();

    private readonly bool   _weighted;
    private readonly long   _maxHops;
    private readonly Random _random;

    private long     _hops;
    private TVertex? _current;

    /// <summary>
    /// Create a new iterator.
    /// </summary>
    /// <param name="graph"> the graph.</param>
    /// <param name="vertex"> the starting vertex.</param>
    public RandomWalkVertexIterator(IGraph<TVertex, TEdge> graph, TVertex vertex)
        : this(graph, vertex, maxHops: long.MaxValue, weighted: false, new Random())
    {
    }

    /// <summary>
    /// Create a new iterator.
    /// </summary>
    /// <param name="graph"> the graph.</param>
    /// <param name="vertex"> the starting vertex.</param>
    /// <param name="maxHops"> maximum hops to perform during the walk.</param>
    public RandomWalkVertexIterator(IGraph<TVertex, TEdge> graph, TVertex vertex, long maxHops)
        : this(graph, vertex, maxHops, weighted: false, new Random())
    {
    }

    /// <summary>
    /// Create a new iterator.
    /// </summary>
    /// <param name="graph"> the graph.</param>
    /// <param name="vertex"> the starting vertex.</param>
    /// <param name="maxHops"> maximum hops to perform during the walk.</param>
    /// <param name="weighted">
    /// whether to perform a weighted random walk (compute probabilities based on the edge weights).
    /// </param>
    /// <param name="random"> the random number generator.</param>
    public RandomWalkVertexIterator(
        IGraph<TVertex, TEdge> graph,
        TVertex                vertex,
        long                   maxHops,
        bool                   weighted,
        Random                 random)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(vertex);
        if (!graph.ContainsVertex(vertex))
        {
            throw new ArgumentException("Random walk must start at a graph vertex", nameof(vertex));
        }

        _graph    = graph;
        _weighted = weighted;
        _current  = vertex;
        _maxHops  = maxHops;
        _random   = random;
    }

    object IEnumerator.Current => Current;

    /// <summary>
    /// Returns the current vertex in the ordering.
    /// </summary>
    /// <returns>the current vertex in the ordering.</returns>
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public TVertex Current => _current ?? throw new NoSuchElementException();

    public bool MoveNext()
    {
        Advance();
        return _current != null;
    }

    public void Reset()
    {
        throw new NotSupportedException("Reset");
    }

    public void Dispose()
    {
        // empty
    }

    private void Advance()
    {
        if (_hops >= _maxHops)
        {
            _current = null;
            return;
        }

        _hops++;
        if (_current == null || _graph.OutDegreeOf(_current) == 0)
        {
            _current = null;
            return;
        }

        TEdge? edge = null;
        if (_weighted)
        {
            if (!_outEdgesTotalWeight.ContainsKey(_current))
            {
                _outEdgesTotalWeight[_current] = _graph.OutgoingEdgesOf(_current).Sum(it => _graph.GetEdgeWeight(it));
            }

            var p = _outEdgesTotalWeight[_current] * _random.NextDouble();

            var cumulativeP = 0d;
            foreach (var curEdge in _graph.OutgoingEdgesOf(_current))
            {
                cumulativeP += _graph.GetEdgeWeight(curEdge);
                if (p <= cumulativeP)
                {
                    edge = curEdge;
                    break;
                }
            }
        }
        else
        {
            var outEdges = _graph.OutgoingEdgesOf(_current);
            edge = outEdges.ElementAt(_random.Next(outEdges.Count));
        }

        _current = Graphs.GetOppositeVertex(_graph, edge!, _current);
    }
}
