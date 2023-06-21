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
using System.Diagnostics.CodeAnalysis;
using NGraphT.Core.Util;
using Java2Net = J2N.Collections.Generic;

namespace NGraphT.Core.Traverse;

/// <summary>
/// A maximum cardinality search iterator for an undirected graph.
///
/// <para>
/// For every vertex in graph its cardinality is defined as the number of its neighbours, which have
/// been already visited by this iterator. Iterator chooses vertex with the maximum cardinality,
/// breaking ties arbitrarily. For more information of maximum cardinality search see: Berry, A.,
/// Blair, J., Heggernes, P. et al. Algorithmica (2004) 39: 287.
/// https://doi.org/10.1007/s00453-004-1084-3
/// <a href="http://www.ii.uib.no/~pinar/MCS-M.pdf"><i>Maximum Cardinality Search for Computing
/// Minimal Triangulations</i></a>.
/// </para>
///
/// <para>
/// For this iterator to work correctly the graph must not be modified during iteration. Currently
/// there are no means to ensure that, nor to fail-fast. The results of such modifications are
/// undefined.
/// </para>
///
/// <para>
/// Note: only vertex events are fired by this iterator.
/// </para>
///
/// </summary>
///
/// <typeparam name="TVertex"> the graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>.
///
/// <remarks>Author: Timofey Chudakov.</remarks>
public sealed class MaximumCardinalityIterator<TVertex, TEdge> : AbstractGraphIterator<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Disjoint sets of vertices of the graph, indexed by the cardinalities of already visited
    /// neighbours.
    /// </summary>
    private readonly List<ISet<TVertex>?> _buckets = new();

    /// <summary>
    /// Maps every vertex to its cardinality.
    /// </summary>
    private readonly IDictionary<TVertex, int> _cardinalityMap;

    /// <summary>
    /// The maximum index of non-empty set in <c>buckets</c>.
    /// </summary>
    private int _maxCardinality;

    /// <summary>
    /// Number of unvisited vertices.
    /// </summary>
    private int _remainingVertices;

    /// <summary>
    /// Contains current vertex.
    /// </summary>
    private TVertex? _current;

    /// <summary>
    /// Creates a maximum cardinality iterator for the <c>graph</c>.
    /// </summary>
    /// <param name="graph"> the graph to be iterated.</param>
    public MaximumCardinalityIterator(IGraph<TVertex, TEdge> graph)
        : base(graph)
    {
        // TODO: GraphTests.RequireUndirected(graph);
        _remainingVertices = graph.VertexSet().Count;
        if (_remainingVertices > 0)
        {
            _buckets[0]     = new Java2Net.LinkedHashSet<TVertex>(graph.VertexSet());
            _cardinalityMap = CollectionUtil.NewHashMapWithExpectedSize<TVertex, int>(graph.VertexSet().Count);
            foreach (var v in graph.VertexSet())
            {
                _cardinalityMap[v] = 0;
            }
        }
        else
        {
            _cardinalityMap = new Dictionary<TVertex, int>();
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para>
    /// Always returns true since this iterator doesn't care about connected components.
    /// </para>
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

    /// <summary>
    /// Retrieves a vertex from the <c>buckets</c> with the maximum cardinality and returns it.
    /// </summary>
    /// <returns>vertex retrieved from <c>buckets</c>.</returns>
    private TVertex? Advance()
    {
        if (_remainingVertices <= 0)
        {
            return null;
        }

        var bucket = _buckets[_maxCardinality];
        Debug.Assert(bucket != null, $"{nameof(bucket)} != null");

        var vertex = bucket.First();
        RemoveFromBucket(vertex);
        if (bucket.Count == 0)
        {
            _buckets[_maxCardinality] = null;
            _maxCardinality--;
            while (_maxCardinality >= 0 && _buckets[_maxCardinality] == null)
            {
                _maxCardinality--;
            }
        }

        UpdateNeighbours(vertex);
        _remainingVertices--;
        return vertex;
    }

    /// <summary>
    /// Removes <c>vertex</c> from the bucket it was contained in.
    /// </summary>
    /// <param name="vertex"> the vertex, which has to be removed from the bucket it was contained in.</param>
    /// <returns>the cardinality of the removed vertex or -1, if the vertex wasn't contained in any
    ///         bucket.</returns>
    private int RemoveFromBucket(TVertex vertex)
    {
        if (_cardinalityMap.ContainsKey(vertex))
        {
            var cardinality = _cardinalityMap[vertex];
            var bucket      = _buckets[cardinality]!;
            bucket.Remove(vertex);
            _cardinalityMap.Remove(vertex);
            if (bucket.Count == 0)
            {
                _buckets[cardinality] = null;
            }

            return cardinality;
        }

        return -1;
    }

    /// <summary>
    /// Adds the <c>vertex</c> to the bucket with the given <c>cardinality</c>.
    /// </summary>
    /// <param name="vertex"> the vertex, which has to be added to the the bucket.</param>
    /// <param name="cardinality"> the cardinality of the destination bucket.</param>
    private void AddToBucket(TVertex vertex, int cardinality)
    {
        _cardinalityMap[vertex] = cardinality;

        _buckets[cardinality] ??= new Java2Net.LinkedHashSet<TVertex>();
        _buckets[cardinality]!.Add(vertex);
    }

    /// <summary>
    /// Increments the cardinalities of the neighbours of the <c>vertex</c> by 1. If the maximum
    /// cardinality increases, increments <c>maxCardinality</c> by 1.
    /// </summary>
    /// <param name="vertex"> the vertex whose neighbours are to be updated.</param>
    private void UpdateNeighbours(TVertex vertex)
    {
        var processed = new HashSet<TVertex>();
        foreach (var edge in Graph.EdgesOf(vertex))
        {
            var opposite = Graphs.GetOppositeVertex(Graph, edge, vertex);
            if (_cardinalityMap.ContainsKey(opposite) && !processed.Contains(opposite))
            {
                processed.Add(opposite);
                AddToBucket(opposite, RemoveFromBucket(opposite) + 1);
            }
        }

        if (_maxCardinality < Graph.VertexSet().Count && _maxCardinality >= 0 && _buckets[_maxCardinality + 1] != null)
        {
            _maxCardinality++;
        }
    }
}
