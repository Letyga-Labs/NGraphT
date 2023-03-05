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
    /// A traversal event for a graph vertex.
    /// </summary>
    /// @param <V> the graph vertex type
    /// 
    /// @author Barak Naveh </param>
    public class VertexTraversalEvent<V> : EventObject
    {
        private const long SerialVersionUID = 3688790267213918768L;

        /// <summary>
        /// The traversed vertex.
        /// </summary>
        protected internal V vertex;

        /// <summary>
        /// Creates a new VertexTraversalEvent.
        /// </summary>
        /// <param name="eventSource"> the source of the event. </param>
        /// <param name="vertex"> the traversed vertex. </param>
        public VertexTraversalEvent(object eventSource, V vertex) : base(eventSource)
        {
            this.vertex = vertex;
        }

        /// <summary>
        /// Returns the traversed vertex.
        /// </summary>
        /// <returns> the traversed vertex. </returns>
        public virtual V Vertex
        {
            get
            {
                return vertex;
            }
        }
    }

}