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

using System.Globalization;

namespace NGraphT.Core.Util;

/// <summary>
/// Helper class for suppliers.
/// </summary>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public static class SupplierUtil
{
    public static Func<TEdge> CreateSupplier<TEdge>()
        where TEdge : new()
    {
        return () => new TEdge();
    }

    /// <summary>
    /// Create a supplier from a class which calls the default constructor.
    /// </summary>
    /// <returns>the supplier.</returns>
    /// <typeparam name="TValue"> the type of results supplied by this supplier.</typeparam>
    public static Func<TValue> CreateSupplierByReflection<TValue>()
    {
        return Activator.CreateInstance<TValue>;
    }

    /// <summary>
    /// Create an integer supplier which returns a sequence starting from zero.
    /// </summary>
    /// <returns>an integer supplier.</returns>
    public static Func<int> CreateIntegerSupplier()
    {
        return CreateIntegerSupplier(0);
    }

    /// <summary>
    /// Create an integer supplier which returns a sequence starting from a specific numbers.
    /// </summary>
    /// <param name="start"> where to start the sequence.</param>
    /// <returns>an integer supplier.</returns>
    public static Func<int> CreateIntegerSupplier(int start)
    {
        var modifiableInt = new[] { start }; // like a modifiable int
        return () => modifiableInt[0]++;
    }

    /// <summary>
    /// Create a long supplier which returns a sequence starting from zero.
    /// </summary>
    /// <returns>a long supplier.</returns>
    public static Func<long> CreateLongSupplier()
    {
        return CreateLongSupplier(0);
    }

    /// <summary>
    /// Create a long supplier which returns a sequence starting from a specific numbers.
    /// </summary>
    /// <param name="start"> where to start the sequence.</param>
    /// <returns>a long supplier.</returns>
    public static Func<long> CreateLongSupplier(long start)
    {
        var modifiableLong = new[] { start }; // like a modifiable long
        return () => modifiableLong[0]++;
    }

    /// <summary>
    /// Create a string supplier which returns unique strings. The returns strings are simply
    /// integers starting from zero.
    /// </summary>
    /// <returns>a string supplier.</returns>
    public static Func<string> CreateStringSupplier()
    {
        return CreateStringSupplier(0);
    }

    /// <summary>
    /// Create a string supplier which returns unique strings. The returns strings are simply
    /// integers starting from start.
    /// </summary>
    /// <param name="start"> where to start the sequence.</param>
    /// <returns>a string supplier.</returns>
    public static Func<string> CreateStringSupplier(int start)
    {
        var container = new[] { start };
        return () => (container[0]++).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Create a string supplier which returns random GUIDs.
    /// </summary>
    /// <returns>a string supplier.</returns>
    public static Func<string> CreateRandomGuidStringSupplier()
    {
        return () => Guid.NewGuid().ToString();
    }
}
