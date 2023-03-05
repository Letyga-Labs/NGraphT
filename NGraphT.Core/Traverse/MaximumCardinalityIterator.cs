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

namespace NGraphT.Core.Traverse;

using Core;
using Util;

/// <summary>
/// A maximum cardinality search iterator for an undirected graph.
/// <para>
/// For every vertex in graph its cardinality is defined as the number of its neighbours, which have
/// been already visited by this iterator. Iterator chooses vertex with the maximum cardinality,
/// breaking ties arbitrarily. For more information of maximum cardinality search see: Berry, A.,
/// Blair, J., Heggernes, P. et al. Algorithmica (2004) 39: 287.
/// https://doi.org/10.1007/s00453-004-1084-3
/// <a href="http://www.ii.uib.no/~pinar/MCS-M.pdf"><i>Maximum Cardinality Search for Computing
/// Minimal Triangulations</i></a>.
/// </para>
/// <para>
/// For this iterator to work correctly the graph must not be modified during iteration. Currently
/// there are no means to ensure that, nor to fail-fast. The results of such modifications are
/// undefined.
/// </para>
/// <para>
/// Note: only vertex events are fired by this iterator.
/// </para>
/// </summary>
/// @param <TNode> the graph vertex type.</param>
/// <typeparam name="TEdge">The graph edge type.</typeparam>.
/// <remarks>Author: Timofey Chudakov.</remarks>
public class MaximumCardinalityIterator<TNode, TEdge> : AbstractGraphIterator<TNode, TEdge>
{
    /// <summary>
    /// The maximum index of non-empty set in {@code buckets}.
    /// </summary>
    private int _maxCardinality;

    /// <summary>
    /// Number of unvisited vertices.
    /// </summary>
    private int _remainingVertices;

    /// <summary>
    /// Contains current vertex.
    /// </summary>
    private TNode _current;

    /// <summary>
    /// Disjoint sets of vertices of the graph, indexed by the cardinalities of already visited
    /// neighbours.
    /// </summary>
    private List<ISet<TNode>> _buckets;

    /// <summary>
    /// Maps every vertex to its cardinality.
    /// </summary>
    private IDictionary<TNode, int> _cardinalityMap;

    /// <summary>
    /// Creates a maximum cardinality iterator for the {@code graph}.
    /// </summary>
    /// <param name="graph"> the graph to be iterated.</param>
    public MaximumCardinalityIterator(IGraph<TNode, TEdge> graph)
        : base(graph)
    {
        _remainingVertices = graph.VertexSet().Count;
        if (_remainingVertices > 0)
        {
            GraphTests.RequireUndirected(graph);
            _buckets        = new List<ISet<TNode>>(Collections.nCopies(graph.VertexSet().Count, null));
            _buckets[0]     = new LinkedHashSet<TNode>(graph.VertexSet());
            _cardinalityMap = CollectionUtil.NewHashMapWithExpectedSize(graph.VertexSet().Count);
            foreach (var node in graph.VertexSet())
            {
                _cardinalityMap[node] = 0;
            }

            _maxCardinality = 0;
        }
    }

    /// <summary>
    /// Checks whether there exists unvisited vertex.
    /// </summary>
    /// <returns>true if there exists unvisited vertex.</returns>
    public override bool HasNext()
    {
        if (_current != null)
        {
            return true;
        }

        _current = Advance();
        if (_current != null && NListeners != 0)
        {
            FireVertexTraversed(CreateVertexTraversalEvent(_current));
        }

        return _current != null;
    }

    /// <summary>
    /// Returns the next vertex in the ordering.
    /// </summary>
    /// <returns>the next vertex in the ordering.</returns>
    public override TNode Next()
    {
        if (!HasNext())
        {
            throw new NoSuchElementException();
        }

        var result = _current;
        _current = default(TNode);
        if (NListeners != 0)
        {
            FireVertexFinished(CreateVertexTraversalEvent(result));
        }

        return result;
    }

    /// <summary>
    /// {@inheritDoc}
    /// <para>
    /// Always returns true since this iterator doesn't care about connected components.
    /// </para>
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


    /// <summary>
    /// Retrieves a vertex from the {@code buckets} with the maximum cardinality and returns it.
    /// </summary>
    /// <returns>vertex retrieved from {@code buckets}.</returns>
    private TNode Advance()
    {
        if (_remainingVertices > 0)
        {
            ISet<TNode> bucket = _buckets[_maxCardinality];
            TNode       vertex = bucket.GetEnumerator().next();
            RemoveFromBucket(vertex);
            if (bucket.Count == 0)
            {
                _buckets[_maxCardinality] = null;
                do
                {
                    --_maxCardinality;
                } while (_maxCardinality >= 0 && _buckets[_maxCardinality] == null);
            }

            UpdateNeighbours(vertex);
            --_remainingVertices;
            return vertex;
        }
        else
        {
            return default(TNode);
        }
    }

    /// <summary>
    /// Removes {@code vertex} from the bucket it was contained in.
    /// </summary>
    /// <param name="vertex"> the vertex, which has to be removed from the bucket it was contained in.</param>
    /// <returns>the cardinality of the removed vertex or -1, if the vertex wasn't contained in any
    ///         bucket.</returns>
    private int RemoveFromBucket(TNode vertex)
    {
        if (_cardinalityMap.ContainsKey(vertex))
        {
            var cardinality = _cardinalityMap[vertex];
            _buckets[cardinality].remove(vertex);
            _cardinalityMap.Remove(vertex);
            if (_buckets[cardinality].Count == 0)
            {
                _buckets[cardinality] = null;
            }

            return cardinality;
        }

        return -1;
    }

    /// <summary>
    /// Adds the {@code vertex} to the bucket with the given {@code cardinality}.
    /// </summary>
    /// <param name="vertex"> the vertex, which has to be added to the the bucket.</param>
    /// <param name="cardinality"> the cardinality of the destination bucket.</param>
    private void AddToBucket(TNode vertex, int cardinality)
    {
        _cardinalityMap[vertex] = cardinality;
        if (_buckets[cardinality] == null)
        {
            _buckets[cardinality] = new LinkedHashSet<TNode>();
        }

        _buckets[cardinality].Add(vertex);
    }

    /// <summary>
    /// Increments the cardinalities of the neighbours of the {@code vertex} by 1. If the maximum
    /// cardinality increases, increments {@code maxCardinality} by 1.
    /// </summary>
    /// <param name="vertex"> the vertex whose neighbours are to be updated.</param>
    private void UpdateNeighbours(TNode vertex)
    {
        ISet<TNode> processed = new HashSet<TNode>();
        foreach (TEdge edge in graph.EdgesOf(vertex))
        {
            var opposite = Graphs.GetOppositeVertex(graph, edge, vertex);
            if (_cardinalityMap.ContainsKey(opposite) && !processed.Contains(opposite))
            {
                processed.Add(opposite);
                AddToBucket(opposite, RemoveFromBucket(opposite) + 1);
            }
        }

        if (_maxCardinality < graph.VertexSet().Count && _maxCardinality >= 0 && _buckets[_maxCardinality + 1] != null)
        {
            ++_maxCardinality;
        }
    }
}
