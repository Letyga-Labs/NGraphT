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

/// <summary>
/// A builder class for <see cref="Graph"/>. This is a helper class which helps adding vertices and edges
/// into an already constructed graph instance.
///
/// <para>
/// Each graph implementation contains a static helper method for the construction of such a builder.
/// For example class <see cref="DefaultDirectedGraph"/> contains method
/// <see cref="DefaultDirectedGraph.createBuilder(Supplier)"/>.
/// </para>
/// <para>
/// See <see cref="GraphTypeBuilder"/> for a builder of the actual graph instance.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
/// @param <G> type of the resulting graph
///
/// <remarks>Author: Andrew Chen.</remarks>
/// <seealso cref="GraphTypeBuilder"/>
public class GraphBuilder<TNode, TEdge, TG> : AbstractGraphBuilder<TNode, TEdge, TG, GraphBuilder<TNode, TEdge, TG>>
    where TG : IGraph<TNode, TEdge>
{
    ///<summary>
    ///Creates a builder based on {@code baseGraph}. {@code baseGraph} must be mutable.
    ///
    ///<para>
    ///The recommended way to use this constructor is: {@code new
    ///GraphBuilderBase<...>(new YourGraph<...>(...))}.
    ///</para>
    ///
    ///<para>
    ///NOTE: {@code baseGraph} should not be an existing graph. If you want to add an existing graph
    ///to the graph being built, you should use the <see cref="addVertex(Object)"/> method.
    ///</para>
    ///
    ///</summary>
    ///<param name="baseGraph"> the graph object to base building on.</param>
    public GraphBuilder(TG baseGraph)
        : base(baseGraph)
    {
    }

    protected internal override GraphBuilder<TNode, TEdge, TG> Self()
    {
        return this;
    }
}
