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

using System.Collections.Immutable;
using System.Diagnostics;

/*
 * (C) Copyright 2007-2021, by France Telecom and Contributors.
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
/// An unmodifiable subgraph induced by a vertex/edge masking function. The subgraph will keep track
/// of edges being added to its vertex subset as well as deletion of edges and vertices. When
/// iterating over the vertices/edges, it will iterate over the vertices/edges of the base graph and
/// discard vertices/edges that are masked (an edge with a masked extremity vertex is discarded as well).
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
public sealed class MaskSubgraph<TVertex, TEdge> : AbstractGraph<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private const string UNMODIFIABLE = "this graph is unmodifiable";

    private readonly IGraph<TVertex, TEdge> _base;
    private readonly IGraphType             _baseType;

    private readonly ISet<TEdge>   _edges;
    private readonly ISet<TVertex> _vertices;

    private readonly Predicate<TVertex> _vertexMask;
    private readonly Predicate<TEdge>   _edgeMask;

    /// <summary>
    /// Creates a new induced subgraph. Running-time = O(1).
    /// </summary>
    /// <param name="base"> the base (backing) graph on which the subgraph will be based.</param>
    /// <param name="vertexMask">
    /// vertices to exclude in the subgraph. If a vertex is masked, it is as if it
    /// is not in the subgraph. Edges incident to the masked vertex are also masked.
    /// </param>
    /// <param name="edgeMask">
    /// edges to exclude in the subgraph. If an edge is masked, it is as if it is not in the subgraph.
    /// </param>
    public MaskSubgraph(
        IGraph<TVertex, TEdge> @base,
        Predicate<TVertex>     vertexMask,
        Predicate<TEdge>       edgeMask
    )
    {
        ArgumentNullException.ThrowIfNull(@base);

        _base       = @base;
        _baseType   = @base.Type;
        _vertexMask = vertexMask;
        _edgeMask   = edgeMask;
        _vertices   = new MaskVertexSet<TVertex>(@base.VertexSet(), vertexMask);
        _edges      = new MaskEdgeSet<TVertex, TEdge>(@base, @base.EdgeSet(), vertexMask, edgeMask);
    }

    /// <inheritdoc/>
    public override Func<TVertex>? VertexSupplier => _base.VertexSupplier;

    /// <inheritdoc/>
    public override Func<TEdge>? EdgeSupplier => _base.EdgeSupplier;

    /// <inheritdoc/>
    public override IGraphType Type => _baseType.AsUnmodifiable();

    /// <inheritdoc/>
    public override ISet<TVertex> VertexSet()
    {
        return _vertices;
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgeSet()
    {
        return _edges;
    }

    /// <inheritdoc/>
    public override TEdge AddEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override TVertex AddVertex()
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool AddVertex(TVertex vertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool ContainsEdge(TEdge? edge)
    {
        return edge != null && EdgeSet().Contains(edge);
    }

    /// <inheritdoc/>
    public override bool ContainsVertex(TVertex? vertex)
    {
        return vertex != null && VertexSet().Contains(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgesOf(TVertex vertex)
    {
        AssertVertexExist(vertex);

        return new MaskEdgeSet<TVertex, TEdge>(_base, _base.EdgesOf(vertex), _vertexMask, _edgeMask);
    }

    /// <summary>
    /// <inheritdoc/>
    ///
    /// <para>
    /// By default this method returns the sum of in-degree and out-degree. The exact value returned
    /// depends on the type of the underlying graph.
    /// </para>
    /// </summary>
    ///
    /// <returns>the degree of the specified vertex.</returns>
    public override int DegreeOf(TVertex vertex)
    {
        if (_baseType.IsDirected)
        {
            return InDegreeOf(vertex) + OutDegreeOf(vertex);
        }

        return EdgesOf(vertex).Count(edge => GetEdgeSource(edge).Equals(GetEdgeTarget(edge)));
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TVertex vertex)
    {
        AssertVertexExist(vertex);

        return new MaskEdgeSet<TVertex, TEdge>(_base, _base.IncomingEdgesOf(vertex), _vertexMask, _edgeMask);
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

        return new MaskEdgeSet<TVertex, TEdge>(_base, _base.OutgoingEdgesOf(vertex), _vertexMask, _edgeMask);
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TVertex vertex)
    {
        return _baseType.IsUndirected ? DegreeOf(vertex) : OutgoingEdgesOf(vertex).Count;
    }

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TVertex? sourceVertex, TVertex? targetVertex)
    {
        if (ContainsVertex(sourceVertex) && ContainsVertex(targetVertex))
        {
            return new MaskEdgeSet<TVertex, TEdge>(
                _base,
                _base.GetAllEdges(sourceVertex, targetVertex),
                _vertexMask,
                _edgeMask
            );
        }
        else
        {
            return ImmutableHashSet<TEdge>.Empty;
        }
    }

    /// <inheritdoc/>
    public override TEdge? GetEdge(TVertex? sourceVertex, TVertex? targetVertex)
    {
        var edges = GetAllEdges(sourceVertex, targetVertex);
        return edges.FirstOrDefault();
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeSource(TEdge edge)
    {
        Debug.Assert(EdgeSet().Contains(edge));
        return _base.GetEdgeSource(edge);
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeTarget(TEdge edge)
    {
        Debug.Assert(EdgeSet().Contains(edge));
        return _base.GetEdgeTarget(edge);
    }

    /// <inheritdoc/>
    public override double GetEdgeWeight(TEdge edge)
    {
        Debug.Assert(EdgeSet().Contains(edge));
        return _base.GetEdgeWeight(edge);
    }

    /// <inheritdoc/>
    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        Debug.Assert(EdgeSet().Contains(edge));
        _base.SetEdgeWeight(edge, weight);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> RemoveAllEdges(TVertex sourceVertex, TVertex targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool RemoveAllEdges(IEnumerable<TEdge> edges)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool RemoveAllVertices(IEnumerable<TVertex> vertices)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool RemoveEdge(TEdge? edge)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override TEdge RemoveEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool RemoveVertex(TVertex? vertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }
}
