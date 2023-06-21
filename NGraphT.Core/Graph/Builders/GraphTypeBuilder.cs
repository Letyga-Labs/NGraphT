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

namespace NGraphT.Core.Graph.Builders;

/// <summary>
/// A builder class for the hierarchy of <see cref="Graph"/>s that the library provides.
///
/// <para>
/// <example>
/// The following example creates a directed graph which allows multiple (parallel) edges and
/// self-loops:
/// <code>
/// Graph&lt;Integer,
///     DefaultEdge&gt; g = GraphTypeBuilder
///         .&lt;Integer, DefaultEdge&gt; directed().allowingMultipleEdges(true).allowingSelfLoops(true)
///         .edgeClass(DefaultEdge.class).buildGraph();
///
/// </code>
/// </example>
///
/// <example>
/// Similarly one could get a weighted multigraph by using:
/// <code>
/// Graph&lt;Integer, DefaultWeightedEdge&gt; g = GraphTypeBuilder
///     .&lt;Integer, DefaultWeightedEdge&gt; undirected().allowingMultipleEdges(true)
///     .allowingSelfLoops(false).edgeClass(DefaultWeightedEdge.class).weighted(true).buildGraph();
///
/// </code>
/// </example>
///
/// <example>
/// The builder also provides the ability to construct a graph from another graph such as:
/// <code>
/// Graph&lt;Integer, DefaultWeightedEdge&gt; g1 = GraphTypeBuilder
///     .&lt;Integer, DefaultWeightedEdge&gt; undirected().allowingMultipleEdges(true)
///     .allowingSelfLoops(false).edgeClass(DefaultWeightedEdge.class).weighted(true).buildGraph();
///
/// Graph&lt;Integer, DefaultWeightedEdge&gt; g2 = GraphTypeBuilder.asGraph(g1).buildGraph();
/// </code>
/// </example>
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <seealso cref="IGraphType"/>
/// <seealso cref="GraphBuilder{TVertex,TEdge,TGraph}"/>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public sealed class GraphTypeBuilder<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private bool _isUndirected;
    private bool _isDirected;

    private bool _isWeighted;
    private bool _allowingMultipleEdges;
    private bool _allowingSelfLoops;

    private Func<TVertex>? _vertexSupplier;
    private Func<TEdge>?   _edgeSupplier;

    internal GraphTypeBuilder(bool isDirected, bool isUndirected)
    {
        _isDirected   = isDirected;
        _isUndirected = isUndirected;
    }

    private GraphTypeBuilder()
    {
    }

    /// <summary>
    /// Set whether the graph will be weighted or not.
    /// </summary>
    /// <param name="weighted"> if true the graph will be weighted.</param>
    /// <returns>the graph type builder.</returns>
    public GraphTypeBuilder<TVertex, TEdge> Weighted(bool weighted)
    {
        _isWeighted = weighted;
        return this;
    }

    /// <summary>
    /// Set whether the graph will allow self loops (edges with same source and target vertices).
    /// </summary>
    /// <param name="allowingSelfLoops"> if true the graph will allow self-loops.</param>
    /// <returns>the graph type builder.</returns>
    public GraphTypeBuilder<TVertex, TEdge> AllowingSelfLoops(bool allowingSelfLoops)
    {
        _allowingSelfLoops = allowingSelfLoops;
        return this;
    }

    /// <summary>
    /// Set whether the graph will allow multiple (parallel) edges between the same two vertices.
    /// </summary>
    /// <param name="allowingMultipleEdges"> if true the graph will allow multiple (parallel) edges.</param>
    /// <returns>the graph type builder.</returns>
    public GraphTypeBuilder<TVertex, TEdge> AllowingMultipleEdges(bool allowingMultipleEdges)
    {
        _allowingMultipleEdges = allowingMultipleEdges;
        return this;
    }

    /// <summary>
    /// Set the vertex supplier.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier to use.</param>
    /// <returns>the graph type builder.</returns>
    /// <typeparam name="TNewVertex"> the graph vertex type.</typeparam>
    public GraphTypeBuilder<TNewVertex, TEdge> VertexSupplier<TNewVertex>(Func<TNewVertex>? vertexSupplier)
        where TNewVertex : class, TVertex
    {
        var newBuilder = new GraphTypeBuilder<TNewVertex, TEdge>();
        newBuilder.CopyFrom(this);
        newBuilder._vertexSupplier = vertexSupplier;
        return newBuilder;
    }

    /// <summary>
    /// Set the edge supplier.
    /// </summary>
    /// <param name="edgeSupplier"> the edge supplier to use.</param>
    /// <returns>the graph type builder.</returns>
    /// <typeparam name="TNewEdge"> the graph edge type.</typeparam>
    public GraphTypeBuilder<TVertex, TNewEdge> EdgeSupplier<TNewEdge>(Func<TNewEdge>? edgeSupplier)
        where TNewEdge : class, TEdge
    {
        var newBuilder = new GraphTypeBuilder<TVertex, TNewEdge>();
        newBuilder.CopyFrom(this);
        newBuilder._edgeSupplier = edgeSupplier;
        return newBuilder;
    }

    /// <summary>
    /// Set the vertex class.
    /// </summary>
    /// <returns>the graph type builder.</returns>
    /// <typeparam name="TNewVertex"> the graph vertex type.</typeparam>
    public GraphTypeBuilder<TNewVertex, TEdge> VertexClass<TNewVertex>()
        where TNewVertex : class, TVertex
    {
        var newBuilder = new GraphTypeBuilder<TNewVertex, TEdge>();
        newBuilder.CopyFrom(this);
        newBuilder._vertexSupplier = SupplierUtil.CreateSupplierByReflection<TNewVertex>();
        return newBuilder;
    }

    /// <summary>
    /// Set the edge class.
    /// </summary>
    /// <returns>the graph type builder.</returns>
    /// <typeparam name="TNewEdge"> the graph edge type.</typeparam>
    public GraphTypeBuilder<TVertex, TNewEdge> EdgeClass<TNewEdge>()
        where TNewEdge : class, TEdge
    {
        var newBuilder = new GraphTypeBuilder<TVertex, TNewEdge>();
        newBuilder.CopyFrom(this);
        newBuilder._edgeSupplier = SupplierUtil.CreateSupplierByReflection<TNewEdge>();
        return newBuilder;
    }

    /// <summary>
    /// Build the graph type.
    /// </summary>
    /// <returns>a graph type.</returns>
    public IGraphType BuildType()
    {
        var typeBuilder = new DefaultGraphType.Builder();
        if (_isDirected && _isUndirected)
        {
            typeBuilder = typeBuilder.Mixed();
        }
        else if (_isDirected)
        {
            typeBuilder = typeBuilder.Directed();
        }
        else if (_isUndirected)
        {
            typeBuilder = typeBuilder.Undirected();
        }

        return typeBuilder
            .AllowMultipleEdges(_allowingMultipleEdges)
            .AllowSelfLoops(_allowingSelfLoops)
            .Weighted(_isWeighted)
            .Build();
    }

    /// <summary>
    /// Build the graph and acquire a <see cref="GraphBuilder{TVertex,TEdge,TGraph}"/>
    /// in order to add vertices and edges.
    /// </summary>
    /// <returns>a graph builder.</returns>
    public GraphBuilder<TVertex, TEdge, IGraph<TVertex, TEdge>> BuildGraphBuilder()
    {
        return new GraphBuilder<TVertex, TEdge, IGraph<TVertex, TEdge>>(BuildGraph());
    }

    /// <summary>
    /// Build the actual graph.
    /// </summary>
    /// <returns>the graph.</returns>
    /// <exception cref="NotSupportedException"> in case a graph type is not supported.</exception>
    public IGraph<TVertex, TEdge> BuildGraph()
    {
        if (_isDirected && _isUndirected)
        {
            throw new NotSupportedException("Mixed graphs are not supported");
        }

        if (_isDirected)
        {
            if (_allowingSelfLoops && _allowingMultipleEdges)
            {
                return _isWeighted
                    ? new DirectedWeightedPseudograph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier)
                    : new DirectedPseudograph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier, weighted: false);
            }

            if (_allowingMultipleEdges)
            {
                return _isWeighted
                    ? new DirectedWeightedMultigraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier)
                    : new DirectedMultigraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier, weighted: false);
            }

            if (_allowingSelfLoops)
            {
                return _isWeighted
                    ? new DefaultDirectedWeightedGraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier)
                    : new DefaultDirectedGraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier, weighted: false);
            }

            return _isWeighted
                ? new SimpleDirectedWeightedGraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier)
                : new SimpleDirectedGraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier, weighted: false);
        }

        if (_allowingSelfLoops && _allowingMultipleEdges)
        {
            return _isWeighted
                ? new WeightedPseudograph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier)
                : new Pseudograph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier, weighted: false);
        }

        if (_allowingMultipleEdges)
        {
            return _isWeighted
                ? new WeightedMultigraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier)
                : new Multigraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier, weighted: false);
        }

        if (_allowingSelfLoops)
        {
            return _isWeighted
                ? new DefaultUndirectedWeightedGraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier)
                : new DefaultUndirectedGraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier, weighted: false);
        }

        return _isWeighted
            ? new SimpleWeightedGraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier)
            : new SimpleGraph<TVertex, TEdge>(_vertexSupplier, _edgeSupplier, weighted: false);
    }

    private void CopyFrom<TV, TE>(GraphTypeBuilder<TV, TE> source)
        where TV : class
        where TE : class
    {
        _isDirected            = source._isDirected;
        _isUndirected          = source._isUndirected;
        _isWeighted            = source._isWeighted;
        _allowingMultipleEdges = source._allowingMultipleEdges;
        _allowingSelfLoops     = source._allowingSelfLoops;
    }
}
