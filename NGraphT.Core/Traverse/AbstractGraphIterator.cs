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

using System.Collections;
using NGraphT.Core.Events;
using Java2Net = J2N.Collections.Generic;

namespace NGraphT.Core.Traverse;

/// <summary>
/// An empty implementation of a graph iterator to minimize the effort required to implement graph
/// iterators.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public abstract class AbstractGraphIterator<TVertex, TEdge> : IGraphIterator<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private readonly ISet<ITraversalListener<TVertex, TEdge>> _traversalListeners =
        new Java2Net.LinkedHashSet<ITraversalListener<TVertex, TEdge>>();

    /// <summary>
    /// Create a new iterator.
    /// </summary>
    /// <param name="graph"> the graph.</param>
    protected AbstractGraphIterator(IGraph<TVertex, TEdge> graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        Graph                   = graph;
        CrossComponentTraversal = true;
        ReuseEvents             = false;
        ReusableEdgeEvent       = new FlyweightEdgeEventArgs<TEdge>(this, default!);
        ReusableVertexEvent     = new FlyweightVertexEventArgs<TVertex>(this, default!);
    }

    public abstract TVertex Current { get; }

    object? IEnumerator.Current => Current;

    public virtual bool CrossComponentTraversal { get; set; }

    public virtual bool ReuseEvents { get; set; }

    protected virtual IGraph<TVertex, TEdge> Graph { get; set; }

    // We keep this cached redundantly with traversalListeners.size()
    // so that subclasses can use it as a fast check to see if
    // event firing calls can be skipped.
    protected virtual int NListeners { get; set; }

    protected FlyweightEdgeEventArgs<TEdge> ReusableEdgeEvent { get; set; }

    protected FlyweightVertexEventArgs<TVertex> ReusableVertexEvent { get; set; }

    public virtual void AddTraversalListener(ITraversalListener<TVertex, TEdge> l)
    {
        _traversalListeners.Add(l);
        NListeners = _traversalListeners.Count;
    }

    public virtual void Remove()
    {
        throw new NotSupportedException("Remove");
    }

    public virtual void Reset()
    {
        throw new NotSupportedException("Reset");
    }

    public virtual void RemoveTraversalListener(ITraversalListener<TVertex, TEdge> l)
    {
        _traversalListeners.Remove(l);
        NListeners = _traversalListeners.Count;
    }

    public abstract bool MoveNext();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // empty
        }
    }

    /// <summary>
    /// Informs all listeners that the traversal of the current connected component finished.
    /// </summary>
    /// <param name="edge"> the connected component finished event.</param>
    protected virtual void FireConnectedComponentFinished(ConnectedComponentTraversalEventArgs edge)
    {
        foreach (var l in _traversalListeners)
        {
            l.ConnectedComponentFinished(edge);
        }
    }

    /// <summary>
    /// Informs all listeners that a traversal of a new connected component has started.
    /// </summary>
    /// <param name="edge"> the connected component started event.</param>
    protected virtual void FireConnectedComponentStarted(ConnectedComponentTraversalEventArgs edge)
    {
        foreach (var l in _traversalListeners)
        {
            l.ConnectedComponentStarted(edge);
        }
    }

    /// <summary>
    /// Informs all listeners that a the specified edge was visited.
    /// </summary>
    /// <param name="edge"> the edge traversal event.</param>
    protected virtual void FireEdgeTraversed(EdgeTraversalEventArgs<TEdge> edge)
    {
        foreach (var l in _traversalListeners)
        {
            l.EdgeTraversed(edge);
        }
    }

    /// <summary>
    /// Informs all listeners that a the specified vertex was visited.
    /// </summary>
    /// <param name="edge"> the vertex traversal event.</param>
    protected virtual void FireVertexTraversed(VertexTraversalEventArgs<TVertex> edge)
    {
        foreach (var l in _traversalListeners)
        {
            l.VertexTraversed(edge);
        }
    }

    /// <summary>
    /// Informs all listeners that a the specified vertex was finished.
    /// </summary>
    /// <param name="edge"> the vertex traversal event.</param>
    protected virtual void FireVertexFinished(VertexTraversalEventArgs<TVertex> edge)
    {
        foreach (var l in _traversalListeners)
        {
            l.VertexFinished(edge);
        }
    }

    /// <summary>
    /// Create a vertex traversal event.
    /// </summary>
    /// <param name="vertex"> the vertex.</param>
    /// <returns>the event.</returns>
    protected virtual VertexTraversalEventArgs<TVertex> CreateVertexTraversalEvent(TVertex vertex)
    {
        if (ReuseEvents)
        {
            ReusableVertexEvent.SetVertex(vertex);
            return ReusableVertexEvent;
        }
        else
        {
            return new VertexTraversalEventArgs<TVertex>(this, vertex);
        }
    }

    /// <summary>
    /// Create an edge traversal event.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <returns>the event.</returns>
    protected virtual EdgeTraversalEventArgs<TEdge> CreateEdgeTraversalEvent(TEdge edge)
    {
        if (ReuseEvents)
        {
            ReusableEdgeEvent.SetEdge(edge);
            return ReusableEdgeEvent;
        }
        else
        {
            return new EdgeTraversalEventArgs<TEdge>(this, edge);
        }
    }

    /// <summary>
    /// A reusable edge event.
    ///
    /// <remarks>Author: Barak Naveh.</remarks>
    /// </summary>
    protected internal class FlyweightEdgeEventArgs<TEdge1> : EdgeTraversalEventArgs<TEdge1>
    {
        /// <summary>
        /// Creates a new FlyweightEdgeEvent.
        /// </summary>
        /// <param name="eventSource"> the source of the event.</param>
        /// <param name="edge"> the traversed edge.</param>
        public FlyweightEdgeEventArgs(object eventSource, TEdge1 edge)
            : base(eventSource, edge)
        {
        }

        public void SetEdge(TEdge1 edge)
        {
            Edge = edge;
        }
    }

    /// <summary>
    /// A reusable vertex event.
    ///
    /// <remarks>Author: Barak Naveh.</remarks>
    /// </summary>
    protected internal class FlyweightVertexEventArgs<TV> : VertexTraversalEventArgs<TV>
    {
        /// <summary>
        /// Creates a new FlyweightVertexEvent.
        /// </summary>
        /// <param name="eventSource"> the source of the event.</param>
        /// <param name="vertex"> the traversed vertex.</param>
        public FlyweightVertexEventArgs(object eventSource, TV vertex)
            : base(eventSource, vertex)
        {
        }

        public void SetVertex(TV v)
        {
            Vertex = v;
        }
    }
}
