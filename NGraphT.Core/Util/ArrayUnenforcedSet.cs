/*
 * (C) Copyright 2006-2021, by John TNode Sichi and Contributors.
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
/// Helper for efficiently representing small sets whose elements are known to be unique by
/// construction, implying we don't need to enforce the uniqueness property in the data structure
/// itself. Use with caution.
///
/// <para>
/// Note that for equals/hashCode, the class implements the Set behavior (unordered), not the list
/// behavior (ordered); the fact that it subclasses ArrayList should be considered an implementation
/// detail.
///
/// </para>
/// </summary>
/// @param <TEdge> the element type
///
/// <remarks>Author: John TNode. Sichi.</remarks>
public class ArrayUnenforcedSet<TEdge> : List<TEdge>, ISet<TEdge>
{
    /// <summary>
    /// Constructs a new empty set
    /// </summary>
    public ArrayUnenforcedSet()
        : base()
    {
    }

    /// <summary>
    /// Constructs a set containing the elements of the specified collection.
    /// </summary>
    /// <param name="c"> the collection whose elements are to be placed into this set.</param>
    /// <exception cref="NullReferenceException"> if the specified collection is null.</exception>
//JAVA TO C# CONVERTER TODO TASK: Wildcard generics in method parameters are not converted:
//ORIGINAL LINE: public ArrayUnenforcedSet(Collection<? extends TEdge> c)
    public ArrayUnenforcedSet(ICollection<TEdge> c)
        : base(c)
    {
    }

    /// <summary>
    /// Constructs an empty set with the specified initial capacity.
    /// </summary>
    /// <param name="n"> the initial capacity of the set.</param>
    /// <exception cref="ArgumentException"> if the specified initial capacity is negative.</exception>
    public ArrayUnenforcedSet(int n)
        : base(n)
    {
    }

    public override bool Equals(object o)
    {
        return (new SetForEquality(this)).Equals(o);
    }

    public override int GetHashCode()
    {
        return (new SetForEquality(this)).GetHashCode();
    }

    /// <summary>
    /// Multiple inheritance helper.
    /// </summary>
    private class SetForEquality : AbstractSet<TEdge>
    {
        private readonly ArrayUnenforcedSet<TEdge> _outerInstance;

        public SetForEquality(ArrayUnenforcedSet<TEdge> outerInstance)
        {
            _outerInstance = outerInstance;
        }

        public override IEnumerator<TEdge> Iterator()
        {
            return _outerInstance.GetEnumerator();
        }

        public override int Size()
        {
            return _outerInstance.Count;
        }
    }
}
