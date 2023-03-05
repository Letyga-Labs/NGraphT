/*
 * (C) Copyright 2020-2021, by Hannes Wellmann and Contributors.
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
/// Utility class to create <see cref="System.Collections.ICollection"/> instances.
///
/// <remarks>Author: Hannes Wellmann.</remarks>
/// 
/// </summary>
public class CollectionUtil
{
    private CollectionUtil()
    {
        // static use only
    }

    /// <summary>
    /// Returns a <see cref="System.Collections.Hashtable"/> with an initial capacity that is sufficient to hold
    /// {@code expectedSize} mappings without rehashing its internal backing storage.
    /// <para>
    /// The returned {@code HashMap} has a capacity that is the specified expected size divided by
    /// the load factor of the Map, which is sufficient to hold {@code expectedSize} mappings without
    /// rehashing. As the Javadoc of <see cref="System.Collections.Hashtable"/> states: "If the initial capacity is greater than
    /// the maximum number of entries divided by the load factor, no rehash operations will ever
    /// occur".
    /// </para>
    /// </summary>
    /// @param <K> the type of keys in the returned {@code HashMap} </param>
    /// @param <TNode> the type of values in the returned {@code HashMap} </param>
    /// <param name="expectedSize"> of mappings that will be put into the returned {@code HashMap} </param>
    /// <returns>an empty {@code HashMap} with sufficient capacity to hold expectedSize mappings.</returns>
    /// <#### cref="HashMap"/>
    public static Dictionary<TK, TNode> NewHashMapWithExpectedSize<TK, TNode>(int expectedSize)
    {
        return new Dictionary<TK, TNode>(CapacityForSize(expectedSize));
    }

    /// <summary>
    /// Returns a <#### cref="LinkedHashMap"/> with an initial capacity that is sufficient to hold
    /// {@code expectedSize} mappings without rehashing its internal backing storage.
    /// <para>
    /// Because {@code LinkedHashMap} extends <#### cref="System.Collections.Hashtable"/> it inherits the issue that the capacity
    /// is not equivalent to the number of mappings it can hold without rehashing. See
    /// <#### cref="newHashMapWithExpectedSize(int)"/> for details.
    /// </para>
    /// </summary>
    /// @param <K> the type of keys in the returned {@code LinkedHashMap} </param>
    /// @param <TNode> the type of values in the returned {@code LinkedHashMap} </param>
    /// <param name="expectedSize"> of mappings that will be put into the returned {@code LinkedHashMap} </param>
    /// <returns>an empty {@code LinkedHashMap} with sufficient capacity to hold expectedSize mappings.</returns>
    /// <#### cref="HashMap"/>
    public static LinkedHashMap<TK, TNode> NewLinkedHashMapWithExpectedSize<TK, TNode>(int expectedSize)
    {
        return new LinkedHashMap<TK, TNode>(CapacityForSize(expectedSize));
    }

    /// <summary>
    /// Returns a <#### cref="System.Collections.Generic.HashSet<object>"/> with an initial capacity that is sufficient to hold
    /// {@code expectedSize} elements without rehashing its internal backing storage.
    /// <para>
    /// Because a {@code HashSet} is backed by a <#### cref="System.Collections.Hashtable"/> it inherits the issue that the
    /// capacity is not equivalent to the number of elements it can hold without rehashing. See
    /// <#### cref="newHashMapWithExpectedSize(int)"/> for details.
    /// </para>
    /// </summary>
    /// @param <TEdge> the type of elements in the returned {@code HashSet} </param>
    /// <param name="expectedSize"> of elements that will be add to the returned {@code HashSet} </param>
    /// <returns>an empty {@code HashSet} with sufficient capacity to hold expectedSize elements.</returns>
    /// <#### cref="HashMap"/>
    public static HashSet<TEdge> NewHashSetWithExpectedSize<TEdge>(int expectedSize)
    {
        return new HashSet<TEdge>(CapacityForSize(expectedSize));
    }

    /// <summary>
    /// Returns a <#### cref="LinkedHashSet"/> with an initial capacity that is sufficient to hold
    /// {@code expectedSize} elements without rehashing its internal backing storage.
    /// <para>
    /// Because a {@code LinkedHashSet} is backed by a <#### cref="System.Collections.Hashtable"/> it inherits the issue that the
    /// capacity is not equivalent to the number of elements it can hold without rehashing. See
    /// <#### cref="newHashMapWithExpectedSize(int)"/> for details.
    /// </para>
    /// </summary>
    /// @param <TEdge> the type of elements in the returned {@code LinkedHashSet} </param>
    /// <param name="expectedSize"> of elements that will be add to the returned {@code LinkedHashSet} </param>
    /// <returns>an empty {@code LinkedHashSet} with sufficient capacity to hold expectedSize elements.</returns>
    /// <#### cref="HashMap"/>
    public static LinkedHashSet<TEdge> NewLinkedHashSetWithExpectedSize<TEdge>(int expectedSize)
    {
        return new LinkedHashSet<TEdge>(CapacityForSize(expectedSize));
    }

    private static int CapacityForSize(int size)
    {
        // consider default load factor 0.75f of (Linked)HashMap
        return (int)(size / 0.75f + 1.0f); // let (Linked)HashMap limit it if it's too large
    }

    /// <summary>
    /// Returns from the given {@code Iterable} the element with the given {@code index}.
    /// <para>
    /// The order to which the index applies is that defined by the <#### cref="Iterable.iterator()"/>.
    /// </para>
    /// </summary>
    /// @param <TEdge> the type of elements in the {@code Iterable} </param>
    /// <param name="iterable"> the Iterable from which the element at {@code index} is returned.</param>
    /// <param name="index"> the index of the returned element.</param>
    /// <returns>the element with {@code index} in the {@code iterable}</returns>
    public static TEdge GetElement<TEdge>(IEnumerable<TEdge> iterable, int index)
    {
        if (iterable is System.Collections.IList)
        {
            return ((IList<TEdge>)iterable)[index];
        }

        IEnumerator<TEdge> it = iterable.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
        for (var i = 0; i < index && it.hasNext(); i++)
        {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            it.next();
        }

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
        if (it.hasNext())
        {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            return it.next();
        }
        else
        {
            throw new IndexOutOfRangeException(index);
        }
    }
}
