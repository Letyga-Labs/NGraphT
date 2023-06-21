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

namespace NGraphT.Core.Graph.Concurrent;

/// <summary>
/// A builder for <see cref="AsSynchronizedGraph{TVertex,TEdge}"/>.
/// </summary>
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: CHEN Kui.</remarks>
public sealed class GraphSynchronizationBuilder<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private bool _isCacheEnabled;
    private bool _isFair;
    private bool _isCopyless;

    /// <summary>
    /// Construct a new Builder with non-fair mode, cache disabled, and copyless mode disabled.
    /// </summary>
    public GraphSynchronizationBuilder()
    {
        _isCacheEnabled = false;
        _isFair         = false;
        _isCopyless     = false;
    }

    /// <summary>
    /// Construct a new Builder matching the settings of an existing graph.
    /// </summary>
    /// <param name="graph"> the graph on which to base the builder.</param>
    public GraphSynchronizationBuilder(AsSynchronizedGraph<TVertex, TEdge> graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        _isCacheEnabled = graph.CacheEnabled;
        _isFair         = graph.Fair;
        _isCopyless     = graph.Copyless;
    }

    /// <summary>
    /// Return whether a cache will be used for the synchronized graph being built.
    /// </summary>
    /// <returns><c>true</c> if cache will be used, <c>false</c> if cache will not be
    ///         used.</returns>
    public bool IsCacheEnabled => _isCacheEnabled;

    /// <summary>
    /// Return whether fair mode will be used for the synchronized graph being built.
    /// </summary>
    /// <returns><c>true</c> if constructed as fair mode, <c>false</c> if non-fair.</returns>
    public bool IsFair => _isFair;

    /// <summary>
    /// Return whether copyless mode will be used for the synchronized graph being built.
    /// </summary>
    /// <returns><c>true</c> if constructed as copyless, <c>false</c> otherwise.</returns>
    public bool IsCopyless => _isCopyless;

    /// <summary>
    /// Request a synchronized graph without caching.
    /// </summary>
    /// <returns>the Builder.</returns>
    public GraphSynchronizationBuilder<TVertex, TEdge> CacheDisable()
    {
        _isCacheEnabled = false;
        return this;
    }

    /// <summary>
    /// Request a synchronized graph with caching.
    /// </summary>
    /// <returns>the Builder.</returns>
    public GraphSynchronizationBuilder<TVertex, TEdge> CacheEnable()
    {
        _isCacheEnabled = true;
        return this;
    }

    /// <summary>
    /// Request a synchronized graph which does not return collection copies.
    /// </summary>
    /// <returns>the Builder.</returns>
    public GraphSynchronizationBuilder<TVertex, TEdge> SetCopyless()
    {
        _isCopyless = true;
        return this;
    }

    /// <summary>
    /// Request a synchronized graph which returns collection copies.
    /// </summary>
    /// <returns>the Builder.</returns>
    public GraphSynchronizationBuilder<TVertex, TEdge> ClearCopyless()
    {
        _isCopyless = false;
        return this;
    }

    /// <summary>
    /// Request a synchronized graph with fair mode.
    /// </summary>
    /// <returns>the SynchronizedGraphParams.</returns>
    public GraphSynchronizationBuilder<TVertex, TEdge> SetFair()
    {
        _isFair = true;
        return this;
    }

    /// <summary>
    /// Request a synchronized graph with non-fair mode.
    /// </summary>
    /// <returns>the SynchronizedGraphParams.</returns>
    public GraphSynchronizationBuilder<TVertex, TEdge> SetNonfair()
    {
        _isFair = false;
        return this;
    }

    /// <summary>
    /// Build the AsSynchronizedGraph.
    /// </summary>
    /// <param name="graph"> the backing graph (the delegate).</param>
    /// <returns>the AsSynchronizedGraph.</returns>
    public AsSynchronizedGraph<TVertex, TEdge> Build(IGraph<TVertex, TEdge> graph)
    {
        return new AsSynchronizedGraph<TVertex, TEdge>(graph, _isCacheEnabled, _isFair, _isCopyless);
    }
}
