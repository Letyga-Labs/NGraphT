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

using NGraphT.Core.Util;

namespace NGraphT.Core;

/// <summary>
/// Presents a graph as a collection of views suitable for graphs which contain a very large number
/// of vertices or edges. Graph algorithms written these methods can work with graphs without the
/// restrictions imposed by 32-bit arithmetic.
///
/// <para>
/// Whether the returned iterators support removal of elements is left to the graph implementation.
/// It is the responsibility of callers who rely on this behavior to only use graph implementations
/// which support it.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public interface IGraphIterables<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Get the underlying graph.
    /// </summary>
    /// <returns>the underlying graph.</returns>
    IGraph<TVertex, TEdge> Graph { get; }

    /// <summary>
    /// Returns an iterable over the edges of the graph.
    ///
    /// <para>
    /// Whether the ordering is deterministic, depends on the actual graph implementation. It is the
    /// responsibility of callers who rely on this behavior to only use graph implementations which
    /// support it.
    /// </para>
    /// </summary>
    ///
    /// <returns>an iterable over the edges of the graph.</returns>
    IEnumerable<TEdge> Edges()
    {
        return new LiveIterableWrapper<TEdge>(() => Graph.EdgeSet());
    }

    /// <summary>
    /// Return the number of edges in the graph.
    /// </summary>
    /// <returns>the number of edges.</returns>
    long EdgeCount()
    {
        return Graph.EdgeSet().Count;
    }

    /// <summary>
    /// Returns an iterable view over the vertices contained in this graph. The returned iterator is
    /// a live view of the vertices. If the graph is modified while an iteration is in progress, the
    /// results of the iteration are undefined.
    ///
    /// <para>
    /// The graph implementation may maintain a particular ordering for deterministic iteration, but
    /// this is not required. It is the responsibility of callers who rely on this behavior to only
    /// use graph implementations which support it.
    /// </para>
    /// </summary>
    ///
    /// <returns>an iterable view of the vertices contained in this graph.</returns>
    IEnumerable<TVertex> Vertices()
    {
        return new LiveIterableWrapper<TVertex>(() => Graph.VertexSet());
    }

    /// <summary>
    /// Return the number of vertices in the graph.
    /// </summary>
    /// <returns>the number of vertices.</returns>
    long VertexCount()
    {
        return Graph.VertexSet().Count;
    }

    /// <summary>
    /// Returns an iterable view over all edges touching the specified vertex. The returned iterators
    /// are live views. If the graph is modified while an iteration is in progress, the results of
    /// the iteration are undefined. If no edges are touching the specified vertex, the returned
    /// iterators are already exhausted.
    /// </summary>
    ///
    /// <param name="vertex"> input vertex.</param>
    /// <returns>an iterable view of the vertices contained in this graph.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    IEnumerable<TEdge> EdgesOf(TVertex vertex)
    {
        return new LiveIterableWrapper<TEdge>(() => Graph.EdgesOf(vertex));
    }

    /// <summary>
    /// Returns the degree of the specified vertex.
    ///
    /// <para>
    /// A degree of a vertex in an undirected graph is the number of edges touching that vertex.
    /// Edges with same source and target vertices (self-loops) are counted twice.
    /// </para>
    ///
    /// <para>
    /// In directed graphs this method returns the sum of the "in degree" and the "out degree".
    /// </para>
    /// </summary>
    ///
    /// <param name="vertex"> vertex whose degree is to be calculated.</param>
    /// <returns>the degree of the specified vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    long DegreeOf(TVertex vertex)
    {
        return Graph.DegreeOf(vertex);
    }

    /// <summary>
    /// Returns an iterable view over all edges incoming into the specified vertex. The returned
    /// iterators are live views. If the graph is modified while an iteration is in progress, the
    /// results of the iteration are undefined.
    ///
    /// <para>
    /// In the case of undirected graphs the returned iterators return all edges touching the vertex,
    /// thus, some of the returned edges may have their source and target vertices in the opposite
    /// order.
    /// </para>
    ///
    /// </summary>
    ///
    /// <param name="vertex"> input vertex.</param>
    /// <returns>an iterable view of all edges incoming into the specified vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    IEnumerable<TEdge> IncomingEdgesOf(TVertex vertex)
    {
        return new LiveIterableWrapper<TEdge>(() => Graph.IncomingEdgesOf(vertex));
    }

    /// <summary>
    /// Returns the "in degree" of the specified vertex.
    ///
    /// <para>
    /// The "in degree" of a vertex in a directed graph is the number of inward directed edges from
    /// that vertex. See <a href="http://mathworld.wolfram.com/Indegree.html">
    /// http://mathworld.wolfram.com/Indegree.html</a>.
    /// </para>
    ///
    /// <para>
    /// In the case of undirected graphs this method returns the number of edges touching the vertex.
    /// Edges with same source and target vertices (self-loops) are counted twice.
    /// </para>
    /// </summary>
    ///
    /// <param name="vertex"> vertex whose degree is to be calculated.</param>
    /// <returns>the degree of the specified vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    long InDegreeOf(TVertex vertex)
    {
        return Graph.InDegreeOf(vertex);
    }

    /// <summary>
    /// Returns an iterable view over all edges outgoing into the specified vertex. The returned
    /// iterators are live views. If the graph is modified while an iteration is in progress, the
    /// results of the iteration are undefined.
    ///
    /// <para>
    /// In the case of undirected graphs the returned iterators return all edges touching the vertex,
    /// thus, some of the returned edges may have their source and target vertices in the opposite
    /// order.
    /// </para>
    /// </summary>
    ///
    /// <param name="vertex"> input vertex.</param>
    /// <returns>an iterable view of all edges outgoing from the specified vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    IEnumerable<TEdge> OutgoingEdgesOf(TVertex vertex)
    {
        return new LiveIterableWrapper<TEdge>(() => Graph.OutgoingEdgesOf(vertex));
    }

    /// <summary>
    /// Returns the "out degree" of the specified vertex.
    ///
    /// <para>
    /// The "out degree" of a vertex in a directed graph is the number of outward directed edges from
    /// that vertex. See <a href="http://mathworld.wolfram.com/Outdegree.html">
    /// http://mathworld.wolfram.com/Outdegree.html</a>.
    /// </para>
    ///
    /// <para>
    /// In the case of undirected graphs this method returns the number of edges touching the vertex.
    /// Edges with same source and target vertices (self-loops) are counted twice.
    /// </para>
    ///
    /// </summary>
    ///
    /// <param name="vertex"> vertex whose degree is to be calculated.</param>
    /// <returns>the degree of the specified vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    long OutDegreeOf(TVertex vertex)
    {
        return Graph.OutDegreeOf(vertex);
    }

    /// <summary>
    /// Returns an iterable view over all edges connecting source vertex to target vertex if such
    /// vertices exist in this graph. The returned iterators are live views. If the graph is modified
    /// while an iteration is in progress, the results of the iteration are undefined.
    ///
    /// If any of the vertices does not exist or is <c>null</c>, returns <c>null</c>. If
    /// both vertices exist but no edges found, returns an iterable which returns exhausted
    /// iterators.
    ///
    /// <para>
    /// In undirected graphs, some of the returned edges may have their source and target vertices in
    /// the opposite order. In simple graphs the returned set is either singleton set or empty set.
    /// </para>
    /// </summary>
    ///
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    /// <returns>an iterable view of all edges connecting source to target vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    IEnumerable<TEdge> AllEdges(TVertex sourceVertex, TVertex targetVertex)
    {
        return new LiveIterableWrapper<TEdge>(() => Graph.GetAllEdges(sourceVertex, targetVertex));
    }
}
