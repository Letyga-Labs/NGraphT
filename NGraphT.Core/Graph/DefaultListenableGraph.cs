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

namespace NGraphT.Core.Graph;

using Core;
using Util;

/// <summary>
/// A graph backed by the the graph specified at the constructor, which can be listened by
/// <c>GraphListener</c> s and by <c>
/// VertexSetListener</c> s. Operations on this graph "pass through" to the to the backing graph.
/// Any modification made to this graph or the backing graph is reflected by the other.
///
/// <para>
/// This graph does <i>not</i> pass the hashCode and equals operations through to the backing graph,
/// but relies on <c>Object</c>'s <c>equals</c> and <c>hashCode</c> methods.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
/// <seealso cref="GraphListener"/>
/// <seealso cref="VertexSetListener"/>
public class DefaultListenableGraph<TNode, TEdge> : GraphDelegator<TNode, TEdge>, IListenableGraph<TNode, TEdge>,
    ICloneable
{
    private IList<GraphListener<TNode, TEdge>> _graphListeners     = new List<GraphListener<TNode, TEdge>>();
    private IList<VertexSetListener<TNode>>    _vertexSetListeners = new List<VertexSetListener<TNode>>();
    private FlyweightEdgeEvent<TNode, TEdge>   _reuseableEdgeEvent;
    private FlyweightVertexEvent<TNode>        _reuseableVertexEvent;
    private bool                               _reuseEvents;

    ///<summary>
    ///Creates a new listenable graph.
    ///</summary>
    ///<param name="g"> the backing graph.</param>
    public DefaultListenableGraph(IGraph<TNode, TEdge> g)
        : this(g, false)
    {
    }

    ///<summary>
    ///Creates a new listenable graph. If the <c>reuseEvents</c> flag is set to
    ///<c>true</c> this class will reuse previously fired events and will not create a new
    ///object for each event. This option increases performance but should be used with care,
    ///especially in multithreaded environment.
    ///</summary>
    ///<param name="g"> the backing graph.</param>
    ///<param name="reuseEvents"> whether to reuse previously fired event objects instead of creating a new
    ///       event object for each event.</param>
    ///<exception cref="ArgumentException"> if the backing graph is already a listenable graph.</exception>
    public DefaultListenableGraph(IGraph<TNode, TEdge> g, bool reuseEvents)
        : base(g)
    {
        _reuseEvents     = reuseEvents;
        _reuseableEdgeEvent   = new FlyweightEdgeEvent<TNode, TEdge>(this, -1, null);
        _reuseableVertexEvent = new FlyweightVertexEvent<TNode>(this, -1, null);

        // the following restriction could be probably relaxed in the future.
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: if (g instanceof ListenableGraph<?, ?>)
        if (g is IListenableGraph<object, object>)
        {
            throw new ArgumentException("base graph cannot be listenable");
        }
    }

    ///<summary>
    ///If the <c>reuseEvents</c> flag is set to <c>true</c> this class will reuse
    ///previously fired events and will not create a new object for each event. This option
    ///increases performance but should be used with care, especially in multithreaded environment.
    ///</summary>
    ///<param name="reuseEvents"> whether to reuse previously fired event objects instead of creating a new
    ///       event object for each event.</param>
    public virtual bool ReuseEvents
    {
        set
        {
            _reuseEvents = value;
        }
        get
        {
            return _reuseEvents;
        }
    }


    public override TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
    {
        var edge = base.AddEdge(sourceVertex, targetVertex);

        if (edge != null)
        {
            FireEdgeAdded(edge, sourceVertex, targetVertex, Graph.DEFAULT_EDGE_WEIGHT);
        }

        return edge;
    }

    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        var added = base.AddEdge(sourceVertex, targetVertex, edge);

        if (added)
        {
            FireEdgeAdded(edge, sourceVertex, targetVertex, Graph.DEFAULT_EDGE_WEIGHT);
        }

        return added;
    }

    public virtual void AddGraphListener(GraphListener<TNode, TEdge> l)
    {
        AddToListenerList(_graphListeners, l);
    }

    public override TNode AddVertex()
    {
        var node = base.AddVertex();
        if (node != null)
        {
            FireVertexAdded(node);
        }

        return node;
    }

    public override bool AddVertex(TNode node)
    {
        var modified = base.AddVertex(node);

        if (modified)
        {
            FireVertexAdded(node);
        }

        return modified;
    }

    public virtual void AddVertexSetListener(VertexSetListener<TNode> l)
    {
        AddToListenerList(_vertexSetListeners, l);
    }

    public override object Clone()
    {
        try
        {
            DefaultListenableGraph<TNode, TEdge> g = TypeUtil.UncheckedCast(base.clone());
            g._graphListeners     = new List<GraphListener<TNode, TEdge>>();
            g._vertexSetListeners = new List<VertexSetListener<TNode>>();

            return g;
        }
        catch (CloneNotSupportedException edge)
        {
            // should never get here since we're Cloneable
            Console.WriteLine(edge.ToString());
            Console.Write(edge.StackTrace);
            throw new Exception("internal error");
        }
    }

    public override TEdge RemoveEdge(TNode sourceVertex, TNode targetVertex)
    {
        var edge = base.GetEdge(sourceVertex, targetVertex);
        if (edge != null)
        {
            var weight = base.GetEdgeWeight(edge);
            if (base.RemoveEdge(edge))
            {
                FireEdgeRemoved(edge, sourceVertex, targetVertex, weight);
            }
        }

        return edge;
    }

    public override bool RemoveEdge(TEdge edge)
    {
        var  sourceVertex = GetEdgeSource(edge);
        var  targetVertex = GetEdgeTarget(edge);
        var weight       = GetEdgeWeight(edge);

        var modified = base.RemoveEdge(edge);

        if (modified)
        {
            FireEdgeRemoved(edge, sourceVertex, targetVertex, weight);
        }

        return modified;
    }

    public virtual void RemoveGraphListener(GraphListener<TNode, TEdge> l)
    {
        _graphListeners.Remove(l);
    }

    public override bool RemoveVertex(TNode node)
    {
        if (ContainsVertex(node))
        {
            var touchingEdgesList = EdgesOf(node);

            // copy set to avoid ConcurrentModificationException
            removeAllEdges(new List<>(touchingEdgesList));

            base.RemoveVertex(node); // remove the vertex itself

            FireVertexRemoved(node);

            return true;
        }
        else
        {
            return false;
        }
    }

    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        base.SetEdgeWeight(edge, weight);

        var sourceVertex = GetEdgeSource(edge);
        var targetVertex = GetEdgeTarget(edge);

        FireEdgeWeightUpdated(edge, sourceVertex, targetVertex, weight);
    }

    public virtual void RemoveVertexSetListener(VertexSetListener<TNode> l)
    {
        _vertexSetListeners.Remove(l);
    }

    ///<summary>
    ///Notify listeners that the specified edge was added.
    ///</summary>
    ///<param name="edge"> the edge that was added.</param>
    ///<param name="source"> edge source.</param>
    ///<param name="target"> edge target.</param>
    ///<param name="weight"> edge weight.</param>
    protected internal virtual void FireEdgeAdded(TEdge edge, TNode source, TNode target, double weight)
    {
        GraphEdgeChangeEvent<TNode, TEdge> edge =
            CreateGraphEdgeChangeEvent(GraphEdgeChangeEvent.EDGE_ADDED, edge, source, target, weight);

        foreach (GraphListener<TNode, TEdge> l in _graphListeners)
        {
            l.EdgeAdded(edge);
        }
    }

    ///<summary>
    ///Notify listeners that the specified edge was removed.
    ///</summary>
    ///<param name="edge"> the edge that was removed.</param>
    ///<param name="source"> edge source.</param>
    ///<param name="target"> edge target.</param>
    ///<param name="weight"> edge weight.</param>
    protected internal virtual void FireEdgeRemoved(TEdge edge, TNode source, TNode target, double weight)
    {
        GraphEdgeChangeEvent<TNode, TEdge> edge =
            CreateGraphEdgeChangeEvent(GraphEdgeChangeEvent.EDGE_REMOVED, edge, source, target, weight);

        foreach (GraphListener<TNode, TEdge> l in _graphListeners)
        {
            l.EdgeRemoved(edge);
        }
    }

    ///<summary>
    ///Notify listeners that the weight of an edge has changed.
    ///</summary>
    ///<param name="edge"> the edge whose weight has changed.</param>
    ///<param name="source"> edge source.</param>
    ///<param name="target"> edge target.</param>
    ///<param name="weight"> the edge weight.</param>
    protected internal virtual void FireEdgeWeightUpdated(TEdge edge, TNode source, TNode target, double weight)
    {
        GraphEdgeChangeEvent<TNode, TEdge> edge =
            CreateGraphEdgeChangeEvent(GraphEdgeChangeEvent.EDGE_WEIGHT_UPDATED, edge, source, target, weight);

        foreach (GraphListener<TNode, TEdge> l in _graphListeners)
        {
            l.EdgeWeightUpdated(edge);
        }
    }

    ///<summary>
    ///Notify listeners that the specified vertex was added.
    ///</summary>
    ///<param name="vertex"> the vertex that was added.</param>
    protected internal virtual void FireVertexAdded(TNode vertex)
    {
        GraphVertexChangeEvent<TNode> edge = CreateGraphVertexChangeEvent(GraphVertexChangeEvent.VERTEX_ADDED, vertex);

        foreach (VertexSetListener<TNode> l in _vertexSetListeners)
        {
            l.VertexAdded(edge);
        }

        foreach (GraphListener<TNode, TEdge> l in _graphListeners)
        {
            l.VertexAdded(edge);
        }
    }

    ///<summary>
    ///Notify listeners that the specified vertex was removed.
    ///</summary>
    ///<param name="vertex"> the vertex that was removed.</param>
    protected internal virtual void FireVertexRemoved(TNode vertex)
    {
        GraphVertexChangeEvent<TNode>
            edge = CreateGraphVertexChangeEvent(GraphVertexChangeEvent.VERTEX_REMOVED, vertex);

        foreach (VertexSetListener<TNode> l in _vertexSetListeners)
        {
            l.VertexRemoved(edge);
        }

        foreach (GraphListener<TNode, TEdge> l in _graphListeners)
        {
            l.VertexRemoved(edge);
        }
    }

    private static void AddToListenerList<TL>(IList<TL> list, TL l) where TL : EventListener
    {
        if (!list.Contains(l))
        {
            list.Add(l);
        }
    }

    private GraphEdgeChangeEvent<TNode, TEdge> CreateGraphEdgeChangeEvent(
        int    eventType,
        TEdge  edge,
        TNode  source,
        TNode  target,
        double weight
    )
    {
        if (_reuseEvents)
        {
            _reuseableEdgeEvent.Type       = eventType;
            _reuseableEdgeEvent.Edge       = edge;
            _reuseableEdgeEvent.EdgeSource = source;
            _reuseableEdgeEvent.EdgeTarget = target;
            _reuseableEdgeEvent.EdgeWeight = weight;

            return _reuseableEdgeEvent;
        }
        else
        {
            return new GraphEdgeChangeEvent<TNode, TEdge>(this, eventType, edge, source, target, weight);
        }
    }

    private GraphVertexChangeEvent<TNode> CreateGraphVertexChangeEvent(int eventType, TNode vertex)
    {
        if (_reuseEvents)
        {
            _reuseableVertexEvent.Type   = eventType;
            _reuseableVertexEvent.Vertex = vertex;

            return _reuseableVertexEvent;
        }
        else
        {
            return new GraphVertexChangeEvent<TNode>(this, eventType, vertex);
        }
    }

    ///<summary>
    ///A reuseable edge event.
    ///
    ///<remarks>Author: Barak Naveh.</remarks>
    ///</summary>
    private class FlyweightEdgeEvent<TVv, TEe> : GraphEdgeChangeEvent<TVv, TEe>
    {
        internal const long SerialVersionUID = 3907207152526636089L;

        ///<see cref="GraphEdgeChangeEvent"/>
        public FlyweightEdgeEvent(object eventSource, int type, TEe edge)
            : base(eventSource, type, edge, default(TNode), default(TNode))
        {
        }

        ///<summary>
        ///Sets the edge of this event.
        ///</summary>
        ///<param name="edge"> the edge to be set.</param>
        protected internal virtual TEe Edge
        {
            set
            {
                this.edge = value;
            }
        }

        protected internal virtual TVv EdgeSource
        {
            set
            {
                this.edgeSource = value;
            }
        }

        protected internal virtual TVv EdgeTarget
        {
            set
            {
                this.edgeTarget = value;
            }
        }

        protected internal virtual double EdgeWeight
        {
            set
            {
                this.edgeWeight = value;
            }
        }

        ///<summary>
        ///Set the event type of this event.
        ///</summary>
        ///<param name="type"> the type to be set.</param>
        protected internal virtual int Type
        {
            set
            {
                this.type = value;
            }
        }
    }

    ///<summary>
    ///A reuseable vertex event.
    ///
    ///<remarks>Author: Barak Naveh.</remarks>
    ///</summary>
    private class FlyweightVertexEvent<TVv> : GraphVertexChangeEvent<TVv>
    {
        internal const long SerialVersionUID = 3257848787857585716L;

        ///<see cref="GraphVertexChangeEvent.GraphVertexChangeEvent(Object, int, Object)"/>
        public FlyweightVertexEvent(object eventSource, int type, TVv vertex)
            : base(eventSource, type, vertex)
        {
        }

        ///<summary>
        /// Set the event type of this event.
        /// </summary>
        /// <param name="type"> type to be set.</param>
        protected internal virtual int Type
        {
            set
            {
                this.type = value;
            }
        }

        /// <summary>
        /// Sets the vertex of this event.
        /// </summary>
        /// <param name="vertex"> the vertex to be set.</param>
        protected internal virtual TVv Vertex
        {
            set
            {
                this.vertex = value;
            }
        }
    }
}
