using System.Diagnostics;

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
/// Provides a cross-connected-component traversal functionality for iterator subclasses.
/// </summary>
/// @param <TNode> vertex type.</param>
/// @param <TEdge> edge type.</param>
/// @param <D> type of data associated to seen vertices
///
/// <remarks>Author: Barak Naveh.</remarks>
public abstract class CrossComponentIterator<TNode, TEdge, TD> : AbstractGraphIterator<TNode, TEdge>
{
    private bool _instanceFieldsInitialized = false;

    private void InitializeInstanceFields()
    {
        _ccFinishedEvent =
            new ConnectedComponentTraversalEvent(this, ConnectedComponentTraversalEvent.ConnectedComponentFinished);
        _ccStartedEvent =
            new ConnectedComponentTraversalEvent(this, ConnectedComponentTraversalEvent.ConnectedComponentStarted);
    }

    private const int CcsBeforeComponent = 1;
    private const int CcsWithinComponent = 2;
    private const int CcsAfterComponent  = 3;

    private ConnectedComponentTraversalEvent _ccFinishedEvent;
    private ConnectedComponentTraversalEvent _ccStartedEvent;

    /// <summary>
    /// Stores the vertices that have been seen during iteration and (optionally) some additional
    /// traversal info regarding each vertex.
    /// </summary>
    private IDictionary<TNode, TD> _seen = new Dictionary<TNode, TD>();

    /// <summary>
    /// Iterator which provides start vertices for cross-component iteration.
    /// </summary>
    private IEnumerator<TNode> _entireGraphVertexIterator = null;

    /// <summary>
    /// Iterator which provides start vertices for specified start vertices.
    /// </summary>
    private IEnumerator<TNode> _startVertexIterator = null;

    /// <summary>
    /// The current vertex.
    /// </summary>
    private TNode _startVertex;

    /// <summary>
    /// The connected component state
    /// </summary>
    private int _state = CcsBeforeComponent;

    /// <summary>
    /// Creates a new iterator for the specified graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    public CrossComponentIterator(IGraph<TNode, TEdge> g)
        : this(g, (TNode)null)
    {
        if (!_instanceFieldsInitialized)
        {
            InitializeInstanceFields();
            _instanceFieldsInitialized = true;
        }
    }

    /// <summary>
    /// Creates a new iterator for the specified graph. Iteration will start at the specified start
    /// vertex. If the specified start vertex is <c>
    /// null</c>, Iteration will start at an arbitrary graph vertex.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertex"> the vertex iteration to be started.</param>
    /// <exception cref="ArgumentException"> if <c>g==null</c> or does not contain
    ///         <c>startVertex</c> </exception>
    public CrossComponentIterator(IGraph<TNode, TEdge> g, TNode startVertex)
        : this(g, startVertex == null ? null : Collections.singletonList(startVertex))
    {
        if (!_instanceFieldsInitialized)
        {
            InitializeInstanceFields();
            _instanceFieldsInitialized = true;
        }
    }

    /// <summary>
    /// Creates a new iterator for the specified graph. Iteration will start at the specified start
    /// vertices. If the specified start vertices is <c>
    /// null</c>, Iteration will start at an arbitrary graph vertex.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertices"> the vertices iteration to be started.</param>
    /// <exception cref="ArgumentException"> if <c>g==null</c> or does not contain
    ///         <c>startVertex</c> </exception>
    public CrossComponentIterator(IGraph<TNode, TEdge> g, IEnumerable<TNode> startVertices)
        : base(g)
    {
        if (!_instanceFieldsInitialized)
        {
            InitializeInstanceFields();
            _instanceFieldsInitialized = true;
        }

        /*
         * Initialize crossComponentTraversal and test for containment
         */
        if (startVertices == null)
        {
            this.crossComponentTraversal = true;
        }
        else
        {
            this.crossComponentTraversal = false;
            _startVertexIterator    = startVertices.GetEnumerator();
        }

        /*
         * Initialize start vertex
         */
        IEnumerator<TNode> it = crossComponentTraversal ? EntireGraphVertexIterator : _startVertexIterator;
        // pick a start vertex if possible
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
        if (it.hasNext())
        {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            _startVertex = it.next();
            if (!graph.ContainsVertex(_startVertex))
            {
                throw new ArgumentException("graph must contain the start vertex");
            }
        }
        else
        {
            _startVertex = default(TNode);
        }
    }

    public override bool HasNext()
    {
        if (_startVertex != null)
        {
            EncounterStartVertex();
        }

        if (ConnectedComponentExhausted)
        {
            if (_state == CcsWithinComponent)
            {
                _state = CcsAfterComponent;
                if (NListeners != 0)
                {
                    FireConnectedComponentFinished(_ccFinishedEvent);
                }
            }

            IEnumerator<TNode> it = CrossComponentTraversal ? EntireGraphVertexIterator : _startVertexIterator;
            while (it != null && it.MoveNext())
            {
                var node = it.Current;
                if (!graph.ContainsVertex(node))
                {
                    throw new ArgumentException("graph must contain the start vertex");
                }

                if (!IsSeenVertex(node))
                {
                    EncounterVertex(node, default(TEdge));
                    _state = CcsBeforeComponent;

                    return true;
                }
            }

            return false;
        }
        else
        {
            return true;
        }
    }

