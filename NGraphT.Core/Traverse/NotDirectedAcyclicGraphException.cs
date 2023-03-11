/*
 * (C) Copyright 2021-2021, by Kaiichiro Ota and Contributors.
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

namespace NGraphT.Core.Traverse;

/// <summary>
/// An exception to signal that <see cref="TopologicalOrderIterator{TNode,TEdge}"/>
/// is used for a non-directed acyclic graph.
/// Note that this class extends <see cref="System.ArgumentException"/> for backward compatibility.
/// </summary>
///
/// <remarks>Author: Kaiichiro Ota.</remarks>
public class NotDirectedAcyclicGraphException : ArgumentException
{
    private const string GraphIsNotADag = "Graph is not a DAG";

    public NotDirectedAcyclicGraphException()
        : base(GraphIsNotADag)
    {
    }
}
