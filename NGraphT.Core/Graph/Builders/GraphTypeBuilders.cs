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

public static class GraphTypeBuilders
{
    /// <summary>
    /// Create a graph type builder for a directed graph.
    /// </summary>
    /// <returns>the graph type builder.</returns>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    public static GraphTypeBuilder<TVertex, TEdge> Directed<TVertex, TEdge>()
        where TVertex : class
        where TEdge : class
    {
        return new GraphTypeBuilder<TVertex, TEdge>(true, false);
    }

    /// <summary>
    /// Create a graph type builder for an undirected graph.
    /// </summary>
    /// <returns>the graph type builder.</returns>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    public static GraphTypeBuilder<TVertex, TEdge> Undirected<TVertex, TEdge>()
        where TVertex : class
        where TEdge : class
    {
        return new GraphTypeBuilder<TVertex, TEdge>(false, true);
    }

    /// <summary>
    /// Create a graph type builder for a mixed graph.
    /// </summary>
    /// <returns>the graph type builder.</returns>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    public static GraphTypeBuilder<TVertex, TEdge> Mixed<TVertex, TEdge>()
        where TVertex : class
        where TEdge : class
    {
        return new GraphTypeBuilder<TVertex, TEdge>(true, true);
    }

    /// <summary>
    /// Create a graph type builder which will create a graph with the same type as the one provided.
    /// </summary>
    /// <param name="type"> the graph type.</param>
    /// <returns>the graph type builder.</returns>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    public static GraphTypeBuilder<TVertex, TEdge> ForGraphType<TVertex, TEdge>(IGraphType type)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(type);

        var builder = new GraphTypeBuilder<TVertex, TEdge>(
            isDirected: type.IsDirected || type.IsMixed,
            isUndirected: type.IsUndirected || type.IsMixed
        );

        return builder
            .Weighted(type.IsWeighted)
            .AllowingSelfLoops(type.IsAllowingSelfLoops)
            .AllowingMultipleEdges(type.IsAllowingMultipleEdges);
    }

    /// <summary>
    /// Create a graph type builder which will create the same graph type as the parameter graph. The
    /// new graph will use the same vertex and edge suppliers as the input graph.
    /// </summary>
    /// <param name="graph"> a graph.</param>
    /// <returns>a type builder.</returns>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    public static GraphTypeBuilder<TVertex, TEdge> ForGraph<TVertex, TEdge>(IGraph<TVertex, TEdge> graph)
        where TVertex : class
        where TEdge : class
    {
        ArgumentNullException.ThrowIfNull(graph);

        return ForGraphType<TVertex, TEdge>(graph.Type)
            .VertexSupplier(graph.VertexSupplier)
            .EdgeSupplier(graph.EdgeSupplier);
    }
}
