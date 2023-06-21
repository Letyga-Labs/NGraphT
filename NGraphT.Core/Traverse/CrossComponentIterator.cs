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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NGraphT.Core.Events;
using NGraphT.Core.Util;

namespace NGraphT.Core.Traverse;

/// <summary>
/// Provides a cross-connected-component traversal functionality for iterator subclasses.
/// </summary>
///
/// <typeparam name="TVertex"> vertex type.</typeparam>
/// <typeparam name="TEdge"> edge type.</typeparam>
/// <typeparam name="TVertexData"> type of data associated to seen vertices.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public abstract class CrossComponentIterator<TVertex, TEdge, TVertexData> : AbstractGraphIterator<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private const int CcsBeforeComponent = 1;
    private const int CcsWithinComponent = 2;
    private const int CcsAfterComponent  = 3;

    /// <summary>
    /// Stores the vertices that have been seen during iteration and (optionally) some additional
    /// traversal info regarding each vertex.
    /// </summary>
    private readonly IDictionary<TVertex, TVertexData> _seen = new Dictionary<TVertex, TVertexData>();

    private readonly ConnectedComponentTraversalEventArgs _ccFinishedEvent;
    private readonly ConnectedComponentTraversalEventArgs _ccStartedEvent;

    /// <summary>
    /// Iterator which provides start vertices for specified start vertices.
    /// </summary>
    private readonly IEnumerator<TVertex>? _startVertexIterator;

    /// <summary>
    /// Iterator which provides start vertices for cross-component iteration.
    /// </summary>
    private IEnumerator<TVertex>? _entireGraphVertexIterator;

    /// <summary>
    /// The current vertex.
    /// </summary>
    private TVertex? _startVertex;

    /// <summary>
    /// The connected component state.
    /// </summary>
    private int _state = CcsBeforeComponent;

    /// <summary>
    /// Contains current vertex of the <c>graph</c>.
    /// </summary>
    private TVertex? _current;

    /// <summary>
    /// Creates a new iterator for the specified graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    protected CrossComponentIterator(IGraph<TVertex, TEdge> g)
        : this(g, (TVertex?)null)
    {
    }

    /// <summary>
    /// Creates a new iterator for the specified graph. Iteration will start at the specified start
    /// vertex. If the specified start vertex is <c> null</c>, Iteration will start at an arbitrary graph vertex.
    /// </summary>
    ///
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertex"> the vertex iteration to be started.</param>
    ///
    /// <exception cref="ArgumentException">
    ///     if <c>g==null</c> or does not contain <c>startVertex</c>.
    /// </exception>
    protected CrossComponentIterator(IGraph<TVertex, TEdge> g, TVertex? startVertex)
        : this(g, startVertex == null ? Array.Empty<TVertex>() : new List<TVertex> { startVertex })
    {
    }

    /// <summary>
    /// Creates a new iterator for the specified graph. Iteration will start at the specified start
    /// vertices. If the specified start vertices is <c> null</c>, Iteration will start at an arbitrary graph vertex.
    /// </summary>
    ///
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertices"> the vertices iteration to be started.</param>
    ///
    /// <exception cref="ArgumentException">
    ///     if <c>g==null</c> or does not contain <c>startVertex</c>.
    /// </exception>
    protected CrossComponentIterator(IGraph<TVertex, TEdge> g, IEnumerable<TVertex>? startVertices)
        : base(g)
    {
        _ccFinishedEvent = new ConnectedComponentTraversalEventArgs(
            this,
            ConnectedComponentTraversalEventArgs.ConnectedComponentFinished
        );
        _ccStartedEvent = new ConnectedComponentTraversalEventArgs(
            this,
            ConnectedComponentTraversalEventArgs.ConnectedComponentStarted
        );

        // Initialize crossComponentTraversal and test for containment
        if (startVertices == null)
        {
            CrossComponentTraversal = true;
        }
        else
        {
            CrossComponentTraversal = false;
            _startVertexIterator    = startVertices.GetEnumerator();
        }

        // Initialize start vertex
        var it = CrossComponentTraversal ? GetEntireGraphVertexIterator() : _startVertexIterator;
        if (it != null && it.MoveNext())
        {
            _startVertex = it.Current;
            if (!Graph.ContainsVertex(_startVertex))
            {
                throw new ArgumentException("graph must contain the start vertex", nameof(startVertices));
            }
        }
        else
        {
            _startVertex = null;
        }
    }

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override TVertex Current => _current ?? throw new NoSuchElementException();

    /// <summary>
    /// Returns <c>true</c> if there are no more uniterated vertices in the currently iterated
    /// connected component; <c>false</c> otherwise.
    /// </summary>
    /// <returns>
    /// <c>true</c> if there are no more uniterated vertices in the currently iterated
    /// connected component; <c>false</c> otherwise.
    /// </returns>
    protected abstract bool ConnectedComponentExhausted { get; }

    public override bool MoveNext()
    {
        if (_startVertex != null)
        {
            EncounterStartVertex();
        }

        if (!HasNext())
        {
            return false;
        }

        if (_state == CcsBeforeComponent)
        {
            _state = CcsWithinComponent;
            if (NListeners != 0)
            {
                FireConnectedComponentStarted(_ccStartedEvent);
            }
        }

        var nextVertex = ProvideNextVertex();
        if (NListeners != 0)
        {
            FireVertexTraversed(CreateVertexTraversalEvent(nextVertex));
        }

        AddUnseenChildrenOf(nextVertex);

        _current = nextVertex;
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _startVertexIterator?.Dispose();
            _entireGraphVertexIterator?.Dispose();
        }
    }

    /// <summary>
    /// Lazily instantiates <c>entireGraphVertexIterator</c>.
    /// </summary>
    /// <returns>iterator which provides start vertices for cross-component iteration.</returns>
    protected IEnumerator<TVertex> GetEntireGraphVertexIterator()
    {
        if (_entireGraphVertexIterator == null)
        {
            Debug.Assert(CrossComponentTraversal, $"{nameof(CrossComponentTraversal)}must be set to true");
            _entireGraphVertexIterator = Graph.VertexSet().GetEnumerator();
        }

        return _entireGraphVertexIterator;
    }

    /// <summary>
    /// Update data structures the first time we see a vertex.
    /// </summary>
    /// <param name="vertex"> the vertex encountered.</param>
    /// <param name="edge"> the edge via which the vertex was encountered, or null if the vertex is a
    ///        starting point.</param>
    protected abstract void EncounterVertex(TVertex vertex, TEdge? edge);

    /// <summary>
    /// Called whenever we re-encounter a vertex. The default implementation does nothing.
    /// </summary>
    /// <param name="vertex"> the vertex re-encountered.</param>
    /// <param name="edge"> the edge via which the vertex was re-encountered.</param>
    protected abstract void EncounterVertexAgain(TVertex vertex, TEdge edge);

    /// <summary>
    /// Returns the vertex to be returned in the following call to the iterator <c>next</c>
    /// method.
    /// </summary>
    /// <returns>the next vertex to be returned by this iterator.</returns>
    protected abstract TVertex ProvideNextVertex();

    /// <summary>
    /// Determines whether a vertex has been seen yet by this traversal.
    /// </summary>
    /// <param name="vertex"> vertex in question.</param>>
    /// <returns><c>true</c> if vertex has already been seen.</returns>
    protected virtual bool IsSeenVertex(TVertex vertex)
    {
        return _seen.ContainsKey(vertex);
    }

    /// <summary>
    /// Access the data stored for a seen vertex.
    /// </summary>
    /// <param name="vertex"> a vertex which has already been seen.</param>
    /// <returns>data associated with the seen vertex or <c>null</c> if no data was associated
    ///         with the vertex. A <c>null</c> return can also indicate that the vertex was
    ///         explicitly associated with <c>
    /// null</c>.</returns>
    protected virtual TVertexData GetSeenData(TVertex vertex)
    {
        return _seen[vertex];
    }

    /// <summary>
    /// Stores iterator-dependent data for a vertex that has been seen.
    /// </summary>
    /// <param name="vertex"> a vertex which has been seen.</param>
    /// <param name="data"> data to be associated with the seen vertex.</param>
    /// <returns>previous value associated with specified vertex or <c>
    /// null</c> if no data was associated with the vertex. A <c>
    /// null</c> return can also indicate that the vertex was explicitly associated with
    ///         <c>null</c>.</returns>
    protected virtual TVertexData PutSeenData(TVertex vertex, TVertexData data)
    {
        return _seen[vertex] = data;
    }

    /// <summary>
    /// Called when a vertex has been finished (meaning is dependent on traversal represented by
    /// subclass).
    /// </summary>
    /// <param name="vertex"> vertex which has been finished.</param>
    protected virtual void FinishVertex(TVertex vertex)
    {
        if (NListeners != 0)
        {
            FireVertexFinished(CreateVertexTraversalEvent(vertex));
        }
    }

    /// <summary>
    /// Selects the outgoing edges for a given vertex based on the source vertex and other traversal
    /// state. The default implementation returns all outgoing edges.
    /// </summary>
    /// <param name="vertex"> vertex in question.</param>
    /// <returns>set of outgoing edges connected to the vertex.</returns>
    protected virtual ISet<TEdge> SelectOutgoingEdges(TVertex vertex)
    {
        return Graph.OutgoingEdgesOf(vertex);
    }

    private void AddUnseenChildrenOf(TVertex vertex)
    {
        foreach (var edge in SelectOutgoingEdges(vertex))
        {
            if (NListeners != 0)
            {
                FireEdgeTraversed(CreateEdgeTraversalEvent(edge));
            }

            var oppositeV = Graphs.GetOppositeVertex(Graph, edge, vertex);

            if (IsSeenVertex(oppositeV))
            {
                EncounterVertexAgain(oppositeV, edge);
            }
            else
            {
                EncounterVertex(oppositeV, edge);
            }
        }
    }

    [SuppressMessage("Usage", "MA0015:Specify the parameter name in ArgumentException")]
    private bool HasNext()
    {
        if (_startVertex != null)
        {
            EncounterStartVertex();
        }

        if (!ConnectedComponentExhausted)
        {
            return true;
        }

        if (_state == CcsWithinComponent)
        {
            _state = CcsAfterComponent;
            if (NListeners != 0)
            {
                FireConnectedComponentFinished(_ccFinishedEvent);
            }
        }

        var it = CrossComponentTraversal ? GetEntireGraphVertexIterator() : _startVertexIterator;
        while (it != null && it.MoveNext())
        {
            var v = it.Current;
            if (!Graph.ContainsVertex(v))
            {
                throw new ArgumentException("graph must contain the start vertex");
            }

            if (!IsSeenVertex(v))
            {
                EncounterVertex(v, edge: null);
                _state = CcsBeforeComponent;

                return true;
            }
        }

        return false;
    }

    private void EncounterStartVertex()
    {
        EncounterVertex(_startVertex!, edge: null);
        _startVertex = null;
    }
}
