/*
 * (C) Copyright 2006-2021, by John TNode Sichi and Contributors.
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

namespace NGraphT.Core.Graph;

using Core;

/// <summary>
/// Provides an edge-reversed view $g'$ of a directed graph $g$. The vertex sets for the two graphs
/// are the same, but g' contains an edge $(v2, v1)$ iff g$$ contains an edge $(v1, v2)$. $g'$ is
/// backed by $g$, so changes to $g$ are reflected in $g'$, and vice versa.
///
/// <para>
/// This class allows you to use a directed graph algorithm in reverse. For example, suppose you have
/// a directed graph representing a tree, with edges from parent to child, and you want to find all
/// of the parents of a node. To do this, simply create an edge-reversed graph and pass that as input
/// to <see cref="NGraphT.Core.Traverse.DepthFirstIterator"/>.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: John TNode. Sichi.</remarks>
/// <seealso cref="AsUndirectedGraph"/>
public class EdgeReversedGraph<TNode, TEdge> : GraphDelegator<TNode, TEdge>, IGraph<TNode, TEdge>
{
    ///<summary>
    ///Creates a new EdgeReversedGraph.
    ///</summary>
    ///<param name="g"> the base (backing) graph on which the edge-reversed view will be based.</param>
    public EdgeReversedGraph(IGraph<TNode, TEdge> g)
        : base(g)
    {
    }

    ///<see cref="Graph.getEdge(Object, Object)"/>
    public override TEdge GetEdge(TNode sourceVertex, TNode targetVertex)
    {
        return base.GetEdge(targetVertex, sourceVertex);
    }

    ///<see cref="Graph.getAllEdges(Object, Object)"/>
    public override ISet<TEdge> GetAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        return base.GetAllEdges(targetVertex, sourceVertex);
    }

    ///<see cref="Graph.addEdge(Object, Object)"/>
    public override TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
    {
        return base.AddEdge(targetVertex, sourceVertex);
    }

    ///<see cref="Graph.addEdge(Object, Object, Object)"/>
    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        return base.AddEdge(targetVertex, sourceVertex, edge);
    }

    ///<see cref="Graph.inDegreeOf(Object)"/>
    public override int InDegreeOf(TNode vertex)
    {
        return base.OutDegreeOf(vertex);
    }

    ///<see cref="Graph.outDegreeOf(Object)"/>
    public override int OutDegreeOf(TNode vertex)
    {
        return base.InDegreeOf(vertex);
    }

    ///<see cref="Graph.incomingEdgesOf(Object)"/>
    public override ISet<TEdge> IncomingEdgesOf(TNode vertex)
    {
        return base.OutgoingEdgesOf(vertex);
    }

    ///<see cref="Graph.outgoingEdgesOf(Object)"/>
    public override ISet<TEdge> OutgoingEdgesOf(TNode vertex)
    {
        return base.IncomingEdgesOf(vertex);
    }

    ///<see cref="Graph.removeEdge(Object, Object)"/>
    public override TEdge RemoveEdge(TNode sourceVertex, TNode targetVertex)
    {
        return base.RemoveEdge(targetVertex, sourceVertex);
    }

    ///<see cref="Graph.getEdgeSource(Object)"/>
    public override TNode GetEdgeSource(TEdge edge)
    {
        return base.GetEdgeTarget(edge);
    }

    ///<see cref="Graph.getEdgeTarget(Object)"/>
    public override TNode GetEdgeTarget(TEdge edge)
    {
        return base.GetEdgeSource(edge);
    }

    ///<see cref="java.lang.Object.toString()"/>
    public override string ToString()
    {
        return toStringFromSets(VertexSet(), EdgeSet(), Type.Directed);
    }
}
