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
/// An event which indicates that a graph vertex has changed, or is about to change. The event can be
/// used either as an indication <i>after</i> the vertex has been added or removed, or <i>before</i>
/// it is added. The type of the event can be tested using the <see ref="GraphChangeEventArgs.Type"/> method.
/// </summary>
///
/// <typeparam name="TVertex">the graph vertex type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public class GraphVertexChangeEventArgs<TVertex> : GraphChangeEventArgs
    where TVertex : class
{
    /// <summary>
    /// Before vertex added event. This event is fired before a vertex is added to a graph.
    /// </summary>
    public const int BeforeVertexAdded = 11;

    /// <summary>
    /// Before vertex removed event. This event is fired before a vertex is removed from a graph.
    /// </summary>
    public const int BeforeVertexRemoved = 12;

    /// <summary>
    /// Vertex added event. This event is fired after a vertex is added to a graph.
    /// </summary>
    public const int VertexAdded = 13;

    /// <summary>
    /// Vertex removed event. This event is fired after a vertex is removed from a graph.
    /// </summary>
    public const int VertexRemoved = 14;

    /// <summary>
    /// Creates a new GraphVertexChangeEvent object.
    /// </summary>
    /// <param name="eventSource"> the source of the event. </param>
    /// <param name="type"> the type of the event. </param>
    /// <param name="vertex"> the vertex that the event is related to. </param>
    public GraphVertexChangeEventArgs(object eventSource, int type, TVertex vertex)
        : base(eventSource, type)
    {
        Vertex = vertex;
    }

    /// <summary>
    /// The vertex that this event is related to.
    /// </summary>
    public TVertex Vertex { get; protected internal set; }
}
