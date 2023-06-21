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
using System.Diagnostics.CodeAnalysis;

namespace NGraphT.Core.Util;

/// <summary>
/// Implementation of a weighted, unmodifiable set. This class can for instance be used to store a
/// weighted vertex cover. The <c>GetHashCode()</c> and <c>Equals()</c> methods are identical to those
/// of a normal set, i.e. they are independent of the <c>weight</c> of this class. All methods are
/// delegated to the underlying set.
/// </summary>
///
/// <typeparam name="TElement">element type.</typeparam>
///
/// <remarks>Author: Joris Kinable.</remarks>
[SuppressMessage("Major Code Smell", "S4035:Classes implementing \"IEquatable<T>\" should be sealed")]
public class WeightedUnmodifiableSet<TElement> : ISet<TElement>, IEquatable<WeightedUnmodifiableSet<TElement>>
{
    private const string SetIsReadOnly = "Cannot perform set modification: set is read only.";

    private readonly ISet<TElement> _backingSet;
    private readonly double         _weight;

    /// <summary>
    /// Constructs a WeightedUnmodifiableSet instance.
    /// </summary>
    /// <param name="backingSet"> underlying set.</param>
    public WeightedUnmodifiableSet(ISet<TElement> backingSet)
    {
        ArgumentNullException.ThrowIfNull(backingSet);
        _backingSet = backingSet;
        _weight     = backingSet.Count;
    }

    /// <summary>
    /// Constructs a WeightedUnmodifiableSet instance.
    /// </summary>
    /// <param name="backingSet"> underlying set.</param>
    /// <param name="weight"> weight of the set.</param>
    public WeightedUnmodifiableSet(ISet<TElement> backingSet, double weight)
    {
        _backingSet = backingSet;
        _weight     = weight;
    }

    /// <summary>
    /// Returns the weight of the set.
    /// </summary>
    /// <returns>weight of the set.</returns>
    public virtual double Weight => _weight;

    public int Count => _backingSet.Count;

    public bool IsReadOnly => true;

    public static bool operator ==(WeightedUnmodifiableSet<TElement>? left, WeightedUnmodifiableSet<TElement>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(WeightedUnmodifiableSet<TElement>? left, WeightedUnmodifiableSet<TElement>? right)
    {
        return !Equals(left, right);
    }

    public bool IsProperSubsetOf(IEnumerable<TElement> other)
    {
        return _backingSet.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<TElement> other)
    {
        return _backingSet.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<TElement> other)
    {
        return _backingSet.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<TElement> other)
    {
        return _backingSet.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<TElement> other)
    {
        return _backingSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<TElement> other)
    {
        return _backingSet.SetEquals(other);
    }

    public bool Contains(TElement item)
    {
        return _backingSet.Contains(item);
    }

    public void CopyTo(TElement[] array, int arrayIndex)
    {
        _backingSet.CopyTo(array, arrayIndex);
    }

    public IEnumerator<TElement> GetEnumerator()
    {
        return _backingSet.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    bool ISet<TElement>.Add(TElement item)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void ExceptWith(IEnumerable<TElement> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void IntersectWith(IEnumerable<TElement> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void SymmetricExceptWith(IEnumerable<TElement> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void UnionWith(IEnumerable<TElement> other)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    void ICollection<TElement>.Add(TElement item)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public void Clear()
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public bool Remove(TElement item)
    {
        throw new NotSupportedException(SetIsReadOnly);
    }

    public bool Equals(WeightedUnmodifiableSet<TElement>? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _backingSet.Equals(other._backingSet);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((WeightedUnmodifiableSet<TElement>)obj);
    }

    public override int GetHashCode()
    {
        return _backingSet.GetHashCode();
    }
}
