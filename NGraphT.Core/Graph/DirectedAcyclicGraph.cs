using System.Collections;

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

using Builder;
using Traverse;
using Util;

/// <summary>
/// A directed acyclic graph (DAG).
///
/// <para>
/// Implements a DAG that can be modified (vertices &amp; edges added and removed), is guaranteed to
/// remain acyclic, and provides fast topological order iteration. An attempt to add an edge which
/// would induce a cycle throws an <seealso cref="System.ArgumentException"/>.
///
/// </para>
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
///
/// </para>
/// <para>
/// The complexity of adding a new edge in the graph depends on the number of edges incident to the
/// "affected region", and should in general be faster than recomputing the whole topological
/// ordering from scratch. For details about the complexity parameters and running times, see the
/// previously mentioned paper.
///
/// </para>
/// <para>
/// This class makes no claims to thread safety, and concurrent usage from multiple threads will
/// produce undefined results.
///
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Peter Giles.</remarks>
public class DirectedAcyclicGraph<TNode, TEdge> : AbstractBaseGraph<TNode, TEdge>, IEnumerable<TNode>
{
    private readonly IComparer<TNode>     _topoComparator;
    private readonly ITopoOrderMap<TNode> _topoOrderMap;
    private          int                  _maxTopoIndex = 0;
    private          int                  _minTopoIndex = 0;

    // this update count is used to keep internal topological iterators honest
    private long _topoModCount = 0;

    /// <summary>
    /// The visited strategy factory to use. Subclasses can change this.
    /// </summary>
    private readonly IVisitedStrategyFactory _visitedStrategyFactory;

    /// <summary>
    /// Construct a directed acyclic graph.
    /// </summary>
    /// <param name="edgeClass"> the edge class.</param>
    public DirectedAcyclicGraph(Type edgeClass)
        : this(null, SupplierUtil.CreateSupplier(edgeClass), false, false)
    {
    }

    /// <summary>
    /// Construct a directed acyclic graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier.</param>
    /// <param name="edgeSupplier"> the edge supplier.</param>
    /// <param name="weighted"> if true the graph will be weighted, otherwise not.</param>
    public DirectedAcyclicGraph(Func<TNode> vertexSupplier, Func<TEdge> edgeSupplier, bool weighted)
        : this(vertexSupplier, edgeSupplier, new VisitedBitSetImpl(), new TopoVertexBiMap<TNode>(), weighted, false)
    {
    }

    /// <summary>
    /// Construct a directed acyclic graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier.</param>
    /// <param name="edgeSupplier"> the edge supplier.</param>
    /// <param name="weighted"> if true the graph will be weighted, otherwise not.</param>
    /// <param name="allowMultipleEdges"> if true the graph will allow multiple edges, otherwise not.</param>
    public DirectedAcyclicGraph(
        Func<TNode> vertexSupplier,
        Func<TEdge> edgeSupplier,
        bool               weighted,
        bool               allowMultipleEdges
    )
        : this(vertexSupplier,
            edgeSupplier,
            new VisitedBitSetImpl(),
            new TopoVertexBiMap<TNode>(),
            weighted,
            allowMultipleEdges
        )
    {
    }

    /// <summary>
    /// Construct a directed acyclic graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier.</param>
    /// <param name="edgeSupplier"> the edge supplier.</param>
    /// <param name="weighted"> if true the graph will be weighted, otherwise not.</param>
    /// <param name="allowMultipleEdges"> if true the graph will allow multiple edges, otherwise not.</param>
    /// <param name="graphSpecificsStrategy"> strategy for constructing low-level graph specifics.</param>
    public DirectedAcyclicGraph(
        Func<TNode>                    vertexSupplier,
        Func<TEdge>                    edgeSupplier,
        bool                                  weighted,
        bool                                  allowMultipleEdges,
        IGraphSpecificsStrategy<TNode, TEdge> graphSpecificsStrategy
    )
        : this(vertexSupplier,
            edgeSupplier,
            new VisitedBitSetImpl(),
            new TopoVertexBiMap<TNode>(),
            weighted,
            allowMultipleEdges,
            graphSpecificsStrategy
        )
    {
    }

    /// <summary>
    /// Construct a directed acyclic graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier.</param>
    /// <param name="edgeSupplier"> the edge supplier.</param>
    /// <param name="visitedStrategyFactory"> the visited strategy factory. Subclasses can change this
    ///        implementation to adjust the performance tradeoffs.</param>
    /// <param name="topoOrderMap"> the topological order map. For performance reasons, subclasses can change
    ///        the way this class stores the topological order.</param>
    /// <param name="weighted"> if true the graph will be weighted, otherwise not.</param>
    protected internal DirectedAcyclicGraph(
        Func<TNode>      vertexSupplier,
        Func<TEdge>      edgeSupplier,
        IVisitedStrategyFactory visitedStrategyFactory,
        ITopoOrderMap<TNode>    topoOrderMap,
        bool                    weighted
    )
        : this(vertexSupplier, edgeSupplier, visitedStrategyFactory, topoOrderMap, weighted, false)
    {
    }

