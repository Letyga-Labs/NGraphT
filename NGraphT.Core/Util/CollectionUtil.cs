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
using Java2Net = J2N.Collections.Generic;

namespace NGraphT.Core.Util;

/// <summary>
/// Utility class to create <see cref="System.Collections.ICollection"/> instances.
/// </summary>
///
/// <remarks>Author: Hannes Wellmann.</remarks>
public static class CollectionUtil
{
    /// <summary>
    /// Returns a <see cref="System.Collections.IDictionary"/> with an initial capacity that is sufficient to hold
    /// <c>expectedSize</c> mappings without rehashing its internal backing storage.
    ///
    /// <para>
    /// The returned <c>Dictionary</c> has a capacity that is the specified expected size divided by
    /// the load factor of the Dictionary, which is sufficient to hold <c>expectedSize</c> mappings without
    /// rehashing.
    /// </para>
    /// </summary>
    ///
    /// <typeparam name="TKey">the type of keys in the returned <c>HashMap</c>.</typeparam>
    /// <typeparam name="TValue">the type of values in the returned <c>HashMap</c>.</typeparam>
    ///
    /// <param name="expectedSize">of mappings that will be put into the returned <c>Dictionary</c>.</param>
    /// <returns>an empty <c>Dictionary</c> with sufficient capacity to hold expectedSize mappings.</returns>
    public static IDictionary<TKey, TValue> NewHashMapWithExpectedSize<TKey, TValue>(int expectedSize)
        where TKey : notnull
    {
        return new Dictionary<TKey, TValue>(CapacityForSize(expectedSize));
    }

    /// <summary>
    /// Returns a <see cref="J2N.Collections.Generic.LinkedDictionary{TKey,TValue}"/> with an initial
    /// capacity that is sufficient to hold <c>expectedSize</c> mappings without rehashing its internal backing storage.
    ///
    /// <para>
    /// Because <c>LinkedHashMap</c> extends <see cref="System.Collections.IDictionary"/>
    /// it inherits the issue that the capacity is not equivalent to the number of mappings
    /// it can hold without rehashing. See <see cref="NewHashMapWithExpectedSize{TKey,TVertex}"/> for details.
    /// </para>
    /// </summary>
    ///
    /// <typeparam name="TKey"> the type of keys in the returned <c>Java2Net.LinkedDictionary</c>. </typeparam>
    /// <typeparam name="TValue"> the type of values in the returned <c>Java2Net.LinkedDictionary</c>. </typeparam>
    ///
    /// <param name="expectedSize">
    ///     of mappings that will be put into the returned <c>Java2Net.LinkedDictionary</c>.
    /// </param>
    ///
    /// <returns>
    ///     an empty <c>Java2Net.LinkedDictionary</c> with sufficient capacity to hold expectedSize mappings.
    /// </returns>
    public static IDictionary NewLinkedHashMapWithExpectedSize<TKey, TValue>(int expectedSize)
    {
        return new Java2Net.LinkedDictionary<TKey, TValue>(CapacityForSize(expectedSize));
    }

    /// <summary>
    /// Returns a <see cref="HashSet{T}"/> with an initial capacity that is sufficient to hold
    /// <c>expectedSize</c> elements without rehashing its internal backing storage.
    ///
    /// <para>
    /// Because a <c>HashSet</c> is backed by a <see cref="System.Collections.Hashtable"/> it inherits
    /// the issue that the capacity is not equivalent to the number of elements it can hold without rehashing.
    /// See <see cref="NewHashMapWithExpectedSize{TKey,TVertex}(int)"/> for details.
    /// </para>
    /// </summary>
    ///
    /// <typeparam name="TElement"> the type of elements in the returned <c>HashSet</c>.</typeparam>
    /// <param name="expectedSize"> of elements that will be add to the returned <c>HashSet</c>.</param>
    /// <returns>an empty <c>HashSet</c> with sufficient capacity to hold expectedSize elements.</returns>
    public static ISet<TElement> NewHashSetWithExpectedSize<TElement>(int expectedSize)
    {
        return new HashSet<TElement>(CapacityForSize(expectedSize));
    }

    /// <summary>
    /// Returns a <see cref="J2N.Collections.Generic.LinkedHashSet{T}"/> with an initial capacity that is
    /// sufficient to hold <c>expectedSize</c> elements without rehashing its internal backing storage.
    ///
    /// <para>
    /// Because a <c>LinkedHashSet</c> is backed by a <see cref="System.Collections.Hashtable"/> it inherits
    /// the issue that the capacity is not equivalent to the number of elements it can hold without rehashing.
    /// See <see cref="NewHashMapWithExpectedSize{TKey,TVertex}(int)"/> for details.
    /// </para>
    /// </summary>
    ///
    /// <typeparam name="TElement"> the type of elements in the returned <c>LinkedHashSet</c>.</typeparam>
    /// <param name="expectedSize"> of elements that will be add to the returned <c>LinkedHashSet</c>.</param>
    /// <returns>an empty <c>LinkedHashSet</c> with sufficient capacity to hold expectedSize elements.</returns>
    public static ISet<TElement> NewLinkedHashSetWithExpectedSize<TElement>(int expectedSize)
    {
        return new Java2Net.LinkedHashSet<TElement>(CapacityForSize(expectedSize));
    }

    private static int CapacityForSize(int size)
    {
        // consider default load factor 0.75f of (Linked)HashMap
        return (int)((size / 0.75f) + 1.0f); // let (Linked)HashMap limit it if it's too large
    }
}
