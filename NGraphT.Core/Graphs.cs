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

using NGraphT.Core.Graph;
using NGraphT.Core.Util;
using Java2Net = J2N.Collections.Generic;

namespace NGraphT.Core;

/// <summary>
/// A collection of utilities to assist with graph manipulation.
/// </summary>
///
/// <remarks>Author: Barak Naveh.</remarks>
public abstract class Graphs
{
    /// <summary>
    /// Creates a new edge and adds it to the specified graph similarly to the
    /// <see cref="IGraph{TVertex,TEdge}.AddEdge(TVertex,TVertex)"/> method.
    /// </summary>
    /// <param name="g"> the graph for which the edge to be added.</param>
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    /// <param name="weight"> weight of the edge.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>The newly created edge if added to the graph, otherwise <c>null</c>.</returns>
    /// <exception cref="NotSupportedException"> if the graph has no edge supplier.</exception>
    /// <seealso cref="IGraph{TVertex,TEdge}.AddEdge(TVertex,TVertex)"/>
    public static TEdge? AddEdge<TVertex, TEdge>(
        IGraph<TVertex, TEdge> g,
        TVertex                sourceVertex,
        TVertex                targetVertex,
        double                 weight)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(g);

        var edgeSupplier = g.EdgeSupplier;
        if (edgeSupplier == null)
        {
            throw new NotSupportedException("Graph contains no edge supplier");
        }

        var edge = edgeSupplier();

        if (g.AddEdge(sourceVertex, targetVertex, edge))
        {
            g.SetEdgeWeight(edge, weight);
            return edge;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Adds the specified source and target vertices to the graph, if not already included, and
    /// creates a new edge and adds it to the specified graph similarly to the
    /// <see cref="IGraph{TVertex,TEdge}.AddEdge(TVertex,TVertex)"/> method.
    /// </summary>
    /// <param name="g"> the graph for which the specified edge to be added.</param>
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>The newly created edge if added to the graph, otherwise <c>null</c>.</returns>
    public static TEdge? AddEdgeWithVertices<TVertex, TEdge>(
        IGraph<TVertex, TEdge> g,
        TVertex                sourceVertex,
        TVertex                targetVertex)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(g);
        g.AddVertex(sourceVertex);
        g.AddVertex(targetVertex);
        return g.AddEdge(sourceVertex, targetVertex);
    }

    /// <summary>
    /// Adds the specified edge to the graph, including its vertices if not already included.
    /// </summary>
    /// <param name="targetGraph"> the graph for which the specified edge to be added.</param>
    /// <param name="sourceGraph"> the graph in which the specified edge is already present.</param>
    /// <param name="edge"> edge to add.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns><c>true</c> if the target graph did not already contain the specified edge.</returns>
    public static bool AddEdgeWithVertices<TVertex, TEdge>(
        IGraph<TVertex, TEdge> targetGraph,
        IGraph<TVertex, TEdge> sourceGraph,
        TEdge                  edge)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(sourceGraph);
        ArgumentNullException.ThrowIfNull(targetGraph);

        var sourceVertex = sourceGraph.GetEdgeSource(edge);
        var targetVertex = sourceGraph.GetEdgeTarget(edge);

        targetGraph.AddVertex(sourceVertex);
        targetGraph.AddVertex(targetVertex);

