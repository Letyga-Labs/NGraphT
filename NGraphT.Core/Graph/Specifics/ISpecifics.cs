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

namespace NGraphT.Core.Graph.Specifics;

/// <summary>
/// An interface encapsulating the basic graph operations. Different implementations have different
/// space-time tradeoffs.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public interface ISpecifics<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Get the vertex set.
    /// </summary>
    /// <returns>the vertex set.</returns>
    ISet<TVertex> VertexSet { get; }

    /// <summary>
    /// Adds a vertex.
    /// </summary>
    /// <param name="vertex"> vertex to be added.</param>
    /// <returns>true if the vertex was added, false if the vertex was already present.</returns>
    bool AddVertex(TVertex vertex);

    /// <summary>
    /// Returns a set of all edges connecting source vertex to target vertex if such vertices exist
    /// in this graph. If any of the vertices does not exist or is <c>null</c>, returns
    /// <c>null</c>. If both vertices exist but no edges found, returns an empty set.
    /// </summary>
    ///
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    ///
    /// <returns>a set of all edges connecting source vertex to target vertex.</returns>
    ISet<TEdge> GetAllEdges(TVertex? sourceVertex, TVertex? targetVertex);

    /// <summary>
    /// Returns an edge connecting source vertex to target vertex if such vertices and such edge
    /// exist in this graph. Otherwise returns <c>null</c>.
    /// If any of the specified vertices is <c>null</c> returns <c>null</c>.
    ///
    /// <para>
    /// In undirected graphs, the returned edge may have its source and target vertices in the
    /// opposite order.
    /// </para>
    /// </summary>
    ///
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    ///
    /// <returns>an edge connecting source vertex to target vertex.</returns>
    TEdge? GetEdge(TVertex? sourceVertex, TVertex? targetVertex);

    /// <summary>
    /// Adds the specified edge to the edge containers of its source and target vertices.
    /// </summary>
    ///
    /// <param name="sourceVertex"> the source vertex.</param>
    /// <param name="targetVertex"> the target vertex.</param>
    /// <param name="edge"> the edge.</param>
    ///
    /// <returns>true if the edge was added, false otherwise.</returns>
    bool AddEdgeToTouchingVertices(TVertex sourceVertex, TVertex targetVertex, TEdge edge);

    /// <summary>
    /// Adds the specified edge to the edge containers of its source and target vertices only if the
    /// edge is not already in the graph.
    /// </summary>
    ///
    /// <param name="sourceVertex"> the source vertex.</param>
    /// <param name="targetVertex"> the target vertex.</param>
    /// <param name="edge"> the edge.</param>
    ///
    /// <returns>true if the edge was added, false otherwise.</returns>
    bool AddEdgeToTouchingVerticesIfAbsent(TVertex sourceVertex, TVertex targetVertex, TEdge edge);

    /// <summary>
    /// Creates an edge given an edge supplier and adds it to the edge containers of its source and
    /// target vertices only if the graph does not contain other edges with the same source and
    /// target vertices.
    /// </summary>
    ///
    /// <param name="sourceVertex"> the source vertex.</param>
    /// <param name="targetVertex"> the target vertex.</param>
    /// <param name="edgeSupplier"> the function which will create the edge.</param>
    ///
    /// <returns>the newly created edge or null if an edge with the same source and target vertices
    ///        was already present.</returns>
    TEdge? CreateEdgeToTouchingVerticesIfAbsent(TVertex sourceVertex, TVertex targetVertex, Func<TEdge> edgeSupplier);

    /// <summary>
    /// Returns the degree of the specified vertex. A degree of a vertex in an undirected graph is
    /// the number of edges touching that vertex.
    /// </summary>
    /// <param name="vertex"> vertex whose degree is to be calculated.</param>
    /// <returns>the degree of the specified vertex.</returns>
    int DegreeOf(TVertex vertex);

    /// <summary>
    /// Returns a set of all edges touching the specified vertex. If no edges are touching the
    /// specified vertex returns an empty set.
    /// </summary>
    /// <param name="vertex"> the vertex for which a set of touching edges is to be returned.</param>
    /// <returns>a set of all edges touching the specified vertex.</returns>
    ISet<TEdge> EdgesOf(TVertex vertex);

    /// <summary>
    /// Returns the "in degree" of the specified vertex.
    /// </summary>
    /// <param name="vertex"> vertex whose in degree is to be calculated.</param>
    /// <returns>the in degree of the specified vertex.</returns>
    int InDegreeOf(TVertex vertex);

    /// <summary>
    /// Returns a set of all edges incoming into the specified vertex.
    /// </summary>
    /// <param name="vertex"> the vertex for which the list of incoming edges to be returned.</param>
    /// <returns>a set of all edges incoming into the specified vertex.</returns>
    ISet<TEdge> IncomingEdgesOf(TVertex vertex);

    /// <summary>
    /// Returns the "out degree" of the specified vertex.
    /// </summary>
    /// <param name="vertex"> vertex whose out degree is to be calculated.</param>
    /// <returns>the out degree of the specified vertex.</returns>
    int OutDegreeOf(TVertex vertex);

    /// <summary>
    /// Returns a set of all edges outgoing from the specified vertex.
    /// </summary>
    /// <param name="vertex"> the vertex for which the list of outgoing edges to be returned.</param>
    /// <returns>a set of all edges outgoing from the specified vertex.</returns>
    ISet<TEdge> OutgoingEdgesOf(TVertex vertex);

    /// <summary>
    /// Removes the specified edge from the edge containers of its source and target vertices.
    /// </summary>
    ///
    /// <param name="sourceVertex"> the source vertex.</param>
    /// <param name="targetVertex"> the target vertex.</param>
    /// <param name="edge"> the edge.</param>
    void RemoveEdgeFromTouchingVertices(TVertex sourceVertex, TVertex targetVertex, TEdge edge);
}
