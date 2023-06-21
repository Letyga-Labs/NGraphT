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
/// An event which indicates that a graph edge has changed, or is about to change. The event can be
/// used either as an indication <i>after</i> the edge has been added or removed, or <i>before</i> it
/// is added. The type of the event can be tested using the <see ref="GraphChangeEventArgs.Type"/> method.
/// </summary>
///
/// <typeparam name="TVertex">the graph vertex type.</typeparam>
/// <typeparam name="TEdge">the graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public class GraphEdgeChangeEventArgs<TVertex, TEdge> : GraphChangeEventArgs
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Before edge added event. This event is fired before an edge is added to a graph.
    /// </summary>
    public const int BeforeEdgeAdded = 21;

    /// <summary>
    /// Before edge removed event. This event is fired before an edge is removed from a graph.
    /// </summary>
    public const int BeforeEdgeRemoved = 22;

    /// <summary>
    /// Edge added event. This event is fired after an edge is added to a graph.
    /// </summary>
    public const int EdgeAdded = 23;

    /// <summary>
    /// Edge removed event. This event is fired after an edge is removed from a graph.
    /// </summary>
    public const int EdgeRemoved = 24;

    /// <summary>
    /// Edge weight updated event. This event is fired after an edge weight is updated in a graph.
    /// </summary>
    public const int EdgeWeightUpdated = 25;

    /// <summary>
    /// Constructor for GraphEdgeChangeEvent.
    /// </summary>
    /// <param name="eventSource"> the source of this event. </param>
    /// <param name="type"> the event type of this event. </param>
    /// <param name="edge"> the edge that this event is related to. </param>
    /// <param name="edgeSource"> edge source vertex.</param>
    /// <param name="edgeTarget"> edge target vertex.</param>
    public GraphEdgeChangeEventArgs(object eventSource, int type, TEdge edge, TVertex edgeSource, TVertex edgeTarget)
        : this(eventSource, type, edge, edgeSource, edgeTarget, IGraph<object, object>.DefaultEdgeWeight)
    {
    }

    /// <summary>
    /// Constructor for GraphEdgeChangeEvent.
    /// </summary>
    /// <param name="eventSource"> the source of this event. </param>
    /// <param name="type"> the event type of this event. </param>
    /// <param name="edge"> the edge that this event is related to. </param>
    /// <param name="edgeSource">Edge source vertex.</param>
    /// <param name="edgeTarget">Edge target vertex.</param>
    /// <param name="edgeWeight">Edge weight.</param>
    public GraphEdgeChangeEventArgs(
        object  eventSource,
        int     type,
        TEdge   edge,
        TVertex edgeSource,
        TVertex edgeTarget,
        double  edgeWeight)
        : base(eventSource, type)
    {
        Edge       = edge;
        EdgeSource = edgeSource;
        EdgeTarget = edgeTarget;
        EdgeWeight = edgeWeight;
    }

    /// <summary>
    /// The edge that this event is related to.
    /// </summary>
    public TEdge Edge { get; protected internal set; }

    /// <summary>
    /// The source vertex of the edge that this event is related to.
    /// </summary>
    public TVertex EdgeSource { get; protected internal set; }

    /// <summary>
    /// The target vertex of the edge that this event is related to.
    /// </summary>
    public TVertex EdgeTarget { get; protected internal set; }

    /// <summary>
    /// The weight of the edge that this event is related to.
    /// </summary>
    public double EdgeWeight { get; protected internal set; }
}
