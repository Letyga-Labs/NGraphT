/*
 * (C) Copyright 2018-2021, by Alexandru Valeanu and Contributors.
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

namespace NGraphT.Core.Util;

/// <summary>
/// Helper class for building a one-to-one mapping for a collection of vertices to the integer range
/// $[0, n)$ where $n$ is the number of vertices in the collection.
///
/// <para>
/// This class computes the mapping only once, on instantiation. It does not support live updates.
/// </para>
/// </summary>
///
/// <remarks>Author: Alexandru Valeanu.</remarks>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
public class VertexToIntegerMapping<TNode>
{
    private readonly IDictionary<TNode, int> _vertexMap;
    private readonly IList<TNode>            _indexList;

    /// <summary>
    /// Create a new mapping from a list of vertices. The input list will be used as the
    /// {@code indexList} so it must not be modified.
    /// </summary>
    /// <param name="vertices"> the input list of vertices.</param>
    /// <exception cref="NullReferenceException"> if {@code vertices} is {@code null} </exception>
    /// <exception cref="ArgumentException"> if the vertices are not distinct.</exception>
    public VertexToIntegerMapping(IList<TNode> vertices)
    {
        Objects.requireNonNull(vertices, "the input collection of vertices cannot be null");

        _vertexMap = CollectionUtil.NewHashMapWithExpectedSize(vertices.Count);
        _indexList = vertices;

        foreach (var node in vertices)
        {
            if (_vertexMap.put(node, _vertexMap.Count) != null)
            {
                throw new ArgumentException("vertices are not distinct");
            }
        }
    }

    /// <summary>
    /// Create a new mapping from a collection of vertices.
    /// </summary>
    /// <param name="vertices"> the input collection of vertices.</param>
    /// <exception cref="NullReferenceException"> if {@code vertices} is {@code null} </exception>
    /// <exception cref="ArgumentException"> if the vertices are not distinct.</exception>
    public VertexToIntegerMapping(ICollection<TNode> vertices)
        : this(new List<>(Objects.requireNonNull(vertices, "the input collection of vertices cannot be null")))
    {
    }

    /// <summary>
    /// Get the {@code vertexMap}, a mapping from vertices to integers (i.TEdge. the inverse of
    /// {@code indexList}).
    /// </summary>
    /// <returns>a mapping from vertices to integers.</returns>
    public virtual IDictionary<TNode, int> VertexMap
    {
        get
        {
            return _vertexMap;
        }
    }

    /// <summary>
    /// Get the {@code indexList}, a mapping from integers to vertices (i.TEdge. the inverse of
    /// {@code vertexMap}).
    /// </summary>
    /// <returns>a mapping from integers to vertices.</returns>
    public virtual IList<TNode> IndexList
    {
        get
        {
            return _indexList;
        }
    }
}
