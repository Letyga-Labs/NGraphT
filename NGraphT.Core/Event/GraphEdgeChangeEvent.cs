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
    /// An event which indicates that a graph edge has changed, or is about to change. The event can be
    /// used either as an indication <i>after</i> the edge has been added or removed, or <i>before</i> it
    /// is added. The type of the event can be tested using the
    /// <seealso cref="org.jgrapht.event.GraphChangeEvent.getType()"/> method.
    /// </summary>
    /// @param <V> the graph vertex type </param>
    /// @param <E> the graph edge type
    /// 
    /// @author Barak Naveh </param>
    public class GraphEdgeChangeEvent<V, E> : GraphChangeEvent
    {
        private const long SerialVersionUID = -4421610303769803253L;

        /// <summary>
        /// Before edge added event. This event is fired before an edge is added to a graph.
        /// </summary>
        public const int BeforeEdgeAdded = 21;

        /// <summary>
        /// Before edge removed event. This event is fired before an edge is removed from a graph.
        /// </summary>
        public const int BeforeEdgeRemoved = 22;

        /// <summary>
        /// Edge added event. This event is fired after an edge is added to a graph.
        /// </summary>
        public const int EdgeAdded = 23;

        /// <summary>
        /// Edge removed event. This event is fired after an edge is removed from a graph.
        /// </summary>
        public const int EdgeRemoved = 24;

        /// <summary>
        /// Edge weight updated event. This event is fired after an edge weight is updated in a graph.
        /// </summary>
        public const int EdgeWeightUpdated = 25;

        /// <summary>
        /// The edge that this event is related to.
        /// </summary>
        protected internal E edge;

        /// <summary>
        /// The source vertex of the edge that this event is related to.
        /// </summary>
        protected internal V edgeSource;

        /// <summary>
        /// The target vertex of the edge that this event is related to.
        /// </summary>
        protected internal V edgeTarget;

        /// <summary>
        /// The weight of the edge that this event is related to.
        /// </summary>
        protected internal double edgeWeight;

        /// <summary>
        /// Constructor for GraphEdgeChangeEvent.
        /// </summary>
        /// <param name="eventSource"> the source of this event. </param>
        /// <param name="type"> the event type of this event. </param>
        /// <param name="edge"> the edge that this event is related to. </param>
        /// <param name="edgeSource"> edge source vertex </param>
        /// <param name="edgeTarget"> edge target vertex </param>
        public GraphEdgeChangeEvent(object eventSource, int type, E edge, V edgeSource, V edgeTarget) : this(eventSource, type, edge, edgeSource, edgeTarget, Graph.DEFAULT_EDGE_WEIGHT)
        {
        }

        /// <summary>
        /// Constructor for GraphEdgeChangeEvent.
        /// </summary>
        /// <param name="eventSource"> the source of this event. </param>
        /// <param name="type"> the event type of this event. </param>
        /// <param name="edge"> the edge that this event is related to. </param>
        /// <param name="edgeSource"> edge source vertex </param>
        /// <param name="edgeTarget"> edge target vertex </param>
        /// <param name="edgeWeight"> edge weight </param>
        public GraphEdgeChangeEvent(object eventSource, int type, E edge, V edgeSource, V edgeTarget, double edgeWeight) : base(eventSource, type)
        {
            this.edge = edge;
            this.edgeSource = edgeSource;
            this.edgeTarget = edgeTarget;
            this.edgeWeight = edgeWeight;
        }

        /// <summary>
        /// Returns the edge that this event is related to.
        /// </summary>
        /// <returns> event edge </returns>
        public virtual E Edge
        {
            get
            {
                return edge;
            }
        }

        /// <summary>
        /// Returns the source vertex that this event is related to.
        /// </summary>
        /// <returns> event source vertex </returns>
        public virtual V EdgeSource
        {
            get
            {
                return edgeSource;
            }
        }

        /// <summary>
        /// Returns the target vertex that this event is related to.
        /// </summary>
        /// <returns> event target vertex </returns>
        public virtual V EdgeTarget
        {
            get
            {
                return edgeTarget;
            }
        }

        /// <summary>
        /// Returns the weight of the edge that this event is related to.
        /// </summary>
        /// <returns> event edge weight </returns>
        public virtual double EdgeWeight
        {
            get
            {
                return edgeWeight;
            }
        }

    }

}