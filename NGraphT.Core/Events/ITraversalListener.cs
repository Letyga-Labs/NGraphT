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

namespace NGraphT.Core.Events;

/// <summary>
/// A listener on graph iterator or on a graph traverser.
/// </summary>
///
/// <typeparam name="TVertex">the graph vertex type.</typeparam>
/// <typeparam name="TEdge">the graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public interface ITraversalListener<TVertex, TEdge>
{
    /// <summary>
    /// Called to inform listeners that the traversal of the current connected component has
    /// finished.
    /// </summary>
    /// <param name="e"> the traversal event. </param>
    void ConnectedComponentFinished(ConnectedComponentTraversalEventArgs e);

    /// <summary>
    /// Called to inform listeners that a traversal of a new connected component has started.
    /// </summary>
    /// <param name="e"> the traversal event. </param>
    void ConnectedComponentStarted(ConnectedComponentTraversalEventArgs e);

    /// <summary>
    /// Called to inform the listener that the specified edge have been visited during the graph
    /// traversal. Depending on the traversal algorithm, edge might be visited more than once.
    /// </summary>
    /// <param name="e"> the edge traversal event. </param>
    void EdgeTraversed(EdgeTraversalEventArgs<TEdge> e);

    /// <summary>
    /// Called to inform the listener that the specified vertex have been visited during the graph
    /// traversal. Depending on the traversal algorithm, vertex might be visited more than once.
    /// </summary>
    /// <param name="e"> the vertex traversal event. </param>
    void VertexTraversed(VertexTraversalEventArgs<TVertex> e);

    /// <summary>
    /// Called to inform the listener that the specified vertex have been finished during the graph
    /// traversal. Exact meaning of "finish" is algorithm-dependent; e.g. for DFS, it means that all
    /// vertices reachable via the vertex have been visited as well.
    /// </summary>
    /// <param name="e"> the vertex traversal event. </param>
    void VertexFinished(VertexTraversalEventArgs<TVertex> e);
}