    public override TNode Next()
    {
        if (_startVertex != null)
        {
            EncounterStartVertex();
        }

        if (HasNext())
        {
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

            return nextVertex;
        }
        else
        {
            throw new NoSuchElementException();
        }
    }

    /// <summary>
    /// Lazily instantiates {@code entireGraphVertexIterator}.
    /// </summary>
    /// <returns>iterator which provides start vertices for cross-component iteration.</returns>
    protected internal virtual IEnumerator<TNode> EntireGraphVertexIterator
    {
        get
        {
            if (_entireGraphVertexIterator == null)
            {
                Debug.Assert((CrossComponentTraversal));
                _entireGraphVertexIterator = graph.VertexSet().GetEnumerator();
            }

            return _entireGraphVertexIterator;
        }
    }

    /// <summary>
    /// Returns <c>true</c> if there are no more uniterated vertices in the currently iterated
    /// connected component; <c>false</c> otherwise.
    /// </summary>
    /// <returns><c>true</c> if there are no more uniterated vertices in the currently iterated
    ///         connected component; <c>false</c> otherwise.</returns>
    protected internal abstract bool ConnectedComponentExhausted { get; }

    /// <summary>
    /// Update data structures the first time we see a vertex.
    /// </summary>
    /// <param name="vertex"> the vertex encountered.</param>
    /// <param name="edge"> the edge via which the vertex was encountered, or null if the vertex is a
    ///        starting point.</param>
    protected internal abstract void EncounterVertex(TNode vertex, TEdge edge);

    /// <summary>
    /// Returns the vertex to be returned in the following call to the iterator <c>next</c>
    /// method.
    /// </summary>
    /// <returns>the next vertex to be returned by this iterator.</returns>
    protected internal abstract TNode ProvideNextVertex();

    /// <summary>
    /// Access the data stored for a seen vertex.
    /// </summary>
    /// <param name="vertex"> a vertex which has already been seen.</param>
    /// <returns>data associated with the seen vertex or <c>null</c> if no data was associated
    ///         with the vertex. A <c>null</c> return can also indicate that the vertex was
    ///         explicitly associated with <c>
    /// null</c>.</returns>
    protected internal virtual TD GetSeenData(TNode vertex)
    {
        return _seen[vertex];
    }

    /// <summary>
    /// Determines whether a vertex has been seen yet by this traversal.
    /// </summary>
    /// <param name="vertex"> vertex in question
    /// </param>
    /// <returns><c>true</c> if vertex has already been seen.</returns>
    protected internal virtual bool IsSeenVertex(TNode vertex)
    {
        return _seen.ContainsKey(vertex);
    }

    /// <summary>
    /// Called whenever we re-encounter a vertex. The default implementation does nothing.
    /// </summary>
    /// <param name="vertex"> the vertex re-encountered.</param>
    /// <param name="edge"> the edge via which the vertex was re-encountered.</param>
    protected internal abstract void EncounterVertexAgain(TNode vertex, TEdge edge);

    /// <summary>
    /// Stores iterator-dependent data for a vertex that has been seen.
    /// </summary>
    /// <param name="vertex"> a vertex which has been seen.</param>
    /// <param name="data"> data to be associated with the seen vertex.</param>
    /// <returns>previous value associated with specified vertex or <c>
    /// null</c> if no data was associated with the vertex. A <c>
    /// null</c> return can also indicate that the vertex was explicitly associated with
    ///         <c>null</c>.</returns>
    protected internal virtual TD PutSeenData(TNode vertex, TD data)
    {
        return _seen[vertex] = data;
    }

    /// <summary>
    /// Called when a vertex has been finished (meaning is dependent on traversal represented by
    /// subclass).
    /// </summary>
    /// <param name="vertex"> vertex which has been finished.</param>
    protected internal virtual void FinishVertex(TNode vertex)
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
    protected internal virtual ISet<TEdge> SelectOutgoingEdges(TNode vertex)
    {
        return graph.OutgoingEdgesOf(vertex);
    }

    private void AddUnseenChildrenOf(TNode vertex)
    {
        foreach (var edge in SelectOutgoingEdges(vertex))
        {
            if (NListeners != 0)
            {
                FireEdgeTraversed(CreateEdgeTraversalEvent(edge));
            }

            var oppositeV = Graphs.GetOppositeVertex(graph, edge, vertex);

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

    private void EncounterStartVertex()
    {
        EncounterVertex(_startVertex, default(TEdge));
        _startVertex = default(TNode);
    }
}
