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

namespace NGraphT.Core.Graph.Builders;

/// <summary>
/// Base class for builders of <see cref="Graph"/>.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
/// <typeparam name="TGraph">Type of the resulting graph.</typeparam>
/// <typeparam name="TBuilder">Type of this builder.</typeparam>
///
/// <remarks>Author: Andrew Chen.</remarks>
public abstract class AbstractGraphBuilder<TVertex, TEdge, TGraph, TBuilder>
    where TGraph : IGraph<TVertex, TEdge>
    where TBuilder : AbstractGraphBuilder<TVertex, TEdge, TGraph, TBuilder>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Creates a builder based on <c>baseGraph</c>. <c>baseGraph</c> must be mutable.
    /// </summary>
    /// <param name="baseGraph"> the graph object to base building on.</param>
    protected AbstractGraphBuilder(TGraph baseGraph)
    {
        Graph = baseGraph;
    }

    protected TGraph Graph { get; init; }

    protected abstract TBuilder Self { get; }

    /// <summary>
    /// Adds <c>vertex</c> to the graph being built.
    /// </summary>
    /// <param name="vertex"> the vertex to add.</param>>
    /// <returns>this builder object.</returns>
    /// <see cref="IGraph{TVertex,TEdge}.AddVertex()"/>
    public virtual TBuilder AddVertex(TVertex vertex)
    {
        Graph.AddVertex(vertex);
        return Self;
    }

    /// <summary>
    /// Adds each vertex of <c>vertices</c> to the graph being built.
    /// </summary>
    /// <param name="vertices"> the vertices to add.</param>>
    /// <returns>this builder object.</returns>
    /// <see cref="IGraph{TVertex,TEdge}.AddVertex(TVertex)"/>
    public TBuilder AddVertices(params TVertex[] vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        foreach (var vertex in vertices)
        {
            AddVertex(vertex);
        }

        return Self;
    }

    /// <summary>
    /// Adds an edge to the graph being built. The source and target vertices are added to the graph,
    /// if not already included.
    /// </summary>
    /// <param name="source"> source vertex of the edge.</param>
    /// <param name="target"> target vertex of the edge.</param>
    /// <returns>this builder object.</returns>
    /// <see cref="Graphs.AddEdgeWithVertices{TVertex,TEdge}(NGraphT.Core.IGraph{TVertex,TEdge},TVertex,TVertex)"/>
    public virtual TBuilder AddEdge(TVertex source, TVertex target)
    {
        Graphs.AddEdgeWithVertices(Graph, source, target);
        return Self;
    }

    /// <summary>
    /// Adds the specified edge to the graph being built. The source and target vertices are added to
    /// the graph, if not already included.
    /// </summary>
    /// <param name="source"> source vertex of the edge.</param>
    /// <param name="target"> target vertex of the edge.</param>
    /// <param name="edge"> edge to be added to this graph.</param>
    /// <returns>this builder object.</returns>
    /// <see cref="IGraph{TVertex,TEdge}.AddEdge(TVertex,TVertex,TEdge)"/>
    public virtual TBuilder AddEdge(TVertex source, TVertex target, TEdge edge)
    {
        AddVertex(source);
        AddVertex(target);
        Graph.AddEdge(source, target, edge);
        return Self;
    }

    /// <summary>
    /// Adds an weighted edge to the graph being built. The source and target vertices are added to
    /// the graph, if not already included.
    /// </summary>
    /// <param name="source"> source vertex of the edge.</param>
    /// <param name="target"> target vertex of the edge.</param>
    /// <param name="weight"> weight of the edge.</param>
    /// <returns>this builder object.</returns>
    /// <seealso cref="Graphs.AddEdgeWithVertices{TVertex,TEdge}(NGraphT.Core.IGraph{TVertex,TEdge},TVertex,TVertex)"/>
    public virtual TBuilder AddEdge(TVertex source, TVertex target, double weight)
    {
        Graphs.AddEdgeWithVertices(Graph, source, target, weight);
        return Self;
    }

    /// <summary>
    /// Adds the specified weighted edge to the graph being built. The source and target vertices are
    /// added to the graph, if not already included.
    /// </summary>
    /// <param name="source"> source vertex of the edge.</param>
    /// <param name="target"> target vertex of the edge.</param>
    /// <param name="edge"> edge to be added to this graph.</param>
    /// <param name="weight"> weight of the edge.</param>
    /// <returns>this builder object.</returns>
    /// <seealso cref="AddEdge(TVertex,TVertex)"/>
    /// <seealso cref="IGraph{TVertex,TEdge}.SetEdgeWeight(TEdge,double)"/>
    public virtual TBuilder AddEdge(TVertex source, TVertex target, TEdge edge, double weight)
    {
        AddEdge(source, target, edge); // adds vertices if needed
        Graph.SetEdgeWeight(edge, weight);
        return Self;
    }

    /// <summary>
    /// Adds a chain of edges to the graph being built. The vertices are added to the graph, if not
    /// already included.
    /// </summary>
    /// <param name="first"> the first vertex.</param>
    /// <param name="second"> the second vertex.</param>
    /// <param name="rest"> the remaining vertices.</param>
    /// <returns>this builder object.</returns>
    /// <see cref="AddEdge(TVertex,TVertex)"/>
    public TBuilder AddEdgeChain(TVertex first, TVertex second, params TVertex[] rest)
    {
        ArgumentNullException.ThrowIfNull(rest);

        AddEdge(first, second);
        var last = second;
        foreach (var vertex in rest)
        {
            AddEdge(last, vertex);
            last = vertex;
        }

        return Self;
    }

    /// <summary>
    /// Adds all the vertices and all the edges of the <c>sourceGraph</c> to the graph being built.
    /// </summary>
    /// <param name="sourceGraph"> the source graph.</param>
    /// <returns>this builder object.</returns>
    /// <see cref="Graphs.AddGraph{TSourceVertex,TSourceEdge,TDestinationVertex,TDestinationEdge}"/>
    public virtual TBuilder AddGraph<TSourceVertex, TSourceEdge>(IGraph<TSourceVertex, TSourceEdge> sourceGraph)
        where TSourceVertex : class, TVertex
        where TSourceEdge : class, TEdge
    {
        Graphs.AddGraph(Graph, sourceGraph);
        return Self;
    }

    /// <summary>
    /// Removes <c>vertex</c> from the graph being built, if such vertex exist in graph.
    /// </summary>
    /// <param name="vertex"> the vertex to remove.</param>>
    /// <returns>this builder object.</returns>
    /// <see cref="IGraph{TVertex,TEdge}.RemoveVertex"/>
    public virtual TBuilder RemoveVertex(TVertex vertex)
    {
        Graph.RemoveVertex(vertex);
        return Self;
    }

    /// <summary>
    /// Removes each vertex of <c>vertices</c> from the graph being built, if such vertices exist in
    /// graph.
    /// </summary>
    /// <param name="vertices"> the vertices to remove.</param>>
    /// <returns>this builder object.</returns>
    /// <see cref="RemoveVertex"/>
    public TBuilder RemoveVertices(params TVertex[] vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        foreach (var vertex in vertices)
        {
            RemoveVertex(vertex);
        }

        return Self;
    }

    /// <summary>
    /// Removes an edge going from source vertex to target vertex from the graph being built, if such
    /// vertices and such edge exist in the graph.
    /// </summary>
    /// <param name="source"> source vertex of the edge.</param>
    /// <param name="target"> target vertex of the edge.</param>
    /// <returns>this builder object.</returns>
    /// <seealso cref="IGraph{TVertex,TEdge}.RemoveVertex"/>
    public virtual TBuilder RemoveEdge(TVertex source, TVertex target)
    {
        Graph.RemoveEdge(source, target);
        return Self;
    }

    /// <summary>
    /// Removes the specified edge from the graph. Removes the specified edge from this graph if it
    /// is present.
    /// </summary>
    /// <param name="edge"> edge to be removed from this graph, if present.</param>
    /// <returns>this builder object.</returns>
    /// <seealso cref="IGraph{TVertex,TEdge}.RemoveEdge(TVertex,TVertex)"/>
    public virtual TBuilder RemoveEdge(TEdge edge)
    {
        Graph.RemoveEdge(edge);
        return Self;
    }

    /// <summary>
    /// Build the graph. Calling any method (including this method) on this builder object after
    /// calling this method is undefined behaviour.
    /// </summary>
    /// <returns>the built graph.</returns>
    public virtual TGraph Build()
    {
        return Graph;
    }

    /// <summary>
    /// Build an unmodifiable version graph. Calling any method (including this method) on this
    /// builder object after calling this method is undefined behaviour.
    /// </summary>
    /// <returns>the built unmodifiable graph.</returns>
    /// <seealso cref="Build"/>
    public virtual IGraph<TVertex, TEdge> BuildAsUnmodifiable()
    {
        return new AsUnmodifiableGraph<TVertex, TEdge>(Graph);
    }
}
