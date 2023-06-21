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
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using J2N.Collections.Generic.Extensions;
using Java2Net = J2N.Collections.Generic;

namespace NGraphT.Core.Graph.Concurrent;

/// <summary>
/// Create a synchronized (thread-safe) Graph backed by the specified Graph. This Graph is designed
/// to support concurrent reads which are mutually exclusive with writes. In order to guarantee
/// serial access, it is critical that <strong>all</strong> access to the backing Graph is
/// accomplished through the created Graph.
///
/// <para>
/// Users need to manually synchronize on edge supplier (see <see cref="IGraph{TVertex,TEdge}.EdgeSupplier"/>) if
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
/// synchronize iteration via the <see cref="Lock"/> method. This approach requires quite a bit of care
/// on the part of the calling application, so it is disabled by default.
/// </para>
///
/// <para>
/// Even though this graph implementation is thread-safe, callers should still be aware of potential
/// hazards from removal methods. If calling code obtains a reference to a vertex or edge from the
/// graph, and then calls another graph method to access information about that object, an
/// <see cref="System.ArgumentException"/> may be thrown if another thread has concurrently removed that
/// object. Therefore, calling the remove methods concurrently with a typical algorithm is likely to
/// cause the algorithm to fail with an <see cref="System.ArgumentException"/>. So really the main
/// concurrent read/write use case is add-only.
/// </para>
///
/// <example>
/// If threadA tries to get all edges touching a certain vertex after threadB removes the vertex,
/// the algorithm will be interrupted by <see cref="System.ArgumentException"/>.
/// <code>
/// var threadA = new Thread(() =&gt; {
///     vra vertices = graph.VertexSet;
///     foreach (var vertex : vertices) {
///         // <see cref="System.ArgumentException"/> may be thrown since other threads may have removed
///         // the vertex.
///         var edges = graph.EdgesOf(vertex);
///         doOtherThings();
///     }
/// });
/// var threadB = new Thread(() =&gt; {
///     var vertices = graph.VertexSet;
///     foreach (var vertex : vertices) {
///         if (someConditions) {
///             graph.RemoveVertex(vertex);
///         }
///     }
/// });
/// </code>
/// </example>
///
/// <para>
/// One way to avoid the hazard noted above is for the calling application to explicitly synchronize
/// all iterations using the <see cref="Lock"/> method.
/// </para>
///
/// <para>
/// The created Graph's hashCode is equal to the backing set's hashCode. And the created Graph is
/// equal to another Graph if they are the same Graph or the backing Graph is equal to the other
/// Graph.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: CHEN Kui.</remarks>
[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order")]
public sealed class AsSynchronizedGraph<TVertex, TEdge> : GraphDelegator<TVertex, TEdge>, IDisposable
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// A set encapsulating backing vertexSet.
    /// </summary>
    private readonly CopyOnDemandSet<TVertex> _allVerticesSet;

    /// <summary>
    /// A set encapsulating backing edgeSet.
    /// </summary>
    private readonly CopyOnDemandSet<TEdge> _allEdgesSet;

    private ICacheStrategy<TVertex, TEdge> _cacheStrategy;

    /// <summary>
    /// Constructor for AsSynchronizedGraph with default settings (cache disabled, non-fair mode, and
    /// copyless mode disabled).
    /// </summary>
    /// <param name="g"> the backing graph (the delegate).</param>
    public AsSynchronizedGraph(IGraph<TVertex, TEdge> g)
        : this(g, false, false, false)
    {
    }

    /// <summary>
    /// Constructor for AsSynchronizedGraph with specified properties.
    /// </summary>
    /// <param name="g"> the backing graph (the delegate).</param>
    /// <param name="cacheEnable"> a flag describing whether a cache will be used.</param>
    /// <param name="fair"> a flag describing whether fair mode will be used.</param>
    /// <param name="copyless"> a flag describing whether copyless mode will be used.</param>
    internal AsSynchronizedGraph(IGraph<TVertex, TEdge> g, bool cacheEnable, bool fair, bool copyless)
        : base(g)
    {
        Fair = fair;
        Lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
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
        _allVerticesSet = new CopyOnDemandSet<TVertex>(base.VertexSet(), Lock, copyless);
    }

    /// <summary>
    /// Get the read/write lock used to synchronize all access to this graph. This can be used by
    /// calling applications to explicitly synchronize compound sequences of graph accessses. The
    /// lock is reentrant, so the locks acquired internally by AsSynchronizedGraph will not interfere
    /// with the caller's acquired lock. However, write methods <strong>MUST NOT</strong> be called
    /// while holding a read lock, otherwise a deadlock will occur.
    /// </summary>
    /// <returns>the reentrant read/write lock used to synchronize all access to this graph.</returns>
    public ReaderWriterLockSlim Lock { get; }

    /// <summary>
    /// Return whether the graph uses cache for <c>edgesOf</c>, <c>incomingEdgesOf</c>
    /// and <c>outgoingEdgesOf</c> methods.
    /// </summary>
    /// <returns><c>true</c> if cache is in use, <c>false</c> if cache is not in use.</returns>
    public bool CacheEnabled
    {
        get
        {
            Lock.EnterReadLock();
            try
            {
                return _cacheStrategy.CacheEnabled;
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Return whether copyless mode is used for collection-returning methods.
    /// </summary>
    /// <returns><c>true</c> if the graph uses copyless mode, <c>false</c> otherwise.</returns>
    public bool Copyless => _allVerticesSet.Copyless;

    /// <summary>
    /// Return whether fair mode will be used for the synchronized graph being built.
    /// </summary>
    /// <returns><c>true</c> if constructed as fair mode, <c>false</c> if non-fair.</returns>
    public bool Fair { get; private set; }

    public void Dispose()
    {
        Lock.Dispose();
    }

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TVertex? sourceVertex, TVertex? targetVertex)
    {
        if (sourceVertex == null || targetVertex == null)
        {
            return ImmutableHashSet<TEdge>.Empty;
        }

        Lock.EnterReadLock();
        try
        {
            return base.GetAllEdges(sourceVertex, targetVertex);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override TEdge? GetEdge(TVertex? sourceVertex, TVertex? targetVertex)
    {
        if (sourceVertex == null || targetVertex == null)
        {
            return null;
        }

        Lock.EnterReadLock();
        try
        {
            return base.GetEdge(sourceVertex, targetVertex);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override TEdge? AddEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        Lock.EnterWriteLock();
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
            Lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        Lock.EnterWriteLock();
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
            Lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override bool AddVertex(TVertex vertex)
    {
        Lock.EnterWriteLock();
        try
        {
            if (base.AddVertex(vertex))
            {
                VertexSetModified();
                return true;
            }

            return false;
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override bool ContainsEdge(TVertex? sourceVertex, TVertex? targetVertex)
    {
        if (sourceVertex == null || targetVertex == null)
        {
            return false;
        }

        Lock.EnterReadLock();
        try
        {
            return base.ContainsEdge(sourceVertex, targetVertex);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override bool ContainsEdge(TEdge? edge)
    {
        if (edge == null)
        {
            return false;
        }

        Lock.EnterReadLock();
        try
        {
            return base.ContainsEdge(edge);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override bool ContainsVertex(TVertex? vertex)
    {
        if (vertex == null)
        {
            return false;
        }

        Lock.EnterReadLock();
        try
        {
            return base.ContainsVertex(vertex);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override int DegreeOf(TVertex vertex)
    {
        Lock.EnterReadLock();
        try
        {
            return base.DegreeOf(vertex);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgeSet()
    {
        return _allEdgesSet;
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgesOf(TVertex vertex)
    {
        Lock.EnterReadLock();
        try
        {
            return _cacheStrategy.EdgesOf(vertex);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override int InDegreeOf(TVertex vertex)
    {
        Lock.EnterReadLock();
        try
        {
            return base.InDegreeOf(vertex);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TVertex vertex)
    {
        Lock.EnterReadLock();
        try
        {
            return _cacheStrategy.IncomingEdgesOf(vertex);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TVertex vertex)
    {
        Lock.EnterReadLock();
        try
        {
            return base.OutDegreeOf(vertex);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> OutgoingEdgesOf(TVertex vertex)
    {
        Lock.EnterReadLock();
        try
        {
            return _cacheStrategy.OutgoingEdgesOf(vertex);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override bool RemoveAllEdges(IEnumerable<TEdge> edges)
    {
        Lock.EnterWriteLock();
        try
        {
            return base.RemoveAllEdges(edges);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> RemoveAllEdges(TVertex sourceVertex, TVertex targetVertex)
    {
        Lock.EnterWriteLock();
        try
        {
            return base.RemoveAllEdges(sourceVertex, targetVertex);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override bool RemoveAllVertices(IEnumerable<TVertex> vertices)
    {
        Lock.EnterWriteLock();
        try
        {
            return base.RemoveAllVertices(vertices);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override bool RemoveEdge(TEdge? edge)
    {
        if (edge == null)
        {
            return false;
        }

        Lock.EnterWriteLock();
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
            Lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override TEdge? RemoveEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        Lock.EnterWriteLock();
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
            Lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override bool RemoveVertex(TVertex? vertex)
    {
        Lock.EnterWriteLock();
        try
        {
            if (_cacheStrategy.RemoveVertex(vertex))
            {
                EdgeSetModified();
                VertexSetModified();
                return true;
            }

            return false;
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        Lock.EnterReadLock();
        try
        {
            return base.ToString();
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override ISet<TVertex> VertexSet()
    {
        return _allVerticesSet;
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeSource(TEdge edge)
    {
        Lock.EnterReadLock();
        try
        {
            return base.GetEdgeSource(edge);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeTarget(TEdge edge)
    {
        Lock.EnterReadLock();
        try
        {
            return base.GetEdgeTarget(edge);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override double GetEdgeWeight(TEdge edge)
    {
        Lock.EnterReadLock();
        try
        {
            return base.GetEdgeWeight(edge);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        Lock.EnterWriteLock();
        try
        {
            base.SetEdgeWeight(edge, weight);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Set the cache strategy for <c>edgesOf</c>, <c>incomingEdgesOf</c> and
    /// <c>outgoingEdgesOf</c> methods.
    /// </summary>
    /// <param name="cacheEnabled"> a flag whether to use cache for those methods, if <c>true</c>,
    ///        cache will be used for those methods, otherwise cache will not be used.</param>
    /// <returns>the AsSynchronizedGraph.</returns>
    public AsSynchronizedGraph<TVertex, TEdge> SetCache(bool cacheEnabled)
    {
        Lock.EnterWriteLock();
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
            Lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        Lock.EnterReadLock();
        try
        {
            return Delegate.GetHashCode();
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (this == obj)
        {
            return true;
        }

        Lock.EnterReadLock();
        try
        {
            return Delegate.Equals(obj);
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Create a unmodifiable copy of the set.
    /// </summary>
    /// <param name="set"> the set to be copied.</param>
    /// <returns>a unmodifiable copy of the set.</returns>
    private ISet<TC> CopySet<TC>(ISet<TC> set)
    {
        return new Java2Net.LinkedHashSet<TC>(set).AsReadOnly();
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
    /// <typeparam name="TElement">the class of the objects in the set.</typeparam>
    ///
    /// <remarks>Author: CHEN Kui.</remarks>
    private sealed class CopyOnDemandSet<TElement> : ISet<TElement>
    {
        private const string Unmodifiable = "this set is unmodifiable";

        /// <summary>
        /// Backing set.
        /// </summary>
        private readonly ISet<TElement> _set;

        /// <summary>
        /// When this flag is set, the backing set is used directly rather than a copy.
        /// </summary>
        private readonly bool _copyless;

        private readonly ReaderWriterLockSlim _lock;

        /// <summary>
        /// Backing set's unmodifiable copy. If null, needs to be recomputed on next access.
        /// </summary>
        private volatile ISet<TElement>? _copy;

        /// <summary>
        /// Constructor for CopyOnDemandSet.
        /// </summary>
        /// <param name="s"> the backing set.</param>
        /// <param name="lock"> the ReadWriteLock on which to locked.</param>
        /// <param name="copyless"> whether copyless mode should be used.</param>
        internal CopyOnDemandSet(ISet<TElement> s, ReaderWriterLockSlim @lock, bool copyless)
        {
            ArgumentNullException.ThrowIfNull(s);
            _set      = s;
            _copy     = null;
            _lock     = @lock;
            _copyless = copyless;
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _set.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public bool IsReadOnly => true;

        public bool Copyless => _copyless;

        /// <summary>
        /// Get the backing set's unmodifiable copy, or a direct reference to the backing set if in
        /// copyless mode.
        /// </summary>
        /// <returns>the backing set or its unmodifiable copy.</returns>
        [SuppressMessage("Design",       "MA0064:Avoid locking on publicly accessible instance")]
        [SuppressMessage("Reliability",  "CA2002:Do not lock on objects with weak identity")]
        [SuppressMessage("Critical Bug", "S2551:Shared resources should not be used for locking")]
        internal ISet<TElement> Copy
        {
            get
            {
                if (_copyless)
                {
                    return _set;
                }

                _lock.EnterReadLock();
                try
                {
                    var tempCopy = _copy;
                    if (tempCopy == null)
                    {
                        lock (this)
                        {
                            tempCopy = _copy;
                            if (tempCopy == null)
                            {
                                _copy = tempCopy = new Java2Net.LinkedHashSet<TElement>(_set);
                            }
                        }
                    }

                    return tempCopy;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// If the backing set is modified, call this method to let this set knows the backing set's
        /// copy need to update.
        /// </summary>
        public void Modified()
        {
            _copy = null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an iterator over the elements in the backing set's unmodifiable copy. The
        /// elements are returned in the same order of the backing set.
        /// </summary>
        /// <returns>an iterator over the elements in the backing set's unmodifiable copy.</returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            return Copy.GetEnumerator();
        }

        void ICollection<TElement>.Add(TElement item)
        {
            throw new NotSupportedException(Unmodifiable);
        }

        public bool Contains(TElement item)
        {
            _lock.EnterReadLock();
            try
            {
                return _set.Contains(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void CopyTo(TElement[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                _set.CopyTo(array, arrayIndex);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Remove(TElement item)
        {
            throw new NotSupportedException(Unmodifiable);
        }

        public void ExceptWith(IEnumerable<TElement> other)
        {
            throw new NotSupportedException(Unmodifiable);
        }

        public void IntersectWith(IEnumerable<TElement> other)
        {
            throw new NotSupportedException(Unmodifiable);
        }

        public bool IsProperSubsetOf(IEnumerable<TElement> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _set.IsProperSubsetOf(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool IsProperSupersetOf(IEnumerable<TElement> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _set.IsProperSupersetOf(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool IsSubsetOf(IEnumerable<TElement> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _set.IsSubsetOf(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool IsSupersetOf(IEnumerable<TElement> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _set.IsSupersetOf(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Overlaps(IEnumerable<TElement> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _set.Overlaps(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool SetEquals(IEnumerable<TElement> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _set.SetEquals(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void SymmetricExceptWith(IEnumerable<TElement> other)
        {
            throw new NotSupportedException(Unmodifiable);
        }

        public void UnionWith(IEnumerable<TElement> other)
        {
            throw new NotSupportedException(Unmodifiable);
        }

        public void Clear()
        {
            throw new NotSupportedException(Unmodifiable);
        }

        public bool Add(TElement item)
        {
            throw new NotSupportedException(Unmodifiable);
        }

        /// <summary>
        /// Compares the specified object with this set for equality.
        /// </summary>
        /// <param name="obj"> object to be compared for equality with this set.</param>
        /// <returns><c>true</c> if o and this set are the same object or o is equal to the
        ///         backing object, false otherwise.</returns>
        public override bool Equals(object? obj)
        {
            if (this == obj)
            {
                return true;
            }

            _lock.EnterReadLock();
            try
            {
                return _set.Equals(obj);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Return the backing set's hashcode.
        /// </summary>
        /// <returns>the backing set's hashcode.</returns>
        public override int GetHashCode()
        {
            _lock.EnterReadLock();
            try
            {
                return _set.GetHashCode();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Return the backing set's toString result.
        /// </summary>
        /// <returns>the backing set's toString result.</returns>
        public override string ToString()
        {
            _lock.EnterReadLock();
            try
            {
                return _set.ToString() ?? "null";
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// An interface for cache strategy of AsSynchronizedGraph's <c>edgesOf</c>,
    /// <c>incomingEdgesOf</c> and <c>outgoingEdgesOf</c> methods.
    /// </summary>
    private interface ICacheStrategy<TV, TE>
        where TV : class
        where TE : class
    {
        /// <summary>
        /// Add an edge into AsSynchronizedGraph's backing graph.
        /// </summary>
        /// <returns>Created edge.</returns>
        TE? AddEdge(TV sourceVertex, TV targetVertex);

        /// <summary>
        /// Add an edge into AsSynchronizedGraph's backing graph.
        /// </summary>
        /// <returns>true if edge was added successfully, false otherwise.</returns>
        bool AddEdge(TV sourceVertex, TV targetVertex, TE edge);

        /// <summary>
        /// Get all edges touching the specified vertex in AsSynchronizedGraph's backing graph.
        /// </summary>
        /// <returns>Set of edges incident to the <paramref name="vertex"/>.</returns>
        ISet<TE> EdgesOf(TV vertex);

        /// <summary>
        /// Get a set of all edges in AsSynchronizedGraph's backing graph incoming into the specified
        /// vertex.
        /// </summary>
        /// <returns>Set of incoming edges incident to the <paramref name="vertex"/>.</returns>
        ISet<TE> IncomingEdgesOf(TV vertex);

        /// <summary>
        /// Get a set of all edges in AsSynchronizedGraph's backing graph outgoing from the specified
        /// vertex.
        /// </summary>
        /// <returns>Set of outgoing edges incident to the <paramref name="vertex"/>.</returns>
        ISet<TE> OutgoingEdgesOf(TV vertex);

        /// <summary>
        /// Remove the specified edge from AsSynchronizedGraph's backing graph.
        /// </summary>
        /// <returns>true if edge was removed successfully, false otherwise.</returns>
        bool RemoveEdge(TE edge);

        /// <summary>
        /// Remove an edge from AsSynchronizedGraph's backing graph.
        /// </summary>
        /// <returns>Removed edge if such was, null otherwise.</returns>
        TE? RemoveEdge(TV sourceVertex, TV targetVertex);

        /// <summary>
        /// Remove the specified vertex from AsSynchronizedGraph's backing graph.
        /// </summary>
        /// <returns>true if vertex was removed successfully, false otherwise.</returns>
        bool RemoveVertex(TV? vertex);

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
    private class NoCache : ICacheStrategy<TVertex, TEdge>
    {
        private readonly AsSynchronizedGraph<TVertex, TEdge> _owner;

        public NoCache(AsSynchronizedGraph<TVertex, TEdge> owner)
        {
            _owner = owner;
        }

        /// <inheritdoc/>
        public TEdge? AddEdge(TVertex sourceVertex, TVertex targetVertex)
        {
            return _owner.AddEdge(sourceVertex, targetVertex);
        }

        /// <inheritdoc/>
        public bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
        {
            return _owner.AddEdge(sourceVertex, targetVertex, edge);
        }

        /// <inheritdoc/>
        public virtual ISet<TEdge> EdgesOf(TVertex vertex)
        {
            return _owner.CopySet(_owner.EdgesOf(vertex));
        }

        /// <inheritdoc/>
        public virtual ISet<TEdge> IncomingEdgesOf(TVertex vertex)
        {
            return _owner.CopySet(_owner.IncomingEdgesOf(vertex));
        }

        /// <inheritdoc/>
        public virtual ISet<TEdge> OutgoingEdgesOf(TVertex vertex)
        {
            return _owner.CopySet(_owner.OutgoingEdgesOf(vertex));
        }

        /// <inheritdoc/>
        public bool RemoveEdge(TEdge edge)
        {
            return _owner.RemoveEdge(edge);
        }

        /// <inheritdoc/>
        public TEdge? RemoveEdge(TVertex sourceVertex, TVertex targetVertex)
        {
            return _owner.RemoveEdge(sourceVertex, targetVertex);
        }

        /// <inheritdoc/>
        public bool RemoveVertex(TVertex? vertex)
        {
            return _owner.RemoveVertex(vertex);
        }

        /// <inheritdoc/>
        public bool CacheEnabled => false;
    }

    /// <summary>
    /// Disable cache as per <c>NoCache</c>, and also don't produce copies; instead, just
    /// directly return the results from the underlying graph. This requires the caller to explicitly
    /// synchronize iterations over these collections.
    /// </summary>
    private sealed class NoCopy : NoCache
    {
        private readonly AsSynchronizedGraph<TVertex, TEdge> _owner;

        public NoCopy(AsSynchronizedGraph<TVertex, TEdge> owner)
            : base(owner)
        {
            _owner = owner;
        }

        /// <inheritdoc/>
        public override ISet<TEdge> EdgesOf(TVertex vertex)
        {
            return _owner.EdgesOf(vertex);
        }

        /// <inheritdoc/>
        public override ISet<TEdge> IncomingEdgesOf(TVertex vertex)
        {
            return _owner.IncomingEdgesOf(vertex);
        }

        /// <inheritdoc/>
        public override ISet<TEdge> OutgoingEdgesOf(TVertex vertex)
        {
            return _owner.OutgoingEdgesOf(vertex);
        }
    }

    /// <summary>
    /// Use cache for AsSynchronizedGraph's <c>edgesOf</c>, <c>incomingEdgesOf</c> and
    /// <c>outgoingEdgesOf</c> methods.
    /// </summary>
    private sealed class CacheAccess : ICacheStrategy<TVertex, TEdge>
    {
        private readonly AsSynchronizedGraph<TVertex, TEdge> _owner;

        public CacheAccess(AsSynchronizedGraph<TVertex, TEdge> owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// A map caching for incomingEdges operation.
        /// </summary>
        private readonly IDictionary<TVertex, ISet<TEdge>> _incomingEdgesMap =
            new ConcurrentDictionary<TVertex, ISet<TEdge>>();

        /// <summary>
        /// A map caching for outgoingEdges operation.
        /// </summary>
        private readonly IDictionary<TVertex, ISet<TEdge>> _outgoingEdgesMap =
            new ConcurrentDictionary<TVertex, ISet<TEdge>>();

        /// <summary>
        /// A map caching for edgesOf operation.
        /// </summary>
        private readonly IDictionary<TVertex, ISet<TEdge>> _edgesOfMap =
            new ConcurrentDictionary<TVertex, ISet<TEdge>>();

        /// <inheritdoc/>
        public TEdge? AddEdge(TVertex sourceVertex, TVertex targetVertex)
        {
            var edge = _owner.AddEdge(sourceVertex, targetVertex);
            if (edge != null)
            {
                EdgeModified(sourceVertex, targetVertex);
            }

            return edge;
        }

        /// <inheritdoc/>
        public bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
        {
            if (_owner.AddEdge(sourceVertex, targetVertex, edge))
            {
                EdgeModified(sourceVertex, targetVertex);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public ISet<TEdge> EdgesOf(TVertex vertex)
        {
            if (_edgesOfMap.TryGetValue(vertex, out var s))
            {
                return s;
            }

            s = _owner.CopySet(_owner.EdgesOf(vertex));

            _edgesOfMap[vertex] = s;
            return s;
        }

        /// <inheritdoc/>
        public ISet<TEdge> IncomingEdgesOf(TVertex vertex)
        {
            if (_incomingEdgesMap.TryGetValue(vertex, out var s))
            {
                return s;
            }

            s = _owner.CopySet(_owner.IncomingEdgesOf(vertex));

            _incomingEdgesMap[vertex] = s;
            return s;
        }

        /// <inheritdoc/>
        public ISet<TEdge> OutgoingEdgesOf(TVertex vertex)
        {
            if (_outgoingEdgesMap.TryGetValue(vertex, out var s))
            {
                return s;
            }

            s = _owner.CopySet(_owner.OutgoingEdgesOf(vertex));

            _outgoingEdgesMap[vertex] = s;
            return s;
        }

        /// <inheritdoc/>
        public bool RemoveEdge(TEdge edge)
        {
            var sourceVertex = _owner.GetEdgeSource(edge);
            var targetVertex = _owner.GetEdgeTarget(edge);
            if (_owner.RemoveEdge(edge))
            {
                EdgeModified(sourceVertex, targetVertex);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public TEdge? RemoveEdge(TVertex sourceVertex, TVertex targetVertex)
        {
            var edge = _owner.RemoveEdge(sourceVertex, targetVertex);
            if (edge != null)
            {
                EdgeModified(sourceVertex, targetVertex);
            }

            return edge;
        }

        /// <inheritdoc/>
        public bool RemoveVertex(TVertex? vertex)
        {
            if (_owner.RemoveVertex(vertex))
            {
                _edgesOfMap.Clear();
                _incomingEdgesMap.Clear();
                _outgoingEdgesMap.Clear();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clear the copies which the edge to be added or removed can affect.
        /// </summary>
        /// <param name="sourceVertex"> source vertex of the modified edge.</param>
        /// <param name="targetVertex"> target vertex of the modified edge.</param>
        internal void EdgeModified(TVertex sourceVertex, TVertex targetVertex)
        {
            _outgoingEdgesMap.Remove(sourceVertex);
            _incomingEdgesMap.Remove(targetVertex);
            _edgesOfMap.Remove(sourceVertex);
            _edgesOfMap.Remove(targetVertex);
            if (!_owner.Type.IsDirected)
            {
                _outgoingEdgesMap.Remove(targetVertex);
                _incomingEdgesMap.Remove(sourceVertex);
            }
        }

        /// <inheritdoc/>
        public bool CacheEnabled => true;
    }
}
