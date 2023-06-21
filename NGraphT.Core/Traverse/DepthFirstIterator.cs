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

/// <summary>
/// A depth-first iterator for a directed or undirected graph.
///
/// <para>
/// For this iterator to work correctly the graph must not be modified during iteration. Currently
/// there are no means to ensure that, nor to fail-fast. The results of such modifications are
/// undefined.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Liviu Rau.</remarks>
/// <remarks>Author: Barak Naveh.</remarks>
public sealed class DepthFirstIterator<TVertex, TEdge>
    : CrossComponentIterator<TVertex, TEdge, DepthFirstIterator<TVertex, TEdge>.VisitColor>
    where TVertex : class
    where TEdge : class
{
    private readonly Stack<TVertex> _stack = new();

    /// <summary>
    /// Creates a new depth-first iterator for the specified graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    public DepthFirstIterator(IGraph<TVertex, TEdge> g)
        : this(g, (TVertex?)null)
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
    public DepthFirstIterator(IGraph<TVertex, TEdge> g, TVertex? startVertex)
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
    public DepthFirstIterator(IGraph<TVertex, TEdge> g, IEnumerable<TVertex> startVertices)
        : base(g, startVertices)
    {
    }

    /// <summary>
    /// Standard vertex visit state enumeration.
    /// </summary>
    public enum VisitColor
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
        Black,
    }

    /// <summary>
    /// Retrieves the LIFO stack of vertices which have been encountered but not yet visited (WHITE).
    /// This stack also contains <em>sentinel</em> entries representing vertices which have been
    /// visited but are still GRAY. A sentinel entry is a sequence (TVertex, SENTINEL), whereas a
    /// non-sentinel entry is just (TVertex).
    /// </summary>
    /// <returns>stack.</returns>
    public Stack<TVertex> Stack => _stack;

    protected override bool ConnectedComponentExhausted
    {
        get
        {
            RewindGrayUntilWhite();
            return _stack.Count > 0;
        }
    }

    protected override void EncounterVertex(TVertex vertex, TEdge? edge)
    {
        PutSeenData(vertex, VisitColor.White);
        _stack.Push(vertex);
    }

    protected override void EncounterVertexAgain(TVertex vertex, TEdge edge)
    {
        // do nothing
    }

    protected override TVertex ProvideNextVertex()
    {
        RewindGrayUntilWhite();
        var topMostWhite = _stack.Peek();
        PutSeenData(topMostWhite, VisitColor.Gray);
        return topMostWhite;
    }

    private void RewindGrayUntilWhite()
    {
        for (var top = _stack.Peek(); _stack.Count != 0; _stack.Pop())
        {
            switch (GetSeenData(top))
            {
                case VisitColor.White:
                    return;
                case VisitColor.Gray:
                    RecordFinish();
                    break;
                case VisitColor.Black:
                    Debug.Fail("Black vertices must not be on the stack!");
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    private void RecordFinish()
    {
        var v = _stack.Pop();
        PutSeenData(v, VisitColor.Black);
        FinishVertex(v);
    }
}
