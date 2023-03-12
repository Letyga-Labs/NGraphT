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
/// An uniform weights variant of the intrusive edges specifics.
///
/// <para>
/// The implementation optimizes the use of <see cref="DefaultEdge"/> and subclasses. For other custom user
/// edge types, a map is used to store vertex source and target.
///
/// <remarks>Author: Barak Naveh.</remarks>
/// <remarks>Author: Dimitrios Michail.</remarks>
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
public class UniformIntrusiveEdgesSpecifics<TNode, TEdge> : BaseIntrusiveEdgesSpecifics<TNode, TEdge, IntrusiveEdge>,
    IIntrusiveEdgesSpecifics<TNode, TEdge>
{
    ///<summary>
    ///Constructor.
    ///</summary>
    ///<param name="map"> the map to use for storage.</param>
    public UniformIntrusiveEdgesSpecifics(IDictionary<TEdge, IntrusiveEdge> map)
        : base(map)
    {
    }

    public override bool Add(TEdge edge, TNode sourceVertex, TNode targetVertex)
    {
        if (edge is IntrusiveEdge)
        {
            return AddIntrusiveEdge(edge, sourceVertex, targetVertex, (IntrusiveEdge)edge);
        }
        else
        {
            var           previousSize  = EdgeMap.Count;
            IntrusiveEdge intrusiveEdge = EdgeMap.computeIfAbsent(edge, i => new IntrusiveEdge());
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

    protected override IntrusiveEdge GetIntrusiveEdge(TEdge edge)
    {
        if (edge is IntrusiveEdge)
        {
            return (IntrusiveEdge)edge;
        }

        return EdgeMap[edge];
    }
}
