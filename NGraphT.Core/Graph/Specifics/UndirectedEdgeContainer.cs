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

using J2N.Collections.Generic.Extensions;

namespace NGraphT.Core.Graph.Specifics;

/// <summary>
/// A container for vertex edges.
///
/// <para>
/// In this edge container we use array lists to minimize memory toll. However, for high-degree
/// vertices we replace the entire edge container with a direct access subclass (to be implemented).
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public sealed class UndirectedEdgeContainer<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private ISet<TEdge>? _unmodifiableVertexEdges;

    internal UndirectedEdgeContainer(IEdgeSetFactory<TVertex, TEdge> edgeSetFactory, TVertex vertex)
    {
        VertexEdges = edgeSetFactory.CreateEdgeSet(vertex);
    }

    /// <summary>
    /// A lazy build of unmodifiable list of vertex edges.
    /// </summary>
    /// <returns>an unmodifiable set of vertex edges.</returns>
    public ISet<TEdge> UnmodifiableVertexEdges => _unmodifiableVertexEdges ??= VertexEdges.AsReadOnly();

    internal ISet<TEdge> VertexEdges { get; }

    /// <summary>
    /// Add a vertex edge.
    /// </summary>
    /// <param name="edge"> the edge to add.</param>
    public void AddEdge(TEdge edge)
    {
        VertexEdges.Add(edge);
    }

    /// <summary>
    /// Get number of vertex edges.
    /// </summary>
    /// <returns>the number of vertex edges.</returns>
    public int EdgeCount()
    {
        return VertexEdges.Count;
    }

    /// <summary>
    /// Remove a vertex edge.
    /// </summary>
    /// <param name="edge"> the edge to remove.</param>
    public void RemoveEdge(TEdge edge)
    {
        VertexEdges.Remove(edge);
    }
}
