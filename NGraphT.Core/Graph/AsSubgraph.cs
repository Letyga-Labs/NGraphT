using System.Diagnostics;

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
/// A subgraph is a graph that has a subset of vertices and a subset of edges with respect to some
/// base graph. More formally, a subgraph G(TNode,TEdge) that is based on a base graph Gb(Vb,Eb) satisfies
/// the following <b><i>subgraph property</i></b>: TNode is a subset of Vb and TEdge is a subset of Eb. Other
/// than this property, a subgraph is a graph with any respect and fully complies with the
/// <c>Graph</c> interface.
///
/// <para>
/// If the base graph is a <seealso cref="IListenableGraph{TNode,TEdge}"/>, the subgraph listens on the base
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
/// <seealso cref="LinkedHashSet"/>).
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
/// @param <TNode> the vertex type.</param>
/// @param <TEdge> the edge type
///
/// <remarks>Author: Barak Naveh.</remarks>
/// <seealso cref="Graph"/>
/// <seealso cref="Set"/>
public class AsSubgraph<TNode, TEdge> : AbstractGraph<TNode, TEdge>
{
    private const string NoSuchEdgeInBase                    = "no such edge in base graph";
    private const string NoSuchVertexInBase                  = "no such vertex in base graph";
    private const string CannotCreateNewVerticesFromSubgraph = "Cannot create new vertices from subgraph";

    protected internal readonly ISet<TEdge>          EdgeSet   = new LinkedHashSet<TEdge>();
    protected internal readonly ISet<TNode>          VertexSet = new LinkedHashSet<TNode>();
    protected internal readonly IGraph<TNode, TEdge> Base;
    protected internal readonly IGraphType           BaseType;
    protected internal readonly bool                 IsInduced;

    private ISet<TEdge> _unmodifiableEdgeSet = null;

    private ISet<TNode> _unmodifiableVertexSet = null;

    /// <summary>
    /// Creates a new subgraph.
    /// </summary>
    /// <param name="base"> the base (backing) graph on which the subgraph will be based.</param>
    /// <param name="vertexSubset"> vertices to include in the subgraph. If <c>null</c> then all
    ///        vertices are included.</param>
    /// <param name="edgeSubset"> edges to in include in the subgraph. If <c>null</c> then all the
    ///        edges whose vertices found in the graph are included.</param>
//JAVA TO C# CONVERTER TODO TASK: Wildcard generics in method parameters are not converted:
//ORIGINAL LINE: public AsSubgraph(Graph<TNode, TEdge> super, Set<? extends TNode> vertexSubset, Set<? extends TEdge> edgeSubset)
    public AsSubgraph(IGraph<TNode, TEdge> @base, ISet<TNode> vertexSubset, ISet<TEdge> edgeSubset)
        : base()
    {
        Base      = GraphTests.RequireDirectedOrUndirected(@base);
        BaseType  = @base.Type;
        IsInduced = edgeSubset == null;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: if (super instanceof ListenableGraph<?, ?>)
        if (@base is IListenableGraph<object, object>)
        {
            ((IListenableGraph<TNode, TEdge>)@base).AddGraphListener(new BaseGraphListener(this));
        }

        Initialize(vertexSubset, edgeSubset);
    }

    /// <summary>
    /// Creates a new induced subgraph. The subgraph will keep track of edges being added to its
    /// vertex subset as well as deletion of edges and vertices. If base it not listenable, this is
    /// identical to the call Subgraph(base, vertexSubset, null).
    /// </summary>
    /// <param name="base"> the base (backing) graph on which the subgraph will be based.</param>
    /// <param name="vertexSubset"> vertices to include in the subgraph. If <c>null</c> then all
    ///        vertices are included.</param>
//JAVA TO C# CONVERTER TODO TASK: Wildcard generics in method parameters are not converted:
//ORIGINAL LINE: public AsSubgraph(Graph<TNode, TEdge> super, Set<? extends TNode> vertexSubset)
    public AsSubgraph(IGraph<TNode, TEdge> @base, ISet<TNode> vertexSubset)
        : this(@base, vertexSubset, null)
    {
    }

    /// <summary>
    /// Creates a new induced Subgraph with all vertices included. The subgraph will keep track of
    /// edges being added to its vertex subset as well as deletion of edges and vertices. If base is
    /// not listenable, this is identical to the call Subgraph(base, null, null).
    /// </summary>
    /// <param name="base"> the base (backing) graph on which the subgraph will be based.</param>
    public AsSubgraph(IGraph<TNode, TEdge> @base)
        : this(@base, null, null)
    {
    }

