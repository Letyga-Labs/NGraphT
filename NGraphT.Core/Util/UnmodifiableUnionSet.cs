/*
 * (C) Copyright 2018-2021, by Dimitrios Michail and Contributors.
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
/// An unmodifiable live view of the union of two sets.
/// </summary>
/// @param <TEdge> the element type
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public class UnmodifiableUnionSet<TEdge> : AbstractSet<TEdge>
{
    private readonly ISet<TEdge> _first;
    private readonly ISet<TEdge> _second;

    /// <summary>
    /// Constructs a new set.
    /// </summary>
    /// <param name="first"> the first set.</param>
    /// <param name="second"> the second set.</param>
    public UnmodifiableUnionSet(ISet<TEdge> first, ISet<TEdge> second)
    {
        Objects.requireNonNull(first);
        Objects.requireNonNull(second);
        _first  = first;
        _second = second;
    }

    public override IEnumerator<TEdge> Iterator()
    {
        return new UnionIterator(this, OrderSetsBySize());
    }

    /// <summary>
    /// <inheritdoc/>
    ///
    /// Since the view is live, this operation is no longer a constant time operation.
    /// </summary>
    public override int Size()
    {
        var ordering = OrderSetsBySize();
        var     bigger   = ordering.Bigger;
        var             count    = ordering.BiggerSize;
        foreach (var edge in ordering.Smaller)
        {
            if (!bigger.Contains(edge))
            {
                count++;
            }
        }

        return count;
    }

    public override bool Contains(object o)
    {
        return _first.Contains(o) || _second.Contains(o);
    }

    private SetSizeOrdering OrderSetsBySize()
    {
        var firstSize  = _first.Count;
        var secondSize = _second.Count;
        if (secondSize > firstSize)
        {
            return new SetSizeOrdering(this, _second, _first, secondSize, firstSize);
        }
        else
        {
            return new SetSizeOrdering(this, _first, _second, firstSize, secondSize);
        }
    }

    // note that these inner classes could be static, but we
    // declare them as non-static to avoid the clutter from
    // duplicating the generic type parameter

    private class SetSizeOrdering
    {
        private readonly UnmodifiableUnionSet<TEdge> _outerInstance;

        internal readonly ISet<TEdge> Bigger;
        internal readonly ISet<TEdge> Smaller;
        internal readonly int         BiggerSize;
        internal readonly int         SmallerSize;

        internal SetSizeOrdering(
            UnmodifiableUnionSet<TEdge> outerInstance,
            ISet<TEdge>                 bigger,
            ISet<TEdge>                 smaller,
            int                         biggerSize,
            int                         smallerSize
        )
        {
            _outerInstance = outerInstance;
            Bigger         = bigger;
            Smaller        = smaller;
            BiggerSize     = biggerSize;
            SmallerSize    = smallerSize;
        }
    }

    private class UnionIterator : IEnumerator<TEdge>
    {
        private readonly UnmodifiableUnionSet<TEdge> _outerInstance;

        internal SetSizeOrdering    Ordering;
        internal bool               InBiggerSet;
        internal IEnumerator<TEdge> Iterator;
        internal TEdge              Cur;

        internal UnionIterator(UnmodifiableUnionSet<TEdge> outerInstance, SetSizeOrdering ordering)
        {
            _outerInstance = outerInstance;
            Ordering       = ordering;
            InBiggerSet    = true;
            Iterator       = ordering.Bigger.GetEnumerator();
            Cur            = Prefetch();
        }

        public override bool HasNext()
        {
            if (Cur != null)
            {
                return true;
            }

            return (Cur = Prefetch()) != null;
        }

        public override TEdge Next()
        {
            if (!HasNext())
            {
                throw new NoSuchElementException();
            }

            var result = Cur;
            Cur = default(TEdge);
            return result;
        }

        internal virtual TEdge Prefetch()
        {
            while (true)
            {
                if (InBiggerSet)
                {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                    if (Iterator.hasNext())
                    {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                        return Iterator.next();
                    }
                    else
                    {
                        InBiggerSet = false;
                        Iterator    = Ordering.Smaller.GetEnumerator();
                    }
                }
                else
                {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                    if (Iterator.hasNext())
                    {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                        TEdge elem = Iterator.next();
                        if (!Ordering.Bigger.Contains(elem))
                        {
                            return elem;
                        }
                    }
                    else
                    {
                        return default(TEdge);
                    }
                }
            }
        }
    }
}
