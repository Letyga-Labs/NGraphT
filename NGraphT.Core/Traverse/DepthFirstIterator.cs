using System.Diagnostics;

/*
 * (C) Copyright 2003-2021, by Liviu Rau and Contributors.
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
using Util;

/// <summary>
/// A depth-first iterator for a directed or undirected graph.
///
/// <para>
/// For this iterator to work correctly the graph must not be modified during iteration. Currently
/// there are no means to ensure that, nor to fail-fast. The results of such modifications are
/// undefined.
///
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Liviu Rau.</remarks>
/// <remarks>Author: Barak Naveh.</remarks>
public class DepthFirstIterator<TNode, TEdge> : CrossComponentIterator<TNode, TEdge, DepthFirstIterator.VisitColor>
{
    /// <summary>
    /// Sentinel object. Unfortunately, we can't use null, because ArrayDeque won't accept those. And
    /// we don't want to rely on the caller to provide a sentinel object for us. So we have to play
    /// typecasting games.
    /// </summary>
    public static readonly object Sentinel = new object();

    /// <summary>
    /// Standard vertex visit state enumeration.
    /// </summary>
    protected internal enum VisitColor
    {
        /// <summary>
        /// Vertex has not been returned via iterator yet.
        /// </summary>
        White,

        /// <summary>
        /// Vertex has been returned via iterator, but we're not done with all of its out-edges yet.
        /// </summary>
        Gray,

        /// <summary>
        /// Vertex has been returned via iterator, and we're done with all of its out-edges.
        /// </summary>
        Black
    }

    private LinkedList<object> _stack = new LinkedList<object>();

    /// <summary>
    /// Creates a new depth-first iterator for the specified graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    public DepthFirstIterator(IGraph<TNode, TEdge> g)
        : this(g, (TNode)null)
    {
    }

    /// <summary>
    /// Creates a new depth-first iterator for the specified graph. Iteration will start at the
    /// specified start vertex and will be limited to the connected component that includes that
    /// vertex. If the specified start vertex is <c>null</c>, iteration will start at an
    /// arbitrary vertex and will not be limited, that is, will be able to traverse all the graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertex"> the vertex iteration to be started.</param>
    public DepthFirstIterator(IGraph<TNode, TEdge> g, TNode startVertex)
        : base(g, startVertex)
    {
    }

    /// <summary>
    /// Creates a new depth-first iterator for the specified graph. Iteration will start at the
    /// specified start vertices and will be limited to the connected component that includes those
    /// vertices. If the specified start vertices is <c>null</c>, iteration will start at an
    /// arbitrary vertex and will not be limited, that is, will be able to traverse all the graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertices"> the vertices iteration to be started.</param>
    public DepthFirstIterator(IGraph<TNode, TEdge> g, IEnumerable<TNode> startVertices)
        : base(g, startVertices)
    {
    }

    protected internal override bool ConnectedComponentExhausted
    {
        get
        {
            for (;;)
            {
                if (_stack.Count == 0)
                {
                    return true;
                }

                if (_stack.Last.Value != Sentinel)
                {
                    // Found a non-sentinel.
                    return false;
                }

                // Found a sentinel: pop it, record the finish time,
                // and then loop to check the rest of the stack.

                // Pop null we peeked at above.
                _stack.RemoveLast();

                // This will pop corresponding vertex to be recorded as finished.
                RecordFinish();
            }
        }
    }

    protected internal override void EncounterVertex(TNode vertex, TEdge edge)
    {
        PutSeenData(vertex, VisitColor.White);
        _stack.AddLast(vertex);
    }

    protected internal override void EncounterVertexAgain(TNode vertex, TEdge edge)
    {
        VisitColor color = GetSeenData(vertex);
        if (color != VisitColor.White)
        {
            // We've already visited this vertex; no need to mess with the
            // stack (either it's BLACK and not there at all, or it's GRAY
            // and therefore just a sentinel).
            return;
        }

        // Since we've encountered it before, and it's still WHITE, it
        // *must* be on the stack. Use removeLastOccurrence on the
        // assumption that for typical topologies and traversals,
        // it's likely to be nearer the top of the stack than
        // the bottom of the stack.
        bool found = _stack.removeLastOccurrence(vertex);
        Debug.Assert((found));
        _stack.AddLast(vertex);
    }

    protected internal override TNode ProvideNextVertex()
    {
        TNode node;
        for (;;)
        {
            object o = _stack.RemoveLast();
            if (o == Sentinel)
            {
                // This is a finish-time sentinel we previously pushed.
                RecordFinish();
                // Now carry on with another pop until we find a non-sentinel
            }
            else
            {
                // Got a real vertex to start working on
                node = TypeUtil.UncheckedCast(o);
                break;
            }
        }

        // Push a sentinel for TNode onto the stack so that we'll know
        // when we're done with it.
        _stack.AddLast(node);
        _stack.AddLast(Sentinel);
        PutSeenData(node, VisitColor.Gray);
        return node;
    }

    private void RecordFinish()
    {
        TNode node = TypeUtil.UncheckedCast(_stack.RemoveLast());
        PutSeenData(node, VisitColor.Black);
        FinishVertex(node);
    }

    /// <summary>
    /// Retrieves the LIFO stack of vertices which have been encountered but not yet visited (WHITE).
    /// This stack also contains <em>sentinel</em> entries representing vertices which have been
    /// visited but are still GRAY. A sentinel entry is a sequence (TNode, SENTINEL), whereas a
    /// non-sentinel entry is just (TNode).
    /// </summary>
    /// <returns>stack.</returns>
    public virtual LinkedList<object> Stack
    {
        get
        {
            return _stack;
        }
    }
}
