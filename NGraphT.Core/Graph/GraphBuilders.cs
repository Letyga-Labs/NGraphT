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

using NGraphT.Core.Graph.Builders;

namespace NGraphT.Core.Graph;

public static class GraphBuilders
{
    /// <summary>
    /// Create a builder for default directed graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier of the new graph.</param>
    /// <param name="edgeSupplier"> the edge supplier of the new graph.</param>
    /// <param name="weighted"> whether graph is weighted.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a builder for this kind of graph.</returns>
    public static GraphBuilder<TVertex, TEdge, DefaultDirectedGraph<TVertex, TEdge>> DefaultDirected<TVertex, TEdge>(
        Func<TVertex>? vertexSupplier = null,
        Func<TEdge>?   edgeSupplier   = null,
        bool           weighted       = false
    )
        where TVertex : class
        where TEdge : class
    {
        return new GraphBuilder<TVertex, TEdge, DefaultDirectedGraph<TVertex, TEdge>>(
            new DefaultDirectedGraph<TVertex, TEdge>(vertexSupplier, edgeSupplier, weighted)
        );
    }

    /// <summary>
    /// Create a builder for simple directed graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier of the new graph.</param>
    /// <param name="edgeSupplier"> the edge supplier of the new graph.</param>
    /// <param name="weighted"> whether graph is weighted.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a builder for this kind of graph.</returns>
    public static GraphBuilder<TVertex, TEdge, SimpleDirectedGraph<TVertex, TEdge>> SimpleDirectedGraph<TVertex, TEdge>(
        Func<TVertex>? vertexSupplier = null,
        Func<TEdge>?   edgeSupplier   = null,
        bool           weighted       = false
    )
        where TVertex : class
        where TEdge : class
    {
        return new GraphBuilder<TVertex, TEdge, SimpleDirectedGraph<TVertex, TEdge>>(
            new SimpleDirectedGraph<TVertex, TEdge>(vertexSupplier, edgeSupplier, weighted)
        );
    }

    /// <summary>
    /// Create a builder for this kind of graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier of the new graph.</param>
    /// <param name="edgeSupplier"> the edge supplier of the new graph.</param>
    /// <param name="weighted"> whether graph is weighted.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a builder for this kind of graph.</returns>
    public static GraphBuilder<TVertex, TEdge, DirectedAcyclicGraph<TVertex, TEdge>> DirectedAcyclic<TVertex, TEdge>(
        Func<TVertex>? vertexSupplier = null,
        Func<TEdge>?   edgeSupplier   = null,
        bool           weighted       = false
    )
        where TVertex : class
        where TEdge : class
    {
        return new GraphBuilder<TVertex, TEdge, DirectedAcyclicGraph<TVertex, TEdge>>(
            new DirectedAcyclicGraph<TVertex, TEdge>(vertexSupplier, edgeSupplier, false)
        );
    }

    /// <summary>
    /// Create a builder for directed multigraph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier of the new graph.</param>
    /// <param name="edgeSupplier"> the edge supplier of the new graph.</param>
    /// <param name="weighted"> whether graph is weighted.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a builder for this kind of graph.</returns>
    public static GraphBuilder<TVertex, TEdge, DirectedMultigraph<TVertex, TEdge>> DirectedMultigraph<TVertex, TEdge>(
        Func<TVertex>? vertexSupplier = null,
        Func<TEdge>?   edgeSupplier   = null,
        bool           weighted       = false
    )
        where TVertex : class
        where TEdge : class
    {
        return new GraphBuilder<TVertex, TEdge, DirectedMultigraph<TVertex, TEdge>>(
            new DirectedMultigraph<TVertex, TEdge>(
                vertexSupplier,
                edgeSupplier,
                weighted
            )
        );
    }

