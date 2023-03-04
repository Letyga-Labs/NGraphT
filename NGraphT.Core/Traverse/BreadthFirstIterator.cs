using System.Diagnostics;

/*
 * (C) Copyright 2003-2021, by Barak Naveh and Contributors.
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

/// <summary>
/// A breadth-first iterator for a directed or undirected graph.
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
/// <remarks>Author: Barak Naveh.</remarks>
public class
    BreadthFirstIterator<TNode, TEdge> : CrossComponentIterator<TNode, TEdge,
        BreadthFirstIterator.SearchNodeData<TEdge>>
{
    private LinkedList<TNode> _queue = new LinkedList<TNode>();

    /// <summary>
    /// Creates a new breadth-first iterator for the specified graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    public BreadthFirstIterator(IGraph<TNode, TEdge> g)
        : this(g, (TNode)null)
    {
    }

    /// <summary>
    /// Creates a new breadth-first iterator for the specified graph. Iteration will start at the
    /// specified start vertex and will be limited to the connected component that includes that
    /// vertex. If the specified start vertex is <c>null</c>, iteration will start at an
    /// arbitrary vertex and will not be limited, that is, will be able to traverse all the graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertex"> the vertex iteration to be started.</param>
    public BreadthFirstIterator(IGraph<TNode, TEdge> g, TNode startVertex)
        : base(g, startVertex)
    {
    }

    /// <summary>
    /// Creates a new breadth-first iterator for the specified graph. Iteration will start at the
    /// specified start vertices and will be limited to the connected component that includes those
    /// vertices. If the specified start vertices is <c>null</c>, iteration will start at an
    /// arbitrary vertex and will not be limited, that is, will be able to traverse all the graph.
    /// </summary>
    /// <param name="g"> the graph to be iterated.</param>
    /// <param name="startVertices"> the vertices iteration to be started.</param>
    public BreadthFirstIterator(IGraph<TNode, TEdge> g, IEnumerable<TNode> startVertices)
        : base(g, startVertices)
    {
    }

    /// <inheritdoc/>
    protected internal override bool ConnectedComponentExhausted
    {
        get
        {
            return _queue.Count == 0;
        }
    }

    /// <inheritdoc/>
    protected internal override void EncounterVertex(TNode vertex, TEdge edge)
    {
        var depth = (edge == null ? 0 : GetSeenData(Graphs.GetOppositeVertex(graph, edge, vertex)).depth + 1);
        PutSeenData(vertex, new SearchNodeData<TEdge>(edge, depth));
        _queue.AddLast(vertex);
    }

    /// <inheritdoc/>
    protected internal override void EncounterVertexAgain(TNode vertex, TEdge edge)
    {
    }

    /// <summary>
    /// Returns the parent node of vertex $v$ in the BFS search tree, or null if $v$ is the root
    /// node. This method can only be invoked on a vertex $v$ once the iterator has visited vertex
    /// $v$!
    /// </summary>
    /// <param name="node"> vertex.</param>
    /// <returns>parent node of vertex $v$ in the BFS search tree, or null if $v$ is a root node.</returns>
    public virtual TNode GetParent(TNode node)
    {
        Debug.Assert(GetSeenData(node) != null);
        TEdge edge = GetSeenData(node).edge;
        if (edge == null)
        {
            return default(TNode);
        }
        else
        {
            return Graphs.GetOppositeVertex(graph, edge, node);
        }
    }

    /// <summary>
    /// Returns the edge connecting vertex $v$ to its parent in the spanning tree formed by the BFS
    /// search, or null if $v$ is a root node. This method can only be invoked on a vertex $v$ once
    /// the iterator has visited vertex $v$!
    /// </summary>
    /// <param name="node"> vertex.</param>
    /// <returns>edge connecting vertex $v$ in the BFS search tree to its parent, or null if $v$ is a
    ///         root node.</returns>
    public virtual TEdge GetSpanningTreeEdge(TNode node)
    {
        Debug.Assert(GetSeenData(node) != null);
        return GetSeenData(node).edge;
    }

    /// <summary>
    /// Returns the depth of vertex $v$ in the search tree. The depth of a vertex $v$ is defined as
    /// the number of edges traversed on the path from the root of the BFS tree to vertex $v$. The
    /// root of the search tree has depth 0. This method can only be invoked on a vertex $v$ once the
    /// iterator has visited vertex $v$!
    /// </summary>
    /// <param name="node"> vertex.</param>
    /// <returns>depth of vertex $v$ in the search tree.</returns>
    public virtual int GetDepth(TNode node)
    {
        Debug.Assert(GetSeenData(node) != null);
        return GetSeenData(node).depth;
    }

    /// <seealso cref="CrossComponentIterator.provideNextVertex()"/>
    protected internal override TNode ProvideNextVertex()
    {
        return _queue.RemoveFirst();
    }

    /// <summary>
    /// Data kept for discovered vertices.
    /// </summary>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    protected internal class SearchNodeData<TEdge>
    {
        internal readonly TEdge Edge;
        internal readonly int   Depth;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="edge"> edge to parent.</param>
        /// <param name="depth"> depth of node in search tree.</param>
        public SearchNodeData(TEdge edge, int depth)
        {
            this.edge  = edge;
            this.depth = depth;
        }

        /// <summary>
        /// Edge to parent
        /// </summary>
        /// <returns>the edge to the parent.</returns>
        public virtual TEdge Edge
        {
            get
            {
                return edge;
            }
        }

        /// <summary>
        /// Depth of node in search tree
        /// </summary>
        /// <returns>the depth of the node in the search tree.</returns>
        public virtual int Depth
        {
            get
            {
                return depth;
            }
        }
    }
}