    /// <summary>
    /// Construct a directed acyclic graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier.</param>
    /// <param name="edgeSupplier"> the edge supplier.</param>
    /// <param name="visitedStrategyFactory"> the visited strategy factory. Subclasses can change this
    ///        implementation to adjust the performance tradeoffs.</param>
    /// <param name="topoOrderMap"> the topological order map. For performance reasons, subclasses can change
    ///        the way this class stores the topological order.</param>
    /// <param name="weighted"> if true the graph will be weighted, otherwise not.</param>
    /// <param name="allowMultipleEdges"> if true the graph will allow multiple edges, otherwise not.</param>
    protected internal DirectedAcyclicGraph(
        Func<TNode>      vertexSupplier,
        Func<TEdge>      edgeSupplier,
        IVisitedStrategyFactory visitedStrategyFactory,
        ITopoOrderMap<TNode>    topoOrderMap,
        bool                    weighted,
        bool                    allowMultipleEdges
    )
        : this(vertexSupplier,
            edgeSupplier,
            visitedStrategyFactory,
            topoOrderMap,
            weighted,
            allowMultipleEdges,
            new FastLookupGraphSpecificsStrategy<TNode, TEdge>()
        )
    {
    }

    /// <summary>
    /// Construct a directed acyclic graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier.</param>
    /// <param name="edgeSupplier"> the edge supplier.</param>
    /// <param name="visitedStrategyFactory"> the visited strategy factory. Subclasses can change this
    ///        implementation to adjust the performance tradeoffs.</param>
    /// <param name="topoOrderMap"> the topological order map. For performance reasons, subclasses can change
    ///        the way this class stores the topological order.</param>
    /// <param name="weighted"> if true the graph will be weighted, otherwise not.</param>
    /// <param name="allowMultipleEdges"> if true the graph will allow multiple edges, otherwise not.</param>
    /// <param name="graphSpecificsStrategy"> strategy for constructing low-level graph specifics.</param>
    protected internal DirectedAcyclicGraph(
        Func<TNode>                    vertexSupplier,
        Func<TEdge>                    edgeSupplier,
        IVisitedStrategyFactory               visitedStrategyFactory,
        ITopoOrderMap<TNode>                  topoOrderMap,
        bool                                  weighted,
        bool                                  allowMultipleEdges,
        IGraphSpecificsStrategy<TNode, TEdge> graphSpecificsStrategy
    )
        : base(vertexSupplier,
            edgeSupplier,
            (new DefaultGraphType.Builder()).Directed().AllowMultipleEdges(allowMultipleEdges).AllowSelfLoops(false)
            .Weighted(weighted).AllowCycles(false).Build(),
            graphSpecificsStrategy
        )
    {
        _visitedStrategyFactory = Objects.requireNonNull(visitedStrategyFactory, "Visited factory cannot be null");
        _topoOrderMap           = Objects.requireNonNull(topoOrderMap, "Topological order map cannot be null");
        _topoComparator         = new TopoComparator(this);
    }

    /// <summary>
    /// Create a builder for this kind of graph.
    /// </summary>
    /// <param name="edgeClass"> class on which to base factory for edges.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>a builder for this kind of graph.</returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: public static <TNode, TEdge> GraphBuilder<TNode, TEdge, ? extends DirectedAcyclicGraph<TNode, TEdge>> createBuilder(Class edgeClass)
    public static GraphBuilder<TNode, TEdge, DirectedAcyclicGraph<TNode, TEdge>> CreateBuilder<TNode, TEdge>(
        Type edgeClass
    )
    {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: return new GraphBuilder<>(new DirectedAcyclicGraph<>(edgeClass));
        return new GraphBuilder<TNode, TEdge, DirectedAcyclicGraph<TNode, TEdge>>(
            new DirectedAcyclicGraph<TNode, TEdge, DirectedAcyclicGraph<TNode, TEdge>>(edgeClass)
        );
    }

    /// <summary>
    /// Create a builder for this kind of graph.
    /// </summary>
    /// <param name="edgeSupplier"> edge supplier for the edges.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>a builder for this kind of graph.</returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: public static <TNode, TEdge> GraphBuilder<TNode, TEdge, ? extends DirectedAcyclicGraph<TNode, TEdge>> createBuilder(Supplier<TEdge> edgeSupplier)
    public static GraphBuilder<TNode, TEdge, DirectedAcyclicGraph<TNode, TEdge>> CreateBuilder<TNode, TEdge>(
        Func<TEdge> edgeSupplier
    )
    {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: return new GraphBuilder<>(new DirectedAcyclicGraph<>(null, edgeSupplier, false));
        return new GraphBuilder<TNode, TEdge, DirectedAcyclicGraph<TNode, TEdge>>(
            new DirectedAcyclicGraph<TNode, TEdge, DirectedAcyclicGraph<TNode, TEdge>>(null, edgeSupplier, false)
        );
    }

