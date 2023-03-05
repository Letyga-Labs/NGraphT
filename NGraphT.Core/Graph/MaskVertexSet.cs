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

using Util;

/// <summary>
/// Helper for <see cref="MaskSubgraph"/>.
/// 
/// </summary>
internal class MaskVertexSet<TNode> : AbstractSet<TNode>
{
    private readonly ISet<TNode>             _vertexSet;
    private readonly Predicate<TNode> _mask;

    public MaskVertexSet(ISet<TNode> vertexSet, Predicate<TNode> mask)
    {
        _vertexSet = vertexSet;
        _mask      = mask;
    }

    ///<inheritdoc/>
    public override bool Contains(object o)
    {
        if (!_vertexSet.Contains(o))
        {
            return false;
        }

        TNode node = TypeUtil.UncheckedCast(o);
        return !_mask.test(node);
    }

    ///<inheritdoc/>
    public override IEnumerator<TNode> Iterator()
    {
        return _vertexSet.Where(_mask.negate()).GetEnumerator();
    }

    ///<inheritdoc/>
    public override int Size()
    {
        return (int)_vertexSet.Where(_mask.negate()).Count();
    }

    ///<inheritdoc/>
    public override bool Empty
    {
        get
        {
            //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            return !Iterator().hasNext();
        }
    }
}
