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

namespace NGraphT.Core.Traverse;

/// <summary>
/// An exception to signal that <see cref="TopologicalOrderIterator{TVertex,TEdge}"/>
/// is used for a non-directed acyclic graph.
/// Note that this class extends <see cref="System.ArgumentException"/> for backward compatibility.
/// </summary>
///
/// <remarks>Author: Kaiichiro Ota.</remarks>
public sealed class NotDirectedAcyclicGraphException : ArgumentException
{
    private const string GraphIsNotADag = "Graph is not a DAG";

    public NotDirectedAcyclicGraphException()
        : base(GraphIsNotADag)
    {
    }

    public NotDirectedAcyclicGraphException(string? message, string? paramName)
        : base(message, paramName)
    {
    }

    public NotDirectedAcyclicGraphException(string? message, string? paramName, Exception? innerException)
        : base(message, paramName, innerException)
    {
    }
}
