/*
 * (C) Copyright 2015-2021, by Andrew Chen and Contributors.
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

namespace NGraphT.Core.Graph.Builder;

using Core;
using Graph;

/// <summary>
/// Base class for builders of <see cref="Graph"/>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
/// <typeparam name="G"> type of the resulting graph.</typeparam>
/// @param <B> type of this builder
///
/// <remarks>Author: Andrew Chen.</remarks>
public abstract class AbstractGraphBuilder<TNode, TEdge, TG, TB> where TG : IGraph<TNode, TEdge>
    where TB : AbstractGraphBuilder<TNode, TEdge, TG, TB>
{
    protected internal readonly TG Graph;

    ///<summary>
    ///Creates a builder based on {@code baseGraph}. {@code baseGraph} must be mutable.
    ///</summary>
    ///<param name="baseGraph"> the graph object to base building on.</param>
    public AbstractGraphBuilder(TG baseGraph)
    {
        Graph = baseGraph;
    }

    ///<returns>the {@code this} object.</returns>
    protected internal abstract TB Self();

    ///<summary>
    ///Adds {@code vertex} to the graph being built.
    ///</summary>
    ///<param name="vertex"> the vertex to add.</param>>
    ///<returns>this builder object.</returns>
    ///<see cref="Graph.addVertex(Object)"/>
    public virtual TB AddVertex(TNode vertex)
    {
        Graph.addVertex(vertex);
        return Self();
    }

    ///<summary>
    ///Adds each vertex of {@code vertices} to the graph being built.
    ///</summary>
    ///<param name="vertices"> the vertices to add.</param>>
    ///<returns>this builder object.</returns>
    ///<see cref=".addVertex(Object)"/>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public final B addVertices(TNode... vertices)
    public TB AddVertices(params TNode[] vertices)
    {
        foreach (var vertex in vertices)
        {
            AddVertex(vertex);
        }

        return Self();
    }

    ///<summary>
    ///Adds an edge to the graph being built. The source and target vertices are added to the graph,
    ///if not already included.
    ///</summary>
    ///<param name="source"> source vertex of the edge.</param>
    ///<param name="target"> target vertex of the edge.</param>
    ///<returns>this builder object.</returns>
    ///<see cref="Graphs.addEdgeWithVertices(Graph, Object, Object)"/>
    public virtual TB AddEdge(TNode source, TNode target)
    {
        Graphs.addEdgeWithVertices(Graph, source, target);
        return Self();
    }

    ///<summary>
    ///Adds the specified edge to the graph being built. The source and target vertices are added to
    ///the graph, if not already included.
    ///</summary>
    ///<param name="source"> source vertex of the edge.</param>
    ///<param name="target"> target vertex of the edge.</param>
    ///<param name="edge"> edge to be added to this graph.</param>
    ///<returns>this builder object.</returns>
    ///<see cref="Graph.addEdge(Object, Object, Object)"/>
    public virtual TB AddEdge(TNode source, TNode target, TEdge edge)
    {
        AddVertex(source);
        AddVertex(target);
        Graph.addEdge(source, target, edge);
        return Self();
    }

    ///<summary>
    ///Adds a chain of edges to the graph being built. The vertices are added to the graph, if not
    ///already included.
    ///</summary>
    ///<param name="first"> the first vertex.</param>
    ///<param name="second"> the second vertex.</param>
    ///<param name="rest"> the remaining vertices.</param>
    ///<returns>this builder object.</returns>
    ///<see cref=".addEdge(Object, Object)"/>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public final B addEdgeChain(TNode first, TNode second, TNode... rest)
    public TB AddEdgeChain(TNode first, TNode second, params TNode[] rest)
    {
        AddEdge(first, second);
        var last = second;
        foreach (var vertex in rest)
        {
            AddEdge(last, vertex);
            last = vertex;
        }

        return Self();
    }

    ///<summary>
    ///Adds all the vertices and all the edges of the {@code sourceGraph} to the graph being built.
    ///</summary>
    ///<param name="sourceGraph"> the source graph.</param>
    ///<returns>this builder object.</returns>
    ///<see cref="Graphs.addGraph(Graph, Graph)"/>
    public virtual TB AddGraph<T1, T2>(IGraph<T1, T2> sourceGraph) where T1 : TNode where T2 : TEdge
    {
        Graphs.AddGraph(Graph, sourceGraph);
        return Self();
    }

    ///<summary>
    ///Removes {@code vertex} from the graph being built, if such vertex exist in graph.
    ///</summary>
    ///<param name="vertex"> the vertex to remove.</param>>
    ///<returns>this builder object.</returns>
    ///<see cref="Graph.removeVertex(Object)"/>
    public virtual TB RemoveVertex(TNode vertex)
    {
        Graph.removeVertex(vertex);
        return Self();
    }

    ///<summary>
    ///Removes each vertex of {@code vertices} from the graph being built, if such vertices exist in
    ///graph.
    ///</summary>
    ///<param name="vertices"> the vertices to remove.</param>>
    ///<returns>this builder object.</returns>
    ///<see cref=".removeVertex(Object)"/>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public final B removeVertices(TNode... vertices)
    public TB RemoveVertices(params TNode[] vertices)
    {
        foreach (var vertex in vertices)
        {
            RemoveVertex(vertex);
        }

        return Self();
    }

    ///<summary>
    ///Removes an edge going from source vertex to target vertex from the graph being built, if such
    /// vertices and such edge exist in the graph.
    /// </summary>
    /// <param name="source"> source vertex of the edge.</param>
    /// <param name="target"> target vertex of the edge.</param>
    /// <returns>this builder object.</returns>
    /// <seealso cref="Graph.removeVertex(Object)"/>
    public virtual TB RemoveEdge(TNode source, TNode target)
    {
        Graph.removeEdge(source, target);
        return Self();
    }

    /// <summary>
    /// Removes the specified edge from the graph. Removes the specified edge from this graph if it
    /// is present.
    /// </summary>
    /// <param name="edge"> edge to be removed from this graph, if present.</param>
    /// <returns>this builder object.</returns>
    /// <seealso cref="Graph.removeEdge(Object)"/>
    public virtual TB RemoveEdge(TEdge edge)
    {
        Graph.removeEdge(edge);
        return Self();
    }

    /// <summary>
    /// Adds an weighted edge to the graph being built. The source and target vertices are added to
    /// the graph, if not already included.
    /// </summary>
    /// <param name="source"> source vertex of the edge.</param>
    /// <param name="target"> target vertex of the edge.</param>
    /// <param name="weight"> weight of the edge.</param>
    /// <returns>this builder object.</returns>
    /// <seealso cref="Graphs.addEdgeWithVertices(Graph, Object, Object, double)"/>
    public virtual TB AddEdge(TNode source, TNode target, double weight)
    {
        Graphs.AddEdgeWithVertices(Graph, source, target, weight);
        return Self();
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
    /// <seealso cref=".addEdge(Object, Object, Object)"/>
    /// <seealso cref="Graph.setEdgeWeight(Object, double)"/>
    public virtual TB AddEdge(TNode source, TNode target, TEdge edge, double weight)
    {
        AddEdge(source, target, edge); // adds vertices if needed
        Graph.setEdgeWeight(edge, weight);
        return Self();
    }

    /// <summary>
    /// Build the graph. Calling any method (including this method) on this builder object after
    /// calling this method is undefined behaviour.
    /// </summary>
    /// <returns>the built graph.</returns>
    public virtual TG Build()
    {
        return Graph;
    }

    /// <summary>
    /// Build an unmodifiable version graph. Calling any method (including this method) on this
    /// builder object after calling this method is undefined behaviour.
    /// </summary>
    /// <returns>the built unmodifiable graph.</returns>
    /// <seealso cref=".build()"/>
    public virtual IGraph<TNode, TEdge> BuildAsUnmodifiable()
    {
        return new AsUnmodifiableGraph<TNode, TEdge>(Graph);
    }
}
