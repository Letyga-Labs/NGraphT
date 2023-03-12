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

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using NGraphT.Core.Graph.Concurrent;
using NGraphT.Core.Graph.Specifics;

namespace NGraphT.Core.Graph;

/// <summary>
/// The most general implementation of the <see cref="NGraphT.Core.Graph"/> interface.
///
/// <para>
/// Its subclasses add various restrictions to get more specific graphs. The decision whether it is
/// directed or undirected is decided at construction time and cannot be later modified (see
/// constructor for details).
/// </para>
///
/// <para>
/// The behavior of this class can be adjusted by changing the <see cref="IGraphSpecificsStrategy{TNode,TEdge}"/>
/// that is provided from the constructor. All implemented strategies guarantee deterministic vertex and edge
/// set ordering (via <see cref="J2N.Collections.Generic.LinkedDictionary{TKey,TValue}"/>
/// and <see cref="J2N.Collections.Generic.LinkedHashSet{T}"/>). The defaults are reasonable
/// for most use-cases, only change if you know what you are doing.
/// </para>
///
/// <para>
/// The default graph implementations are not safe for concurrent reads and writes from different
/// threads. If an application attempts to modify a graph in one thread while another thread is
/// reading or writing the same graph, undefined behavior will result. However, concurrent reads
/// against the same graph from different threads are safe. (Note that the {@link NGraphT.Core.Graph
/// Graph interface} itself makes no such guarantee, so for non-default implementations, different
/// rules may apply.)
/// </para>
///
/// <para>
/// If you need support for concurrent reads and writes, consider using the
/// <see cref="AsSynchronizedGraph{TNode,TEdge}"/>.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
/// <remarks>Author: Dimitrios Michail.</remarks>
public abstract class AbstractBaseGraph<TNode, TEdge> : AbstractGraph<TNode, TEdge>, IGraph<TNode, TEdge>
    where TNode : class
    where TEdge : class
{
    private const string LoopsNotAllowed             = "loops not allowed";
    private const string GraphSpecificsMustNotBeNull = "Graph specifics must not be null";

    private readonly ISpecifics<TNode, TEdge>               _specifics;
    private readonly IIntrusiveEdgesSpecifics<TNode, TEdge> _intrusiveEdgesSpecifics;

    [SuppressMessage(
        "Critical Code Smell",
        "S4487:Unread \"private\" fields should be removed",
        Justification = "Should be used later in Copy implementation"
    )]
    private readonly IGraphSpecificsStrategy<TNode, TEdge> _graphSpecificsStrategy;

    private readonly Lazy<ISet<TNode>>                   _unmodifiableVertexSet;
    private readonly Lazy<IGraphIterables<TNode, TEdge>> _graphIterables;

    /// <summary>
    /// Construct a new graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier, can be null.</param>
    /// <param name="edgeSupplier"> the edge supplier, can be null.</param>
    /// <param name="type"> the graph type.</param>>
    /// <exception cref="ArgumentException"> if the graph type is mixed.</exception>
    protected AbstractBaseGraph(
        Func<TNode> vertexSupplier,
        Func<TEdge> edgeSupplier,
        IGraphType  type
    )
        : this(vertexSupplier, edgeSupplier, type, new FastLookupGraphSpecificsStrategy<TNode, TEdge>())
    {
    }

    /// <summary>
    /// Construct a new graph.
    /// </summary>
    /// <param name="vertexSupplier"> the vertex supplier, can be null.</param>
    /// <param name="edgeSupplier"> the edge supplier, can be null.</param>
    /// <param name="type"> the graph type.</param>
    /// <param name="graphSpecificsStrategy"> strategy for constructing low-level graph specifics.</param>>
    /// <exception cref="ArgumentException"> if the graph type is mixed.</exception>
    protected AbstractBaseGraph(
        Func<TNode>                           vertexSupplier,
        Func<TEdge>                           edgeSupplier,
        IGraphType                            type,
        IGraphSpecificsStrategy<TNode, TEdge> graphSpecificsStrategy)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(graphSpecificsStrategy);

        VertexSupplier = vertexSupplier;
        EdgeSupplier   = edgeSupplier;

        Type = type;
        if (type.Mixed)
        {
            throw new ArgumentException("Mixed graph not supported", nameof(type));
        }

        _graphSpecificsStrategy = graphSpecificsStrategy;

        _specifics =
            graphSpecificsStrategy.SpecificsFactory(this, type) ??
            throw new InvalidOperationException(GraphSpecificsMustNotBeNull);

        _intrusiveEdgesSpecifics =
            graphSpecificsStrategy.IntrusiveEdgesSpecificsFactory(type) ??
            throw new InvalidOperationException(GraphSpecificsMustNotBeNull);

        _unmodifiableVertexSet = new Lazy<ISet<TNode>>(() => _specifics.VertexSet.ToImmutableHashSet());
        _graphIterables = new Lazy<IGraphIterables<TNode, TEdge>>(() => new DefaultGraphIterables<TNode, TEdge>(this));
    }

    public override Func<TEdge> EdgeSupplier { get; }

    public override Func<TNode> VertexSupplier { get; }

    public override IGraphType Type { get; }

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TNode? sourceVertex, TNode? targetVertex)
    {
        return _specifics.GetAllEdges(sourceVertex, targetVertex);
    }

    public abstract override void SetEdgeWeight(TNode sourceVertex, TNode targetVertex, double weight);

    /// <inheritdoc/>
    public override TEdge? GetEdge(TNode? sourceVertex, TNode? targetVertex)
    {
        return _specifics.GetEdge(sourceVertex, targetVertex);
    }

    /// <inheritdoc/>
    public override TEdge? AddEdge(TNode sourceVertex, TNode targetVertex)
    {
        AssertVertexExist(sourceVertex);
        AssertVertexExist(targetVertex);

        if (!Type.AllowingSelfLoops && sourceVertex.Equals(targetVertex))
        {
            throw new ArgumentException(LoopsNotAllowed, nameof(sourceVertex));
        }

        if (EdgeSupplier == null)
        {
            throw new NotSupportedException("The graph contains no edge supplier");
        }

        if (!Type.AllowingMultipleEdges)
        {
            var edge = _specifics.CreateEdgeToTouchingVerticesIfAbsent(sourceVertex, targetVertex, EdgeSupplier);
            if (edge == null)
            {
                return null;
            }

            var edgeAdded = false;
            try
            {
                edgeAdded = _intrusiveEdgesSpecifics.Add(edge, sourceVertex, targetVertex);
            }
            finally
            {
                if (!edgeAdded)
                {
                    // edge was already present or adding threw an exception -> revert add
                    _specifics.RemoveEdgeFromTouchingVertices(sourceVertex, targetVertex, edge);
                }
            }

            if (edgeAdded)
            {
                return edge;
            }
        }
        else
        {
            var edge = EdgeSupplier();
            if (_intrusiveEdgesSpecifics.Add(edge, sourceVertex, targetVertex))
            {
                _specifics.AddEdgeToTouchingVertices(sourceVertex, targetVertex, edge);
                return edge;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);

        AssertVertexExist(sourceVertex);
        AssertVertexExist(targetVertex);

        if (!Type.AllowingSelfLoops && sourceVertex.Equals(targetVertex))
        {
            throw new ArgumentException(LoopsNotAllowed, nameof(sourceVertex));
        }

        if (!Type.AllowingMultipleEdges)
        {
            if (!_specifics.AddEdgeToTouchingVerticesIfAbsent(sourceVertex, targetVertex, edge))
            {
                return false;
            }

            var edgeAdded = false;
            try
            {
                edgeAdded = _intrusiveEdgesSpecifics.Add(edge, sourceVertex, targetVertex);
            }
            finally
            {
                if (!edgeAdded)
                {
                    // edge was already present or adding threw an exception -> revert add
                    _specifics.RemoveEdgeFromTouchingVertices(sourceVertex, targetVertex, edge);
                }
            }

            return edgeAdded;
        }

        if (_intrusiveEdgesSpecifics.Add(edge, sourceVertex, targetVertex))
        {
            _specifics.AddEdgeToTouchingVertices(sourceVertex, targetVertex, edge);
            return true;
        }

        return false;
    }

    [SuppressMessage("Usage", "MA0015:Specify the parameter name in ArgumentException")]
    public override TNode AddVertex()
    {
        if (VertexSupplier == null)
        {
            throw new NotSupportedException("The graph contains no vertex supplier");
        }

        var node = VertexSupplier();
        if (!_specifics.AddVertex(node))
        {
            throw new ArgumentException("Invalid vertex supplier (does not return unique vertices on each call).");
        }

        return node;
    }

    /// <inheritdoc/>
    public override bool AddVertex(TNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (ContainsVertex(node))
        {
            return false;
        }
        else
        {
            _specifics.AddVertex(node);
            return true;
        }
    }

    /// <inheritdoc/>
    public override TNode GetEdgeSource(TEdge edge)
    {
        return _intrusiveEdgesSpecifics.GetEdgeSource(edge);
    }

    /// <inheritdoc/>
    public override TNode GetEdgeTarget(TEdge edge)
    {
        return _intrusiveEdgesSpecifics.GetEdgeTarget(edge);
    }

    /// <inheritdoc/>
    public override bool ContainsEdge([NotNullWhen(true)] TEdge? edge)
    {
        return edge != null && _intrusiveEdgesSpecifics.ContainsEdge(edge);
    }

    /// <inheritdoc/>
    public override bool ContainsVertex([NotNullWhen(true)] TNode? node)
    {
        return node != null && _specifics.VertexSet.Contains(node);
    }

    /// <inheritdoc/>
    public override int DegreeOf(TNode vertex)
    {
        AssertVertexExist(vertex);
        return _specifics.DegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgeSet()
    {
        return _intrusiveEdgesSpecifics.EdgeSet;
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgesOf(TNode vertex)
    {
        AssertVertexExist(vertex);
        return _specifics.EdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override int InDegreeOf(TNode vertex)
    {
        AssertVertexExist(vertex);
        return _specifics.InDegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TNode vertex)
    {
        AssertVertexExist(vertex);
        return _specifics.IncomingEdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TNode vertex)
    {
        AssertVertexExist(vertex);
        return _specifics.OutDegreeOf(vertex);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> OutgoingEdgesOf(TNode vertex)
    {
        AssertVertexExist(vertex);
        return _specifics.OutgoingEdgesOf(vertex);
    }

    /// <inheritdoc/>
    public override TEdge? RemoveEdge(TNode sourceVertex, TNode targetVertex)
    {
        var edge = GetEdge(sourceVertex, targetVertex);

        if (edge != null)
        {
            _specifics.RemoveEdgeFromTouchingVertices(sourceVertex, targetVertex, edge);
            _intrusiveEdgesSpecifics.Remove(edge);
        }

        return edge;
    }

    /// <inheritdoc/>
    public override bool RemoveEdge(TEdge? edge)
    {
        if (ContainsEdge(edge))
        {
            var sourceVertex = GetEdgeSource(edge);
            var targetVertex = GetEdgeTarget(edge);
            _specifics.RemoveEdgeFromTouchingVertices(sourceVertex, targetVertex, edge);
            _intrusiveEdgesSpecifics.Remove(edge);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool RemoveVertex(TNode? node)
    {
        if (ContainsVertex(node))
        {
            var touchingEdgesList = EdgesOf(node);
            // cannot iterate over list - will cause ConcurrentModificationException
            RemoveAllEdges(new List<TEdge>(touchingEdgesList));
            _specifics.VertexSet.Remove(node); // remove the vertex itself
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override ISet<TNode> VertexSet()
    {
        return _unmodifiableVertexSet.Value;
    }

    /// <inheritdoc/>
    public override double GetEdgeWeight(TEdge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);
        return _intrusiveEdgesSpecifics.GetEdgeWeight(edge);
    }

    /// <summary>
    /// Set an edge weight.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <param name="weight"> the weight.</param>
    /// <exception cref="NotSupportedException"> if the graph is not weighted.</exception>
    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        ArgumentNullException.ThrowIfNull(edge);
        _intrusiveEdgesSpecifics.SetEdgeWeight(edge, weight);
    }

    /// <inheritdoc/>
    public override IGraphIterables<TNode, TEdge> Iterables()
    {
        return _graphIterables.Value;
    }
}
