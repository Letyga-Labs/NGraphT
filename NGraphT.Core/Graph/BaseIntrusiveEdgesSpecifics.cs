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
using Util;

/// <summary>
/// A base implementation for the intrusive edges specifics.
/// </summary>
///
/// <remarks>Author: Barak Naveh.</remarks>
/// <remarks>Author: Dimitrios Michail.</remarks>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
/// <typeparam name="IE"> the intrusive edge type.</typeparam>
public abstract class BaseIntrusiveEdgesSpecifics<TNode, TEdge, TIe> where TIe : IntrusiveEdge
{
    protected internal IDictionary<TEdge, TIe> EdgeMap;

    protected internal ISet<TEdge> UnmodifiableEdgeSet = null;

    ///<summary>
    ///Constructor.
    ///</summary>
    ///<param name="edgeMap"> the map to use for storage.</param>
    public BaseIntrusiveEdgesSpecifics(IDictionary<TEdge, TIe> edgeMap)
    {
        EdgeMap = Objects.requireNonNull(edgeMap);
    }

    ///<summary>
    ///Check if an edge exists.
    ///</summary>
    ///<param name="edge"> the edge.</param>
    ///<returns>true if the edge exists, false otherwise.</returns>
    public virtual bool ContainsEdge(TEdge edge)
    {
        return EdgeMap.ContainsKey(edge);
    }

    ///<summary>
    ///Get the edge set.
    ///</summary>
    ///<returns>an unmodifiable edge set.</returns>
    public virtual ISet<TEdge> EdgeSet
    {
        get
        {
            if (UnmodifiableEdgeSet == null)
            {
                UnmodifiableEdgeSet = Collections.unmodifiableSet(EdgeMap.Keys);
            }

            return UnmodifiableEdgeSet;
        }
    }

    ///<summary>
    ///Remove an edge.
    ///</summary>
    ///<param name="edge"> the edge.</param>
    public virtual void Remove(TEdge edge)
    {
        EdgeMap.Remove(edge);
    }

    ///<summary>
    ///Get the source of an edge.
    ///</summary>
    ///<param name="edge"> the edge.</param>
    ///<returns>the source vertex of an edge.</returns>
    public virtual TNode GetEdgeSource(TEdge edge)
    {
        IntrusiveEdge ie = GetIntrusiveEdge(edge);
        if (ie == null)
        {
            throw new ArgumentException("no such edge in graph: " + edge.ToString());
        }

        return TypeUtil.UncheckedCast(ie.Source);
    }

    ///<summary>
    ///Get the target of an edge.
    ///</summary>
    ///<param name="edge"> the edge.</param>
    ///<returns>the target vertex of an edge.</returns>
    public virtual TNode GetEdgeTarget(TEdge edge)
    {
        IntrusiveEdge ie = GetIntrusiveEdge(edge);
        if (ie == null)
        {
            throw new ArgumentException("no such edge in graph: " + edge.ToString());
        }

        return TypeUtil.UncheckedCast(ie.Target);
    }

    ///<summary>
    ///Get the weight of an edge.
    ///</summary>
    ///<param name="edge"> the edge.</param>
    ///<returns>the weight of an edge.</returns>
    public virtual double GetEdgeWeight(TEdge edge)
    {
        return Graph.DEFAULT_EDGE_WEIGHT;
    }

    ///<summary>
    ///Set the weight of an edge.
    ///</summary>
    ///<param name="edge"> the edge.</param>
    ///<param name="weight"> the new weight.</param>
    public virtual void SetEdgeWeight(TEdge edge, double weight)
    {
        throw new NotSupportedException();
    }

    ///<summary>
    ///Add a new edge.
    ///</summary>
    ///<param name="edge"> the edge.</param>
    ///<param name="sourceVertex"> the source vertex of the edge.</param>
    ///<param name="targetVertex"> the target vertex of the edge.</param>
    ///<returns>true if the edge was added, false if the edge was already present.</returns>
    public abstract bool Add(TEdge edge, TNode sourceVertex, TNode targetVertex);

    protected internal virtual bool AddIntrusiveEdge(TEdge edge, TNode sourceVertex, TNode targetVertex, TIe edge)
    {
        if (edge.Source == null && edge.Target == null)
        {
            // edge not yet in any graph
            edge.Source = sourceVertex;
            edge.Target = targetVertex;
        }
        else if (edge.Source != sourceVertex || edge.Target != targetVertex)
        {
            // Edge already contained in this or another graph but with different touching
            // edges. Reject the edge to not reset the touching vertices of the edge.
            // Changing the touching vertices causes major inconsistent behavior.
            throw new IntrusiveEdgeException(edge.Source, edge.Target);
        }

        return EdgeMap.putIfAbsent(edge, edge) == null;
    }

    ///<summary>
    ///Get the intrusive edge of an edge.
    ///</summary>
    ///<param name="edge"> the edge.</param>
    ///<returns>the intrusive edge.</returns>
    protected internal abstract TIe GetIntrusiveEdge(TEdge edge);
}
