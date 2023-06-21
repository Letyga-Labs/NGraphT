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

namespace NGraphT.Core.Graph;

/// <summary>
/// A graph specifics construction factory.
///
/// <para>
/// Such a strategy can be used to adjust the internals of the default graph implementations.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <seealso cref="FastLookupGraphSpecificsStrategy{TVertex,TEdge}"/>
/// <seealso cref="DefaultGraphSpecificsStrategy{TVertex,TEdge}"/>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public interface IGraphSpecificsStrategy<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Get a function which creates the intrusive edges specifics. The factory will accept the graph
    /// type as a parameter.
    ///
    /// <para>
    /// Note that it is very important to use a map implementation which respects iteration order.
    /// </para>
    /// </summary>
    /// <returns>a function which creates intrusive edges specifics.</returns>
    Func<IGraphType, IIntrusiveEdgesSpecifics<TVertex, TEdge>> IntrusiveEdgesSpecificsFactory { get; }

    /// <summary>
    /// Get a function which creates the specifics. The factory will accept the graph type as a
    /// parameter.
    /// </summary>
    /// <returns>a function which creates intrusive edges specifics.</returns>
    Func<IGraph<TVertex, TEdge>, IGraphType, ISpecifics<TVertex, TEdge>> SpecificsFactory { get; }

    /// <summary>
    /// Get an edge set factory.
    /// </summary>
    /// <returns>an edge set factory.</returns>
    IEdgeSetFactory<TVertex, TEdge> EdgeSetFactory => new ArrayUnenforcedSetEdgeSetFactory<TVertex, TEdge>();
}
