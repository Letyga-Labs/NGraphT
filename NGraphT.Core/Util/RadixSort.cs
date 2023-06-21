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

using System.Diagnostics.CodeAnalysis;

namespace NGraphT.Core.Util;

/// <summary>
/// Sorts the specified list of integers into ascending order using the Radix Sort method.
/// This algorithms runs in $O(N + TVertex)$ time and uses $O(N + TVertex)$ extra memory, where $V = 256$.
/// If $N \leq RadixSort.CUT\_OFF$ then the standard Java sorting algorithm is used.
/// The specified list must be modifiable, but need not be resizable.
/// </summary>
public static class RadixSort
{
    private const int _maxDigits = 32;
    private const int _maxD      = 4;
    private const int _sizeRadix = 1 << (_maxDigits / _maxD);
    private const int _mAsk      = _sizeRadix - 1;

    private static int[] _count = new int[_sizeRadix];

    public static int CutOff { get; set; } = 40;

    /// <summary>
    /// Sort the given list in ascending order.
    /// </summary>
    /// <param name="list"> the input list of integers.</param>
    [SuppressMessage("Design", "MA0016:Prefer returning collection abstraction instead of implementation")]
    [SuppressMessage("Design", "CA1002:Do not expose generic lists")]
    public static void Sort(List<int>? list)
    {
        if (list == null)
        {
            return;
        }

        var n = list.Count;

        if (n <= CutOff)
        {
            list.Sort();
            return;
        }

        var array = list.ToArray();

        DoRadixSort(array, n, new int[n], _count);

        for (var i = 0; i < list.Count; i++)
        {
            list[i] = array[i];
        }
    }

    private static void DoRadixSort(int[] array, int n, int[] tempArray, int[] cnt)
    {
        for (int d = 0, shift = 0; d < _maxD; d++, shift += _maxDigits / _maxD)
        {
            Array.Fill(cnt, 0);

            for (var i = 0; i < n; ++i)
            {
                ++cnt[(array[i] >> shift) & _mAsk];
            }

            for (var i = 1; i < _sizeRadix; ++i)
            {
                cnt[i] += cnt[i - 1];
            }

            for (var i = n - 1; i >= 0; i--)
            {
                tempArray[--cnt[(array[i] >> shift) & _mAsk]] = array[i];
            }

            Array.Copy(tempArray, 0, array, 0, n);
        }
    }
}
