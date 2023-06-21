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

using J2N.Collections.Generic.Extensions;
using NGraphT.Core.Util;

namespace NGraphT.Core.Graph;

/// <summary>
/// Read-only union of two graphs.
///
/// <para>
/// Read-only union of two graphs: G<sub>1</sub> and G<sub>2</sub>. If G<sub>1</sub> =
/// (TVertex<sub>1</sub>, TEdge<sub>1</sub>) and G<sub>2</sub> = (TVertex<sub>2</sub>, TEdge<sub>2</sub>) then their
/// union G = (TVertex, TEdge), where TVertex is the union of TVertex<sub>1</sub> and TVertex<sub>2</sub>,
/// and TEdge is the union of TEdge<sub>1</sub> and TEdge<sub>2</sub>.
/// A <see cref="WeightCombiner"/> in order to calculate edge weights.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex"> the vertex type.</typeparam>
/// <typeparam name="TEdge"> the edge type.</typeparam>
///
/// <remarks>Author: Ilya Razenshteyn.</remarks>
public class AsGraphUnion<TVertex, TEdge> : AbstractGraph<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private const string ReadOnly = "union of graphs is read-only";

    private readonly IGraph<TVertex, TEdge> _g1;
    private readonly IGraph<TVertex, TEdge> _g2;

    private readonly WeightCombiner _operator;

    /// <summary>
    /// Construct a new graph union.
    /// </summary>
    /// <param name="g1"> the first graph.</param>
    /// <param name="g2"> the second graph.</param>
    /// <param name="operator"> the weight combiner (policy for edge weight calculation).</param>
    public AsGraphUnion(IGraph<TVertex, TEdge> g1, IGraph<TVertex, TEdge> g2, WeightCombiner @operator)
    {
        ArgumentNullException.ThrowIfNull(g1);
        ArgumentNullException.ThrowIfNull(g2);
        if (ReferenceEquals(g1, g2))
        {
            throw new ArgumentException("g1 is the same as g2", nameof(g1));
        }

        // TODO: GraphTests.RequireDirectedOrUndirected(g1);
        // TODO: GraphTests.RequireDirectedOrUndirected(g2);
        _g1 = g1;
        _g2 = g2;

        _operator = @operator;

        // compute result type
        var type1   = g1.Type;
        var type2   = g2.Type;
        var builder = new DefaultGraphType.Builder();
        if (type1.IsDirected && type2.IsDirected)
        {
            builder = builder.Directed();
        }
        else if (type1.IsUndirected && type2.IsUndirected)
        {
            builder = builder.Undirected();
        }
        else
        {
            builder = builder.Mixed();
        }

        Type = builder
            .AllowSelfLoops(type1.IsAllowingSelfLoops || type2.IsAllowingSelfLoops)
            .AllowMultipleEdges(true)
            .Weighted(true)
            .Modifiable(false)
            .Build();
    }

    /// <summary>
    /// Construct a new graph union. The union will use the <see cref="WeightCombiners.Sum"/> weight
    /// combiner.
    /// </summary>
    /// <param name="g1"> the first graph.</param>
    /// <param name="g2"> the second graph.</param>
    public AsGraphUnion(IGraph<TVertex, TEdge> g1, IGraph<TVertex, TEdge> g2)
        : this(g1, g2, WeightCombiners.Sum)
    {
    }

    public override IGraphType Type { get; }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override Func<TVertex> VertexSupplier => throw new NotSupportedException(ReadOnly);

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override Func<TEdge> EdgeSupplier => throw new NotSupportedException(ReadOnly);

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TVertex? sourceVertex, TVertex? targetVertex)
    {
        var inG1 = _g1.ContainsVertex(sourceVertex) && _g1.ContainsVertex(targetVertex);
        var inG2 = _g2.ContainsVertex(sourceVertex) && _g2.ContainsVertex(targetVertex);

        if (inG1 && inG2)
        {
            return new ReadOnlySetUnion<TEdge>(
                _g1.GetAllEdges(sourceVertex, targetVertex),
                _g2.GetAllEdges(sourceVertex, targetVertex)
            );
        }

        if (inG1)
        {
            return _g1.GetAllEdges(sourceVertex, targetVertex).AsReadOnly();
        }

        if (inG2)
        {
            return _g2.GetAllEdges(sourceVertex, targetVertex).AsReadOnly();
        }

        return new HashSet<TEdge>();
    }

    /// <inheritdoc/>
    public override TEdge? GetEdge(TVertex? sourceVertex, TVertex? targetVertex)
    {
        TEdge? res = null;
        if (_g1.ContainsVertex(sourceVertex) && _g1.ContainsVertex(targetVertex))
        {
            res = _g1.GetEdge(sourceVertex, targetVertex);
        }

        if (res == null && _g2.ContainsVertex(sourceVertex) && _g2.ContainsVertex(targetVertex))
        {
            res = _g2.GetEdge(sourceVertex, targetVertex);
        }

        return res;
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override TEdge AddEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override TVertex AddVertex()
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override bool AddVertex(TVertex vertex)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    public override bool ContainsEdge(TEdge? edge)
    {
        return edge != null && (_g1.ContainsEdge(edge) || _g2.ContainsEdge(edge));
    }

    /// <inheritdoc/>
    public override bool ContainsVertex(TVertex? vertex)
    {
        return vertex != null && (_g1.ContainsVertex(vertex) || _g2.ContainsVertex(vertex));
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgeSet()
    {
        return new ReadOnlySetUnion<TEdge>(_g1.EdgeSet(), _g2.EdgeSet());
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgesOf(TVertex vertex)
    {
        var inG1 = _g1.ContainsVertex(vertex);
        var inG2 = _g2.ContainsVertex(vertex);

        if (inG1 && inG2)
        {
            return new ReadOnlySetUnion<TEdge>(_g1.EdgesOf(vertex), _g2.EdgesOf(vertex));
        }

        if (inG1)
        {
            return _g1.EdgesOf(vertex).AsReadOnly();
        }

        if (inG2)
        {
            return _g2.EdgesOf(vertex).AsReadOnly();
        }

        throw new ArgumentException($"no such vertex in graph: {vertex}", nameof(vertex));
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TVertex vertex)
    {
        var inG1 = _g1.ContainsVertex(vertex);
        var inG2 = _g2.ContainsVertex(vertex);

        if (inG1 && inG2)
        {
            return new ReadOnlySetUnion<TEdge>(_g1.IncomingEdgesOf(vertex), _g2.IncomingEdgesOf(vertex));
        }

        if (inG1)
        {
            return _g1.IncomingEdgesOf(vertex).AsReadOnly();
        }

        if (inG2)
        {
            return _g2.IncomingEdgesOf(vertex).AsReadOnly();
        }

        throw new ArgumentException($"no such vertex in graph: {vertex}", nameof(vertex));
    }

    /// <inheritdoc/>
    public override ISet<TEdge> OutgoingEdgesOf(TVertex vertex)
    {
        var inG1 = _g1.ContainsVertex(vertex);
        var inG2 = _g2.ContainsVertex(vertex);

        if (inG1 && inG2)
        {
            return new ReadOnlySetUnion<TEdge>(_g1.OutgoingEdgesOf(vertex), _g2.OutgoingEdgesOf(vertex));
        }

        if (inG1)
        {
            return _g1.OutgoingEdgesOf(vertex).AsReadOnly();
        }

        if (inG2)
        {
            return _g2.OutgoingEdgesOf(vertex).AsReadOnly();
        }

        throw new ArgumentException($"no such vertex in graph: {vertex}", nameof(vertex));
    }

    /// <inheritdoc/>
    public override int DegreeOf(TVertex vertex)
    {
        if (Type.IsMixed)
        {
            var d = 0;
            if (_g1.ContainsVertex(vertex))
            {
                d += _g1.DegreeOf(vertex);
            }

            if (_g2.ContainsVertex(vertex))
            {
                d += _g2.DegreeOf(vertex);
            }

            return d;
        }
        else if (Type.IsUndirected)
        {
            var degree = 0;
            foreach (var edge in EdgesOf(vertex))
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
            return IncomingEdgesOf(vertex).Count + OutgoingEdgesOf(vertex).Count;
        }
    }

    /// <inheritdoc/>
    public override int InDegreeOf(TVertex vertex)
    {
        if (Type.IsMixed)
        {
            var d = 0;
            if (_g1.ContainsVertex(vertex))
            {
                d += _g1.InDegreeOf(vertex);
            }

            if (_g2.ContainsVertex(vertex))
            {
                d += _g2.InDegreeOf(vertex);
            }

            return d;
        }
        else if (Type.IsUndirected)
        {
            return DegreeOf(vertex);
        }
        else
        {
            return IncomingEdgesOf(vertex).Count;
        }
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TVertex vertex)
    {
        if (Type.IsMixed)
        {
            var d = 0;
            if (_g1.ContainsVertex(vertex))
            {
                d += _g1.OutDegreeOf(vertex);
            }

            if (_g2.ContainsVertex(vertex))
            {
                d += _g2.OutDegreeOf(vertex);
            }

            return d;
        }
        else if (Type.IsUndirected)
        {
            return DegreeOf(vertex);
        }
        else
        {
            return OutgoingEdgesOf(vertex).Count;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override TEdge RemoveEdge(TVertex sourceVertex, TVertex targetVertex)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override bool RemoveEdge(TEdge? edge)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override bool RemoveVertex(TVertex? vertex)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    public override ISet<TVertex> VertexSet()
    {
        return new ReadOnlySetUnion<TVertex>(_g1.VertexSet(), _g2.VertexSet());
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeSource(TEdge edge)
    {
        if (_g1.ContainsEdge(edge))
        {
            return _g1.GetEdgeSource(edge);
        }

        if (_g2.ContainsEdge(edge))
        {
            return _g2.GetEdgeSource(edge);
        }

        throw new ArgumentException($"no such edge in graph union: {edge}", nameof(edge));
    }

    /// <inheritdoc/>
    public override TVertex GetEdgeTarget(TEdge edge)
    {
        if (_g1.ContainsEdge(edge))
        {
            return _g1.GetEdgeTarget(edge);
        }

        if (_g2.ContainsEdge(edge))
        {
            return _g2.GetEdgeTarget(edge);
        }

        throw new ArgumentException($"no such edge in graph union: {edge}", nameof(edge));
    }

    /// <inheritdoc/>
    public override double GetEdgeWeight(TEdge edge)
    {
        if (_g1.ContainsEdge(edge) && _g2.ContainsEdge(edge))
        {
            return _operator(_g1.GetEdgeWeight(edge), _g2.GetEdgeWeight(edge));
        }

        if (_g1.ContainsEdge(edge))
        {
            return _g1.GetEdgeWeight(edge);
        }

        if (_g2.ContainsEdge(edge))
        {
            return _g2.GetEdgeWeight(edge);
        }

        throw new ArgumentException($"no such edge in graph union: {edge}", nameof(edge));
    }

    /// <inheritdoc/>
    /// <summary>
    /// Throws <see cref="System.NotSupportedException"/> since graph union is read-only.
    /// </summary>
    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        throw new NotSupportedException(ReadOnly);
    }
}
