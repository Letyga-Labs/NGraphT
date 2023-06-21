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

namespace NGraphT.Core.Traverse;

/// <summary>
/// A breadth-first iterator for a directed or undirected graph.
///
/// <para>
/// For this iterator to work correctly the graph must not be modified during iteration.
/// Currently there are no means to ensure that, nor to fail-fast.
/// The results of such modifications are undefined.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public sealed class BreadthFirstIterator<TVertex, TEdge>
    : CrossComponentIterator<TVertex, TEdge, BreadthFirstIterator<TVertex, TEdge>.SearchVertexData>
    where TVertex : class
    where TEdge : class
{
    private readonly LinkedList<TVertex> _queue = new();

    /// <summary>
    /// Creates a new breadth-first iterator for the specified graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    public BreadthFirstIterator(IGraph<TVertex, TEdge> g)
        : this(g, (TVertex?)null)
    {
    }

    /// <summary>
    /// Creates a new breadth-first iterator for the specified graph. Iteration will start at the
    /// specified start vertex and will be limited to the connected component that includes that
    /// vertex. If the specified start vertex is <c>null</c>, iteration will start at an
    /// arbitrary vertex and will not be limited, that is, will be able to traverse all the graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertex"> the vertex iteration to be started.</param>
    public BreadthFirstIterator(IGraph<TVertex, TEdge> g, TVertex? startVertex)
        : base(g, startVertex)
    {
    }

    /// <summary>
    /// Creates a new breadth-first iterator for the specified graph. Iteration will start at the
    /// specified start vertices and will be limited to the connected component that includes those
    /// vertices. If the specified start vertices is <c>null</c>, iteration will start at an
    /// arbitrary vertex and will not be limited, that is, will be able to traverse all the graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertices"> the vertices iteration to be started.</param>
    public BreadthFirstIterator(IGraph<TVertex, TEdge> g, IEnumerable<TVertex> startVertices)
        : base(g, startVertices)
    {
    }

    /// <inheritdoc/>
    protected override bool ConnectedComponentExhausted => _queue.Count == 0;

    /// <summary>
    /// Returns the parent vertex of vertex $v$ in the BFS search tree, or null if $v$ is the root
    /// vertex. This method can only be invoked on a vertex $v$ once the iterator has visited vertex $v$.
    /// </summary>
    /// <param name="v"> vertex.</param>
    /// <returns>parent vertex of vertex $v$ in the BFS search tree, or null if $v$ is a root vertex.</returns>
    public TVertex? GetParent(TVertex v)
    {
        var edge = GetSeenData(v).Edge;
        return edge == null ? null : Graphs.GetOppositeVertex(Graph, edge, v);
    }

    /// <summary>
    /// Returns the edge connecting vertex $v$ to its parent in the spanning tree formed by the BFS
    /// search, or null if $v$ is a root vertex. This method can only be invoked on a vertex $v$ once
    /// the iterator has visited vertex $v$.
    /// </summary>
    /// <param name="vertex"> vertex.</param>
    /// <returns>
    ///     edge connecting vertex $v$ in the BFS search tree to its parent, or null if $v$ is a root vertex.
    /// </returns>
    public TEdge? GetSpanningTreeEdge(TVertex vertex)
    {
        return GetSeenData(vertex).Edge;
    }

    /// <summary>
    /// Returns the depth of vertex $v$ in the search tree. The depth of a vertex $v$ is defined as
    /// the number of edges traversed on the path from the root of the BFS tree to vertex $v$. The
    /// root of the search tree has depth 0. This method can only be invoked on a vertex $v$ once the
    /// iterator has visited vertex $v$.
    /// </summary>
    /// <param name="v"> vertex.</param>
    /// <returns>depth of vertex $v$ in the search tree.</returns>
    public int GetDepth(TVertex v)
    {
        return GetSeenData(v).Depth;
    }

    /// <inheritdoc/>
    protected override void EncounterVertex(TVertex vertex, TEdge? edge)
    {
        var depth = edge == null ? 0 : GetSeenData(Graphs.GetOppositeVertex(Graph, edge, vertex)).Depth + 1;
        PutSeenData(vertex, new SearchVertexData(edge, depth));
        _queue.AddLast(vertex);
    }

    /// <inheritdoc/>
    protected override void EncounterVertexAgain(TVertex vertex, TEdge edge)
    {
    }

    /// <inheritdoc/>
    protected override TVertex ProvideNextVertex()
    {
        var next = _queue.First!.Value;
        _queue.RemoveFirst();
        return next;
    }

    /// <summary>
    /// Data kept for discovered vertices.
    /// </summary>
    public sealed class SearchVertexData
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="edge"> edge to parent.</param>
        /// <param name="depth"> depth of vertex in search tree.</param>
        public SearchVertexData(TEdge? edge, int depth)
        {
            Edge  = edge;
            Depth = depth;
        }

        internal TEdge? Edge  { get; }
        internal int    Depth { get; }
    }
}
