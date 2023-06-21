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

using System.Collections.Immutable;
using NGraphT.Core.DotNetUtil;

namespace NGraphT.Core.Graph.Specifics;

/// <summary>
/// Fast implementation of DirectedSpecifics. This class uses additional data structures to improve
/// the performance of methods which depend on edge retrievals, e.g.
/// <c>getEdge(TVertex u, TVertex v)</c>,
/// <c>containsEdge(TVertex u, TVertex v)</c>,
/// <c>addEdge(TVertex u, TVertex v)</c>.
/// A disadvantage is an increase in memory consumption. If
/// memory utilization is an issue, use a <see cref="DirectedSpecifics{TVertex,TEdge}"/> instead.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Joris Kinable.</remarks>
public class FastLookupDirectedSpecifics<TVertex, TEdge> : DirectedSpecifics<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Construct a new fast lookup directed specifics.
    /// </summary>
    /// <param name="graph"> the graph for which these specifics are for.</param>
    /// <param name="vertexMap">
    /// map for the storage of vertex edge sets. Needs to have a predictable iteration order.
    /// </param>
    /// <param name="touchingVerticesToEdgeMap">
    /// Additional map for caching. No need for a predictable iteration order.
    /// </param>
    /// <param name="edgeSetFactory"> factory for the creation of vertex edge sets.</param>
    public FastLookupDirectedSpecifics(
        IGraph<TVertex, TEdge>                                      graph,
        IDictionary<TVertex, DirectedEdgeContainer<TVertex, TEdge>> vertexMap,
        IDictionary<(TVertex U, TVertex V), ISet<TEdge>>            touchingVerticesToEdgeMap,
        IEdgeSetFactory<TVertex, TEdge>                             edgeSetFactory
    )
        : base(graph, vertexMap, edgeSetFactory)
    {
        ArgumentNullException.ThrowIfNull(touchingVerticesToEdgeMap);
        TouchingVerticesToEdgeMap = touchingVerticesToEdgeMap;
    }

    /// <summary>
    /// Maps a pair of vertices &lt;u,TVertex&gt; to a set of edges {(u,TVertex)}. In case of a multigraph, all
    /// edges which touch both u and TVertex are included in the set.
    /// </summary>
    protected IDictionary<(TVertex U, TVertex V), ISet<TEdge>> TouchingVerticesToEdgeMap { get; init; }

    public override ISet<TEdge> GetAllEdges(TVertex? sourceVertex, TVertex? targetVertex)
    {
        if (!Graph.ContainsVertex(sourceVertex) || !Graph.ContainsVertex(targetVertex))
        {
            return ImmutableHashSet<TEdge>.Empty;
        }

        if (!TouchingVerticesToEdgeMap.TryGetValue((U: sourceVertex, V: targetVertex), out var edges))
        {
            return ImmutableHashSet<TEdge>.Empty;
        }

        var edgeSet = EdgeSetFactory.CreateEdgeSet(sourceVertex);
        edgeSet.AddRange(edges);
        return edgeSet;
    }

    public override TEdge? GetEdge(TVertex? sourceVertex, TVertex? targetVertex)
    {
        if (sourceVertex == null ||
            targetVertex == null ||
            !TouchingVerticesToEdgeMap.TryGetValue((U: sourceVertex, V: targetVertex), out var edges))
        {
            return null;
        }

        return edges.FirstOrDefault();
    }

    public override bool AddEdgeToTouchingVertices(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        if (!base.AddEdgeToTouchingVertices(sourceVertex, targetVertex, edge))
        {
            return false;
        }

        AddToIndex(sourceVertex, targetVertex, edge);
        return true;
    }

    public override bool AddEdgeToTouchingVerticesIfAbsent(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        // first lookup using our own index
        if (GetEdge(sourceVertex, targetVertex) != null)
        {
            return false;
        }

        return AddEdgeToTouchingVertices(sourceVertex, targetVertex, edge);
    }

    public override TEdge? CreateEdgeToTouchingVerticesIfAbsent(
        TVertex     sourceVertex,
        TVertex     targetVertex,
        Func<TEdge> edgeSupplier
    )
    {
        ArgumentNullException.ThrowIfNull(edgeSupplier);

        // first lookup using our own index
        if (GetEdge(sourceVertex, targetVertex) != null)
        {
            return null;
        }

        var edge = edgeSupplier();
        AddEdgeToTouchingVertices(sourceVertex, targetVertex, edge);
        return edge;
    }

    public override void RemoveEdgeFromTouchingVertices(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        base.RemoveEdgeFromTouchingVertices(sourceVertex, targetVertex, edge);

        RemoveFromIndex(sourceVertex, targetVertex, edge);
    }

    /// <summary>
    /// Add an edge to the index.
    /// </summary>
    /// <param name="sourceVertex"> the source vertex.</param>
    /// <param name="targetVertex"> the target vertex.</param>
    /// <param name="edge"> the edge.</param>
    protected virtual void AddToIndex(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        var vertexPair = (U: sourceVertex, V: targetVertex);
        if (TouchingVerticesToEdgeMap.TryGetValue(vertexPair, out var edgeSet))
        {
            edgeSet.Add(edge);
        }
        else
        {
            edgeSet = EdgeSetFactory.CreateEdgeSet(sourceVertex);
            edgeSet.Add(edge);
            TouchingVerticesToEdgeMap[vertexPair] = edgeSet;
        }
    }

    /// <summary>
    /// Remove an edge from the index.
    /// </summary>
    /// <param name="sourceVertex"> the source vertex.</param>
    /// <param name="targetVertex"> the target vertex.</param>
    /// <param name="edge"> the edge.</param>
    protected virtual void RemoveFromIndex(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        var vertexPair = (U: sourceVertex, V: targetVertex);
        if (TouchingVerticesToEdgeMap.TryGetValue(vertexPair, out var edgeSet))
        {
            edgeSet.Remove(edge);
            if (edgeSet.Count == 0)
            {
                TouchingVerticesToEdgeMap.Remove(vertexPair);
            }
        }
    }
}
