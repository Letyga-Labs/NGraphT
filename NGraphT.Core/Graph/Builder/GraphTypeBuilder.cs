/*
 * (C) Copyright 2018-2021, by Dimitrios Michail and Contributors.
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
using Util;

/// <summary>
/// A builder class for the hierarchy of <see cref="Graph"/>s that the library provides.
///
/// <para>
/// The following example creates a directed graph which allows multiple (parallel) edges and
/// self-loops: <blockquote>
/// 
/// <pre>
/// Graph&lt;Integer,
///     DefaultEdge&gt; g = GraphTypeBuilder
///         .&lt;Integer, DefaultEdge&gt; directed().allowingMultipleEdges(true).allowingSelfLoops(true)
///         .edgeClass(DefaultEdge.class).buildGraph();
/// </pre>
/// 
/// </blockquote>
/// 
/// Similarly one could get a weighted multigraph by using: <blockquote>
/// 
/// <pre>
/// Graph&lt;Integer, DefaultWeightedEdge&gt; g = GraphTypeBuilder
///     .&lt;Integer, DefaultWeightedEdge&gt; undirected().allowingMultipleEdges(true)
///     .allowingSelfLoops(false).edgeClass(DefaultWeightedEdge.class).weighted(true).buildGraph();
/// </pre>
/// 
/// </blockquote>
/// </para>
/// <para>
/// The builder also provides the ability to construct a graph from another graph such as:
/// <blockquote>
/// 
/// <pre>
/// Graph&lt;Integer, DefaultWeightedEdge&gt; g1 = GraphTypeBuilder
///     .&lt;Integer, DefaultWeightedEdge&gt; undirected().allowingMultipleEdges(true)
///     .allowingSelfLoops(false).edgeClass(DefaultWeightedEdge.class).weighted(true).buildGraph();
/// 
/// Graph&lt;Integer, DefaultWeightedEdge&gt; g2 = GraphTypeBuilder.asGraph(g1).buildGraph();
/// </pre>
/// 
/// </blockquote>
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
/// </param>
/// <seealso cref="IGraphType"/>
/// <seealso cref="GraphBuilder"/>
public sealed class GraphTypeBuilder<TNode, TEdge>
{
    private bool               _undirected;
    private bool               _directed;
    private bool               _weighted;
    private bool               _allowingMultipleEdges;
    private bool               _allowingSelfLoops;
    private Func<TNode> _vertexSupplier;
    private Func<TEdge> _edgeSupplier;

    private GraphTypeBuilder(bool directed, bool undirected)
    {
        _directed              = directed;
        _undirected            = undirected;
        _weighted              = false;
        _allowingMultipleEdges = false;
        _allowingSelfLoops     = false;
    }

    ///<summary>
    ///Create a graph type builder for a directed graph.
    ///</summary>
    ///<returns>the graph type builder.</returns>
    ///<typeparam name="TNode">The graph vertex type.</typeparam>
    ///<typeparam name="TEdge">The graph edge type.</typeparam> </param>
    public static GraphTypeBuilder<TNode, TEdge> Directed<TNode, TEdge>()
    {
        return new GraphTypeBuilder<TNode, TEdge>(true, false);
    }

    ///<summary>
    ///Create a graph type builder for an undirected graph.
    ///</summary>
    ///<returns>the graph type builder.</returns>
    ///<typeparam name="TNode">The graph vertex type.</typeparam>
    ///<typeparam name="TEdge">The graph edge type.</typeparam> </param>
    public static GraphTypeBuilder<TNode, TEdge> Undirected<TNode, TEdge>()
    {
        return new GraphTypeBuilder<TNode, TEdge>(false, true);
    }

    ///<summary>
    ///Create a graph type builder for a mixed graph.
    ///</summary>
    ///<returns>the graph type builder.</returns>
    ///<typeparam name="TNode">The graph vertex type.</typeparam>
    ///<typeparam name="TEdge">The graph edge type.</typeparam> </param>
    public static GraphTypeBuilder<TNode, TEdge> Mixed<TNode, TEdge>()
    {
        return new GraphTypeBuilder<TNode, TEdge>(true, true);
    }

    ///<summary>
    ///Create a graph type builder which will create a graph with the same type as the one provided.
    ///</summary>
    ///<param name="type"> the graph type.</param>
    ///<returns>the graph type builder.</returns>
    ///<typeparam name="TNode">The graph vertex type.</typeparam>
    ///<typeparam name="TEdge">The graph edge type.</typeparam> </param>
    public static GraphTypeBuilder<TNode, TEdge> ForGraphType<TNode, TEdge>(IGraphType type)
    {
        var builder =
            new GraphTypeBuilder<TNode, TEdge>(type.Directed || type.Mixed, type.Undirected || type.Mixed);
        builder._weighted              = type.Weighted;
        builder._allowingSelfLoops     = type.AllowingSelfLoops;
        builder._allowingMultipleEdges = type.AllowingMultipleEdges;
        return builder;
    }

    ///<summary>
    ///Create a graph type builder which will create the same graph type as the parameter graph. The
    ///new graph will use the same vertex and edge suppliers as the input graph.
    ///</summary>
    ///<param name="graph"> a graph.</param>
    ///<returns>a type builder.</returns>
    ///<typeparam name="TNode">The graph vertex type.</typeparam>
    ///<typeparam name="TEdge">The graph edge type.</typeparam> </param>
    public static GraphTypeBuilder<TNode, TEdge> ForGraph<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        GraphTypeBuilder<TNode, TEdge> builder = ForGraphType(graph.Type);
        builder._vertexSupplier = graph.VertexSupplier;
        builder._edgeSupplier   = graph.EdgeSupplier;
        return builder;
    }

    ///<summary>
    ///Set whether the graph will be weighted or not.
    ///</summary>
    ///<param name="weighted"> if true the graph will be weighted.</param>
    ///<returns>the graph type builder.</returns>
    public GraphTypeBuilder<TNode, TEdge> Weighted(bool weighted)
    {
        _weighted = weighted;
        return this;
    }

    ///<summary>
    ///Set whether the graph will allow self loops (edges with same source and target vertices).
    ///</summary>
    ///<param name="allowingSelfLoops"> if true the graph will allow self-loops.</param>
    ///<returns>the graph type builder.</returns>
    public GraphTypeBuilder<TNode, TEdge> AllowingSelfLoops(bool allowingSelfLoops)
    {
        _allowingSelfLoops = allowingSelfLoops;
        return this;
    }

    ///<summary>
    ///Set whether the graph will allow multiple (parallel) edges between the same two vertices.
    ///</summary>
    ///<param name="allowingMultipleEdges"> if true the graph will allow multiple (parallel) edges.</param>
    ///<returns>the graph type builder.</returns>
    public GraphTypeBuilder<TNode, TEdge> AllowingMultipleEdges(bool allowingMultipleEdges)
    {
        _allowingMultipleEdges = allowingMultipleEdges;
        return this;
    }

    ///<summary>
    ///Set the vertex supplier.
    ///</summary>
    ///<param name="vertexSupplier"> the vertex supplier to use.</param>
    ///<returns>the graph type builder.</returns>
    ///<typeparam name="V1"> the graph vertex type.</typeparam>
    public GraphTypeBuilder<TV1, TEdge> VertexSupplier<TV1>(Func<TV1> vertexSupplier) where TV1 : TNode
    {
        GraphTypeBuilder<TV1, TEdge> newBuilder = TypeUtil.UncheckedCast(this);
        newBuilder._vertexSupplier = vertexSupplier;
        return newBuilder;
    }

    ///<summary>
    ///Set the edge supplier.
    ///</summary>
    ///<param name="edgeSupplier"> the edge supplier to use.</param>
    ///<returns>the graph type builder.</returns>
    ///<typeparam name="E1"> the graph edge type.</typeparam>
    public GraphTypeBuilder<TNode, TE1> EdgeSupplier<TE1>(Func<TE1> edgeSupplier) where TE1 : TEdge
    {
        GraphTypeBuilder<TNode, TE1> newBuilder = TypeUtil.UncheckedCast(this);
        newBuilder._edgeSupplier = edgeSupplier;
        return newBuilder;
    }

    ///<summary>
    ///Set the vertex class.
    ///</summary>
    ///<param name="vertexClass"> the vertex class.</param>
    ///<returns>the graph type builder.</returns>
    ///<typeparam name="V1"> the graph vertex type.</typeparam>
    public GraphTypeBuilder<TV1, TEdge> VertexClass<TV1>(Type vertexClass) where TV1 : TNode
    {
        GraphTypeBuilder<TV1, TEdge> newBuilder = TypeUtil.UncheckedCast(this);
        newBuilder._vertexSupplier = SupplierUtil.CreateSupplier(vertexClass);
        return newBuilder;
    }

    /// <summary>
    /// Set the edge class.
    /// </summary>
    /// <param name="edgeClass"> the edge class.</param>
    /// <returns>the graph type builder.</returns>
    /// <typeparam name="E1"> the graph edge type.</typeparam>
    public GraphTypeBuilder<TNode, TE1> EdgeClass<TE1>(Type edgeClass) where TE1 : TEdge
    {
        GraphTypeBuilder<TNode, TE1> newBuilder = TypeUtil.UncheckedCast(this);
        newBuilder._edgeSupplier = SupplierUtil.CreateSupplier(edgeClass);
        return newBuilder;
    }

    /// <summary>
    /// Build the graph type.
    /// </summary>
    /// <returns>a graph type.</returns>
    public IGraphType BuildType()
    {
        var typeBuilder = new DefaultGraphType.Builder();
        if (_directed && _undirected)
        {
            typeBuilder = typeBuilder.Mixed();
        }
        else if (_directed)
        {
            typeBuilder = typeBuilder.Directed();
        }
        else if (_undirected)
        {
            typeBuilder = typeBuilder.Undirected();
        }

        return typeBuilder.AllowMultipleEdges(_allowingMultipleEdges).AllowSelfLoops(_allowingSelfLoops)
            .Weighted(_weighted).Build();
    }

    /// <summary>
    /// Build the graph and acquire a <see cref="GraphBuilder"/> in order to add vertices and edges.
    /// </summary>
    /// <returns>a graph builder.</returns>
    public GraphBuilder<TNode, TEdge, IGraph<TNode, TEdge>> BuildGraphBuilder()
    {
        return new GraphBuilder<TNode, TEdge, IGraph<TNode, TEdge>>(BuildGraph());
    }

    /// <summary>
    /// Build the actual graph.
    /// </summary>
    /// <returns>the graph.</returns>
    /// <exception cref="NotSupportedException"> in case a graph type is not supported.</exception>
    public IGraph<TNode, TEdge> BuildGraph()
    {
        if (_directed && _undirected)
        {
            throw new NotSupportedException("Mixed graphs are not supported");
        }
        else if (_directed)
        {
            if (_allowingSelfLoops && _allowingMultipleEdges)
            {
                if (_weighted)
                {
                    return new DirectedWeightedPseudograph<TNode, TEdge>(_vertexSupplier, _edgeSupplier);
                }
                else
                {
                    return new DirectedPseudograph<TNode, TEdge>(_vertexSupplier, _edgeSupplier, false);
                }
            }
            else if (_allowingMultipleEdges)
            {
                if (_weighted)
                {
                    return new DirectedWeightedMultigraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier);
                }
                else
                {
                    return new DirectedMultigraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier, false);
                }
            }
            else if (_allowingSelfLoops)
            {
                if (_weighted)
                {
                    return new DefaultDirectedWeightedGraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier);
                }
                else
                {
                    return new DefaultDirectedGraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier, false);
                }
            }
            else
            {
                if (_weighted)
                {
                    return new SimpleDirectedWeightedGraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier);
                }
                else
                {
                    return new SimpleDirectedGraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier, false);
                }
            }
        }
        else
        {
            if (_allowingSelfLoops && _allowingMultipleEdges)
            {
                if (_weighted)
                {
                    return new WeightedPseudograph<TNode, TEdge>(_vertexSupplier, _edgeSupplier);
                }
                else
                {
                    return new Pseudograph<TNode, TEdge>(_vertexSupplier, _edgeSupplier, false);
                }
            }
            else if (_allowingMultipleEdges)
            {
                if (_weighted)
                {
                    return new WeightedMultigraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier);
                }
                else
                {
                    return new Multigraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier, false);
                }
            }
            else if (_allowingSelfLoops)
            {
                if (_weighted)
                {
                    return new DefaultUndirectedWeightedGraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier);
                }
                else
                {
                    return new DefaultUndirectedGraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier, false);
                }
            }
            else
            {
                if (_weighted)
                {
                    return new SimpleWeightedGraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier);
                }
                else
                {
                    return new SimpleGraph<TNode, TEdge>(_vertexSupplier, _edgeSupplier, false);
                }
            }
        }
    }
}
