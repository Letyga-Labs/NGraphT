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
    /// An empty do-nothing implementation of the <seealso cref="TraversalListener"/> interface used for
    /// subclasses.
    /// </summary>
    /// @param <V> the graph vertex type </param>
    /// @param <E> the graph edge type
    /// 
    /// @author Barak Naveh </param>
    public class TraversalListenerAdapter<V, E> : TraversalListener<V, E>
    {
        /// <seealso cref="TraversalListener.connectedComponentFinished(ConnectedComponentTraversalEvent)"/>
        public virtual void ConnectedComponentFinished(ConnectedComponentTraversalEvent e)
        {
        }

        /// <seealso cref="TraversalListener.connectedComponentStarted(ConnectedComponentTraversalEvent)"/>
        public virtual void ConnectedComponentStarted(ConnectedComponentTraversalEvent e)
        {
        }

        /// <seealso cref="TraversalListener.edgeTraversed(EdgeTraversalEvent)"/>
        public virtual void EdgeTraversed(EdgeTraversalEvent<E> e)
        {
        }

        /// <seealso cref="TraversalListener.vertexTraversed(VertexTraversalEvent)"/>
        public virtual void VertexTraversed(VertexTraversalEvent<V> e)
        {
        }

        /// <seealso cref="TraversalListener.vertexFinished(VertexTraversalEvent)"/>
        public virtual void VertexFinished(VertexTraversalEvent<V> e)
        {
        }
    }

}