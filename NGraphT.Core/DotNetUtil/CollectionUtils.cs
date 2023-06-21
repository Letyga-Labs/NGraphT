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

using Java2Net = J2N.Collections.Generic;

namespace NGraphT.Core.DotNetUtil;

internal static class CollectionUtils
{
    public static IEnumerable<(T Previous, T Current)> Pairwise<T>(this IEnumerable<T> source)
    {
        var previous = default(T);

        using var it = source.GetEnumerator();
        if (it.MoveNext())
        {
            previous = it.Current;
        }

        while (it.MoveNext())
        {
            var current = it.Current;
            yield return (Previous: previous!, Current: current);
            previous = current;
        }
    }

    public static bool AddRange<T>(this ISet<T> source, IEnumerable<T> items)
    {
        var allAdded = true;
        foreach (var item in items)
        {
            allAdded &= source.Add(item);
        }

        return allAdded;
    }

    public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        return default;
    }

    public static TValue GetOrCompute<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey                           key,
        Func<TKey, TValue>             valueComputer)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        else
        {
            var result = valueComputer(key);
            dictionary[key] = result;
            return result;
        }
    }

    public static TValue GetOrPut<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey                           key,
        TValue                         valueIfAbsent)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        else
        {
            dictionary[key] = valueIfAbsent;
            return valueIfAbsent;
        }
    }

    public static Java2Net.LinkedHashSet<T> ToLinkedHashSet<T>(this IEnumerable<T> source)
    {
        return new Java2Net.LinkedHashSet<T>(source);
    }
}
