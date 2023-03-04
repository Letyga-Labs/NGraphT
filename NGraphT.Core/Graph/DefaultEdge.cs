/*
 * (C) Copyright 2003-2021, by Barak Naveh and Contributors.
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
/// A default implementation for edges in a <seealso cref="Graph"/>.
///
/// <remarks>Author: Barak Naveh.</remarks>
/// </summary>
public class DefaultEdge : IntrusiveEdge
{
    /// <summary>
    /// Retrieves the source of this edge. This is protected, for use by subclasses only (TEdge.g. for
    /// implementing toString).
    /// </summary>
    /// <returns>source of this edge.</returns>
    protected internal virtual object Source
    {
        get
        {
            return base.Source;
        }
    }

    /// <summary>
    /// Retrieves the target of this edge. This is protected, for use by subclasses only (TEdge.g. for
    /// implementing toString).
    /// </summary>
    /// <returns>target of this edge.</returns>
    protected internal virtual object Target
    {
        get
        {
            return base.Target;
        }
    }

    public override string ToString()
    {
        return "(" + base.Source + " : " + base.Target + ")";
    }
}
