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
/// An event which indicates that a graph has changed. This class is a root for graph change events.
/// </summary>
/// <remarks>Author: Barak Naveh.</remarks>
public class GraphChangeEventArgs : EventArgs
{
    /// <summary>
    /// Creates a new graph change event.
    /// </summary>
    /// <param name="eventSource"> the source of the event. </param>
    /// <param name="type"> the type of event. </param>
    public GraphChangeEventArgs(object eventSource, int type)
    {
        Type = type;
    }

    /// <summary>
    /// The type of graph change this event indicates.
    /// </summary>
    public int Type { get; protected internal set; }
}
