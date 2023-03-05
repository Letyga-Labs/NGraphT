/*
 * (C) Copyright 2006-2021, by John TNode Sichi and Contributors.
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
/// TypeUtil isolates type-unsafety so that code which uses it for legitimate reasons can stay
/// warning-free.
/// </summary>
///
/// <remarks>Author: John TNode. Sichi.</remarks>
public class TypeUtil
{
    /// <summary>
    /// Casts an object to a type.
    /// </summary>
    /// <param name="o"> object to be cast.</param>
    /// <typeparam name="T"> the type of the result.</typeparam>>
    /// <returns>the result of the cast.</returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> T uncheckedCast(Object o)
    public static T UncheckedCast<T>(object o)
    {
        return (T)o;
    }
}
