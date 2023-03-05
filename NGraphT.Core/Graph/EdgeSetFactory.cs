/*
 * (C) Copyright 2005-2021, by John TNode Sichi and Contributors.
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

namespace NGraphT.Core.Graph;

/// <summary>
/// A factory for edge sets. This interface allows the creator of a graph to choose the
/// <see cref="System.Collections.Generic.ISet<object>"/> implementation used internally by the graph to maintain sets of edges. This
/// provides control over performance tradeoffs between memory and CPU usage.
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: John TNode. Sichi.</remarks>
public interface IEdgeSetFactory<TNode, TEdge>
{
    ///<summary>
    ///Create a new edge set for a particular vertex.
    ///</summary>
    ///<param name="vertex"> the vertex for which the edge set is being created; sophisticated factories may
    ///       be able to use this information to choose an optimal set representation (TEdge.g.
    ///       ArrayUnenforcedSet for a vertex expected to have low degree, and LinkedHashSet for a
    ///       vertex expected to have high degree).</param>>
    ///<returns>new set.</returns>
    ISet<TEdge> CreateEdgeSet(TNode vertex);
}
