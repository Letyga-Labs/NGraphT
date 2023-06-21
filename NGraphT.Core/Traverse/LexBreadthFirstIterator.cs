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
using NGraphT.Core.Util;
using Java2Net = J2N.Collections.Generic;

namespace NGraphT.Core.Traverse;

/// <summary>
/// A lexicographical breadth-first iterator for an undirected graph.
///
/// <para>
/// Every vertex has an implicit label (they aren't used explicitly in order to reduce time and
/// memory complexity). When some vertex is returned by this iterator, its index is the number of
/// vertices in this graph minus number of already returned vertices. For a given vertex TVertex its label
/// is a concatenation of indices of already returned vertices, that were also its neighbours, with
/// some separator between them. For example, 7#4#3 is a valid vertex label.
/// </para>
///
/// <para>
/// Iterator chooses vertex with lexicographically largest label and returns it. It breaks ties
/// arbitrarily. For more information on lexicographical BFS see the following article: Corneil D.G.
/// (2004) <a href="https://pdfs.semanticscholar.org/d4b5/a492f781f23a30773841ec79c46d2ec2eb9c.pdf">
/// <i>Lexicographic Breadth First Search – A Survey</i></a>. In: Hromkovič J., Nagl M., Westfechtel
/// B. (eds) Graph-Theoretic Concepts in Computer Science. WG 2004. Lecture Notes in Computer
/// Science, vol 3353. Springer, Berlin, Heidelberg; and the following
/// paper:<a href="http://www.cse.iitd.ac.in/~naveen/courses/CSL851/uwaterloo.pdf"><i>CS 762:
/// Graph-theoretic algorithms. Lecture notes of a graduate course. University of Waterloo</i></a>.
/// </para>
///
/// <para>
/// For this iterator to work correctly the graph must not be modified during iteration. Currently
/// there are no means to ensure that, nor to fail-fast. The results of such modifications are
/// undefined.
/// </para>
///
/// <para>
/// Note: only vertex events are fired by this iterator.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex"> the graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>.
///
/// <remarks>Author: Timofey Chudakov.</remarks>
public sealed class LexBreadthFirstIterator<TVertex, TEdge> : AbstractGraphIterator<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Reference to the <c>BucketList</c> that contains unvisited vertices.
    /// </summary>
    private readonly BucketList _bucketList;

    /// <summary>
    /// Contains current vertex of the <c>graph</c>.
    /// </summary>
    private TVertex? _current;

    /// <summary>
    /// Creates new lexicographical breadth-first iterator for <c>graph</c>.
    /// </summary>
    /// <param name="graph"> the graph to be iterated.</param>
    public LexBreadthFirstIterator(IGraph<TVertex, TEdge> graph)
        : base(graph)
    {
        // TODO: GraphTests.RequireUndirected(graph);
        _bucketList = new BucketList(graph.VertexSet());
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para>
    /// Always returns true since this iterator doesn't care about connected components.
    /// </para>
    /// </summary>
    public override bool CrossComponentTraversal
    {
        get => true;
        set
        {
            if (!value)
            {
                throw new ArgumentException("Iterator is always cross-component", nameof(value));
            }
        }
    }

    /// <summary>
    /// Returns the current vertex in the ordering.
    /// </summary>
    /// <returns>the current vertex in the ordering.</returns>
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override TVertex Current => _current ?? throw new NoSuchElementException();

    /// <summary>
    /// Checks whether there exist unvisited vertices and moves to the next one.
    /// </summary>
    /// <returns>true if there exist unvisited vertices.</returns>
    public override bool MoveNext()
    {
        var previous = _current;
        if (previous != null && NListeners != 0)
        {
            FireVertexFinished(CreateVertexTraversalEvent(previous));
        }

        _current = Advance();
        if (_current != null && NListeners != 0)
        {
            FireVertexTraversed(CreateVertexTraversalEvent(_current));
        }

        return _current != null;
    }

    /// <summary>
    /// Retrieves vertex from the <c>bucketList</c> and returns it.
    /// </summary>
    /// <returns>the vertex retrieved from the <c>bucketList</c>.</returns>
    private TVertex? Advance()
    {
        var vertex = _bucketList.Poll();
        if (vertex != null)
        {
            _bucketList.UpdateBuckets(GetUnvisitedNeighbours(vertex));
        }

        return vertex;
    }

    /// <summary>
    /// Computes and returns neighbours of <c>vertex</c> which haven't been visited by this
    /// iterator.
    /// </summary>
    /// <param name="vertex"> the vertex, whose neighbours are being explored.</param>
    /// <returns>neighbours of <c>vertex</c> which have yet to be visited by this iterator.</returns>
    private ISet<TVertex> GetUnvisitedNeighbours(TVertex vertex)
    {
        var unmapped = new Java2Net.LinkedHashSet<TVertex>();
        var edges    = Graph.EdgesOf(vertex);
        foreach (var edge in edges)
        {
            var oppositeVertex = Graphs.GetOppositeVertex(Graph, edge, vertex);
            if (_bucketList.ContainsBucketWith(oppositeVertex))
            {
                unmapped.Add(oppositeVertex);
            }
        }

        return unmapped;
    }

    /// <summary>
    /// Data structure for performing lexicographical breadth-first search. Allows to add and
    /// retrieve vertices from buckets, update their buckets after a new vertex has been added to the
    /// LexBFS order. Labels aren't used explicitly, which results in time and space optimization.
    ///
    /// <remarks>Author: Timofey Chudakov.</remarks>
    /// </summary>
    private sealed class BucketList
    {
        /// <summary>
        /// Map for mapping vertices to buckets they are currently in. Is used for finding the bucket
        /// of the vertex in constant time.
        /// </summary>
        private readonly IDictionary<TVertex, Bucket> _bucketMap;

        /// <summary>
        /// Bucket with the vertices that have lexicographically largest label.
        /// </summary>
        private Bucket? _head;

        /// <summary>
        /// Creates a <c>BucketList</c> with a single bucket and all specified <c>vertices</c> in
        /// it.
        /// </summary>
        /// <param name="vertices"> the vertices of the graph, that should be stored in the <c>head</c>
        ///        bucket.</param>
        internal BucketList(ICollection<TVertex> vertices)
        {
            _head      = new Bucket(vertices);
            _bucketMap = CollectionUtil.NewHashMapWithExpectedSize<TVertex, Bucket>(vertices.Count);
            foreach (var vertex in vertices)
            {
                _bucketMap[vertex] = _head;
            }
        }

        /// <summary>
        /// Checks whether there exists a bucket with the specified <c>vertex</c>.
        /// </summary>
        /// <param name="vertex"> the vertex whose presence in some <c>Bucket</c> in this <c>BucketList</c>
        ///        is checked.</param>
        /// <returns><c>true</c> if there exists a bucket with <c>vertex</c> in it, otherwise
        ///         <c>false</c>.</returns>
        internal bool ContainsBucketWith(TVertex vertex)
        {
            return _bucketMap.ContainsKey(vertex);
        }

        /// <summary>
        /// Retrieves element from the head bucket by invoking <see cref="Bucket.Poll()"/> or null if this
        /// <c>BucketList</c> is empty.
        /// <para>
        /// Removes the head bucket if it becomes empty after the operation.
        ///
        /// </para>
        /// </summary>
        /// <returns>vertex returned by <see cref="Bucket.Poll()"/> invoked on head bucket or null if this
        ///         <c>BucketList</c> is empty.</returns>
        internal TVertex? Poll()
        {
            if (_bucketMap.Count > 0)
            {
                var res = _head!.Poll();
                _bucketMap.Remove(res);
                if (_head.Empty)
                {
                    _head = _head.Next;
                    if (_head != null)
                    {
                        _head.Prev = null;
                    }
                }

                return res;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// For every bucket B in this <c>BucketList</c>, which contains vertices from the set
        /// <c>vertices</c>, creates a new <c>Bucket</c> B' and moves vertices from B to B' according to
        /// the following rule: $B' = B\cap vertices$ and $B = B\backslash B'$. For every such
        /// <c>Bucket</c> B only one <c>Bucket</c> B' is created. If some bucket B becomes empty
        /// after this operation, it is removed from the data structure.
        /// </summary>
        /// <param name="vertices"> the vertices, that should be moved to new buckets.</param>
        internal void UpdateBuckets(ISet<TVertex> vertices)
        {
            var visitedBuckets = new HashSet<Bucket>();
            foreach (var vertex in vertices)
            {
                var bucket = _bucketMap[vertex];
                if (visitedBuckets.Contains(bucket))
                {
                    bucket.Prev!.AddVertex(vertex);
                    _bucketMap[vertex] = bucket.Prev;
                }
                else
                {
                    visitedBuckets.Add(bucket);
                    var newBucket = new Bucket(vertex);
                    newBucket.InsertBefore(bucket);
                    _bucketMap[vertex] = newBucket;
                    if (_head == bucket)
                    {
                        _head = newBucket;
                    }
                }

                bucket.RemoveVertex(vertex);
                if (bucket.Empty)
                {
                    visitedBuckets.Remove(bucket);
                    bucket.RemoveSelf();
                }
            }
        }

        /// <summary>
        /// Plays the role of the container of vertices. All vertices stored in a bucket have
        /// identical label. Labels aren't used explicitly.
        /// <para>
        /// Encapsulates operations of addition and removal of vertices from the bucket and removal
        /// of a bucket from the data structure.
        /// </para>
        /// </summary>
        private sealed class Bucket
        {
            /// <summary>
            /// Set of vertices currently stored in this bucket.
            /// </summary>
            private readonly ISet<TVertex> _vertices;

            /// <summary>
            /// Creates a new bucket with all <c>vertices</c> stored in it.
            /// </summary>
            /// <param name="vertices"> vertices to store in this bucket.</param>
            internal Bucket(ICollection<TVertex> vertices)
            {
                _vertices = new Java2Net.LinkedHashSet<TVertex>(vertices);
            }

            /// <summary>
            /// Creates a new Bucket with a single <c>vertex</c> in it.
            /// </summary>
            /// <param name="vertex"> the vertex to store in this bucket.</param>
            internal Bucket(TVertex vertex)
            {
                _vertices = new Java2Net.LinkedHashSet<TVertex>
                {
                    vertex,
                };
            }

            /// <summary>
            /// Reference of the bucket with lexicographically smaller label.
            /// </summary>
            internal Bucket? Next { get; private set; }

            /// <summary>
            /// Reference of the bucket with lexicographically larger label.
            /// </summary>
            internal Bucket? Prev { get; set; }

            /// <summary>
            /// Checks whether this bucket is empty.
            /// </summary>
            /// <returns><c>true</c> if this bucket doesn't contain any elements, otherwise false.</returns>
            internal bool Empty => _vertices.Count == 0;

            /// <summary>
            /// Removes the <c>vertex</c> from this bucket.
            /// </summary>
            /// <param name="vertex"> the vertex to remove.</param>
            internal void RemoveVertex(TVertex vertex)
            {
                _vertices.Remove(vertex);
            }

            /// <summary>
            /// Removes this bucket from the data structure.
            /// </summary>
            internal void RemoveSelf()
            {
                if (Next != null)
                {
                    Next.Prev = Prev;
                }

                if (Prev != null)
                {
                    Prev.Next = Next;
                }
            }

            /// <summary>
            /// Inserts this bucket in the data structure before the <c>bucket</c>.
            /// </summary>
            /// <param name="bucket"> the bucket, that will be the next to this bucket.</param>
            internal void InsertBefore(Bucket? bucket)
            {
                Next = bucket;
                if (bucket != null)
                {
                    Prev = bucket.Prev;
                    if (bucket.Prev != null)
                    {
                        bucket.Prev.Next = this;
                    }

                    bucket.Prev = this;
                }
                else
                {
                    Prev = null;
                }
            }

            /// <summary>
            /// Adds the <c>vertex</c> to this bucket.
            /// </summary>
            /// <param name="vertex"> the vertex to add.</param>
            internal void AddVertex(TVertex vertex)
            {
                _vertices.Add(vertex);
            }

            /// <summary>
            /// Retrieves one vertex from this bucket.
            /// </summary>
            /// <returns>vertex, that was removed from this bucket, null if the bucket was empty.</returns>
            internal TVertex Poll()
            {
                if (_vertices.Count == 0)
                {
                    return default!;
                }
                else
                {
                    var vertex = _vertices.First();
                    _vertices.Remove(vertex);
                    return vertex;
                }
            }
        }
    }
}
