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
/// A default implementation for edges in a weighted graph. All access to the weight of an edge must
/// go through the graph interface, which is why this class doesn't expose any public methods.
/// </summary>
///
/// <remarks>Author: John TNode. Sichi.</remarks>
public class DefaultWeightedEdge : IntrusiveWeightedEdge
{
    ///<summary>
    ///Retrieves the source of this edge. This is protected, for use by subclasses only (TEdge.g. for
    ///implementing toString).
    ///</summary>
    ///<returns>source of this edge.</returns>
    protected internal virtual object Source
    {
        get
        {
            return ((IntrusiveEdge)this).Source;
        }
    }

    ///<summary>
    ///Retrieves the target of this edge. This is protected, for use by subclasses only (TEdge.g. for
    ///implementing toString).
    ///</summary>
    ///<returns>target of this edge.</returns>
    protected internal virtual object Target
    {
        get
        {
            return ((IntrusiveEdge)this).Target;
        }
    }

    ///<summary>
    ///Retrieves the weight of this edge. This is protected, for use by subclasses only (TEdge.g. for
    ///implementing toString).
    ///</summary>
    ///<returns>weight of this edge.</returns>
    protected internal virtual double Weight
    {
        get
        {
            return base.Weight;
        }
    }

    public override string ToString()
    {
        return "(" + ((IntrusiveEdge)this).Source + " : " + ((IntrusiveEdge)this).Target + ")";
    }
}
