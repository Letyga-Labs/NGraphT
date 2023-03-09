/*
 * (C) Copyright 2003-2021, by Barak Naveh and Contributors.
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
namespace NGraphT.Core.Event;

/// <summary>
/// A traversal event for a graph edge.
/// </summary>
/// <typeparam name="TEdge">the graph edge type.</typeparam>
/// <remarks>Author: Barak Naveh.</remarks>
public class EdgeTraversalEventArgs<TEdge> : EventArgs
{
    /// <summary>
    /// Creates a new EdgeTraversalEvent.
    /// </summary>
    /// <param name="eventSource"> the source of the event. </param>
    /// <param name="edge"> the traversed edge. </param>
    public EdgeTraversalEventArgs(object eventSource, TEdge edge)
    {
        Edge = edge;
    }

    /// <summary>
    /// The traversed edge.
    /// </summary>
    public TEdge Edge { get; set; }
}
