/*
 * (C) Copyright 2018-2021, by Joris Kinable and Contributors.
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
/// Implementation of a weighted, unmodifiable set. This class can for instance be used to store a
/// weighted vertex cover. The {@code hashCode()} and {@code equals()} methods are identical to those
/// of a normal set, i.TEdge. they are independent of the <c>weight</c> of this class. All methods are
/// delegated to the underlying set.
/// </summary>
/// @param <TEdge> element type
///
/// <remarks>Author: Joris Kinable.</remarks>
public class WeightedUnmodifiableSet<TEdge> : AbstractSet<TEdge>
{
    public readonly ISet<TEdge> BackingSet;
    public readonly double      Weight;

    /// <summary>
    /// Constructs a WeightedUnmodifiableSet instance.
    /// </summary>
    /// <param name="backingSet"> underlying set.</param>
    public WeightedUnmodifiableSet(ISet<TEdge> backingSet)
    {
        BackingSet = backingSet;
        this.weight     = backingSet.Count;
    }

    /// <summary>
    /// Constructs a WeightedUnmodifiableSet instance.
    /// </summary>
    /// <param name="backingSet"> underlying set.</param>
    /// <param name="weight"> weight of the set.</param>
    public WeightedUnmodifiableSet(ISet<TEdge> backingSet, double weight)
    {
        BackingSet = backingSet;
        this.weight     = weight;
    }

    /// <summary>
    /// Returns the weight of the set.
    /// </summary>
    /// <returns>weight of the set.</returns>
    public virtual double Weight
    {
        get
        {
            return weight;
        }
    }

    public override int Size()
    {
        return BackingSet.Count;
    }

    public override bool Empty
    {
        get
        {
            return BackingSet.Count == 0;
        }
    }

    public override bool Contains(object o)
    {
        return BackingSet.Contains(o);
    }

    public override IEnumerator<TEdge> Iterator()
    {
        return BackingSet.GetEnumerator();
    }

    public override object[] ToArray()
    {
        return BackingSet.ToArray();
    }

    public override T[] ToArray<T>(T[] a)
    {
        return BackingSet.toArray(a);
    }

    public override bool Add(TEdge node)
    {
        throw new NotSupportedException("This set is unmodifiable");
    }

    public override bool Remove(object o)
    {
        throw new NotSupportedException("This set is unmodifiable");
    }

    public override bool ContainsAll<T1>(ICollection<T1> c)
    {
        return BackingSet.ContainsAll(c);
    }

    public override bool AddAll<T1>(ICollection<T1> c) where T1 : TEdge
    {
        throw new NotSupportedException("This set is unmodifiable");
    }

    public override bool RetainAll<T1>(ICollection<T1> c)
    {
        throw new NotSupportedException("This set is unmodifiable");
    }

    public override bool RemoveAll<T1>(ICollection<T1> c)
    {
        throw new NotSupportedException("This set is unmodifiable");
    }

    public override void Clear()
    {
        throw new NotSupportedException("This set is unmodifiable");
    }

    public override bool Equals(object o)
    {
        if (this == o)
        {
            return true;
        }

        if (!(o is WeightedUnmodifiableSet))
        {
            return false;
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") WeightedUnmodifiableSet<TEdge> other = (WeightedUnmodifiableSet<TEdge>) o;
        var other = (WeightedUnmodifiableSet<TEdge>)o;
        return BackingSet.SetEquals(other.BackingSet);
    }

    public override int GetHashCode()
    {
        return BackingSet.GetHashCode();
    }
}
