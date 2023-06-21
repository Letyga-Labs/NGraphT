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

namespace NGraphT.Core.Graph;

/// <summary>
/// ParanoidGraph provides a way to verify that objects added to a graph obey the standard
/// equals/hashCode contract. It can be used to wrap an underlying graph to be verified. Note that
/// the verification is very expensive, so ParanoidGraph should only be used during debugging.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: John Sichi.</remarks>
public sealed class ParanoidGraph<TVertex, TEdge> : GraphDelegator<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Create a new paranoid graph.
    /// </summary>
    /// <param name="g"> the underlying wrapped graph.</param>
    public ParanoidGraph(IGraph<TVertex, TEdge> g)
        : base(g)
    {
    }

    /// <inheritdoc/>
    public override bool AddEdge(TVertex sourceVertex, TVertex targetVertex, TEdge edge)
    {
        VerifyAdd(EdgeSet(), edge);
        return base.AddEdge(sourceVertex, targetVertex, edge);
    }

    /// <inheritdoc/>
    public override bool AddVertex(TVertex vertex)
    {
        VerifyAdd(VertexSet(), vertex);
        return base.AddVertex(vertex);
    }

    private static void VerifyAdd<T>(ISet<T> set, T t)
        where T : notnull
    {
        foreach (var o in set)
        {
            if (ReferenceEquals(o, t))
            {
                continue;
            }

            if (Equals(o, t) && o.GetHashCode() != t.GetHashCode())
            {
                throw new ArgumentException(
                    $"ParanoidGraph detected objects o1 (hashCode={o.GetHashCode()}) and o2 (hashCode={t.GetHashCode()}) where o1.equals(o2) but o1.hashCode() != o2.hashCode()",
                    nameof(t)
                );
            }
        }
    }
}
