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

namespace NGraphT.Core.Graph;

using Core;

/// <summary>
/// An unmodifiable view of the backing graph specified in the constructor. This graph allows modules
/// to provide users with "read-only" access to internal graphs. Query operations on this graph "read
/// through" to the backing graph, and attempts to modify this graph result in an <c>
/// UnsupportedOperationException</c>.
///
/// <para>
/// This graph does <i>not</i> pass the hashCode and equals operations through to the backing graph,
/// but relies on <c>Object</c>'s <c>equals</c> and <c>hashCode</c> methods. This
/// graph will be serializable if the backing graph is serializable.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public class AsUnmodifiableGraph<TNode, TEdge> : GraphDelegator<TNode, TEdge>
{
    private const string UNMODIFIABLE = "this graph is unmodifiable";

    ///<summary>
    ///Creates a new unmodifiable graph based on the specified backing graph.
    ///</summary>
    ///<param name="g"> the backing graph on which an unmodifiable graph is to be created.</param>
    public AsUnmodifiableGraph(IGraph<TNode, TEdge> g)
        : base(g)
    {
    }

    ///<see cref="Graph.addEdge(Object, Object)"/>
    public override TEdge AddEdge(TNode sourceVertex, TNode targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    ///<see cref="Graph.addEdge(Object, Object, Object)"/>
    public override bool AddEdge(TNode sourceVertex, TNode targetVertex, TEdge edge)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    ///<see cref="Graph.addVertex()"/>
    public override TNode AddVertex()
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    ///<see cref="Graph.addVertex(Object)"/>
    public override bool AddVertex(TNode node)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    ///<see cref="Graph.removeAllEdges(Collection)"/>
    public virtual bool RemoveAllEdges<T1>(ICollection<T1> edges) where T1 : TEdge
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    ///<see cref="Graph.removeAllEdges(Object, Object)"/>
    public override ISet<TEdge> RemoveAllEdges(TNode sourceVertex, TNode targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    ///<see cref="Graph.removeAllVertices(Collection)"/>
    public virtual bool RemoveAllVertices<T1>(ICollection<T1> vertices) where T1 : TNode
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    ///<see cref="Graph.removeEdge(Object)"/>
    public override bool RemoveEdge(TEdge edge)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    ///<see cref="Graph.removeEdge(Object, Object)"/>
    public override TEdge RemoveEdge(TNode sourceVertex, TNode targetVertex)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    ///<see cref="Graph.removeVertex(Object)"/>
    public override bool RemoveVertex(TNode node)
    {
        throw new NotSupportedException(UNMODIFIABLE);
    }

    ///<inheritdoc/>
    public override IGraphType Type
    {
        get
        {
            return base.Type.AsUnmodifiable();
        }
    }
}
