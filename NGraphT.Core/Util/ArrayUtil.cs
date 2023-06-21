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

namespace NGraphT.Core.Util;

/// <summary>
/// Utility class to simplify handling of arrays.
/// </summary>
///
/// <remarks>Author: Hannes Wellmann.</remarks>
public static class ArrayUtil
{
    /// <summary>
    /// Reverses the order of the elements in the specified range within the given array.
    /// </summary>
    /// <typeparam name="TVertex"> the type of elements in the array.</typeparam>
    /// <param name="arr"> the array.</param>
    /// <param name="from"> the index of the first element (inclusive) inside the range to reverse.</param>
    /// <param name="to"> the index of the last element (inclusive) inside the range to reverse.</param>
    public static void Reverse<TVertex>(TVertex[] arr, int from, int to)
    {
        for (int i = from, j = to; i < j; ++i, --j)
        {
            Swap(arr, i, j);
        }
    }

    /// <summary>
    /// Reverses the order of the elements in the specified range within the given array.
    /// </summary>
    /// <param name="arr"> the array.</param>
    /// <param name="from"> the index of the first element (inclusive) inside the range to reverse.</param>
    /// <param name="to"> the index of the last element (inclusive) inside the range to reverse.</param>
    public static void Reverse(int[] arr, int from, int to)
    {
        ArgumentNullException.ThrowIfNull(arr);
        for (int i = from, j = to; i < j; ++i, --j)
        {
            (arr[j], arr[i]) = (arr[i], arr[j]);
        }
    }

    /// <summary>
    /// Swaps the two elements at the specified indices in the given array.
    /// </summary>
    /// <typeparam name="TVertex"> the type of elements in the array.</typeparam>
    /// <param name="arr"> the array.</param>
    /// <param name="i"> the index of the first element.</param>
    /// <param name="j"> the index of the second element.</param>
    public static void Swap<TVertex>(TVertex[] arr, int i, int j)
    {
        ArgumentNullException.ThrowIfNull(arr);
        (arr[j], arr[i]) = (arr[i], arr[j]);
    }
}
