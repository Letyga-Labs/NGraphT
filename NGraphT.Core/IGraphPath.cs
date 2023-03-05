/*
 * (C) Copyright 2008-2021, by John TNode Sichi and Contributors.
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

using NGraphT.Core.DotNetUtil;

namespace NGraphT.Core;

/// <summary>
/// A GraphPath represents a <a href="http://mathworld.wolfram.com/Path.html"> path</a> in a
/// <see cref="Graph"/>. Unlike some definitions, the path is not required to be a
/// <a href="https://en.wikipedia.org/wiki/Simple_path">Simple Path</a>.
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: John Sichi.</remarks>
public interface IGraphPath<TNode, TEdge>
{
    /// <summary>
    /// Returns the graph over which this path is defined. The path may also be valid with respect to
    /// other graphs.
    /// </summary>
    /// <returns>the containing graph.</returns>
    IGraph<TNode, TEdge> Graph { get; }

    /// <summary>
    /// Returns the start vertex in the path.
    /// </summary>
    /// <returns>the start vertex.</returns>
    TNode StartVertex { get; }

    /// <summary>
    /// Returns the end vertex in the path.
    /// </summary>
    /// <returns>the end vertex.</returns>
    TNode EndVertex { get; }

    /// <summary>
    /// Returns the edges making up the path. The first edge in this path is incident to the start
    /// vertex. The last edge is incident to the end vertex. The vertices along the path can be
    /// obtained by traversing from the start vertex, finding its opposite across the first edge, and
    /// then doing the same successively across subsequent edges; see <see cref="VertexList"/>.
    ///
    /// <para>
    /// Whether or not the returned edge list is modifiable depends on the path implementation.
    /// </para>
    ///
    /// </summary>
    /// <returns>list of edges traversed by the path.</returns>
    IList<TEdge> EdgeList
    {
        get
        {
            var vertexList = VertexList;
            if (vertexList.Count < 2)
            {
                return Array.Empty<TEdge>();
            }

            var g = Graph;
            var edgeList = vertexList.Pairwise()
                .Select(it => g.GetEdge(it.Previous, it.Current))
                .ToList();
            return edgeList;
        }
    }

    /// <summary>
    /// Returns the path as a sequence of vertices.
    /// </summary>
    /// <returns>path, denoted by a list of vertices.</returns>
    IList<TNode> VertexList
    {
        get
        {
            var edgeList = EdgeList;

            if (!edgeList.Any())
            {
                var startVertex = StartVertex;
                if (startVertex != null && startVertex.Equals(EndVertex))
                {
                    return new List<TNode> { startVertex, };
                }
                else
                {
                    return Array.Empty<TNode>();
                }
            }

            var g    = Graph;
            var list = new List<TNode>();
            var node = StartVertex;
            list.Add(node);
            foreach (var edge in edgeList)
            {
                node = Graphs.GetOppositeVertex(g, edge, node);
                list.Add(node);
            }

            return list;
        }
    }

    /// <summary>
    /// Returns the weight assigned to the path. Typically, this will be the sum of the weights of
    /// the edge list entries (as defined by the containing graph), but some path implementations may
    /// use other definitions.
    /// </summary>
    /// <returns>the weight of the path.</returns>
    double Weight { get; }

    /// <summary>
    /// Returns the length of the path, measured in the number of edges.
    /// </summary>
    /// <returns>the length of the path, measured in the number of edges.</returns>
    int Length => EdgeList.Count;
}
