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
/// Plain implementation of UndirectedSpecifics. This implementation requires the least amount of
/// memory, at the expense of slow edge retrievals. Methods which depend on edge retrievals, TEdge.g.
/// getEdge(TNode u, TNode TNode), containsEdge(TNode u, TNode TNode), addEdge(TNode u, TNode TNode), etc may be relatively slow when the
/// average degree of a vertex is high (dense graphs). For a fast implementation, use
/// <see cref="FastLookupUndirectedSpecifics"/>.
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
/// <remarks>Author: Joris Kinable.</remarks>
public class UndirectedSpecifics<TNode, TEdge> : ISpecifics<TNode, TEdge>
{
    protected IGraph<TNode, TEdge>                                      Graph;
    protected IDictionary<TNode, UndirectedEdgeContainer<TNode, TEdge>> VertexMap;
    protected IEdgeSetFactory<TNode, TEdge>                             EdgeSetFactory;

    ///<summary>
    ///Construct a new undirected specifics.
    ///</summary>
    ///<param name="graph"> the graph for which these specifics are for.</param>
    ///<param name="vertexMap"> map for the storage of vertex edge sets. Needs to have a predictable
    ///       iteration order.</param>
    ///<param name="edgeSetFactory"> factory for the creation of vertex edge sets.</param>
    public UndirectedSpecifics(
        IGraph<TNode, TEdge>                                      graph,
        IDictionary<TNode, UndirectedEdgeContainer<TNode, TEdge>> vertexMap,
        IEdgeSetFactory<TNode, TEdge>                             edgeSetFactory
    )
    {
        Graph          = Objects.requireNonNull(graph);
        VertexMap      = Objects.requireNonNull(vertexMap);
        EdgeSetFactory = Objects.requireNonNull(edgeSetFactory);
    }

    ///<inheritdoc/>
    public virtual bool AddVertex(TNode node)
    {
        var ec = VertexMap[node];
        if (ec == null)
        {
            VertexMap[node] = new UndirectedEdgeContainer<TNode, TEdge>(EdgeSetFactory, node);
            return true;
        }

        return false;
    }

    ///<inheritdoc/>
    public virtual ISet<TNode> VertexSet
    {
        get
        {
            return VertexMap.Keys;
        }
    }

    ///<inheritdoc/>
    public virtual ISet<TEdge> GetAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        ISet<TEdge> edges = null;

        if (Graph.ContainsVertex(sourceVertex) && Graph.ContainsVertex(targetVertex))
        {
            edges = new ArrayUnenforcedSet<TEdge>();

            foreach (var edge in GetEdgeContainer(sourceVertex).VertexEdges)
            {
                var equal = IsEqualsStraightOrInverted(sourceVertex, targetVertex, edge);

                if (equal)
                {
                    edges.Add(edge);
                }
            }
        }

        return edges;
    }

    ///<inheritdoc/>
    public virtual TEdge GetEdge(TNode sourceVertex, TNode targetVertex)
    {
        if (Graph.ContainsVertex(sourceVertex) && Graph.ContainsVertex(targetVertex))
        {
            foreach (var edge in GetEdgeContainer(sourceVertex).VertexEdges)
            {
                var equal = IsEqualsStraightOrInverted(sourceVertex, targetVertex, edge);

                if (equal)
                {
                    return edge;
                }
            }
        }

        return default(TEdge);
    }

    private bool IsEqualsStraightOrInverted(object sourceVertex, object targetVertex, TEdge edge)
    {
        var equalStraight = sourceVertex.Equals(Graph.GetEdgeSource(edge)) &&
                            targetVertex.Equals(Graph.GetEdgeTarget(edge));

        var equalInverted = sourceVertex.Equals(Graph.GetEdgeTarget(edge)) &&
                            targetVertex.Equals(Graph.GetEdgeSource(edge));
        return equalStraight || equalInverted;
    }

    public virtual bool AddEdgeToTouchingVertices(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        GetEdgeContainer(sourceVertex).AddEdge(edge);

        if (!sourceVertex.Equals(targetVertex))
        {
            GetEdgeContainer(targetVertex).AddEdge(edge);
        }

        return true;
    }

    public virtual bool AddEdgeToTouchingVerticesIfAbsent(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        // lookup for edge with same source and target
        var ec = GetEdgeContainer(sourceVertex);
        foreach (var edge in ec.VertexEdges)
        {
            if (IsEqualsStraightOrInverted(sourceVertex, targetVertex, edge))
            {
                return false;
            }
        }

        // add
        ec.AddEdge(edge);
        GetEdgeContainer(targetVertex).AddEdge(edge);
        return true;
    }

    public virtual TEdge CreateEdgeToTouchingVerticesIfAbsent(
        TNode              sourceVertex,
        TNode              targetVertex,
        Func<TEdge> edgeSupplier
    )
    {
        // lookup for edge with same source and target
        var ec = GetEdgeContainer(sourceVertex);
        foreach (var edge in ec.VertexEdges)
        {
            if (IsEqualsStraightOrInverted(sourceVertex, targetVertex, edge))
            {
                return default(TEdge);
            }
        }

        // create and add
        var edge = edgeSupplier();
        ec.AddEdge(edge);
        GetEdgeContainer(targetVertex).AddEdge(edge);

        return edge;
    }

    ///<inheritdoc/>
    public virtual int DegreeOf(TNode vertex)
    {
        if (Graph.Type.AllowingSelfLoops)
        {
            /*
             * Then we must count, and add loops twice
             */
            var         degree = 0;
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

    ///<inheritdoc/>
    public virtual ISet<TEdge> EdgesOf(TNode vertex)
    {
        return GetEdgeContainer(vertex).UnmodifiableVertexEdges;
    }

    ///<inheritdoc/>
    public virtual int InDegreeOf(TNode vertex)
    {
        return DegreeOf(vertex);
    }

    ///<inheritdoc/>
    public virtual ISet<TEdge> IncomingEdgesOf(TNode vertex)
    {
        return GetEdgeContainer(vertex).UnmodifiableVertexEdges;
    }

    ///<inheritdoc/>
    public virtual int OutDegreeOf(TNode vertex)
    {
        return DegreeOf(vertex);
    }

    ///<inheritdoc/>
    public virtual ISet<TEdge> OutgoingEdgesOf(TNode vertex)
    {
        return GetEdgeContainer(vertex).UnmodifiableVertexEdges;
    }

    ///<inheritdoc/>
    public virtual void RemoveEdgeFromTouchingVertices(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        GetEdgeContainer(sourceVertex).RemoveEdge(edge);

        if (!sourceVertex.Equals(targetVertex))
        {
            GetEdgeContainer(targetVertex).RemoveEdge(edge);
        }
    }

    ///<summary>
    ///Get the edge container for a specified vertex.
    ///</summary>
    ///<param name="vertex"> a vertex in this graph.</param>>
    ///<returns>an edge container.</returns>
    protected virtual UndirectedEdgeContainer<TNode, TEdge> GetEdgeContainer(TNode vertex)
    {
        var ec = VertexMap[vertex];

        if (ec == null)
        {
            ec                = new UndirectedEdgeContainer<TNode, TEdge>(EdgeSetFactory, vertex);
            VertexMap[vertex] = ec;
        }

        return ec;
    }
}
