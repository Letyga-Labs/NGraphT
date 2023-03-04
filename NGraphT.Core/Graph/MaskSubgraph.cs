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

using Core;

/// <summary>
/// An unmodifiable subgraph induced by a vertex/edge masking function. The subgraph will keep track
/// of edges being added to its vertex subset as well as deletion of edges and vertices. When
/// iterating over the vertices/edges, it will iterate over the vertices/edges of the base graph and
/// discard vertices/edges that are masked (an edge with a masked extremity vertex is discarded as
/// well).
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///  </param>
public class MaskSubgraph<TNode, TEdge> : AbstractGraph<TNode, TEdge>
{
    private const string UNMODIFIABLE = "this graph is unmodifiable";

    protected internal readonly IGraph<TNode, TEdge>    Base;
    protected internal readonly IGraphType              BaseType;
    protected internal readonly ISet<TEdge>             Edges;
    protected internal readonly ISet<TNode>             Vertices;
    protected internal readonly Predicate<TNode> VertexMask;
    protected internal readonly Predicate<TEdge> EdgeMask;

    /// <summary>
    /// Creates a new induced subgraph. Running-time = O(1).
    /// </summary>
    /// <param name="base"> the base (backing) graph on which the subgraph will be based.</param>
    /// <param name="vertexMask"> vertices to exclude in the subgraph. If a vertex is masked, it is as if it
    ///        is not in the subgraph. Edges incident to the masked vertex are also masked.</param>
    /// <param name="edgeMask"> edges to exclude in the subgraph. If an edge is masked, it is as if it is not
    ///        in the subgraph.</param>
    public MaskSubgraph(
        IGraph<TNode, TEdge>    @base,
        Predicate<TNode> vertexMask,
        Predicate<TEdge> edgeMask
    )
        : base()
    {
        Base       = Objects.requireNonNull(@base, "Invalid graph provided");
        BaseType   = @base.Type;
        VertexMask = Objects.requireNonNull(vertexMask, "Invalid vertex mask provided");
        EdgeMask   = Objects.requireNonNull(edgeMask,   "Invalid edge mask provided");
        Vertices   = new MaskVertexSet<TNode>(@base.VertexSet(), vertexMask);
        Edges      = new MaskEdgeSet<TEdge>(@base, @base.EdgeSet(), vertexMask, edgeMask);
    }

    /// <inheritdoc/>
    public override TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override TNode AddVertex()
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool AddVertex(TNode node)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool ContainsEdge(TEdge edge)
    {
        return EdgeSet().Contains(edge);
    }

    /// <inheritdoc/>
    public override bool ContainsVertex(TNode node)
    {
        return VertexSet().Contains(node);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgeSet()
    {
        return Edges;
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgesOf(TNode vertex)
    {
        AssertVertexExist(vertex);

        return new MaskEdgeSet<TEdge>(Base, Base.EdgesOf(vertex), VertexMask, EdgeMask);
    }

    /// <summary>
    /// {@inheritDoc}
    ///
    /// <para>
    /// By default this method returns the sum of in-degree and out-degree. The exact value returned
    /// depends on the type of the underlying graph.
    /// </para>
    /// </summary>
    public override int DegreeOf(TNode vertex)
    {
        if (BaseType.Directed)
        {
            return InDegreeOf(vertex) + OutDegreeOf(vertex);
        }
        else
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
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TNode vertex)
    {
        AssertVertexExist(vertex);

        return new MaskEdgeSet<TEdge>(Base, Base.IncomingEdgesOf(vertex), VertexMask, EdgeMask);
    }

    /// <inheritdoc/>
    public override int InDegreeOf(TNode vertex)
    {
        if (BaseType.Undirected)
        {
            return DegreeOf(vertex);
        }
        else
        {
            return IncomingEdgesOf(vertex).Count;
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> OutgoingEdgesOf(TNode vertex)
    {
        AssertVertexExist(vertex);

        return new MaskEdgeSet<TEdge>(Base, Base.OutgoingEdgesOf(vertex), VertexMask, EdgeMask);
    }

    /// <inheritdoc/>
    public override int OutDegreeOf(TNode vertex)
    {
        if (BaseType.Undirected)
        {
            return DegreeOf(vertex);
        }
        else
        {
            return OutgoingEdgesOf(vertex).Count;
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        if (ContainsVertex(sourceVertex) && ContainsVertex(targetVertex))
        {
            return new MaskEdgeSet<TEdge>(Base, Base.GetAllEdges(sourceVertex, targetVertex), VertexMask, EdgeMask);
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override TEdge GetEdge(TNode sourceVertex, TNode targetVertex)
    {
        ISet<TEdge> edges = GetAllEdges(sourceVertex, targetVertex);

        if (edges == null)
        {
            return default(TEdge);
        }
        else
        {
            return edges.First().orElse(null);
        }
    }

    /// <inheritdoc/>
    public override Func<TNode> VertexSupplier
    {
        get
        {
            return Base.VertexSupplier;
        }
    }

    /// <inheritdoc/>
    public override Func<TEdge> EdgeSupplier
    {
        get
        {
            return Base.EdgeSupplier;
        }
    }

    /// <inheritdoc/>
    public override TNode GetEdgeSource(TEdge edge)
    {
        Debug.Assert((EdgeSet().Contains(edge)));

        return Base.GetEdgeSource(edge);
    }

    /// <inheritdoc/>
    public override TNode GetEdgeTarget(TEdge edge)
    {
        Debug.Assert((EdgeSet().Contains(edge)));

        return Base.GetEdgeTarget(edge);
    }

    /// <inheritdoc/>
    public override IGraphType Type
    {
        get
        {
            return BaseType.AsUnmodifiable();
        }
    }

    /// <inheritdoc/>
    public override double GetEdgeWeight(TEdge edge)
    {
        Debug.Assert((EdgeSet().Contains(edge)));

        return Base.GetEdgeWeight(edge);
    }

    /// <inheritdoc/>
    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        Debug.Assert((EdgeSet().Contains(edge)));

        Base.SetEdgeWeight(edge, weight);
    }

    /// <inheritdoc/>
    public virtual bool RemoveAllEdges<T1>(ICollection<T1> edges) where T1 : TEdge
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> RemoveAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public virtual bool RemoveAllVertices<T1>(ICollection<T1> vertices) where T1 : TNode
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool RemoveEdge(TEdge edge)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override TEdge RemoveEdge(TNode sourceVertex, TNode targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override bool RemoveVertex(TNode node)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    /// <inheritdoc/>
    public override ISet<TNode> VertexSet()
    {
        return Vertices;
    }
}