    /// <inheritdoc/>
    public override ISet<TEdge> GetAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        if (ContainsVertex(sourceVertex) && ContainsVertex(targetVertex))
        {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
            return Base.GetAllEdges(sourceVertex, targetVertex).stream().filter(edgeSet.contains)
                .collect(Collectors.toCollection(LinkedHashSet::new));
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

    /// <summary>
    /// Add an edge to the subgraph. The end-points must exist in the subgraph and the edge must
    /// exist in the base graph. In case multiple such edges exist in the base graph, one that is not
    /// already in the subgraph is chosen arbitrarily and added to the subgraph. In case all such
    /// edges already exist in the subgraph, the method returns null.
    /// </summary>
    /// <param name="sourceVertex"> the source vertex.</param>
    /// <param name="targetVertex"> the source vertex.</param>
    /// <returns>the added edge or null if all such edges from the base graph already belong in the
    ///         subgraph.</returns>
    /// <exception cref="ArgumentException"> if the source or target vertex does not belong to the
    ///         subgraph.</exception>
    /// <exception cref="ArgumentException"> if the base graph does not contain any edge between the two
    ///         end-points.</exception>
    public override TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
    {
        AssertVertexExist(sourceVertex);
        AssertVertexExist(targetVertex);

        if (!Base.ContainsEdge(sourceVertex, targetVertex))
        {
            throw new ArgumentException(NoSuchEdgeInBase);
        }

        ISet<TEdge> edges = Base.GetAllEdges(sourceVertex, targetVertex);

        foreach (var edge in edges)
        {
            if (!ContainsEdge(edge))
            {
                edgeSet.Add(edge);
                return edge;
            }
        }

        return default(TEdge);
    }

    /// <inheritdoc/>
    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        if (edge == null)
        {
            throw new NullReferenceException();
        }

        if (!Base.ContainsEdge(edge))
        {
            throw new ArgumentException(NoSuchEdgeInBase);
        }

        AssertVertexExist(sourceVertex);
        AssertVertexExist(targetVertex);

        Debug.Assert((Base.GetEdgeSource(edge) == sourceVertex));
        Debug.Assert((Base.GetEdgeTarget(edge) == targetVertex));

        return edgeSet.Add(edge);
    }

    public override TNode AddVertex()
    {
        throw new NotSupportedException(CannotCreateNewVerticesFromSubgraph);
    }

    /// <summary>
    /// Adds the specified vertex to this subgraph.
    /// </summary>
    /// <param name="node"> the vertex to be added.</param>
    /// <returns><c>true</c> if the vertex was added, otherwise <c>
    /// false</c>.</returns>
    /// <exception cref="NullReferenceException"> if TNode is null.</exception>
    /// <exception cref="ArgumentException"> if the base graph does not contain the vertex
    /// </exception>
    /// <seealso cref="AsSubgraph"/>
    /// <seealso cref="Graph.addVertex(Object)"/>
    public override bool AddVertex(TNode node)
    {
        if (node == null)
        {
            throw new NullReferenceException();
        }

        if (!Base.ContainsVertex(node))
        {
            throw new ArgumentException(NoSuchVertexInBase);
        }

        return vertexSet.Add(node);
    }

    /// <inheritdoc/>
    public override bool ContainsEdge(TEdge edge)
    {
        return edgeSet.Contains(edge);
    }

