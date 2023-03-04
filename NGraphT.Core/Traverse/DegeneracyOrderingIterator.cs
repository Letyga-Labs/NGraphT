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

namespace NGraphT.Core.Traverse;

using Core;
using Graphs = Graphs;

/// <summary>
/// A degeneracy ordering iterator.
///
/// <para>
/// The degeneracy of a graph $G $is the smallest value d such that every nonempty subgraph of $G$
/// contains a vertex of degree at most $d.$ If a graph has degeneracy $d$, then it has a degeneracy
/// ordering, an ordering such that each vertex has $d$ or fewer neighbors that come later in the
/// ordering.
///
/// </para>
/// <para>
/// The iterator crosses components but does not track them, it only tracks visited vertices.
///
/// </para>
/// <para>
/// The iterator treats the input graph as undirected even if the graph is directed. Moreover, it
/// completely ignores self-loops, meaning that it operates as if self-loops do not contribute to the
/// degree of a vertex.
///
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public class DegeneracyOrderingIterator<TNode, TEdge> : AbstractGraphIterator<TNode, TEdge>
{
    private ISet<TNode>[]           _buckets;
    private IDictionary<TNode, int> _degrees;
    private int                     _minDegree;
    private TNode                   _cur;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="graph"> the graph to be iterated.</param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public DegeneracyOrderingIterator(NGraphT.Core.Graph<TNode, TEdge> graph)
    public DegeneracyOrderingIterator(IGraph<TNode, TEdge> graph)
        : base(graph)
    {
        /*
         * Count degrees, but skip self-loops
         */
        _minDegree = int.MaxValue;
        var maxDegree = 0;
        _degrees = new Dictionary<TNode, int>();
        foreach (var node in graph.VertexSet())
        {
            var d = 0;
            foreach (var edge in graph.EdgesOf(node))
            {
                var u = Graphs.GetOppositeVertex(graph, edge, node);
                if (!node.Equals(u))
                {
                    d++;
                }
            }

            _degrees[node] = d;
            _minDegree     = Math.Min(_minDegree, d);
            maxDegree      = Math.Max(maxDegree, d);
        }

        _minDegree = Math.Min(_minDegree, maxDegree);

        /*
         * Create buckets
         */
        _buckets = (ISet<TNode>[])Array.CreateInstance(typeof(ISet<object>), maxDegree + 1);
        for (var i = 0; i < _buckets.Length; i++)
        {
            _buckets[i] = new LinkedHashSet<TNode>();
        }

        foreach (var node in graph.VertexSet())
        {
            _buckets[_degrees[node]].Add(node);
        }
    }

    /// <summary>
    /// {@inheritDoc}
    /// 
    /// Always returns true since the iterator does not care about components.
    /// </summary>
    public override bool CrossComponentTraversal
    {
        get
        {
            return true;
        }
        set
        {
            if (!value)
            {
                throw new ArgumentException("Iterator is always cross-component");
            }
        }
    }


    public override bool HasNext()
    {
        if (_cur != null)
        {
            return true;
        }

        _cur = Advance();
        if (_cur != null && NListeners != 0)
        {
            FireVertexTraversed(CreateVertexTraversalEvent(_cur));
        }

        return _cur != null;
    }

    public override TNode Next()
    {
        if (!HasNext())
        {
            throw new NoSuchElementException();
        }

        var result = _cur;
        _cur = default(TNode);
        if (NListeners != 0)
        {
            FireVertexFinished(CreateVertexTraversalEvent(result));
        }

        return result;
    }

    private TNode Advance()
    {
        while (_minDegree < _buckets.Length && _buckets[_minDegree].Count == 0)
        {
            _minDegree++;
        }

        var result = default(TNode);

        if (_minDegree < _buckets.Length)
        {
            ISet<TNode> b    = _buckets[_minDegree];
            TNode       node = b.GetEnumerator().next();
            b.remove(node);
            _degrees.Remove(node);

            foreach (TEdge edge in graph.EdgesOf(node))
            {
                var u = Graphs.GetOppositeVertex(graph, edge, node);

                if (node.Equals(u))
                {
                    // ignore self-loop
                    continue;
                }

                if (_degrees.ContainsKey(u))
                {
                    var uDegree = _degrees[u];
                    if (uDegree > _minDegree)
                    {
                        _buckets[uDegree].remove(u);
                        uDegree--;
                        _degrees[u] = uDegree;
                        _buckets[uDegree].Add(u);
                    }
                }
            }

            result = node;
        }

        return result;
    }
}
