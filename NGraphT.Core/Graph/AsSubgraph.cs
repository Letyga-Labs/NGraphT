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
using J2N.Collections.Generic.Extensions;
using NGraphT.Core.DotNetUtil;
using NGraphT.Core.Events;
using Java2Net = J2N.Collections.Generic;

namespace NGraphT.Core.Graph;

/// <summary>
/// A subgraph is a graph that has a subset of vertices and a subset of edges with respect to some
/// base graph. More formally, a subgraph G(TVertex,TEdge) that is based on a base graph Gb(Vb,Eb) satisfies
/// the following <b><i>subgraph property</i></b>: TVertex is a subset of Vb and TEdge is a subset of Eb. Other
/// than this property, a subgraph is a graph with any respect and fully complies with the
/// <c>Graph</c> interface.
///
/// <para>
/// If the base graph is a <see cref="IListenableGraph{TVertex,TEdge}"/>, the subgraph listens on the base
/// graph and guarantees the subgraph property. If an edge or a vertex is removed from the base
/// graph, it is automatically removed from the subgraph. Subgraph listeners are informed on such
/// removal only if it results in a cascaded removal from the subgraph. If the subgraph has been
/// created as an induced subgraph it also keeps track of edges being added to its vertices. If
/// vertices are added to the base graph, the subgraph remains unaffected.
/// </para>
///
/// <para>
/// If the base graph is <i>not</i> a ListenableGraph, then the subgraph property cannot be
/// guaranteed. If edges or vertices are removed from the base graph, they are <i>not</i> removed
/// from the subgraph.
/// </para>
///
/// <para>
/// Modifications to Subgraph are allowed as long as the subgraph property is maintained. Addition of
/// vertices or edges are allowed as long as they also exist in the base graph. Removal of vertices
/// or edges is always allowed. The base graph is <i>never</i> affected by any modification made to
/// the subgraph.
/// </para>
///
/// <para>
/// A subgraph may provide a "live-window" on a base graph, so that changes made to its vertices or
/// edges are immediately reflected in the base graph, and vice versa. For that to happen, vertices
/// and edges added to the subgraph must be <i>identical</i> (that is, reference-equal and not only
/// value-equal) to their respective ones in the base graph. Previous versions of this class enforced
/// such identity, at a severe performance cost. Currently it is no longer enforced. If you want to
/// achieve a "live-window" functionality, your safest tactics would be to NOT override the
/// <c>equals()</c> methods of your vertices and edges. If you use a class that has already
/// overridden the <c>equals()</c> method, such as <c>String</c>, then you can use a
/// wrapper around it, or else use it directly but exercise a great care to avoid having
/// different-but-equal instances in the subgraph and the base graph.
/// </para>
///
/// <para>
/// This graph implementation guarantees deterministic vertex and edge set ordering (via
/// <see cref="J2N.Collections.Generic.LinkedHashSet{T}"/>).
/// </para>
///
/// <para>
/// Note that this implementation tries to maintain a "live-window" on the base graph, which has
/// implications in the performance of the various operations. For example iterating over the
/// adjacent edges of a vertex takes time proportional to the number of adjacent edges of the vertex
/// in the base graph even if the subgraph contains only a small subset of those edges. Therefore,
/// the user must be aware that using this implementation for certain algorithms might come with
/// computational overhead. For certain algorithms it is better to maintain a subgraph by hand
/// instead of using this implementation as a black box.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex"> the vertex type.</typeparam>
/// <typeparam name="TEdge"> the edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
/// <seealso cref="Graph"/>
public sealed class AsSubgraph<TVertex, TEdge> : AbstractGraph<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private const string NoSuchEdgeInBase = "no such edge in base graph";

    private readonly ISet<TEdge>            _edgeSet   = new Java2Net.LinkedHashSet<TEdge>();
    private readonly ISet<TVertex>          _vertexSet = new Java2Net.LinkedHashSet<TVertex>();
    private readonly IGraph<TVertex, TEdge> _base;
    private readonly IGraphType             _baseType;

    private readonly Lazy<ISet<TEdge>>   _unmodifiableEdgeSet;
    private readonly Lazy<ISet<TVertex>> _unmodifiableVertexSet;

    /// <summary>
    /// Creates a new subgraph.
    /// </summary>
    /// <param name="base"> the base (backing) graph on which the subgraph will be based.</param>
    /// <param name="vertexSubset"> vertices to include in the subgraph. If <c>null</c> then all
    ///       vertices are included.</param>
    /// <param name="edgeSubset"> edges to in include in the subgraph. If <c>null</c> then all the
    ///       edges whose vertices found in the graph are included.</param>
    public AsSubgraph(IGraph<TVertex, TEdge> @base, ISet<TVertex>? vertexSubset, ISet<TEdge>? edgeSubset)
    {
        ArgumentNullException.ThrowIfNull(@base);

        // TODO: GraphTests.RequireDirectedOrUndirected(@base)
        _base     = @base;
        _baseType = @base.Type;
        IsInduced = edgeSubset == null;

        (@base as IListenableGraph<TVertex, TEdge>)?.AddGraphListener(new BaseGraphListener(this));

        Initialize(vertexSubset, edgeSubset);

        _unmodifiableEdgeSet   = new Lazy<ISet<TEdge>>(() => _edgeSet.AsReadOnly());
        _unmodifiableVertexSet = new Lazy<ISet<TVertex>>(() => _vertexSet.AsReadOnly());
    }

    /// <summary>
    /// Creates a new induced subgraph. The subgraph will keep track of edges being added to its
    /// vertex subset as well as deletion of edges and vertices. If base it not listenable, this is
    /// identical to the call Subgraph(base, vertexSubset, null).
    /// </summary>
    /// <param name="base"> the base (backing) graph on which the subgraph will be based.</param>
    /// <param name="vertexSubset"> vertices to include in the subgraph. If <c>null</c> then all
    ///       vertices are included.</param>
    public AsSubgraph(IGraph<TVertex, TEdge> @base, ISet<TVertex> vertexSubset)
        : this(@base, vertexSubset, null)
    {
    }

    /// <summary>
    /// Creates a new induced Subgraph with all vertices included. The subgraph will keep track of
    /// edges being added to its vertex subset as well as deletion of edges and vertices. If base is
    /// not listenable, this is identical to the call Subgraph(base, null, null).
    /// </summary>
    /// <param name="base"> the base (backing) graph on which the subgraph will be based.</param>
    public AsSubgraph(IGraph<TVertex, TEdge> @base)
        : this(@base, null, null)
    {
    }

    /// <inheritdoc/>
    public override Func<TVertex>? VertexSupplier => _base.VertexSupplier;

    /// <inheritdoc/>
    public override Func<TEdge>? EdgeSupplier => _base.EdgeSupplier;

    /// <inheritdoc/>
    public override IGraphType Type => _base.Type;

    internal bool IsInduced { get; }

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TVertex? sourceVertex, TVertex? targetVertex)
    {
        return _base
            .GetAllEdges(sourceVertex, targetVertex)
            .Where(_edgeSet.Contains)
            .ToLinkedHashSet();
    }

    /// <inheritdoc/>
    public override TEdge? GetEdge(TVertex? sourceVertex, TVertex? targetVertex)
    {
        return GetAllEdges(sourceVertex, targetVertex).FirstOrDefault();
    }

    /// <summary>
    /// Add an edge to the subgraph. The end-points must exist in the subgraph and the edge must
    /// exist in the base graph. In case multiple such edges exist in the base graph, one that is not
    /// already in the subgraph is chosen arbitrarily and added to the subgraph. In case all such
    /// edges already exist in the subgraph, the method returns null.
    /// </summary>
    ///
    /// <param name="sourceVertex"> the source vertex.</param>
    /// <param name="targetVertex"> the target vertex.</param>
    ///
    /// <returns>
    ///     the added edge or null if all such edges from the base graph already belong in the subgraph.
    /// </returns>
    ///
    /// <exception cref="ArgumentException">
    ///     if the source or target vertex does not belong to the subgraph.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     if the base graph does not contain any edge between the two end-points.
    /// </exception>
    public override TEdge? AddEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        AssertVertexExist(sourceVertex);
        AssertVertexExist(targetVertex);

        if (!_base.ContainsEdge(sourceVertex, targetVertex))
        {
            throw new ArgumentException(NoSuchEdgeInBase, nameof(sourceVertex));
        }

        var edges = _base.GetAllEdges(sourceVertex, targetVertex);

        foreach (var edge in edges)
        {
            if (!ContainsEdge(edge))
            {
                _edgeSet.Add(edge);
                return edge;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public override bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);

        if (!_base.ContainsEdge(edge))
        {
            throw new ArgumentException(NoSuchEdgeInBase, nameof(edge));
        }

        AssertVertexExist(sourceVertex);
        AssertVertexExist(targetVertex);

        Debug.Assert(_base.GetEdgeSource(edge) == sourceVertex);
        Debug.Assert(_base.GetEdgeTarget(edge) == targetVertex);

        return _edgeSet.Add(edge);
    }

    public override TVertex AddVertex()
    {
        throw new NotSupportedException("Cannot create new vertices from subgraph");
    }

    /// <summary>
    /// Adds the specified vertex to this subgraph.
    /// </summary>
    ///
    /// <param name="vertex"> the vertex to be added.</param>
    ///
    /// <returns><c>true</c> if the vertex was added, otherwise <c>false</c>.</returns>
    ///
    /// <exception cref="ArgumentException"> if the base graph does not contain the vertex.</exception>
    ///
    /// <seealso cref="AsSubgraph{TVertex,TEdge}"/>
    /// <seealso cref="IGraph{TVertex,TEdge}.AddVertex(TVertex)"/>
    public override bool AddVertex(TVertex vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        if (!_base.ContainsVertex(vertex))
        {
            throw new ArgumentException("no such vertex in base graph", nameof(vertex));
        }

        return _vertexSet.Add(vertex);
    }

    /// <inheritdoc/>
    public override bool ContainsEdge(TEdge? edge)
    {
        return edge != null && _edgeSet.Contains(edge);
    }

    /// <inheritdoc/>
    public override bool ContainsVertex(TVertex? vertex)
    {
        return vertex != null && _vertexSet.Contains(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgeSet()
    {
        return _unmodifiableEdgeSet.Value;
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgesOf(TVertex vertex)
    {
        AssertVertexExist(vertex);
        return _base.EdgesOf(vertex).Where(_edgeSet.Contains).ToLinkedHashSet();
    }

    /// <summary>
    /// <inheritdoc/>
    ///
    /// <para>
    /// By default this method returns the sum of in-degree and out-degree. The exact value returned
    /// depends on the types of the underlying graph.
    /// </para>
    /// </summary>
    /// <returns>the degree of the specified vertex.</returns>
    public override int DegreeOf(TVertex vertex)
    {
        AssertVertexExist(vertex);

        if (_baseType.IsUndirected)
        {
            var degree = 0;
            foreach (var edge in _base.EdgesOf(vertex).Where(_edgeSet.Contains))
            {
                degree++;
                if (GetEdgeSource(edge).Equals(GetEdgeTarget(edge)))
                {
                    degree++;
                }
            }

            return degree;
        }
        else
        {
            return InDegreeOf(vertex) + OutDegreeOf(vertex);
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TVertex vertex)
    {
        AssertVertexExist(vertex);
        return _base.IncomingEdgesOf(vertex).Where(_edgeSet.Contains).ToLinkedHashSet();
    }

    /// <inheritdoc/>
    public override int InDegreeOf(TVertex vertex)
    {
        return _baseType.IsUndirected ? DegreeOf(vertex) : IncomingEdgesOf(vertex).Count;
    }

    /// <inheritdoc/>
    public override ISet<TEdge> OutgoingEdgesOf(TVertex vertex)
    {
        AssertVertexExist(vertex);
        return _base.OutgoingEdgesOf(vertex).Where(_edgeSet.Contains).ToLinkedHashSet();
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TVertex vertex)
    {
        return _baseType.IsUndirected ? DegreeOf(vertex) : OutgoingEdgesOf(vertex).Count;
    }

    /// <inheritdoc/>
    public override bool RemoveEdge(TEdge? edge)
    {
        return edge != null && _edgeSet.Remove(edge);
    }

    /// <inheritdoc/>
    public override TEdge? RemoveEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        var edge = GetEdge(sourceVertex, targetVertex);
        return edge != null && _edgeSet.Remove(edge) ? edge : null;
    }

    /// <inheritdoc/>
    public override bool RemoveVertex(TVertex? vertex)
    {
        // If the base graph does NOT contain TVertex it means we are here in
        // response to removal of TVertex from the base. In such case we don't need
        // to remove all the edges of TVertex as they were already removed.
        if (ContainsVertex(vertex) && _base.ContainsVertex(vertex))
        {
            RemoveAllEdges(EdgesOf(vertex!));
        }

        return vertex != null && _vertexSet.Remove(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TVertex> VertexSet()
    {
        return _unmodifiableVertexSet.Value;
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeSource(TEdge edge)
    {
        return _base.GetEdgeSource(edge);
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeTarget(TEdge edge)
    {
        return _base.GetEdgeTarget(edge);
    }

    /// <inheritdoc/>
    public override double GetEdgeWeight(TEdge edge)
    {
        return _base.GetEdgeWeight(edge);
    }

    /// <inheritdoc/>
    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        _base.SetEdgeWeight(edge, weight);
    }

    private void Initialize(ISet<TVertex>? vertexFilter, ISet<TEdge>? edgeFilter)
    {
        if (vertexFilter == null && edgeFilter == null)
        {
            _vertexSet.AddRange(_base.VertexSet());
            _edgeSet.AddRange(_base.EdgeSet());
            return;
        }

        // add vertices
        if (vertexFilter == null)
        {
            _vertexSet.AddRange(_base.VertexSet());
        }
        else
        {
            if (vertexFilter.Count > _base.VertexSet().Count)
            {
                foreach (var v in _base.VertexSet().Where(vertexFilter.Contains))
                {
                    _vertexSet.Add(v);
                }
            }
            else
            {
                foreach (var v in vertexFilter.Where(_base.ContainsVertex))
                {
                    _vertexSet.Add(v);
                }
            }
        }

        // add edges
        IEnumerable<TEdge> edges;
        if (edgeFilter == null)
        {
            edges = _base.EdgeSet()
                .Where(edge =>
                    _vertexSet.Contains(_base.GetEdgeSource(edge)) &&
                    _vertexSet.Contains(_base.GetEdgeTarget(edge))
                );
        }
        else if (edgeFilter.Count > _base.EdgeSet().Count)
        {
            edges = _base.EdgeSet()
                .Where(it =>
                    edgeFilter.Contains(it) &&
                    _vertexSet.Contains(_base.GetEdgeSource(it)) &&
                    _vertexSet.Contains(_base.GetEdgeTarget(it))
                );
        }
        else
        {
            edges = edgeFilter.Where(it =>
                _base.ContainsEdge(it) &&
                _vertexSet.Contains(_base.GetEdgeSource(it)) &&
                _vertexSet.Contains(_base.GetEdgeTarget(it))
            );
        }

        _edgeSet.AddRange(edges);
    }

    /// <summary>
    /// An internal listener on the base graph.
    /// </summary>
    /// <remarks>Author: Barak Naveh.</remarks>
    private sealed class BaseGraphListener : IGraphListener<TVertex, TEdge>
    {
        private readonly AsSubgraph<TVertex, TEdge> _owner;

        public BaseGraphListener(AsSubgraph<TVertex, TEdge> owner)
        {
            _owner = owner;
        }

        /// <inheritdoc/>
        public void EdgeAdded(GraphEdgeChangeEventArgs<TVertex, TEdge> e)
        {
            if (_owner.IsInduced)
            {
                var source = e.EdgeSource;
                var target = e.EdgeTarget;
                if (_owner.ContainsVertex(source) && _owner.ContainsVertex(target))
                {
                    _owner.AddEdge(source, target, e.Edge);
                }
            }
        }

        /// <inheritdoc/>
        public void EdgeRemoved(GraphEdgeChangeEventArgs<TVertex, TEdge> e)
        {
            _owner.RemoveEdge(e.Edge);
        }

        /// <inheritdoc/>
        public void VertexAdded(GraphVertexChangeEventArgs<TVertex> e)
        {
            // we don't care
        }

        /// <inheritdoc/>
        public void VertexRemoved(GraphVertexChangeEventArgs<TVertex> e)
        {
            _owner.RemoveVertex(e.Vertex);
        }

        public void EdgeWeightUpdated(GraphEdgeChangeEventArgs<TVertex, TEdge> e)
        {
            // do nothing
        }
    }
}
