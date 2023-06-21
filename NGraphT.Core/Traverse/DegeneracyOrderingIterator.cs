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
using NGraphT.Core.Util;
using Java2Net = J2N.Collections.Generic;

namespace NGraphT.Core.Traverse;

/// <summary>
/// A degeneracy ordering iterator.
///
/// <para>
/// The degeneracy of a graph $G $is the smallest value d such that every nonempty subgraph of $G$
/// contains a vertex of degree at most $d.$ If a graph has degeneracy $d$, then it has a degeneracy
/// ordering, an ordering such that each vertex has $d$ or fewer neighbors that come later in the
/// ordering.
/// </para>
///
/// <para>
/// The iterator crosses components but does not track them, it only tracks visited vertices.
/// </para>
///
/// <para>
/// The iterator treats the input graph as undirected even if the graph is directed. Moreover, it
/// completely ignores self-loops, meaning that it operates as if self-loops do not contribute to the
/// degree of a vertex.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
public sealed class DegeneracyOrderingIterator<TVertex, TEdge> : AbstractGraphIterator<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private readonly ISet<TVertex>[]           _buckets;
    private readonly IDictionary<TVertex, int> _degrees;

    private int      _minDegree;
    private TVertex? _current;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="graph"> the graph to be iterated.</param>
    public DegeneracyOrderingIterator(IGraph<TVertex, TEdge> graph)
        : base(graph)
    {
        // Count degrees, but skip self-loops
        _minDegree = int.MaxValue;
        var maxDegree = 0;
        _degrees = new Dictionary<TVertex, int>();
        foreach (var v in graph.VertexSet())
        {
            var d = 0;
            foreach (var edge in graph.EdgesOf(v))
            {
                var u = Graphs.GetOppositeVertex(graph, edge, v);
                if (!v.Equals(u))
                {
                    d++;
                }
            }

            _degrees[v] = d;
            _minDegree  = Math.Min(_minDegree, d);
            maxDegree   = Math.Max(maxDegree, d);
        }

        _minDegree = Math.Min(_minDegree, maxDegree);

        // Create buckets
        _buckets = new ISet<TVertex>[maxDegree + 1];
        for (var i = 0; i < _buckets.Length; i++)
        {
            _buckets[i] = new Java2Net.LinkedHashSet<TVertex>();
        }

        foreach (var v in graph.VertexSet())
        {
            _buckets[_degrees[v]].Add(v);
        }
    }

    /// <summary>
    /// <inheritdoc/>
    ///
    /// Always returns true since the iterator does not care about components.
    /// </summary>
    public override bool CrossComponentTraversal
    {
        get => true;
        set
        {
            if (!value)
            {
                throw new ArgumentException("Iterator is always cross-component", nameof(value));
            }
        }
    }

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public override TVertex Current => _current ?? throw new NoSuchElementException();

    public override bool MoveNext()
    {
        var previous = _current;
        if (previous != null && NListeners != 0)
        {
            FireVertexFinished(CreateVertexTraversalEvent(previous));
        }

        _current = Advance();
        if (_current != null && NListeners != 0)
        {
            FireVertexTraversed(CreateVertexTraversalEvent(_current));
        }

        return _current != null;
    }

    private TVertex? Advance()
    {
        while (_minDegree < _buckets.Length && _buckets[_minDegree].Count == 0)
        {
            _minDegree++;
        }

        if (_minDegree >= _buckets.Length)
        {
            return null;
        }

        var b = _buckets[_minDegree];
        var v = b.First();
        b.Remove(v);
        _degrees.Remove(v);

        foreach (var edge in Graph.EdgesOf(v))
        {
            var u = Graphs.GetOppositeVertex(Graph, edge, v);

            if (v.Equals(u) || !_degrees.ContainsKey(u))
            {
                // ignore self-loop
                continue;
            }

            var uDegree = _degrees[u];
            if (uDegree > _minDegree)
            {
                _buckets[uDegree].Remove(u);
                uDegree--;
                _degrees[u] = uDegree;
                _buckets[uDegree].Add(u);
            }
        }

        return v;
    }
}
