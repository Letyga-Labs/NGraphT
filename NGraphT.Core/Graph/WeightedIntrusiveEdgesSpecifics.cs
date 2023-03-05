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
/// A weighted variant of the intrusive edges specifics.
///
/// <para>
/// The implementation optimizes the use of <see cref="DefaultWeightedEdge"/> and subclasses. For other
/// custom user edge types, a map is used to store vertex source, target and weight.
///
/// <remarks>Author: Barak Naveh.</remarks>
/// <remarks>Author: Dimitrios Michail.</remarks>
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
public class WeightedIntrusiveEdgesSpecifics<TNode, TEdge> :
    BaseIntrusiveEdgesSpecifics<TNode, TEdge, IntrusiveWeightedEdge>,
    INtrusiveEdgesSpecifics<TNode, TEdge>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="map"> the map to use for storage.</param>
    public WeightedIntrusiveEdgesSpecifics(IDictionary<TEdge, IntrusiveWeightedEdge> map)
        : base(map)
    {
    }

    public override bool Add(TEdge edge, TNode sourceVertex, TNode targetVertex)
    {
        if (edge is IntrusiveWeightedEdge)
        {
            return AddIntrusiveEdge(edge, sourceVertex, targetVertex, (IntrusiveWeightedEdge)edge);
        }
        else
        {
            var                   previousSize  = EdgeMap.Count;
            IntrusiveWeightedEdge intrusiveEdge = EdgeMap.computeIfAbsent(edge, i => new IntrusiveWeightedEdge());
            if (previousSize < EdgeMap.Count)
            {
                // edge was added
                intrusiveEdge.Source = sourceVertex;
                intrusiveEdge.Target = targetVertex;
                return true;
            }

            return false;
        }
    }

    public override double GetEdgeWeight(TEdge edge)
    {
        var ie = GetIntrusiveEdge(edge);
        if (ie == null)
        {
            throw new ArgumentException("no such edge in graph: " + edge.ToString());
        }

        return ie.Weight;
    }

    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        var ie = GetIntrusiveEdge(edge);
        if (ie == null)
        {
            throw new ArgumentException("no such edge in graph: " + edge.ToString());
        }

        ie.Weight = weight;
    }

    protected internal override IntrusiveWeightedEdge GetIntrusiveEdge(TEdge edge)
    {
        if (edge is IntrusiveWeightedEdge)
        {
            return (IntrusiveWeightedEdge)edge;
        }

        return EdgeMap[edge];
    }
}
