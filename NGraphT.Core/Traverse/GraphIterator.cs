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

namespace NGraphT.Core.Traverse;

/// <summary>
/// A graph iterator.
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public interface IGraphIterator<TNode, TEdge> : IEnumerator<TNode>
{
    /// <summary>
    /// Test whether this iterator is set to traverse the graph across connected components.
    /// </summary>
    /// <returns><c>true</c> if traverses across connected components, otherwise
    ///         <c>false</c>.</returns>
    bool CrossComponentTraversal { get; }

    /// <summary>
    /// Tests whether the <c>reuseEvents</c> flag is set. If the flag is set to
    /// <c>true</c> this class will reuse previously fired events and will not create a new
    /// object for each event. This option increases performance but should be used with care,
    /// especially in multithreaded environment.
    /// </summary>
    /// <returns>the value of the <c>reuseEvents</c> flag.</returns>
    bool ReuseEvents { get; set; }


    /// <summary>
    /// Adds the specified traversal listener to this iterator.
    /// </summary>
    /// <param name="l"> the traversal listener to be added.</param>
    void AddTraversalListener(TraversalListener<TNode, TEdge> l);

    /// <summary>
    /// Removes the specified traversal listener from this iterator.
    /// </summary>
    /// <param name="l"> the traversal listener to be removed.</param>
    void RemoveTraversalListener(TraversalListener<TNode, TEdge> l);

    /// <summary>
    /// Unsupported.
    /// </summary>
    /// <exception cref="NotSupportedException"> always since operation is not supported.</exception>
    void Remove();
}
