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
using NGraphT.Core.Graph;

namespace NGraphT.Core;

/// <summary>
/// The root interface in the graph hierarchy. A mathematical graph-theory graph object
/// <c>G(TVertex,TEdge)</c> contains a set <c>TVertex</c> of vertices and a set <c>
/// TEdge</c> of edges. Each edge TEdge=(v1,v2) in TEdge connects vertex v1 to vertex v2. for more information
/// about graphs and their related definitions see <a href="http://mathworld.wolfram.com/Graph.html">
/// http://mathworld.wolfram.com/Graph.html</a>.
///
/// <para>
/// This library generally follows the terminology found at:
/// <a href="http://mathworld.wolfram.com/topics/GraphTheory.html">
/// http://mathworld.wolfram.com/topics/GraphTheory.html</a>. Implementation of this interface can
/// provide simple-graphs, multigraphs, pseudographs etc. The package <c>NGraphT.Core.Graph</c>
/// provides a gallery of abstract and concrete graph implementations.
/// </para>
///
/// <para>
/// This library works best when vertices represent arbitrary objects and edges represent the
/// relationships between them. Vertex and edge instances may be shared by more than one graph.
/// </para>
///
/// <para>
/// Through generics, a graph can be typed to specific classes for vertices <c>TVertex</c> and edges
/// <c>TEdge&lt;T&gt;</c>. Such a graph can contain vertices of type <c>TVertex</c> and all
/// sub-types and Edges of type <c>
/// TEdge</c> and all sub-types.
/// </para>
///
/// <para>
/// For guidelines on vertex and edge classes, see
/// <a href="https://github.com/jgrapht/jgrapht/wiki/EqualsAndHashCode">this wiki page</a>.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">the graph vertex type.</typeparam>
/// <typeparam name="TEdge">the graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public interface IGraph<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// The default weight for an edge.
    /// </summary>
    public const double DefaultEdgeWeight = 1.0;

    /// <summary>
    /// Get the graph type. The graph type can be used to query for additional metadata such as
    /// whether the graph supports directed or undirected edges, self-loops, multiple (parallel)
    /// edges, weights, etc.
    /// </summary>
    ///
    /// <returns>the graph type.</returns>
    IGraphType Type { get; }

    /// <summary>
    /// Return the vertex supplier that the graph uses whenever it needs to create new vertices.
    ///
    /// <para>
    /// A graph uses the vertex supplier to create new vertex objects whenever a user calls method
    /// <see cref="AddVertex()"/>. Users can also create the vertex in user code and then use method
    /// <see cref="AddVertex(TVertex)"/> to add the vertex.
    /// </para>
    ///
    /// <para>
    /// In contrast with the Supplier interface, the vertex supplier has the additional
    /// requirement that a new and distinct result is returned every time it is invoked. More
    /// specifically for a new vertex to be added in a graph <c>TVertex</c> must <i>not</i> be equal
    /// to any other vertex in the graph. More formally, the graph must not contain any vertex
    /// <c>v2</c> such that <c>v2.equals(TVertex)</c>.
    /// </para>
    ///
    /// <para>
    /// Care must also be taken when interchanging calls to methods <see cref="AddVertex(TVertex)"/>
    /// and <see cref="AddVertex()"/>. In such a case the user must make sure never to add vertices
    /// in the graph using method <see cref="AddVertex(TVertex)"/>, which are going to be returned in
    /// the future by the supplied vertex supplier. Such a sequence will result into an
    /// <see cref="System.ArgumentException"/> when calling method <see cref="AddVertex()"/>.
    /// </para>
    /// </summary>
    ///
    /// <returns>the vertex supplier or <c>null</c> if the graph has no such supplier.</returns>
    Func<TVertex>? VertexSupplier { get; }

    /// <summary>
    /// Return the edge supplier that the graph uses whenever it needs to create new edges.
    ///
    /// <para>
    /// A graph uses the edge supplier to create new edge objects whenever a user calls method
    /// <see cref="AddEdge(TVertex,TVertex)"/>. Users can also create the edge in user code and then
    /// use method <see cref="AddEdge(TVertex,TVertex,TEdge)"/> to add the edge.
    /// </para>
    ///
    /// <para>
    /// In contrast with the Supplier interface, the edge supplier has the additional
    /// requirement that a new and distinct result is returned every time it is invoked. More
    /// specifically for a new edge to be added in a graph <c>TEdge</c> must <i>not</i> be equal to
    /// any other edge in the graph (even if the graph allows edge-multiplicity). More formally, the
    /// graph must not contain any edge <c>e2</c> such that <c>e2.equals(TEdge)</c>.
    /// </para>
    /// </summary>
    ///
    /// <returns>the edge supplier or <c>null</c> if the graph has no such supplier.</returns>
    Func<TEdge>? EdgeSupplier { get; }

    /// <summary>
    /// Returns a set of all edges connecting source vertex to target vertex if such vertices exist
    /// in this graph. If any of the vertices does not exist or is <c>null</c>, returns
    /// <c>null</c>. If both vertices exist but no edges found, returns an empty set.
    ///
    /// <para>
    /// In undirected graphs, some of the returned edges may have their source and target vertices in
    /// the opposite order. In simple graphs the returned set is either singleton set or empty set.
    /// </para>
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
    /// Creates a new edge in this graph, going from the source vertex to the target vertex, and
    /// returns the created edge. Some graphs do not allow edge-multiplicity. In such cases, if the
    /// graph already contains an edge from the specified source to the specified target, then this
    /// method does not change the graph and returns <c>null</c>.
    ///
    /// <para>
    /// The source and target vertices must already be contained in this graph. If they are not found
    /// in graph <see cref="System.ArgumentException"/> is thrown.
    /// </para>
    ///
    /// <para>
    /// This method creates the new edge <c>TEdge</c> using this graph's edge supplier (see
    /// <see cref="EdgeSupplier"/>). For the new edge to be added <c>TEdge</c> must <i>not</i> be
    /// equal to any other edge the graph (even if the graph allows edge-multiplicity). More
    /// formally, the graph must not contain any edge <c>e2</c> such that
    /// <c>e2.equals(TEdge)</c>. If such <c>e2</c> is found then the newly created edge <c>TEdge</c> is abandoned,
    /// the method leaves this graph unchanged and returns <c>null</c>.
    /// </para>
    ///
    /// <para>
    /// If the underlying graph implementation's <see cref="EdgeSupplier"/> returns
    /// <c>null</c>, then this method cannot create edges and throws an
    /// <see cref="System.NotSupportedException"/>.
    /// </para>
    /// </summary>
    ///
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    ///
    /// <returns>The newly created edge if added to the graph, otherwise <c>null</c>.</returns>
    ///
    /// <exception cref="ArgumentException"> if source or target vertices are not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if any of the specified vertices is <c>null</c>.</exception>
    /// <exception cref="NotSupportedException"> if the graph was not initialized with an edge supplier.</exception>
    ///
    /// <seealso cref="EdgeSupplier"/>
    TEdge? AddEdge(TVertex sourceVertex, TVertex targetVertex);

    /// <summary>
    /// Adds the specified edge to this graph, going from the source vertex to the target vertex.
    /// More formally, adds the specified edge, <c>
    /// TEdge</c>, to this graph if this graph contains no edge <c>e2</c> such that
    /// <c>e2.equals(TEdge)</c>. If this graph already contains such an edge, the call leaves this
    /// graph unchanged and returns <c>false</c>. Some graphs do not allow edge-multiplicity.
    /// In such cases, if the graph already contains an edge from the specified source to the
    /// specified target, then this method does not change the graph and returns <c>
    /// false</c>. If the edge was added to the graph, returns <c>
    /// true</c>.
    ///
    /// <para>
    /// The source and target vertices must already be contained in this graph. If they are not found
    /// in graph ArgumentException is thrown.
    /// </para>
    /// </summary>
    ///
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    /// <param name="edge"> edge to be added to this graph.</param>
    ///
    /// <returns><c>true</c> if this graph did not already contain the specified edge.</returns>
    ///
    /// <exception cref="ArgumentException"> if source or target vertices are not found in the graph.</exception>
    /// <exception cref="InvalidCastException">
    ///     if the specified edge is not assignment compatible with the class of edges
    ///     produced by the edge factory of this graph.
    /// </exception>
    /// <exception cref="NullReferenceException"> if any of the specified vertices is <c>null</c>.</exception>
    ///
    /// <seealso cref="AddEdge(TVertex,TVertex)"/>
    /// <seealso cref="EdgeSupplier"/>
    bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge);

    /// <summary>
    /// Creates a new vertex in this graph and returns it.
    ///
    /// <para>
    /// This method creates the new vertex <c>TVertex</c> using this graph's vertex supplier (see
    /// <see cref="VertexSupplier"/>). For the new vertex to be added <c>TVertex</c> must <i>not</i>
    /// be equal to any other vertex in the graph. More formally, the graph must not contain any
    /// vertex <c>v2</c> such that <c>v2.equals(TVertex)</c>. If such <c>v2</c> is found then the
    /// newly created vertex <c>TVertex</c> is abandoned, the method
    /// leaves this graph unchanged and throws an <see cref="System.ArgumentException"/>.
    /// </para>
    ///
    /// <para>
    /// If the underlying graph implementation's <see cref="VertexSupplier"/> returns
    /// <c>null</c>, then this method cannot create vertices and throws an
    /// <see cref="System.NotSupportedException"/>.
    /// </para>
    ///
    /// <para>
    /// Care must also be taken when interchanging calls to methods <see cref="AddVertex(TVertex)"/>
    /// and <see cref="AddVertex()"/>. In such a case the user must make sure never to add vertices
    /// in the graph using method <see cref="AddVertex(TVertex)"/>, which are going to be returned in
    /// the future by the supplied vertex supplier. Such a sequence will result into an
    /// <see cref="System.ArgumentException"/> when calling method <see cref="AddVertex()"/>.
    /// </para>
    /// </summary>
    ///
    /// <returns>The newly created vertex if added to the graph.</returns>
    ///
    /// <exception cref="ArgumentException">
    ///     If the graph supplier returns a vertex which is already in the graph.
    /// </exception>
    /// <exception cref="NotSupportedException"> if the graph was not initialized with a vertex supplier.</exception>
    ///
    /// <seealso cref="VertexSupplier"/>
    TVertex AddVertex();

    /// <summary>
    /// Adds the specified vertex to this graph if not already present. More formally, adds the
    /// specified vertex, <c>TVertex</c>, to this graph if this graph contains no vertex
    /// <c>u</c> such that <c>u.equals(TVertex)</c>. If this graph already contains such vertex,
    /// the call leaves this graph unchanged and returns <c>false</c>. In combination with the restriction on
    /// constructors, this ensures that graphs never contain duplicate vertices.
    /// </summary>
    ///
    /// <param name="vertex"> vertex to be added to this graph.</param>
    ///
    /// <returns><c>true</c> if this graph did not already contain the specified vertex.</returns>
    ///
    /// <exception cref="NullReferenceException"> if the specified vertex is <c>null</c>.</exception>
    bool AddVertex(TVertex vertex);

    /// <summary>
    /// Returns <c>true</c> if and only if this graph contains an edge going from the source
    /// vertex to the target vertex. In undirected graphs the same result is obtained when source and
    /// target are inverted. If any of the specified vertices does not exist in the graph, or if is
    /// <c>null</c>, returns <c>false</c>.
    /// </summary>
    ///
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    ///
    /// <returns><c>true</c> if this graph contains the specified edge.</returns>
    bool ContainsEdge([NotNullWhen(true)] TVertex? sourceVertex, [NotNullWhen(true)] TVertex? targetVertex);

    /// <summary>
    /// Returns <c>true</c> if this graph contains the specified edge. More formally, returns
    /// <c>true</c> if and only if this graph contains an edge <c>e2</c> such that
    /// <c>TEdge.equals(e2)</c>. If the specified edge is <c>null</c> returns
    /// <c>false</c>.
    /// </summary>
    ///
    /// <param name="edge"> edge whose presence in this graph is to be tested.</param>
    ///
    /// <returns><c>true</c> if this graph contains the specified edge.</returns>
    bool ContainsEdge([NotNullWhen(true)] TEdge? edge);

    /// <summary>
    /// Returns <c>true</c> if this graph contains the specified vertex. More formally, returns
    /// <c>true</c> if and only if this graph contains a vertex <c>u</c> such that
    /// <c>u.equals(TVertex)</c>. If the specified vertex is <c>null</c> returns <c>false</c>.
    /// </summary>
    ///
    /// <param name="vertex"> vertex whose presence in this graph is to be tested.</param>
    ///
    /// <returns><c>true</c> if this graph contains the specified vertex.</returns>
    bool ContainsVertex([NotNullWhen(true)] TVertex? vertex);

    /// <summary>
    /// Returns a set of the edges contained in this graph. The set is backed by the graph, so
    /// changes to the graph are reflected in the set. If the graph is modified while an iteration
    /// over the set is in progress, the results of the iteration are undefined.
    ///
    /// <para>
    /// The graph implementation may maintain a particular set ordering (e.g. via
    /// <see cref="J2N.Collections.Generic.LinkedHashSet{T}"/>) for deterministic iteration, but this is not required.
    /// It is the responsibility of callers who rely on this behavior to only use graph implementations
    /// which support it.
    /// </para>
    /// </summary>
    ///
    /// <returns>a set of the edges contained in this graph.</returns>
    ISet<TEdge> EdgeSet();

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
    ///
    /// <returns>the degree of the specified vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    /// <exception cref="ArithmeticException"> if the result overflows an int.</exception>
    int DegreeOf(TVertex vertex);

    /// <summary>
    /// Returns a set of all edges touching the specified vertex. If no edges are touching the
    /// specified vertex returns an empty set.
    /// </summary>
    ///
    /// <param name="vertex"> the vertex for which a set of touching edges is to be returned.</param>
    ///
    /// <returns>a set of all edges touching the specified vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    ISet<TEdge> EdgesOf(TVertex vertex);

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
    ///
    /// <returns>the degree of the specified vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    /// <exception cref="ArithmeticException"> if the result overflows an int.</exception>
    int InDegreeOf(TVertex vertex);

    /// <summary>
    /// Returns a set of all edges incoming into the specified vertex.
    ///
    /// <para>
    /// In the case of undirected graphs this method returns all edges touching the vertex, thus,
    /// some of the returned edges may have their source and target vertices in the opposite order.
    /// </para>
    /// </summary>
    ///
    /// <param name="vertex"> the vertex for which the list of incoming edges to be returned.</param>
    ///
    /// <returns>a set of all edges incoming into the specified vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    ISet<TEdge> IncomingEdgesOf(TVertex vertex);

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
    /// </summary>
    ///
    /// <param name="vertex"> vertex whose degree is to be calculated.</param>
    ///
    /// <returns>the degree of the specified vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    /// <exception cref="ArithmeticException"> if the result overflows an int.</exception>
    int OutDegreeOf(TVertex vertex);

    /// <summary>
    /// Returns a set of all edges outgoing from the specified vertex.
    ///
    /// <para>
    /// In the case of undirected graphs this method returns all edges touching the vertex, thus,
    /// some of the returned edges may have their source and target vertices in the opposite order.
    /// </para>
    /// </summary>
    ///
    /// <param name="vertex"> the vertex for which the list of outgoing edges to be returned.</param>
    ///
    /// <returns>a set of all edges outgoing from the specified vertex.</returns>
    ///
    /// <exception cref="ArgumentException"> if vertex is not found in the graph.</exception>
    /// <exception cref="NullReferenceException"> if vertex is <c>null</c>.</exception>
    ISet<TEdge> OutgoingEdgesOf(TVertex vertex);

    /// <summary>
    /// Removes all the edges in this graph that are also contained in the specified edge collection.
    /// After this call returns, this graph will contain no edges in common with the specified edges.
    /// This method will invoke the <see cref="RemoveEdge(TEdge)"/> method.
    /// </summary>
    ///
    /// <param name="edges"> edges to be removed from this graph.</param>
    ///
    /// <returns><c>true</c> if this graph changed as a result of the call.</returns>
    ///
    /// <exception cref="NullReferenceException"> if the specified edge collection is <c>null</c>.</exception>
    ///
    /// <seealso cref="RemoveEdge(TEdge)"/>
    /// <seealso cref="ContainsEdge(TEdge)"/>
    bool RemoveAllEdges(IEnumerable<TEdge> edges);

    /// <summary>
    /// Removes all the edges going from the specified source vertex to the specified target vertex,
    /// and returns a set of all removed edges. Returns <c>null</c> if any of the specified
    /// vertices does not exist in the graph. If both vertices exist but no edge is found, returns an
    /// empty set. This method will either invoke the <see cref="RemoveEdge(TEdge)"/> method, or the
    /// <see cref="RemoveEdge(TVertex,TVertex)"/> method.
    /// </summary>
    ///
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    ///
    /// <returns>the removed edges, or <c>null</c> if either vertex is not part of graph.</returns>
    ISet<TEdge> RemoveAllEdges(TVertex sourceVertex, TVertex targetVertex);

    /// <summary>
    /// Removes all the vertices in this graph that are also contained in the specified vertex
    /// collection. After this call returns, this graph will contain no vertices in common with the
    /// specified vertices. This method will invoke the <see cref="RemoveVertex"/> method.
    /// </summary>
    ///
    /// <param name="vertices"> vertices to be removed from this graph.</param>
    ///
    /// <returns><c>true</c> if this graph changed as a result of the call.</returns>
    ///
    /// <exception cref="NullReferenceException"> if the specified vertex collection is <c>null</c>.</exception>
    ///
    /// <seealso cref="RemoveVertex"/>
    /// <seealso cref="ContainsVertex"/>
    bool RemoveAllVertices(IEnumerable<TVertex> vertices);

    /// <summary>
    /// Removes an edge going from source vertex to target vertex, if such vertices and such edge
    /// exist in this graph. Returns the edge if removed or <c>null</c> otherwise.
    /// </summary>
    ///
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    ///
    /// <returns>The removed edge, or <c>null</c> if no edge removed.</returns>
    TEdge? RemoveEdge(TVertex sourceVertex, TVertex targetVertex);

    /// <summary>
    /// Removes the specified edge from the graph. Removes the specified edge from this graph if it
    /// is present. More formally, removes an edge <c>e2</c> such that <c>e2.equals(TEdge)</c>,
    /// if the graph contains such edge. Returns
    /// <c>true</c> if the graph contained the specified edge. (The graph will not contain the
    /// specified edge once the call returns).
    ///
    /// <para>
    /// If the specified edge is <c>null</c> returns <c>false</c>.
    /// </para>
    /// </summary>
    ///
    /// <param name="edge"> edge to be removed from this graph, if present.</param>
    ///
    /// <returns><c>true</c> if and only if the graph contained the specified edge.</returns>
    bool RemoveEdge([NotNullWhen(true)] TEdge? edge);

    /// <summary>
    /// Removes the specified vertex from this graph including all its touching edges if present.
    /// More formally, if the graph contains a vertex <c>u</c> such that <c>u.equals(TVertex)</c>,
    /// the call removes all edges that touch
    /// <c>u</c> and then removes <c>u</c> itself. If no such <c>u</c> is found,
    /// the call leaves the graph unchanged. Returns <c>true</c> if the graph contained the
    /// specified vertex. (The graph will not contain the specified vertex once the call returns).
    ///
    /// <para>
    /// If the specified vertex is <c>null</c> returns <c>false</c>.
    /// </para>
    /// </summary>
    ///
    /// <param name="vertex"> vertex to be removed from this graph, if present.</param>
    ///
    /// <returns><c>true</c> if the graph contained the specified vertex; <c>false</c>
    ///         otherwise.</returns>
    bool RemoveVertex([NotNullWhen(true)] TVertex? vertex);

    /// <summary>
    /// Returns a set of the vertices contained in this graph. The set is backed by the graph, so
    /// changes to the graph are reflected in the set. If the graph is modified while an iteration
    /// over the set is in progress, the results of the iteration are undefined.
    ///
    /// <para>
    /// The graph implementation may maintain a particular set ordering (e.g. via
    /// <see cref="J2N.Collections.Generic.LinkedHashSet{T}"/>) for deterministic iteration, but this is not required.
    /// It is the responsibility of callers who rely on this behavior to only use graph implementations
    /// which support it.
    /// </para>
    /// </summary>
    ///
    /// <returns>a set view of the vertices contained in this graph.</returns>
    ISet<TVertex> VertexSet();

    /// <summary>
    /// Returns the source vertex of an edge. For an undirected graph, source and target are
    /// distinguishable designations (but without any mathematical meaning).
    /// </summary>
    ///
    /// <param name="edge"> edge of interest.</param>>
    ///
    /// <returns>source vertex.</returns>
    TVertex GetEdgeSource(TEdge edge);

    /// <summary>
    /// Returns the target vertex of an edge. For an undirected graph, source and target are
    /// distinguishable designations (but without any mathematical meaning).
    /// </summary>
    ///
    /// <param name="edge"> edge of interest.</param>>
    ///
    /// <returns>target vertex.</returns>
    TVertex GetEdgeTarget(TEdge edge);

    /// <summary>
    /// Returns the weight assigned to a given edge. Unweighted graphs return 1.0 (as defined by
    /// <see cref="DefaultEdgeWeight"/>), allowing weighted-graph algorithms to apply to them when
    /// meaningful.
    /// </summary>
    ///
    /// <param name="edge"> edge of interest.</param>
    ///
    /// <returns>edge weight.</returns>
    double GetEdgeWeight(TEdge edge);

    /// <summary>
    /// Assigns a weight to an edge.
    /// </summary>
    ///
    /// <param name="edge"> edge on which to set weight.</param>
    /// <param name="weight"> new weight for edge.</param>
    ///
    /// <exception cref="NotSupportedException"> if the graph does not support weights.</exception>
    void SetEdgeWeight(TEdge edge, double weight);

    /// <summary>
    /// Assigns a weight to an edge between <c>sourceVertex</c> and <c>targetVertex</c>.
    /// If no edge exists between <c>sourceVertex</c> and <c>targetVertex</c> or either
    /// of these vertices is <c>null</c>, a <c>NullPointerException</c> is thrown.
    /// <para>
    /// When there exist multiple edges between <c>sourceVertex</c> and
    /// <c>targetVertex</c>, consider using <see cref="SetEdgeWeight(TEdge,double)"/> instead.
    /// </para>
    /// </summary>
    ///
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    /// <param name="weight"> new weight for edge.</param>
    ///
    /// <exception cref="NotSupportedException"> if the graph does not support weights.</exception>
    void SetEdgeWeight(TVertex sourceVertex, TVertex targetVertex, double weight)
    {
        var edge =
            GetEdge(sourceVertex, targetVertex)
            ?? throw new InvalidOperationException("Coud not find edge between given vertices");
        SetEdgeWeight(edge, weight);
    }

    /// <summary>
    /// Access the graph using the <see cref="IGraphIterables{TVertex,TEdge}"/> interface. This allows accessing graphs
    /// without the restrictions imposed by 32-bit arithmetic. Moreover, graph implementations are
    /// free to implement this interface without explicitly materializing intermediate results.
    /// </summary>
    ///
    /// <returns>the graph iterables.</returns>
    IGraphIterables<TVertex, TEdge> Iterables()
    {
        return new DefaultGraphIterables<TVertex, TEdge>(this);
    }
}
