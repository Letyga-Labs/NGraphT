/*
 * (C) Copyright 2015-2021, by Barak Naveh and Contributors.
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

namespace NGraphT.Core.Graph.Specifics;

using Graph;

/// <summary>
/// A container for vertex edges.
///
/// <para>
/// In this edge container we use array lists to minimize memory toll. However, for high-degree
/// vertices we replace the entire edge container with a direct access subclass (to be implemented).
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public class UndirectedEdgeContainer<TNode, TEdge>
{
    private const long        SerialVersionUID = -6623207588411170010L;
    internal      ISet<TEdge> VertexEdges;

    private ISet<TEdge> _unmodifiableVertexEdges = null;

    internal UndirectedEdgeContainer(IEdgeSetFactory<TNode, TEdge> edgeSetFactory, TNode vertex)
    {
        VertexEdges = edgeSetFactory.CreateEdgeSet(vertex);
    }

    /// <summary>
    /// A lazy build of unmodifiable list of vertex edges
    /// </summary>
    /// <returns>an unmodifiable set of vertex edges.</returns>
    public virtual ISet<TEdge> UnmodifiableVertexEdges
    {
        get
        {
            if (_unmodifiableVertexEdges == null)
            {
                _unmodifiableVertexEdges = Collections.unmodifiableSet(VertexEdges);
            }

            return _unmodifiableVertexEdges;
        }
    }

    /// <summary>
    /// Add a vertex edge
    /// </summary>
    /// <param name="edge"> the edge to add.</param>
    public virtual void AddEdge(TEdge edge)
    {
        VertexEdges.Add(edge);
    }

    /// <summary>
    /// Get number of vertex edges
    /// </summary>
    /// <returns>the number of vertex edges.</returns>
    public virtual int EdgeCount()
    {
        return VertexEdges.Count;
    }

    /// <summary>
    /// Remove a vertex edge
    /// </summary>
    /// <param name="edge"> the edge to remove.</param>
    public virtual void RemoveEdge(TEdge edge)
    {
        VertexEdges.remove(edge);
    }
}
