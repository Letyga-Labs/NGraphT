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

namespace NGraphT.Core.Traverse;

using Core;
using Graphs = Graphs;

/// <summary>
/// A random walk iterator.
/// 
/// "Given a graph and a starting point, we select a neighbor of it at random, and move to this
/// neighbor; then we select a neighbor of this point at random, and move to it etc. The (random)
/// sequence of points selected this way is a random walk on the graph." This very simple definition,
/// together with a comprehensive survey can be found at: "Lovász, L. (1993). Random walks on graphs:
/// A survey. Combinatorics, Paul erdos is eighty, 2(1), 1-46."
/// 
/// In its default variant the probability of selecting an outgoing edge is one over the (out) degree
/// of the vertex. In case the user requests a weighted walk, then the probability of each edge is
/// equal to its weight divided by the total weight of all outgoing edges. The walk can also be
/// bounded by a maximum number of hops (edges traversed). The iterator returns
/// <see cref="NoSuchElementException"/> when this bound is reached.
///
/// <remarks>Author: Dimitrios Michail.</remarks>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
public class RandomWalkVertexIterator<TNode, TEdge> : IEnumerator<TNode>
{
    private readonly Random                     _rng;
    private readonly IGraph<TNode, TEdge>       _graph;
    private readonly bool                       _weighted;
    private readonly IDictionary<TNode, double> _outEdgesTotalWeight;
    private readonly long                       _maxHops;
    private          long                       _hops;
    private          TNode                      _nextVertex;

    /// <summary>
    /// Create a new iterator
    /// </summary>
    /// <param name="graph"> the graph.</param>
    /// <param name="vertex"> the starting vertex.</param>
    public RandomWalkVertexIterator(IGraph<TNode, TEdge> graph, TNode vertex)
        : this(graph, vertex, long.MaxValue, false, new Random())
    {
    }

    /// <summary>
    /// Create a new iterator
    /// </summary>
    /// <param name="graph"> the graph.</param>
    /// <param name="vertex"> the starting vertex.</param>
    /// <param name="maxHops"> maximum hops to perform during the walk.</param>
    public RandomWalkVertexIterator(IGraph<TNode, TEdge> graph, TNode vertex, long maxHops)
        : this(graph, vertex, maxHops, false, new Random())
    {
    }

    /// <summary>
    /// Create a new iterator
    /// </summary>
    /// <param name="graph"> the graph.</param>
    /// <param name="vertex"> the starting vertex.</param>
    /// <param name="maxHops"> maximum hops to perform during the walk.</param>
    /// <param name="weighted"> whether to perform a weighted random walk (compute probabilities based on the
    ///        edge weights) </param>
    /// <param name="rng"> the random number generator.</param>
    public RandomWalkVertexIterator(IGraph<TNode, TEdge> graph, TNode vertex, long maxHops, bool weighted, Random rng)
    {
        _graph               = Objects.requireNonNull(graph);
        _weighted            = weighted;
        _outEdgesTotalWeight = new Dictionary<TNode, double>();
        _hops                = 0;
        _nextVertex          = Objects.requireNonNull(vertex);
        if (!graph.ContainsVertex(vertex))
        {
            throw new ArgumentException("Random walk must start at a graph vertex");
        }

        _maxHops = maxHops;
        _rng     = rng;
    }

    public override bool HasNext()
    {
        return _nextVertex != null;
    }

    public override TNode Next()
    {
        if (_nextVertex == null)
        {
            throw new NoSuchElementException();
        }

        var value = _nextVertex;
        ComputeNext();
        return value;
    }

    private void ComputeNext()
    {
        if (_hops >= _maxHops)
        {
            _nextVertex = default(TNode);
            return;
        }

        _hops++;
        if (_graph.OutDegreeOf(_nextVertex) == 0)
        {
            _nextVertex = default(TNode);
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
            var p           = outEdgesWeight * _rng.NextDouble();
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
            IList<TEdge> outEdges = new List<TEdge>(_graph.OutgoingEdgesOf(_nextVertex));
            edge = outEdges[_rng.Next(outEdges.Count)];
        }

        _nextVertex = Graphs.GetOppositeVertex(_graph, edge, _nextVertex);
    }
}
