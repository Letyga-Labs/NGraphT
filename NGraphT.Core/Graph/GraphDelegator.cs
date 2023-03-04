/*
 * (C) Copyright 2003-2021, by Barak Naveh and Contributors.
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

using Core;

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
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public class GraphDelegator<TNode, TEdge> : AbstractGraph<TNode, TEdge>, IGraph<TNode, TEdge>
{
    /*
     * The graph to which operations are delegated.
     */
    private readonly IGraph<TNode, TEdge> _delegate;
    private readonly Func<TNode>   _vertexSupplier;
    private readonly Func<TEdge>   _edgeSupplier;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="graph"> the backing graph (the delegate).</param>
    public GraphDelegator(IGraph<TNode, TEdge> graph)
        : this(graph, null, null)
    {
    }

    /// 
    /// <param name="graph"> the backing graph (the delegate).</param>
    /// <param name="vertexSupplier"> vertex supplier for the delegator. Can be null in which case the
    ///        backing graph vertex supplier will be used.</param>
    /// <param name="edgeSupplier"> edge supplier for the delegator. Can be null in which case the backing
    ///        graph edge supplier will be used.</param>
    public GraphDelegator(
        IGraph<TNode, TEdge> graph,
        Func<TNode>   vertexSupplier,
        Func<TEdge>   edgeSupplier
    )
        : base()
    {
        _delegate       = Objects.requireNonNull(graph, "graph must not be null");
        _vertexSupplier = vertexSupplier;
        _edgeSupplier   = edgeSupplier;
    }

    /// <summary>
    /// {@inheritDoc}
    ///
    /// <para>
    /// Returns the delegator's vertex supplier or the backing graph's vertex supplier in case of
    /// null.
    /// </para>
    /// </summary>
    public override Func<TNode> VertexSupplier
    {
        get
        {
            if (_vertexSupplier != null)
            {
                return _vertexSupplier;
            }
            else
            {
                return _delegate.VertexSupplier;
            }
        }
    }

    /// <summary>
    /// {@inheritDoc}
    ///
    /// <para>
    /// Returns the delegator's edge supplier or the backing graph's edge supplier in case of null.
    /// </para>
    /// </summary>
    public override Func<TEdge> EdgeSupplier
    {
        get
        {
            if (_edgeSupplier != null)
            {
                return _edgeSupplier;
            }
            else
            {
                return _delegate.EdgeSupplier;
            }
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        return _delegate.GetAllEdges(sourceVertex, targetVertex);
    }

    /// <inheritdoc/>
    public override TEdge GetEdge(TNode sourceVertex, TNode targetVertex)
    {
        return _delegate.GetEdge(sourceVertex, targetVertex);
    }

    /// <inheritdoc/>
    public override TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
    {
        /*
         * Use our own edge supplier, if provided.
         */
        if (_edgeSupplier != null)
        {
            TEdge edge = _edgeSupplier.get();
            return AddEdge(sourceVertex, targetVertex, edge) ? edge : default(TEdge);
        }

        return _delegate.AddEdge(sourceVertex, targetVertex);
    }

    /// <inheritdoc/>
    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        return _delegate.AddEdge(sourceVertex, targetVertex, edge);
    }

    /// <inheritdoc/>
    public override TNode AddVertex()
    {
        /*
         * Use our own vertex supplier, if provided.
         */
        if (_vertexSupplier != null)
        {
            TNode node = _vertexSupplier.get();
            return AddVertex(node) ? node : default(TNode);
        }

        return _delegate.AddVertex();
    }

    /// <inheritdoc/>
    public override bool AddVertex(TNode node)
    {
        return _delegate.AddVertex(node);
    }

    /// <inheritdoc/>
    public override bool ContainsEdge(TEdge edge)
    {
        return _delegate.ContainsEdge(edge);
    }

    /// <inheritdoc/>
    public override bool ContainsVertex(TNode node)
    {
        return _delegate.ContainsVertex(node);
    }

    /// <summary>
    /// Returns the degree of the specified vertex.
    /// </summary>
    /// <param name="vertex"> vertex whose degree is to be calculated.</param>
    /// <returns>the degree of the specified vertex.</returns>
    public override int DegreeOf(TNode vertex)
    {
        return _delegate.DegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgeSet()
    {
        return _delegate.EdgeSet();
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgesOf(TNode vertex)
    {
        return _delegate.EdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override int InDegreeOf(TNode vertex)
    {
        return _delegate.InDegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TNode vertex)
    {
        return _delegate.IncomingEdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TNode vertex)
    {
        return _delegate.OutDegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> OutgoingEdgesOf(TNode vertex)
    {
        return _delegate.OutgoingEdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override bool RemoveEdge(TEdge edge)
    {
        return _delegate.RemoveEdge(edge);
    }

    /// <inheritdoc/>
    public override TEdge RemoveEdge(TNode sourceVertex, TNode targetVertex)
    {
        return _delegate.RemoveEdge(sourceVertex, targetVertex);
    }

    /// <inheritdoc/>
    public override bool RemoveVertex(TNode node)
    {
        return _delegate.RemoveVertex(node);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _delegate.ToString();
    }

    /// <inheritdoc/>
    public override ISet<TNode> VertexSet()
    {
        return _delegate.VertexSet();
    }

    /// <inheritdoc/>
    public override TNode GetEdgeSource(TEdge edge)
    {
        return _delegate.GetEdgeSource(edge);
    }

    /// <inheritdoc/>
    public override TNode GetEdgeTarget(TEdge edge)
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

    /// <inheritdoc/>
    public override IGraphType Type
    {
        get
        {
            return _delegate.Type;
        }
    }

    /// <summary>
    /// Return the backing graph (the delegate).
    /// </summary>
    /// <returns>the backing graph (the delegate)</returns>
    protected internal virtual IGraph<TNode, TEdge> Delegate
    {
        get
        {
            return _delegate;
        }
    }
}
