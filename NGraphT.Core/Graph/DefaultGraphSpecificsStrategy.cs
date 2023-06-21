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

using NGraphT.Core.Graph.Specifics;
using Java2Net = J2N.Collections.Generic;

namespace NGraphT.Core.Graph;

/// <summary>
/// A default lookup specifics strategy implementation.
///
/// <para>
/// Graphs constructed using this strategy require the least amount of memory, at the expense of slow
/// edge retrievals. Methods which depend on edge retrievals, e.g.
/// <c>getEdge(TVertex u, TVertex v)</c>,
/// <c>containsEdge(TVertex u, TVertex v)</c>,
/// <c>addEdge(TVertex u, TVertex v)</c>,
/// etc may be relatively slow when the average degree of a vertex is high (dense graphs).
/// For a fast implementation, use <see cref="FastLookupGraphSpecificsStrategy{TVertex,TEdge}"/>.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public sealed class DefaultGraphSpecificsStrategy<TVertex, TEdge> : IGraphSpecificsStrategy<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    public Func<IGraphType, IIntrusiveEdgesSpecifics<TVertex, TEdge>> IntrusiveEdgesSpecificsFactory => graphType =>
    {
        if (graphType.IsWeighted)
        {
            return new WeightedIntrusiveEdgesSpecifics<TVertex, TEdge>(
                new Java2Net.LinkedDictionary<TEdge, IntrusiveWeightedEdge>()
            );
        }
        else
        {
            return new UniformIntrusiveEdgesSpecifics<TVertex, TEdge>(
                new Java2Net.LinkedDictionary<TEdge, IntrusiveEdge>()
            );
        }
    };

    public Func<IGraph<TVertex, TEdge>, IGraphType, ISpecifics<TVertex, TEdge>> SpecificsFactory => (graph, gType) =>
    {
        if (gType.IsDirected)
        {
            return new DirectedSpecifics<TVertex, TEdge>(
                graph,
                new Java2Net.LinkedDictionary<TVertex, DirectedEdgeContainer<TVertex, TEdge>>(),
                ((IGraphSpecificsStrategy<TVertex, TEdge>)this).EdgeSetFactory
            );
        }
        else
        {
            return new UndirectedSpecifics<TVertex, TEdge>(
                graph,
                new Java2Net.LinkedDictionary<TVertex, UndirectedEdgeContainer<TVertex, TEdge>>(),
                ((IGraphSpecificsStrategy<TVertex, TEdge>)this).EdgeSetFactory
            );
        }
    };
}
