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
/// A read-only live view of the union of two sets.
/// </summary>
///
/// <typeparam name="TElement">the element type.</typeparam>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public sealed class ReadOnlySetUnion<TElement> : ISet<TElement>
    where TElement : class
{
    private const string UnionViewIsReadOnly = "Set union is read-only";

    private readonly ISet<TElement> _first;
    private readonly ISet<TElement> _second;

    /// <summary>
    /// Constructs a new set.
    /// </summary>
    /// <param name="first"> the first set.</param>
    /// <param name="second"> the second set.</param>
    public ReadOnlySetUnion(ISet<TElement> first, ISet<TElement> second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        _first  = first;
        _second = second;
    }

    public int Count
    {
        get
        {
            var ordering = OrderSetsBySize();
            var bigger   = ordering.Bigger;
            var count    = ordering.BiggerSize;
            foreach (var edge in ordering.Smaller)
            {
                if (!bigger.Contains(edge))
                {
                    count++;
                }
            }

            return count;
        }
    }

    public bool IsReadOnly => true;

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<TElement> GetEnumerator()
    {
        return new UnionIterator(OrderSetsBySize());
    }

    void ICollection<TElement>.Add(TElement item)
    {
        throw new NotSupportedException(UnionViewIsReadOnly);
    }

    public void Clear()
    {
        throw new NotSupportedException(UnionViewIsReadOnly);
    }

    public bool Contains(TElement item)
    {
        return _first.Contains(item) || _second.Contains(item);
    }

    public void CopyTo(TElement[] array, int arrayIndex)
    {
        _first.CopyTo(array, arrayIndex);
        _second.CopyTo(array, arrayIndex + _first.Count);
    }

    public bool Remove(TElement item)
    {
        throw new NotSupportedException(UnionViewIsReadOnly);
    }

    bool ISet<TElement>.Add(TElement item)
    {
        throw new NotSupportedException(UnionViewIsReadOnly);
    }

    public void ExceptWith(IEnumerable<TElement> other)
    {
        throw new NotSupportedException(UnionViewIsReadOnly);
    }

    public void IntersectWith(IEnumerable<TElement> other)
    {
        throw new NotSupportedException(UnionViewIsReadOnly);
    }

    public bool IsProperSubsetOf(IEnumerable<TElement> other)
    {
        var otherSet = other as ISet<TElement> ?? other.ToHashSet();
        return IsSubsetOf(otherSet) &&
               otherSet.Any(it => !(_first.Contains(it) && _second.Contains(it)));
    }

    public bool IsProperSupersetOf(IEnumerable<TElement> other)
    {
        var otherSet = other as ISet<TElement> ?? other.ToHashSet();
        return IsSupersetOf(otherSet) &&
               (_first.Any(it => !otherSet.Contains(it)) || _second.Any(it => !otherSet.Contains(it)));
    }

    public bool IsSubsetOf(IEnumerable<TElement> other)
    {
        var ordering = OrderSetsBySize();
        var otherSet = other as ISet<TElement> ?? other.ToHashSet();
        return ordering.Smaller.All(it => otherSet.Contains(it)) &&
               ordering.Bigger.All(it => otherSet.Contains(it));
    }

    public bool IsSupersetOf(IEnumerable<TElement> other)
    {
        return other.All(it => _first.Contains(it) || _second.Contains(it));
    }

    public bool Overlaps(IEnumerable<TElement> other)
    {
        return other.Any(it => _first.Contains(it) || _second.Contains(it));
    }

    public bool SetEquals(IEnumerable<TElement> other)
    {
        var otherSet = other as ISet<TElement> ?? other.ToHashSet();
        return _first.Count + _second.Count == otherSet.Count &&
               IsSupersetOf(otherSet) &&
               IsSubsetOf(otherSet);
    }

    public void SymmetricExceptWith(IEnumerable<TElement> other)
    {
        throw new NotSupportedException(UnionViewIsReadOnly);
    }

    public void UnionWith(IEnumerable<TElement> other)
    {
        throw new NotSupportedException(UnionViewIsReadOnly);
    }

    private SetSizeOrdering OrderSetsBySize()
    {
        var firstSize  = _first.Count;
        var secondSize = _second.Count;
        if (secondSize > firstSize)
        {
            return new SetSizeOrdering
            {
                Bigger      = _second,
                Smaller     = _first,
                BiggerSize  = secondSize,
                SmallerSize = firstSize,
            };
        }
        else
        {
            return new SetSizeOrdering
            {
                Bigger      = _first,
                Smaller     = _second,
                BiggerSize  = firstSize,
                SmallerSize = secondSize,
            };
        }
    }

    private sealed record SetSizeOrdering
    {
        internal ISet<TElement> Bigger      { get; init; } = null!;
        internal ISet<TElement> Smaller     { get; init; } = null!;
        internal int            BiggerSize  { get; init; }
        internal int            SmallerSize { get; init; }
    }

    private sealed class UnionIterator : IEnumerator<TElement>
    {
        private readonly SetSizeOrdering _ordering;

        private bool                  _inBiggerSet;
        private IEnumerator<TElement> _iterator;
        private TElement?             _current;

        internal UnionIterator(SetSizeOrdering ordering)
        {
            _ordering    = ordering;
            _inBiggerSet = true;
            _iterator    = ordering.Bigger.GetEnumerator();
            _current     = Advance();
        }

        object? IEnumerator.Current => Current;

        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
        public TElement Current => _current ?? throw new NoSuchElementException();

        public bool MoveNext()
        {
            _current = Advance();
            return _current != null;
        }

        public void Reset()
        {
            throw new NotSupportedException("Reset");
        }

        public void Dispose()
        {
            _iterator.Dispose();
        }

        private TElement? Advance()
        {
            while (true)
            {
                if (_inBiggerSet)
                {
                    if (_iterator.MoveNext())
                    {
                        return _iterator.Current;
                    }
                    else
                    {
                        _inBiggerSet = false;
                        _iterator.Dispose();
                        _iterator = _ordering.Smaller.GetEnumerator();
                    }
                }
                else
                {
                    if (_iterator.MoveNext())
                    {
                        var elem = _iterator.Current;
                        if (!_ordering.Bigger.Contains(elem))
                        {
                            return elem;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}
