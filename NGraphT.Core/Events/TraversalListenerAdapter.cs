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
/// An empty do-nothing implementation of the <seealso cref="ITraversalListener{TVertex,TEdge}"/> interface used for
/// subclasses.
/// </summary>
///
/// <typeparam name="TVertex">the graph vertex type.</typeparam>
/// <typeparam name="TEdge">the graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public class TraversalListenerAdapter<TVertex, TEdge> : ITraversalListener<TVertex, TEdge>
{
    /// <inheritdoc />
    public virtual void ConnectedComponentFinished(ConnectedComponentTraversalEventArgs e)
    {
        // empty
    }

    /// <inheritdoc />
    public virtual void ConnectedComponentStarted(ConnectedComponentTraversalEventArgs e)
    {
        // empty
    }

    /// <inheritdoc />
    public virtual void EdgeTraversed(EdgeTraversalEventArgs<TEdge> e)
    {
        // empty
    }

    /// <inheritdoc />
    public virtual void VertexTraversed(VertexTraversalEventArgs<TVertex> e)
    {
        // empty
    }

    /// <inheritdoc />
    public virtual void VertexFinished(VertexTraversalEventArgs<TVertex> e)
    {
        // empty
    }
}
