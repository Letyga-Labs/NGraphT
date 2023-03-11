/*
 * (C) Copyright 2003-2021, by John TNode Sichi and Contributors.
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
/// A closest-first iterator for a directed or undirected graph. For this iterator to work correctly
/// the graph must not be modified during iteration. Currently there are no means to ensure that, nor
/// to fail-fast. The results of such modifications are undefined.
///
/// <para>
/// The metric for <i>closest</i> here is the weighted path length from a start vertex, i.TEdge.
/// Graph.getEdgeWeight(Edge) is summed to calculate path length. Negative edge weights will result
/// in an ArgumentException. Optionally, path length may be bounded by a finite radius. A
/// custom heap implementation can be specified during the construction time. Pairing heap is used by
/// default.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
/// <remarks>Author: John TNode. Sichi.</remarks>
public class ClosestFirstIterator<TNode, TEdge> : CrossComponentIterator<TNode, TEdge,
    AddressableHeap.Handle<double, ClosestFirstIterator.QueueEntry<TNode, TEdge>>>
{
    /// <summary>
    /// Priority queue of fringe vertices.
    /// </summary>
    private AddressableHeap<double, QueueEntry<TNode, TEdge>> _heap;

    /// <summary>
    /// Maximum distance to search.
    /// </summary>
    private double _radius = double.PositiveInfinity;

    private bool _initialized = false;

    /// <summary>
    /// Creates a new closest-first iterator for the specified graph. Iteration will start at the
    /// specified start vertex and will be limited to the connected component that includes that
    /// vertex. If the specified start vertex is <c>null</c>, iteration will start at an
    /// arbitrary vertex and will not be limited, that is, will be able to traverse all the graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertex"> the vertex iteration to be started.</param>
    public ClosestFirstIterator(IGraph<TNode, TEdge> g, TNode startVertex)
        : this(g, startVertex, double.PositiveInfinity)
    {
    }

    /// <summary>
    /// Creates a new closest-first iterator for the specified graph. Iteration will start at the
    /// specified start vertices and will be limited to the subset of the graph reachable from those
    /// vertices. Iteration order is based on minimum distance from any of the start vertices,
    /// regardless of the order in which the start vertices are supplied. Because of this, the entire
    /// traversal is treated as if it were over a single connected component with respect to events
    /// fired.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertices"> the vertices iteration to be started.</param>
    public ClosestFirstIterator(IGraph<TNode, TEdge> g, IEnumerable<TNode> startVertices)
        : this(g, startVertices, double.PositiveInfinity)
    {
    }

    /// <summary>
    /// Creates a new radius-bounded closest-first iterator for the specified graph. Iteration will
    /// start at the specified start vertex and will be limited to the subset of the connected
    /// component which includes that vertex and is reachable via paths of weighted length less than
    /// or equal to the specified radius. The specified start vertex may not be <c>
    /// null</c>.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertex"> the vertex iteration to be started.</param>
    /// <param name="radius"> limit on weighted path length, or Double.POSITIVE_INFINITY for unbounded
    ///        search.</param>
    public ClosestFirstIterator(IGraph<TNode, TEdge> g, TNode startVertex, double radius)
        : this(g, startVertex == null ? null : Collections.singletonList(startVertex), radius, PairingHeap::new)
    {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
    }

    /// <summary>
    /// Creates a new radius-bounded closest-first iterator for the specified graph. Iteration will
    /// start at the specified start vertex and will be limited to the subset of the connected
    /// component which includes that vertex and is reachable via paths of weighted length less than
    /// or equal to the specified radius. The specified start vertex may not be <c>
    /// null</c>. This algorithm will use the heap supplied by the <c>heapSupplier</c>.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertex"> the vertex iteration to be started.</param>
    /// <param name="radius"> limit on weighted path length, or Double.POSITIVE_INFINITY for unbounded
    ///        search.</param>
    /// <param name="heapSupplier"> supplier of the preferable heap implementation.</param>
    public ClosestFirstIterator(
        IGraph<TNode, TEdge>                                           g,
        TNode                                                          startVertex,
        double                                                         radius,
        Func<AddressableHeap<double, QueueEntry<TNode, TEdge>>> heapSupplier
    )
        : this(g, startVertex == null ? null : Collections.singletonList(startVertex), radius, heapSupplier)
    {
    }

    /// <summary>
    /// Creates a new radius-bounded closest-first iterator for the specified graph. Iteration will
    /// start at the specified start vertices and will be limited to the subset of the graph
    /// reachable from those vertices via paths of weighted length less than or equal to the
    /// specified radius. The specified collection of start vertices may not be <c>null</c>.
    /// Iteration order is based on minimum distance from any of the start vertices, regardless of
    /// the order in which the start vertices are supplied. Because of this, the entire traversal is
    /// treated as if it were over a single connected component with respect to events fired.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertices"> the vertices iteration to be started.</param>
    /// <param name="radius"> limit on weighted path length, or Double.POSITIVE_INFINITY for unbounded
    ///        search.</param>
    public ClosestFirstIterator(IGraph<TNode, TEdge> g, IEnumerable<TNode> startVertices, double radius)
        : this(g, startVertices, radius, PairingHeap::new)
    {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
    }

    /// <summary>
    /// Creates a new radius-bounded closest-first iterator for the specified graph. Iteration will
    /// start at the specified start vertices and will be limited to the subset of the graph
    /// reachable from those vertices via paths of weighted length less than or equal to the
    /// specified radius. The specified collection of start vertices may not be <c>null</c>.
    /// Iteration order is based on minimum distance from any of the start vertices, regardless of
    /// the order in which the start vertices are supplied. Because of this, the entire traversal is
    /// treated as if it were over a single connected component with respect to events fired. This
    /// algorithm will use the heap supplied by the <c>heapSupplier</c>.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertices"> the vertices iteration to be started.</param>
    /// <param name="radius"> limit on weighted path length, or Double.POSITIVE_INFINITY for unbounded
    ///        search.</param>
    /// <param name="heapSupplier"> supplier of the preferable heap implementation.</param>
    public ClosestFirstIterator(
        IGraph<TNode, TEdge>                                           g,
        IEnumerable<TNode>                                             startVertices,
        double                                                         radius,
        Func<AddressableHeap<double, QueueEntry<TNode, TEdge>>> heapSupplier
    )
        : base(g, startVertices)
    {
        _radius = radius;
        Objects.requireNonNull(heapSupplier, "Heap supplier cannot be null");
        _heap = heapSupplier();
        CheckRadiusTraversal(isCrossComponentTraversal());
        _initialized = true;
        if (!crossComponentTraversal)
        {
            // prime the heap by processing the first start vertex
            HasNext();
            var iter = startVertices.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            if (iter.hasNext())
            {
                // discard the first since we already primed it above
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                iter.next();
                // prime the heap with the rest of the start vertices so that
                // we can process them all simultaneously
                while (iter.MoveNext())
                {
                    var node = iter.Current;
                    EncounterVertex(node, default(TEdge));
                }
            }
        }
    }

    // override AbstractGraphIterator
    public override bool CrossComponentTraversal
    {
        set
        {
            if (_initialized)
            {
                CheckRadiusTraversal(value);
            }

            base.setCrossComponentTraversal(value);
        }
    }

    /// <summary>
    /// Get the weighted length of the shortest path known to the given vertex. If the vertex has
    /// already been visited, then it is truly the shortest path length; otherwise, it is the best
    /// known upper bound.
    /// </summary>
    /// <param name="vertex"> vertex being sought from start vertex.</param>
    /// <returns>weighted length of shortest path known, or Double.POSITIVE_INFINITY if no path found
    ///         yet.</returns>
    public virtual double GetShortestPathLength(TNode vertex)
    {
        AddressableHeap.Handle<double, QueueEntry<TNode, TEdge>> node = GetSeenData(vertex);

        if (node == null)
        {
            return double.PositiveInfinity;
        }

        return node.getKey();
    }

    /// <summary>
    /// Get the spanning tree edge reaching a vertex which has been seen already in this traversal.
    /// This edge is the last link in the shortest known path between the start vertex and the
    /// requested vertex. If the vertex has already been visited, then it is truly the minimum
    /// spanning tree edge; otherwise, it is the best candidate seen so far.
    /// </summary>
    /// <param name="vertex"> the spanned vertex.</param>
    /// <returns>the spanning tree edge, or null if the vertex either has not been seen yet or is a
    ///         start vertex.</returns>
    public virtual TEdge GetSpanningTreeEdge(TNode vertex)
    {
        AddressableHeap.Handle<double, QueueEntry<TNode, TEdge>> node = GetSeenData(vertex);

        if (node == null)
        {
            return default(TEdge);
        }

        return node.getValue().spanningTreeEdge;
    }

    /// <seealso cref="CrossComponentIterator.isConnectedComponentExhausted()"/>
    protected internal override bool ConnectedComponentExhausted
    {
        get
        {
            if (_heap.size() == 0)
            {
                return true;
            }
            else
            {
                if (_heap.findMin().getKey() > _radius)
                {
                    _heap.clear();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    /// <seealso cref="CrossComponentIterator.encounterVertex(Object, Object)"/>
    protected internal override void EncounterVertex(TNode vertex, TEdge edge)
    {
        double shortestPathLength;
        if (edge == null)
        {
            shortestPathLength = 0;
        }
        else
        {
            shortestPathLength = CalculatePathLength(vertex, edge);
        }

        AddressableHeap.Handle<double, QueueEntry<TNode, TEdge>> handle = _heap.insert(shortestPathLength,
            new QueueEntry<double, QueueEntry<TNode, TEdge>>(vertex, edge)
        );
        PutSeenData(vertex, handle);
    }

    /// <summary>
    /// Override superclass. When we see a vertex again, we need to see if the new edge provides a
    /// shorter path than the old edge.
    /// </summary>
    /// <param name="vertex"> the vertex re-encountered.</param>
    /// <param name="edge"> the edge via which the vertex was re-encountered.</param>
    protected internal override void EncounterVertexAgain(TNode vertex, TEdge edge)
    {
        AddressableHeap.Handle<double, QueueEntry<TNode, TEdge>> node = GetSeenData(vertex);

        if (node.getValue().frozen)
        {
            // no improvement for this vertex possible
            return;
        }

        var candidatePathLength = CalculatePathLength(vertex, edge);

        if (candidatePathLength < node.getKey())
        {
            node.getValue().spanningTreeEdge = edge;
            node.decreaseKey(candidatePathLength);
        }
    }

    /// <seealso cref="CrossComponentIterator.provideNextVertex()"/>
    protected internal override TNode ProvideNextVertex()
    {
        AddressableHeap.Handle<double, QueueEntry<TNode, TEdge>> node = _heap.deleteMin();
        node.getValue().frozen = true;

        return node.getValue().vertex;
    }

    private void AssertNonNegativeEdge(TEdge edge)
    {
        if (getGraph().getEdgeWeight(edge) < 0)
        {
            throw new ArgumentException("negative edge weights not allowed");
        }
    }

    /// <summary>
    /// Determine weighted path length to a vertex via an edge, using the path length for the
    /// opposite vertex.
    /// </summary>
    /// <param name="vertex"> the vertex for which to calculate the path length.</param>
    /// <param name="edge"> the edge via which the path is being extended.</param>
    /// <returns>calculated path length.</returns>
    private double CalculatePathLength(TNode vertex, TEdge edge)
    {
        AssertNonNegativeEdge(edge);

        var otherVertex = Graphs.GetOppositeVertex(getGraph(), edge, vertex);
        AddressableHeap.Handle<double, QueueEntry<TNode, TEdge>> otherEntry = GetSeenData(otherVertex);

        return otherEntry.getKey() + getGraph().getEdgeWeight(edge);
    }

    private void CheckRadiusTraversal(bool crossComponentTraversal)
    {
        if (crossComponentTraversal && (_radius != double.PositiveInfinity))
        {
            throw new ArgumentException("radius may not be specified for cross-component traversal");
        }
    }

    /// <summary>
    /// Private data to associate with each entry in the priority queue.
    /// </summary>
    internal class QueueEntry<TNode, TEdge>
    {
        /// <summary>
        /// The vertex reached.
        /// </summary>
        internal TNode Vertex;

        /// <summary>
        /// Best spanning tree edge to vertex seen so far.
        /// </summary>
        internal TEdge SpanningTreeEdge;

        /// <summary>
        /// True once spanningTreeEdge is guaranteed to be the true minimum.
        /// </summary>
        internal bool Frozen;

        internal QueueEntry(TNode vertex, TEdge spanningTreeEdge)
        {
            Vertex           = vertex;
            SpanningTreeEdge = spanningTreeEdge;
        }
    }
}
