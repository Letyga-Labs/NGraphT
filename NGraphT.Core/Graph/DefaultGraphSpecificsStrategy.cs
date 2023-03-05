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

namespace NGraphT.Core.Graph;

using Core;
using Specifics;

/// <summary>
/// A default lookup specifics strategy implementation.
///
/// <para>
/// Graphs constructed using this strategy require the least amount of memory, at the expense of slow
/// edge retrievals. Methods which depend on edge retrievals, TEdge.g. getEdge(TNode u, TNode TNode), containsEdge(TNode
/// u, TNode TNode), addEdge(TNode u, TNode TNode), etc may be relatively slow when the average degree of a vertex is
/// high (dense graphs). For a fast implementation, use <see cref="FastLookupGraphSpecificsStrategy"/>.
///
/// <remarks>Author: Dimitrios Michail.</remarks>
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
public class DefaultGraphSpecificsStrategy<TNode, TEdge> : IGraphSpecificsStrategy<TNode, TEdge>
{
    public virtual Func<IGraphType, INtrusiveEdgesSpecifics<TNode, TEdge>> IntrusiveEdgesSpecificsFactory
    {
        get
        {
            return (Func<IGraphType, INtrusiveEdgesSpecifics<TNode, TEdge>> & Serializable)(type) =>
            {
                if (type.isWeighted())
                {
                    return new WeightedIntrusiveEdgesSpecifics<TNode, TEdge>(
                        new LinkedHashMap<TEdge, IntrusiveWeightedEdge>()
                    );
                }
                else
                {
                    return new UniformIntrusiveEdgesSpecifics<IGraphType, INtrusiveEdgesSpecifics<TNode, TEdge>>(
                        new LinkedHashMap<IGraphType, INtrusiveEdgesSpecifics<TNode, TEdge>>()
                    );
                }
            }
            ;
        }
    }

    public virtual Func<IGraph<TNode, TEdge>, IGraphType, ISpecifics<TNode, TEdge>> SpecificsFactory
    {
        get
        {
            return (Func<IGraph<TNode, TEdge>, IGraphType, ISpecifics<TNode, TEdge>> &
                    Serializable)(graph, type) =>
            {
                if (type.isDirected())
                {
                    return new DirectedSpecifics<TNode, TEdge>(graph,
                            new LinkedHashMap<TNode, DirectedEdgeContainer<TNode, TEdge>>(),
                            IEdgeSetFactory <,  >
                        );
                }
                else
                {
                    return new UndirectedSpecifics<IGraph<TNode, TEdge>, IGraphType, ISpecifics<TNode, TEdge>>(graph,
                            new LinkedHashMap<IGraph<TNode, TEdge>, IGraphType, ISpecifics<TNode, TEdge>>(),
                            IEdgeSetFactory <,  >
                        );
                }
            }
            ;
        }
    }
}
