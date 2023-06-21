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

using J2N.Collections.Generic.Extensions;

namespace NGraphT.Core.Graph.Specifics;

/// <summary>
/// A container for vertex edges.
///
/// <para>
/// In this edge container we use array lists to minimize memory toll. However, for high-degree
/// vertices we replace the entire edge container with a direct access subclass (to be implemented).
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Barak Naveh.</remarks>
public sealed class DirectedEdgeContainer<TVertex, TEdge>
    where TVertex : class
    where TEdge : class
{
    private ISet<TEdge>? _unmodifiableIncoming;
    private ISet<TEdge>? _unmodifiableOutgoing;

    internal DirectedEdgeContainer(IEdgeSetFactory<TVertex, TEdge> edgeSetFactory, TVertex vertex)
    {
        Incoming = edgeSetFactory.CreateEdgeSet(vertex);
        Outgoing = edgeSetFactory.CreateEdgeSet(vertex);
    }

    /// <summary>
    /// A lazy build of unmodifiable incoming edge set.
    /// </summary>
    /// <returns>an unmodifiable version of the incoming edge set.</returns>
    public ISet<TEdge> UnmodifiableIncomingEdges => _unmodifiableIncoming ??= Incoming.AsReadOnly();

    /// <summary>
    /// A lazy build of unmodifiable outgoing edge set.
    /// </summary>
    /// <returns>an unmodifiable version of the outgoing edge set.</returns>
    public ISet<TEdge> UnmodifiableOutgoingEdges => _unmodifiableOutgoing ??= Outgoing.AsReadOnly();

    internal ISet<TEdge> Incoming { get; }
    internal ISet<TEdge> Outgoing { get; }

    /// <summary>
    /// Add an incoming edge.
    /// </summary>
    /// <param name="edge"> the edge to add.</param>
    public void AddIncomingEdge(TEdge edge)
    {
        Incoming.Add(edge);
    }

    /// <summary>
    /// Add an outgoing edge.
    /// </summary>
    /// <param name="edge"> the edge to add.</param>
    public void AddOutgoingEdge(TEdge edge)
    {
        Outgoing.Add(edge);
    }

    /// <summary>
    /// Remove an incoming edge.
    /// </summary>
    /// <param name="edge"> the edge to remove.</param>
    public void RemoveIncomingEdge(TEdge edge)
    {
        Incoming.Remove(edge);
    }

    /// <summary>
    /// Remove an outgoing edge.
    /// </summary>
    /// <param name="edge"> the edge to remove.</param>
    public void RemoveOutgoingEdge(TEdge edge)
    {
        Outgoing.Remove(edge);
    }
}
