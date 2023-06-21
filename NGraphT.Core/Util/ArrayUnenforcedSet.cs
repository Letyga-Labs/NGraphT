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

namespace NGraphT.Core.Util;

/// <summary>
/// Helper for efficiently representing small sets whose elements are known to be unique by
/// construction, implying we don't need to enforce the uniqueness property in the data structure
/// itself. Use with caution.
///
/// <para>
/// Note that for equals/hashCode, the class implements the Set behavior (unordered), not the list
/// behavior (ordered); the fact that it subclasses ArrayList should be considered an implementation
/// detail.
/// </para>
/// </summary>
///
/// <typeparam name="TElement">the element type.</typeparam>
///
/// <remarks>Author: John V. Sichi.</remarks>
public sealed class ArrayUnenforcedSet<TElement> : IList<TElement>, ISet<TElement>
{
    private List<TElement> _delegate;

    /// <summary>
    /// Constructs a new empty set.
    /// </summary>
    public ArrayUnenforcedSet()
    {
        _delegate = new List<TElement>();
    }

    /// <summary>
    /// Constructs a set containing the elements of the specified collection.
    /// </summary>
    /// <param name="c"> the collection whose elements are to be placed into this set.</param>
    /// <exception cref="NullReferenceException"> if the specified collection is null.</exception>
    public ArrayUnenforcedSet(ICollection<TElement> c)
    {
        _delegate = new List<TElement>(c);
    }

    /// <summary>
    /// Constructs an empty set with the specified initial capacity.
    /// </summary>
    /// <param name="capacity"> the initial capacity of the set.</param>
    /// <exception cref="ArgumentException"> if the specified initial capacity is negative.</exception>
    public ArrayUnenforcedSet(int capacity)
    {
        _delegate = new List<TElement>(capacity);
    }

    /// <inheritdoc/>
    public int Count => _delegate.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public TElement this[int index]
    {
        get => _delegate[index];
        set => _delegate[index] = value;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<TElement> GetEnumerator()
    {
        return _delegate.GetEnumerator();
    }

    /// <inheritdoc/>
    void ICollection<TElement>.Add(TElement item)
    {
        _delegate.Add(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _delegate.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(TElement item)
    {
        return _delegate.Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(TElement[] array, int arrayIndex)
    {
        _delegate.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public bool Remove(TElement item)
    {
        return _delegate.Remove(item);
    }

    /// <inheritdoc/>
    public int IndexOf(TElement item)
    {
        return _delegate.IndexOf(item);
    }

    /// <inheritdoc/>
    public void Insert(int index, TElement item)
    {
        _delegate.Insert(index, item);
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        _delegate.RemoveAt(index);
    }

    /// <inheritdoc/>
    bool ISet<TElement>.Add(TElement item)
    {
        if (Contains(item))
        {
            return false;
        }
        else
        {
            _delegate.Add(item);
            return true;
        }
    }

    /// <inheritdoc/>
    public void ExceptWith(IEnumerable<TElement> other)
    {
        _delegate = _delegate.Except(other).ToList();
    }

    /// <inheritdoc/>
    public void IntersectWith(IEnumerable<TElement> other)
    {
        _delegate = _delegate.Intersect(other).ToList();
    }

    /// <inheritdoc/>
    public bool IsProperSubsetOf(IEnumerable<TElement> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var found       = new bool[Count];
        var anyNotFound = false;
        foreach (var element in other)
        {
            var index = _delegate.IndexOf(element);
            if (index != -1)
            {
                found[index] = true;
            }
            else
            {
                anyNotFound = true;
            }
        }

        return Array.TrueForAll(found, it => it) && anyNotFound;
    }

    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<TElement> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var found = new bool[Count];
        foreach (var element in other)
        {
            var index = _delegate.IndexOf(element);
            if (index != -1)
            {
                found[index] = true;
            }
            else
            {
                return false;
            }
        }

        return Array.Exists(found, it => !it);
    }

    /// <inheritdoc/>
    public bool IsSubsetOf(IEnumerable<TElement> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var found = new bool[Count];
        foreach (var element in other)
        {
            var index = _delegate.IndexOf(element);
            if (index != -1)
            {
                found[index] = true;
            }
        }

        return Array.TrueForAll(found, it => it);
    }

    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<TElement> other)
    {
        return other.All(_delegate.Contains);
    }

    /// <inheritdoc/>
    public bool Overlaps(IEnumerable<TElement> other)
    {
        return other.Any(_delegate.Contains);
    }

    /// <inheritdoc/>
    public bool SetEquals(IEnumerable<TElement> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var found = new bool[Count];
        foreach (var element in other)
        {
            var index = _delegate.IndexOf(element);
            if (index != -1)
            {
                found[index] = true;
            }
            else
            {
                return false;
            }
        }

        return Array.TrueForAll(found, it => it);
    }

    /// <inheritdoc/>
    public void SymmetricExceptWith(IEnumerable<TElement> other)
    {
        var interseciton = _delegate.Intersect(other).ToList();
        _delegate = _delegate.Where(it => !interseciton.Contains(it)).ToList();
    }

    /// <inheritdoc/>
    public void UnionWith(IEnumerable<TElement> other)
    {
        _delegate = _delegate.Union(other).ToList();
    }
}
