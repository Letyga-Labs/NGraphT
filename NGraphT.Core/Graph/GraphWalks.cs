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

public static class GraphWalks
{
    /// <summary>
    /// Convenience method which creates an empty walk.
    /// </summary>
    /// <param name="graph"> input Graph.</param>
    /// <typeparam name="TVertex"> vertex type.</typeparam>
    /// <typeparam name="TEdge"> edge type.</typeparam>
    /// <returns>an empty walk.</returns>
    public static GraphWalk<TVertex, TEdge> EmptyWalk<TVertex, TEdge>(IGraph<TVertex, TEdge> graph)
        where TVertex : class
        where TEdge : class
    {
        return new GraphWalk<TVertex, TEdge>(
            graph,
            null,
            null,
            Array.Empty<TVertex>(),
            Array.Empty<TEdge>(),
            0.0
        );
    }

    /// <summary>
    /// Convenience method which creates a walk consisting of a single vertex with weight 0.0.
    /// </summary>
    /// <param name="graph"> input Graph.</param>
    /// <param name="vertex"> single vertex.</param>
    /// <typeparam name="TVertex"> vertex type.</typeparam>
    /// <typeparam name="TEdge"> edge type.</typeparam>
    /// <returns>an empty walk.</returns>
    public static GraphWalk<TVertex, TEdge> SingletonWalk<TVertex, TEdge>(IGraph<TVertex, TEdge> graph, TVertex vertex)
        where TVertex : class
        where TEdge : class
    {
        return SingletonWalk(graph, vertex, 0d);
    }

    /// <summary>
    /// Convenience method which creates a walk consisting of a single vertex.
    /// </summary>
    /// <param name="graph"> input Graph.</param>
    /// <param name="vertex"> single vertex.</param>
    /// <param name="weight"> weight of the path.</param>
    /// <typeparam name="TVertex"> vertex type.</typeparam>
    /// <typeparam name="TEdge"> edge type.</typeparam>
    /// <returns>an empty walk.</returns>
    public static GraphWalk<TVertex, TEdge> SingletonWalk<TVertex, TEdge>(
        IGraph<TVertex, TEdge> graph,
        TVertex                vertex,
        double                 weight
    )
        where TVertex : class
        where TEdge : class
    {
        return new GraphWalk<TVertex, TEdge>(
            graph,
            vertex,
            vertex,
            new List<TVertex> { vertex },
            Array.Empty<TEdge>(),
            weight
        );
    }
}