    public override TNode AddVertex()
    {
        var node = base.AddVertex();

        if (node != null)
        {
            // add to the topological map
            ++_maxTopoIndex;
            _topoOrderMap.PutVertex(_maxTopoIndex, node);
            ++_topoModCount;
        }

        return node;
    }

    public override bool AddVertex(TNode node)
    {
        var added = base.AddVertex(node);

        if (added)
        {
            // add to the topological map
            ++_maxTopoIndex;
            _topoOrderMap.PutVertex(_maxTopoIndex, node);
            ++_topoModCount;
        }

        return added;
    }

    public override bool RemoveVertex(TNode node)
    {
        var removed = base.RemoveVertex(node);

        if (removed)
        {
            /*
             * Depending on the topoOrderMap implementation, this can leave holes in the topological
             * ordering, which can degrade performance for certain operations over time.
             */
            var topoIndex = _topoOrderMap.RemoveVertex(node);

            // if possible contract minTopoIndex
            if (topoIndex.Value == _minTopoIndex)
            {
                while ((_minTopoIndex < 0) && (_topoOrderMap.GetVertex(_minTopoIndex) == null))
                {
                    ++_minTopoIndex;
                }
            }

            // if possible contract maxTopoIndex
            if (topoIndex.Value == _maxTopoIndex)
            {
                while ((_maxTopoIndex > 0) && (_topoOrderMap.GetVertex(_maxTopoIndex) == null))
                {
                    --_maxTopoIndex;
                }
            }

            ++_topoModCount;
        }

        return removed;
    }

    /// <summary>
    /// {@inheritDoc}
    ///
    /// <para>
    /// The complexity of adding a new edge in the graph depends on the number of edges incident to
    /// the "affected region", and should in general be faster than recomputing the whole topological
    /// ordering from scratch.
    ///
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentException"> if the vertex is not in the graph.</exception>
    /// <exception cref="GraphCycleProhibitedException"> if the vertex would induce a cycle in the graph.</exception>
    public override TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
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
    /// {@inheritDoc}
    ///
    /// <para>
    /// The complexity of adding a new edge in the graph depends on the number of edges incident to
    /// the "affected region", and should in general be faster than recomputing the whole topological
    /// ordering from scratch.
    ///
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentException"> if the vertex is not in the graph.</exception>
    /// <exception cref="GraphCycleProhibitedException"> if the vertex would induce a cycle in the graph.</exception>
    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        if (edge == null)
        {
            throw new NullReferenceException();
        }
        else if (ContainsEdge(edge))
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
    /// <returns><seealso cref="System.Collections.Generic.ISet<object>"/> of ancestors of a vertex.</returns>
    public virtual ISet<TNode> GetAncestors(TNode vertex)
    {
        EdgeReversedGraph<TNode, TEdge> reversedGraph = new EdgeReversedGraph<TNode, TEdge>(this);
        IEnumerator<TNode>              iterator      = new DepthFirstIterator<TNode>(reversedGraph, vertex);
        ISet<TNode>                     ancestors     = new HashSet<TNode>();

        // Do not add start vertex to result.
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
        if (iterator.hasNext())
        {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            iterator.next();
        }

        iterator.forEachRemaining(ancestors.add);

        return ancestors;
    }

    /// <summary>
    /// Get the descendants of a vertex.
    /// </summary>
    /// <param name="vertex"> the vertex to get the descendants of.</param>
    /// <returns><seealso cref="System.Collections.Generic.ISet<object>"/> of descendants of a vertex.</returns>
    public virtual ISet<TNode> GetDescendants(TNode vertex)
    {
        IEnumerator<TNode> iterator    = new DepthFirstIterator<TNode>(this, vertex);
        ISet<TNode>        descendants = new HashSet<TNode>();

        // Do not add start vertex to result.
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
        if (iterator.hasNext())
        {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            iterator.next();
        }

        iterator.forEachRemaining(descendants.add);

        return descendants;
    }

    /// <summary>
    /// Returns a topological order iterator.
    /// </summary>
    /// <returns>a topological order iterator.</returns>
    public virtual IEnumerator<TNode> GetEnumerator()
    {
        return new TopoIterator(this);
    }

