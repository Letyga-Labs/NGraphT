/*
 * (C) Copyright 2007-2021, by John TNode Sichi and Contributors.
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

using Core;

/// <summary>
/// ParanoidGraph provides a way to verify that objects added to a graph obey the standard
/// equals/hashCode contract. It can be used to wrap an underlying graph to be verified. Note that
/// the verification is very expensive, so ParanoidGraph should only be used during debugging.
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: John Sichi.</remarks>
public class ParanoidGraph<TNode, TEdge> : GraphDelegator<TNode, TEdge>
{
    /// <summary>
    /// Create a new paranoid graph.
    /// </summary>
    /// <param name="g"> the underlying wrapped graph.</param>
    public ParanoidGraph(IGraph<TNode, TEdge> g)
        : base(g)
    {
    }

    /// <inheritdoc/>
    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        VerifyAdd(EdgeSet(), edge);
        return base.AddEdge(sourceVertex, targetVertex, edge);
    }

    /// <inheritdoc/>
    public override bool AddVertex(TNode node)
    {
        VerifyAdd(VertexSet(), node);
        return base.AddVertex(node);
    }

    private static void VerifyAdd<T>(ISet<T> set, T t)
    {
        foreach (var o in set)
        {
            if (o == t)
            {
                continue;
            }

            if (o.Equals(t) && (o.GetHashCode() != t.GetHashCode()))
            {
                throw new ArgumentException("ParanoidGraph detected objects " + "o1 (hashCode=" +
                                                   o.GetHashCode() + ") and o2 (hashCode=" + t.GetHashCode() +
                                                   ") where o1.equals(o2) " + "but o1.hashCode() != o2.hashCode()"
                );
            }
        }
    }
}