        return targetGraph.AddEdge(sourceVertex, targetVertex, edge);
    }

    /// <summary>
    /// Adds the specified source and target vertices to the graph, if not already included, and
    /// creates a new weighted edge and adds it to the specified graph similarly to the
    /// <see cref="IGraph{TVertex,TEdge}.AddEdge(TVertex,TVertex)"/> method.
    /// </summary>
    /// <param name="g"> the graph for which the specified edge to be added.</param>
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    /// <param name="weight"> weight of the edge.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>The newly created edge if added to the graph, otherwise <c>null</c>.</returns>
    public static TEdge? AddEdgeWithVertices<TVertex, TEdge>(
        IGraph<TVertex, TEdge> g,
        TVertex                sourceVertex,
        TVertex                targetVertex,
        double                 weight)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(g);
        g.AddVertex(sourceVertex);
        g.AddVertex(targetVertex);
        return AddEdge(g, sourceVertex, targetVertex, weight);
    }

    /// <summary>
    /// Adds all the vertices and all the edges of the specified source graph to the specified
    /// destination graph. First all vertices of the source graph are added to the destination graph.
    /// Then every edge of the source graph is added to the destination graph. This method returns
    /// <c>true</c> if the destination graph has been modified as a result of this operation,
    /// otherwise it returns <c>false</c>.
    ///
    /// <para>
    /// The behavior of this operation is undefined if any of the specified graphs is modified while
    /// operation is in progress.
    /// </para>
    /// </summary>
    /// <param name="destination"> the graph to which vertices and edges are added.</param>
    /// <param name="source"> the graph used as source for vertices and edges to add.</param>
    /// <typeparam name="TSourceVertex">The source graph vertex type.</typeparam>
    /// <typeparam name="TSourceEdge">The source graph edge type.</typeparam>
    /// <typeparam name="TDestinationVertex">The destination graph vertex type.</typeparam>
    /// <typeparam name="TDestinationEdge">The destination graph edge type.</typeparam>
    /// <returns><c>true</c> if and only if the destination graph has been changed as a result
    ///         of this operation.</returns>
    public static bool AddGraph<TSourceVertex, TSourceEdge, TDestinationVertex, TDestinationEdge>(
        IGraph<TDestinationVertex, TDestinationEdge> destination,
        IGraph<TSourceVertex, TSourceEdge>           source)
        where TSourceVertex : class, TDestinationVertex
        where TSourceEdge : class, TDestinationEdge
        where TDestinationVertex : class
        where TDestinationEdge : class
    {
        ArgumentNullException.ThrowIfNull(source);

        var modified = AddAllVertices(destination, source.VertexSet());
        modified |= AddAllEdges(destination, source, source.EdgeSet());

        return modified;
    }

    /// <summary>
    /// Adds all the vertices and all the edges of the specified source digraph to the specified
    /// destination digraph, reversing all of the edges. If you want to do this as a linked view of
    /// the source graph (rather than by copying to a destination graph), use
    /// <see cref="EdgeReversedGraph{TVertex,TEdge}"/> instead.
    ///
    /// <para>
    /// The behavior of this operation is undefined if any of the specified graphs is modified while
    /// operation is in progress.
    /// </para>
    ///
    /// </summary>
    /// <param name="destination"> the graph to which vertices and edges are added.</param>
    /// <param name="source"> the graph used as source for vertices and edges to add.</param>
    /// <typeparam name="TSourceVertex">The source graph vertex type.</typeparam>
    /// <typeparam name="TSourceEdge">The source graph edge type.</typeparam>
    /// <typeparam name="TDestinationVertex">The destination graph vertex type.</typeparam>
    /// <typeparam name="TDestinationEdge">The destination graph edge type.</typeparam>
    /// <seealso cref="EdgeReversedGraph{TVertex,TEdge}"/>
    public static void AddGraphReversed<TSourceVertex, TSourceEdge, TDestinationVertex, TDestinationEdge>(
        IGraph<TDestinationVertex, TDestinationEdge> destination,
        IGraph<TSourceVertex, TSourceEdge>           source)
        where TSourceVertex : class, TDestinationVertex
        where TSourceEdge : class, TDestinationEdge
        where TDestinationVertex : class
        where TDestinationEdge : class
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(source);

        if (!destination.Type.IsDirected)
        {
            throw new ArgumentException("graph must be directed", nameof(destination));
        }

        if (!source.Type.IsDirected)
        {
            throw new ArgumentException("graph must be directed", nameof(source));
        }

        AddAllVertices(destination, source.VertexSet());

        foreach (var edge in source.EdgeSet())
        {
            destination.AddEdge(source.GetEdgeTarget(edge), source.GetEdgeSource(edge));
        }
    }

    /// <summary>
    /// Adds a subset of the edges of the specified source graph to the specified destination graph.
    /// The behavior of this operation is undefined if either of the graphs is modified while the
    /// operation is in progress. <see cref="AddEdgeWithVertices{TVertex,TEdge}(NGraphT.Core.IGraph{TVertex,TEdge},TVertex,TVertex)"/>
    /// is used for the transfer, so source vertexes will be added automatically to the target graph.
    /// </summary>
    /// <param name="destination"> the graph to which edges are to be added.</param>
    /// <param name="source"> the graph used as a source for edges to add.</param>
    /// <param name="edges"> the edges to be added.</param>
    /// <typeparam name="TSourceVertex">The source graph vertex type.</typeparam>
    /// <typeparam name="TSourceEdge">The source graph edge type.</typeparam>
    /// <typeparam name="TDestinationVertex">The destination graph vertex type.</typeparam>
    /// <typeparam name="TDestinationEdge">The destination graph edge type.</typeparam>
    /// <returns><c>true</c> if this graph changed as a result of the call.</returns>
    public static bool AddAllEdges<TSourceVertex, TSourceEdge, TDestinationVertex, TDestinationEdge>(
        IGraph<TDestinationVertex, TDestinationEdge> destination,
        IGraph<TSourceVertex, TSourceEdge>           source,
        ICollection<TSourceEdge>                     edges)
        where TSourceVertex : class, TDestinationVertex
        where TSourceEdge : class, TDestinationEdge
        where TDestinationVertex : class
        where TDestinationEdge : class
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(edges);

        var modified = false;

        foreach (var edge in edges)
        {
            var s = source.GetEdgeSource(edge);
            var t = source.GetEdgeTarget(edge);
            destination.AddVertex(s);
            destination.AddVertex(t);
            modified |= destination.AddEdge(s, t, edge);
        }

        return modified;
    }

    /// <summary>
    /// Adds all of the specified vertices to the destination graph. The behavior of this operation
    /// is undefined if the specified vertex collection is modified while the operation is in
    /// progress. This method will invoke the <see cref="IGraph{TVertex,TEdge}.AddVertex()"/> method.
    /// </summary>
    /// <param name="destination"> the graph to which edges are to be added.</param>
    /// <param name="vertices"> the vertices to be added to the graph.</param>
    /// <typeparam name="TVertexSource">Vertex type of vertices collection which we want to add to graph.</typeparam>
    /// <typeparam name="TVertexDestination">The destionation graph vertex type.</typeparam>
    /// <typeparam name="TEdgeDestination">The destination graph edge type.</typeparam>
    /// <returns><c>true</c> if graph changed as a result of the call.</returns>
    /// <exception cref="NullReferenceException">
    /// if the specified vertices contains one or more null vertices, or
    /// if the specified vertex collection is <c>null</c>.
    /// </exception>
    /// <seealso cref="IGraph{TVertex,TEdge}.AddVertex()"/>
    public static bool AddAllVertices<TVertexSource, TVertexDestination, TEdgeDestination>(
        IGraph<TVertexDestination, TEdgeDestination> destination,
        ICollection<TVertexSource>                   vertices)
        where TVertexSource : class, TVertexDestination
        where TVertexDestination : class
        where TEdgeDestination : class
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(vertices);

        var modified = false;
        foreach (var v in vertices)
        {
            modified |= destination.AddVertex(v);
        }

        return modified;
    }

    /// <summary>
    /// Returns a list of vertices that are the neighbors of a specified vertex. If the graph is a
    /// multigraph vertices may appear more than once in the returned list.
    ///
    /// <para>
    /// The method uses <see cref="IGraph{TVertex,TEdge}.EdgesOf"/> to traverse the graph.
    /// </para>
    ///
    /// </summary>
    /// <param name="g"> the graph to look for neighbors in.</param>
    /// <param name="vertex"> the vertex to get the neighbors of.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a list of the vertices that are the neighbors of the specified vertex.</returns>
    public static IList<TVertex> NeighborListOf<TVertex, TEdge>(IGraph<TVertex, TEdge> g, TVertex vertex)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(g);

        var neighbors = new List<TVertex>();
        foreach (var edge in g.Iterables().EdgesOf(vertex))
        {
            neighbors.Add(GetOppositeVertex(g, edge, vertex));
        }

        return neighbors;
    }

    /// <summary>
    /// Returns a set of vertices that are neighbors of a specified vertex.
    /// </summary>
    /// <param name="g"> the graph to look for neighbors in.</param>
    /// <param name="vertex"> the vertex to get the neighbors of.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a set of the vertices that are neighbors of the specified vertex.</returns>
    public static ISet<TVertex> NeighborSetOf<TVertex, TEdge>(IGraph<TVertex, TEdge> g, TVertex vertex)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(g);

        var neighbors = new Java2Net.LinkedHashSet<TVertex>();
        foreach (var edge in g.Iterables().EdgesOf(vertex))
        {
            neighbors.Add(GetOppositeVertex(g, edge, vertex));
        }

        return neighbors;
    }

    /// <summary>
    /// Returns a list of vertices that are the direct predecessors of a specified vertex. If the
    /// graph is a multigraph, vertices may appear more than once in the returned list.
    ///
    /// <para>
    /// The method uses <see cref="IGraph{TVertex,TEdge}.IncomingEdgesOf"/> to traverse the graph.
    /// </para>
    ///
    /// </summary>
    /// <param name="g"> the graph to look for predecessors in.</param>
    /// <param name="vertex"> the vertex to get the predecessors of.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a list of the vertices that are the direct predecessors of the specified vertex.</returns>
    public static IList<TVertex> PredecessorListOf<TVertex, TEdge>(IGraph<TVertex, TEdge> g, TVertex vertex)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(g);

        var predecessors = new List<TVertex>();
        foreach (var edge in g.Iterables().IncomingEdgesOf(vertex))
        {
            predecessors.Add(GetOppositeVertex(g, edge, vertex));
        }

        return predecessors;
    }

    /// <summary>
    /// Returns a list of vertices that are the direct successors of a specified vertex. If the graph
    /// is a multigraph vertices may appear more than once in the returned list.
    ///
    /// <para>
    /// The method uses <see cref="IGraph{TVertex,TEdge}.OutgoingEdgesOf"/> to traverse the graph.
    /// </para>
    ///
    /// </summary>
    /// <param name="g"> the graph to look for successors in.</param>
    /// <param name="vertex"> the vertex to get the successors of.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a list of the vertices that are the direct successors of the specified vertex.</returns>
    public static IList<TVertex> SuccessorListOf<TVertex, TEdge>(IGraph<TVertex, TEdge> g, TVertex vertex)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(g);

        var successors = new List<TVertex>();
        foreach (var edge in g.Iterables().OutgoingEdgesOf(vertex))
        {
            successors.Add(GetOppositeVertex(g, edge, vertex));
        }

        return successors;
    }

    /// <summary>
    /// Returns an undirected view of the specified graph. If the specified graph is directed,
    /// returns an undirected view of it. If the specified graph is already undirected, just returns it.
    /// </summary>
    /// <param name="g"> the graph for which an undirected view is to be returned.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>
    /// an undirected view of the specified graph, if it is directed, or or the specified
    /// graph itself if it is already undirected.
    /// </returns>
    /// <exception cref="ArgumentException"> if the graph is neither directed nor undirected.</exception>
    /// <seealso cref="AsUndirectedGraph{TVertex,TEdge}"/>
    public static IGraph<TVertex, TEdge> UndirectedGraph<TVertex, TEdge>(IGraph<TVertex, TEdge> g)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(g);

        if (g.Type.IsDirected)
        {
            return new AsUndirectedGraph<TVertex, TEdge>(g);
        }
        else if (g.Type.IsUndirected)
        {
            return g;
        }
        else
        {
            throw new ArgumentException("graph must be either directed or undirected", nameof(g));
        }
    }

    /// <summary>
    /// Tests whether an edge is incident to a vertex.
    /// </summary>
    /// <param name="g"> graph containing TEdge and TVertex.</param>
    /// <param name="edge"> edge in g.</param>
    /// <param name="v"> vertex in g.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true iff TEdge is incident on TVertex.</returns>
    public static bool TestIncidence<TVertex, TEdge>(IGraph<TVertex, TEdge> g, TEdge edge, TVertex v)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(g);
        return Equals(g.GetEdgeSource(edge), v) || Equals(g.GetEdgeTarget(edge), v);
    }

    /// <summary>
    /// Gets the vertex opposite another vertex across an edge.
    /// </summary>
    /// <param name="g"> graph containing TEdge and TVertex.</param>
    /// <param name="edge"> edge in g.</param>
    /// <param name="v"> vertex in g.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>vertex opposite to TVertex across TEdge.</returns>
    public static TVertex GetOppositeVertex<TVertex, TEdge>(IGraph<TVertex, TEdge> g, TEdge edge, TVertex v)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(g);
        var source = g.GetEdgeSource(edge);
        var target = g.GetEdgeTarget(edge);
        if (Equals(v, source))
        {
            return target;
        }
        else if (Equals(v, target))
        {
            return source;
        }
        else
        {
            throw new ArgumentException("no such vertex", nameof(v));
        }
    }

    /// <summary>
    /// Removes the given vertex from the given graph. If the vertex to be removed has one or more
    /// predecessors, the predecessors will be connected directly to the successors of the vertex to
    /// be removed.
    /// </summary>
    /// <param name="graph"> graph to be mutated.</param>
    /// <param name="vertex"> vertex to be removed from this graph, if present.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true if the graph contained the specified vertex; false otherwise.</returns>
    public static bool RemoveVertexAndPreserveConnectivity<TVertex, TEdge>(IGraph<TVertex, TEdge> graph, TVertex vertex)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(graph);

        if (!graph.ContainsVertex(vertex))
        {
            return false;
        }

        if (VertexHasPredecessors(graph, vertex))
        {
            var predecessors = PredecessorListOf(graph, vertex);
            var successors   = SuccessorListOf(graph, vertex);

            foreach (var predecessor in predecessors)
            {
                AddOutgoingEdges(graph, predecessor, successors);
            }
        }

        graph.RemoveVertex(vertex);
        return true;
    }

    /// <summary>
    /// Filters vertices from the given graph and subsequently removes them. If the vertex to be
    /// removed has one or more predecessors, the predecessors will be connected directly to the
    /// successors of the vertex to be removed.
    /// </summary>
    /// <param name="graph"> graph to be mutated.</param>
    /// <param name="predicate"> a non-interfering stateless predicate to apply to each vertex to determine
    ///        if it should be removed from the graph.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true if at least one vertex has been removed; false otherwise.</returns>
    public static bool RemoveVerticesAndPreserveConnectivity<TVertex, TEdge>(
        IGraph<TVertex, TEdge> graph,
        Predicate<TVertex>     predicate)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(graph);
        var verticesToRemove = graph.VertexSet().Where(it => predicate(it)).ToList();
        return RemoveVerticesAndPreserveConnectivity(graph, verticesToRemove);
    }

    /// <summary>
    /// Removes all the given vertices from the given graph. If the vertex to be removed has one or
    /// more predecessors, the predecessors will be connected directly to the successors of the
    /// vertex to be removed.
    /// </summary>
    /// <param name="graph"> to be mutated.</param>
    /// <param name="vertices"> vertices to be removed from this graph, if present.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true if at least one vertex has been removed; false otherwise.</returns>
    public static bool RemoveVerticesAndPreserveConnectivity<TVertex, TEdge>(
        IGraph<TVertex, TEdge> graph,
        IEnumerable<TVertex>   vertices)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(vertices);

        var atLeastOneVertexHasBeenRemoved = false;
        foreach (var vertex in vertices)
        {
            if (RemoveVertexAndPreserveConnectivity(graph, vertex))
            {
                atLeastOneVertexHasBeenRemoved = true;
            }
        }

        return atLeastOneVertexHasBeenRemoved;
    }

    /// <summary>
    /// Add edges from one source vertex to multiple target vertices. Whether duplicates are created
    /// depends on the underlying <see cref="Graph"/> implementation.
    /// </summary>
    /// <param name="graph"> graph to be mutated.</param>
    /// <param name="source"> source vertex of the new edges.</param>
    /// <param name="targets"> target vertices for the new edges.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    public static void AddOutgoingEdges<TVertex, TEdge>(
        IGraph<TVertex, TEdge> graph,
        TVertex                source,
        IEnumerable<TVertex>   targets)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(targets);

        if (!graph.ContainsVertex(source))
        {
            graph.AddVertex(source);
        }

        foreach (var target in targets)
        {
            if (!graph.ContainsVertex(target))
            {
                graph.AddVertex(target);
            }

            graph.AddEdge(source, target);
        }
    }

    /// <summary>
    /// Add edges from multiple source vertices to one target vertex. Whether duplicates are created
    /// depends on the underlying <see cref="Graph"/> implementation.
    /// </summary>
    /// <param name="graph"> graph to be mutated.</param>
    /// <param name="target"> target vertex for the new edges.</param>
    /// <param name="sources"> source vertices for the new edges.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    public static void AddIncomingEdges<TVertex, TEdge>(
        IGraph<TVertex, TEdge> graph,
        TVertex                target,
        IEnumerable<TVertex>   sources)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(sources);

        if (!graph.ContainsVertex(target))
        {
            graph.AddVertex(target);
        }

        foreach (var source in sources)
        {
            if (!graph.ContainsVertex(source))
            {
                graph.AddVertex(source);
            }

            graph.AddEdge(source, target);
        }
    }

    /// <summary>
    /// Check if a vertex has any direct successors.
    /// </summary>
    /// <param name="graph"> the graph to look for successors.</param>
    /// <param name="vertex"> the vertex to look for successors.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true if the vertex has any successors, false otherwise.</returns>
    public static bool VertexHasSuccessors<TVertex, TEdge>(IGraph<TVertex, TEdge> graph, TVertex vertex)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(graph);
        return graph.OutgoingEdgesOf(vertex).Count > 0;
    }

    /// <summary>
    /// Check if a vertex has any direct predecessors.
    /// </summary>
    /// <param name="graph"> the graph to look for predecessors.</param>
    /// <param name="vertex"> the vertex to look for predecessors.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true if the vertex has any predecessors, false otherwise.</returns>
    public static bool VertexHasPredecessors<TVertex, TEdge>(IGraph<TVertex, TEdge> graph, TVertex vertex)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(graph);
        return graph.IncomingEdgesOf(vertex).Count > 0;
    }

    /// <summary>
    /// Compute a new mapping from the vertices of a graph to the integer range $[0, n)$ where $n$ is
    /// the number of vertices in the graph.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <exception cref="NullReferenceException"> if <c>graph</c> is <c>null</c>.</exception>
    /// <returns>the mapping as an object containing the <c>vertexMap</c> and the <c>indexList</c>.</returns>
    /// <seealso cref="VertexToIntegerMapping{TVertex}"/>
    public static VertexToIntegerMapping<TVertex> GetVertexToIntegerMapping<TVertex, TEdge>(
        IGraph<TVertex, TEdge> graph)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(graph);
        return new VertexToIntegerMapping<TVertex>(graph.VertexSet());
    }
}
