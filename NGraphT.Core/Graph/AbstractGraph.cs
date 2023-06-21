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
using System.Text;

namespace NGraphT.Core.Graph;

/// <summary>
/// A skeletal implementation of the <c>Graph</c> interface, to minimize the effort required to
/// implement graph interfaces. This implementation is applicable to both: directed graphs and
/// undirected graphs.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <seealso cref="Graph"/>
///
/// <remarks>Author: Barak Naveh.</remarks>
public abstract class AbstractGraph<TVertex, TEdge> : IGraph<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Construct a new empty graph object.
    /// </summary>
    protected AbstractGraph()
    {
    }

    public abstract IGraphType Type { get; }

    public abstract Func<TEdge>?   EdgeSupplier   { get; }
    public abstract Func<TVertex>? VertexSupplier { get; }

    public abstract ISet<TVertex> VertexSet();
    public abstract ISet<TEdge>   EdgeSet();

    public abstract int DegreeOf(TVertex    vertex);
    public abstract int InDegreeOf(TVertex  vertex);
    public abstract int OutDegreeOf(TVertex vertex);

    public abstract ISet<TEdge> EdgesOf(TVertex         vertex);
    public abstract ISet<TEdge> IncomingEdgesOf(TVertex vertex);
    public abstract ISet<TEdge> OutgoingEdgesOf(TVertex vertex);

    public abstract TVertex GetEdgeTarget(TEdge edge);
    public abstract TVertex GetEdgeSource(TEdge edge);

    public abstract TEdge?      GetEdge(TVertex?     sourceVertex, TVertex? targetVertex);
    public abstract ISet<TEdge> GetAllEdges(TVertex? sourceVertex, TVertex? targetVertex);

    public abstract TVertex AddVertex();
    public abstract bool    AddVertex(TVertex vertex);
    public abstract bool    AddEdge(TVertex   sourceVertex, TVertex targetVertex, TEdge edge);
    public abstract TEdge?  AddEdge(TVertex   sourceVertex, TVertex targetVertex);

    public abstract bool   RemoveVertex(TVertex? vertex);
    public abstract bool   RemoveEdge(TEdge?     edge);
    public abstract TEdge? RemoveEdge(TVertex    sourceVertex, TVertex targetVertex);

    public abstract bool ContainsVertex(TVertex? vertex);
    public abstract bool ContainsEdge(TEdge?     edge);

    public abstract double GetEdgeWeight(TEdge edge);
    public abstract void   SetEdgeWeight(TEdge edge, double weight);

    /// <inheritdoc/>
    public virtual bool ContainsEdge(TVertex? sourceVertex, TVertex? targetVertex)
    {
        return GetEdge(sourceVertex, targetVertex) != null;
    }

    /// <inheritdoc/>
    public virtual bool RemoveAllEdges(IEnumerable<TEdge> edges)
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
    public virtual ISet<TEdge> RemoveAllEdges(TVertex sourceVertex, TVertex targetVertex)
    {
        var toRemove = GetAllEdges(sourceVertex, targetVertex);
        RemoveAllEdges(toRemove);
        return toRemove;
    }

    /// <inheritdoc/>
    public virtual bool RemoveAllVertices(IEnumerable<TVertex> vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        var modified = false;
        foreach (var v in vertices)
        {
            modified |= RemoveVertex(v);
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
        var isDirected = Type.IsDirected;
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

        var g = (IGraph<TVertex, TEdge>)obj;

        if (!VertexSet().SetEquals(g.VertexSet()))
        {
            return false;
        }

        if (EdgeSet().Count != g.EdgeSet().Count)
        {
            return false;
        }

        var isDirected = Type.IsDirected;
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
    /// Returns a string of the parenthesized pair (TVertex, TEdge) representing this G=(TVertex,TEdge) graph.
    /// 'TVertex' is the string representation of the vertex set, and 'TEdge' is the string representation
    /// of the edge set.
    /// </summary>
    /// <returns>a string representation of this graph.</returns>
    public override string ToString()
    {
        return ToStringFromSets(VertexSet(), EdgeSet(), Type.IsDirected);
    }

    /// <summary>
    /// Ensures that the specified vertex exists in this graph, or else throws exception.
    /// </summary>
    ///
    /// <param name="v"> vertex.</param>
    ///
    /// <returns><c>true</c> if this assertion holds.</returns>
    ///
    /// <exception cref="NullReferenceException"> if specified vertex is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"> if specified vertex does not exist in this graph.</exception>
    protected virtual bool AssertVertexExist([NotNull] TVertex v)
    {
        ArgumentNullException.ThrowIfNull(v);

        if (ContainsVertex(v))
        {
            return true;
        }
        else
        {
            throw new ArgumentException($"no such vertex in graph: {v}", nameof(v));
        }
    }

    /// <summary>
    /// Helper for subclass implementations of toString( ).
    /// </summary>
    /// <param name="vertexSet"> the vertex set TVertex to be printed.</param>
    /// <param name="edgeSet"> the edge set TEdge to be printed.</param>
    /// <param name="directed"> true to use parens for each edge (representing directed); false to use curly
    ///        braces (representing undirected).</param>>
    /// <returns>a string representation of (TVertex,TEdge).</returns>
    protected virtual string ToStringFromSets<T1, T2>(
        ICollection<T1> vertexSet,
        IEnumerable<T2> edgeSet,
        bool            directed
    )
        where T1 : TVertex
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
