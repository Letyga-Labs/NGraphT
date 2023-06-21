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
using J2N.Collections.Generic.Extensions;
using NGraphT.Core.DotNetUtil;
using NGraphT.Core.Util;

namespace NGraphT.Core.Graph.Specifics;

/// <summary>
/// Plain implementation of DirectedSpecifics. This implementation requires the least amount of
/// memory, at the expense of slow edge retrievals. Methods which depend on edge retrievals, e.g.
/// <c>getEdge(TVertex u, TVertex v)</c>,
/// <c>containsEdge(TVertex u, TVertex v)</c>,
/// <c>addEdge(TVertex u, TVertex v)</c>,
/// etc may be relatively slow when the average degree of a vertex is high (dense graphs).
/// For a fast implementation, use <see cref="FastLookupDirectedSpecifics{TVertex,TEdge}"/>.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Authors: Barak Naveh, Joris Kinable.</remarks>
public class DirectedSpecifics<TVertex, TEdge> : ISpecifics<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Construct a new directed specifics.
    /// </summary>
    /// <param name="graph"> the graph for which these specifics are for.</param>
    /// <param name="vertexMap">
    /// map for the storage of vertex edge sets. Needs to have a predictable iteration order.
    /// </param>
    /// <param name="edgeSetFactory"> factory for the creation of vertex edge sets.</param>
    public DirectedSpecifics(
        IGraph<TVertex, TEdge>                                      graph,
        IDictionary<TVertex, DirectedEdgeContainer<TVertex, TEdge>> vertexMap,
        IEdgeSetFactory<TVertex, TEdge>                             edgeSetFactory
    )
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(vertexMap);
        ArgumentNullException.ThrowIfNull(edgeSetFactory);

        Graph          = graph;
        VertexMap      = vertexMap;
        EdgeSetFactory = edgeSetFactory;
    }

    /// <inheritdoc/>
    public virtual ISet<TVertex> VertexSet => VertexMap.Keys.ToHashSet();

    protected IGraph<TVertex, TEdge> Graph { get; init; }

    protected IEdgeSetFactory<TVertex, TEdge> EdgeSetFactory { get; init; }

    protected IDictionary<TVertex, DirectedEdgeContainer<TVertex, TEdge>> VertexMap { get; init; }

    /// <inheritdoc/>
    public virtual bool AddVertex(TVertex vertex)
    {
        if (!VertexMap.ContainsKey(vertex))
        {
            VertexMap[vertex] = new DirectedEdgeContainer<TVertex, TEdge>(EdgeSetFactory, vertex);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> GetAllEdges(TVertex? sourceVertex, TVertex? targetVertex)
    {
        if (!Graph.ContainsVertex(sourceVertex) || !Graph.ContainsVertex(targetVertex))
        {
            return ImmutableHashSet<TEdge>.Empty;
        }

        var edges = (ISet<TEdge>)new ArrayUnenforcedSet<TEdge>();

        var ec = GetEdgeContainer(sourceVertex);

        foreach (var edge in ec.Outgoing)
        {
            if (Graph.GetEdgeTarget(edge).Equals(targetVertex))
            {
                edges.Add(edge);
            }
        }

        return ImmutableHashSet<TEdge>.Empty;
    }

    /// <inheritdoc/>
    public virtual TEdge? GetEdge(TVertex? sourceVertex, TVertex? targetVertex)
    {
        if (!Graph.ContainsVertex(sourceVertex) || !Graph.ContainsVertex(targetVertex))
        {
            return null;
        }

        var ec = GetEdgeContainer(sourceVertex);

        foreach (var edge in ec.Outgoing)
        {
            if (Graph.GetEdgeTarget(edge).Equals(targetVertex))
            {
                return edge;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public virtual bool AddEdgeToTouchingVertices(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        GetEdgeContainer(sourceVertex).AddOutgoingEdge(edge);
        GetEdgeContainer(targetVertex).AddIncomingEdge(edge);
        return true;
    }

    public virtual bool AddEdgeToTouchingVerticesIfAbsent(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        // lookup for edge with same source and target
        var ec = GetEdgeContainer(sourceVertex);
        foreach (var outEdge in ec.Outgoing)
        {
            if (Graph.GetEdgeTarget(outEdge).Equals(targetVertex))
            {
                return false;
            }
        }

        // add
        ec.AddOutgoingEdge(edge);
        GetEdgeContainer(targetVertex).AddIncomingEdge(edge);

        return true;
    }

    public virtual TEdge? CreateEdgeToTouchingVerticesIfAbsent(
        TVertex     sourceVertex,
        TVertex     targetVertex,
        Func<TEdge> edgeSupplier
    )
    {
        ArgumentNullException.ThrowIfNull(edgeSupplier);

        // lookup for edge with same source and target
        var ec = GetEdgeContainer(sourceVertex);
        foreach (var e in ec.Outgoing)
        {
            if (Graph.GetEdgeTarget(e).Equals(targetVertex))
            {
                return null;
            }
        }

        // create and add
        var edge = edgeSupplier();
        ec.AddOutgoingEdge(edge);
        GetEdgeContainer(targetVertex).AddIncomingEdge(edge);

        return edge;
    }

    /// <inheritdoc/>
    public virtual int DegreeOf(TVertex vertex)
    {
        return InDegreeOf(vertex) + OutDegreeOf(vertex);
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> EdgesOf(TVertex vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        var inAndOut = (ISet<TEdge>)new ArrayUnenforcedSet<TEdge>(GetEdgeContainer(vertex).Incoming);

        if (Graph.Type.IsAllowingSelfLoops)
        {
            foreach (var edge in GetEdgeContainer(vertex).Outgoing)
            {
                var target = Graph.GetEdgeTarget(edge);
                if (!vertex.Equals(target))
                {
                    inAndOut.Add(edge);
                }
            }
        }
        else
        {
            inAndOut.AddRange(GetEdgeContainer(vertex).Outgoing);
        }

        return inAndOut.AsReadOnly();
    }

    /// <inheritdoc/>
    public virtual int InDegreeOf(TVertex vertex)
    {
        return GetEdgeContainer(vertex).Incoming.Count;
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> IncomingEdgesOf(TVertex vertex)
    {
        return GetEdgeContainer(vertex).UnmodifiableIncomingEdges;
    }

    /// <inheritdoc/>
    public virtual int OutDegreeOf(TVertex vertex)
    {
        return GetEdgeContainer(vertex).Outgoing.Count;
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> OutgoingEdgesOf(TVertex vertex)
    {
        return GetEdgeContainer(vertex).UnmodifiableOutgoingEdges;
    }

    /// <inheritdoc/>
    public virtual void RemoveEdgeFromTouchingVertices(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        GetEdgeContainer(sourceVertex).RemoveOutgoingEdge(edge);
        GetEdgeContainer(targetVertex).RemoveIncomingEdge(edge);
    }

    /// <summary>
    /// Get the edge container for specified vertex.
    /// </summary>
    /// <param name="vertex"> a vertex in this graph.</param>
    /// <returns>an edge container.</returns>
    protected virtual DirectedEdgeContainer<TVertex, TEdge> GetEdgeContainer(TVertex vertex)
    {
        if (!VertexMap.TryGetValue(vertex, out var ec))
        {
            ec                = new DirectedEdgeContainer<TVertex, TEdge>(EdgeSetFactory, vertex);
            VertexMap[vertex] = ec;
        }

        return ec;
    }
}
