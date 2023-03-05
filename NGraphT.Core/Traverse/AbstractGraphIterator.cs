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

using Core;

/// <summary>
/// An empty implementation of a graph iterator to minimize the effort required to implement graph
/// iterators.
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public abstract class AbstractGraphIterator<TNode, TEdge> : IGraphIterator<TNode, TEdge>
{
    private readonly ISet<TraversalListener<TNode, TEdge>> _traversalListeners =
        new LinkedHashSet<TraversalListener<TNode, TEdge>>();

    // We keep this cached redundantly with traversalListeners.size()
    // so that subclasses can use it as a fast check to see if
    // event firing calls can be skipped.
    protected internal int NListeners = 0;

    protected internal readonly FlyweightEdgeEvent<TEdge>   ReusableEdgeEvent;
    protected internal readonly FlyweightVertexEvent<TNode> ReusableVertexEvent;
    protected internal readonly IGraph<TNode, TEdge>        Graph;
    protected internal          bool                        CrossComponentTraversal;
    protected internal          bool                        ReuseEvents;

    /// <summary>
    /// Create a new iterator.
    /// </summary>
    /// <param name="graph"> the graph.</param>
    public AbstractGraphIterator(IGraph<TNode, TEdge> graph)
    {
        this.graph                   = Objects.requireNonNull(graph, "graph must not be null");
        ReusableEdgeEvent       = new FlyweightEdgeEvent<TEdge>(this, default(TEdge));
        ReusableVertexEvent     = new FlyweightVertexEvent<TNode>(this, default(TNode));
        this.crossComponentTraversal = true;
        this.reuseEvents             = false;
    }

    /// <summary>
    /// Get the graph being traversed.
    /// </summary>
    /// <returns>the graph being traversed.</returns>
    public virtual IGraph<TNode, TEdge> Graph
    {
        get
        {
            return graph;
        }
    }

    /// <summary>
    /// Sets the cross component traversal flag - indicates whether to traverse the graph across
    /// connected components.
    /// </summary>
    /// <param name="crossComponentTraversal"> if <c>true</c> traverses across connected components.</param>
    public virtual bool CrossComponentTraversal
    {
        set
        {
            this.crossComponentTraversal = value;
        }
        get
        {
            return crossComponentTraversal;
        }
    }


    public virtual bool ReuseEvents
    {
        set
        {
            this.reuseEvents = value;
        }
        get
        {
            return reuseEvents;
        }
    }


    public virtual void AddTraversalListener(TraversalListener<TNode, TEdge> l)
    {
        _traversalListeners.Add(l);
        NListeners = _traversalListeners.Count;
    }

    public virtual void Remove()
    {
        throw new NotSupportedException("remove");
    }

    public virtual void RemoveTraversalListener(TraversalListener<TNode, TEdge> l)
    {
        _traversalListeners.remove(l);
        NListeners = _traversalListeners.Count;
    }

    /// <summary>
    /// Informs all listeners that the traversal of the current connected component finished.
    /// </summary>
    /// <param name="edge"> the connected component finished event.</param>
    protected internal virtual void FireConnectedComponentFinished(ConnectedComponentTraversalEvent edge)
    {
        foreach (TraversalListener<TNode, TEdge> l in _traversalListeners)
        {
            l.ConnectedComponentFinished(edge);
        }
    }

    /// <summary>
    /// Informs all listeners that a traversal of a new connected component has started.
    /// </summary>
    /// <param name="edge"> the connected component started event.</param>
    protected internal virtual void FireConnectedComponentStarted(ConnectedComponentTraversalEvent edge)
    {
        foreach (TraversalListener<TNode, TEdge> l in _traversalListeners)
        {
            l.ConnectedComponentStarted(edge);
        }
    }

    /// <summary>
    /// Informs all listeners that a the specified edge was visited.
    /// </summary>
    /// <param name="edge"> the edge traversal event.</param>
    protected internal virtual void FireEdgeTraversed(EdgeTraversalEvent<TEdge> edge)
    {
        foreach (TraversalListener<TNode, TEdge> l in _traversalListeners)
        {
            l.EdgeTraversed(edge);
        }
    }

    /// <summary>
    /// Informs all listeners that a the specified vertex was visited.
    /// </summary>
    /// <param name="edge"> the vertex traversal event.</param>
    protected internal virtual void FireVertexTraversed(VertexTraversalEvent<TNode> edge)
    {
        foreach (TraversalListener<TNode, TEdge> l in _traversalListeners)
        {
            l.VertexTraversed(edge);
        }
    }

    /// <summary>
    /// Informs all listeners that a the specified vertex was finished.
    /// </summary>
    /// <param name="edge"> the vertex traversal event.</param>
    protected internal virtual void FireVertexFinished(VertexTraversalEvent<TNode> edge)
    {
        foreach (TraversalListener<TNode, TEdge> l in _traversalListeners)
        {
            l.VertexFinished(edge);
        }
    }

    /// <summary>
    /// Create a vertex traversal event.
    /// </summary>
    /// <param name="vertex"> the vertex.</param>
    /// <returns>the event.</returns>
    protected internal virtual VertexTraversalEvent<TNode> CreateVertexTraversalEvent(TNode vertex)
    {
        if (reuseEvents)
        {
            ReusableVertexEvent.Vertex = vertex;
            return ReusableVertexEvent;
        }
        else
        {
            return new VertexTraversalEvent<TNode>(this, vertex);
        }
    }

    /// <summary>
    /// Create an edge traversal event.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <returns>the event.</returns>
    protected internal virtual EdgeTraversalEvent<TEdge> CreateEdgeTraversalEvent(TEdge edge)
    {
        if (ReuseEvents)
        {
            ReusableEdgeEvent.Edge = edge;
            return ReusableEdgeEvent;
        }
        else
        {
            return new EdgeTraversalEvent<TEdge>(this, edge);
        }
    }

    /// <summary>
    /// A reusable edge event.
    ///
    /// <remarks>Author: Barak Naveh.</remarks>
    /// </summary>
    internal class FlyweightEdgeEvent<TEdge> : EdgeTraversalEvent<TEdge>
    {
        internal const long SerialVersionUID = 4051327833765000755L;

        /// <summary>
        /// Creates a new FlyweightEdgeEvent.
        /// </summary>
        /// <param name="eventSource"> the source of the event.</param>
        /// <param name="edge"> the traversed edge.</param>
        public FlyweightEdgeEvent(object eventSource, TEdge edge)
            : base(eventSource, edge)
        {
        }

        /// <summary>
        /// Sets the edge of this event.
        /// </summary>
        /// <param name="edge"> the edge to be set.</param>
        protected internal virtual TEdge Edge
        {
            set
            {
                this.edge = value;
            }
        }
    }

    /// <summary>
    /// A reusable vertex event.
    ///
    /// <remarks>Author: Barak Naveh.</remarks>
    /// </summary>
    internal class FlyweightVertexEvent<TNode> : VertexTraversalEvent<TNode>
    {
        internal const long SerialVersionUID = 3834024753848399924L;

        /// <summary>
        /// Creates a new FlyweightVertexEvent.
        /// </summary>
        /// <param name="eventSource"> the source of the event.</param>
        /// <param name="vertex"> the traversed vertex.</param>
        public FlyweightVertexEvent(object eventSource, TNode vertex)
            : base(eventSource, vertex)
        {
        }

        /// <summary>
        /// Sets the vertex of this event.
        /// </summary>
        /// <param name="vertex"> the vertex to be set.</param>
        protected internal virtual TNode Vertex
        {
            set
            {
                this.vertex = value;
            }
        }
    }
}
