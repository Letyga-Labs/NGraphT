/*
 * (C) Copyright 2015-2021, by Barak Naveh and Contributors.
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

namespace NGraphT.Core.Graph.Specifics;

using Core;
using Graph;
using Util;

/// <summary>
/// Plain implementation of DirectedSpecifics. This implementation requires the least amount of
/// memory, at the expense of slow edge retrievals. Methods which depend on edge retrievals, TEdge.g.
/// getEdge(TNode u, TNode TNode), containsEdge(TNode u, TNode TNode), addEdge(TNode u, TNode TNode), etc may be relatively slow when the
/// average degree of a vertex is high (dense graphs). For a fast implementation, use
/// <see cref="FastLookupDirectedSpecifics"/>.
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
/// <remarks>Author: Joris Kinable.</remarks>
public class DirectedSpecifics<TNode, TEdge> : ISpecifics<TNode, TEdge>
{
    protected internal IGraph<TNode, TEdge>                                    Graph;
    protected internal IDictionary<TNode, DirectedEdgeContainer<TNode, TEdge>> VertexMap;
    protected internal IEdgeSetFactory<TNode, TEdge>                           EdgeSetFactory;

    /// <summary>
    /// Construct a new directed specifics.
    /// </summary>
    /// <param name="graph"> the graph for which these specifics are for.</param>
    /// <param name="vertexMap"> map for the storage of vertex edge sets. Needs to have a predictable
    ///        iteration order.</param>
    /// <param name="edgeSetFactory"> factory for the creation of vertex edge sets.</param>
    public DirectedSpecifics(
        IGraph<TNode, TEdge>                                    graph,
        IDictionary<TNode, DirectedEdgeContainer<TNode, TEdge>> vertexMap,
        IEdgeSetFactory<TNode, TEdge>                           edgeSetFactory
    )
    {
        Graph          = Objects.requireNonNull(graph);
        VertexMap      = Objects.requireNonNull(vertexMap);
        EdgeSetFactory = Objects.requireNonNull(edgeSetFactory);
    }

    /// <inheritdoc/>
    public virtual bool AddVertex(TNode node)
    {
        DirectedEdgeContainer<TNode, TEdge> ec = VertexMap[node];
        if (ec == null)
        {
            VertexMap[node] = new DirectedEdgeContainer<TNode, TEdge>(EdgeSetFactory, node);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public virtual ISet<TNode> VertexSet
    {
        get
        {
            return VertexMap.Keys;
        }
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> GetAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        ISet<TEdge> edges = null;

        if (Graph.ContainsVertex(sourceVertex) && Graph.ContainsVertex(targetVertex))
        {
            edges = new ArrayUnenforcedSet<TEdge>();

            DirectedEdgeContainer<TNode, TEdge> ec = GetEdgeContainer(sourceVertex);

            foreach (var edge in ec.Outgoing)
            {
                if (Graph.GetEdgeTarget(edge).Equals(targetVertex))
                {
                    edges.Add(edge);
                }
            }
        }

        return edges;
    }

    /// <inheritdoc/>
    public virtual TEdge GetEdge(TNode sourceVertex, TNode targetVertex)
    {
        if (Graph.ContainsVertex(sourceVertex) && Graph.ContainsVertex(targetVertex))
        {
            DirectedEdgeContainer<TNode, TEdge> ec = GetEdgeContainer(sourceVertex);

            foreach (var edge in ec.Outgoing)
            {
                if (Graph.GetEdgeTarget(edge).Equals(targetVertex))
                {
                    return edge;
                }
            }
        }

        return default(TEdge);
    }

    /// <inheritdoc/>
    public virtual bool AddEdgeToTouchingVertices(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        GetEdgeContainer(sourceVertex).AddOutgoingEdge(edge);
        GetEdgeContainer(targetVertex).AddIncomingEdge(edge);
        return true;
    }

    public virtual bool AddEdgeToTouchingVerticesIfAbsent(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        // lookup for edge with same source and target
        DirectedEdgeContainer<TNode, TEdge> ec = GetEdgeContainer(sourceVertex);
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

    public virtual TEdge CreateEdgeToTouchingVerticesIfAbsent(
        TNode              sourceVertex,
        TNode              targetVertex,
        Func<TEdge> edgeSupplier
    )
    {
        // lookup for edge with same source and target
        DirectedEdgeContainer<TNode, TEdge> ec = GetEdgeContainer(sourceVertex);
        foreach (var edge in ec.Outgoing)
        {
            if (Graph.GetEdgeTarget(edge).Equals(targetVertex))
            {
                return default(TEdge);
            }
        }

        // create and add
        var edge = edgeSupplier();
        ec.AddOutgoingEdge(edge);
        GetEdgeContainer(targetVertex).AddIncomingEdge(edge);

        return edge;
    }

    /// <inheritdoc/>
    public virtual int DegreeOf(TNode vertex)
    {
        return InDegreeOf(vertex) + OutDegreeOf(vertex);
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> EdgesOf(TNode vertex)
    {
        ArrayUnenforcedSet<TEdge> inAndOut = new ArrayUnenforcedSet<TEdge>(GetEdgeContainer(vertex).Incoming);

        if (Graph.Type.AllowingSelfLoops)
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

        return Collections.unmodifiableSet(inAndOut);
    }

    /// <inheritdoc/>
    public virtual int InDegreeOf(TNode vertex)
    {
        return GetEdgeContainer(vertex).Incoming.Count;
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> IncomingEdgesOf(TNode vertex)
    {
        return GetEdgeContainer(vertex).UnmodifiableIncomingEdges;
    }

    /// <inheritdoc/>
    public virtual int OutDegreeOf(TNode vertex)
    {
        return GetEdgeContainer(vertex).Outgoing.Count;
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> OutgoingEdgesOf(TNode vertex)
    {
        return GetEdgeContainer(vertex).UnmodifiableOutgoingEdges;
    }

    /// <inheritdoc/>
    public virtual void RemoveEdgeFromTouchingVertices(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        GetEdgeContainer(sourceVertex).RemoveOutgoingEdge(edge);
        GetEdgeContainer(targetVertex).RemoveIncomingEdge(edge);
    }

    /// <summary>
    /// Get the edge container for specified vertex.
    /// </summary>
    /// <param name="vertex"> a vertex in this graph.</param>
    /// <returns>an edge container.</returns>
    protected internal virtual DirectedEdgeContainer<TNode, TEdge> GetEdgeContainer(TNode vertex)
    {
        DirectedEdgeContainer<TNode, TEdge> ec = VertexMap[vertex];

        if (ec == null)
        {
            ec                = new DirectedEdgeContainer<TNode, TEdge>(EdgeSetFactory, vertex);
            VertexMap[vertex] = ec;
        }

        return ec;
    }
}
