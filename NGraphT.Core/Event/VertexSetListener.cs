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
    /// A listener that is notified when the graph's vertex set changes. It should be used when
    /// <i>only</i> notifications on vertex-set changes are of interest. If all graph notifications are
    /// of interest better use <code>
    /// GraphListener</code>.
    /// </summary>
    /// @param <V> the graph vertex type
    /// 
    /// @author Barak Naveh </param>
    /// <seealso cref="GraphListener"/>
    public interface VertexSetListener<V> : EventListener
    {
        /// <summary>
        /// Notifies that a vertex has been added to the graph.
        /// </summary>
        /// <param name="e"> the vertex event. </param>
        void VertexAdded(GraphVertexChangeEvent<V> e);

        /// <summary>
        /// Notifies that a vertex has been removed from the graph.
        /// </summary>
        /// <param name="e"> the vertex event. </param>
        void VertexRemoved(GraphVertexChangeEvent<V> e);
    }

}