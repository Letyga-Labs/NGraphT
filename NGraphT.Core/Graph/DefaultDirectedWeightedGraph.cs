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

namespace NGraphT.Core.Graph;

using Builder;
using Util;

/// <summary>
/// The default implementation of a directed weighted graph. A default directed weighted graph is a
/// non-simple directed graph in which multiple (parallel) edges between any two vertices are
/// <i>not</i> permitted, but loops are. The graph has weights on its edges.
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
/// </param>
/// <seealso cref="DefaultDirectedGraph"/>
public class DefaultDirectedWeightedGraph<TNode, TEdge> : DefaultDirectedGraph<TNode, TEdge>
{
    /// <summary>
    /// Creates a new graph.
    /// </summary>
    /// <param name="edgeClass"> class on which to base the edge supplier.</param>
    public DefaultDirectedWeightedGraph(Type edgeClass)
        : this(null, SupplierUtil.CreateSupplier(edgeClass))
    {
    }

    /// <summary>
    /// Creates a new graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier, can be null.</param>
    /// <param name="edgeSupplier"> the edge supplier, can be null.</param>
    public DefaultDirectedWeightedGraph(Func<TNode> vertexSupplier, Func<TEdge> edgeSupplier)
        : base(vertexSupplier, edgeSupplier, true)
    {
    }

    /// <summary>
    /// Create a builder for this kind of graph.
    /// </summary>
    /// <param name="edgeClass"> class on which to base factory for edges.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>a builder for this kind of graph.</returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: public static <TNode, TEdge> GraphBuilder<TNode, TEdge, ? extends DefaultDirectedWeightedGraph<TNode, TEdge>> createBuilder(Class edgeClass)
    public static GraphBuilder<TNode, TEdge, DefaultDirectedWeightedGraph<TNode, TEdge>> CreateBuilder<TNode, TEdge>(
        Type edgeClass
    )
    {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: return new GraphBuilder<>(new DefaultDirectedWeightedGraph<>(edgeClass));
        return new GraphBuilder<TNode, TEdge, DefaultDirectedWeightedGraph<TNode, TEdge>>(
            new DefaultDirectedWeightedGraph<TNode, TEdge, DefaultDirectedWeightedGraph<TNode, TEdge>>(edgeClass)
        );
    }

    /// <summary>
    /// Create a builder for this kind of graph.
    /// </summary>
    /// <param name="edgeSupplier"> the edge supplier.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>a builder for this kind of graph.</returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: public static <TNode, TEdge> GraphBuilder<TNode, TEdge, ? extends DefaultDirectedWeightedGraph<TNode, TEdge>> createBuilder(Supplier<TEdge> edgeSupplier)
    public static GraphBuilder<TNode, TEdge, DefaultDirectedWeightedGraph<TNode, TEdge>> CreateBuilder<TNode, TEdge>(
        Func<TEdge> edgeSupplier
    )
    {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: return new GraphBuilder<>(new DefaultDirectedWeightedGraph<>(null, edgeSupplier));
        return new GraphBuilder<TNode, TEdge, DefaultDirectedWeightedGraph<TNode, TEdge>>(
            new DefaultDirectedWeightedGraph<TNode, TEdge, DefaultDirectedWeightedGraph<TNode, TEdge>>(null,
                edgeSupplier
            )
        );
    }
}
