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

using NGraphT.Core.DotNetUtil;

namespace NGraphT.Core.Graph;

/// <summary>
/// Implementation of the GraphMapping interface. The performance of <c>getVertex/EdgeCorrespondence</c>
/// is based on the performance of the concrete IDictionary class which is passed in the constructor.
/// For example, using <see cref="Dictionary{TKey,TValue}"/> will provide expected $O(1)$ performance.
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Assaf Lehr.</remarks>
public sealed class DefaultGraphMapping<TVertex, TEdge> : IGraphMapping<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private readonly IDictionary<TVertex, TVertex> _graphMappingForward;
    private readonly IDictionary<TVertex, TVertex> _graphMappingReverse;

    private readonly IGraph<TVertex, TEdge> _graph1;
    private readonly IGraph<TVertex, TEdge> _graph2;

    /// <summary>
    /// The maps themselves are used. There is no defensive-copy. Assumption: The key and value in
    /// the mappings are of valid graph objects. It is not checked.
    /// </summary>
    /// <param name="g1ToG2"> vertex mapping from the first graph to the second.</param>
    /// <param name="g2ToG1"> vertex mapping from the second graph to the first.</param>
    /// <param name="g1"> the first graph.</param>
    /// <param name="g2"> the second graph.</param>
    public DefaultGraphMapping(
        IDictionary<TVertex, TVertex> g1ToG2,
        IDictionary<TVertex, TVertex> g2ToG1,
        IGraph<TVertex, TEdge>        g1,
        IGraph<TVertex, TEdge>        g2
    )
    {
        _graph1              = g1;
        _graph2              = g2;
        _graphMappingForward = g1ToG2;
        _graphMappingReverse = g2ToG1;
    }

    public TEdge? GetEdgeCorrespondence(TEdge edge, bool forward)
    {
        ArgumentNullException.ThrowIfNull(edge);

        IGraph<TVertex, TEdge> sourceGraph, targetGraph;

        if (forward)
        {
            sourceGraph = _graph1;
            targetGraph = _graph2;
        }
        else
        {
            sourceGraph = _graph2;
            targetGraph = _graph1;
        }

        var mappedSourceVertex = GetVertexCorrespondence(sourceGraph.GetEdgeSource(edge), forward);
        var mappedTargetVertex = GetVertexCorrespondence(sourceGraph.GetEdgeTarget(edge), forward);
        if (mappedSourceVertex == null || mappedTargetVertex == null)
        {
            return null;
        }

        return targetGraph.GetEdge(mappedSourceVertex, mappedTargetVertex);
    }

    public TVertex? GetVertexCorrespondence(TVertex vertex, bool forward)
    {
        ArgumentNullException.ThrowIfNull(vertex);
        return forward
            ? _graphMappingForward.GetOrDefault(vertex)
            : _graphMappingReverse.GetOrDefault(vertex);
    }
}
