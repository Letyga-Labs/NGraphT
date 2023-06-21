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

/// <summary>
/// IntrusiveEdge encapsulates the internals for the default edge implementation. It is not intended
/// to be referenced directly (which is why it's not public); use DefaultEdge for that.
/// </summary>
///
/// <remarks>Author: John V. Sichi.</remarks>
public record IntrusiveEdge
{
    /// <summary>
    /// Retrieves the source of this edge. This is protected, for use by subclasses only
    /// (e.g. for implementing toString).
    /// </summary>
    /// <returns>source of this edge.</returns>
    protected internal virtual object? Source { get; set; }

    /// <summary>
    /// Retrieves the target of this edge. This is protected, for use by subclasses only
    /// (e.g. for implementing toString).
    /// </summary>
    /// <returns>target of this edge.</returns>
    protected internal virtual object? Target { get; set; }
}
