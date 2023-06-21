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

namespace NGraphT.Core.Graph;

/// <summary>
/// Implementation of a <a href="http://mathworld.wolfram.com/SimpleGraph.html">Simple Graph</a>. A
/// Simple Graph is an undirected graph containing no
/// <a href="http://mathworld.wolfram.com/GraphLoop.html">graph loops</a> or
/// <a href="http://mathworld.wolfram.com/MultipleEdge.html">multiple edges</a>. This particular
/// implementation supports both weighted and unweighted edges.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
public class SimpleGraph<TVertex, TEdge> : AbstractBaseGraph<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Creates a new simple graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier, can be null.</param>
    /// <param name="edgeSupplier"> the edge supplier, can be null.</param>
    /// <param name="weighted"> whether the graph is weighted or not.</param>
    public SimpleGraph(Func<TVertex>? vertexSupplier, Func<TEdge>? edgeSupplier, bool weighted)
        : base(
            vertexSupplier,
            edgeSupplier,
            BuildMyGraphType(weighted)
        )
    {
    }

    private static DefaultGraphType BuildMyGraphType(bool weighted)
    {
        return new DefaultGraphType.Builder()
            .Undirected()
            .AllowMultipleEdges(false)
            .AllowSelfLoops(false)
            .Weighted(weighted)
            .Build();
    }
}
