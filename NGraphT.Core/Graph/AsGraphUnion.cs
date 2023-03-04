/*
 * (C) Copyright 2009-2021, by Ilya Razenshteyn and Contributors.
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
using Util;

/// <summary>
/// Read-only union of two graphs.
///
/// <para>
/// Read-only union of two graphs: G<sub>1</sub> and G<sub>2</sub>. If G<sub>1</sub> =
/// (TNode<sub>1</sub>, TEdge<sub>1</sub>) and G<sub>2</sub> = (TNode<sub>2</sub>, TEdge<sub>2</sub>) then their
/// union G = (TNode, TEdge), where TNode is the union of TNode<sub>1</sub> and TNode<sub>2</sub>, and TEdge is the union of
/// TEdge<sub>1</sub> and TEdge<sub>2</sub>. A <seealso cref="IWeightCombiner"/> in order to calculate edge weights.
///
/// </para>
/// </summary>
/// @param <TNode> the vertex type.</param>
/// @param <TEdge> the edge type
///
/// <remarks>Author: Ilya Razenshteyn.</remarks>
public class AsGraphUnion<TNode, TEdge> : AbstractGraph<TNode, TEdge>
{
    private const string ReadOnly = "union of graphs is read-only";

    private readonly IGraph<TNode, TEdge> _g1;
    private readonly IGraphType           _type1;
    private readonly IGraph<TNode, TEdge> _g2;
    private readonly IGraphType           _type2;
    public virtual   Type { get; }
    private readonly IWeightCombiner _operator;

    /// <summary>
    /// Construct a new graph union.
    /// </summary>
    /// <param name="g1"> the first graph.</param>
    /// <param name="g2"> the second graph.</param>
    /// <param name="operator"> the weight combiner (policy for edge weight calculation) </param>
    public AsGraphUnion(IGraph<TNode, TEdge> g1, IGraph<TNode, TEdge> g2, IWeightCombiner @operator)
    {
        _g1    = GraphTests.RequireDirectedOrUndirected(g1);
        _type1 = g1.Type;

        _g2    = GraphTests.RequireDirectedOrUndirected(g2);
        _type2 = g2.Type;

        if (g1 == g2)
        {
            throw new ArgumentException("g1 is equal to g2");
        }

        _operator = Objects.requireNonNull(@operator, "Weight combiner cannot be null");

        // compute result type
        var builder = new DefaultGraphType.Builder();
        if (_type1.Directed && _type2.Directed)
        {
            builder = builder.Directed();
        }
        else if (_type1.Undirected && _type2.Undirected)
        {
            builder = builder.Undirected();
        }
        else
        {
            builder = builder.Mixed();
        }

        Type = builder.AllowSelfLoops(_type1.AllowingSelfLoops || _type2.AllowingSelfLoops)
            .AllowMultipleEdges(true).Weighted(true).Modifiable(false).Build();
    }

    /// <summary>
    /// Construct a new graph union. The union will use the <seealso cref="IIWeightCombiner.Sum/> weight
    /// combiner.
    /// </summary>
    /// <param name="g1"> the first graph.</param>
    /// <param name="g2"> the second graph.</param>
    public AsGraphUnion(IGraph<TNode, TEdge> g1, IGraph<TNode, TEdge> g2)
        : this(g1, g2, IWeightCombiner.Sum)
    {
    }

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        var inG1 = _g1.ContainsVertex(sourceVertex) && _g1.ContainsVertex(targetVertex);
        var inG2 = _g2.ContainsVertex(sourceVertex) && _g2.ContainsVertex(targetVertex);

        if (inG1 && inG2)
        {
            return new UnmodifiableUnionSet<TEdge>(_g1.GetAllEdges(sourceVertex, targetVertex),
                _g2.GetAllEdges(sourceVertex, targetVertex)
            );
        }
        else if (inG1)
        {
            return Collections.unmodifiableSet(_g1.GetAllEdges(sourceVertex, targetVertex));
        }
        else if (inG2)
        {
            return Collections.unmodifiableSet(_g2.GetAllEdges(sourceVertex, targetVertex));
        }

        return java.util.Collections.emptySet();
    }

    /// <inheritdoc/>
    public override TEdge GetEdge(TNode sourceVertex, TNode targetVertex)
    {
        var res = default(TEdge);
        if (_g1.ContainsVertex(sourceVertex) && _g1.ContainsVertex(targetVertex))
        {
            res = _g1.GetEdge(sourceVertex, targetVertex);
        }

        if ((res == null) && _g2.ContainsVertex(sourceVertex) && _g2.ContainsVertex(targetVertex))
        {
            res = _g2.GetEdge(sourceVertex, targetVertex);
        }

        return res;
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override Func<TNode> VertexSupplier
    {
        get
        {
            throw new NotSupportedException(ReadOnly);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override Func<TEdge> EdgeSupplier
    {
        get
        {
            throw new NotSupportedException(ReadOnly);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override TNode AddVertex()
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override bool AddVertex(TNode node)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    public override bool ContainsEdge(TEdge edge)
    {
        return _g1.ContainsEdge(edge) || _g2.ContainsEdge(edge);
    }

    /// <inheritdoc/>
    public override bool ContainsVertex(TNode node)
    {
        return _g1.ContainsVertex(node) || _g2.ContainsVertex(node);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgeSet()
    {
        return new UnmodifiableUnionSet<TEdge>(_g1.EdgeSet(), _g2.EdgeSet());
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgesOf(TNode vertex)
    {
        var inG1 = _g1.ContainsVertex(vertex);
        var inG2 = _g2.ContainsVertex(vertex);

        if (inG1 && inG2)
        {
            return new UnmodifiableUnionSet<TEdge>(_g1.EdgesOf(vertex), _g2.EdgesOf(vertex));
        }
        else if (inG1)
        {
            return Collections.unmodifiableSet(_g1.EdgesOf(vertex));
        }
        else if (inG2)
        {
            return Collections.unmodifiableSet(_g2.EdgesOf(vertex));
        }
        else
        {
            throw new ArgumentException("no such vertex in graph: " + vertex.ToString());
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TNode vertex)
    {
        var inG1 = _g1.ContainsVertex(vertex);
        var inG2 = _g2.ContainsVertex(vertex);

        if (inG1 && inG2)
        {
            return new UnmodifiableUnionSet<TEdge>(_g1.IncomingEdgesOf(vertex), _g2.IncomingEdgesOf(vertex));
        }
        else if (inG1)
        {
            return Collections.unmodifiableSet(_g1.IncomingEdgesOf(vertex));
        }
        else if (inG2)
        {
            return Collections.unmodifiableSet(_g2.IncomingEdgesOf(vertex));
        }
        else
        {
            throw new ArgumentException("no such vertex in graph: " + vertex.ToString());
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> OutgoingEdgesOf(TNode vertex)
    {
        var inG1 = _g1.ContainsVertex(vertex);
        var inG2 = _g2.ContainsVertex(vertex);

        if (inG1 && inG2)
        {
            return new UnmodifiableUnionSet<TEdge>(_g1.OutgoingEdgesOf(vertex), _g2.OutgoingEdgesOf(vertex));
        }
        else if (inG1)
        {
            return Collections.unmodifiableSet(_g1.OutgoingEdgesOf(vertex));
        }
        else if (inG2)
        {
            return Collections.unmodifiableSet(_g2.OutgoingEdgesOf(vertex));
        }
        else
        {
            throw new ArgumentException("no such vertex in graph: " + vertex.ToString());
        }
    }

    /// <inheritdoc/>
    public override int DegreeOf(TNode vertex)
    {
        if (Type.Mixed)
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
        else if (Type.Undirected)
        {
            var                degree = 0;
            IEnumerator<TEdge> it     = EdgesOf(vertex).GetEnumerator();
            while (it.MoveNext())
            {
                var edge = it.Current;
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
    public override int InDegreeOf(TNode vertex)
    {
        if (Type.Mixed)
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
        else if (Type.Undirected)
        {
            return DegreeOf(vertex);
        }
        else
        {
            return IncomingEdgesOf(vertex).Count;
        }
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TNode vertex)
    {
        if (Type.Mixed)
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
        else if (Type.Undirected)
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
    public override TEdge RemoveEdge(TNode sourceVertex, TNode targetVertex)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override bool RemoveEdge(TEdge edge)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"> always, since operation is unsupported.</exception>
    public override bool RemoveVertex(TNode node)
    {
        throw new NotSupportedException(ReadOnly);
    }

    /// <inheritdoc/>
    public override ISet<TNode> VertexSet()
    {
        return new UnmodifiableUnionSet<TNode>(_g1.VertexSet(), _g2.VertexSet());
    }

    /// <inheritdoc/>
    public override TNode GetEdgeSource(TEdge edge)
    {
        if (_g1.ContainsEdge(edge))
        {
            return _g1.GetEdgeSource(edge);
        }

        if (_g2.ContainsEdge(edge))
        {
            return _g2.GetEdgeSource(edge);
        }

        return default(TNode);
    }

    /// <inheritdoc/>
    public override TNode GetEdgeTarget(TEdge edge)
    {
        if (_g1.ContainsEdge(edge))
        {
            return _g1.GetEdgeTarget(edge);
        }

        if (_g2.ContainsEdge(edge))
        {
            return _g2.GetEdgeTarget(edge);
        }

        return default(TNode);
    }

    /// <inheritdoc/>
    public override double GetEdgeWeight(TEdge edge)
    {
        if (_g1.ContainsEdge(edge) && _g2.ContainsEdge(edge))
        {
            return _operator.Combine(_g1.GetEdgeWeight(edge), _g2.GetEdgeWeight(edge));
        }

        if (_g1.ContainsEdge(edge))
        {
            return _g1.GetEdgeWeight(edge);
        }

        if (_g2.ContainsEdge(edge))
        {
            return _g2.GetEdgeWeight(edge);
        }

        throw new ArgumentException("no such edge in the union");
    }

    /// <inheritdoc/>
    /// <summary>
    /// Throws <seealso cref="System.NotSupportedException"/> since graph union is read-only.
    /// </summary>
    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        throw new NotSupportedException(ReadOnly);
    }
}
