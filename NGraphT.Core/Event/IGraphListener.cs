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
namespace NGraphT.Core.Event;

/// <summary>
/// A listener that is notified when the graph changes.
///
/// <para>
/// If only notifications on vertex set changes are required it is more efficient to use the
/// VertexSetListener.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">the graph vertex type.</typeparam>
/// <typeparam name="TEdge">the graph edge type.</typeparam>
///
/// <seealso cref="IVertexSetListener{V}"/>
/// <remarks>Author: Barak Naveh.</remarks>
public interface IGraphListener<TNode, TEdge> : IVertexSetListener<TNode>
{
    /// <summary>
    /// Notifies that an edge has been added to the graph.
    /// </summary>
    /// <param name="e">The edge event.</param>
    void EdgeAdded(GraphEdgeChangeEventArgs<TNode, TEdge> e);

    /// <summary>
    /// Notifies that an edge has been removed from the graph.
    /// </summary>
    /// <param name="e">The edge event.</param>
    void EdgeRemoved(GraphEdgeChangeEventArgs<TNode, TEdge> e);

    /// <summary>
    /// Notifies that an edge weight has been updated.
    /// </summary>
    /// <param name="e">The edge event.</param>
    void EdgeWeightUpdated(GraphEdgeChangeEventArgs<TNode, TEdge> e);
}
