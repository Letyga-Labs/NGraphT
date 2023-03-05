/*
 * (C) Copyright 2005-2021, by Assaf Lehr and Contributors.
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
/// Implementation of the GraphMapping interface. The performance of <c>
/// getVertex/EdgeCorrespondence</c> is based on the performance of the concrete Map class which
/// is passed in the constructor. For example, using <see cref="System.Collections.Hashtable"/> will provide expected $O(1)$
/// performance.
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Assaf Lehr.</remarks>
public class DefaultGraphMapping<TNode, TEdge> : IGraphMapping<TNode, TEdge>
{
    private IDictionary<TNode, TNode> _graphMappingForward;
    private IDictionary<TNode, TNode> _graphMappingReverse;

    private IGraph<TNode, TEdge> _graph1;
    private IGraph<TNode, TEdge> _graph2;

    ///<summary>
    ///The maps themselves are used. There is no defensive-copy. Assumption: The key and value in
    ///the mappings are of valid graph objects. It is not checked.
    ///</summary>
    ///<param name="g1ToG2"> vertex mapping from the first graph to the second.</param>
    ///<param name="g2ToG1"> vertex mapping from the second graph to the first.</param>
    ///<param name="g1"> the first graph.</param>
    ///<param name="g2"> the second graph.</param>
    public DefaultGraphMapping(
        IDictionary<TNode, TNode> g1ToG2,
        IDictionary<TNode, TNode> g2ToG1,
        IGraph<TNode, TEdge>      g1,
        IGraph<TNode, TEdge>      g2
    )
    {
        _graph1              = g1;
        _graph2              = g2;
        _graphMappingForward = g1ToG2;
        _graphMappingReverse = g2ToG1;
    }

    public virtual TEdge GetEdgeCorrespondence(TEdge currEdge, bool forward)
    {
        IGraph<TNode, TEdge> sourceGraph, targetGraph;

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

        var mappedSourceVertex = GetVertexCorrespondence(sourceGraph.GetEdgeSource(currEdge), forward);
        var mappedTargetVertex = GetVertexCorrespondence(sourceGraph.GetEdgeTarget(currEdge), forward);
        if ((mappedSourceVertex == null) || (mappedTargetVertex == null))
        {
            return default(TEdge);
        }
        else
        {
            return targetGraph.GetEdge(mappedSourceVertex, mappedTargetVertex);
        }
    }

    public virtual TNode GetVertexCorrespondence(TNode keyVertex, bool forward)
    {
        IDictionary<TNode, TNode> graphMapping;
        if (forward)
        {
            graphMapping = _graphMappingForward;
        }
        else
        {
            graphMapping = _graphMappingReverse;
        }

        return graphMapping[keyVertex];
    }
}
