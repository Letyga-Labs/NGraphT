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
using NGraphT.Core.DotNetUtil;

namespace NGraphT.Core.Graph;

/// <summary>
/// A walk in a Graph is an alternating sequence of vertices and edges, starting and ending at a
/// vertex, in which each edge is adjacent in the sequence to its two endpoints. More precisely, a
/// walk is a connected sequence of vertices and edges in a Graph $v_0, e_0, v_1, e_1, v_2, \dotso,
/// v_{k-1}, e_{k-1}, v_{k}$, such that for $1 \leq i \leq k$, the edge $e_i$ has endpoints $v_{i-1}$
/// and $v_i$. The class makes no assumptions with respect to the shape of the walk: edges may be
/// repeated, and the start and end point of the walk may be different.
///
/// <para>
/// See <a href="http://mathworld.wolfram.com/Walk.html">http://mathworld.wolfram.com/Walk.html</a>.
/// </para>
///
/// <para>
/// GraphWalk is the default implementation of <see cref="IGraphPath{TVertex,TEdge}"/>.
/// </para>
///
/// <para>
/// Two special cases exist:
/// <list type="number">
/// <item>
/// A singleton GraphWalk has an empty edge list (the length of the path equals 0), the vertex
/// list contains a single vertex TVertex, and the start and end vertex equal TVertex.
/// </item>
/// <item>
/// An empty Graphwalk has empty edge and vertex lists, and the start and end vertex are both null.
/// </item>
/// </list>
/// </para>
///
/// <para>
/// This class is implemented as a light-weight data structure; this class does not verify whether
/// the sequence of edges or the sequence of vertices provided during construction forms an actual
/// walk. It is the responsibility of the invoking class to provide correct input data.
/// </para>
///
/// <para>
/// Note: Serialization of a GraphWalk implies the serialization of the entire underlying Graph.
/// </para>
/// </summary>
///
/// <typeparam name="TVertex">The Graph vertex type.</typeparam>
/// <typeparam name="TEdge">The Graph edge type.</typeparam>
///
/// <remarks>Author: Joris Kinable.</remarks>
public sealed class GraphWalk<TVertex, TEdge> : IGraphPath<TVertex, TEdge>, IEquatable<GraphWalk<TVertex, TEdge>>
    where TVertex : class
    where TEdge : class
{
    private readonly IList<TVertex>? _vertexList;
    private readonly IList<TEdge>?   _edgeList;

    /// <summary>
    /// Creates a walk defined by a sequence of edges. A walk defined by its edges can be specified
    /// for non-simple graphs. Edge repetition is permitted, the start and end point points ($v_0$
    /// and $v_k$) can be different.
    /// </summary>
    /// <param name="graph"> the Graph.</param>
    /// <param name="startVertex"> the starting vertex.</param>
    /// <param name="endVertex"> the last vertex of the path.</param>
    /// <param name="edgeList"> the list of edges of the path.</param>
    /// <param name="weight"> the total weight of the path.</param>
    public GraphWalk(
        IGraph<TVertex, TEdge> graph,
        TVertex                startVertex,
        TVertex                endVertex,
        IList<TEdge>           edgeList,
        double                 weight
    )
        : this(graph, startVertex, endVertex, null, edgeList, weight)
    {
    }

    /// <summary>
    /// Creates a walk defined by a sequence of vertices. Note that the input Graph must be simple,
    /// otherwise the vertex sequence does not necessarily define a unique path. Furthermore, all
    /// vertices must be pairwise adjacent.
    /// </summary>
    /// <param name="graph"> the Graph.</param>
    /// <param name="vertexList"> the list of vertices of the path.</param>
    /// <param name="weight"> the total weight of the path.</param>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
    public GraphWalk(IGraph<TVertex, TEdge> graph, IList<TVertex> vertexList, double weight)
        : this(
            graph,
            vertexList.Count == 0 ? null : vertexList[0],
            vertexList.Count == 0 ? null : vertexList[^1],
            vertexList,
            null,
            weight
        )
    {
    }

    /// <summary>
    /// Creates a walk defined by both a sequence of edges and a sequence of vertices. Note that both
    /// the sequence of edges and the sequence of vertices must describe the same path! This is not
    /// verified during the construction of the walk. This constructor makes it possible to store
    /// both a vertex and an edge view of the same walk, thereby saving computational overhead when
    /// switching from one to the other.
    /// </summary>
    /// <param name="graph"> the Graph.</param>
    /// <param name="startVertex"> the starting vertex.</param>
    /// <param name="endVertex"> the last vertex of the path.</param>
    /// <param name="vertexList"> the list of vertices of the path.</param>
    /// <param name="edgeList"> the list of edges of the path.</param>
    /// <param name="weight"> the total weight of the path.</param>
    public GraphWalk(
        IGraph<TVertex, TEdge> graph,
        TVertex?               startVertex,
        TVertex?               endVertex,
        IList<TVertex>?        vertexList,
        IList<TEdge>?          edgeList,
        double                 weight
    )
    {
        ArgumentNullException.ThrowIfNull(graph);

        // Some necessary but not sufficient conditions for valid paths
        if (vertexList == null && edgeList == null)
        {
            throw new ArgumentException("Vertex list and edge list cannot both be null!", nameof(vertexList));
        }

        if (startVertex != null && vertexList != null && edgeList != null && edgeList.Count + 1 != vertexList.Count)
        {
            throw new ArgumentException(
                "VertexList and edgeList do not correspond to the same path (cardinality of vertexList +1 must equal the cardinality of the edgeList)",
                nameof(vertexList)
            );
        }

        if ((startVertex == null) ^ (endVertex == null))
        {
            throw new ArgumentException(
                "Either the start and end vertices must both be null, or they must both be not null (one of them is null)",
                nameof(startVertex)
            );
        }

        Graph       = graph;
        StartVertex = startVertex;
        EndVertex   = endVertex;
        _vertexList = vertexList;
        _edgeList   = edgeList;
        Weight      = weight;
    }

    public IGraph<TVertex, TEdge> Graph { get; }

    public double Weight { get; private set; }

    public TVertex? StartVertex { get; }

    public TVertex? EndVertex { get; }

    public IList<TVertex> VertexList
    {
        get
        {
            if (_vertexList != null)
            {
                return _vertexList;
            }

            var edgeList = EdgeList;

            if (!edgeList.Any())
            {
                var startVertex = StartVertex;
                if (startVertex != null && startVertex.Equals(EndVertex))
                {
                    return new List<TVertex>
                    {
                        startVertex,
                    };
                }
                else
                {
                    return Array.Empty<TVertex>();
                }
            }

            var list = new List<TVertex>();
            var v    = StartVertex;
            list.Add(v!);
            foreach (var edge in edgeList)
            {
                v = Graphs.GetOppositeVertex(Graph, edge, v!);
                list.Add(v);
            }

            return list;
        }
    }

    public IList<TEdge> EdgeList
    {
        get
        {
            if (_edgeList != null)
            {
                return _edgeList;
            }

            var vertexList = VertexList;
            if (vertexList.Count < 2)
            {
                return Array.Empty<TEdge>();
            }

            var g = Graph;
            var edgeList = vertexList
                .Pairwise()
                .Select(it =>
                    g.GetEdge(it.Previous, it.Current)
                    ?? throw new InvalidOperationException($"Could not find edge between {it.Previous} and {it.Current}"
                    )
                )
                .ToList();
            return edgeList;
        }
    }

    public int Length
    {
        get
        {
            if (_edgeList != null)
            {
                return _edgeList.Count;
            }

            if (_vertexList != null && _vertexList.Count > 0)
            {
                return _vertexList.Count - 1;
            }

            return 0;
        }
    }

    /// <summary>
    /// Returns true if the path is an empty path, that is, a path with _startVertex=_endVertex=null
    /// and with an empty vertex and edge list.
    /// </summary>
    /// <returns>Returns true if the path is an empty path.</returns>
    public bool Empty => StartVertex == null;

    public static bool operator ==(GraphWalk<TVertex, TEdge>? left, GraphWalk<TVertex, TEdge>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(GraphWalk<TVertex, TEdge>? left, GraphWalk<TVertex, TEdge>? right)
    {
        return !Equals(left, right);
    }

    public bool Equals(GraphWalk<TVertex, TEdge>? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (!EqualityComparer<TVertex?>.Default.Equals(StartVertex, other.StartVertex) ||
            !EqualityComparer<TVertex?>.Default.Equals(EndVertex,   other.EndVertex))
        {
            return false;
        }

        // If this path is expressed as a vertex list, we may get away by comparing the other path's vertex list
        // This only works if its _vertexList identifies a unique path in the Graph
        if (_edgeList == null && !other.Graph.Type.IsAllowingMultipleEdges)
        {
            return _vertexList!.SequenceEqual(other.VertexList);
        }
        else
        {
            // Unlucky, we need to compare the edge lists
            return EdgeList.SequenceEqual(other.EdgeList);
        }
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) ||
               (obj is GraphWalk<TVertex, TEdge> other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_vertexList, _edgeList, StartVertex, EndVertex);
    }

    public override string? ToString()
    {
        return _vertexList != null ? _vertexList.ToString() : _edgeList!.ToString();
    }

    /// <summary>
    /// Reverses the direction of the walk. In case of directed/mixed graphs, the arc directions will
    /// be reversed. An exception is thrown if reversing an arc $(u,TVertex)$ is impossible because arc
    /// $(TVertex,u)$ is not present in the Graph. The weight of the resulting walk equals the sum of edge
    /// weights in the walk.
    /// </summary>
    /// <exception cref="InvalidGraphWalkException"> if the path is invalid.</exception>
    /// <returns>a reversed GraphWalk.</returns>
    public GraphWalk<TVertex, TEdge> Reverse()
    {
        return Reverse(null);
    }

    /// <summary>
    /// Reverses the direction of the walk. In case of directed/mixed graphs, the arc directions will
    /// be reversed. An exception is thrown if reversing an arc $(u,TVertex)$ is impossible because arc
    /// $(TVertex,u)$ is not present in the Graph.
    /// </summary>
    /// <param name="walkWeightCalculator"> Function used to calculate the weight of the reversed GraphWalk.</param>
    /// <exception cref="InvalidGraphWalkException"> if the path is invalid.</exception>
    /// <returns>a reversed GraphWalk.</returns>
    public GraphWalk<TVertex, TEdge> Reverse(Func<GraphWalk<TVertex, TEdge>, double>? walkWeightCalculator)
    {
        List<TVertex>? revVertexList = null;
        List<TEdge>?   revEdgeList   = null;
        double         revWeight     = 0;

        if (_vertexList != null)
        {
            revVertexList = new List<TVertex>(_vertexList);
            revVertexList.Reverse();
            if (Graph.Type.IsUndirected)
            {
                revWeight = Weight;
            }

            // Check validity of the path. If the path is invalid, then calculating its weight may
            // result in an undefined exception.
            // If an _edgeList is provided, then this check can be postponed to the construction of
            // the reversed edge list
            if (!Graph.Type.IsUndirected && _edgeList == null)
            {
                for (var i = 0; i < revVertexList.Count - 1; i++)
                {
                    var u    = revVertexList[i];
                    var v    = revVertexList[i + 1];
                    var edge = Graph.GetEdge(u, v);
                    if (edge == null)
                    {
                        throw new InvalidGraphWalkException(
                            $"this walk cannot be reversed. The Graph does not contain a reverse arc for arc {Graph.GetEdge(v, u)}"
                        );
                    }
                    else
                    {
                        revWeight += Graph.GetEdgeWeight(edge);
                    }
                }
            }
        }

        if (_edgeList != null)
        {
            revEdgeList = new List<TEdge>(_edgeList.Count);

            if (Graph.Type.IsUndirected)
            {
                revEdgeList.AddRange(_edgeList);
                revEdgeList.Reverse();
                revWeight = Weight;
            }
            else
            {
                foreach (var edge in _edgeList.Reverse())
                {
                    var u       = Graph.GetEdgeSource(edge);
                    var v       = Graph.GetEdgeTarget(edge);
                    var revEdge = Graph.GetEdge(v, u);
                    if (revEdge == null)
                    {
                        throw new InvalidGraphWalkException(
                            $"this walk cannot be reversed. The Graph does not contain a reverse arc for arc {edge}"
                        );
                    }

                    revEdgeList.Add(revEdge);
                    revWeight += Graph.GetEdgeWeight(revEdge);
                }
            }
        }

        // Update weight of reversed walk
        var gw = new GraphWalk<TVertex, TEdge>(Graph, EndVertex, StartVertex, revVertexList, revEdgeList, 0);
        gw.Weight = walkWeightCalculator?.Invoke(gw) ?? revWeight;

        return gw;
    }

    /// <summary>
    /// Concatenates the specified GraphWalk to the end of this GraphWalk. This action can only be
    /// performed if the end vertex of this GraphWalk is the same as the start vertex of the
    /// extending GraphWalk.
    /// </summary>
    /// <param name="extension"> GraphPath used for the concatenation.</param>
    /// <param name="walkWeightCalculator">
    /// Function used to calculate the weight of the GraphWalk obtained after the concatenation.
    /// </param>
    /// <returns>
    /// a GraphWalk that represents the concatenation of this object's walk followed by the
    /// walk specified in the extension argument.
    /// </returns>
    public GraphWalk<TVertex, TEdge> Concat(
        GraphWalk<TVertex, TEdge>               extension,
        Func<GraphWalk<TVertex, TEdge>, double> walkWeightCalculator
    )
    {
        ArgumentNullException.ThrowIfNull(extension);
        ArgumentNullException.ThrowIfNull(walkWeightCalculator);

        if (Empty)
        {
            throw new ArgumentException("An empty path cannot be extended", nameof(extension));
        }

        if (!Equals(EndVertex, extension.StartVertex))
        {
            throw new ArgumentException(
                "This path can only be extended by another path if the end vertex of the orginal path and the start vertex of the extension are equal.",
                nameof(extension)
            );
        }

        List<TVertex>? concatVertexList = null;
        List<TEdge>?   concatEdgeList   = null;

        if (_vertexList != null)
        {
            concatVertexList = new List<TVertex>(_vertexList);
            var vertexListExtension = extension.VertexList;
            concatVertexList.AddRange(vertexListExtension.Skip(1));
        }

        if (_edgeList != null)
        {
            concatEdgeList = new List<TEdge>(_edgeList);
            concatEdgeList.AddRange(extension.EdgeList);
        }

        var gw = new GraphWalk<TVertex, TEdge>(
            Graph,
            StartVertex,
            extension.EndVertex,
            concatVertexList,
            concatEdgeList,
            0
        );
        gw.Weight = walkWeightCalculator(gw);
        return gw;
    }

    /// <summary>
    /// Convenience method which verifies whether the given path is feasible wrt the input Graph and
    /// forms an actual path.
    /// </summary>
    /// <exception cref="InvalidGraphWalkException"> if the path is invalid.</exception>
    public void Verify()
    {
        if (Empty)
        {
            return;
        }

        if (_vertexList != null && _vertexList.Count > 0)
        {
            if (!Equals(StartVertex, _vertexList[0]))
            {
                throw new InvalidGraphWalkException("The start vertex must be the first vertex in the vertex list");
            }

            if (!Equals(EndVertex, _vertexList[^1]))
            {
                throw new InvalidGraphWalkException("The end vertex must be the last vertex in the vertex list");
            }

            // All vertices and edges in the path must be contained in the Graph
            if (!Graph.VertexSet().IsSupersetOf(_vertexList))
            {
                throw new InvalidGraphWalkException("Not all vertices in the path are contained in the Graph");
            }

            if (_edgeList == null)
            {
                // Verify sequence
                foreach (var (previous, current) in _vertexList.Pairwise())
                {
                    if (Graph.GetEdge(previous, current) == null)
                    {
                        throw new InvalidGraphWalkException(
                            $"The _vertexList does not constitute to a feasible path. Edge ({previous},{current}) does not exist in the Graph."
                        );
                    }
                }
            }
        }

        if (_edgeList != null && _edgeList.Count > 0)
        {
            if (!Graphs.TestIncidence(Graph, _edgeList[0], StartVertex!))
            {
                throw new InvalidGraphWalkException("The first edge in the edge list must leave the start vertex");
            }

            if (!Graph.EdgeSet().IsSupersetOf(_edgeList))
            {
                throw new InvalidGraphWalkException("Not all edges in the path are contained in the Graph");
            }

            if (_vertexList == null)
            {
                var u = StartVertex;
                foreach (var edge in _edgeList)
                {
                    if (!Graphs.TestIncidence(Graph, edge, u!))
                    {
                        throw new InvalidGraphWalkException(
                            $"The _edgeList does not constitute to a feasible path. Conflicting edge: {edge}"
                        );
                    }

                    u = Graphs.GetOppositeVertex(Graph, edge, u!);
                }

                if (!Equals(u, EndVertex))
                {
                    throw new InvalidGraphWalkException(
                        "The path defined by the _edgeList does not end in the _endVertex."
                    );
                }
            }
        }

        if (_vertexList != null && _edgeList != null)
        {
            // Verify that the path is an actual path in the Graph
            if (_edgeList.Count + 1 != _vertexList.Count)
            {
                throw new InvalidGraphWalkException(
                    "VertexList and _edgeList do not correspond to the same path (cardinality of _vertexList +1 must equal the cardinality of the _edgeList)"
                );
            }

            for (var i = 0; i < _vertexList.Count - 1; i++)
            {
                var u    = _vertexList[i];
                var v    = _vertexList[i + 1];
                var edge = EdgeList[i];

                if (Graph.Type.IsDirected)
                {
                    // Directed Graph
                    if (!Graph.GetEdgeSource(edge).Equals(u) || !Graph.GetEdgeTarget(edge).Equals(v))
                    {
                        throw new InvalidGraphWalkException("VertexList and _edgeList do not form a feasible path");
                    }
                }
                else
                {
                    // Undirected or mixed
                    if (!Graphs.TestIncidence(Graph, edge, u) || !Graphs.GetOppositeVertex(Graph, edge, u).Equals(v))
                    {
                        throw new InvalidGraphWalkException("VertexList and _edgeList do not form a feasible path");
                    }
                }
            }
        }
    }
}
