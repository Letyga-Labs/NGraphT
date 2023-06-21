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
/// Math Utilities.
/// </summary>
///
/// <remarks>Author: Assaf Lehr.</remarks>
public static class MathUtil
{
    /// <summary>
    /// Calculate the factorial of $n$.
    /// </summary>
    /// <param name="n"> the input number.</param>
    /// <returns>the factorial.</returns>
    public static long Factorial(int n)
    {
        long multi = 1;
        for (var i = 1; i <= n; i++)
        {
            multi *= i;
        }

        return multi;
    }

    /// <summary>
    /// Calculate the floor of the binary logarithm of $n$.
    /// </summary>
    /// <param name="n"> the input number.</param>
    /// <returns>the binary logarithm.</returns>
    public static int Log2(int n)
    {
        // returns 0 for n=0
        var log = 0;
        if ((n & 0xffff0000) != 0)
        {
            n   = (int)((uint)n >> 16);
            log = 16;
        }

        if (n >= 256)
        {
            n   =  (int)((uint)n >> 8);
            log += 8;
        }

        if (n >= 16)
        {
            n   =  (int)((uint)n >> 4);
            log += 4;
        }

        if (n >= 4)
        {
            n   =  (int)((uint)n >> 2);
            log += 2;
        }

        return log + ((int)((uint)n >> 1));
    }
}
