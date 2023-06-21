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

using NGraphT.Core.DotNetUtil;

namespace NGraphT.Core.Graph;

/// <summary>
/// An uniform weights variant of the intrusive edges specifics.
///
/// <para>
/// The implementation optimizes the use of <see cref="DefaultEdge"/> and subclasses. For other custom user
/// edge types, a map is used to store vertex source and target.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Authors: Dimitrios Michail, Barak Naveh.</remarks>
public class UniformIntrusiveEdgesSpecifics<TVertex, TEdge> :
    BaseIntrusiveEdgesSpecifics<TVertex, TEdge, IntrusiveEdge>,
    IIntrusiveEdgesSpecifics<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="map"> the map to use for storage.</param>
    public UniformIntrusiveEdgesSpecifics(IDictionary<TEdge, IntrusiveEdge> map)
        : base(map)
    {
    }

    public override bool Add(TEdge edge, TVertex sourceVertex, TVertex targetVertex)
    {
        if (edge is IntrusiveEdge intrusive)
        {
            return AddIntrusiveEdge(edge, sourceVertex, targetVertex, intrusive);
        }

        var previousSize  = EdgeMap.Count;
        var intrusiveEdge = EdgeMap.GetOrCompute(edge, _ => new IntrusiveEdge());
        if (previousSize < EdgeMap.Count)
        {
            // edge was added
            intrusiveEdge.Source = sourceVertex;
            intrusiveEdge.Target = targetVertex;
            return true;
        }

        return false;
    }

    protected override IntrusiveEdge GetIntrusiveEdge(TEdge edge)
    {
        return edge switch
        {
            IntrusiveEdge intrusiveEdge => intrusiveEdge,
            _                           => EdgeMap[edge],
        };
    }
}
