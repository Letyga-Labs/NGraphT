/*
 * (C) Copyright 2003-2021, by Barak Naveh and Contributors.
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

namespace NGraphT.Core;

using NGraphT.Core.Graph;
using NGraphT.Core.Util;

/// <summary>
/// A collection of utilities to assist with graph manipulation.
/// </summary>
///
/// <remarks>Author: Barak Naveh.</remarks>
public abstract class Graphs
{
    /// <summary>
    /// Creates a new edge and adds it to the specified graph similarly to the
    /// <see cref="IGraph{TNode,TEdge}.AddEdge(TNode,TNode)"/> method.
    /// </summary>
    /// <param name="g"> the graph for which the edge to be added.</param>
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    /// <param name="weight"> weight of the edge.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>The newly created edge if added to the graph, otherwise <c>
    /// null</c>.</returns>
    /// <exception cref="NotSupportedException"> if the graph has no edge supplier.</exception>
    /// <see cref="IGraph{TNode,TEdge}.AddEdge(TNode,TNode)"/>
    public static TEdge AddEdge<TNode, TEdge>(IGraph<TNode, TEdge> g, TNode sourceVertex, TNode targetVertex, double weight)
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
            return default(TEdge);
        }
    }

    /// <summary>
    /// Adds the specified source and target vertices to the graph, if not already included, and
    /// creates a new edge and adds it to the specified graph similarly to the
    /// <see cref="IGraph{TNode,TEdge}.AddEdge(TNode,TNode)"/> method.
    /// </summary>
    /// <param name="g"> the graph for which the specified edge to be added.</param>
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>The newly created edge if added to the graph, otherwise <c>
    /// null</c>.</returns>
    public static TEdge AddEdgeWithVertices<TNode, TEdge>(IGraph<TNode, TEdge> g, TNode sourceVertex, TNode targetVertex)
    {
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
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns><c>true</c> if the target graph did not already contain the specified edge.</returns>
    public static bool AddEdgeWithVertices<TNode, TEdge>(IGraph<TNode, TEdge> targetGraph, IGraph<TNode, TEdge> sourceGraph, TEdge edge)
    {
        var sourceVertex = sourceGraph.GetEdgeSource(edge);
        var targetVertex = sourceGraph.GetEdgeTarget(edge);

        targetGraph.AddVertex(sourceVertex);
        targetGraph.AddVertex(targetVertex);

        return targetGraph.AddEdge(sourceVertex, targetVertex, edge);
    }

    /// <summary>
    /// Adds the specified source and target vertices to the graph, if not already included, and
    /// creates a new weighted edge and adds it to the specified graph similarly to the
    /// <see cref="IGraph{TNode,TEdge}.AddEdge(TNode,TNode)"/> method.
    /// </summary>
    /// <param name="g"> the graph for which the specified edge to be added.</param>
    /// <param name="sourceVertex"> source vertex of the edge.</param>
    /// <param name="targetVertex"> target vertex of the edge.</param>
    /// <param name="weight"> weight of the edge.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>The newly created edge if added to the graph, otherwise <c>
    /// null</c>.</returns>
    public static TEdge AddEdgeWithVertices<TNode, TEdge>(IGraph<TNode, TEdge> g, TNode sourceVertex, TNode targetVertex, double weight)
    {
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
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns><c>true</c> if and only if the destination graph has been changed as a result
    ///         of this operation.</returns>
//JAVA TO C# CONVERTER TODO TASK: There is no C# equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <TNode, TEdge> boolean addGraph(Graph<? super TNode, ? super TEdge> destination, Graph<TNode, TEdge> source)
    public static bool AddGraph<TNode, TEdge, T1, T2>(IGraph<T1, T2> destination, IGraph<TNode, TEdge> source)
    {
        var modified = AddAllVertices(destination, source.VertexSet());
        modified |= AddAllEdges(destination, source, source.EdgeSet());

        return modified;
    }

    /// <summary>
    /// Adds all the vertices and all the edges of the specified source digraph to the specified
    /// destination digraph, reversing all of the edges. If you want to do this as a linked view of
    /// the source graph (rather than by copying to a destination graph), use
    /// <see cref="EdgeReversedGraph"/> instead.
    ///
    /// <para>
    /// The behavior of this operation is undefined if any of the specified graphs is modified while
    /// operation is in progress.
    /// </para>
    ///
    /// </summary>
    /// <param name="destination"> the graph to which vertices and edges are added.</param>
    /// <param name="source"> the graph used as source for vertices and edges to add.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <seealso cref="EdgeReversedGraph"/>
//JAVA TO C# CONVERTER TODO TASK: There is no C# equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <TNode, TEdge> void addGraphReversed(Graph<? super TNode, ? super TEdge> destination, Graph<TNode, TEdge> source)
    public static void AddGraphReversed<TNode, TEdge, T1, T2>(IGraph<T1, T2> destination, IGraph<TNode, TEdge> source)
    {
        if (!source.Type.Directed || !destination.Type.Directed)
        {
            throw new ArgumentException("graph must be directed");
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
    /// operation is in progress. <see cref="addEdgeWithVertices"/> is used for the transfer, so source
    /// vertexes will be added automatically to the target graph.
    /// </summary>
    /// <param name="destination"> the graph to which edges are to be added.</param>
    /// <param name="source"> the graph used as a source for edges to add.</param>
    /// <param name="edges"> the edges to be added.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns><c>true</c> if this graph changed as a result of the call.</returns>
//JAVA TO C# CONVERTER TODO TASK: There is no C# equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <TNode, TEdge> boolean addAllEdges(Graph<? super TNode, ? super TEdge> destination, Graph<TNode, TEdge> source, Collection<? extends TEdge> edges)
    public static bool AddAllEdges<TNode, TEdge, T1, T2, T3>(
        IGraph<T1, T2>   destination,
        IGraph<TNode, TEdge>     source,
        ICollection<T3> edges
    ) where T3 : TEdge
    {
        var modified = false;

        foreach (TEdge edge in edges)
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
    /// progress. This method will invoke the <see cref="Graph.addVertex(Object)"/> method.
    /// </summary>
    /// <param name="destination"> the graph to which edges are to be added.</param>
    /// <param name="vertices"> the vertices to be added to the graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns><c>true</c> if graph changed as a result of the call.</returns>
    /// <exception cref="NullReferenceException"> if the specified vertices contains one or more null vertices, or
    ///         if the specified vertex collection is <c>
    /// null</c>.</exception>
    /// <see cref="Graph.addVertex(Object)"/>
//JAVA TO C# CONVERTER TODO TASK: There is no C# equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <TNode, TEdge> boolean addAllVertices(Graph<? super TNode, ? super TEdge> destination, Collection<? extends TNode> vertices)
    public static bool AddAllVertices<TNode, TEdge, T1, T2, T3>(IGraph<T1, T2> destination, ICollection<T3> vertices)
        where T3 : TNode
    {
        var modified = false;

        foreach (TNode node in vertices)
        {
            modified |= destination.AddVertex(node);
        }

        return modified;
    }

    /// <summary>
    /// Returns a list of vertices that are the neighbors of a specified vertex. If the graph is a
    /// multigraph vertices may appear more than once in the returned list.
    ///
    /// <para>
    /// The method uses <see cref="Graph.edgesOf(Object)"/> to traverse the graph.
    /// </para>
    ///
    /// </summary>
    /// <param name="g"> the graph to look for neighbors in.</param>
    /// <param name="vertex"> the vertex to get the neighbors of.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a list of the vertices that are the neighbors of the specified vertex.</returns>
    public static IList<TNode> NeighborListOf<TNode, TEdge>(IGraph<TNode, TEdge> g, TNode vertex)
    {
        IList<TNode> neighbors = new List<TNode>();

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
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>a set of the vertices that are neighbors of the specified vertex.</returns>
    public static ISet<TNode> NeighborSetOf<TNode, TEdge>(IGraph<TNode, TEdge> g, TNode vertex)
    {
        ISet<TNode> neighbors = new LinkedHashSet<TNode>();

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
    /// The method uses <see cref="Graph.incomingEdgesOf(Object)"/> to traverse the graph.
    /// </para>
    ///
    /// </summary>
    /// <param name="g"> the graph to look for predecessors in.</param>
    /// <param name="vertex"> the vertex to get the predecessors of.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a list of the vertices that are the direct predecessors of the specified vertex.</returns>
    public static IList<TNode> PredecessorListOf<TNode, TEdge>(IGraph<TNode, TEdge> g, TNode vertex)
    {
        IList<TNode> predecessors = new List<TNode>();

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
    /// The method uses <see cref="Graph.outgoingEdgesOf(Object)"/> to traverse the graph.
    /// </para>
    ///
    /// </summary>
    /// <param name="g"> the graph to look for successors in.</param>
    /// <param name="vertex"> the vertex to get the successors of.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a list of the vertices that are the direct successors of the specified vertex.</returns>
    public static IList<TNode> SuccessorListOf<TNode, TEdge>(IGraph<TNode, TEdge> g, TNode vertex)
    {
        IList<TNode> successors = new List<TNode>();

        foreach (var edge in g.Iterables().OutgoingEdgesOf(vertex))
        {
            successors.Add(GetOppositeVertex(g, edge, vertex));
        }

        return successors;
    }

    /// <summary>
    /// Returns an undirected view of the specified graph. If the specified graph is directed,
    /// returns an undirected view of it. If the specified graph is already undirected, just returns
    /// it.
    /// </summary>
    /// <param name="g"> the graph for which an undirected view is to be returned.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>an undirected view of the specified graph, if it is directed, or or the specified
    ///         graph itself if it is already undirected.</returns>
    /// <exception cref="ArgumentException"> if the graph is neither directed nor undirected.</exception>
    /// <seealso cref="AsUndirectedGraph"/>
    public static IGraph<TNode, TEdge> UndirectedGraph<TNode, TEdge>(IGraph<TNode, TEdge> g)
    {
        if (g.Type.Directed)
        {
            return new AsUndirectedGraph<TNode, TEdge>(g);
        }
        else if (g.Type.Undirected)
        {
            return g;
        }
        else
        {
            throw new ArgumentException("graph must be either directed or undirected");
        }
    }

    /// <summary>
    /// Tests whether an edge is incident to a vertex.
    /// </summary>
    /// <param name="g"> graph containing TEdge and TNode.</param>
    /// <param name="edge"> edge in g.</param>
    /// <param name="node"> vertex in g.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true iff TEdge is incident on TNode.</returns>
    public static bool TestIncidence<TNode, TEdge>(IGraph<TNode, TEdge> g, TEdge edge, TNode node)
    {
        return (g.GetEdgeSource(TEdge).Equals(TNode)) || (g.GetEdgeTarget(TEdge).Equals(TNode));
    }

    /// <summary>
    /// Gets the vertex opposite another vertex across an edge.
    /// </summary>
    /// <param name="g"> graph containing TEdge and TNode.</param>
    /// <param name="edge"> edge in g.</param>
    /// <param name="node"> vertex in g.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>vertex opposite to TNode across TEdge.</returns>
    public static TNode GetOppositeVertex<TNode, TEdge>(IGraph<TNode, TEdge> g, TEdge edge, TNode node)
    {
        var source = g.GetEdgeSource(TEdge);
        var target = g.GetEdgeTarget(TEdge);
        if (TNode.Equals(source))
        {
            return target;
        }
        else if (TNode.Equals(target))
        {
            return source;
        }
        else
        {
            throw new ArgumentException("no such vertex: " + TNode.ToString());
        }
    }

    /// <summary>
    /// Removes the given vertex from the given graph. If the vertex to be removed has one or more
    /// predecessors, the predecessors will be connected directly to the successors of the vertex to
    /// be removed.
    /// </summary>
    /// <param name="graph"> graph to be mutated.</param>
    /// <param name="vertex"> vertex to be removed from this graph, if present.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true if the graph contained the specified vertex; false otherwise.</returns>
    public static bool RemoveVertexAndPreserveConnectivity<TNode, TEdge>(IGraph<TNode, TEdge> graph, TNode vertex)
    {
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
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true if at least one vertex has been removed; false otherwise.</returns>
    public static bool RemoveVerticesAndPreserveConnectivity<TNode, TEdge>(IGraph<TNode, TEdge> graph, Predicate<TNode> predicate)
    {
        IList<TNode> verticesToRemove = new List<TNode>();

        foreach (var node in graph.VertexSet())
        {
            if (predicate(node))
            {
                verticesToRemove.Add(node);
            }
        }

        return removeVertexAndPreserveConnectivity(graph, verticesToRemove);
    }

    /// <summary>
    /// Removes all the given vertices from the given graph. If the vertex to be removed has one or
    /// more predecessors, the predecessors will be connected directly to the successors of the
    /// vertex to be removed.
    /// </summary>
    /// <param name="graph"> to be mutated.</param>
    /// <param name="vertices"> vertices to be removed from this graph, if present.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true if at least one vertex has been removed; false otherwise.</returns>
    public static bool RemoveVertexAndPreserveConnectivity<TNode, TEdge>(IGraph<TNode, TEdge> graph, IEnumerable<TNode> vertices)
    {
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
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    public static void AddOutgoingEdges<TNode, TEdge>(IGraph<TNode, TEdge> graph, TNode source, IEnumerable<TNode> targets)
    {
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
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    public static void AddIncomingEdges<TNode, TEdge>(IGraph<TNode, TEdge> graph, TNode target, IEnumerable<TNode> sources)
    {
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
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true if the vertex has any successors, false otherwise.</returns>
    public static bool VertexHasSuccessors<TNode, TEdge>(IGraph<TNode, TEdge> graph, TNode vertex)
    {
        return graph.OutgoingEdgesOf(vertex).Count > 0;
    }

    /// <summary>
    /// Check if a vertex has any direct predecessors.
    /// </summary>
    /// <param name="graph"> the graph to look for predecessors.</param>
    /// <param name="vertex"> the vertex to look for predecessors.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>true if the vertex has any predecessors, false otherwise.</returns>
    public static bool VertexHasPredecessors<TNode, TEdge>(IGraph<TNode, TEdge> graph, TNode vertex)
    {
        return graph.IncomingEdgesOf(vertex).Count > 0;
    }

    /// <summary>
    /// Compute a new mapping from the vertices of a graph to the integer range $[0, n)$ where $n$ is
    /// the number of vertices in the graph.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <exception cref="NullReferenceException"> if {@code graph} is {@code null}.</exception>
    /// <returns>the mapping as an object containing the {@code vertexMap} and the {@code indexList}</returns>
    /// <seealso cref="VertexToIntegerMapping"/>
    public static VertexToIntegerMapping<TNode> GetVertexToIntegerMapping<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        return new VertexToIntegerMapping<TNode>(Objects.requireNonNull(graph).vertexSet());
    }
}
