/*
 * (C) Copyright 2007-2021, by France Telecom and Contributors.
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
using Util;

/// <summary>
/// Helper for <see cref="MaskSubgraph"/>.
/// 
/// </summary>
internal class MaskEdgeSet<TNode, TEdge> : AbstractSet<TEdge>
{
    private readonly IGraph<TNode, TEdge>    _graph;
    private readonly ISet<TEdge>             _edgeSet;
    private readonly Predicate<TNode> _vertexMask;
    private readonly Predicate<TEdge> _edgeMask;

    public MaskEdgeSet(
        IGraph<TNode, TEdge>    graph,
        ISet<TEdge>             edgeSet,
        Predicate<TNode> vertexMask,
        Predicate<TEdge> edgeMask
    )
    {
        _graph      = graph;
        _edgeSet    = edgeSet;
        _vertexMask = vertexMask;
        _edgeMask   = edgeMask;
    }

    /// <inheritdoc/>
    public override bool Contains(object o)
    {
        if (!_edgeSet.Contains(o))
        {
            return false;
        }

        TEdge edge = TypeUtil.UncheckedCast(o);

        return !_edgeMask.test(edge) && !_vertexMask.test(_graph.GetEdgeSource(edge)) &&
               !_vertexMask.test(_graph.GetEdgeTarget(edge));
    }

    /// <inheritdoc/>
    public override IEnumerator<TEdge> Iterator()
    {
        return _edgeSet.Where(edge =>
            !_edgeMask.test(edge) && !_vertexMask.test(_graph.GetEdgeSource(edge)) &&
            !_vertexMask.test(_graph.GetEdgeTarget(edge))
        ).GetEnumerator();
    }

    /// <inheritdoc/>
    public override int Size()
    {
        return (int)_edgeSet.Where(edge =>
            !_edgeMask.test(edge) && !_vertexMask.test(_graph.GetEdgeSource(edge)) &&
            !_vertexMask.test(_graph.GetEdgeTarget(edge))
        ).Count();
    }

    /// <inheritdoc/>
    public override bool Empty
    {
        get
        {
            //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            return !Iterator().hasNext();
        }
    }
}
