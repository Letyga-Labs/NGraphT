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
/// A builder class for <see cref="Graph"/>. This is a helper class which helps adding vertices and edges
/// into an already constructed graph instance.
///
/// <para>
/// Each graph implementation contains a static helper method in <see cref="GraphBuilders"/>
/// for the construction of such a builder.
/// </para>
/// <para>
/// See <see cref="GraphTypeBuilder{TVertex,TEdge}"/> for a builder of the actual graph instance.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
/// <typeparam name="TGraph">type of the resulting graph.</typeparam>
///
/// <remarks>Author: Andrew Chen.</remarks>
public sealed class GraphBuilder<TVertex, TEdge, TGraph> :
    AbstractGraphBuilder<TVertex, TEdge, TGraph, GraphBuilder<TVertex, TEdge, TGraph>>
    where TGraph : IGraph<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Creates a builder based on <c>baseGraph</c>. <c>baseGraph</c> must be mutable.
    /// <para>
    /// The recommended way to use this constructor is:
    /// <c>new GraphBuilderBase&lt;...&gt;(new YourGraph&lt;...&gt;(...))</c>.
    /// </para>
    /// <para>
    /// NOTE: <c>baseGraph</c> should not be an existing graph. If you want to add an existing graph
    /// to the graph being built, you should use the
    /// <see cref="AbstractGraphBuilder{TVertex,TEdge,TGraph,TBuilder}.AddGraph{TSourceVertex,TSourceEdge}"/> method.
    /// </para>
    /// </summary>
    /// <param name="baseGraph"> the graph object to base building on.</param>
    public GraphBuilder(TGraph baseGraph)
        : base(baseGraph)
    {
    }

    protected override GraphBuilder<TVertex, TEdge, TGraph> Self => this;
}
