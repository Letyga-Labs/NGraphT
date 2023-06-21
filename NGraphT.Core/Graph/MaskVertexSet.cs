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
internal sealed class MaskVertexSet<TVertex> : ISet<TVertex>
    where TVertex : class
{
    private const string SetIsReadOnly = "Cannot perform set modification: set is read only.";

    private readonly ISet<TVertex>      _vertexSet;
    private readonly Predicate<TVertex> _mask;

    public MaskVertexSet(ISet<TVertex> vertexSet, Predicate<TVertex> mask)
    {
        _vertexSet = vertexSet;
        _mask      = mask;
    }

    public int Count => _vertexSet.Count(it => !_mask(it));

    public bool IsReadOnly => true;

    public bool IsProperSubsetOf(IEnumerable<TVertex> other)
    {
        var copy = _vertexSet.Where(it => !_mask(it)).ToHashSet();
        return copy.IsProperSupersetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<TVertex> other)
    {
        var tmp = other.ToList();
        return !tmp.Exists(it => _mask(it)) && _vertexSet.IsProperSupersetOf(tmp);
    }

    public bool IsSubsetOf(IEnumerable<TVertex> other)
    {
        var copy = _vertexSet.Where(it => !_mask(it)).ToHashSet();
        return copy.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<TVertex> other)
    {
        var tmp = other.ToList();
        return !tmp.Exists(it => _mask(it)) && _vertexSet.IsSupersetOf(tmp);
    }

    public bool Overlaps(IEnumerable<TVertex> other)
    {
        return other.Any(it => _vertexSet.Contains(it) && !_mask(it));
    }

    public bool SetEquals(IEnumerable<TVertex> other)
    {
        var copy = _vertexSet.Where(it => !_mask(it)).ToHashSet();
        return copy.SetEquals(other);
    }

    public bool Contains(TVertex item)
    {
        return _vertexSet.Contains(item) && !_mask(item);
    }

    public void CopyTo(TVertex[] array, int arrayIndex)
    {
        foreach (var vertex in _vertexSet.Where(it => !_mask(it)))
        {
            array[arrayIndex++] = vertex;
        }
    }

    public IEnumerator<TVertex> GetEnumerator()
    {
        return _vertexSet.Where(it => !_mask(it)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _vertexSet.GetEnumerator();
    }

    bool ISet<TVertex>.Add(TVertex item)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void ExceptWith(IEnumerable<TVertex> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void IntersectWith(IEnumerable<TVertex> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void SymmetricExceptWith(IEnumerable<TVertex> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void UnionWith(IEnumerable<TVertex> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    void ICollection<TVertex>.Add(TVertex item)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void Clear()
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public bool Remove(TVertex item)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }
}