    /// <inheritdoc/>
    public override bool ContainsVertex(TNode node)
    {
        return vertexSet.Contains(node);
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgeSet()
    {
        if (_unmodifiableEdgeSet == null)
        {
            _unmodifiableEdgeSet = Collections.unmodifiableSet(edgeSet);
        }

        return _unmodifiableEdgeSet;
    }

    /// <inheritdoc/>
    public override ISet<TEdge> EdgesOf(TNode vertex)
    {
        AssertVertexExist(vertex);

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
        return Base.EdgesOf(vertex).stream().filter(edgeSet.contains)
            .collect(Collectors.toCollection(LinkedHashSet::new));
    }

    /// <summary>
    /// {@inheritDoc}
    ///
    /// <para>
    /// By default this method returns the sum of in-degree and out-degree. The exact value returned
    /// depends on the types of the underlying graph.
    /// </para>
    /// </summary>
    public override int DegreeOf(TNode vertex)
    {
        AssertVertexExist(vertex);

        if (BaseType.Undirected)
        {
            var                degree = 0;
            IEnumerator<TEdge> it     = Base.EdgesOf(vertex).Where(edgeSet.contains).GetEnumerator();
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
            return InDegreeOf(vertex) + OutDegreeOf(vertex);
        }
    }

    /// <inheritdoc/>
    public override ISet<TEdge> IncomingEdgesOf(TNode vertex)
    {
        AssertVertexExist(vertex);

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
        return Base.IncomingEdgesOf(vertex).stream().filter(edgeSet.contains)
            .collect(Collectors.toCollection(LinkedHashSet::new));
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

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
        return Base.OutgoingEdgesOf(vertex).stream().filter(edgeSet.contains)
            .collect(Collectors.toCollection(LinkedHashSet::new));
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
    public override bool RemoveEdge(TEdge edge)
    {
        return edgeSet.remove(edge);
    }

    /// <inheritdoc/>
    public override TEdge RemoveEdge(TNode sourceVertex, TNode targetVertex)
    {
        var edge = GetEdge(sourceVertex, targetVertex);

        return edgeSet.remove(edge) ? edge : default(TEdge);
    }

    /// <inheritdoc/>
    public override bool RemoveVertex(TNode node)
    {
        // If the base graph does NOT contain TNode it means we are here in
        // response to removal of TNode from the base. In such case we don't need
        // to remove all the edges of TNode as they were already removed.
        if (ContainsVertex(node) && Base.ContainsVertex(node))
        {
            removeAllEdges(EdgesOf(node));
        }

        return vertexSet.remove(node);
    }

    /// <inheritdoc/>
    public override ISet<TNode> VertexSet()
    {
        if (_unmodifiableVertexSet == null)
        {
            _unmodifiableVertexSet = Collections.unmodifiableSet(vertexSet);
        }

        return _unmodifiableVertexSet;
    }

    /// <inheritdoc/>
    public override TNode GetEdgeSource(TEdge edge)
    {
        return Base.GetEdgeSource(edge);
    }

    /// <inheritdoc/>
    public override TNode GetEdgeTarget(TEdge edge)
    {
        return Base.GetEdgeTarget(edge);
    }

    /// <inheritdoc/>
    public override IGraphType Type
    {
        get
        {
            return Base.Type;
        }
    }

    /// <inheritdoc/>
    public override double GetEdgeWeight(TEdge edge)
    {
        return Base.GetEdgeWeight(edge);
    }

    /// <inheritdoc/>
    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        Base.SetEdgeWeight(edge, weight);
    }

    private void Initialize<T1, T2>(ISet<T1> vertexFilter, ISet<T2> edgeFilter) where T1 : TNode where T2 : TEdge
    {
        if (vertexFilter == null && edgeFilter == null)
        {
            vertexSet.addAll(Base.VertexSet());
            edgeSet.addAll(Base.EdgeSet());
            return;
        }

        // add vertices
        if (vertexFilter == null)
        {
            vertexSet.addAll(Base.VertexSet());
        }
        else
        {
            if (vertexFilter.Count > Base.VertexSet().Count)
            {
                Base.VertexSet().Where(vertexFilter.contains).ForEach(vertexSet.add);
            }
            else
            {
                vertexFilter.Where(node => node != null && Base.ContainsVertex(node)).ForEach(vertexSet.add);
            }
        }

        // add edges
        if (edgeFilter == null)
        {
            Base.EdgeSet()
                .Where(edge =>
                    vertexSet.Contains(Base.GetEdgeSource(edge)) && vertexSet.Contains(Base.GetEdgeTarget(edge))
                )
                .ForEach(edgeSet.add);
        }
        else
        {
            if (edgeFilter.Count > Base.EdgeSet().Count)
            {
                Base.EdgeSet()
                    .Where(edge => edgeFilter.Contains(edge) && vertexSet.Contains(Base.GetEdgeSource(edge)) &&
                                   vertexSet.Contains(Base.GetEdgeTarget(edge))
                    ).ForEach(edgeSet.add);
            }
            else
            {
                edgeFilter.Where(edge =>
                    edge != null && Base.ContainsEdge(edge) && vertexSet.Contains(Base.GetEdgeSource(edge)) &&
                    vertexSet.Contains(Base.GetEdgeTarget(edge))
                ).ForEach(edgeSet.add);
            }
        }
    }

    /// <summary>
    /// An internal listener on the base graph.
    /// 
    /// <remarks>Author: Barak Naveh.</remarks>
    /// </summary>
        private class BaseGraphListener : GraphListener<TNode, TEdge>
    {
        private readonly AsSubgraph<TNode, TEdge> _outerInstance;

        public BaseGraphListener(AsSubgraph<TNode, TEdge> outerInstance)
        {
            _outerInstance = outerInstance;
        }

        internal const long SerialVersionUID = 4343535244243546391L;

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual void EdgeAdded(GraphEdgeChangeEvent<TNode, TEdge> edge)
        {
            if (outerInstance.isInduced)
            {
                TEdge edge   = TEdge.Edge;
                TNode source = TEdge.EdgeSource;
                TNode target = TEdge.EdgeTarget;
                if (outerInstance.ContainsVertex(source) && outerInstance.ContainsVertex(target))
                {
                    outerInstance.AddEdge(source, target, edge);
                }
            }
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual void EdgeRemoved(GraphEdgeChangeEvent<TNode, TEdge> edge)
        {
            TEdge edge = TEdge.Edge;

            outerInstance.RemoveEdge(edge);
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual void VertexAdded(GraphVertexChangeEvent<TNode> edge)
        {
            // we don't care
        }

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        public virtual void VertexRemoved(GraphVertexChangeEvent<TNode> edge)
        {
            TNode vertex = edge.Vertex;

            outerInstance.RemoveVertex(vertex);
        }
    }
}
