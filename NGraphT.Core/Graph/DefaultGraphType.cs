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

using System.Diagnostics.CodeAnalysis;

namespace NGraphT.Core.Graph;

/// <summary>
/// Default implementation of the graph type.
///
/// <para>
/// The graph type describes various properties of a graph such as whether it is directed, undirected
/// or mixed, whether it contain self-loops (a self-loop is an edge where the source vertex is the
/// same as the target vertex), whether it contain multiple (parallel) edges (multiple edges which
/// connect the same pair of vertices) and whether it is weighted or not.
/// </para>
///
/// <para>
/// The type of a graph can be queried on runtime using method <see cref="IGraph{TVertex,TEdge}.Type"/>.
/// This way, for example, an algorithm can have different behavior based on whether the input graph is directed or
/// undirected, etc.
/// </para>
/// </summary>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public sealed class DefaultGraphType : IGraphType
{
    private readonly bool _directed;
    private readonly bool _undirected;
    private readonly bool _selfLoops;
    private readonly bool _multipleEdges;
    private readonly bool _weighted;
    private readonly bool _allowsCycles;
    private readonly bool _modifiable;

    private DefaultGraphType(
        bool directed,
        bool undirected,
        bool selfLoops,
        bool multipleEdges,
        bool weighted,
        bool allowsCycles,
        bool modifiable
    )
    {
        _directed      = directed;
        _undirected    = undirected;
        _selfLoops     = selfLoops;
        _multipleEdges = multipleEdges;
        _weighted      = weighted;
        _allowsCycles  = allowsCycles;
        _modifiable    = modifiable;
    }

    public bool IsDirected => _directed && !_undirected;

    public bool IsUndirected => _undirected && !_directed;

    public bool IsMixed => _undirected && _directed;

    public bool IsAllowingMultipleEdges => _multipleEdges;

    public bool IsAllowingSelfLoops => _selfLoops;

    public bool IsWeighted => _weighted;

    public bool IsAllowingCycles => _allowsCycles;

    public bool IsModifiable => _modifiable;

    public bool IsSimple => !IsAllowingMultipleEdges && !IsAllowingSelfLoops;

    public bool IsPseudograph => IsAllowingMultipleEdges && IsAllowingSelfLoops;

    public bool IsMultigraph => IsAllowingMultipleEdges && !IsAllowingSelfLoops;

    /// <summary>
    /// A simple graph type. An undirected graph for which at most one edge connects any two
    /// vertices, and self-loops are not permitted.
    /// </summary>
    /// <returns>a simple graph type.</returns>
    public static DefaultGraphType Simple()
    {
        return new Builder()
            .Undirected()
            .AllowSelfLoops(false)
            .AllowMultipleEdges(false)
            .Weighted(false)
            .Build();
    }

    /// <summary>
    /// A multigraph type. A non-simple undirected graph in which no self-loops are permitted, but
    /// multiple edges between any two vertices are.
    /// </summary>
    /// <returns>a multigraph type.</returns>
    public static DefaultGraphType Multigraph()
    {
        return new Builder()
            .Undirected()
            .AllowSelfLoops(false)
            .AllowMultipleEdges(true)
            .Weighted(false)
            .Build();
    }

    /// <summary>
    /// A pseudograph type. A non-simple undirected graph in which both graph self-loops and multiple
    /// edges are permitted.
    /// </summary>
    /// <returns>a pseudograph type.</returns>
    public static DefaultGraphType Pseudograph()
    {
        return new Builder()
            .Undirected()
            .AllowSelfLoops(true)
            .AllowMultipleEdges(true)
            .Weighted(false)
            .Build();
    }

    /// <summary>
    /// A directed simple graph type. An undirected graph for which at most one edge connects any two
    /// vertices, and self-loops are not permitted.
    /// </summary>
    /// <returns>a directed simple graph type.</returns>
    public static DefaultGraphType DirectedSimple()
    {
        return new Builder()
            .Directed()
            .AllowSelfLoops(false)
            .AllowMultipleEdges(false)
            .Weighted(false)
            .Build();
    }

    /// <summary>
    /// A directed multigraph type. A non-simple undirected graph in which no self-loops are
    /// permitted, but multiple edges between any two vertices are.
    /// </summary>
    /// <returns>a directed multigraph type.</returns>
    public static DefaultGraphType DirectedMultigraph()
    {
        return new Builder()
            .Directed()
            .AllowSelfLoops(false)
            .AllowMultipleEdges(true)
            .Weighted(false)
            .Build();
    }

    /// <summary>
    /// A directed pseudograph type. A non-simple undirected graph in which both graph self-loops and
    /// multiple edges are permitted.
    /// </summary>
    /// <returns>a directed pseudograph type.</returns>
    public static DefaultGraphType DirectedPseudograph()
    {
        return new Builder()
            .Directed()
            .AllowSelfLoops(true)
            .AllowMultipleEdges(true)
            .Weighted(false)
            .Build();
    }

    /// <summary>
    /// A mixed graph type. A graph having a set of undirected and a set of directed edges, which may
    /// contain self-loops and multiple edges are permitted.
    /// </summary>
    /// <returns>a mixed graph type.</returns>
    public static DefaultGraphType Mixed()
    {
        return new Builder()
            .Mixed()
            .AllowSelfLoops(true)
            .AllowMultipleEdges(true)
            .Weighted(false)
            .Build();
    }

    /// <summary>
    /// A directed acyclic graph.
    /// </summary>
    /// <returns>a directed acyclic graph type.</returns>
    public static DefaultGraphType Dag()
    {
        return new Builder()
            .Directed()
            .AllowSelfLoops(false)
            .AllowMultipleEdges(true)
            .AllowCycles(false)
            .Weighted(false)
            .Build();
    }

    public IGraphType AsDirected()
    {
        return new Builder(this).Directed().Build();
    }

    public IGraphType AsUndirected()
    {
        return new Builder(this).Undirected().Build();
    }

    public IGraphType AsMixed()
    {
        return new Builder(this).Mixed().Build();
    }

    public IGraphType AsUnweighted()
    {
        return new Builder(this).Weighted(false).Build();
    }

    public IGraphType AsWeighted()
    {
        return new Builder(this).Weighted(true).Build();
    }

    public IGraphType AsModifiable()
    {
        return new Builder(this).Modifiable(true).Build();
    }

    public IGraphType AsUnmodifiable()
    {
        return new Builder(this).Modifiable(false).Build();
    }

    public override string ToString()
    {
        return
            $"DefaultGraphType [directed={_directed}, undirected={_undirected}, self-loops={_selfLoops}, multiple-edges={_multipleEdges}, weighted={_weighted}, allows-cycles={_allowsCycles}, modifiable={_modifiable}]";
    }

    /// <summary>
    /// A builder for <see cref="DefaultGraphType"/>.
    /// </summary>
    /// <remarks>Author: Dimitrios Michail.</remarks>
    public sealed class Builder
    {
        private bool _directed;
        private bool _undirected;
        private bool _allowSelfLoops;
        private bool _allowMultipleEdges;
        private bool _weighted;
        private bool _allowCycles;
        private bool _modifiable;

        /// <summary>
        /// Construct a new Builder.
        /// </summary>
        public Builder()
        {
            _directed           = false;
            _undirected         = true;
            _allowSelfLoops     = true;
            _allowMultipleEdges = true;
            _weighted           = false;
            _allowCycles        = true;
            _modifiable         = true;
        }

        /// <summary>
        /// Construct a new Builder.
        /// </summary>
        /// <param name="type"> the type to base the builder.</param>
        public Builder(IGraphType type)
        {
            ArgumentNullException.ThrowIfNull(type);

            _directed           = type.IsDirected || type.IsMixed;
            _undirected         = type.IsUndirected || type.IsMixed;
            _allowSelfLoops     = type.IsAllowingSelfLoops;
            _allowMultipleEdges = type.IsAllowingMultipleEdges;
            _weighted           = type.IsWeighted;
            _allowCycles        = type.IsAllowingCycles;
            _modifiable         = type.IsModifiable;
        }

        /// <summary>
        /// Construct a new Builder.
        /// </summary>
        /// <param name="directed"> whether the graph contains directed edges.</param>
        /// <param name="undirected"> whether the graph contains undirected edges.</param>
        public Builder(bool directed, bool undirected)
        {
            if (!directed && !undirected)
            {
                throw new ArgumentException("At least one of directed or undirected must be true", nameof(directed));
            }

            _directed           = directed;
            _undirected         = undirected;
            _allowSelfLoops     = true;
            _allowMultipleEdges = true;
            _weighted           = false;
            _allowCycles        = true;
            _modifiable         = true;
        }

        /// <summary>
        /// Set the type as directed.
        /// </summary>
        /// <returns>the builder.</returns>
        public Builder Directed()
        {
            _directed   = true;
            _undirected = false;
            return this;
        }

        /// <summary>
        /// Set the type as undirected.
        /// </summary>
        /// <returns>the builder.</returns>
        public Builder Undirected()
        {
            _directed   = false;
            _undirected = true;
            return this;
        }

        /// <summary>
        /// Set the type as mixed.
        /// </summary>
        /// <returns>the builder.</returns>
        [SuppressMessage(
            "Critical Code Smell",
            "S3218:Inner class members should not shadow outer class \"static\" or type members"
        )]
        public Builder Mixed()
        {
            _directed   = true;
            _undirected = true;
            return this;
        }

        /// <summary>
        /// Set whether to allow self-loops.
        /// </summary>
        /// <param name="value"> if true self-values are allowed, otherwise not.</param>
        /// <returns>the builder.</returns>
        public Builder AllowSelfLoops(bool value)
        {
            _allowSelfLoops = value;
            return this;
        }

        /// <summary>
        /// Set whether to allow multiple edges.
        /// </summary>
        /// <param name="value"> if true multiple edges are allowed, otherwise not.</param>
        /// <returns>the builder.</returns>
        public Builder AllowMultipleEdges(bool value)
        {
            _allowMultipleEdges = value;
            return this;
        }

        /// <summary>
        /// Set whether the graph will be weighted.
        /// </summary>
        /// <param name="value"> if true the graph will be weighted, otherwise unweighted.</param>
        /// <returns>the builder.</returns>
        public Builder Weighted(bool value)
        {
            _weighted = value;
            return this;
        }

        /// <summary>
        /// Set whether the graph will allow cycles.
        /// </summary>
        /// <param name="value"> if true the graph will allow cycles, otherwise not.</param>
        /// <returns>the builder.</returns>
        public Builder AllowCycles(bool value)
        {
            _allowCycles = value;
            return this;
        }

        /// <summary>
        /// Set whether the graph is modifiable.
        /// </summary>
        /// <param name="value"> if true the graph will be modifiable, otherwise not.</param>
        /// <returns>the builder.</returns>
        public Builder Modifiable(bool value)
        {
            _modifiable = value;
            return this;
        }

        /// <summary>
        /// Build the type.
        /// </summary>
        /// <returns>the type.</returns>
        public DefaultGraphType Build()
        {
            return new DefaultGraphType(
                _directed,
                _undirected,
                _allowSelfLoops,
                _allowMultipleEdges,
                _weighted,
                _allowCycles,
                _modifiable
            );
        }
    }
}
