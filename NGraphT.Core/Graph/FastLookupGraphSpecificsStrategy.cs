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
using NGraphT.Core.Util;
using Java2Net = J2N.Collections.Generic;

namespace NGraphT.Core.Graph;

/// <summary>
/// The fast lookup specifics strategy implementation.
///
/// <para>
/// Graphs constructed using this strategy use additional data structures to improve the performance
/// of methods which depend on edge retrievals, e.g.
/// <c>getEdge(TVertex u, TVertex v)</c>,
/// <c>containsEdge(TVertex u, TVertex v)</c>,
/// <c>addEdge(TVertex u, TVertex v)</c>.
/// A disadvantage is an increase in memory consumption. If memory utilization
/// is an issue, use the <see cref="DefaultGraphSpecificsStrategy{TVertex,TEdge}"/> instead.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public class FastLookupGraphSpecificsStrategy<TVertex, TEdge> : IGraphSpecificsStrategy<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    public virtual Func<IGraphType, IIntrusiveEdgesSpecifics<TVertex, TEdge>> IntrusiveEdgesSpecificsFactory => gType =>
    {
        if (gType.IsWeighted)
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

    public virtual Func<IGraph<TVertex, TEdge>, IGraphType, ISpecifics<TVertex, TEdge>> SpecificsFactory =>
        (graph, type) =>
        {
            if (type.IsDirected)
            {
                return new FastLookupDirectedSpecifics<TVertex, TEdge>(
                    graph,
                    new Java2Net.LinkedDictionary<TVertex, DirectedEdgeContainer<TVertex, TEdge>>(),
                    new Dictionary<(TVertex U, TVertex V), ISet<TEdge>>(),
                    ((IGraphSpecificsStrategy<TVertex, TEdge>)this).EdgeSetFactory
                );
            }
            else
            {
                return new
                    FastLookupUndirectedSpecifics<TVertex, TEdge>(
                        graph,
                        new Java2Net.LinkedDictionary<TVertex, UndirectedEdgeContainer<TVertex, TEdge>>(),
                        new Dictionary<UnorderedPair<TVertex, TVertex>, ISet<TEdge>>(),
                        ((IGraphSpecificsStrategy<TVertex, TEdge>)this).EdgeSetFactory
                    );
            }
        };
}
