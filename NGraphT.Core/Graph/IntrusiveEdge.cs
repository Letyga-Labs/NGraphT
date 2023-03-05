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

namespace NGraphT.Core.Graph;

/// <summary>
/// IntrusiveEdge encapsulates the internals for the default edge implementation. It is not intended
/// to be referenced directly (which is why it's not public); use DefaultEdge for that.
/// </summary>
///
/// <remarks>Author: John TNode. Sichi.</remarks>
internal class IntrusiveEdge : ICloneable
{
    internal object Source;

    internal object Target;

    /// <seealso cref="Object.clone()"/>
    public override object Clone()
    {
        try
        {
            return base.clone();
        }
        catch (CloneNotSupportedException)
        {
            // shouldn't happen as we are Cloneable
            throw new InternalError();
        }
    }
}
