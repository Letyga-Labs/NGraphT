using System.Collections.Concurrent;

/*
 * (C) Copyright 2018-2021, by CHEN Kui and Contributors.
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
namespace NGraphT.Core.Graph.Concurrent;

using Core;
using Graph;

/// <summary>
/// Create a synchronized (thread-safe) Graph backed by the specified Graph. This Graph is designed
/// to support concurrent reads which are mutually exclusive with writes. In order to guarantee
/// serial access, it is critical that <strong>all</strong> access to the backing Graph is
/// accomplished through the created Graph.
///
/// <para>
/// Users need to manually synchronize on edge supplier (see <seealso cref="Graph.getEdgeSupplier()"/>) if
/// creating an edge needs to access shared resources. Failure to follow this advice may result in
/// non-deterministic behavior.
/// </para>
///
/// <para>
/// For all methods returning a Set, the Graph guarantees that all operations on the returned Set do
/// not affect the backing Graph. For <c>edgeSet</c> and <c>vertexSet</c> methods, the
/// returned Set is backed by the underlying graph, but when a traversal over the set is started via
/// a method such as iterator(), a snapshot of the underlying Set is copied for iteration purposes.
/// For <c>edgesOf</c>, <c>incomingEdgesOf</c> and <c>outgoingEdgesOf</c> methods,
/// the returned Set is a unmodifiable copy of the result produced by the underlying Graph. Users can
/// control whether those copies should be cached; caching may significantly increase memory
/// requirements. If users decide to cache those copies and the backing graph's changes don't affect
/// them, those copies will be returned the next time the method is called. If the backing graph's
/// changes affect them, they will be removed from cache and re-created the next time the method is
/// called. If users decide to not cache those copies, the graph will create ephemeral copies every
/// time the method is called. For other methods returning a Set, the Set is just the backing Graph's
/// return.
/// </para>
///
/// <para>
/// As an alternative, a <em>copyless mode</em> is supported. When enabled, no collection copies are
/// made at all (and hence the cache setting is ignored). This requires the caller to explicitly
/// synchronize iteration via the <seealso cref="getLock"/> method. This approach requires quite a bit of care
/// on the part of the calling application, so it is disabled by default.
/// </para>
///
/// <para>
/// Even though this graph implementation is thread-safe, callers should still be aware of potential
/// hazards from removal methods. If calling code obtains a reference to a vertex or edge from the
/// graph, and then calls another graph method to access information about that object, an
/// <seealso cref="System.ArgumentException"/> may be thrown if another thread has concurrently removed that
/// object. Therefore, calling the remove methods concurrently with a typical algorithm is likely to
/// cause the algorithm to fail with an <seealso cref="System.ArgumentException"/>. So really the main
/// concurrent read/write use case is add-only. <br>
/// eg: If threadA tries to get all edges touching a certain vertex after threadB removes the vertex,
/// the algorithm will be interrupted by <seealso cref="System.ArgumentException"/>.
/// </para>
/// 
/// <pre>
/// Thread threadA = new Thread(() -&gt; {
///     Set vertices = graph.vertexSet();
///     for (Object TNode : vertices) {
///         // <seealso cref="System.ArgumentException"/> may be thrown since other threads may have removed
///         // the vertex.
///         Set edges = graph.edgesOf(TNode);
///         doOtherThings();
///     }
/// });
/// Thread threadB = new Thread(() -&gt; {
///     Set vertices = graph.vertexSet();
///     for (Object TNode : vertices) {
///         if (someConditions) {
///             graph.removeVertex(TNode);
///         }
///     }
/// });
/// </pre>
///
/// <para>
/// 
/// One way to avoid the hazard noted above is for the calling application to explicitly synchronize
/// all iterations using the <seealso cref="getLock"/> method.
/// </para>
///
/// <para>
/// The created Graph's hashCode is equal to the backing set's hashCode. And the created Graph is
/// equal to another Graph if they are the same Graph or the backing Graph is equal to the other
/// Graph.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: CHEN Kui.</remarks>
public class AsSynchronizedGraph<TNode, TEdge> : GraphDelegator<TNode, TEdge>, IGraph<TNode, TEdge>
{
    public virtual Lock { get; }

    // A set encapsulating backing vertexSet.
    private CopyOnDemandSet<TNode> _allVerticesSet;

    // A set encapsulating backing edgeSet.
    private CopyOnDemandSet<TEdge> _allEdgesSet;

    private ICacheStrategy<TNode, TEdge> _cacheStrategy;

    /// <summary>
    /// Constructor for AsSynchronizedGraph with default settings (cache disabled, non-fair mode, and
    /// copyless mode disabled).
    /// </summary>
    /// <param name="g"> the backing graph (the delegate) </param>
    public AsSynchronizedGraph(IGraph<TNode, TEdge> g)
        : this(g, false, false, false)
    {
    }

    /// <summary>
    /// Constructor for AsSynchronizedGraph with specified properties.
    /// </summary>
    /// <param name="g"> the backing graph (the delegate) </param>
    /// <param name="cacheEnable"> a flag describing whether a cache will be used.</param>
    /// <param name="fair"> a flag describing whether fair mode will be used.</param>
    /// <param name="copyless"> a flag describing whether copyless mode will be used.</param>
    private AsSynchronizedGraph(IGraph<TNode, TEdge> g, bool cacheEnable, bool fair, bool copyless)
        : base(g)
    {
        Lock = new ReentrantReadWriteLock(fair);
        if (copyless)
        {
            _cacheStrategy = new NoCopy(this);
        }
        else if (cacheEnable)
        {
            _cacheStrategy = new CacheAccess(this);
        }
        else
        {
            _cacheStrategy = new NoCache(this);
        }

        _allEdgesSet    = new CopyOnDemandSet<TEdge>(base.EdgeSet(), Lock, copyless);
        _allVerticesSet = new CopyOnDemandSet<TNode>(base.VertexSet(), Lock, copyless);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        Lock.readLock().@lock();
        try
        {
            return base.GetAllEdges(sourceVertex, targetVertex);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override TEdge GetEdge(TNode sourceVertex, TNode targetVertex)
    {
        Lock.readLock().@lock();
        try
        {
            return base.GetEdge(sourceVertex, targetVertex);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
    {
        Lock.writeLock().@lock();
        try
        {
            var edge = _cacheStrategy.AddEdge(sourceVertex, targetVertex);
            if (edge != null)
            {
                EdgeSetModified();
            }

            return edge;
        }
        finally
        {
            Lock.writeLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        Lock.writeLock().@lock();
        try
        {
            if (_cacheStrategy.AddEdge(sourceVertex, targetVertex, edge))
            {
                EdgeSetModified();
                return true;
            }

            return false;
        }
        finally
        {
            Lock.writeLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override bool AddVertex(TNode node)
    {
        Lock.writeLock().@lock();
        try
        {
            if (base.AddVertex(node))
            {
                VertexSetModified();
                return true;
            }

            return false;
        }
        finally
        {
            Lock.writeLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override bool ContainsEdge(TNode sourceVertex, TNode targetVertex)
    {
        Lock.readLock().@lock();
        try
        {
            return base.ContainsEdge(sourceVertex, targetVertex);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override bool ContainsEdge(TEdge edge)
    {
        Lock.readLock().@lock();
        try
        {
            return base.ContainsEdge(edge);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override bool ContainsVertex(TNode node)
    {
        Lock.readLock().@lock();
        try
        {
            return base.ContainsVertex(node);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override int DegreeOf(TNode vertex)
    {
        Lock.readLock().@lock();
        try
        {
            return base.DegreeOf(vertex);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgeSet()
    {
        return _allEdgesSet;
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgesOf(TNode vertex)
    {
        Lock.readLock().@lock();
        try
        {
            return _cacheStrategy.EdgesOf(vertex);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override int InDegreeOf(TNode vertex)
    {
        Lock.readLock().@lock();
        try
        {
            return base.InDegreeOf(vertex);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TNode vertex)
    {
        Lock.readLock().@lock();
        try
        {
            return _cacheStrategy.IncomingEdgesOf(vertex);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TNode vertex)
    {
        Lock.readLock().@lock();
        try
        {
            return base.OutDegreeOf(vertex);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> OutgoingEdgesOf(TNode vertex)
    {
        Lock.readLock().@lock();
        try
        {
            return _cacheStrategy.OutgoingEdgesOf(vertex);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public virtual bool RemoveAllEdges<T1>(ICollection<T1> edges) where T1 : TEdge
    {
        Lock.writeLock().@lock();
        try
        {
            return base.RemoveAllEdges(edges);
        }
        finally
        {
            Lock.writeLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> RemoveAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        Lock.writeLock().@lock();
        try
        {
            return base.RemoveAllEdges(sourceVertex, targetVertex);
        }
        finally
        {
            Lock.writeLock().unlock();
        }
    }

    /// <inheritdoc/>
    public virtual bool RemoveAllVertices<T1>(ICollection<T1> vertices) where T1 : TNode
    {
        Lock.writeLock().@lock();
        try
        {
            return base.RemoveAllVertices(vertices);
        }
        finally
        {
            Lock.writeLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override bool RemoveEdge(TEdge edge)
    {
        Lock.writeLock().@lock();
        try
        {
            if (_cacheStrategy.RemoveEdge(edge))
            {
                EdgeSetModified();
                return true;
            }

            return false;
        }
        finally
        {
            Lock.writeLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override TEdge RemoveEdge(TNode sourceVertex, TNode targetVertex)
    {
        Lock.writeLock().@lock();
        try
        {
            var edge = _cacheStrategy.RemoveEdge(sourceVertex, targetVertex);
            if (edge != null)
            {
                EdgeSetModified();
            }

            return edge;
        }
        finally
        {
            Lock.writeLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override bool RemoveVertex(TNode node)
    {
        Lock.writeLock().@lock();
        try
        {
            if (_cacheStrategy.RemoveVertex(node))
            {
                EdgeSetModified();
                VertexSetModified();
                return true;
            }

            return false;
        }
        finally
        {
            Lock.writeLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        Lock.readLock().@lock();
        try
        {
            return base.ToString();
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override ISet<TNode> VertexSet()
    {
        return _allVerticesSet;
    }

    /// <inheritdoc/>
    public override TNode GetEdgeSource(TEdge edge)
    {
        Lock.readLock().@lock();
        try
        {
            return base.GetEdgeSource(edge);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override TNode GetEdgeTarget(TEdge edge)
    {
        Lock.readLock().@lock();
        try
        {
            return base.GetEdgeTarget(edge);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override double GetEdgeWeight(TEdge edge)
    {
        Lock.readLock().@lock();
        try
        {
            return base.GetEdgeWeight(edge);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        Lock.writeLock().@lock();
        try
        {
            base.SetEdgeWeight(edge, weight);
        }
        finally
        {
            Lock.writeLock().unlock();
        }
    }

    /// <summary>
    /// Return whether the graph uses cache for <c>edgesOf</c>, <c>incomingEdgesOf</c>
    /// and <c>outgoingEdgesOf</c> methods.
    /// </summary>
    /// <returns><c>true</c> if cache is in use, <c>false</c> if cache is not in use.</returns>
    public virtual bool CacheEnabled
    {
        get
        {
            Lock.readLock().@lock();
            try
            {
                return _cacheStrategy.CacheEnabled;
            }
            finally
            {
                Lock.readLock().unlock();
            }
        }
    }

    /// <summary>
    /// Return whether copyless mode is used for collection-returning methods.
    /// </summary>
    /// <returns><c>true</c> if the graph uses copyless mode, <c>false</c> otherwise.</returns>
    public virtual bool Copyless
    {
        get
        {
            return _allVerticesSet.Copyless;
        }
    }

    /// <summary>
    /// Set the cache strategy for <c>edgesOf</c>, <c>incomingEdgesOf</c> and
    /// <c>outgoingEdgesOf</c> methods.
    /// </summary>
    /// <param name="cacheEnabled"> a flag whether to use cache for those methods, if <c>true</c>,
    ///        cache will be used for those methods, otherwise cache will not be used.</param>
    /// <returns>the AsSynchronizedGraph.</returns>
    public virtual AsSynchronizedGraph<TNode, TEdge> SetCache(bool cacheEnabled)
    {
        Lock.writeLock().@lock();
        try
        {
            if (cacheEnabled == CacheEnabled)
            {
                return this;
            }

            if (cacheEnabled)
            {
                _cacheStrategy = new CacheAccess(this);
            }
            else
            {
                _cacheStrategy = new NoCache(this);
            }

            return this;
        }
        finally
        {
            Lock.writeLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        Lock.readLock().@lock();
        try
        {
            return Delegate.GetHashCode();
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object o)
    {
        if (this == o)
        {
            return true;
        }

        Lock.readLock().@lock();
        try
        {
            return Delegate.Equals(o);
        }
        finally
        {
            Lock.readLock().unlock();
        }
    }

    /// <summary>
    /// Create a unmodifiable copy of the set.
    /// </summary>
    /// <param name="set"> the set to be copied.</param>
    /// <returns>a unmodifiable copy of the set.</returns>
    private ISet<TC> CopySet<TC>(ISet<TC> set)
    {
        return Collections.unmodifiableSet(new LinkedHashSet<>(set));
    }

    /// <summary>
    /// Inform allVerticesSet that the backing data has been modified.
    /// </summary>
    private void VertexSetModified()
    {
        _allVerticesSet.Modified();
    }

    /// <summary>
    /// Inform allEdgesSet that the backing data has been modified.
    /// </summary>
    private void EdgeSetModified()
    {
        _allEdgesSet.Modified();
    }

    /// <summary>
    /// Return whether fair mode is used for synchronizing access to this graph.
    /// </summary>
    /// <returns><c>true</c> if the graph uses fair mode, <c>false</c> if non-fair mode.</returns>
    public virtual bool Fair
    {
        get
        {
            return Lock.isFair();
        }
    }

    /// <summary>
    /// Get the read/write lock used to synchronize all access to this graph. This can be used by
    /// calling applications to explicitly synchronize compound sequences of graph accessses. The
    /// lock is reentrant, so the locks acquired internally by AsSynchronizedGraph will not interfere
    /// with the caller's acquired lock. However, write methods <strong>MUST NOT</strong> be called
    /// while holding a read lock, otherwise a deadlock will occur.
    /// </summary>
    /// <returns>the reentrant read/write lock used to synchronize all access to this graph.</returns>
    /// <summary>
    /// Create a synchronized (thread-safe) and unmodifiable Set backed by the specified Set. In
    /// order to guarantee serial access, it is critical that <strong>all</strong> access to the
    /// backing Set is accomplished through the created Set.
    ///
    /// <para>
    /// When a traversal over the set is started via a method such as iterator(), a snapshot of the
    /// underlying set is copied for iteration purposes (unless copyless mode is enabled).
    /// </para>
    ///
    /// <para>
    /// The created Set's hashCode is equal to the backing Set's hashCode. And the created Set is
    /// equal to another set if they are the same Set or the backing Set is equal to the other Set.
    /// </para>
    ///
    /// <para>
    /// The created set will be serializable if the backing set is serializable.
    /// </para>
    /// </summary>
    /// @param <TEdge> the class of the objects in the set
    /// 
    /// <remarks>Author: CHEN Kui.</remarks>
        private class CopyOnDemandSet<TEdge> : ISet<TEdge>
    {
        internal const long SerialVersionUID = 5553953818148294283L;

        // Backing set.
        internal ISet<TEdge> Set;

        // When this flag is set, the backing set is used directly rather than
        // a copy.
        internal readonly bool Copyless;

        // Backing set's unmodifiable copy. If null, needs to be recomputed on next access.
            volatile internal ISet<TEdge> Copy;

        internal readonly ReadWriteLock ReadWriteLock;

        internal const string UNMODIFIABLE = "this set is unmodifiable";

        /// <summary>
        /// Constructor for CopyOnDemandSet.
        /// </summary>
        /// <param name="s"> the backing set.</param>
        /// <param name="readWriteLock"> the ReadWriteLock on which to locked.</param>
        /// <param name="copyless"> whether copyless mode should be used.</param>
        internal CopyOnDemandSet(ISet<TEdge> s, ReadWriteLock readWriteLock, bool copyless)
        {
            Set                = Objects.requireNonNull(s, "s must not be null");
            copy               = null;
            ReadWriteLock = readWriteLock;
            this.copyless      = copyless;
        }

        /// <summary>
        /// Return whether copyless mode is used for iteration.
        /// </summary>
        /// <returns><c>true</c> if the set uses copyless mode, <c>false</c> otherwise.</returns>
        public virtual bool Copyless
        {
            get
            {
                return copyless;
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override int Size()
        {
            ReadWriteLock.readLock().@lock();
            try
            {
                return Set.Count;
            }
            finally
            {
                ReadWriteLock.readLock().unlock();
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool Empty
        {
            get
            {
                ReadWriteLock.readLock().@lock();
                try
                {
                    return Set.Count == 0;
                }
                finally
                {
                    ReadWriteLock.readLock().unlock();
                }
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool Contains(object o)
        {
            ReadWriteLock.readLock().@lock();
            try
            {
                return Set.Contains(o);
            }
            finally
            {
                ReadWriteLock.readLock().unlock();
            }
        }

        /// <summary>
        /// Returns an iterator over the elements in the backing set's unmodifiable copy. The
        /// elements are returned in the same order of the backing set.
        /// </summary>
        /// <returns>an iterator over the elements in the backing set's unmodifiable copy.</returns>
        public override IEnumerator<TEdge> Iterator()
        {
            return Copy.GetEnumerator();
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override object[] ToArray()
        {
            ReadWriteLock.readLock().@lock();
            try
            {
                return Set.ToArray();
            }
            finally
            {
                ReadWriteLock.readLock().unlock();
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override T[] ToArray<T>(T[] a)
        {
            ReadWriteLock.readLock().@lock();
            try
            {
                return Set.toArray(a);
            }
            finally
            {
                ReadWriteLock.readLock().unlock();
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool Add(TEdge edge)
        {
            throw new NotSupportedException(UNMODIFIABLE);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool Remove(object o)
        {
            throw new NotSupportedException(UNMODIFIABLE);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool ContainsAll<T1>(ICollection<T1> c)
        {
            ReadWriteLock.readLock().@lock();
            try
            {
                return Set.ContainsAll(c);
            }
            finally
            {
                ReadWriteLock.readLock().unlock();
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool AddAll<T1>(ICollection<T1> c) where T1 : TEdge
        {
            throw new NotSupportedException(UNMODIFIABLE);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool RetainAll<T1>(ICollection<T1> c)
        {
            throw new NotSupportedException(UNMODIFIABLE);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override bool RemoveAll<T1>(ICollection<T1> c)
        {
            throw new NotSupportedException(UNMODIFIABLE);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override void Clear()
        {
            throw new NotSupportedException(UNMODIFIABLE);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        // Override default methods in Collection
//JAVA TO C# CONVERTER TODO TASK: There is no C# equivalent to the Java 'super' constraint:
//ORIGINAL LINE: @Override public void forEach(Consumer<? super TEdge> action)
        public override void ForEach<T1>(Action<T1> action)
        {
            ReadWriteLock.readLock().@lock();
            try
            {
                Set.forEach(action);
            }
            finally
            {
                ReadWriteLock.readLock().unlock();
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no C# equivalent to the Java 'super' constraint:
//ORIGINAL LINE: @Override public boolean removeIf(Predicate<? super TEdge> filter)
        public override bool RemoveIf<T1>(Predicate<T1> filter)
        {
            throw new NotSupportedException(UNMODIFIABLE);
        }

        /// <summary>
        /// Creates a <c>Spliterator</c> over the elements in the set's unmodifiable copy.
        /// </summary>
        /// <returns>a <c>Spliterator</c> over the elements in the backing set's unmodifiable
        ///         copy.</returns>
        public override Spliterator<TEdge> Spliterator()
        {
            return Copy.spliterator();
        }

        /// <summary>
        /// Return a sequential <c>Stream</c> with the backing set's unmodifiable copy as its
        /// source.
        /// </summary>
        /// <returns>a sequential <c>Stream</c> with the backing set's unmodifiable copy as its
        ///         source.</returns>
        public override Stream<TEdge> Stream()
        {
            return Copy.stream();
        }

        /// <summary>
        /// Return a possibly parallel <c>Stream</c> with the backing set's unmodifiable copy
        /// as its source.
        /// </summary>
        /// <returns>a possibly parallel <c>Stream</c> with the backing set's unmodifiable copy
        ///         as its source.</returns>
        public override Stream<TEdge> ParallelStream()
        {
            return Copy.parallelStream();
        }

        /// <summary>
        /// Compares the specified object with this set for equality.
        /// </summary>
        /// <param name="o"> object to be compared for equality with this set.</param>
        /// <returns><c>true</c> if o and this set are the same object or o is equal to the
        ///         backing object, false otherwise.</returns>
        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            ReadWriteLock.readLock().@lock();
            try
            {
                return Set.SetEquals(o);
            }
            finally
            {
                ReadWriteLock.readLock().unlock();
            }
        }

        /// <summary>
        /// Return the backing set's hashcode.
        /// </summary>
        /// <returns>the backing set's hashcode.</returns>
        public override int GetHashCode()
        {
            ReadWriteLock.readLock().@lock();
            try
            {
                return Set.GetHashCode();
            }
            finally
            {
                ReadWriteLock.readLock().unlock();
            }
        }

        /// <summary>
        /// Return the backing set's toString result.
        /// </summary>
        /// <returns>the backing set's toString result.</returns>
        public override string ToString()
        {
            ReadWriteLock.readLock().@lock();
            try
            {
                return Set.ToString();
            }
            finally
            {
                ReadWriteLock.readLock().unlock();
            }
        }

        /// <summary>
        /// Get the backing set's unmodifiable copy, or a direct reference to the backing set if in
        /// copyless mode.
        /// </summary>
        /// <returns>the backing set or its unmodifiable copy.</returns>
        internal virtual ISet<TEdge> Copy
        {
            get
            {
                if (copyless)
                {
                    return Set;
                }

                ReadWriteLock.readLock().@lock();
                try
                {
                    ISet<TEdge> tempCopy = copy;
                    if (tempCopy == null)
                    {
                        lock (this)
                        {
                            tempCopy = copy;
                            if (tempCopy == null)
                            {
                                copy = tempCopy = new LinkedHashSet<TEdge>(Set);
                            }
                        }
                    }

                    return tempCopy;
                }
                finally
                {
                    ReadWriteLock.readLock().unlock();
                }
            }
        }

        /// <summary>
        /// If the backing set is modified, call this method to let this set knows the backing set's
        /// copy need to update.
        /// </summary>
        internal virtual void Modified()
        {
            copy = null;
        }
    }

    /// <summary>
    /// An interface for cache strategy of AsSynchronizedGraph's <c>edgesOf</c>,
    /// <c>incomingEdgesOf</c> and <c>outgoingEdgesOf</c> methods.
    /// </summary>
    private interface ICacheStrategy<TNode, TEdge>
    {
        /// <summary>
        /// Add an edge into AsSynchronizedGraph's backing graph.
        /// </summary>
        TEdge AddEdge(TNode sourceVertex, TNode targetVertex);

        /// <summary>
        /// Add an edge into AsSynchronizedGraph's backing graph.
        /// </summary>
        bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge);

        /// <summary>
        /// Get all edges touching the specified vertex in AsSynchronizedGraph's backing graph.
        /// </summary>
        ISet<TEdge> EdgesOf(TNode vertex);

        /// <summary>
        /// Get a set of all edges in AsSynchronizedGraph's backing graph incoming into the specified
        /// vertex.
        /// </summary>
        ISet<TEdge> IncomingEdgesOf(TNode vertex);

        /// <summary>
        /// Get a set of all edges in AsSynchronizedGraph's backing graph outgoing from the specified
        /// vertex.
        /// </summary>
        ISet<TEdge> OutgoingEdgesOf(TNode vertex);

        /// <summary>
        /// Remove the specified edge from AsSynchronizedGraph's backing graph.
        /// </summary>
        bool RemoveEdge(TEdge edge);

        /// <summary>
        /// Remove an edge from AsSynchronizedGraph's backing graph.
        /// </summary>
        TEdge RemoveEdge(TNode sourceVertex, TNode targetVertex);

        /// <summary>
        /// Remove the specified vertex from AsSynchronizedGraph's backing graph.
        /// </summary>
        bool RemoveVertex(TNode node);

        /// <summary>
        /// Return whether the graph uses cache for <c>edgesOf</c>,
        /// <c>incomingEdgesOf</c> and <c>outgoingEdgesOf</c> methods.
        /// </summary>
        /// <returns><c>true</c> if cache is in use, <c>false</c> if cache is not in use.</returns>
        bool CacheEnabled { get; }
    }

    /// <summary>
    /// Don't use cache for AsSynchronizedGraph's <c>edgesOf</c>, <c>incomingEdgesOf</c>
    /// and <c>outgoingEdgesOf</c> methods.
    /// </summary>
        private class NoCache : ICacheStrategy<TNode, TEdge>
    {
        private readonly AsSynchronizedGraph<TNode, TEdge> _outerInstance;

        public NoCache(AsSynchronizedGraph<TNode, TEdge> outerInstance)
        {
            _outerInstance = outerInstance;
        }

        internal const long SerialVersionUID = 19246150051213471L;

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
        {
            return _outerInstance.AddEdge(sourceVertex, targetVertex);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
        {
            return _outerInstance.AddEdge(sourceVertex, targetVertex, edge);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual ISet<TEdge> EdgesOf(TNode vertex)
        {
            return outerInstance.CopySet(_outerInstance.EdgesOf(vertex));
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual ISet<TEdge> IncomingEdgesOf(TNode vertex)
        {
            return outerInstance.CopySet(_outerInstance.IncomingEdgesOf(vertex));
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual ISet<TEdge> OutgoingEdgesOf(TNode vertex)
        {
            return outerInstance.CopySet(_outerInstance.OutgoingEdgesOf(vertex));
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual bool RemoveEdge(TEdge edge)
        {
            return _outerInstance.RemoveEdge(edge);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual TEdge RemoveEdge(TNode sourceVertex, TNode targetVertex)
        {
            return _outerInstance.RemoveEdge(sourceVertex, targetVertex);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual bool RemoveVertex(TNode node)
        {
            return _outerInstance.RemoveVertex(node);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual bool CacheEnabled
        {
            get
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Disable cache as per <c>NoCache</c>, and also don't produce copies; instead, just
    /// directly return the results from the underlying graph. This requires the caller to explicitly
    /// synchronize iterations over these collections.
    /// </summary>
        private class NoCopy : NoCache
    {
        private readonly AsSynchronizedGraph<TNode, TEdge> _outerInstance;

        public NoCopy(AsSynchronizedGraph<TNode, TEdge> outerInstance)
            : base(outerInstance)
        {
            _outerInstance = outerInstance;
        }

        internal const long SerialVersionUID = -5046944235164395939L;

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override ISet<TEdge> EdgesOf(TNode vertex)
        {
            return _outerInstance.EdgesOf(vertex);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override ISet<TEdge> IncomingEdgesOf(TNode vertex)
        {
            return _outerInstance.IncomingEdgesOf(vertex);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public override ISet<TEdge> OutgoingEdgesOf(TNode vertex)
        {
            return _outerInstance.OutgoingEdgesOf(vertex);
        }
    }

    /// <summary>
    /// Use cache for AsSynchronizedGraph's <c>edgesOf</c>, <c>incomingEdgesOf</c> and
    /// <c>outgoingEdgesOf</c> methods.
    /// </summary>
        private class CacheAccess : ICacheStrategy<TNode, TEdge>
    {
        private readonly AsSynchronizedGraph<TNode, TEdge> _outerInstance;

        public CacheAccess(AsSynchronizedGraph<TNode, TEdge> outerInstance)
        {
            _outerInstance = outerInstance;
        }

        internal const long SerialVersionUID = -18262921841829294L;

        // A map caching for incomingEdges operation.
            internal readonly IDictionary<TNode, ISet<TEdge>> IncomingEdgesMap =
            new ConcurrentDictionary<TNode, ISet<TEdge>>();

        // A map caching for outgoingEdges operation.
            internal readonly IDictionary<TNode, ISet<TEdge>> OutgoingEdgesMap =
            new ConcurrentDictionary<TNode, ISet<TEdge>>();

        // A map caching for edgesOf operation.
            internal readonly IDictionary<TNode, ISet<TEdge>> EdgesOfMap = new ConcurrentDictionary<TNode, ISet<TEdge>>();

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
        {
            var edge = _outerInstance.AddEdge(sourceVertex, targetVertex);
            if (edge != null)
            {
                EdgeModified(sourceVertex, targetVertex);
            }

            return edge;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
        {
            if (_outerInstance.AddEdge(sourceVertex, targetVertex, edge))
            {
                EdgeModified(sourceVertex, targetVertex);
                return true;
            }

            return false;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual ISet<TEdge> EdgesOf(TNode vertex)
        {
            ISet<TEdge> s = EdgesOfMap[vertex];
            if (s != null)
            {
                return s;
            }

            s                  = outerInstance.CopySet(_outerInstance.EdgesOf(vertex));
            EdgesOfMap[vertex] = s;
            return s;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual ISet<TEdge> IncomingEdgesOf(TNode vertex)
        {
            ISet<TEdge> s = IncomingEdgesMap[vertex];
            if (s != null)
            {
                return s;
            }

            s                        = outerInstance.CopySet(_outerInstance.IncomingEdgesOf(vertex));
            IncomingEdgesMap[vertex] = s;
            return s;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual ISet<TEdge> OutgoingEdgesOf(TNode vertex)
        {
            ISet<TEdge> s = OutgoingEdgesMap[vertex];
            if (s != null)
            {
                return s;
            }

            s                        = outerInstance.CopySet(_outerInstance.OutgoingEdgesOf(vertex));
            OutgoingEdgesMap[vertex] = s;
            return s;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual bool RemoveEdge(TEdge edge)
        {
            var sourceVertex = GetEdgeSource(edge);
            var targetVertex = GetEdgeTarget(edge);
            if (_outerInstance.RemoveEdge(edge))
            {
                EdgeModified(sourceVertex, targetVertex);
                return true;
            }

            return false;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual TEdge RemoveEdge(TNode sourceVertex, TNode targetVertex)
        {
            var edge = _outerInstance.RemoveEdge(sourceVertex, targetVertex);
            if (edge != null)
            {
                EdgeModified(sourceVertex, targetVertex);
            }

            return edge;
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual bool RemoveVertex(TNode node)
        {
            if (_outerInstance.RemoveVertex(node))
            {
                EdgesOfMap.Clear();
                IncomingEdgesMap.Clear();
                OutgoingEdgesMap.Clear();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clear the copies which the edge to be added or removed can affect.
        /// </summary>
        /// <param name="sourceVertex"> source vertex of the modified edge.</param>
        /// <param name="targetVertex"> target vertex of the modified edge.</param>
        internal virtual void EdgeModified(TNode sourceVertex, TNode targetVertex)
        {
            OutgoingEdgesMap.Remove(sourceVertex);
            IncomingEdgesMap.Remove(targetVertex);
            EdgesOfMap.Remove(sourceVertex);
            EdgesOfMap.Remove(targetVertex);
            if (!_outerInstance.Type.Directed)
            {
                OutgoingEdgesMap.Remove(targetVertex);
                IncomingEdgesMap.Remove(sourceVertex);
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual bool CacheEnabled
        {
            get
            {
                return true;
            }
        }
    }

    /// <summary>
    /// A builder for <seealso cref="AsSynchronizedGraph"/>.
    /// </summary>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// 
    /// <remarks>Author: CHEN Kui.</remarks>
    public class Builder<TNode, TEdge>
    {
        internal bool CacheEnable;
        internal bool Fair;
        internal bool Copyless;

        /// <summary>
        /// Construct a new Builder with non-fair mode, cache disabled, and copyless mode disabled.
        /// </summary>
        public Builder()
        {
            cacheEnable = false;
            fair        = false;
            copyless    = false;
        }

        /// <summary>
        /// Construct a new Builder matching the settings of an existing graph.
        /// </summary>
        /// <param name="graph"> the graph on which to base the builder.</param>
        public Builder(AsSynchronizedGraph<TNode, TEdge> graph)
        {
            this.cacheEnable = graph.CacheEnabled;
            this.fair        = graph.Fair;
            this.copyless    = graph.Copyless;
        }

        /// <summary>
        /// Request a synchronized graph without caching.
        /// </summary>
        /// <returns>the Builder.</returns>
        public virtual Builder<TNode, TEdge> CacheDisable()
        {
            cacheEnable = false;
            return this;
        }

        /// <summary>
        /// Request a synchronized graph with caching.
        /// </summary>
        /// <returns>the Builder.</returns>
        public virtual Builder<TNode, TEdge> CacheEnable()
        {
            cacheEnable = true;
            return this;
        }

        /// <summary>
        /// Return whether a cache will be used for the synchronized graph being built.
        /// </summary>
        /// <returns><c>true</c> if cache will be used, <c>false</c> if cache will not be
        ///         used.</returns>
        public virtual bool CacheEnable
        {
            get
            {
                return cacheEnable;
            }
        }

        /// <summary>
        /// Request a synchronized graph which does not return collection copies.
        /// </summary>
        /// <returns>the Builder.</returns>
        public virtual Builder<TNode, TEdge> SetCopyless()
        {
            copyless = true;
            return this;
        }

        /// <summary>
        /// Request a synchronized graph which returns collection copies.
        /// </summary>
        /// <returns>the Builder.</returns>
        public virtual Builder<TNode, TEdge> ClearCopyless()
        {
            copyless = false;
            return this;
        }

        /// <summary>
        /// Return whether copyless mode will be used for the synchronized graph being built.
        /// </summary>
        /// <returns><c>true</c> if constructed as copyless, <c>false</c> otherwise.</returns>
        public virtual bool Copyless
        {
            get
            {
                return copyless;
            }
        }

        /// <summary>
        /// Request a synchronized graph with fair mode.
        /// </summary>
        /// <returns>the SynchronizedGraphParams.</returns>
        public virtual Builder<TNode, TEdge> SetFair()
        {
            fair = true;
            return this;
        }

        /// <summary>
        /// Request a synchronized graph with non-fair mode.
        /// </summary>
        /// <returns>the SynchronizedGraphParams.</returns>
        public virtual Builder<TNode, TEdge> SetNonfair()
        {
            fair = false;
            return this;
        }

        /// <summary>
        /// Return whether fair mode will be used for the synchronized graph being built.
        /// </summary>
        /// <returns><c>true</c> if constructed as fair mode, <c>false</c> if non-fair.</returns>
        public virtual bool Fair
        {
            get
            {
                return fair;
            }
        }

        /// <summary>
        /// Build the AsSynchronizedGraph.
        /// </summary>
        /// <param name="graph"> the backing graph (the delegate) </param>
        /// <returns>the AsSynchronizedGraph.</returns>
        public virtual AsSynchronizedGraph<TNode, TEdge> Build(IGraph<TNode, TEdge> graph)
        {
            return new AsSynchronizedGraph<TNode, TEdge>(graph, cacheEnable, fair, copyless);
        }
    }
}
