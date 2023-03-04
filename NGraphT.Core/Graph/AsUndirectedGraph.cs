/*
 * (C) Copyright 2003-2021, by John TNode Sichi and Contributors.
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
using Util;

/// <summary>
/// An undirected view of the backing directed graph specified in the constructor. This graph allows
/// modules to apply algorithms designed for undirected graphs to a directed graph by simply ignoring
/// edge direction. If the backing directed graph is an
/// <a href="http://mathworld.wolfram.com/OrientedGraph.html">oriented graph</a>, then the view will
/// be a simple graph; otherwise, it will be a multigraph. Query operations on this graph "read
/// through" to the backing graph. Attempts to add edges will result in an
/// <c>UnsupportedOperationException</c>, but vertex addition/removal and edge removal are all
/// supported (and immediately reflected in the backing graph).
///
/// <para>
/// Note that edges returned by this graph's accessors are really just the edges of the underlying
/// directed graph. Since there is no interface distinction between directed and undirected edges,
/// this detail should be irrelevant to algorithms.
/// </para>
///
/// <para>
/// This graph does <i>not</i> pass the hashCode and equals operations through to the backing graph,
/// but relies on <c>Object</c>'s <c>equals</c> and <c>hashCode</c> methods. This
/// graph will be serializable if the backing graph is serializable.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: John TNode. Sichi.</remarks>
public class AsUndirectedGraph<TNode, TEdge> : GraphDelegator<TNode, TEdge>, IGraph<TNode, TEdge>
{
    private const string NoEdgeAdd = "this graph does not support edge addition";

    /// <summary>
    /// Constructor for AsUndirectedGraph.
    /// </summary>
    /// <param name="g"> the backing directed graph over which an undirected view is to be created.</param>
    /// <exception cref="ArgumentException"> if the graph is not directed.</exception>
    public AsUndirectedGraph(IGraph<TNode, TEdge> g)
        : base(g)
    {
        GraphTests.RequireDirected(g);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        ISet<TEdge> forwardList = base.GetAllEdges(sourceVertex, targetVertex);

        if (sourceVertex.Equals(targetVertex))
        {
            // avoid duplicating loops
            return forwardList;
        }

        ISet<TEdge> reverseList = base.GetAllEdges(targetVertex, sourceVertex);
        ISet<TEdge> list        = new ArrayUnenforcedSet<TEdge>(forwardList.Count + reverseList.Count);
        list.addAll(forwardList);
        list.addAll(reverseList);

        return list;
    }

    /// <inheritdoc/>
    public override TEdge GetEdge(TNode sourceVertex, TNode targetVertex)
    {
        var edge = base.GetEdge(sourceVertex, targetVertex);

        if (edge != null)
        {
            return edge;
        }

        // try the other direction
        return base.GetEdge(targetVertex, sourceVertex);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
    {
        throw new NotSupportedException(NoEdgeAdd);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        throw new NotSupportedException(NoEdgeAdd);
    }

    /// <inheritdoc/>
    public override int DegreeOf(TNode vertex)
    {
        return base.DegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TNode vertex)
    {
        return base.EdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override int InDegreeOf(TNode vertex)
    {
        return base.DegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> OutgoingEdgesOf(TNode vertex)
    {
        return base.EdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TNode vertex)
    {
        return base.DegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override IGraphType Type
    {
        get
        {
            return base.Type.AsUndirected();
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return base.toStringFromSets(VertexSet(), EdgeSet(), false);
    }
}
