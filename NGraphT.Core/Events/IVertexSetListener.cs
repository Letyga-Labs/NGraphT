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
/// A listener that is notified when the graph's vertex set changes. It should be used when
/// <i>only</i> notifications on vertex-set changes are of interest. If all graph notifications are
/// of interest better use <c>GraphListener</c>.
/// </summary>
///
/// <typeparam name="TVertex">the graph vertex type.</typeparam>
///
/// <seealso cref="IGraphListener{V,E}"/>
/// <remarks>Author: Barak Naveh.</remarks>
public interface IVertexSetListener<TVertex>
    where TVertex : class
{
    /// <summary>
    /// Notifies that a vertex has been added to the graph.
    /// </summary>
    /// <param name="e">The vertex event.</param>
    void VertexAdded(GraphVertexChangeEventArgs<TVertex> e);

    /// <summary>
    /// Notifies that a vertex has been removed from the graph.
    /// </summary>
    /// <param name="e">The vertex event.</param>
    void VertexRemoved(GraphVertexChangeEventArgs<TVertex> e);
}