    /// <summary>
    /// Create a builder for directed pseudograph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier of the new graph.</param>
    /// <param name="edgeSupplier"> the edge supplier of the new graph.</param>
    /// <param name="weighted"> whether graph is weighted.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a builder for this kind of graph.</returns>
    public static GraphBuilder<TVertex, TEdge, DirectedPseudograph<TVertex, TEdge>> DirectedPseudograph<TVertex, TEdge>(
        Func<TVertex>? vertexSupplier = null,
        Func<TEdge>?   edgeSupplier   = null,
        bool           weighted       = false
    )
        where TVertex : class
        where TEdge : class
    {
        return new GraphBuilder<TVertex, TEdge, DirectedPseudograph<TVertex, TEdge>>(
            new DirectedPseudograph<TVertex, TEdge>(vertexSupplier, edgeSupplier, weighted)
        );
    }

    /// <summary>
    /// Create a builder for default undirected graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier of the new graph.</param>
    /// <param name="edgeSupplier"> the edge supplier of the new graph.</param>
    /// <param name="weighted"> whether graph is weighted.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a builder for this kind of graph.</returns>
    public static GraphBuilder<TVertex, TEdge, DefaultUndirectedGraph<TVertex, TEdge>>
        DefaultUndirected<TVertex, TEdge>(
            Func<TVertex>? vertexSupplier = null,
            Func<TEdge>?   edgeSupplier   = null,
            bool           weighted       = false
        )
        where TVertex : class
        where TEdge : class
    {
        return new GraphBuilder<TVertex, TEdge, DefaultUndirectedGraph<TVertex, TEdge>>(
            new DefaultUndirectedGraph<TVertex, TEdge>(vertexSupplier, edgeSupplier, weighted)
        );
    }

    /// <summary>
    /// Create a builder for multigraph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier of the new graph.</param>
    /// <param name="edgeSupplier"> the edge supplier of the new graph.</param>
    /// <param name="weighted"> whether graph is weighted.</param>
    /// <returns>a builder for this kind of graph.</returns>
    public static GraphBuilder<TVertex, TEdge, Multigraph<TVertex, TEdge>> Multigraph<TVertex, TEdge>(
        Func<TVertex>? vertexSupplier = null,
        Func<TEdge>?   edgeSupplier   = null,
        bool           weighted       = false
    )
        where TVertex : class
        where TEdge : class
    {
        return new GraphBuilder<TVertex, TEdge, Multigraph<TVertex, TEdge>>(
            new Multigraph<TVertex, TEdge>(vertexSupplier, edgeSupplier, weighted)
        );
    }

    /// <summary>
    /// Create a builder for pseudograph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier of the new graph.</param>
    /// <param name="edgeSupplier"> the edge supplier of the new graph.</param>
    /// <param name="weighted"> whether graph is weighted.</param>
    /// <returns>a builder for this kind of graph.</returns>
    public static GraphBuilder<TVertex, TEdge, Pseudograph<TVertex, TEdge>> Pseudograph<TVertex, TEdge>(
        Func<TVertex>? vertexSupplier = null,
        Func<TEdge>?   edgeSupplier   = null,
        bool           weighted       = false
    )
        where TVertex : class
        where TEdge : class
    {
        return new GraphBuilder<TVertex, TEdge, Pseudograph<TVertex, TEdge>>(
            new Pseudograph<TVertex, TEdge>(vertexSupplier, edgeSupplier, weighted)
        );
    }

    /// <summary>
    /// Create a builder for this kind of graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier of the new graph.</param>
    /// <param name="edgeSupplier"> the edge supplier of the new graph.</param>
    /// <param name="weighted"> whether graph is weighted.</param>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// <returns>a builder for this kind of graph.</returns>
    public static GraphBuilder<TVertex, TEdge, SimpleGraph<TVertex, TEdge>> SimpleGraph<TVertex, TEdge>(
        Func<TVertex>? vertexSupplier = null,
        Func<TEdge>?   edgeSupplier   = null,
        bool           weighted       = false
    )
        where TVertex : class
        where TEdge : class
    {
        return new GraphBuilder<TVertex, TEdge, SimpleGraph<TVertex, TEdge>>(
            new SimpleGraph<TVertex, TEdge>(vertexSupplier, edgeSupplier, weighted)
        );
    }
}
