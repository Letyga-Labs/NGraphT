/*
 * (C) Copyright 2020-2021, by Dimitrios Michail and Contributors.
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
/// A survey. Combinatorics, Paul erdos is eighty, 2(1), 1-46."
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
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
[SuppressMessage("Security", "CA5394:Do not use insecure randomness")]
public class RandomWalkVertexIterator<TNode, TEdge> : IEnumerator<TNode>
    where TNode : class
{
    private readonly IGraph<TNode, TEdge>       _graph;
    private readonly IDictionary<TNode, double> _outEdgesTotalWeight = new Dictionary<TNode, double>();

    private readonly bool   _weighted;
    private readonly long   _maxHops;
    private readonly Random _random;

    private long   _hops;
    private TNode? _nextVertex;

    /// <summary>
    /// Create a new iterator.
    /// </summary>
    /// <param name="graph"> the graph.</param>
    /// <param name="vertex"> the starting vertex.</param>
    public RandomWalkVertexIterator(IGraph<TNode, TEdge> graph, TNode vertex)
        : this(graph, vertex, maxHops: long.MaxValue, weighted: false, new Random())
    {
    }

    /// <summary>
    /// Create a new iterator.
    /// </summary>
    /// <param name="graph"> the graph.</param>
    /// <param name="vertex"> the starting vertex.</param>
    /// <param name="maxHops"> maximum hops to perform during the walk.</param>
    public RandomWalkVertexIterator(IGraph<TNode, TEdge> graph, TNode vertex, long maxHops)
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
        IGraph<TNode, TEdge> graph,
        TNode                vertex,
        long                 maxHops,
        bool                 weighted,
        Random               random)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(vertex);
        if (!graph.ContainsVertex(vertex))
        {
            throw new ArgumentException("Random walk must start at a graph vertex", nameof(vertex));
        }

        _graph      = graph;
        _weighted   = weighted;
        _nextVertex = vertex;
        _maxHops    = maxHops;
        _random     = random;
    }

    object IEnumerator.Current => Current;

    public TNode Current { get; }

    public bool MoveNext()
    {
        if (_nextVertex == null)
        {
            throw new NoSuchElementException();
        }

        var value = _nextVertex;
        ComputeNext();
        return value;
    }

    public void Reset()
    {
        throw new NotSupportedException("Reset");
    }

    private void ComputeNext()
    {
        if (_hops >= _maxHops)
        {
            _nextVertex = null;
            return;
        }

        _hops++;
        if (_graph.OutDegreeOf(_nextVertex) == 0)
        {
            _nextVertex = null;
            return;
        }

        var edge = default(TEdge);
        if (_weighted)
        {
            double outEdgesWeight = _outEdgesTotalWeight.computeIfAbsent(_nextVertex,
                node =>
                {
                    return _graph.OutgoingEdgesOf(node).collect(Collectors.summingDouble(_graph.getEdgeWeight));
                }
            );
            var p           = outEdgesWeight * _random.NextDouble();
            var cumulativeP = 0d;
            foreach (var curEdge in _graph.OutgoingEdgesOf(_nextVertex))
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
            var outEdges = new List<TEdge>(_graph.OutgoingEdgesOf(_nextVertex));
            edge = outEdges[_random.Next(outEdges.Count)];
        }

        _nextVertex = Graphs.GetOppositeVertex(_graph, edge, _nextVertex);
    }
}
