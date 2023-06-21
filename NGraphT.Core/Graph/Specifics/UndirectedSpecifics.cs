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
using System.Diagnostics.CodeAnalysis;
using NGraphT.Core.Util;

namespace NGraphT.Core.Graph.Specifics;

/// <summary>
/// Plain implementation of UndirectedSpecifics. This implementation requires the least amount of
/// memory, at the expense of slow edge retrievals. Methods which depend on edge retrievals, e.g.
/// <c>getEdge(TVertex u, TVertex v)</c>,
/// <c>containsEdge(TVertex u, TVertex v)</c>,
/// <c>addEdge(TVertex u, TVertex v)</c>,
/// etc may be relatively slow when the average degree of a vertex is high (dense graphs).
/// For a fast implementation, use <see cref="FastLookupUndirectedSpecifics{TVertex,TEdge}"/>.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Authors: Barak Naveh, Joris Kinable.</remarks>
public class UndirectedSpecifics<TVertex, TEdge> : ISpecifics<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Construct a new undirected specifics.
    /// </summary>
    /// <param name="graph"> the graph for which these specifics are for.</param>
    /// <param name="vertexMap">
    /// map for the storage of vertex edge sets. Needs to have a predictable iteration order.
    /// </param>
    /// <param name="edgeSetFactory"> factory for the creation of vertex edge sets.</param>
    public UndirectedSpecifics(
        IGraph<TVertex, TEdge>                                        graph,
        IDictionary<TVertex, UndirectedEdgeContainer<TVertex, TEdge>> vertexMap,
        IEdgeSetFactory<TVertex, TEdge>                               edgeSetFactory
    )
    {
        Graph          = graph;
        VertexMap      = vertexMap;
        EdgeSetFactory = edgeSetFactory;
    }

    /// <inheritdoc/>
    public virtual ISet<TVertex> VertexSet => VertexMap.Keys.ToHashSet();

    protected IGraph<TVertex, TEdge> Graph { get; init; }

    protected IDictionary<TVertex, UndirectedEdgeContainer<TVertex, TEdge>> VertexMap { get; init; }

    protected IEdgeSetFactory<TVertex, TEdge> EdgeSetFactory { get; init; }

    /// <inheritdoc/>
    public virtual bool AddVertex(TVertex vertex)
    {
        if (!VertexMap.ContainsKey(vertex))
        {
            VertexMap[vertex] = new UndirectedEdgeContainer<TVertex, TEdge>(EdgeSetFactory, vertex);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
    public virtual ISet<TEdge> GetAllEdges(TVertex? sourceVertex, TVertex? targetVertex)
    {
        if (!Graph.ContainsVertex(sourceVertex) || !Graph.ContainsVertex(targetVertex))
        {
            return ImmutableHashSet<TEdge>.Empty;
        }

        var edges = (ISet<TEdge>)new ArrayUnenforcedSet<TEdge>();

        foreach (var edge in GetEdgeContainer(sourceVertex).VertexEdges)
        {
            if (IsEqualsStraightOrInverted(sourceVertex, targetVertex, edge))
            {
                edges.Add(edge);
            }
        }

        return edges;
    }

    /// <inheritdoc/>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
    public virtual TEdge? GetEdge(TVertex? sourceVertex, TVertex? targetVertex)
    {
        if (!Graph.ContainsVertex(sourceVertex) || !Graph.ContainsVertex(targetVertex))
        {
            return null;
        }

        foreach (var edge in GetEdgeContainer(sourceVertex).VertexEdges)
        {
            if (IsEqualsStraightOrInverted(sourceVertex, targetVertex, edge))
            {
                return edge;
            }
        }

        return null;
    }

    public virtual bool AddEdgeToTouchingVertices(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        ArgumentNullException.ThrowIfNull(sourceVertex);

        GetEdgeContainer(sourceVertex).AddEdge(edge);

        if (!sourceVertex.Equals(targetVertex))
        {
            GetEdgeContainer(targetVertex).AddEdge(edge);
        }

        return true;
    }

    public virtual bool AddEdgeToTouchingVerticesIfAbsent(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        ArgumentNullException.ThrowIfNull(sourceVertex);
        ArgumentNullException.ThrowIfNull(targetVertex);

        // lookup for edge with same source and target
        var ec = GetEdgeContainer(sourceVertex);
        foreach (var e in ec.VertexEdges)
        {
            if (IsEqualsStraightOrInverted(sourceVertex, targetVertex, e))
            {
                return false;
            }
        }

        // add
        ec.AddEdge(edge);
        GetEdgeContainer(targetVertex).AddEdge(edge);
        return true;
    }

    public virtual TEdge? CreateEdgeToTouchingVerticesIfAbsent(
        TVertex     sourceVertex,
        TVertex     targetVertex,
        Func<TEdge> edgeSupplier
    )
    {
        ArgumentNullException.ThrowIfNull(sourceVertex);
        ArgumentNullException.ThrowIfNull(targetVertex);
        ArgumentNullException.ThrowIfNull(edgeSupplier);

        // lookup for edge with same source and target
        var ec = GetEdgeContainer(sourceVertex);
        foreach (var e in ec.VertexEdges)
        {
            if (IsEqualsStraightOrInverted(sourceVertex, targetVertex, e))
            {
                return null;
            }
        }

        // create and add
        var edge = edgeSupplier();
        ec.AddEdge(edge);
        GetEdgeContainer(targetVertex).AddEdge(edge);

        return edge;
    }

    /// <inheritdoc/>
    public virtual int DegreeOf(TVertex vertex)
    {
        if (Graph.Type.IsAllowingSelfLoops)
        {
            // Then we must count, and add loops twice
            var degree = 0;
            var edges  = GetEdgeContainer(vertex).VertexEdges;

            foreach (var edge in edges)
            {
                if (Graph.GetEdgeSource(edge).Equals(Graph.GetEdgeTarget(edge)))
                {
                    degree += 2;
                }
                else
                {
                    degree += 1;
                }
            }

            return degree;
        }
        else
        {
            return GetEdgeContainer(vertex).EdgeCount();
        }
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> EdgesOf(TVertex vertex)
    {
        return GetEdgeContainer(vertex).UnmodifiableVertexEdges;
    }

    /// <inheritdoc/>
    public virtual int InDegreeOf(TVertex vertex)
    {
        return DegreeOf(vertex);
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> IncomingEdgesOf(TVertex vertex)
    {
        return GetEdgeContainer(vertex).UnmodifiableVertexEdges;
    }

    /// <inheritdoc/>
    public virtual int OutDegreeOf(TVertex vertex)
    {
        return DegreeOf(vertex);
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> OutgoingEdgesOf(TVertex vertex)
    {
        return GetEdgeContainer(vertex).UnmodifiableVertexEdges;
    }

    /// <inheritdoc/>
    public virtual void RemoveEdgeFromTouchingVertices(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        ArgumentNullException.ThrowIfNull(sourceVertex);

        GetEdgeContainer(sourceVertex).RemoveEdge(edge);

        if (!sourceVertex.Equals(targetVertex))
        {
            GetEdgeContainer(targetVertex).RemoveEdge(edge);
        }
    }

    /// <summary>
    /// Get the edge container for a specified vertex.
    /// </summary>
    /// <param name="vertex"> a vertex in this graph.</param>>
    /// <returns>an edge container.</returns>
    protected virtual UndirectedEdgeContainer<TVertex, TEdge> GetEdgeContainer(TVertex vertex)
    {
        if (!VertexMap.TryGetValue(vertex, out var ec))
        {
            ec                = new UndirectedEdgeContainer<TVertex, TEdge>(EdgeSetFactory, vertex);
            VertexMap[vertex] = ec;
        }

        return ec;
    }

    private bool IsEqualsStraightOrInverted(object sourceVertex, object targetVertex, TEdge edge)
    {
        var equalStraight =
            sourceVertex.Equals(Graph.GetEdgeSource(edge)) && targetVertex.Equals(Graph.GetEdgeTarget(edge));

        var equalInverted =
            sourceVertex.Equals(Graph.GetEdgeTarget(edge)) && targetVertex.Equals(Graph.GetEdgeSource(edge));

        return equalStraight || equalInverted;
    }
}