    /// <summary>
    /// Update as if a new edge is added.
    /// </summary>
    /// <param name="sourceVertex"> the source vertex.</param>
    /// <param name="targetVertex"> the target vertex.</param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void updateDag(TNode sourceVertex, TNode targetVertex) throws CycleFoundException
    private void UpdateDag(TNode sourceVertex, TNode targetVertex)
    {
        var lb = _topoOrderMap.GetTopologicalIndex(targetVertex);
        var ub = _topoOrderMap.GetTopologicalIndex(sourceVertex);

        if (lb < ub)
        {
            ISet<TNode> df = new HashSet<TNode>();
            ISet<TNode> db = new HashSet<TNode>();

            // discovery
            Region           affectedRegion = new Region(lb.Value, ub.Value);
            IVisitedStrategy visited        = _visitedStrategyFactory.GetVisitedStrategy(affectedRegion);

            // throws CycleFoundException if there is a cycle
            DfsF(targetVertex, df, visited, affectedRegion);
            DfsB(sourceVertex, db, visited, affectedRegion);
            Reorder(df, db, visited);

            /*
             * if we do a reorder, then the topology has been updated
             */
            ++_topoModCount;
        }
    }

    /// <summary>
    /// Depth first search forward, building up the set (df) of forward-connected vertices in the
    /// Affected Region
    /// </summary>
    /// <param name="initialVertex"> the vertex being visited.</param>
    /// <param name="df"> the set we are populating with forward connected vertices in the Affected Region.</param>
    /// <param name="visited"> a simple data structure that lets us know if we already visited a node with a
    ///        given topo index
    /// </param>
    /// <exception cref="CycleFoundException"> if a cycle is discovered.</exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dfsF(TNode initialVertex, Set<TNode> df, VisitedStrategy visited, Region affectedRegion) throws CycleFoundException
    private void DfsF(TNode initialVertex, ISet<TNode> df, IVisitedStrategy visited, Region affectedRegion)
    {
        LinkedList<TNode> vertices = new LinkedList<TNode>();
        vertices.AddFirst(initialVertex);

        while (vertices.Count > 0)
        {
            TNode vertex    = vertices.RemoveFirst();
            var   topoIndex = _topoOrderMap.GetTopologicalIndex(vertex).Value;

            if (visited.GetVisited(topoIndex))
            {
                continue;
            }

            // Assumption: vertex is in the AR and so it will be in visited
            visited.Visited = topoIndex;

            df.Add(vertex);

            foreach (var outEdge in OutgoingEdgesOf(vertex))
            {
                var nextVertex          = GetEdgeTarget(outEdge);
                var  nextVertexTopoIndex = _topoOrderMap.GetTopologicalIndex(nextVertex);

                if (nextVertexTopoIndex.Value == affectedRegion.finish)
                {
                    // reset visited
                    try
                    {
                        foreach (var visitedVertex in df)
                        {
                            visited.ClearVisited(_topoOrderMap.GetTopologicalIndex(visitedVertex).Value);
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
                if (affectedRegion.IsIn(nextVertexTopoIndex.Value) && !visited.GetVisited(nextVertexTopoIndex.Value))
                {
                    vertices.AddFirst(nextVertex); // recurse
                }
            }
        }
    }

    /// <summary>
    /// Depth first search backward, building up the set (db) of back-connected vertices in the
    /// Affected Region
    /// </summary>
    /// <param name="initialVertex"> the vertex being visited.</param>
    /// <param name="db"> the set we are populating with back-connected vertices in the AR.</param>
    /// <param name="visited"> </param>
    private void DfsB(TNode initialVertex, ISet<TNode> db, IVisitedStrategy visited, Region affectedRegion)
    {
        LinkedList<TNode> vertices = new LinkedList<TNode>();
        vertices.AddFirst(initialVertex);

        while (vertices.Count > 0)
        {
            TNode vertex = vertices.RemoveFirst();
            // Assumption: vertex is in the AR and so we will get a topoIndex from
            // the map
            var topoIndex = _topoOrderMap.GetTopologicalIndex(vertex).Value;

            if (visited.GetVisited(topoIndex))
            {
                continue;
            }

            visited.Visited = topoIndex;

            db.Add(vertex);

            foreach (var inEdge in IncomingEdgesOf(vertex))
            {
                var previousVertex          = GetEdgeSource(inEdge);
                var  previousVertexTopoIndex = _topoOrderMap.GetTopologicalIndex(previousVertex);

                /*
                 * Note, order of checks is important as we need to make sure the vertex is in the
                 * affected region before we check its visited status (otherwise we will be causing
                 * an ArrayIndexOutOfBoundsException).
                 */
                if (affectedRegion.IsIn(previousVertexTopoIndex.Value) &&
                    !visited.GetVisited(previousVertexTopoIndex.Value))
                {
                    // if previousVertexTopoIndex != null, the vertex is in the
                    // Affected Region according to our topoIndexMap
                    vertices.AddFirst(previousVertex);
                }
            }
        }
    }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private void reorder(Set<TNode> df, Set<TNode> db, VisitedStrategy visited)
    private void Reorder(ISet<TNode> df, ISet<TNode> db, IVisitedStrategy visited)
    {
        IList<TNode> topoDf = new List<TNode>(df);
        IList<TNode> topoDb = new List<TNode>(db);

        topoDf.Sort(_topoComparator);
        topoDb.Sort(_topoComparator);

        // merge these suckers together in topological order
        var availableTopoIndices = new SortedSet<int>();

        // we have to cast to the generic type, can't do "new TNode[size]" in java
        // 5;
        TNode[] bigL   = (TNode[])new object[df.Count + db.Count];
        var     lIndex = 0; // this index is used for the sole purpose of pushing
        // into

        // the correct index of bigL
        // assume (for now) that we are resetting visited
        var clearVisited = true;

        foreach (var vertex in topoDb)
        {
            var topoIndex = _topoOrderMap.GetTopologicalIndex(vertex);

            // add the available indices to the set
            availableTopoIndices.Add(topoIndex);

            bigL[lIndex++] = vertex;

            if (clearVisited)
            {
                // reset visited status if supported
                try
                {
                    visited.ClearVisited(topoIndex.Value);
                }
                catch (NotSupportedException)
                {
                    clearVisited = false;
                }
            }
        }

        foreach (var vertex in topoDf)
        {
            var topoIndex = _topoOrderMap.GetTopologicalIndex(vertex);

            // add the available indices to the set
            availableTopoIndices.Add(topoIndex);
            bigL[lIndex++] = vertex;

            if (clearVisited)
            {
                // reset visited status if supported
                try
                {
                    visited.ClearVisited(topoIndex.Value);
                }
                catch (NotSupportedException)
                {
                    clearVisited = false;
                }
            }
        }

        lIndex = 0; // reusing lIndex
        foreach (int? topoIndex in availableTopoIndices)
        {
            // assign the indexes to the elements of bigL in order
            var vertex = bigL[lIndex++]; // note the post-increment
            _topoOrderMap.PutVertex(topoIndex, vertex);
        }
    }

    /// <summary>
    /// An interface for storing the topological ordering.
    /// </summary>
    /// @param <TNode> the graph vertex type
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    protected internal interface ITopoOrderMap<TNode>
    {
        /// <summary>
        /// Add a vertex at the given topological index.
        /// </summary>
        /// <param name="index"> the topological index.</param>
        /// <param name="vertex"> the vertex.</param>
        void PutVertex(int? index, TNode vertex);

        /// <summary>
        /// Get the vertex at the given topological index.
        /// </summary>
        /// <param name="index"> the topological index.</param>
        /// <returns>vertex the vertex.</returns>
        TNode GetVertex(int? index);

        /// <summary>
        /// Get the topological index of the given vertex.
        /// </summary>
        /// <param name="vertex"> the vertex.</param>
        /// <returns>the index that the vertex is at, or null if the vertex isn't in the topological
        ///         ordering.</returns>
        int? GetTopologicalIndex(TNode vertex);

        /// <summary>
        /// Remove the given vertex from the topological ordering.
        /// </summary>
        /// <param name="vertex"> the vertex.</param>
        /// <returns>the index that the vertex was at, or null if the vertex wasn't in the topological
        ///         ordering.</returns>
        int? RemoveVertex(TNode vertex);

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
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    /// </para>
    /// </summary>
    protected internal interface IVisitedStrategy
    {
        /// <summary>
        /// Mark the given topological index as visited.
        /// </summary>
        /// <param name="index"> the topological index.</param>
        int Visited { set; }

        /// <summary>
        /// Get if the given topological index has been visited.
        /// </summary>
        /// <param name="index"> the topological index.</param>
        /// <returns>true if the given topological index has been visited, false otherwise.</returns>
        bool GetVisited(int index);

        /// <summary>
        /// Clear the visited state of the given topological index.
        /// </summary>
        /// <param name="index"> the index.</param>
        /// <exception cref="NotSupportedException"> if the implementation doesn't support (or doesn't
        ///         need) clearance. For example, if the factory creates a new instance every time,
        ///         it is a waste of cycles to reset the state after the search of the Affected
        ///         Region is done, so an UnsupportedOperationException *should* be thrown. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void clearVisited(int index) throws UnsupportedOperationException;
        void ClearVisited(int index);
    }

    /// <summary>
    /// A visited strategy factory.
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    /// </summary>
    protected internal interface IVisitedStrategyFactory
    {
        /// <summary>
        /// Create a new instance of <seealso cref="IVisitedStrategy"/>.
        /// </summary>
        /// <param name="affectedRegion"> the affected region.</param>
        /// <returns>a new instance of <seealso cref="IVisitedStrategy"/> for the affected region.</returns>
        IVisitedStrategy GetVisitedStrategy(Region affectedRegion);
    }

    /// <summary>
    /// A dual map implementation of the topological order map.
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    /// </summary>
        protected internal class TopoVertexBiMap<TNode> : ITopoOrderMap<TNode>
    {
        internal const long SerialVersionUID = 1L;

        internal readonly IDictionary<int, TNode> TopoToVertex = new Dictionary<int, TNode>();
        internal readonly IDictionary<TNode, int> VertexToTopo = new Dictionary<TNode, int>();

        /// <summary>
        /// Constructor
        /// </summary>
        public TopoVertexBiMap()
        {
        }

        public virtual void PutVertex(int? index, TNode vertex)
        {
            TopoToVertex[index]  = vertex;
            VertexToTopo[vertex] = index.Value;
        }

        public virtual TNode GetVertex(int? index)
        {
            return TopoToVertex[index];
        }

        public virtual int? GetTopologicalIndex(TNode vertex)
        {
            return VertexToTopo[vertex];
        }

        public virtual int? RemoveVertex(TNode vertex)
        {
            int? topoIndex = VertexToTopo.Remove(vertex);
            if (topoIndex != null)
            {
                TopoToVertex.Remove(topoIndex);
            }

            return topoIndex;
        }

        public virtual void RemoveAllVertices()
        {
            VertexToTopo.Clear();
            TopoToVertex.Clear();
        }
    }

    /// <summary>
    /// An implementation of the topological order map which for performance and flexibility uses an
    /// ArrayList for topological index to vertex mapping, and a HashMap for vertex to topological
    /// index mapping.
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    /// </summary>
        protected internal class TopoVertexMap : ITopoOrderMap<TNode>
    {
        private readonly DirectedAcyclicGraph<TNode, TEdge> _outerInstance;

        internal const long SerialVersionUID = 1L;

        internal readonly IList<TNode>            TopoToVertex = new List<TNode>();
        internal readonly IDictionary<TNode, int> VertexToTopo = new Dictionary<TNode, int>();

        /// <summary>
        /// Constructor
        /// </summary>
        public TopoVertexMap(DirectedAcyclicGraph<TNode, TEdge> outerInstance)
        {
            _outerInstance = outerInstance;
        }

        public virtual void PutVertex(int? index, TNode vertex)
        {
            var translatedIndex = TranslateIndex(index.Value);

            // grow topoToVertex as needed to accommodate elements
            while ((translatedIndex + 1) > TopoToVertex.Count)
            {
                TopoToVertex.Add(default(TNode));
            }

            TopoToVertex[translatedIndex] = vertex;
            VertexToTopo[vertex]          = index.Value;
        }

        public virtual TNode GetVertex(int? index)
        {
            return TopoToVertex[TranslateIndex(index.Value)];
        }

        public virtual int? GetTopologicalIndex(TNode vertex)
        {
            return VertexToTopo[vertex];
        }

        public virtual int? RemoveVertex(TNode vertex)
        {
            int? topoIndex = VertexToTopo.Remove(vertex);
            if (topoIndex != null)
            {
                TopoToVertex[TranslateIndex(topoIndex.Value)] = default(TNode);
            }

            return topoIndex;
        }

        public virtual void RemoveAllVertices()
        {
            VertexToTopo.Clear();
            TopoToVertex.Clear();
        }

        /// <summary>
        /// We translate the topological index to an ArrayList index. We have to do this because
        /// topological indices can be negative, and we want to do it because we can make better use
        /// of space by only needing an ArrayList of size |AR|.
        /// </summary>
        /// <returns>the ArrayList index.</returns>
        internal virtual int TranslateIndex(int index)
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
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    /// </summary>
        protected internal class Region
    {
        internal const long SerialVersionUID = 1L;

        internal readonly int Start;
        internal readonly int Finish;

        /// <summary>
        /// Construct a new region.
        /// </summary>
        /// <param name="start"> the start of the region.</param>
        /// <param name="finish"> the end of the region (inclusive) </param>
        public Region(int start, int finish)
        {
            if (start > finish)
            {
                throw new ArgumentException("(start > finish): invariant broken");
            }

            this.start  = start;
            this.finish = finish;
        }

        /// <summary>
        /// Get the size of the region.
        /// </summary>
        /// <returns>the size of the region.</returns>
        public virtual int Size
        {
            get
            {
                return (finish - start) + 1;
            }
        }

        /// <summary>
        /// Check if index is in the region.
        /// </summary>
        /// <param name="index"> the index to check.</param>
        /// <returns>true if the index is in the region, false otherwise.</returns>
        public virtual bool IsIn(int index)
        {
            return (index >= start) && (index <= finish);
        }

        /// <summary>
        /// Get the start of the region.
        /// </summary>
        /// <returns>the start of the region.</returns>
        public virtual int Start
        {
            get
            {
                return start;
            }
        }

        /// <summary>
        /// Get the end of the region (inclusive).
        /// </summary>
        /// <returns>the end of the region (inclusive)</returns>
        public virtual int Finish
        {
            get
            {
                return finish;
            }
        }
    }

    /// <summary>
    /// A visited strategy which uses a <seealso cref="System.Collections.BitArray"/>.
    ///
    /// <para>
    /// This implementation is close to the performance of <seealso cref="VisitedArrayListImpl"/>, with 1/8 the
    /// memory usage.
    /// 
    /// <remarks>Author: John TNode. Sichi.</remarks>
    /// </para>
    /// </summary>
        protected internal class VisitedBitSetImpl : IVisitedStrategy, IVisitedStrategyFactory
    {
        internal const long SerialVersionUID = 1L;

        internal readonly BitArray Visited = new BitArray();
        internal          Region   AffectedRegion;

        /// <summary>
        /// Constructor
        /// </summary>
        public VisitedBitSetImpl()
        {
        }

        public virtual IVisitedStrategy GetVisitedStrategy(Region affectedRegion)
        {
            AffectedRegion = affectedRegion;
            return this;
        }

        public virtual int Visited
        {
            set
            {
                visited.Set(TranslateIndex(value), true);
            }
        }

        public virtual bool GetVisited(int index)
        {
            return visited.Get(TranslateIndex(index));
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearVisited(int index) throws UnsupportedOperationException
        public virtual void ClearVisited(int index)
        {
            visited.Set(TranslateIndex(index), false);
        }

        /// <summary>
        /// We translate the topological index to an ArrayList index. We have to do this because
        /// topological indices can be negative, and we want to do it because we can make better use
        /// of space by only needing an ArrayList of size |AR|.
        /// </summary>
        /// <returns>the ArrayList index.</returns>
        internal virtual int TranslateIndex(int index)
        {
            return index - AffectedRegion.start;
        }
    }

    /// <summary>
    /// A visited strategy using an <seealso cref="System.Collections.ArrayList"/>.
    ///
    /// <para>
    /// This implementation seems to offer the best performance in most cases. It grows the internal
    /// ArrayList as needed to be as large as |AR|, so it will be more memory intensive than the
    /// HashSet implementation, and unlike the Array implementation, it will hold on to that memory
    /// (it expands, but never contracts).
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    /// </para>
    /// </summary>
        protected internal class VisitedArrayListImpl : IVisitedStrategy, IVisitedStrategyFactory
    {
        internal const long SerialVersionUID = 1L;

        internal readonly IList<bool> Visited = new List<bool>();
        internal          Region      AffectedRegion;

        /// <summary>
        /// Constructor
        /// </summary>
        public VisitedArrayListImpl()
        {
        }

        public virtual IVisitedStrategy GetVisitedStrategy(Region affectedRegion)
        {
            // Make sure visited is big enough
            var minSize = (affectedRegion.finish - affectedRegion.start) + 1;
            /* plus one because the region range is inclusive of both indices */

            while (visited.Count < minSize)
            {
                visited.Add(false);
            }

            AffectedRegion = affectedRegion;
            return this;
        }

        public virtual int Visited
        {
            set
            {
                visited[TranslateIndex(value)] = true;
            }
        }

        public virtual bool GetVisited(int index)
        {
            return visited[TranslateIndex(index)];
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearVisited(int index) throws UnsupportedOperationException
        public virtual void ClearVisited(int index)
        {
            visited[TranslateIndex(index)] = false;
        }

        /// <summary>
        /// We translate the topological index to an ArrayList index. We have to do this because
        /// topological indices can be negative, and we want to do it because we can make better use
        /// of space by only needing an ArrayList of size |AR|.
        /// </summary>
        /// <returns>the ArrayList index.</returns>
        internal virtual int TranslateIndex(int index)
        {
            return index - AffectedRegion.start;
        }
    }

    /// <summary>
    /// A visited strategy using a <seealso cref="System.Collections.Generic.HashSet<object>"/>.
    ///
    /// <para>
    /// This implementation doesn't seem to perform as well, though I can imagine circumstances where
    /// it should shine (lots and lots of vertices). It also should have the lowest memory footprint
    /// as it only uses storage for indices that have been visited.
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    /// </para>
    /// </summary>
        protected internal class VisitedHashSetImpl : IVisitedStrategy, IVisitedStrategyFactory
    {
        internal const long SerialVersionUID = 1L;

        internal readonly ISet<int> Visited = new HashSet<int>();

        /// <summary>
        /// Constructor
        /// </summary>
        public VisitedHashSetImpl()
        {
        }

        public virtual IVisitedStrategy GetVisitedStrategy(Region affectedRegion)
        {
            visited.Clear();
            return this;
        }

        public virtual int Visited
        {
            set
            {
                visited.Add(value);
            }
        }

        public virtual bool GetVisited(int index)
        {
            return visited.Contains(index);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearVisited(int index) throws UnsupportedOperationException
        public virtual void ClearVisited(int index)
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
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    /// </para>
    /// </summary>
        protected internal class VisitedArrayImpl : IVisitedStrategy, IVisitedStrategyFactory
    {
        internal const long SerialVersionUID = 1L;

        internal readonly bool[] Visited;
        internal readonly Region Region;

        /// <summary>
        /// Constructs empty instance
        /// </summary>
        public VisitedArrayImpl()
            : this(null)
        {
        }

        /// <summary>
        /// Construct an empty instance for a region.
        /// </summary>
        /// <param name="region"> the region.</param>
        public VisitedArrayImpl(Region region)
        {
            if (region == null)
            {
                // make empty instance
                this.visited = null;
                Region  = null;
            }
            else
            {
                // fill in the needed pieces
                Region = region;

                // initialized to all false by default
                visited = new bool[region.Size];
            }
        }

        public virtual IVisitedStrategy GetVisitedStrategy(Region affectedRegion)
        {
            return new VisitedArrayImpl(affectedRegion);
        }

        public virtual int Visited
        {
            set
            {
                visited[value - Region.start] = true;
            }
        }

        public virtual bool GetVisited(int index)
        {
            return visited[index - Region.start];
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void clearVisited(int index) throws UnsupportedOperationException
        public virtual void ClearVisited(int index)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Exception used in dfsF when a cycle is found
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    /// </summary>
    private class CycleFoundException : Exception
    {
        internal const long SerialVersionUID = 5583471522212552754L;
    }

    /// <summary>
    /// Comparator for vertices based on their topological ordering
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    /// </summary>
        private class TopoComparator : IComparer<TNode>
    {
        private readonly DirectedAcyclicGraph<TNode, TEdge> _outerInstance;

        public TopoComparator(DirectedAcyclicGraph<TNode, TEdge> outerInstance)
        {
            _outerInstance = outerInstance;
        }

        internal const long SerialVersionUID = 8144905376266340066L;

        public virtual int Compare(TNode o1, TNode o2)
        {
            return outerInstance._topoOrderMap.GetTopologicalIndex(o1)
                .compareTo(outerInstance._topoOrderMap.GetTopologicalIndex(o2));
        }
    }

    /// <summary>
    /// An iterator which follows topological order
    /// 
    /// <remarks>Author: Peter Giles.</remarks>
    /// </summary>
    private class TopoIterator : IEnumerator<TNode>
    {
        internal bool InstanceFieldsInitialized = false;

        internal virtual void InitializeInstanceFields()
        {
            ExpectedTopoModCount = outerInstance._topoModCount;
        }

        private readonly DirectedAcyclicGraph<TNode, TEdge> _outerInstance;

        internal int  CurrentTopoIndex;
        internal long ExpectedTopoModCount;
        internal int? NextIndex = null;

        public TopoIterator(DirectedAcyclicGraph<TNode, TEdge> outerInstance)
        {
            _outerInstance = outerInstance;

            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }

            CurrentTopoIndex = outerInstance._minTopoIndex - 1;
        }

        public override bool HasNext()
        {
            if (ExpectedTopoModCount != outerInstance._topoModCount)
            {
                throw new ConcurrentModificationException();
            }

            nextIndex = NextIndex;
            return nextIndex != null;
        }

        public override TNode Next()
        {
            if (ExpectedTopoModCount != outerInstance._topoModCount)
            {
                throw new ConcurrentModificationException();
            }

            if (nextIndex == null)
            {
                // find nextIndex
                nextIndex = NextIndex;
            }

            if (nextIndex == null)
            {
                throw new NoSuchElementException();
            }

            CurrentTopoIndex = nextIndex.Value;
            nextIndex        = null;
            return outerInstance._topoOrderMap.GetVertex(CurrentTopoIndex);
        }

        public override void Remove()
        {
            if (ExpectedTopoModCount != outerInstance._topoModCount)
            {
                throw new ConcurrentModificationException();
            }

            TNode vertexToRemove;
            if ((vertexToRemove = outerInstance._topoOrderMap.GetVertex(CurrentTopoIndex)) != null)
            {
                outerInstance._topoOrderMap.RemoveVertex(vertexToRemove);
            }
            else
            {
                // should only happen if next() hasn't been called
                throw new InvalidOperationException();
            }
        }

        internal virtual int? NextIndex
        {
            get
            {
                for (var i = CurrentTopoIndex + 1; i <= outerInstance._maxTopoIndex; i++)
                {
                    if (outerInstance._topoOrderMap.GetVertex(i) != null)
                    {
                        return i;
                    }
                }

                return null;
            }
        }
    }
}
