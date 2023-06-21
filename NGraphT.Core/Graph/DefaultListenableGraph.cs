// (C) Copyright 2003-2023, by Barak Naveh and Contributors.
//
// NGraphT : a free .NET graph-theory library.
// It is a third-party port of the JGraphT library and it
// strictly inherits all legal conditions of its origin:
// licenses, authorship rights, restrictions and permissions.
//
// See the CONTRIBUTORS.md file distributed with this work for additional
// information regarding copyright ownership.
//
// This program and the accompanying materials are made available under the
// terms of the Eclipse Public License 2.0 which is available at
// http://www.eclipse.org/legal/epl-2.0, or the
// GNU Lesser General Public License v2.1 or later
// which is available at
// http://www.gnu.org/licenses/old-licenses/lgpl-2.1-standalone.html.
//
// SPDX-License-Identifier: EPL-2.0 OR LGPL-2.1-or-later

using System.Diagnostics.CodeAnalysis;
using NGraphT.Core.Events;

namespace NGraphT.Core.Graph;

/// <summary>
/// A graph backed by the the graph specified at the constructor, which can be listened by
/// <c>IGraphListener</c>s and by <c>IVertexSetListener</c>s.
/// Operations on this graph "pass through" to the to the backing graph.
/// Any modification made to this graph or the backing graph is reflected by the other.
///
/// <para>
/// This graph does <i>not</i> pass the hashCode and equals operations through to the backing graph,
/// but relies on <c>Object</c>'s <c>equals</c> and <c>hashCode</c> methods.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <seealso cref="IGraphListener{TVertex,TEdge}"/>
/// <seealso cref="IVertexSetListener{TVertex}"/>
///
/// <remarks>Author: Barak Naveh.</remarks>
public sealed class DefaultListenableGraph<TVertex, TEdge> :
    GraphDelegator<TVertex, TEdge>,
    IListenableGraph<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private readonly IList<IGraphListener<TVertex, TEdge>> _graphListeners = new List<IGraphListener<TVertex, TEdge>>();
    private readonly IList<IVertexSetListener<TVertex>> _vertexSetListeners = new List<IVertexSetListener<TVertex>>();

    private readonly FlyweightEdgeEventArgs<TVertex, TEdge> _reuseableEdgeEventArgs;
    private readonly FlyweightVertexEventArgs<TVertex>      _reuseableVertexEventArgs;

    /// <summary>
    /// Creates a new listenable graph.
    /// </summary>
    /// <param name="g"> the backing graph.</param>
    public DefaultListenableGraph(IGraph<TVertex, TEdge> g)
        : this(g, reuseEvents: false)
    {
    }

    /// <summary>
    /// Creates a new listenable graph. If the <c>reuseEvents</c> flag is set to
    /// <c>true</c> this class will reuse previously fired events and will not create a new
    /// object for each event. This option increases performance but should be used with care,
    /// especially in multithreaded environment.
    /// </summary>
    /// <param name="g"> the backing graph.</param>
    /// <param name="reuseEvents">
    /// whether to reuse previously fired event objects instead of creating a new event object for each event.
    /// </param>
    /// <exception cref="ArgumentException"> if the backing graph is already a listenable graph.</exception>
    public DefaultListenableGraph(IGraph<TVertex, TEdge> g, bool reuseEvents)
        : base(g)
    {
        ReuseEvents               = reuseEvents;
        _reuseableEdgeEventArgs   = new FlyweightEdgeEventArgs<TVertex, TEdge>(this, -1, null);
        _reuseableVertexEventArgs = new FlyweightVertexEventArgs<TVertex>(this, -1, null);

        // the following restriction could be probably relaxed in the future.
        if (g is IListenableGraph<TVertex, TEdge>)
        {
            throw new ArgumentException("base graph cannot be listenable", nameof(g));
        }
    }

    /// <summary>
    /// If the <c>reuseEvents</c> flag is set to <c>true</c> this class will reuse
    /// previously fired events and will not create a new object for each event. This option
    /// increases performance but should be used with care, especially in multithreaded environment.
    /// </summary>
    public bool ReuseEvents { get; set; }

    public override TEdge? AddEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        var edge = base.AddEdge(sourceVertex, targetVertex);

        if (edge != null)
        {
            FireEdgeAdded(edge, sourceVertex, targetVertex, IGraph<object, object>.DefaultEdgeWeight);
        }

        return edge;
    }

    public override bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        var added = base.AddEdge(sourceVertex, targetVertex, edge);

        if (added)
        {
            FireEdgeAdded(edge, sourceVertex, targetVertex, IGraph<object, object>.DefaultEdgeWeight);
        }

        return added;
    }

    public void AddGraphListener(IGraphListener<TVertex, TEdge> l)
    {
        AddToListenerList(_graphListeners, l);
    }

    public override TVertex AddVertex()
    {
        var vertex = base.AddVertex();
        FireVertexAdded(vertex);
        return vertex;
    }

    public override bool AddVertex(TVertex vertex)
    {
        var modified = base.AddVertex(vertex);

        if (modified)
        {
            FireVertexAdded(vertex);
        }

        return modified;
    }

    public void AddVertexSetListener(IVertexSetListener<TVertex> l)
    {
        AddToListenerList(_vertexSetListeners, l);
    }

    public override TEdge? RemoveEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        var edge = GetEdge(sourceVertex, targetVertex);
        if (edge == null)
        {
            return edge;
        }

        var weight = GetEdgeWeight(edge);
        if (base.RemoveEdge(edge))
        {
            FireEdgeRemoved(edge, sourceVertex, targetVertex, weight);
        }

        return edge;
    }

    public override bool RemoveEdge([NotNullWhen(true)] TEdge? edge)
    {
        if (edge == null)
        {
            return false;
        }

        var sourceVertex = GetEdgeSource(edge);
        var targetVertex = GetEdgeTarget(edge);
        var weight       = GetEdgeWeight(edge);

        var modified = base.RemoveEdge(edge);

        if (modified)
        {
            FireEdgeRemoved(edge, sourceVertex, targetVertex, weight);
        }

        return modified;
    }

    public void RemoveGraphListener(IGraphListener<TVertex, TEdge> l)
    {
        _graphListeners.Remove(l);
    }

    public override bool RemoveVertex([NotNullWhen(true)] TVertex? vertex)
    {
        if (!ContainsVertex(vertex))
        {
            return false;
        }

        // copy set to avoid ConcurrentModificationException
        var touchingEdgesList = new List<TEdge>(EdgesOf(vertex));
        RemoveAllEdges(touchingEdgesList);

        // remove the vertex itself
        base.RemoveVertex(vertex);
        FireVertexRemoved(vertex);
        return true;
    }

    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        base.SetEdgeWeight(edge, weight);

        var sourceVertex = GetEdgeSource(edge);
        var targetVertex = GetEdgeTarget(edge);

        FireEdgeWeightUpdated(edge, sourceVertex, targetVertex, weight);
    }

    public void RemoveVertexSetListener(IVertexSetListener<TVertex> l)
    {
        _vertexSetListeners.Remove(l);
    }

    private static void AddToListenerList<TListener>(IList<TListener> list, TListener l)
    {
        if (!list.Contains(l))
        {
            list.Add(l);
        }
    }

    /// <summary>
    /// Notify listeners that the specified edge was added.
    /// </summary>
    /// <param name="edge"> the edge that was added.</param>
    /// <param name="source"> edge source.</param>
    /// <param name="target"> edge target.</param>
    /// <param name="weight"> edge weight.</param>
    private void FireEdgeAdded(TEdge edge, TVertex source, TVertex target, double weight)
    {
        var eventArgs = CreateGraphEdgeChangeEvent(
            GraphEdgeChangeEventArgs<TVertex, TEdge>.EdgeAdded, edge, source, target, weight
        );
        foreach (var l in _graphListeners)
        {
            l.EdgeAdded(eventArgs);
        }
    }

    /// <summary>
    /// Notify listeners that the specified edge was removed.
    /// </summary>
    /// <param name="edge"> the edge that was removed.</param>
    /// <param name="source"> edge source.</param>
    /// <param name="target"> edge target.</param>
    /// <param name="weight"> edge weight.</param>
    private void FireEdgeRemoved(TEdge edge, TVertex source, TVertex target, double weight)
    {
        var eventArgs = CreateGraphEdgeChangeEvent(
            GraphEdgeChangeEventArgs<TVertex, TEdge>.EdgeRemoved, edge, source, target, weight
        );
        foreach (var l in _graphListeners)
        {
            l.EdgeRemoved(eventArgs);
        }
    }

    /// <summary>
    /// Notify listeners that the weight of an edge has changed.
    /// </summary>
    /// <param name="edge"> the edge whose weight has changed.</param>
    /// <param name="source"> edge source.</param>
    /// <param name="target"> edge target.</param>
    /// <param name="weight"> the edge weight.</param>
    private void FireEdgeWeightUpdated(TEdge edge, TVertex source, TVertex target, double weight)
    {
        var eventArgs = CreateGraphEdgeChangeEvent(
            GraphEdgeChangeEventArgs<TVertex, TEdge>.EdgeWeightUpdated, edge, source, target, weight
        );
        foreach (var l in _graphListeners)
        {
            l.EdgeWeightUpdated(eventArgs);
        }
    }

    /// <summary>
    /// Notify listeners that the specified vertex was added.
    /// </summary>
    /// <param name="vertex"> the vertex that was added.</param>
    private void FireVertexAdded(TVertex vertex)
    {
        var eventArgs = CreateGraphVertexChangeEvent(GraphVertexChangeEventArgs<TVertex>.VertexAdded, vertex);

        foreach (var l in _vertexSetListeners)
        {
            l.VertexAdded(eventArgs);
        }

        foreach (var l in _graphListeners)
        {
            l.VertexAdded(eventArgs);
        }
    }

    /// <summary>
    /// Notify listeners that the specified vertex was removed.
    /// </summary>
    /// <param name="vertex"> the vertex that was removed.</param>
    private void FireVertexRemoved(TVertex vertex)
    {
        var eventArgs = CreateGraphVertexChangeEvent(GraphVertexChangeEventArgs<TVertex>.VertexRemoved, vertex);

        foreach (var l in _vertexSetListeners)
        {
            l.VertexRemoved(eventArgs);
        }

        foreach (var l in _graphListeners)
        {
            l.VertexRemoved(eventArgs);
        }
    }

    private GraphEdgeChangeEventArgs<TVertex, TEdge> CreateGraphEdgeChangeEvent(
        int     eventType,
        TEdge   edge,
        TVertex source,
        TVertex target,
        double  weight
    )
    {
        if (ReuseEvents)
        {
            _reuseableEdgeEventArgs.Type       = eventType;
            _reuseableEdgeEventArgs.Edge       = edge;
            _reuseableEdgeEventArgs.EdgeSource = source;
            _reuseableEdgeEventArgs.EdgeTarget = target;
            _reuseableEdgeEventArgs.EdgeWeight = weight;

            return _reuseableEdgeEventArgs;
        }

        return new GraphEdgeChangeEventArgs<TVertex, TEdge>(this, eventType, edge, source, target, weight);
    }

    private GraphVertexChangeEventArgs<TVertex> CreateGraphVertexChangeEvent(int eventType, TVertex vertex)
    {
        if (ReuseEvents)
        {
            _reuseableVertexEventArgs.Type   = eventType;
            _reuseableVertexEventArgs.Vertex = vertex;

            return _reuseableVertexEventArgs;
        }

        return new GraphVertexChangeEventArgs<TVertex>(this, eventType, vertex);
    }

    /// <summary>
    /// A reuseable edge event.
    /// </summary>
    /// <remarks>Author: Barak Naveh.</remarks>
    private sealed class FlyweightEdgeEventArgs<TV, TE> : GraphEdgeChangeEventArgs<TV, TE>
        where TV : class
        where TE : class
    {
        public FlyweightEdgeEventArgs(object eventSource, int type, TE? edge)
            : base(eventSource, type, edge!, null!, null!)
        {
        }
    }

    /// <summary>
    /// A reuseable vertex event.
    /// </summary>
    /// <remarks>Author: Barak Naveh.</remarks>
    private sealed class FlyweightVertexEventArgs<TV> : GraphVertexChangeEventArgs<TV>
        where TV : class
    {
        public FlyweightVertexEventArgs(object eventSource, int type, TV? vertex)
            : base(eventSource, type, vertex!)
        {
        }
    }
}
