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

using NGraphT.Core.Events;

namespace NGraphT.Core;

/// <summary>
/// A graph that supports listeners on structural change events.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh/>.</remarks>
/// <seealso cref="IGraphListener{TVertex,TEdge}"/>
/// <seealso cref="IVertexSetListener{TVertex}"/>
public interface IListenableGraph<TVertex, TEdge> : IGraph<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Adds the specified graph listener to this graph, if not already present.
    /// </summary>
    /// <param name="l"> the listener to be added.</param>
    void AddGraphListener(IGraphListener<TVertex, TEdge> l);

    /// <summary>
    /// Adds the specified vertex set listener to this graph, if not already present.
    /// </summary>
    /// <param name="l"> the listener to be added.</param>
    void AddVertexSetListener(IVertexSetListener<TVertex> l);

    /// <summary>
    /// Removes the specified graph listener from this graph, if present.
    /// </summary>
    /// <param name="l"> the listener to be removed.</param>
    void RemoveGraphListener(IGraphListener<TVertex, TEdge> l);

    /// <summary>
    /// Removes the specified vertex set listener from this graph, if present.
    /// </summary>
    /// <param name="l"> the listener to be removed.</param>
    void RemoveVertexSetListener(IVertexSetListener<TVertex> l);
}
