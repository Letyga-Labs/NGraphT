/*
 * (C) Copyright 2015-2021, by Joris Kinable and Contributors.
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

/// <summary>
/// Fast implementation of DirectedSpecifics. This class uses additional data structures to improve
/// the performance of methods which depend on edge retrievals, TEdge.g. getEdge(TNode u, TNode TNode),
/// containsEdge(TNode u, TNode TNode),addEdge(TNode u, TNode TNode). A disadvantage is an increase in memory consumption. If
/// memory utilization is an issue, use a <see cref="DirectedSpecifics"/> instead.
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Joris Kinable.</remarks>
public class FastLookupDirectedSpecifics<TNode, TEdge> : DirectedSpecifics<TNode, TEdge>
{
    ///<summary>
    ///Maps a pair of vertices &lt;u,TNode&gt; to a set of edges {(u,TNode)}. In case of a multigraph, all
    ///edges which touch both u and TNode are included in the set.
    ///</summary>
    protected IDictionary<Pair<TNode, TNode>, ISet<TEdge>> TouchingVerticesToEdgeMap;

    ///<summary>
    ///Construct a new fast lookup directed specifics.
    ///</summary>
    ///<param name="graph"> the graph for which these specifics are for.</param>
    ///<param name="vertexMap"> map for the storage of vertex edge sets. Needs to have a predictable
    ///       iteration order.</param>
    ///<param name="touchingVerticesToEdgeMap"> Additional map for caching. No need for a predictable
    ///       iteration order.</param>
    ///<param name="edgeSetFactory"> factory for the creation of vertex edge sets.</param>
    public FastLookupDirectedSpecifics(
        IGraph<TNode, TEdge>                                    graph,
        IDictionary<TNode, DirectedEdgeContainer<TNode, TEdge>> vertexMap,
        IDictionary<Pair<TNode, TNode>, ISet<TEdge>>            touchingVerticesToEdgeMap,
        IEdgeSetFactory<TNode, TEdge>                           edgeSetFactory
    )
        : base(graph, vertexMap, edgeSetFactory)
    {
        TouchingVerticesToEdgeMap = Objects.requireNonNull(touchingVerticesToEdgeMap);
    }

    public override ISet<TEdge> GetAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        if (Graph.ContainsVertex(sourceVertex) && Graph.ContainsVertex(targetVertex))
        {
            var edges = TouchingVerticesToEdgeMap[new Pair<TEdge>(sourceVertex, targetVertex)];
            if (edges == null)
            {
                return java.util.Collections.emptySet();
            }
            else
            {
                var edgeSet = EdgeSetFactory.CreateEdgeSet(sourceVertex);
                edgeSet.addAll(edges);
                return edgeSet;
            }
        }
        else
        {
            return null;
        }
    }

    public override TEdge GetEdge(TNode sourceVertex, TNode targetVertex)
    {
        var edges = TouchingVerticesToEdgeMap[new Pair<TEdge>(sourceVertex, targetVertex)];
        if (edges == null || edges.Count == 0)
        {
            return default(TEdge);
        }
        else
        {
            return edges.GetEnumerator().next();
        }
    }

    public override bool AddEdgeToTouchingVertices(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        if (!base.AddEdgeToTouchingVertices(sourceVertex, targetVertex, edge))
        {
            return false;
        }

        AddToIndex(sourceVertex, targetVertex, edge);
        return true;
    }

    public override bool AddEdgeToTouchingVerticesIfAbsent(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        // first lookup using our own index
        var edge = GetEdge(sourceVertex, targetVertex);
        if (edge != null)
        {
            return false;
        }

        return AddEdgeToTouchingVertices(sourceVertex, targetVertex, TEdge);
    }

    public override TEdge CreateEdgeToTouchingVerticesIfAbsent(
        TNode              sourceVertex,
        TNode              targetVertex,
        Func<TEdge> edgeSupplier
    )
    {
        // first lookup using our own index
        var edge = GetEdge(sourceVertex, targetVertex);
        if (edge != null)
        {
            return default(TEdge);
        }

        var edge = edgeSupplier();
        AddEdgeToTouchingVertices(sourceVertex, targetVertex, edge);
        return edge;
    }

    public override void RemoveEdgeFromTouchingVertices(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        base.RemoveEdgeFromTouchingVertices(sourceVertex, targetVertex, edge);

        RemoveFromIndex(sourceVertex, targetVertex, edge);
    }

    ///<summary>
    ///Add an edge to the index.
    ///</summary>
    ///<param name="sourceVertex"> the source vertex.</param>
    ///<param name="targetVertex"> the target vertex.</param>
    ///<param name="edge"> the edge.</param>
    protected virtual void AddToIndex(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        Pair<TNode, TNode> vertexPair = new Pair<TNode, TNode>(sourceVertex, targetVertex);
        var        edgeSet    = TouchingVerticesToEdgeMap[vertexPair];
        if (edgeSet != null)
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

    ///<summary>
    ///Remove an edge from the index.
    ///</summary>
    ///<param name="sourceVertex"> the source vertex.</param>
    ///<param name="targetVertex"> the target vertex.</param>
    ///<param name="edge"> the edge.</param>
    protected virtual void RemoveFromIndex(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        Pair<TNode, TNode> vertexPair = new Pair<TNode, TNode>(sourceVertex, targetVertex);
        var        edgeSet    = TouchingVerticesToEdgeMap[vertexPair];
        if (edgeSet != null)
        {
            edgeSet.remove(edge);
            if (edgeSet.Count == 0)
            {
                TouchingVerticesToEdgeMap.Remove(vertexPair);
            }
        }
    }
}
