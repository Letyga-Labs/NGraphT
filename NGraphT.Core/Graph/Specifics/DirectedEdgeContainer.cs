/*
 * (C) Copyright 2015-2021, by Barak Naveh and Contributors.
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

namespace NGraphT.Core.Graph.Specifics;

using Graph;

/// <summary>
/// A container for vertex edges.
///
/// <para>
/// In this edge container we use array lists to minimize memory toll. However, for high-degree
/// vertices we replace the entire edge container with a direct access subclass (to be implemented).
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public class DirectedEdgeContainer<TNode, TEdge>
{
    private const long        SerialVersionUID = 7494242245729767106L;
    internal      ISet<TEdge> Incoming;
    internal      ISet<TEdge> Outgoing;

    private ISet<TEdge> _unmodifiableIncoming = null;

    private ISet<TEdge> _unmodifiableOutgoing = null;

    internal DirectedEdgeContainer(IEdgeSetFactory<TNode, TEdge> edgeSetFactory, TNode vertex)
    {
        Incoming = edgeSetFactory.CreateEdgeSet(vertex);
        Outgoing = edgeSetFactory.CreateEdgeSet(vertex);
    }

    ///<summary>
    ///A lazy build of unmodifiable incoming edge set.
    ///</summary>
    ///<returns>an unmodifiable version of the incoming edge set.</returns>
    public virtual ISet<TEdge> UnmodifiableIncomingEdges
    {
        get
        {
            if (_unmodifiableIncoming == null)
            {
                _unmodifiableIncoming = Collections.unmodifiableSet(Incoming);
            }

            return _unmodifiableIncoming;
        }
    }

    ///<summary>
    ///A lazy build of unmodifiable outgoing edge set.
    ///</summary>
    ///<returns>an unmodifiable version of the outgoing edge set.</returns>
    public virtual ISet<TEdge> UnmodifiableOutgoingEdges
    {
        get
        {
            if (_unmodifiableOutgoing == null)
            {
                _unmodifiableOutgoing = Collections.unmodifiableSet(Outgoing);
            }

            return _unmodifiableOutgoing;
        }
    }

    ///<summary>
    ///Add an incoming edge.
    ///</summary>
    ///<param name="edge"> the edge to add.</param>
    public virtual void AddIncomingEdge(TEdge edge)
    {
        Incoming.Add(edge);
    }

    ///<summary>
    ///Add an outgoing edge.
    ///</summary>
    ///<param name="edge"> the edge to add.</param>
    public virtual void AddOutgoingEdge(TEdge edge)
    {
        Outgoing.Add(edge);
    }

    ///<summary>
    ///Remove an incoming edge.
    ///</summary>
    ///<param name="edge"> the edge to remove.</param>
    public virtual void RemoveIncomingEdge(TEdge edge)
    {
        Incoming.remove(edge);
    }

    ///<summary>
    ///Remove an outgoing edge.
    ///</summary>
    ///<param name="edge"> the edge to remove.</param>
    public virtual void RemoveOutgoingEdge(TEdge edge)
    {
        Outgoing.remove(edge);
    }
}
