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
/// Exception indicating that the vertexes supplied to
/// <see cref="DirectedAcyclicGraph{TVertex,TEdge}"/> would cause a cycle.
/// </summary>
///
/// <remarks>Author: EnderCrypt (Magnus Gunnarsson).</remarks>
public class GraphCycleProhibitedException : InvalidOperationException
{
    // TODO: add diagnostic information: which edge or vertex is a problem
    public GraphCycleProhibitedException()
        : base("Edge would induce a cycle")
    {
    }
}
