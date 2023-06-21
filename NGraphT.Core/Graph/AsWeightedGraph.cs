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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NGraphT.Core.DotNetUtil;

/*
 * (C) Copyright 2018-2021, by Lukas Harzenetter and Contributors.
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
/// Provides a weighted view of a graph. The class stores edge weights internally.
/// All <see cref="GetEdgeWeight"/> calls are handled by this view; all other graph operations are
/// propagated to the graph backing this view.
///
/// <para>
/// This class can be used to make an unweighted graph weighted, to override the weights of a
/// weighted graph, or to provide different weighted views of the same underlying graph. For
/// instance, the edges of a graph representing a road network might have two weights associated with
/// them: a travel time and a travel distance. Instead of creating two weighted graphs of the same
/// network, one would simply create two weighted views of the same underlying graph.
/// </para>
///
/// <para>
/// This class offers two ways to associate a weight with an edge:
/// <list type="bullet">
///     <item>Explicitly through a map which contains a mapping from an edge to a weight</item>
///     <item>Implicitly through a function which computes a weight for a given edge</item>
/// </list>
/// In the first way, the map is used to lookup edge weights. In the second way, a function is
/// provided to calculate the weight of an edge. If the map does not contain a particular edge, or
/// the function does not provide a weight for a particular edge, the <see cref="GetEdgeWeight"/> call is
/// propagated to the backing graph.
/// </para>
///
/// <para>
/// Finally, the view provides a <see cref="SetEdgeWeight"/> method. This method behaves differently
/// depending on how the view is constructed. See <see cref="SetEdgeWeight"/> for details.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
public class AsWeightedGraph<TVertex, TEdge> : GraphDelegator<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private readonly Func<TEdge, double>?       _weightFunction;
    private readonly IDictionary<TEdge, double> _weights;
    private readonly bool                       _writeWeightsThrough;
    private readonly bool                       _cacheWeights;

    /// <summary>
    /// Constructor for AsWeightedGraph where the weights are provided through a map. Invocations of
    /// the <see cref="SetEdgeWeight"/> method will update the map. Moreover, calls to <see cref="SetEdgeWeight"/>
    /// are propagated to the underlying graph.
    /// </summary>
    ///
    /// <param name="graph"> the backing graph over which a weighted view is to be created.</param>
    /// <param name="weights"> the map containing the edge weights.</param>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
    public AsWeightedGraph(IGraph<TVertex, TEdge> graph, IDictionary<TEdge, double> weights)
        : this(graph, weights, graph.Type.IsWeighted)
    {
    }

    /// <summary>
    /// Constructor for AsWeightedGraph which allows weight write propagation to be requested explicitly.
    /// </summary>
    ///
    /// <param name="graph"> the backing graph over which an weighted view is to be created.</param>
    /// <param name="weights"> the map containing the edge weights.</param>
    /// <param name="writeWeightsThrough">
    ///     if set to true, the weights will get propagated to the backing graph in the <c>setEdgeWeight()</c> method.
    /// </param>
    ///
    /// <exception cref="ArgumentException">
    ///     if <c>writeWeightsThrough</c> is set to true and <c>graph</c> is not a weighted graph.
    /// </exception>
    public AsWeightedGraph(IGraph<TVertex, TEdge> graph, IDictionary<TEdge, double> weights, bool writeWeightsThrough)
        : base(graph)
    {
        ArgumentNullException.ThrowIfNull(weights);

        _weights             = weights;
        _weightFunction      = null;
        _cacheWeights        = false;
        _writeWeightsThrough = writeWeightsThrough;

        if (_writeWeightsThrough)
        {
            // TODO: GraphTests.RequireWeighted(graph);
        }
    }

    /// <summary>
    /// Constructor for AsWeightedGraph which uses a weight function to compute edge weights. When
    /// the weight of an edge is queried, the weight function is invoked. If
    /// <c>cacheWeights</c> is set to <c>true</c>, the weight of an edge returned by the
    /// <c>weightFunction</c> after its first invocation is stored in a map. The weight of an
    /// edge returned by subsequent calls to <see cref="GetEdgeWeight"/> for the same edge will then be
    /// retrieved directly from the map, instead of re-invoking the weight function. If
    /// <c>cacheWeights</c> is set to <c>false</c>, each invocation of
    /// the <see cref="GetEdgeWeight"/> method will invoke the weight function. Caching the edge weights is
    /// particularly useful when pre-computing all edge weights is expensive and it is expected that
    /// the weights of only a subset of all edges will be queried.
    /// </summary>
    ///
    /// <param name="graph"> the backing graph over which an weighted view is to be created.</param>
    /// <param name="weightFunction"> function which maps an edge to a weight.</param>
    /// <param name="cacheWeights">
    ///     if set to <c>true</c>, weights are cached once computed by the weight function.
    /// </param>
    /// <param name="writeWeightsThrough">
    ///     if set to <c>true</c>, the weight set directly by the <see cref="SetEdgeWeight"/> method
    /// will be propagated to the backing graph.
    /// </param>
    ///
    /// <exception cref="NullReferenceException"> if the graph or the weight function is null.</exception>
    /// <exception cref="ArgumentException">
    ///     if <c>writeWeightsThrough</c> is set to true and <c>graph</c> is not a weighted graph.
    /// </exception>
    public AsWeightedGraph(
        IGraph<TVertex, TEdge> graph,
        Func<TEdge, double>    weightFunction,
        bool                   cacheWeights,
        bool                   writeWeightsThrough
    )
        : base(graph)
    {
        ArgumentNullException.ThrowIfNull(weightFunction);

        _weightFunction      = weightFunction;
        _cacheWeights        = cacheWeights;
        _writeWeightsThrough = writeWeightsThrough;
        _weights             = new Dictionary<TEdge, double>();

        if (_writeWeightsThrough)
        {
            // TODO: GraphTests.RequireWeighted(graph);
        }
    }

    public override IGraphType Type => base.Type.AsWeighted();

    /// <summary>
    /// Returns the weight assigned to a given edge. If weights are provided through a map, first a
    /// map lookup is performed. If the edge is not found, the <see cref="GetEdgeWeight"/> method of the
    /// underlying graph is invoked instead. If, on the other hand, the weights are provided through
    /// a function, this method will first attempt to lookup the weight of an edge in the cache (that
    /// is, if <c>cacheWeights</c> is set to <c>true</c> in the constructor). If caching
    /// was disabled, or the edge could not be found in the cache, the weight function is invoked. If
    /// the function does not provide a weight for a given edge, the call is again propagated to the
    /// underlying graph.
    /// </summary>
    ///
    /// <param name="edge"> edge of interest.</param>
    ///
    /// <returns>the edge weight.</returns>
    public override double GetEdgeWeight(TEdge edge)
    {
        double? weight;
        if (_weightFunction != null)
        {
            // If weights are cached, check map first before invoking the weight
            if (_cacheWeights)
            {
                // function
                weight = _weights.GetOrCompute(edge, _weightFunction);
            }
            else
            {
                weight = _weightFunction(edge);
            }
        }
        else
        {
            weight = _weights.GetOrDefault(edge);
        }

        if (!weight.HasValue)
        {
            weight = base.GetEdgeWeight(edge);
        }

        return weight.Value;
    }

    /// <summary>
    /// Assigns a weight to an edge. If <c>writeWeightsThrough</c> is set to <c>true</c>,
    /// the same weight is set in the backing graph. If this class was constructed using a weight
    /// function, it only makes sense to invoke this method when <c>cacheWeights</c> is set to
    /// true. This method can then be used to preset weights in the cache, or to overwrite existing
    /// values.
    /// </summary>
    ///
    /// <param name="edge"> edge on which to set weight.</param>
    /// <param name="weight"> new weight for edge.</param>
    public override void SetEdgeWeight(TEdge edge, double weight)
    {
        Debug.Assert(edge != null);

        if (_weightFunction != null && !_cacheWeights)
        {
            throw new NotSupportedException(
                "Cannot set an edge weight when a weight function is used and caching is disabled"
            );
        }

        _weights[edge] = weight;

        if (_writeWeightsThrough)
        {
            Delegate.SetEdgeWeight(edge, weight);
        }
    }
}
