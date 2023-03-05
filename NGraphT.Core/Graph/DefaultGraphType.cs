/*
 * (C) Copyright 2017-2021, by Dimitrios Michail and Contributors.
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
/// Default implementation of the graph type.
///
/// <para>
/// The graph type describes various properties of a graph such as whether it is directed, undirected
/// or mixed, whether it contain self-loops (a self-loop is an edge where the source vertex is the
/// same as the target vertex), whether it contain multiple (parallel) edges (multiple edges which
/// connect the same pair of vertices) and whether it is weighted or not.
/// </para>
/// <para>
/// The type of a graph can be queried on runtime using method <see cref="Graph.getType()"/>. This way, for
/// example, an algorithm can have different behavior based on whether the input graph is directed or
/// undirected, etc.
///
/// <remarks>Author: Dimitrios Michail.</remarks>
/// </para>
/// </summary>
public class DefaultGraphType : IGraphType
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

    public virtual bool Directed
    {
        get
        {
            return _directed && !_undirected;
        }
    }

    public virtual bool Undirected
    {
        get
        {
            return _undirected && !_directed;
        }
    }

    public virtual bool Mixed
    {
        get
        {
            return _undirected && _directed;
        }
    }

    public virtual bool AllowingMultipleEdges
    {
        get
        {
            return _multipleEdges;
        }
    }

    public virtual bool AllowingSelfLoops
    {
        get
        {
            return _selfLoops;
        }
    }

    public virtual bool Weighted
    {
        get
        {
            return _weighted;
        }
    }

    public virtual bool AllowingCycles
    {
        get
        {
            return _allowsCycles;
        }
    }

    public virtual bool Modifiable
    {
        get
        {
            return _modifiable;
        }
    }

    public virtual bool Simple
    {
        get
        {
            return !AllowingMultipleEdges && !AllowingSelfLoops;
        }
    }

    public virtual bool Pseudograph
    {
        get
        {
            return AllowingMultipleEdges && AllowingSelfLoops;
        }
    }

    public virtual bool Multigraph
    {
        get
        {
            return AllowingMultipleEdges && !AllowingSelfLoops;
        }
    }

    public virtual IGraphType AsDirected()
    {
        return (new Builder(this)).Directed().Build();
    }

    public virtual IGraphType AsUndirected()
    {
        return (new Builder(this)).Undirected().Build();
    }

    public virtual IGraphType AsMixed()
    {
        return (new Builder(this)).Mixed().Build();
    }

    public virtual IGraphType AsUnweighted()
    {
        return (new Builder(this)).Weighted(false).Build();
    }

    public virtual IGraphType AsWeighted()
    {
        return (new Builder(this)).Weighted(true).Build();
    }

    public virtual IGraphType AsModifiable()
    {
        return (new Builder(this)).Modifiable(true).Build();
    }

    public virtual IGraphType AsUnmodifiable()
    {
        return (new Builder(this)).Modifiable(false).Build();
    }

    /// <summary>
    /// A simple graph type. An undirected graph for which at most one edge connects any two
    /// vertices, and self-loops are not permitted.
    /// </summary>
    /// <returns>a simple graph type.</returns>
    public static DefaultGraphType Simple()
    {
        return (new Builder()).Undirected().AllowSelfLoops(false).AllowMultipleEdges(false).Weighted(false).Build();
    }

    /// <summary>
    /// A multigraph type. A non-simple undirected graph in which no self-loops are permitted, but
    /// multiple edges between any two vertices are.
    /// </summary>
    /// <returns>a multigraph type.</returns>
    public static DefaultGraphType Multigraph()
    {
        return (new Builder()).Undirected().AllowSelfLoops(false).AllowMultipleEdges(true).Weighted(false).Build();
    }

    /// <summary>
    /// A pseudograph type. A non-simple undirected graph in which both graph self-loops and multiple
    /// edges are permitted.
    /// </summary>
    /// <returns>a pseudograph type.</returns>
    public static DefaultGraphType Pseudograph()
    {
        return (new Builder()).Undirected().AllowSelfLoops(true).AllowMultipleEdges(true).Weighted(false).Build();
    }

    /// <summary>
    /// A directed simple graph type. An undirected graph for which at most one edge connects any two
    /// vertices, and self-loops are not permitted.
    /// </summary>
    /// <returns>a directed simple graph type.</returns>
    public static DefaultGraphType DirectedSimple()
    {
        return (new Builder()).Directed().AllowSelfLoops(false).AllowMultipleEdges(false).Weighted(false).Build();
    }

    /// <summary>
    /// A directed multigraph type. A non-simple undirected graph in which no self-loops are
    /// permitted, but multiple edges between any two vertices are.
    /// </summary>
    /// <returns>a directed multigraph type.</returns>
    public static DefaultGraphType DirectedMultigraph()
    {
        return (new Builder()).Directed().AllowSelfLoops(false).AllowMultipleEdges(true).Weighted(false).Build();
    }

    /// <summary>
    /// A directed pseudograph type. A non-simple undirected graph in which both graph self-loops and
    /// multiple edges are permitted.
    /// </summary>
    /// <returns>a directed pseudograph type.</returns>
    public static DefaultGraphType DirectedPseudograph()
    {
        return (new Builder()).Directed().AllowSelfLoops(true).AllowMultipleEdges(true).Weighted(false).Build();
    }

    /// <summary>
    /// A mixed graph type. A graph having a set of undirected and a set of directed edges, which may
    /// contain self-loops and multiple edges are permitted.
    /// </summary>
    /// <returns>a mixed graph type.</returns>
    public static DefaultGraphType Mixed()
    {
        return (new Builder()).Mixed().AllowSelfLoops(true).AllowMultipleEdges(true).Weighted(false).Build();
    }

    /// <summary>
    /// A directed acyclic graph.
    /// </summary>
    /// <returns>a directed acyclic graph type.</returns>
    public static DefaultGraphType Dag()
    {
        return (new Builder()).Directed().AllowSelfLoops(false).AllowMultipleEdges(true).AllowCycles(false)
            .Weighted(false).Build();
    }

    public override string ToString()
    {
        return "DefaultGraphType [directed=" + _directed + ", undirected=" + _undirected + ", self-loops=" +
               _selfLoops + ", multiple-edges=" + _multipleEdges + ", weighted=" + _weighted + ", allows-cycles=" +
               _allowsCycles + ", modifiable=" + _modifiable + "]";
    }

    /// <summary>
    /// A builder for <see cref="DefaultGraphType"/>.
    ///
    /// <remarks>Author: Dimitrios Michail.</remarks>
    /// </summary>
    public class Builder
    {
        internal bool Directed;
        internal bool Undirected;
        internal bool AllowSelfLoops;
        internal bool AllowMultipleEdges;
        internal bool Weighted;
        internal bool AllowCycles;
        internal bool Modifiable;

        /// <summary>
        /// Construct a new Builder.
        /// </summary>
        public Builder()
        {
            this.directed           = false;
            this.undirected         = true;
            this.allowSelfLoops     = true;
            this.allowMultipleEdges = true;
            this.weighted           = false;
            this.allowCycles        = true;
            this.modifiable         = true;
        }

        /// <summary>
        /// Construct a new Builder.
        /// </summary>
        /// <param name="type"> the type to base the builder.</param>
        public Builder(IGraphType type)
        {
            this.directed           = type.Directed   || type.Mixed;
            this.undirected         = type.Undirected || type.Mixed;
            this.allowSelfLoops     = type.AllowingSelfLoops;
            this.allowMultipleEdges = type.AllowingMultipleEdges;
            this.weighted           = type.Weighted;
            this.allowCycles        = type.AllowingCycles;
            this.modifiable         = type.Modifiable;
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
                throw new ArgumentException("At least one of directed or undirected must be true");
            }

            this.directed           = directed;
            this.undirected         = undirected;
            this.allowSelfLoops     = true;
            this.allowMultipleEdges = true;
            this.weighted           = false;
            this.allowCycles        = true;
            this.modifiable         = true;
        }

        /// <summary>
        /// Set the type as directed.
        /// </summary>
        /// <returns>the builder.</returns>
        public virtual Builder Directed()
        {
            this.directed   = true;
            this.undirected = false;
            return this;
        }

        /// <summary>
        /// Set the type as undirected.
        /// </summary>
        /// <returns>the builder.</returns>
        public virtual Builder Undirected()
        {
            this.directed   = false;
            this.undirected = true;
            return this;
        }

        /// <summary>
        /// Set the type as mixed.
        /// </summary>
        /// <returns>the builder.</returns>
        public virtual Builder Mixed()
        {
            this.directed   = true;
            this.undirected = true;
            return this;
        }

        /// <summary>
        /// Set whether to allow self-loops.
        /// </summary>
        /// <param name="value"> if true self-values are allowed, otherwise not.</param>
        /// <returns>the builder.</returns>
        public virtual Builder AllowSelfLoops(bool value)
        {
            this.allowSelfLoops = value;
            return this;
        }

        /// <summary>
        /// Set whether to allow multiple edges.
        /// </summary>
        /// <param name="value"> if true multiple edges are allowed, otherwise not.</param>
        /// <returns>the builder.</returns>
        public virtual Builder AllowMultipleEdges(bool value)
        {
            this.allowMultipleEdges = value;
            return this;
        }

        /// <summary>
        /// Set whether the graph will be weighted.
        /// </summary>
        /// <param name="value"> if true the graph will be weighted, otherwise unweighted.</param>
        /// <returns>the builder.</returns>
        public virtual Builder Weighted(bool value)
        {
            this.weighted = value;
            return this;
        }

        /// <summary>
        /// Set whether the graph will allow cycles.
        /// </summary>
        /// <param name="value"> if true the graph will allow cycles, otherwise not.</param>
        /// <returns>the builder.</returns>
        public virtual Builder AllowCycles(bool value)
        {
            this.allowCycles = value;
            return this;
        }

        /// <summary>
        /// Set whether the graph is modifiable.
        /// </summary>
        /// <param name="value"> if true the graph will be modifiable, otherwise not.</param>
        /// <returns>the builder.</returns>
        public virtual Builder Modifiable(bool value)
        {
            this.modifiable = value;
            return this;
        }

        /// <summary>
        /// Build the type.
        /// </summary>
        /// <returns>the type.</returns>
        public virtual DefaultGraphType Build()
        {
            return new DefaultGraphType(directed,
                undirected,
                allowSelfLoops,
                allowMultipleEdges,
                weighted,
                allowCycles,
                modifiable
            );
        }
    }
}
