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
/// The default implementation of a directed weighted graph. A default directed weighted graph is a
/// non-simple directed graph in which multiple (parallel) edges between any two vertices are
/// <i>not</i> permitted, but loops are. The graph has weights on its edges.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <seealso cref="DefaultDirectedGraph{TVertex,TEdge}"/>
public sealed class DefaultDirectedWeightedGraph<TVertex, TEdge> : DefaultDirectedGraph<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Creates a new graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier, can be null.</param>
    /// <param name="edgeSupplier"> the edge supplier, can be null.</param>
    public DefaultDirectedWeightedGraph(Func<TVertex>? vertexSupplier, Func<TEdge>? edgeSupplier)
        : base(vertexSupplier, edgeSupplier, true)
    {
    }
}
