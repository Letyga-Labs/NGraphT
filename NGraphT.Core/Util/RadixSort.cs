/*
 * (C) Copyright 2018-2021, by Alexandru Valeanu and Contributors.
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
/// Sorts the specified list of integers into ascending order using the Radix Sort method.
/// 
/// This algorithms runs in $O(N + TNode)$ time and uses $O(N + TNode)$ extra memory, where $V = 256$.
/// 
/// If $N \leq RadixSort.CUT\_OFF$ then the standard Java sorting algorithm is used.
/// 
/// The specified list must be modifiable, but need not be resizable.
/// </summary>
public class RadixSort
{
    /// @deprecated use <#### cref="setCutOff(int)"/> instead 
    [
    Obsolete
    (
    "use <#### cref=\"setCutOff(int)\"/> instead"
    )
    ]
    (since = "1.5.2", forRemoval = true) public static int CutOff = 40; // @CS.suppress[StaticVariableName]
    // TODO: make this static field private, rename it to "cutOff" to comply
    // with checkstyle naming rules and remove the // @CS.supress comment and in jgrapht_checks.xml
    // the rule SuppressWithNearbyCommentFilter

    public static void SetCutOff(int cutOff)
    {
        CutOff = cutOff;
    }

    private static final int _maxDigits = 32;
    private static final int _maxD      = 4;
    private static final int _sizeRadix = 1 << (_maxDigits / _maxD);
    private static final int _mAsk      = _sizeRadix - 1;

    private static int[] _count = new int[_sizeRadix];

    // Suppresses default constructor, ensuring non-instantiability.
    private RadixSort()
    {
    }

    private static void RadixSort(int array[], int _n, int _tempArray[], int _cnt[])
    {
        for (int d = 0, shift = 0; d < _maxD; d++, shift += (_maxDigits / _maxD))
        {
            Arrays.Fill(_cnt, 0);

            for (var i = 0; i < _n; ++i)
            {
                ++_cnt[(array[i] >> shift) & _mAsk];
            }

            for (var i = 1; i < _sizeRadix; ++i)
            {
                _cnt[i] += _cnt[i - 1];
            }

            for (var i = _n - 1; i >= 0; i--)
            {
                _tempArray[--_cnt[(array[i] >> shift) & _mAsk]] = array[i];
            }

            Array.Copy(_tempArray, 0, array, 0, _n);
        }
    }

    /// <summary>
    /// Sort the given list in ascending order.
    /// </summary>
    /// <param name="list"> the input list of integers.</param>
    public static void Sort(IList<int> list)
    {
        if (list == null)
        {
            return;
        }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int n = list.size();
        int n = list.size();

        if (n <= CutOff)
        {
            list.sort(null);
            return;
        }

        var array = new int[n];

//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
        var listIterator = list.GetEnumerator();

        while (listIterator.MoveNext())
        {
            array[listIterator.nextIndex()] = listIterator.Current;
        }

        RadixSort(array, n, new int[n], _count);

//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
        listIterator = list.GetEnumerator();

        while (listIterator.MoveNext())
        {
            listIterator.Current;
            listIterator.set(array[listIterator.previousIndex()]);
        }
    }
}
