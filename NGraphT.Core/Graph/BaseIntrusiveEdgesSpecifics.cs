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

namespace NGraphT.Core.Graph;

/// <summary>
/// A base implementation for the intrusive edges specifics.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
/// <typeparam name="TIntrusiveEdge"> the intrusive edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
/// <remarks>Author: Dimitrios Michail.</remarks>
public abstract class BaseIntrusiveEdgesSpecifics<TVertex, TEdge, TIntrusiveEdge>
    where TVertex : class
    where TEdge : class
    where TIntrusiveEdge : IntrusiveEdge
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="edgeMap"> the map to use for storage.</param>
    protected BaseIntrusiveEdgesSpecifics(IDictionary<TEdge, TIntrusiveEdge> edgeMap)
    {
        ArgumentNullException.ThrowIfNull(edgeMap);

        EdgeMap             = edgeMap;
        UnmodifiableEdgeSet = new Lazy<ISet<TEdge>>(() => EdgeSet.AsReadOnly());
    }

    /// <summary>
    /// Get the edge set.
    /// </summary>
    /// <returns>an unmodifiable edge set.</returns>
    public virtual ISet<TEdge> EdgeSet => UnmodifiableEdgeSet.Value;

    protected IDictionary<TEdge, TIntrusiveEdge> EdgeMap { get; }

    protected Lazy<ISet<TEdge>> UnmodifiableEdgeSet { get; }

    /// <summary>
    /// Check if an edge exists.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <returns>true if the edge exists, false otherwise.</returns>
    public virtual bool ContainsEdge(TEdge edge)
    {
        return EdgeMap.ContainsKey(edge);
    }

    /// <summary>
    /// Remove an edge.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    public virtual void Remove(TEdge edge)
    {
        EdgeMap.Remove(edge);
    }

    /// <summary>
    /// Get the source of an edge.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <returns>the source vertex of an edge.</returns>
    public virtual TVertex GetEdgeSource(TEdge edge)
    {
        var ie = GetIntrusiveEdge(edge);
        if (ie == null)
        {
            throw new ArgumentException($"no such edge in graph: {edge}", nameof(edge));
        }

        return (TVertex)ie.Source!;
    }

    /// <summary>
    /// Get the target of an edge.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <returns>the target vertex of an edge.</returns>
    public virtual TVertex GetEdgeTarget(TEdge edge)
    {
        var ie = GetIntrusiveEdge(edge);
        if (ie == null)
        {
            throw new ArgumentException($"no such edge in graph: {edge}", nameof(edge));
        }

        return (TVertex)ie.Target!;
    }

    /// <summary>
    /// Get the weight of an edge.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <returns>the weight of an edge.</returns>
    public virtual double GetEdgeWeight(TEdge edge)
    {
        return IGraph<TVertex, TEdge>.DefaultEdgeWeight;
    }

    /// <summary>
    /// Set the weight of an edge.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <param name="weight"> the new weight.</param>
    public virtual void SetEdgeWeight(TEdge edge, double weight)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Add a new edge.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <param name="sourceVertex"> the source vertex of the edge.</param>
    /// <param name="targetVertex"> the target vertex of the edge.</param>
    /// <returns>true if the edge was added, false if the edge was already present.</returns>
    public abstract bool Add(TEdge edge, TVertex sourceVertex, TVertex targetVertex);

    protected virtual bool AddIntrusiveEdge(TEdge e, TVertex sourceVertex, TVertex targetVertex, TIntrusiveEdge iEdge)
    {
        ArgumentNullException.ThrowIfNull(iEdge);

        if (iEdge.Source == null && iEdge.Target == null)
        {
            // edge not yet in any graph
            iEdge.Source = sourceVertex;
            iEdge.Target = targetVertex;
        }
        else if (iEdge.Source != sourceVertex || iEdge.Target != targetVertex)
        {
            // Edge already contained in this or another graph but with different touching
            // edges. Reject the edge to not reset the touching vertices of the edge.
            // Changing the touching vertices causes major inconsistent behavior.
            throw new IntrusiveEdgeException(iEdge.Source, iEdge.Target);
        }

        if (EdgeMap.ContainsKey(e))
        {
            return false;
        }
        else
        {
            EdgeMap[e] = iEdge;
            return true;
        }
    }

    /// <summary>
    /// Get the intrusive edge of an edge.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <returns>the intrusive edge.</returns>
    protected abstract TIntrusiveEdge GetIntrusiveEdge(TEdge edge);
}
