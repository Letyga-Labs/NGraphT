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

namespace NGraphT.Core;

/// <summary>
/// GraphMapping represents a bidirectional mapping between two graphs (called graph1 and graph2),
/// which allows the caller to obtain the matching vertex or edge in either direction, from graph1 to
/// graph2, or from graph2 to graph1. It does not have to always be a complete bidirectional mapping
/// (it could return null for some lookups).
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Assaf Lehr.</remarks>
public interface IGraphMapping<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Gets the mapped value where the key is <c>vertex</c>.
    /// </summary>
    /// <param name="vertex"> vertex in one of the graphs.</param>
    /// <param name="forward">
    /// if true, uses mapping from graph1 to graph2; if false, use mapping from graph2 to graph1.
    /// </param>>
    /// <returns>corresponding vertex in other graph, or null if none.</returns>
    TVertex? GetVertexCorrespondence(TVertex vertex, bool forward);

    /// <summary>
    /// Gets the mapped value where the key is <c>edge</c>.
    /// </summary>
    /// <param name="edge"> edge in one of the graphs.</param>
    /// <param name="forward">
    /// if true, uses mapping from graph1 to graph2; if false, use mapping from graph2 to graph1.
    /// </param>>
    /// <returns>corresponding edge in other graph, or null if none.</returns>
    TEdge? GetEdgeCorrespondence(TEdge edge, bool forward);
}
