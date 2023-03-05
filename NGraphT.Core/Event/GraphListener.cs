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
namespace NGraphT.Core.Event
{
    /// <summary>
    /// A listener that is notified when the graph changes.
    /// 
    /// <para>
    /// If only notifications on vertex set changes are required it is more efficient to use the
    /// VertexSetListener.
    /// </para>
    /// </summary>
    /// @param <V> the graph vertex type </param>
    /// @param <E> the graph edge type
    /// 
    /// @author Barak Naveh </param>
    /// <seealso cref="VertexSetListener"/>
    public interface GraphListener<V, E> : VertexSetListener<V>
    {
        /// <summary>
        /// Notifies that an edge has been added to the graph.
        /// </summary>
        /// <param name="e"> the edge event. </param>
        void EdgeAdded(GraphEdgeChangeEvent<V, E> e);

        /// <summary>
        /// Notifies that an edge has been removed from the graph.
        /// </summary>
        /// <param name="e"> the edge event. </param>
        void EdgeRemoved(GraphEdgeChangeEvent<V, E> e);

        /// <summary>
        /// Notifies that an edge weight has been updated.
        /// </summary>
        /// <param name="e"> the edge event. </param>
        void EdgeWeightUpdated(GraphEdgeChangeEvent<V, E> e)
        {
        }

    }

}