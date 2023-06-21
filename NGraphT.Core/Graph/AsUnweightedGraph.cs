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

namespace NGraphT.Core.Graph;

/// <summary>
/// Provides an unweighted view on a graph.
///
/// Algorithms designed for unweighted graphs should also work on weighted graphs. This class
/// emulates an unweighted graph based on a weighted one by returning <c>Graph.DEFAULT_EDGE_WEIGHT
/// </c> for each edge weight. The underlying weighted graph is provided at the constructor.
/// Modifying operations (adding/removing vertexes/edges) are also passed through to the underlying
/// weighted graph. As edge weight, Graph.DEFAULT_EDGE_WEIGHT is used. Setting an edge weight is not
/// supported. The edges are not modified. So, if an edge is asked for, the one from the underlying
/// weighted graph is returned. In case the underlying graph is serializable, this one is
/// serializable, too.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
public class AsUnweightedGraph<TVertex, TEdge> : GraphDelegator<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Constructor for AsUnweightedGraph.
    /// </summary>
    /// <param name="g"> the backing directed graph over which an undirected view is to be created.</param>
    /// <exception cref="NullReferenceException"> if the graph is null.</exception>
    public AsUnweightedGraph(IGraph<TVertex, TEdge> g)
        : base(g)
    {
    }

    public override IGraphType Type => base.Type.AsUnweighted();

    public override double GetEdgeWeight(TEdge edge)
    {
        return IGraph<TVertex, TEdge>.DefaultEdgeWeight;
    }

    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        throw new NotSupportedException("Edge weight is not supported");
    }
}
