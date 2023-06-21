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

namespace NGraphT.Core.Graph;

/// <summary>
/// A graph backed by the the graph specified at the constructor, which delegates all its methods to
/// the backing graph. Operations on this graph "pass through" to the to the backing graph. Any
/// modification made to this graph or the backing graph is reflected by the other.
///
/// <para>
/// This graph does <i>not</i> pass the hashCode and equals operations through to the backing graph,
/// but relies on <c>Object</c>'s <c>equals</c> and <c>hashCode</c> methods.
/// </para>
///
/// <para>
/// This class is mostly used as a base for extending subclasses. It can also be used in order to
/// override the vertex and edge supplier of a graph.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public class GraphDelegator<TVertex, TEdge> : AbstractGraph<TVertex, TEdge>, IGraph<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private readonly IGraph<TVertex, TEdge> _delegate;
    private readonly Func<TVertex>?         _vertexSupplier;
    private readonly Func<TEdge>?           _edgeSupplier;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="graph"> the backing graph (the delegate).</param>
    public GraphDelegator(IGraph<TVertex, TEdge> graph)
        : this(graph, null, null)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="graph"> the backing graph (the delegate).</param>
    /// <param name="vertexSupplier"> vertex supplier for the delegator. Can be null in which case the
    ///        backing graph vertex supplier will be used.</param>
    /// <param name="edgeSupplier"> edge supplier for the delegator. Can be null in which case the backing
    ///        graph edge supplier will be used.</param>
    public GraphDelegator(
        IGraph<TVertex, TEdge> graph,
        Func<TVertex>?         vertexSupplier,
        Func<TEdge>?           edgeSupplier
    )
    {
        ArgumentNullException.ThrowIfNull(graph);

        _delegate       = graph;
        _vertexSupplier = vertexSupplier;
        _edgeSupplier   = edgeSupplier;
    }

    /// <inheritdoc/>
    public override IGraphType Type => _delegate.Type;

    /// <inheritdoc/>
    public override Func<TVertex>? VertexSupplier => _vertexSupplier ?? _delegate.VertexSupplier;

    /// <inheritdoc/>
    public override Func<TEdge>? EdgeSupplier => _edgeSupplier ?? _delegate.EdgeSupplier;

    /// <summary>
    /// The backing graph (the delegate).
    /// </summary>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
    protected virtual IGraph<TVertex, TEdge> Delegate => _delegate;

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TVertex? sourceVertex, TVertex? targetVertex)
    {
        return _delegate.GetAllEdges(sourceVertex, targetVertex);
    }

    /// <inheritdoc/>
    public override TEdge? GetEdge(TVertex? sourceVertex, TVertex? targetVertex)
    {
        return _delegate.GetEdge(sourceVertex, targetVertex);
    }

    /// <inheritdoc/>
    public override TEdge? AddEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        // Use our own edge supplier, if provided.
        if (_edgeSupplier != null)
        {
            var edge = _edgeSupplier();
            return AddEdge(sourceVertex, targetVertex, edge) ? edge : null;
        }

        return _delegate.AddEdge(sourceVertex, targetVertex);
    }

    /// <inheritdoc/>
    public override bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        return _delegate.AddEdge(sourceVertex, targetVertex, edge);
    }

    /// <inheritdoc/>
    public override TVertex AddVertex()
    {
        // Use our own vertex supplier, if provided.
        if (_vertexSupplier != null)
        {
            var v = _vertexSupplier();
            if (AddVertex(v))
            {
                return v;
            }
            else
            {
#pragma warning disable MA0015
                throw new ArgumentException("vertex supplier returned vertex which is already in the graph");
#pragma warning restore MA0015
            }
        }

        return _delegate.AddVertex();
    }

    /// <inheritdoc/>
    public override bool AddVertex(TVertex vertex)
    {
        return _delegate.AddVertex(vertex);
    }

    /// <inheritdoc/>
    public override bool ContainsEdge([NotNullWhen(true)] TEdge? edge)
    {
        return _delegate.ContainsEdge(edge);
    }

    /// <inheritdoc/>
    public override bool ContainsVertex([NotNullWhen(true)] TVertex? vertex)
    {
        return _delegate.ContainsVertex(vertex);
    }

    /// <summary>
    /// Returns the degree of the specified vertex.
    /// </summary>
    /// <param name="vertex"> vertex whose degree is to be calculated.</param>
    /// <returns>the degree of the specified vertex.</returns>
    public override int DegreeOf(TVertex vertex)
    {
        return _delegate.DegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgeSet()
    {
        return _delegate.EdgeSet();
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgesOf(TVertex vertex)
    {
        return _delegate.EdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override int InDegreeOf(TVertex vertex)
    {
        return _delegate.InDegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TVertex vertex)
    {
        return _delegate.IncomingEdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TVertex vertex)
    {
        return _delegate.OutDegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> OutgoingEdgesOf(TVertex vertex)
    {
        return _delegate.OutgoingEdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override bool RemoveEdge(TEdge? edge)
    {
        return _delegate.RemoveEdge(edge);
    }

    /// <inheritdoc/>
    public override TEdge? RemoveEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        return _delegate.RemoveEdge(sourceVertex, targetVertex);
    }

    /// <inheritdoc/>
    public override bool RemoveVertex(TVertex? vertex)
    {
        return _delegate.RemoveVertex(vertex);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _delegate.ToString()!;
    }

    /// <inheritdoc/>
    public override ISet<TVertex> VertexSet()
    {
        return _delegate.VertexSet();
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeSource(TEdge edge)
    {
        return _delegate.GetEdgeSource(edge);
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeTarget(TEdge edge)
    {
        return _delegate.GetEdgeTarget(edge);
    }

    /// <inheritdoc/>
    public override double GetEdgeWeight(TEdge edge)
    {
        return _delegate.GetEdgeWeight(edge);
    }

    /// <inheritdoc/>
    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        _delegate.SetEdgeWeight(edge, weight);
    }
}
