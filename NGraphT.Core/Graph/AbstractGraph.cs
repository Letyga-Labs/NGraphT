using System.Diagnostics.CodeAnalysis;
using System.Text;

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

/// <summary>
/// A skeletal implementation of the <c>Graph</c> interface, to minimize the effort required to
/// implement graph interfaces. This implementation is applicable to both: directed graphs and
/// undirected graphs.
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <seealso cref="Graph"/>
///
/// <remarks>Author: Barak Naveh.</remarks>
public abstract class AbstractGraph<TNode, TEdge> : IGraph<TNode, TEdge>
    where TNode : class
    where TEdge : class
{
    /// <summary>
    /// Construct a new empty graph object.
    /// </summary>
    protected AbstractGraph()
    {
    }

    public abstract IGraphType Type { get; }

    public abstract Func<TEdge> EdgeSupplier   { get; }
    public abstract Func<TNode> VertexSupplier { get; }

    public abstract ISet<TNode> VertexSet();
    public abstract ISet<TEdge> EdgeSet();

    public abstract int DegreeOf(TNode    vertex);
    public abstract int InDegreeOf(TNode  vertex);
    public abstract int OutDegreeOf(TNode vertex);

    public abstract ISet<TEdge> EdgesOf(TNode         vertex);
    public abstract ISet<TEdge> IncomingEdgesOf(TNode vertex);
    public abstract ISet<TEdge> OutgoingEdgesOf(TNode vertex);

    public abstract TNode GetEdgeTarget(TEdge edge);
    public abstract TNode GetEdgeSource(TEdge edge);

    public abstract TEdge?      GetEdge(TNode?     sourceVertex, TNode? targetVertex);
    public abstract ISet<TEdge> GetAllEdges(TNode? sourceVertex, TNode? targetVertex);

    public abstract TNode  AddVertex();
    public abstract bool   AddVertex(TNode node);
    public abstract bool   AddEdge(TNode   sourceVertex, TNode targetVertex, TEdge edge);
    public abstract TEdge? AddEdge(TNode   sourceVertex, TNode targetVertex);

    public abstract bool   RemoveVertex(TNode? node);
    public abstract bool   RemoveEdge(TEdge?   edge);
    public abstract TEdge? RemoveEdge(TNode    sourceVertex, TNode targetVertex);

    public abstract bool ContainsVertex(TNode? node);
    public abstract bool ContainsEdge(TEdge?   edge);

    public abstract double GetEdgeWeight(TEdge edge);
    public abstract void   SetEdgeWeight(TNode sourceVertex, TNode  targetVertex, double weight);
    public abstract void   SetEdgeWeight(TEdge edge,         double weight);

    public abstract IGraphIterables<TNode, TEdge> Iterables();

    /// <inheritdoc/>
    public virtual bool ContainsEdge(TNode? sourceVertex, TNode? targetVertex)
    {
        return GetEdge(sourceVertex, targetVertex) != null;
    }

    /// <inheritdoc/>
    public bool RemoveAllEdges(IEnumerable<TEdge> edges)
    {
        ArgumentNullException.ThrowIfNull(edges);

        var modified = false;
        foreach (var edge in edges)
        {
            modified |= RemoveEdge(edge);
        }

        return modified;
    }

    /// <inheritdoc/>
    public virtual ISet<TEdge> RemoveAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        var toRemove = GetAllEdges(sourceVertex, targetVertex);
        RemoveAllEdges(toRemove);
        return toRemove;
    }

    /// <inheritdoc/>
    public bool RemoveAllVertices(IEnumerable<TNode> vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        var modified = false;
        foreach (var node in vertices)
        {
            modified |= RemoveVertex(node);
        }

        return modified;
    }

    /// <summary>
    /// Returns a hash code value for this graph. The hash code of a graph is defined to be the sum
    /// of the hash codes of vertices and edges in the graph. It is also based on graph topology and
    /// edges weights.
    /// </summary>
    ///
    /// <returns>the hash code value this graph.</returns>
    ///
    /// <seealso cref="object.GetHashCode()"/>
    public override int GetHashCode()
    {
        var hash       = VertexSet().GetHashCode();
        var isDirected = Type.Directed;
        foreach (var edge in EdgeSet())
        {
            var part = edge.GetHashCode();

            var source = GetEdgeSource(edge).GetHashCode();
            var target = GetEdgeTarget(edge).GetHashCode();

            var pairing = source + target;
            if (isDirected)
            {
                // see http://en.wikipedia.org/wiki/Pairing_function (VK);
                pairing = (pairing * (pairing + 1) / 2) + target;
            }

            part = (31 * part) + pairing;
            part = (31 * part) + GetEdgeWeight(edge).GetHashCode();

            hash += part;
        }

        return hash;
    }

    /// <summary>
    /// Indicates whether some other object is "equal to" this graph. Returns <c>true</c> if
    /// the given object is also a graph, the two graphs are instances of the same graph class, have
    /// identical vertices and edges sets with the same weights.
    /// </summary>
    ///
    /// <param name="obj"> object to be compared for equality with this graph.</param>>
    ///
    /// <returns><c>true</c> if the specified object is equal to this graph.</returns>
    ///
    /// <seealso cref="object.Equals(object?)"/>
    public override bool Equals(object? obj)
    {
        if (this == obj)
        {
            return true;
        }

        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var g = (IGraph<TNode, TEdge>)obj;

        if (!VertexSet().SetEquals(g.VertexSet()))
        {
            return false;
        }

        if (EdgeSet().Count != g.EdgeSet().Count)
        {
            return false;
        }

        var isDirected = Type.Directed;
        foreach (var edge in EdgeSet())
        {
            var source = GetEdgeSource(edge);
            var target = GetEdgeTarget(edge);

            if (!g.ContainsEdge(edge))
            {
                return false;
            }

            var gSource = g.GetEdgeSource(edge);
            var gTarget = g.GetEdgeTarget(edge);

            if (isDirected)
            {
                if (!gSource.Equals(source) || !gTarget.Equals(target))
                {
                    return false;
                }
            }
            else
            {
                if ((!gSource.Equals(source) || !gTarget.Equals(target)) &&
                    (!gSource.Equals(target) || !gTarget.Equals(source)))
                {
                    return false;
                }
            }

            if (GetEdgeWeight(edge).CompareTo(g.GetEdgeWeight(edge)) != 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns a string of the parenthesized pair (TNode, TEdge) representing this G=(TNode,TEdge) graph.
    /// 'TNode' is the string representation of the vertex set, and 'TEdge' is the string representation
    /// of the edge set.
    /// </summary>
    /// <returns>a string representation of this graph.</returns>
    public override string ToString()
    {
        return ToStringFromSets(VertexSet(), EdgeSet(), Type.Directed);
    }

    /// <summary>
    /// Ensures that the specified vertex exists in this graph, or else throws exception.
    /// </summary>
    ///
    /// <param name="node"> vertex.</param>
    ///
    /// <returns><c>true</c> if this assertion holds.</returns>
    ///
    /// <exception cref="NullReferenceException"> if specified vertex is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"> if specified vertex does not exist in this graph.</exception>
    protected virtual bool AssertVertexExist([MaybeNullWhen(false)] TNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (ContainsVertex(node))
        {
            return true;
        }
        else
        {
            throw new ArgumentException("no such vertex in graph: " + node, nameof(node));
        }
    }

    /// <summary>
    /// Helper for subclass implementations of toString( ).
    /// </summary>
    /// <param name="vertexSet"> the vertex set TNode to be printed.</param>
    /// <param name="edgeSet"> the edge set TEdge to be printed.</param>
    /// <param name="directed"> true to use parens for each edge (representing directed); false to use curly
    ///        braces (representing undirected).</param>>
    /// <returns>a string representation of (TNode,TEdge).</returns>
    protected virtual string ToStringFromSets<T1, T2>(
        ICollection<T1> vertexSet,
        IEnumerable<T2> edgeSet,
        bool            directed
    )
        where T1 : TNode
        where T2 : TEdge
    {
        ArgumentNullException.ThrowIfNull(edgeSet);

        var sb            = new StringBuilder();
        var renderedEdges = new List<string>();
        foreach (var edge in edgeSet)
        {
            if (edge.GetType() != typeof(DefaultEdge) &&
                edge.GetType() != typeof(DefaultWeightedEdge))
            {
                sb.Append(edge);
                sb.Append('=');
            }

            sb.Append(directed ? '(' : '{');

            sb.Append(GetEdgeSource(edge));
            sb.Append(',');
            sb.Append(GetEdgeTarget(edge));
            sb.Append(directed ? ')' : '}');

            // REVIEW jvs 29-May-2006: dump weight somewhere?
            renderedEdges.Add(sb.ToString());
            sb.Clear();
        }

        return $"({vertexSet}, {renderedEdges})";
    }
}
