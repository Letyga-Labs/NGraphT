/*
 * (C) Copyright 2020-2021, by Dimitrios Michail and Contributors.
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
/// A wrapper around a supplier of an iterable.
/// </summary>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
/// <typeparam name="TEdge"> the element type.</typeparam>
public class LiveIterableWrapper<TEdge> : IEnumerable<TEdge>
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
    public virtual Func<IEnumerable<TEdge>> Supplier { get; }

    public virtual IEnumerator<TEdge> GetEnumerator()
    {
        return Supplier().GetEnumerator();
    }
}
