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
/// Special <see cref="System.Exception"/> to signal that <see cref="IntrusiveEdge"/> is used incorrectly.
/// </summary>
///
/// <remarks>Author: Hannes Wellmann.</remarks>
public sealed class IntrusiveEdgeException : Exception
{
    public IntrusiveEdgeException(object? sourceVertex, object? targetVertex)
        : base($"There is edge already associated with source <{sourceVertex}> and target <{targetVertex}>")
    {
        SourceVertex = sourceVertex;
        TargetVertex = targetVertex;
    }

    internal object? SourceVertex { get; }

    internal object? TargetVertex { get; }
}
