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

using NGraphT.Core.Util;

namespace NGraphT.Core.Graph.Specifics;

/// <summary>
/// An edge set factory which creates <see cref="ArrayUnenforcedSet{TElement}"/> of size 1, suitable for small degree
/// vertices.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public sealed class ArrayUnenforcedSetEdgeSetFactory<TVertex, TEdge> : IEdgeSetFactory<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <inheritdoc/>
    public ISet<TEdge> CreateEdgeSet(TVertex vertex)
    {
        // NOTE: use size 1 to keep memory usage under control
        // for the common case of vertices with low degree
        return new ArrayUnenforcedSet<TEdge>(1);
    }
}
