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
using System.Diagnostics.CodeAnalysis;
using NGraphT.Core.DotNetUtil;
using NGraphT.Core.Traverse;
using NGraphT.Core.Util;

/*
 * (C) Copyright 2008-2021, by Peter Giles and Contributors.
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

/// <summary>
/// A directed acyclic graph (DAG).
///
/// <para>
/// Implements a DAG that can be modified (vertices &amp; edges added and removed), is guaranteed to
/// remain acyclic, and provides fast topological order iteration. An attempt to add an edge which
/// would induce a cycle throws an <see cref="System.ArgumentException"/>.
/// </para>
///
/// <para>
/// This is done using a dynamic topological sort which is based on the algorithm described in "David
/// J. Pearce &amp; Paul H. J. Kelly. A dynamic topological sort algorithm for directed acyclic
/// graphs. Journal of Experimental Algorithmics, 11, 2007." (see
/// <a href="http://www.mcs.vuw.ac.nz/~djp/files/PK-JEA07.pdf">paper</a> or
/// <a href="http://doi.acm.org/10.1145/1187436.1210590">ACM link</a> for details). The
/// implementation differs from the algorithm specified in the above paper in some ways, perhaps most
/// notably in that the topological ordering is stored by default using two hash maps, which will
/// have some effects on the runtime, but also allow for vertex addition and removal. This storage
/// mechanism can be adjusted by subclasses.
/// </para>
///
/// <para>
/// The complexity of adding a new edge in the graph depends on the number of edges incident to the
/// "affected region", and should in general be faster than recomputing the whole topological
/// ordering from scratch. For details about the complexity parameters and running times, see the
/// previously mentioned paper.
/// </para>
///
/// <para>
/// This class makes no claims to thread safety, and concurrent usage from multiple threads will
/// produce undefined results.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Peter Giles.</remarks>
[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order")]
public sealed class DirectedAcyclicGraph<TVertex, TEdge> : AbstractBaseGraph<TVertex, TEdge>, IEnumerable<TVertex>
    where TVertex : class
    where TEdge : class
{
    private readonly IComparer<TVertex>     _topoComparator;
    private readonly ITopoOrderMap<TVertex> _topoOrderMap;

    /// <summary>
    /// The visited strategy factory to use. Subclasses can change this.
    /// </summary>
    private readonly IVisitedStrategyFactory _visitedStrategyFactory;

    private int _maxTopoIndex;
    private int _minTopoIndex;

    // this update count is used to keep internal topological iterators honest
    private long _topoModCount;

    /// <summary>
    /// Construct a directed acyclic graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier.</param>
    /// <param name="edgeSupplier"> the edge supplier.</param>
    /// <param name="weighted"> if true the graph will be weighted, otherwise not.</param>
    public DirectedAcyclicGraph(Func<TVertex>? vertexSupplier, Func<TEdge>? edgeSupplier, bool weighted)
        : this(
            vertexSupplier,
            edgeSupplier,
            new VisitedBitSetImpl(),
            new TopoVertexBiMap<TVertex>(),
            weighted,
            false,
            new FastLookupGraphSpecificsStrategy<TVertex, TEdge>()
        )
    {
    }

    /// <summary>
    /// Construct a directed acyclic graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier.</param>
    /// <param name="edgeSupplier"> the edge supplier.</param>
    /// <param name="visitedStrategyFactory">
    /// the visited strategy factory. Subclasses can change this
    /// implementation to adjust the performance tradeoffs.
    /// </param>
    /// <param name="topoOrderMap">
    /// the topological order map. For performance reasons, subclasses can change
    /// the way this class stores the topological order.
    /// </param>
    /// <param name="weighted"> if true the graph will be weighted, otherwise not.</param>
    /// <param name="allowMultipleEdges"> if true the graph will allow multiple edges, otherwise not.</param>
    /// <param name="graphSpecificsStrategy"> strategy for constructing low-level graph specifics.</param>
    internal DirectedAcyclicGraph(
        Func<TVertex>?                          vertexSupplier,
        Func<TEdge>?                            edgeSupplier,
        IVisitedStrategyFactory                 visitedStrategyFactory,
        ITopoOrderMap<TVertex>                  topoOrderMap,
        bool                                    weighted,
        bool                                    allowMultipleEdges,
        IGraphSpecificsStrategy<TVertex, TEdge> graphSpecificsStrategy
    )
        : base(
            vertexSupplier,
            edgeSupplier,
            BuildMyGraphType(weighted, allowMultipleEdges),
            graphSpecificsStrategy
        )
    {
        ArgumentNullException.ThrowIfNull(visitedStrategyFactory);
        ArgumentNullException.ThrowIfNull(topoOrderMap);

        _visitedStrategyFactory = visitedStrategyFactory;
        _topoOrderMap           = topoOrderMap;
        _topoComparator         = new TopoComparator(this);
    }

    public override TVertex AddVertex()
    {
        var v = base.AddVertex();

        // add to the topological map
        ++_maxTopoIndex;
        _topoOrderMap.PutVertex(_maxTopoIndex, v);
        ++_topoModCount;

        return v;
    }

    public override bool AddVertex(TVertex vertex)
    {
        var added = base.AddVertex(vertex);

        if (added)
        {
            // add to the topological map
            ++_maxTopoIndex;
            _topoOrderMap.PutVertex(_maxTopoIndex, vertex);
            ++_topoModCount;
        }

        return added;
    }

    public override bool RemoveVertex([NotNullWhen(true)] TVertex? vertex)
    {
        var removed = base.RemoveVertex(vertex);
        if (!removed)
        {
            return removed;
        }

        /*
         * Depending on the topoOrderMap implementation, this can leave holes in the topological
         * ordering, which can degrade performance for certain operations over time.
         */
        var topoIndex = _topoOrderMap.RemoveVertex(vertex!);

        // if possible contract minTopoIndex
        if (topoIndex.Equals(_minTopoIndex))
        {
            while (_minTopoIndex < 0 && _topoOrderMap.GetVertex(_minTopoIndex) == null)
            {
                ++_minTopoIndex;
            }
        }

        // if possible contract maxTopoIndex
        if (topoIndex.Equals(_maxTopoIndex))
        {
            while (_maxTopoIndex > 0 && _topoOrderMap.GetVertex(_maxTopoIndex) == null)
            {
                _maxTopoIndex--;
            }
        }

        ++_topoModCount;

        return removed;
    }

    /// <summary>
    /// <inheritdoc/>
    ///
    /// <para>
    /// The complexity of adding a new edge in the graph depends on the number of edges incident to
    /// the "affected region", and should in general be faster than recomputing the whole topological
    /// ordering from scratch.
    /// </para>
    /// </summary>
    ///
    /// <returns>The newly created edge if added to the graph, otherwise <c>null</c>.</returns>
    ///
    /// <exception cref="ArgumentException"> if the vertex is not in the graph.</exception>
    /// <exception cref="GraphCycleProhibitedException"> if the vertex would induce a cycle in the graph.</exception>
    public override TEdge? AddEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        AssertVertexExist(sourceVertex);
        AssertVertexExist(targetVertex);

        try
        {
            UpdateDag(sourceVertex, targetVertex);
            return base.AddEdge(sourceVertex, targetVertex);
        }
        catch (CycleFoundException)
        {
            throw new GraphCycleProhibitedException();
        }
    }

    /// <summary>
    /// <inheritdoc/>
    ///
    /// <para>
    /// The complexity of adding a new edge in the graph depends on the number of edges incident to
    /// the "affected region", and should in general be faster than recomputing the whole topological
    /// ordering from scratch.
    /// </para>
    /// </summary>
    ///
    /// <returns><c>true</c> if this graph did not already contain the specified edge.</returns>
    ///
    /// <exception cref="ArgumentException"> if the vertex is not in the graph.</exception>
    /// <exception cref="GraphCycleProhibitedException"> if the vertex would induce a cycle in the graph.</exception>
    public override bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);

        if (ContainsEdge(edge))
        {
            return false;
        }

        AssertVertexExist(sourceVertex);
        AssertVertexExist(targetVertex);

        try
        {
            UpdateDag(sourceVertex, targetVertex);
            return base.AddEdge(sourceVertex, targetVertex, edge);
        }
        catch (CycleFoundException)
        {
            throw new GraphCycleProhibitedException();
        }
    }

    /// <summary>
    /// Get the ancestors of a vertex.
    /// </summary>
    /// <param name="vertex"> the vertex to get the ancestors of.</param>
    /// <returns>Set of ancestors of a vertex.</returns>
    public ISet<TVertex> GetAncestors(TVertex vertex)
    {
        var reversedGraph = new EdgeReversedGraph<TVertex, TEdge>(this);
        var ancestors     = new HashSet<TVertex>();

        using var iterator = new DepthFirstIterator<TVertex, TEdge>(reversedGraph, vertex);

        // Do not add start vertex to result.
        iterator.MoveNext();

        while (iterator.MoveNext())
        {
            ancestors.Add(iterator.Current);
        }

        return ancestors;
    }

    /// <summary>
    /// Get the descendants of a vertex.
    /// </summary>
    /// <param name="vertex"> the vertex to get the descendants of.</param>
    /// <returns>Set of descendants of a vertex.</returns>
    public ISet<TVertex> GetDescendants(TVertex vertex)
    {
        var       descendants = new HashSet<TVertex>();
        using var iterator    = new DepthFirstIterator<TVertex, TEdge>(this, vertex);

        // Do not add start vertex to result.
        iterator.MoveNext();

        while (iterator.MoveNext())
        {
            descendants.Add(iterator.Current);
        }

        return descendants;
    }

    /// <summary>
    /// Returns a topological order iterator.
    /// </summary>
    /// <returns>a topological order iterator.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Returns a topological order iterator.
    /// </summary>
    /// <returns>a topological order iterator.</returns>
    public IEnumerator<TVertex> GetEnumerator()
    {
        return new TopoIterator(this);
    }

    private static DefaultGraphType BuildMyGraphType(bool weighted, bool allowMultipleEdges)
    {
        return new DefaultGraphType.Builder()
            .Directed()
            .AllowMultipleEdges(allowMultipleEdges)
            .AllowSelfLoops(false)
            .Weighted(weighted)
            .AllowCycles(false)
            .Build();
    }

    /// <summary>
    /// Update as if a new edge is added.
    /// </summary>
    /// <param name="sourceVertex"> the source vertex.</param>
    /// <param name="targetVertex"> the target vertex.</param>
    private void UpdateDag(TVertex sourceVertex, TVertex targetVertex)
    {
        var lb = _topoOrderMap.GetTopologicalIndex(targetVertex);
        var ub = _topoOrderMap.GetTopologicalIndex(sourceVertex);

        if (!(lb < ub))
        {
            return;
        }

        var df = new HashSet<TVertex>();
        var db = new HashSet<TVertex>();

        // discovery
        var affectedRegion = new Region(lb.Value, ub.Value);
        var visited        = _visitedStrategyFactory.GetVisitedStrategy(affectedRegion);

        // throws CycleFoundException if there is a cycle
        DfsF(targetVertex, df, visited, affectedRegion);
        DfsB(sourceVertex, db, visited, affectedRegion);
        Reorder(df, db, visited);

        // if we do a reorder, then the topology has been updated
        ++_topoModCount;
    }

    /// <summary>
    /// Depth first search forward, building up the set (df) of forward-connected vertices in the
    /// Affected Region.
    /// </summary>
    ///
    /// <param name="initialVertex"> the vertex being visited.</param>
    /// <param name="df"> the set we are populating with forward connected vertices in the Affected Region.</param>
    /// <param name="visited">
    /// a simple data structure that lets us know if we already visited a vertex with a given topo index.
    /// </param>
    /// <param name="affectedRegion">part of the grpah to perform dfs in.</param>
    ///
    /// <exception cref="CycleFoundException"> if a cycle is discovered.</exception>
    private void DfsF(TVertex initialVertex, ISet<TVertex> df, IVisitedStrategy visited, Region affectedRegion)
    {
        var vertices = new LinkedList<TVertex>();
        vertices.AddFirst(initialVertex);

        while (vertices.Count > 0)
        {
            var vertex = vertices.First!.Value;
            vertices.RemoveFirst();

            var topoIndex = _topoOrderMap.GetTopologicalIndex(vertex)!.Value;

            if (visited.GetVisited(topoIndex))
            {
                continue;
            }

            // Assumption: vertex is in the AR and so it will be in visited
            visited.SetVisited(topoIndex);

            df.Add(vertex);

            foreach (var outEdge in OutgoingEdgesOf(vertex))
            {
                var nextVertex          = GetEdgeTarget(outEdge);
                var nextVertexTopoIndex = _topoOrderMap.GetTopologicalIndex(nextVertex)!.Value;

                if (nextVertexTopoIndex == affectedRegion.Finish)
                {
                    // reset visited
                    try
                    {
                        foreach (var visitedVertex in df)
                        {
                            visited.ClearVisited(_topoOrderMap.GetTopologicalIndex(visitedVertex)!.Value);
                        }
                    }
                    catch (NotSupportedException)
                    {
                        // okay, fine, some implementations (ones that automatically
                        // reset themselves out) don't work this way
                    }

                    throw new CycleFoundException();
                }

                /*
                 * Note, order of checks is important as we need to make sure the vertex is in the
                 * affected region before we check its visited status (otherwise we will be causing
                 * an ArrayIndexOutOfBoundsException).
                 */
                if (affectedRegion.IsIn(nextVertexTopoIndex) && !visited.GetVisited(nextVertexTopoIndex))
                {
                    vertices.AddFirst(nextVertex); // recurse
                }
            }
        }
    }

    /// <summary>
    /// Depth first search backward, building up the set (db) of back-connected vertices in the
    /// Affected Region.
    /// </summary>
    ///
    /// <param name="initialVertex">the vertex being visited.</param>
    /// <param name="db">the set we are populating with back-connected vertices in the AR.</param>
    /// <param name="visited">a strategy for marking vertices as visited.</param>
    /// <param name="affectedRegion">part of the grpah to perform dfs in.</param>
    private void DfsB(TVertex initialVertex, ISet<TVertex> db, IVisitedStrategy visited, Region affectedRegion)
    {
        var vertices = new LinkedList<TVertex>();
        vertices.AddFirst(initialVertex);

        while (vertices.Count > 0)
        {
            var vertex = vertices.First!.Value;
            vertices.RemoveFirst();

            // Assumption: vertex is in the AR and so we will get a topoIndex from the map
            var topoIndex = _topoOrderMap.GetTopologicalIndex(vertex)!.Value;

            if (visited.GetVisited(topoIndex))
            {
                continue;
            }

            visited.SetVisited(topoIndex);

            db.Add(vertex);

            foreach (var inEdge in IncomingEdgesOf(vertex))
            {
                var previousVertex          = GetEdgeSource(inEdge);
                var previousVertexTopoIndex = _topoOrderMap.GetTopologicalIndex(previousVertex)!.Value;

                /*
                 * Note, order of checks is important as we need to make sure the vertex is in the
                 * affected region before we check its visited status (otherwise we will be causing
                 * an ArrayIndexOutOfBoundsException).
                 */
                if (affectedRegion.IsIn(previousVertexTopoIndex) &&
                    !visited.GetVisited(previousVertexTopoIndex))
                {
                    // if previousVertexTopoIndex != null, the vertex is in the
                    // Affected Region according to our topoIndexMap
                    vertices.AddFirst(previousVertex);
                }
            }
        }
    }

    private void Reorder(ISet<TVertex> df, ISet<TVertex> db, IVisitedStrategy visited)
    {
        var topoDf = new List<TVertex>(df);
        var topoDb = new List<TVertex>(db);

        topoDf.Sort(_topoComparator);
        topoDb.Sort(_topoComparator);

        // merge these suckers together in topological order
        var availableTopoIndices = new SortedSet<int>();

        // we have to cast to the generic type, can't do "new TVertex[size]" in java 5;
        var bigL   = (TVertex[])new object[df.Count + db.Count];
        var lIndex = 0; // this index is used for the sole purpose of pushing into

        // the correct index of bigL
        // assume (for now) that we are resetting visited
        var clearVisited = true;

        foreach (var vertex in topoDb)
        {
            var topoIndex = _topoOrderMap.GetTopologicalIndex(vertex)!.Value;

            // add the available indices to the set
            availableTopoIndices.Add(topoIndex);

            bigL[lIndex++] = vertex;

            if (clearVisited)
            {
                // reset visited status if supported
                try
                {
                    visited.ClearVisited(topoIndex);
                }
                catch (NotSupportedException)
                {
                    clearVisited = false;
                }
            }
        }

        foreach (var vertex in topoDf)
        {
            var topoIndex = _topoOrderMap.GetTopologicalIndex(vertex)!.Value;

            // add the available indices to the set
            availableTopoIndices.Add(topoIndex);
            bigL[lIndex++] = vertex;

            if (clearVisited)
            {
                // reset visited status if supported
                try
                {
                    visited.ClearVisited(topoIndex);
                }
                catch (NotSupportedException)
                {
                    clearVisited = false;
                }
            }
        }

        lIndex = 0; // reusing lIndex
        foreach (var topoIndex in availableTopoIndices)
        {
            // assign the indexes to the elements of bigL in order
            var vertex = bigL[lIndex++]; // note the post-increment
            _topoOrderMap.PutVertex(topoIndex, vertex);
        }
    }

    /// <summary>
    /// Exception used in dfsF when a cycle is found.
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    /// </summary>
    public sealed class CycleFoundException : Exception
    {
    }

    /// <summary>
    /// An interface for storing the topological ordering.
    /// </summary>
    ///
    /// <typeparam name="TV">the graph vertex type.</typeparam>
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    internal interface ITopoOrderMap<TV>
    {
        /// <summary>
        /// Add a vertex at the given topological index.
        /// </summary>
        /// <param name="index"> the topological index.</param>
        /// <param name="vertex"> the vertex.</param>
        void PutVertex(int index, TV vertex);

        /// <summary>
        /// Get the vertex at the given topological index.
        /// </summary>
        /// <param name="index"> the topological index.</param>
        /// <returns>vertex the vertex.</returns>
        TV? GetVertex(int index);

        /// <summary>
        /// Get the topological index of the given vertex.
        /// </summary>
        /// <param name="vertex"> the vertex.</param>
        /// <returns>
        /// the index that the vertex is at, or null if the vertex isn't in the topological ordering.
        /// </returns>
        int? GetTopologicalIndex(TV vertex);

        /// <summary>
        /// Remove the given vertex from the topological ordering.
        /// </summary>
        /// <param name="vertex"> the vertex.</param>
        /// <returns>
        /// the index that the vertex was at, or null if the vertex wasn't in the topological ordering.
        /// </returns>
        int? RemoveVertex(TV vertex);

        /// <summary>
        /// Remove all vertices from the topological ordering.
        /// </summary>
        void RemoveAllVertices();
    }

    /// <summary>
    /// A strategy for marking vertices as visited.
    ///
    /// <para>
    /// Vertices are indexed by their topological index, to avoid using the vertex type in the
    /// interface.
    /// </para>
    /// </summary>
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    internal interface IVisitedStrategy
    {
        /// <summary>
        /// Mark the given topological index as visited.
        /// </summary>
        /// <param name="index"> the topological index.</param>
        void SetVisited(int index);

        /// <summary>
        /// Get if the given topological index has been visited.
        /// </summary>
        /// <param name="index"> the topological index.</param>
        /// <returns>true if the given topological index has been visited, false otherwise.</returns>
        bool GetVisited(int index);

        /// <summary>
        /// Clear the visited state of the given topological index.
        /// </summary>
        ///
        /// <param name="index"> the index.</param>
        ///
        /// <exception cref="NotSupportedException">
        /// if the implementation doesn't support (or doesn't need) clearance.
        /// For example, if the factory creates a new instance every time,
        /// it is a waste of cycles to reset the state after the search of the Affected
        /// Region is done, so an UnsupportedOperationException *should* be thrown.
        /// </exception>
        void ClearVisited(int index);
    }

    /// <summary>
    /// A visited strategy factory.
    /// </summary>
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    internal interface IVisitedStrategyFactory
    {
        /// <summary>
        /// Create a new instance of <see cref="IVisitedStrategy"/>.
        /// </summary>
        /// <param name="affectedRegion"> the affected region.</param>
        /// <returns>a new instance of <see cref="IVisitedStrategy"/> for the affected region.</returns>
        IVisitedStrategy GetVisitedStrategy(Region affectedRegion);
    }

    /// <summary>
    /// A dual map implementation of the topological order map.
    /// </summary>
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    internal sealed class TopoVertexBiMap<TV> : ITopoOrderMap<TV>
        where TV : class
    {
        private readonly Dictionary<int, TV> _topoToVertex = new();
        private readonly Dictionary<TV, int> _vertexToTopo = new();

        public void PutVertex(int index, TV vertex)
        {
            _topoToVertex[index]  = vertex;
            _vertexToTopo[vertex] = index;
        }

        public TV GetVertex(int index)
        {
            return _topoToVertex[index];
        }

        public int? GetTopologicalIndex(TV vertex)
        {
            return _vertexToTopo.GetOrDefault(vertex);
        }

        public int? RemoveVertex(TV vertex)
        {
            if (_vertexToTopo.Remove(vertex, out var topoIndex))
            {
                _topoToVertex.Remove(topoIndex);
            }

            return topoIndex;
        }

        public void RemoveAllVertices()
        {
            _vertexToTopo.Clear();
            _topoToVertex.Clear();
        }
    }

    /// <summary>
    /// An implementation of the topological order map which for performance and flexibility uses an
    /// ArrayList for topological index to vertex mapping, and a HashMap for vertex to topological
    /// index mapping.
    /// </summary>
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    internal sealed class TopoVertexMap : ITopoOrderMap<TVertex>
    {
        private readonly List<TVertex?>           _topoToVertex = new();
        private readonly Dictionary<TVertex, int> _vertexToTopo = new();

        public void PutVertex(int index, TVertex vertex)
        {
            var translatedIndex = TranslateIndex(index);

            // grow topoToVertex as needed to accommodate elements
            while (translatedIndex + 1 > _topoToVertex.Count)
            {
                _topoToVertex.Add(null);
            }

            _topoToVertex[translatedIndex] = vertex;
            _vertexToTopo[vertex]          = index;
        }

        public TVertex? GetVertex(int index)
        {
            return _topoToVertex[TranslateIndex(index)];
        }

        public int? GetTopologicalIndex(TVertex vertex)
        {
            return _vertexToTopo.GetOrDefault(vertex);
        }

        public int? RemoveVertex(TVertex vertex)
        {
            if (_vertexToTopo.Remove(vertex, out var topoIndex))
            {
                _topoToVertex[TranslateIndex(topoIndex)] = null;
            }

            return topoIndex;
        }

        public void RemoveAllVertices()
        {
            _vertexToTopo.Clear();
            _topoToVertex.Clear();
        }

        /// <summary>
        /// We translate the topological index to an ArrayList index. We have to do this because
        /// topological indices can be negative, and we want to do it because we can make better use
        /// of space by only needing an ArrayList of size |AR|.
        /// </summary>
        /// <returns>the ArrayList index.</returns>
        private int TranslateIndex(int index)
        {
            if (index >= 0)
            {
                return 2 * index;
            }

            return -1 * ((index * 2) - 1);
        }
    }

    /// <summary>
    /// An inclusive range of indices: [start, finish].
    /// </summary>
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    internal sealed class Region
    {
        /// <summary>
        /// Construct a new region.
        /// </summary>
        /// <param name="start"> the start of the region.</param>
        /// <param name="finish"> the end of the region (inclusive).</param>
        public Region(int start, int finish)
        {
            if (start > finish)
            {
                throw new ArgumentException("(start > finish): invariant broken", nameof(start));
            }

            Start  = start;
            Finish = finish;
        }

        /// <summary>
        /// Get the start of the region.
        /// </summary>
        /// <returns>the start of the region.</returns>
        public int Start { get; }

        /// <summary>
        /// Get the end of the region (inclusive).
        /// </summary>
        /// <returns>the end of the region (inclusive).</returns>
        public int Finish { get; }

        /// <summary>
        /// Get the size of the region.
        /// </summary>
        /// <returns>the size of the region.</returns>
        public int Size => Finish - Start + 1;

        /// <summary>
        /// Check if index is in the region.
        /// </summary>
        /// <param name="index"> the index to check.</param>
        /// <returns>true if the index is in the region, false otherwise.</returns>
        public bool IsIn(int index)
        {
            return index >= Start && index <= Finish;
        }
    }

    /// <summary>
    /// A visited strategy which uses a <see cref="System.Collections.BitArray"/>.
    ///
    /// <para>
    /// This implementation is close to the performance of <see cref="VisitedArrayListImpl"/>, with 1/8 the
    /// memory usage.
    /// </para>
    /// </summary>
    ///
    /// <remarks>Author: John V. Sichi.</remarks>
    internal sealed class VisitedBitSetImpl : IVisitedStrategy, IVisitedStrategyFactory
    {
        private readonly BitArray _visited = new(0);

        private Region _affectedRegion = null!;

        public IVisitedStrategy GetVisitedStrategy(Region affectedRegion)
        {
            _visited.Length = affectedRegion.Size;
            _affectedRegion = affectedRegion;
            return this;
        }

        public void SetVisited(int index)
        {
            _visited.Set(TranslateIndex(index), true);
        }

        public bool GetVisited(int index)
        {
            return _visited.Get(TranslateIndex(index));
        }

        public void ClearVisited(int index)
        {
            _visited.Set(TranslateIndex(index), false);
        }

        /// <summary>
        /// We translate the topological index to an ArrayList index. We have to do this because
        /// topological indices can be negative, and we want to do it because we can make better use
        /// of space by only needing an ArrayList of size |AR|.
        /// </summary>
        /// <returns>the ArrayList index.</returns>
        private int TranslateIndex(int index)
        {
            return index - _affectedRegion.Start;
        }
    }

    /// <summary>
    /// A visited strategy using an <see cref="System.Collections.ArrayList"/>.
    ///
    /// <para>
    /// This implementation seems to offer the best performance in most cases. It grows the internal
    /// ArrayList as needed to be as large as |AR|, so it will be more memory intensive than the
    /// HashSet implementation, and unlike the Array implementation, it will hold on to that memory
    /// (it expands, but never contracts).
    /// </para>
    /// </summary>
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    internal sealed class VisitedArrayListImpl : IVisitedStrategy, IVisitedStrategyFactory
    {
        private readonly List<bool> _visited = new();

        private Region _affectedRegion = null!;

        public IVisitedStrategy GetVisitedStrategy(Region affectedRegion)
        {
            // Make sure visited is big enough
            _visited.EnsureCapacity(affectedRegion.Size);
            _affectedRegion = affectedRegion;
            return this;
        }

        public void SetVisited(int index)
        {
            _visited[TranslateIndex(index)] = true;
        }

        public bool GetVisited(int index)
        {
            return _visited[TranslateIndex(index)];
        }

        public void ClearVisited(int index)
        {
            _visited[TranslateIndex(index)] = false;
        }

        /// <summary>
        /// We translate the topological index to an ArrayList index. We have to do this because
        /// topological indices can be negative, and we want to do it because we can make better use
        /// of space by only needing an ArrayList of size |AR|.
        /// </summary>
        /// <returns>the ArrayList index.</returns>
        private int TranslateIndex(int index)
        {
            return index - _affectedRegion.Start;
        }
    }

    /// <summary>
    /// A visited strategy using a <see cref="HashSet{T}"/>.
    ///
    /// <para>
    /// This implementation doesn't seem to perform as well, though I can imagine circumstances where
    /// it should shine (lots and lots of vertices). It also should have the lowest memory footprint
    /// as it only uses storage for indices that have been visited.
    /// </para>
    /// </summary>
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    internal sealed class VisitedHashSetImpl : IVisitedStrategy, IVisitedStrategyFactory
    {
        private readonly HashSet<int> _visited = new();

        public IVisitedStrategy GetVisitedStrategy(Region affectedRegion)
        {
            _visited.Clear();
            return this;
        }

        public void SetVisited(int index)
        {
            _visited.Add(index);
        }

        public bool GetVisited(int index)
        {
            return _visited.Contains(index);
        }

        public void ClearVisited(int index)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// A visited strategy using an array.
    ///
    /// <para>
    /// This implementation, somewhat to my surprise, is slower than the ArrayList version, probably
    /// due to its reallocation of the underlying array for every topology reorder that is required.
    /// </para>
    /// </summary>
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    internal sealed class VisitedArrayImpl : IVisitedStrategy, IVisitedStrategyFactory
    {
        private readonly bool[] _visited;
        private readonly Region _affectedRegion;

        /// <summary>
        /// Construct an empty instance for a region.
        /// </summary>
        /// <param name="affectedRegion"> the region.</param>
        public VisitedArrayImpl(Region? affectedRegion)
        {
            if (affectedRegion == null)
            {
                // make empty instance for fectory usage only
                _visited        = null!;
                _affectedRegion = null!;
            }
            else
            {
                // fill in the needed pieces
                _affectedRegion = affectedRegion;

                // initialized to all false by default
                _visited = new bool[affectedRegion.Size];
            }
        }

        public IVisitedStrategy GetVisitedStrategy(Region affectedRegion)
        {
            return new VisitedArrayImpl(affectedRegion);
        }

        public void SetVisited(int index)
        {
            _visited[index - _affectedRegion.Start] = true;
        }

        public bool GetVisited(int index)
        {
            return _visited[index - _affectedRegion.Start];
        }

        public void ClearVisited(int index)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Comparator for vertices based on their topological ordering.
    /// </summary>
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    private sealed class TopoComparator : IComparer<TVertex>
    {
        private readonly DirectedAcyclicGraph<TVertex, TEdge> _graph;

        public TopoComparator(DirectedAcyclicGraph<TVertex, TEdge> graph)
        {
            _graph = graph;
        }

        public int Compare(TVertex? x, TVertex? y)
        {
            var xTopoIndex = _graph._topoOrderMap.GetTopologicalIndex(x!)!.Value;
            var yTopoIndex = _graph._topoOrderMap.GetTopologicalIndex(y!)!.Value;
            return xTopoIndex.CompareTo(yTopoIndex);
        }
    }

    /// <summary>
    /// An iterator which follows topological order.
    /// </summary>
    ///
    /// <remarks>Author: Peter Giles.</remarks>
    private sealed class TopoIterator : IEnumerator<TVertex>
    {
        private readonly DirectedAcyclicGraph<TVertex, TEdge> _graph;

        private readonly long _expectedTopoModCount;

        private int      _currentTopoIndex;
        private TVertex? _currentVertex;
        private int?     _nextIndex;

        public TopoIterator(DirectedAcyclicGraph<TVertex, TEdge> graph)
        {
            _graph                = graph;
            _expectedTopoModCount = _graph._topoModCount;
            _currentTopoIndex     = graph._minTopoIndex - 1;
        }

        object IEnumerator.Current => Current;

        public TVertex Current => _currentVertex ?? throw new NoSuchElementException();

        public bool MoveNext()
        {
            if (_expectedTopoModCount != _graph._topoModCount)
            {
                throw new InvalidOperationException("Concurrent modification exception");
            }

            _nextIndex ??= GetNextIndex();
            if (!_nextIndex.HasValue)
            {
                return false;
            }

            _currentTopoIndex = _nextIndex.Value;
            _nextIndex        = null;
            _currentVertex    = _graph._topoOrderMap.GetVertex(_currentTopoIndex);
            return true;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            // empty
        }

        [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
        public void Remove()
        {
            if (_expectedTopoModCount != _graph._topoModCount)
            {
                throw new InvalidOperationException("Concurrent modification exception");
            }

            var vertexToRemove = _graph._topoOrderMap.GetVertex(_currentTopoIndex);
            if (vertexToRemove != null)
            {
                _graph._topoOrderMap.RemoveVertex(vertexToRemove);
            }
            else
            {
                // should only happen if next() hasn't been called
                throw new InvalidOperationException();
            }
        }

        private int? GetNextIndex()
        {
            for (var i = _currentTopoIndex + 1; i <= _graph._maxTopoIndex; i++)
            {
                if (_graph._topoOrderMap.GetVertex(i) != null)
                {
                    return i;
                }
            }

            return null;
        }
    }
}
