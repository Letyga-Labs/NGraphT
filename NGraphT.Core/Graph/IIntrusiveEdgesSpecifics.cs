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
/// An interface for the set of intrusive edges of a graph.
///
/// <para>
/// Since the library supports edges which can be any user defined object, we need to provide
/// explicit support for storing vertex source, target and weight.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public interface IIntrusiveEdgesSpecifics<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    /// <summary>
    /// Get the edge set.
    /// </summary>
    /// <returns>the edge set.</returns>
    ISet<TEdge> EdgeSet { get; }

    /// <summary>
    /// Get the source vertex of an edge.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <returns>the source vertex.</returns>
    TVertex GetEdgeSource(TEdge edge);

    /// <summary>
    /// Get the target vertex of an edge.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <returns>the target vertex.</returns>
    TVertex GetEdgeTarget(TEdge edge);

    /// <summary>
    /// Add a new edge.
    /// </summary>
    /// <param name="edge"> the edge to add.</param>
    /// <param name="sourceVertex"> the source vertex.</param>
    /// <param name="targetVertex"> the target vertex.</param>
    /// <returns>true if the edge was added, false if the edge was already present.</returns>
    bool Add(TEdge edge, TVertex sourceVertex, TVertex targetVertex);

    /// <summary>
    /// Check if an edge exists.
    /// </summary>
    /// <param name="edge"> the input edge.</param>
    /// <returns>true if an edge exists, false otherwise.</returns>
    bool ContainsEdge(TEdge edge);

    /// <summary>
    /// Remove an edge.
    /// </summary>
    /// <param name="edge"> the edge to remove.</param>
    void Remove(TEdge edge);

    /// <summary>
    /// Get the weight of an edge.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <returns>the edge weight.</returns>
    double GetEdgeWeight(TEdge edge);

    /// <summary>
    /// Set the edge weight.
    /// </summary>
    /// <param name="edge"> the edge.</param>
    /// <param name="weight"> the new weight.</param>
    void SetEdgeWeight(TEdge edge, double weight);
}
