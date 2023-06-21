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

namespace NGraphT.Core.Graph;

/// <summary>
/// An unmodifiable view of the backing graph specified in the constructor. This graph allows modules
/// to provide users with "read-only" access to internal graphs. Query operations on this graph "read
/// through" to the backing graph, and attempts to modify this graph result in an <c>
/// UnsupportedOperationException</c>.
///
/// <para>
/// This graph does <i>not</i> pass the hashCode and equals operations through to the backing graph,
/// but relies on <c>Object</c>'s <c>equals</c> and <c>hashCode</c> methods. This
/// graph will be serializable if the backing graph is serializable.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public class AsUnmodifiableGraph<TVertex, TEdge> : GraphDelegator<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private const string UNMODIFIABLE = "this graph is unmodifiable";

    /// <summary>
    /// Creates a new unmodifiable graph based on the specified backing graph.
    /// </summary>
    /// <param name="g"> the backing graph on which an unmodifiable graph is to be created.</param>
    public AsUnmodifiableGraph(IGraph<TVertex, TEdge> g)
        : base(g)
    {
    }

    /// <inheritdoc/>
    public override IGraphType Type => base.Type.AsUnmodifiable();

    /// <inheritdoc/>
    public override TEdge? AddEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override TVertex AddVertex()
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool AddVertex(TVertex vertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> RemoveAllEdges(TVertex sourceVertex, TVertex targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool RemoveEdge(TEdge? edge)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override TEdge RemoveEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool RemoveVertex(TVertex? vertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }
}
