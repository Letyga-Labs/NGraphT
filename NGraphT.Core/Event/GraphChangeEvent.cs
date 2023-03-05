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
    /// An event which indicates that a graph has changed. This class is a root for graph change events.
    /// 
    /// @author Barak Naveh
    /// </summary>
    public class GraphChangeEvent : EventObject
    {
        private const long SerialVersionUID = 3834592106026382391L;

        /// <summary>
        /// The type of graph change this event indicates.
        /// </summary>
        protected internal int type;

        /// <summary>
        /// Creates a new graph change event.
        /// </summary>
        /// <param name="eventSource"> the source of the event. </param>
        /// <param name="type"> the type of event. </param>
        public GraphChangeEvent(object eventSource, int type) : base(eventSource)
        {
            this.type = type;
        }

        /// <summary>
        /// Returns the event type.
        /// </summary>
        /// <returns> the event type. </returns>
        public virtual int Type
        {
            get
            {
                return type;
            }
        }
    }

}