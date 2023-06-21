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

using System.Collections;

namespace NGraphT.Core.Graph;

/// <summary>
/// Helper for <see cref="MaskSubgraph{TVertex,TEdge}"/>.
/// </summary>
internal sealed class MaskEdgeSet<TVertex, TEdge> : ISet<TEdge>
    where TVertex : class
    where TEdge : class
{
    private const string SetIsReadOnly = "Cannot perform set modification: set is read only.";

    private readonly IGraph<TVertex, TEdge> _graph;
    private readonly ISet<TEdge>            _edgeSet;
    private readonly Predicate<TVertex>     _vertexMask;
    private readonly Predicate<TEdge>       _edgeMask;

    public MaskEdgeSet(
        IGraph<TVertex, TEdge> graph,
        ISet<TEdge>            edgeSet,
        Predicate<TVertex>     vertexMask,
        Predicate<TEdge>       edgeMask
    )
    {
        _graph      = graph;
        _edgeSet    = edgeSet;
        _vertexMask = vertexMask;
        _edgeMask   = edgeMask;
    }

    public int Count => _edgeSet.Count(IsNotMaskedEdge);

    public bool IsReadOnly => true;

    public bool IsProperSubsetOf(IEnumerable<TEdge> other)
    {
        var copy = _edgeSet.Where(IsNotMaskedEdge).ToHashSet();
        return copy.IsProperSupersetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<TEdge> other)
    {
        var tmp = other.ToList();
        return tmp.TrueForAll(IsNotMaskedEdge) && _edgeSet.IsProperSupersetOf(tmp);
    }

    public bool IsSubsetOf(IEnumerable<TEdge> other)
    {
        var copy = _edgeSet.Where(IsNotMaskedEdge).ToHashSet();
        return copy.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<TEdge> other)
    {
        var tmp = other.ToList();
        return tmp.TrueForAll(IsNotMaskedEdge) && _edgeSet.IsSupersetOf(tmp);
    }

    public bool Overlaps(IEnumerable<TEdge> other)
    {
        return other.Any(it => _edgeSet.Contains(it) && IsNotMaskedEdge(it));
    }

    public bool SetEquals(IEnumerable<TEdge> other)
    {
        var copy = _edgeSet.Where(IsNotMaskedEdge).ToHashSet();
        return copy.SetEquals(other);
    }

    public bool Contains(TEdge item)
    {
        return _edgeSet.Contains(item) && IsNotMaskedEdge(item);
    }

    public void CopyTo(TEdge[] array, int arrayIndex)
    {
        foreach (var vertex in _edgeSet.Where(IsNotMaskedEdge))
        {
            array[arrayIndex++] = vertex;
        }
    }

    public IEnumerator<TEdge> GetEnumerator()
    {
        return _edgeSet.Where(IsNotMaskedEdge).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _edgeSet.GetEnumerator();
    }

    bool ISet<TEdge>.Add(TEdge item)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void ExceptWith(IEnumerable<TEdge> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void IntersectWith(IEnumerable<TEdge> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void SymmetricExceptWith(IEnumerable<TEdge> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void UnionWith(IEnumerable<TEdge> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    void ICollection<TEdge>.Add(TEdge item)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void Clear()
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public bool Remove(TEdge item)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    private bool IsNotMaskedEdge(TEdge edge)
    {
        return !_edgeMask(edge) &&
               !_vertexMask(_graph.GetEdgeSource(edge)) &&
               !_vertexMask(_graph.GetEdgeTarget(edge));
    }
}
