using System.Diagnostics;

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

using Core;

/// <summary>
/// Provides a weighted view of a graph. The class stores edge weights internally.
/// All @link{getEdgeWeight} calls are handled by this view; all other graph operations are
/// propagated to the graph backing this view.
///
/// <para>
/// This class can be used to make an unweighted graph weighted, to override the weights of a
/// weighted graph, or to provide different weighted views of the same underlying graph. For
/// instance, the edges of a graph representing a road network might have two weights associated with
/// them: a travel time and a travel distance. Instead of creating two weighted graphs of the same
/// network, one would simply create two weighted views of the same underlying graph.
/// </para>
/// <para>
/// This class offers two ways to associate a weight with an edge:
/// <ol>
/// <li>Explicitly through a map which contains a mapping from an edge to a weight</li>
/// <li>Implicitly through a function which computes a weight for a given edge</li>
/// </ol>
/// In the first way, the map is used to lookup edge weights. In the second way, a function is
/// provided to calculate the weight of an edge. If the map does not contain a particular edge, or
/// the function does not provide a weight for a particular edge, the @link{getEdgeWeight} call is
/// propagated to the backing graph.
/// 
/// Finally, the view provides a @link{setEdgeWeight} method. This method behaves differently
/// depending on how the view is constructed. See @link{setEdgeWeight} for details.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
public class AsWeightedGraph<TNode, TEdge> : GraphDelegator<TNode, TEdge>, IGraph<TNode, TEdge>
{
    private const    long                       SerialVersionUID = -6838132233557L;
    private readonly Func<TEdge, double> _weightFunction;
    private readonly IDictionary<TEdge, double> _weights;
    private readonly bool                       _writeWeightsThrough;
    private readonly bool                       _cacheWeights;

    ///<summary>
    ///Constructor for AsWeightedGraph where the weights are provided through a map. Invocations of
    ///the @link{setEdgeWeight} method will update the map. Moreover, calls to @link{setEdgeWeight}
    ///are propagated to the underlying graph.
    ///</summary>
    ///<param name="graph"> the backing graph over which a weighted view is to be created.</param>
    ///<param name="weights"> the map containing the edge weights.</param>
    ///<exception cref="NullReferenceException"> if the graph or the weights are null.</exception>
    public AsWeightedGraph(IGraph<TNode, TEdge> graph, IDictionary<TEdge, double> weights)
        : this(graph, weights, graph.Type.Weighted)
    {
    }

    ///<summary>
    ///Constructor for AsWeightedGraph which allows weight write propagation to be requested
    ///explicitly.
    ///</summary>
    ///<param name="graph"> the backing graph over which an weighted view is to be created.</param>
    ///<param name="weights"> the map containing the edge weights.</param>
    ///<param name="writeWeightsThrough"> if set to true, the weights will get propagated to the backing
    ///       graph in the <c>setEdgeWeight()</c> method.</param>
    ///<exception cref="NullReferenceException"> if the graph or the weights are null.</exception>
    ///<exception cref="ArgumentException"> if <c>writeWeightsThrough</c> is set to true and
    ///        <c>graph</c> is not a weighted graph.</exception>
    public AsWeightedGraph(IGraph<TNode, TEdge> graph, IDictionary<TEdge, double> weights, bool writeWeightsThrough)
        : base(graph)
    {
        _weights             = Objects.requireNonNull(weights);
        _weightFunction      = null;
        _cacheWeights        = false;
        _writeWeightsThrough = writeWeightsThrough;

        if (_writeWeightsThrough)
        {
            GraphTests.RequireWeighted(graph);
        }
    }

    ///<summary>
    ///Constructor for AsWeightedGraph which uses a weight function to compute edge weights. When
    ///the weight of an edge is queried, the weight function is invoked. If
    ///<c>cacheWeights</c> is set to <c>true</c>, the weight of an edge returned by the
    ///<c>weightFunction</c> after its first invocation is stored in a map. The weight of an
    ///edge returned by subsequent calls to @link{getEdgeWeight} for the same edge will then be
    ///retrieved directly from the map, instead of re-invoking the weight function. If
    /// <c>cacheWeights</c> is set to <c>false</c>, each invocation of
    /// the @link{getEdgeWeight} method will invoke the weight function. Caching the edge weights is
    /// particularly useful when pre-computing all edge weights is expensive and it is expected that
    /// the weights of only a subset of all edges will be queried.
    /// </summary>
    /// <param name="graph"> the backing graph over which an weighted view is to be created.</param>
    /// <param name="weightFunction"> function which maps an edge to a weight.</param>
    /// <param name="cacheWeights"> if set to <c>true</c>, weights are cached once computed by the
    ///        weight function.</param>
    /// <param name="writeWeightsThrough"> if set to <c>true</c>, the weight set directly by
    ///        the @link{setEdgeWeight} method will be propagated to the backing graph.</param>
    /// <exception cref="NullReferenceException"> if the graph or the weight function is null.</exception>
    /// <exception cref="ArgumentException"> if <c>writeWeightsThrough</c> is set to true and
    ///         <c>graph</c> is not a weighted graph.</exception>
    public AsWeightedGraph(
        IGraph<TNode, TEdge>       graph,
        Func<TEdge, double> weightFunction,
        bool                       cacheWeights,
        bool                       writeWeightsThrough
    )
        : base(graph)
    {
        _weightFunction      = Objects.requireNonNull(weightFunction);
        _cacheWeights        = cacheWeights;
        _writeWeightsThrough = writeWeightsThrough;
        _weights             = new Dictionary<TEdge, double>();

        if (_writeWeightsThrough)
        {
            GraphTests.RequireWeighted(graph);
        }
    }

    /// <summary>
    /// Returns the weight assigned to a given edge. If weights are provided through a map, first a
    /// map lookup is performed. If the edge is not found, the @link{getEdgeWeight} method of the
    /// underlying graph is invoked instead. If, on the other hand, the weights are provided through
    /// a function, this method will first attempt to lookup the weight of an edge in the cache (that
    /// is, if <c>cacheWeights</c> is set to <c>true</c> in the constructor). If caching
    /// was disabled, or the edge could not be found in the cache, the weight function is invoked. If
    /// the function does not provide a weight for a given edge, the call is again propagated to the
    /// underlying graph.
    /// </summary>
    /// <param name="edge"> edge of interest.</param>
    /// <returns>the edge weight.</returns>
    /// <exception cref="NullReferenceException"> if the edge is null.</exception>
    public override double GetEdgeWeight(TEdge edge)
    {
        double? weight;
        if (_weightFunction != null)
        {
            if (_cacheWeights) // If weights are cached, check map first before invoking the weight
            {
                // function
                weight = _weights.computeIfAbsent(edge, _weightFunction);
            }
            else
            {
                weight = _weightFunction.apply(edge);
            }
        }
        else
        {
            weight = _weights[edge];
        }

        if (weight == null)
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
    /// <param name="edge"> edge on which to set weight.</param>
    /// <param name="weight"> new weight for edge.</param>
    /// <exception cref="NullReferenceException"> if the edge is null.</exception>
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

    public override IGraphType Type
    {
        get
        {
            return base.Type.AsWeighted();
        }
    }
}
