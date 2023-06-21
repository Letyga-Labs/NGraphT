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

namespace NGraphT.Core;

/// <summary>
/// A graph type.
///
/// <para>
/// The graph type describes various properties of a graph such as whether it is directed, undirected
/// or mixed, whether it contain self-loops (a self-loop is an edge where the source vertex is the
/// same as the target vertex), whether it contain multiple (parallel) edges (multiple edges which
/// connect the same pair of vertices) and whether it is weighted or not.
/// </para>
///
/// <para>
/// The type of a graph can be queried on runtime using method <see cref="IGraph{TVertex,TEdge}.Type"/>. This way, for
/// example, an algorithm can have different behavior based on whether the input graph is directed or
/// undirected, etc.
/// </para>
/// </summary>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public interface IGraphType
{
    /// <summary>
    /// Returns true if all edges of the graph are directed, false otherwise.
    /// </summary>
    /// <returns>true if all edges of the graph are directed, false otherwise.</returns>
    bool IsDirected { get; }

    /// <summary>
    /// Returns true if all edges of the graph are undirected, false otherwise.
    /// </summary>
    /// <returns>true if all edges of the graph are undirected, false otherwise.</returns>
    bool IsUndirected { get; }

    /// <summary>
    /// Returns true if the graph contain both directed and undirected edges, false otherwise.
    /// </summary>
    /// <returns>true if the graph contain both directed and undirected edges, false otherwise.</returns>
    bool IsMixed { get; }

    /// <summary>
    /// Returns <c>true</c> if and only if multiple (parallel) edges are allowed in this graph.
    /// The meaning of multiple edges is that there can be many edges going from vertex v1 to vertex v2.
    /// </summary>
    /// <returns><c>true</c> if and only if multiple (parallel) edges are allowed.</returns>
    bool IsAllowingMultipleEdges { get; }

    /// <summary>
    /// Returns <c>true</c> if and only if self-loops are allowed in this graph. A self loop is
    /// an edge that its source and target vertices are the same.
    /// </summary>
    /// <returns><c>true</c> if and only if graph self-loops are allowed.</returns>
    bool IsAllowingSelfLoops { get; }

    /// <summary>
    /// Returns <c>true</c> if and only if cycles are allowed in this graph.
    /// </summary>
    /// <returns><c>true</c> if and only if graph cycles are allowed.</returns>
    bool IsAllowingCycles { get; }

    /// <summary>
    /// Returns <c>true</c> if and only if the graph supports edge weights.
    /// </summary>
    /// <returns><c>true</c> if the graph supports edge weights, <c>false</c> otherwise.</returns>
    bool IsWeighted { get; }

    /// <summary>
    /// Returns <c>true</c> if the graph is simple, <c>false</c> otherwise.
    /// </summary>
    /// <returns><c>true</c> if the graph is simple, <c>false</c> otherwise.</returns>
    bool IsSimple { get; }

    /// <summary>
    /// Returns <c>true</c> if the graph is a pseudograph, <c>false</c> otherwise.
    /// </summary>
    /// <returns><c>true</c> if the graph is a pseudograph, <c>false</c> otherwise.</returns>
    bool IsPseudograph { get; }

    /// <summary>
    /// Returns <c>true</c> if the graph is a multigraph, <c>false</c> otherwise.
    /// </summary>
    /// <returns><c>true</c> if the graph is a multigraph, <c>false</c> otherwise.</returns>
    bool IsMultigraph { get; }

    /// <summary>
    /// Returns <c>true</c> if the graph is modifiable, <c>false</c> otherwise.
    /// </summary>
    /// <returns><c>true</c> if the graph is modifiable, <c>false</c> otherwise.</returns>
    bool IsModifiable { get; }

    /// <summary>
    /// Create a directed variant of the current graph type.
    /// </summary>
    /// <returns>a directed variant of the current graph type.</returns>
    IGraphType AsDirected();

    /// <summary>
    /// Create an undirected variant of the current graph type.
    /// </summary>
    /// <returns>an undirected variant of the current graph type.</returns>
    IGraphType AsUndirected();

    /// <summary>
    /// Create a mixed variant of the current graph type.
    /// </summary>
    /// <returns>a mixed variant of the current graph type.</returns>
    IGraphType AsMixed();

    /// <summary>
    /// Create an unweighted variant of the current graph type.
    /// </summary>
    /// <returns>an unweighted variant of the current graph type.</returns>
    IGraphType AsUnweighted();

    /// <summary>
    /// Create a weighted variant of the current graph type.
    /// </summary>
    /// <returns>a weighted variant of the current graph type.</returns>
    IGraphType AsWeighted();

    /// <summary>
    /// Create a modifiable variant of the current graph type.
    /// </summary>
    /// <returns>a modifiable variant of the current graph type.</returns>
    IGraphType AsModifiable();

    /// <summary>
    /// Create an unmodifiable variant of the current graph type.
    /// </summary>
    /// <returns>a unmodifiable variant of the current graph type.</returns>
    IGraphType AsUnmodifiable();
}
