/*
 * (C) Copyright 2020-2021, by Dimitrios Michail and Contributors.
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

/// <summary>
/// The default implementation of the graph iterables which simply delegates to the set
/// implementations.
/// </summary>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
public class DefaultGraphIterables<TNode, TEdge> : IGraphIterables<TNode, TEdge>
{
    ///<summary>
    ///The underlying graph.
    ///</summary>
    protected internal IGraph<TNode, TEdge> Graph;

    ///<summary>
    ///Create new graph iterables.
    ///</summary>
    public DefaultGraphIterables()
        : this(null)
    {
    }

    ///<summary>
    ///Create new graph iterables.
    ///</summary>
    ///<param name="graph"> the underlying graph.</param>
    public DefaultGraphIterables(IGraph<TNode, TEdge> graph)
    {
        this.graph = Objects.requireNonNull(graph);
    }

    public virtual IGraph<TNode, TEdge> Graph
    {
        get
        {
            return graph;
        }
    }
}
