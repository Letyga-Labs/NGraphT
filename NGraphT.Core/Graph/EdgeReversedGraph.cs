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

using System.Diagnostics.CodeAnalysis;
using NGraphT.Core.Traverse;

namespace NGraphT.Core.Graph;

/// <summary>
/// Provides an edge-reversed view $g'$ of a directed graph $g$. The vertex sets for the two graphs
/// are the same, but g' contains an edge $(v2, v1)$ iff g$$ contains an edge $(v1, v2)$. $g'$ is
/// backed by $g$, so changes to $g$ are reflected in $g'$, and vice versa.
///
/// <para>
/// This class allows you to use a directed graph algorithm in reverse. For example, suppose you have
/// a directed graph representing a tree, with edges from parent to child, and you want to find all
/// of the parents of a vertex. To do this, simply create an edge-reversed graph and pass that as input
/// to <see cref="DepthFirstIterator{TVertex,TEdge}"/>.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <seealso cref="AsUndirectedGraph{TVertex,TEdge}"/>
///
/// <remarks>Author: John V. Sichi.</remarks>
[SuppressMessage("Major Code Smell", "S2234:Parameters should be passed in the correct order")]
public sealed class EdgeReversedGraph<TVertex, TEdge> : GraphDelegator<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Creates a new EdgeReversedGraph.
    /// </summary>
    /// <param name="g"> the base (backing) graph on which the edge-reversed view will be based.</param>
    public EdgeReversedGraph(IGraph<TVertex, TEdge> g)
        : base(g)
    {
    }

    /// <inheritdoc/>
    public override TEdge? GetEdge(TVertex? sourceVertex, TVertex? targetVertex)
    {
        return base.GetEdge(targetVertex, sourceVertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TVertex? sourceVertex, TVertex? targetVertex)
    {
        return base.GetAllEdges(targetVertex, sourceVertex);
    }

    /// <inheritdoc/>
    public override TEdge? AddEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        return base.AddEdge(targetVertex, sourceVertex);
    }

    /// <inheritdoc/>
    public override bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        return base.AddEdge(targetVertex, sourceVertex, edge);
    }

    /// <inheritdoc/>
    public override int InDegreeOf(TVertex vertex)
    {
        return base.OutDegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TVertex vertex)
    {
        return base.InDegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TVertex vertex)
    {
        return base.OutgoingEdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> OutgoingEdgesOf(TVertex vertex)
    {
        return base.IncomingEdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override TEdge? RemoveEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        return base.RemoveEdge(targetVertex, sourceVertex);
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeSource(TEdge edge)
    {
        return base.GetEdgeTarget(edge);
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeTarget(TEdge edge)
    {
        return base.GetEdgeSource(edge);
    }

    public override string ToString()
    {
        return ToStringFromSets(VertexSet(), EdgeSet(), Type.IsDirected);
    }
}
