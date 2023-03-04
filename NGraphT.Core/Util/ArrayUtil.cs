/*
 * (C) Copyright 2021-2021, by Hannes Wellmann and Contributors.
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
/// Utility class to simplify handling of arrays.
///
/// <remarks>Author: Hannes Wellmann.</remarks>
/// 
/// </summary>
public class ArrayUtil
{
    private ArrayUtil()
    {
        // static use only
    }

    /// <summary>
    /// Reverses the order of the elements in the specified range within the given array.
    /// </summary>
    /// @param <TNode> the type of elements in the array.</param>
    /// <param name="arr"> the array.</param>
    /// <param name="from"> the index of the first element (inclusive) inside the range to reverse.</param>
    /// <param name="to"> the index of the last element (inclusive) inside the range to reverse.</param>
    public static void Reverse<TNode>(TNode[] arr, int from, int to)
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
        for (int i = from, j = to; i < j; ++i, --j)
        {
            var tmp = arr[j];
            arr[j] = arr[i];
            arr[i] = tmp;
        }
    }

    /// <summary>
    /// Swaps the two elements at the specified indices in the given array.
    /// </summary>
    /// @param <TNode> the type of elements in the array.</param>
    /// <param name="arr"> the array.</param>
    /// <param name="i"> the index of the first element.</param>
    /// <param name="j"> the index of the second element.</param>
    public static void Swap<TNode>(TNode[] arr, int i, int j)
    {
        var tmp = arr[j];
        arr[j] = arr[i];
        arr[i] = tmp;
    }
}
