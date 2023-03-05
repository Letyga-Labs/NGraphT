/*
 * (C) Copyright 2018-2021, by Timofey Chudakov and Contributors.
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
using GraphTests = GraphTests;
using Graphs = Graphs;
using CollectionUtil = Util.CollectionUtil;

/// <summary>
/// A lexicographical breadth-first iterator for an undirected graph.
/// <para>
/// Every vertex has an implicit label (they aren't used explicitly in order to reduce time and
/// memory complexity). When some vertex is returned by this iterator, its index is the number of
/// vertices in this graph minus number of already returned vertices. For a given vertex TNode its label
/// is a concatenation of indices of already returned vertices, that were also its neighbours, with
/// some separator between them. For example, 7#4#3 is a valid vertex label.
/// </para>
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
/// <para>
/// For this iterator to work correctly the graph must not be modified during iteration. Currently
/// there are no means to ensure that, nor to fail-fast. The results of such modifications are
/// undefined.
/// </para>
/// <para>
/// Note: only vertex events are fired by this iterator.
/// </para>
/// </summary>
/// <typeparam name="TNode"> the graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>.
/// <remarks>Author: Timofey Chudakov.</remarks>
public class LexBreadthFirstIterator<TNode, TEdge> : AbstractGraphIterator<TNode, TEdge>
{
    /// <summary>
    /// Reference to the {@code BucketList} that contains unvisited vertices.
    /// </summary>
    private BucketList _bucketList;

    /// <summary>
    /// Contains current vertex of the {@code graph}.
    /// </summary>
    private TNode _current;

    /// <summary>
    /// Creates new lexicographical breadth-first iterator for {@code graph}.
    /// </summary>
    /// <param name="graph"> the graph to be iterated.</param>
    public LexBreadthFirstIterator(IGraph<TNode, TEdge> graph)
        : base(graph)
    {
        GraphTests.RequireUndirected(graph);
        _bucketList = new BucketList(this, graph.VertexSet());
    }

    /// <summary>
    /// Checks whether there exist unvisited vertices.
    /// </summary>
    /// <returns>true if there exist unvisited vertices.</returns>
    public override bool HasNext()
    {
        if (_current != null)
        {
            return true;
        }

        _current = Advance();
        if (_current != null && NListeners != 0)
        {
            FireVertexTraversed(CreateVertexTraversalEvent(_current));
        }

        return _current != null;
    }

    /// <summary>
    /// Returns the next vertex in the ordering.
    /// </summary>
    /// <returns>the next vertex in the ordering.</returns>
    public override TNode Next()
    {
        if (!HasNext())
        {
            throw new NoSuchElementException();
        }

        var result = _current;
        _current = default(TNode);
        if (NListeners != 0)
        {
            FireVertexFinished(CreateVertexTraversalEvent(result));
        }

        return result;
    }

    /// <summary>
    /// {@inheritDoc}
    /// <para>
    /// Always returns true since this iterator doesn't care about connected components.
    /// </para>
    /// </summary>
    public override bool CrossComponentTraversal
    {
        get
        {
            return true;
        }
        set
        {
            if (!value)
            {
                throw new ArgumentException("Iterator is always cross-component");
            }
        }
    }


    /// <summary>
    /// Retrieves vertex from the {@code bucketList} and returns it.
    /// </summary>
    /// <returns>the vertex retrieved from the {@code bucketList}.</returns>
    private TNode Advance()
    {
        var vertex = _bucketList.Poll();
        if (vertex != null)
        {
            _bucketList.UpdateBuckets(GetUnvisitedNeighbours(vertex));
        }

        return vertex;
    }

    /// <summary>
    /// Computes and returns neighbours of {@code vertex} which haven't been visited by this
    /// iterator.
    /// </summary>
    /// <param name="vertex"> the vertex, whose neighbours are being explored.</param>
    /// <returns>neighbours of {@code vertex} which have yet to be visited by this iterator.</returns>
    private ISet<TNode> GetUnvisitedNeighbours(TNode vertex)
    {
        ISet<TNode> unmapped = new LinkedHashSet<TNode>();
        ISet<TEdge> edges    = graph.EdgesOf(vertex);
        foreach (var edge in edges)
        {
            var oppositeVertex = Graphs.GetOppositeVertex(graph, edge, vertex);
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
    internal class BucketList
    {
        private readonly LexBreadthFirstIterator<TNode, TEdge> _outerInstance;

        /// <summary>
        /// Bucket with the vertices that have lexicographically largest label.
        /// </summary>
        internal Bucket Head;

        /// <summary>
        /// Map for mapping vertices to buckets they are currently in. Is used for finding the bucket
        /// of the vertex in constant time.
        /// </summary>
        internal IDictionary<TNode, Bucket> BucketMap;

        /// <summary>
        /// Creates a {@code BucketList} with a single bucket and all specified {@code vertices} in
        /// it.
        /// </summary>
        /// <param name="vertices"> the vertices of the graph, that should be stored in the {@code head}
        ///        bucket.</param>
        internal BucketList(LexBreadthFirstIterator<TNode, TEdge> outerInstance, ICollection<TNode> vertices)
        {
            _outerInstance = outerInstance;
            Head                = new Bucket(this, vertices);
            BucketMap           = CollectionUtil.NewHashMapWithExpectedSize(vertices.Count);
            foreach (var vertex in vertices)
            {
                BucketMap[vertex] = Head;
            }
        }

        /// <summary>
        /// Checks whether there exists a bucket with the specified {@code vertex}.
        /// </summary>
        /// <param name="vertex"> the vertex whose presence in some {@code Bucket} in this {@code BucketList}
        ///        is checked.</param>
        /// <returns><tt>true</tt> if there exists a bucket with {@code vertex} in it, otherwise
        ///         <tt>false</tt>.</returns>
        internal virtual bool ContainsBucketWith(TNode vertex)
        {
            return BucketMap.ContainsKey(vertex);
        }

        /// <summary>
        /// Retrieves element from the head bucket by invoking <see cref="Bucket.poll()"/> or null if this
        /// {@code BucketList} is empty.
        /// <para>
        /// Removes the head bucket if it becomes empty after the operation.
        ///
        /// </para>
        /// </summary>
        /// <returns>vertex returned by <see cref="Bucket.poll()"/> invoked on head bucket or null if this
        ///         {@code BucketList} is empty.</returns>
        internal virtual TNode Poll()
        {
            if (BucketMap.Count > 0)
            {
                var res = Head.Poll();
                BucketMap.Remove(res);
                if (Head.Empty)
                {
                    Head = Head.Next;
                    if (Head != null)
                    {
                        Head.Prev = null;
                    }
                }

                return res;
            }
            else
            {
                return default(TNode);
            }
        }

        /// <summary>
        /// For every bucket B in this {@code BucketList}, which contains vertices from the set
        /// {@code
        /// vertices}, creates a new {@code Bucket} B' and moves vertices from B to B' according to
        /// the following rule: $B' = B\cap vertices$ and $B = B\backslash B'$. For every such
        /// {@code Bucket} B only one {@code Bucket} B' is created. If some bucket B becomes empty
        /// after this operation, it is removed from the data structure.
        /// </summary>
        /// <param name="vertices"> the vertices, that should be moved to new buckets.</param>
        internal virtual void UpdateBuckets(ISet<TNode> vertices)
        {
            ISet<Bucket> visitedBuckets = new HashSet<Bucket>();
            foreach (var vertex in vertices)
            {
                var bucket = BucketMap[vertex];
                if (visitedBuckets.Contains(bucket))
                {
                    bucket.Prev.AddVertex(vertex);
                    BucketMap[vertex] = bucket.Prev;
                }
                else
                {
                    visitedBuckets.Add(bucket);
                    var newBucket = new Bucket(this, vertex);
                    newBucket.InsertBefore(bucket);
                    BucketMap[vertex] = newBucket;
                    if (Head == bucket)
                    {
                        Head = newBucket;
                    }
                }

                bucket.RemoveVertex(vertex);
                if (bucket.Empty)
                {
                    visitedBuckets.remove(bucket);
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
        private class Bucket
        {
            private readonly BucketList _outerInstance;

            /// <summary>
            /// Reference of the bucket with lexicographically smaller label.
            /// </summary>
            internal Bucket Next;

            /// <summary>
            /// Reference of the bucket with lexicographically larger label.
            /// </summary>
            internal Bucket Prev;

            /// <summary>
            /// Set of vertices currently stored in this bucket.
            /// </summary>
            internal ISet<TNode> Vertices;

            /// <summary>
            /// Creates a new bucket with all {@code vertices} stored in it.
            /// </summary>
            /// <param name="vertices"> vertices to store in this bucket.</param>
            internal Bucket(BucketList outerInstance, ICollection<TNode> vertices)
            {
                _outerInstance = outerInstance;
                Vertices       = new LinkedHashSet<TNode>(vertices);
            }

            /// <summary>
            /// Creates a new Bucket with a single {@code vertex} in it.
            /// </summary>
            /// <param name="vertex"> the vertex to store in this bucket.</param>
            internal Bucket(BucketList outerInstance, TNode vertex)
            {
                _outerInstance = outerInstance;
                Vertices       = new LinkedHashSet<TNode>();
                Vertices.Add(vertex);
            }

            /// <summary>
            /// Removes the {@code vertex} from this bucket.
            /// </summary>
            /// <param name="vertex"> the vertex to remove.</param>
            internal virtual void RemoveVertex(TNode vertex)
            {
                Vertices.remove(vertex);
            }

            /// <summary>
            /// Removes this bucket from the data structure.
            /// </summary>
            internal virtual void RemoveSelf()
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
            /// Inserts this bucket in the data structure before the {@code bucket}.
            /// </summary>
            /// <param name="bucket"> the bucket, that will be the next to this bucket.</param>
            internal virtual void InsertBefore(Bucket bucket)
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
            /// Adds the {@code vertex} to this bucket.
            /// </summary>
            /// <param name="vertex"> the vertex to add.</param>
            internal virtual void AddVertex(TNode vertex)
            {
                Vertices.Add(vertex);
            }

            /// <summary>
            /// Retrieves one vertex from this bucket.
            /// </summary>
            /// <returns>vertex, that was removed from this bucket, null if the bucket was empty.</returns>
            internal virtual TNode Poll()
            {
                if (Vertices.Count == 0)
                {
                    return default(TNode);
                }
                else
                {
                    TNode vertex = Vertices.GetEnumerator().next();
                    Vertices.remove(vertex);
                    return vertex;
                }
            }

            /// <summary>
            /// Checks whether this bucket is empty.
            /// </summary>
            /// <returns><tt>true</tt> if this bucket doesn't contain any elements, otherwise false.</returns>
            internal virtual bool Empty
            {
                get
                {
                    return Vertices.Count == 0;
                }
            }
        }
    }
}
