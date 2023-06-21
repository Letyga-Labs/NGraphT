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

namespace NGraphT.Core.Util;

/// <summary>
/// A wrapper around a supplier of an iterable.
/// </summary>
///
/// <typeparam name="TEdge"> the element type.</typeparam>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public sealed class LiveIterableWrapper<TEdge> : IEnumerable<TEdge>
{
    /// <summary>
    /// Create a new wrapper.
    /// </summary>
    /// <param name="supplier"> the supplier which provides the iterable.</param>
    public LiveIterableWrapper(Func<IEnumerable<TEdge>> supplier)
    {
        ArgumentNullException.ThrowIfNull(supplier);
        Supplier = supplier;
    }

    /// <summary>
    /// Get the supplier.
    /// </summary>
    /// <returns>the supplier.</returns>
    public Func<IEnumerable<TEdge>> Supplier { get; }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<TEdge> GetEnumerator()
    {
        return Supplier().GetEnumerator();
    }
}
