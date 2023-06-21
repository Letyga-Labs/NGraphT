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
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
///
/// <remarks>Author: Alexandru Valeanu.</remarks>
public sealed class VertexToIntegerMapping<TVertex>
    where TVertex : class
{
    private readonly IDictionary<TVertex, int> _vertexMap;
    private readonly IList<TVertex>            _indexList;

    /// <summary>
    /// Create a new mapping from a list of vertices. The input list will be used as the
    /// <c>indexList</c> so it must not be modified.
    /// </summary>
    /// <param name="vertices"> the input list of vertices.</param>
    /// <exception cref="NullReferenceException"> if <c>vertices</c> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"> if the vertices are not distinct.</exception>
    public VertexToIntegerMapping(IList<TVertex> vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        _vertexMap = CollectionUtil.NewHashMapWithExpectedSize<TVertex, int>(vertices.Count);
        _indexList = vertices;

        foreach (var v in vertices)
        {
            if (_vertexMap.TryGetValue(v, out _))
            {
                throw new ArgumentException("vertices are not distinct", nameof(vertices));
            }
            else
            {
                _vertexMap[v] = _vertexMap.Count;
            }
        }
    }

    /// <summary>
    /// Create a new mapping from a collection of vertices.
    /// </summary>
    /// <param name="vertices"> the input collection of vertices.</param>
    /// <exception cref="NullReferenceException"> if <c>vertices</c> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"> if the vertices are not distinct.</exception>
    public VertexToIntegerMapping(ICollection<TVertex> vertices)
        : this(new List<TVertex>(vertices))
    {
    }

    /// <summary>
    /// Get the <c>vertexMap</c>, a mapping from vertices to integers (i.e. the inverse of <c>indexList</c>).
    /// </summary>
    /// <returns>a mapping from vertices to integers.</returns>
    public IDictionary<TVertex, int> VertexMap => _vertexMap;

    /// <summary>
    /// Get the <c>indexList</c>, a mapping from integers to vertices (i.e. the inverse of <c>vertexMap</c>).
    /// </summary>
    /// <returns>a mapping from integers to vertices.</returns>
    public IList<TVertex> IndexList => _indexList;
}
