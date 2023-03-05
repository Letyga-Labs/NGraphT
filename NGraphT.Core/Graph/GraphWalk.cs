/*
 * (C) Copyright 2016-2021, by Joris Kinable and Contributors.
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
/// A walk in a graph is an alternating sequence of vertices and edges, starting and ending at a
/// vertex, in which each edge is adjacent in the sequence to its two endpoints. More precisely, a
/// walk is a connected sequence of vertices and edges in a graph $v_0, e_0, v_1, e_1, v_2, \dotso,
/// v_{k-1}, e_{k-1}, v_{k}$, such that for $1 \leq i \leq k$, the edge $e_i$ has endpoints $v_{i-1}$
/// and $v_i$. The class makes no assumptions with respect to the shape of the walk: edges may be
/// repeated, and the start and end point of the walk may be different.
///
/// <para>
/// See <a href="http://mathworld.wolfram.com/Walk.html">http://mathworld.wolfram.com/Walk.html</a>
/// </para>
/// <para>
/// GraphWalk is the default implementation of <see cref="GraphPath"/>.
/// </para>
/// <para>
/// Two special cases exist:
/// <ol>
/// <li>A singleton GraphWalk has an empty edge list (the length of the path equals 0), the vertex
/// list contains a single vertex TNode, and the start and end vertex equal TNode.</li>
/// <li>An empty Graphwalk has empty edge and vertex lists, and the start and end vertex are both
/// null.</li>
/// </ol>
/// </para>
/// <para>
/// This class is implemented as a light-weight data structure; this class does not verify whether
/// the sequence of edges or the sequence of vertices provided during construction forms an actual
/// walk. It is the responsibility of the invoking class to provide correct input data.
/// </para>
/// <para>
/// Note: Serialization of a GraphWalk implies the serialization of the entire underlying graph.
/// </para>
/// </summary>
///
/// <typeparam name="TNode">The graph vertex type.</typeparam>
/// <typeparam name="TEdge">The graph edge type.</typeparam>
///
/// <remarks>Author: Joris Kinable.</remarks>
///  </param>
public class GraphWalk<TNode, TEdge> : IGraphPath<TNode, TEdge>
{
    private const      long                 SerialVersionUID = 7663410644865380676L;
    protected internal IGraph<TNode, TEdge> Graph;

    protected internal IList<TNode> VertexList;
    protected internal IList<TEdge> EdgeList;

    protected internal TNode StartVertex;

    protected internal TNode EndVertex;

    protected internal double Weight;

    ///<summary>
    ///Creates a walk defined by a sequence of edges. A walk defined by its edges can be specified
    ///for non-simple graphs. Edge repetition is permitted, the start and end point points ($v_0$
    ///and $v_k$) can be different.
    ///</summary>
    ///<param name="graph"> the graph.</param>
    ///<param name="startVertex"> the starting vertex.</param>
    ///<param name="endVertex"> the last vertex of the path.</param>
    ///<param name="edgeList"> the list of edges of the path.</param>
    ///<param name="weight"> the total weight of the path.</param>
    public GraphWalk(
        IGraph<TNode, TEdge> graph,
        TNode                startVertex,
        TNode                endVertex,
        IList<TEdge>         edgeList,
        double               weight
    )
        : this(graph, startVertex, endVertex, null, edgeList, weight)
    {
    }

    ///<summary>
    ///Creates a walk defined by a sequence of vertices. Note that the input graph must be simple,
    ///otherwise the vertex sequence does not necessarily define a unique path. Furthermore, all
    ///vertices must be pairwise adjacent.
    ///</summary>
    ///<param name="graph"> the graph.</param>
    ///<param name="vertexList"> the list of vertices of the path.</param>
    ///<param name="weight"> the total weight of the path.</param>
    public GraphWalk(IGraph<TNode, TEdge> graph, IList<TNode> vertexList, double weight)
        : this(graph,
            (vertexList.Count == 0 ? null : vertexList[0]),
            (vertexList.Count == 0 ? null : vertexList[vertexList.Count - 1]),
            vertexList,
            null,
            weight
        )
    {
    }

    ///<summary>
    ///Creates a walk defined by both a sequence of edges and a sequence of vertices. Note that both
    ///the sequence of edges and the sequence of vertices must describe the same path! This is not
    ///verified during the construction of the walk. This constructor makes it possible to store
    ///both a vertex and an edge view of the same walk, thereby saving computational overhead when
    ///switching from one to the other.
    ///</summary>
    ///<param name="graph"> the graph.</param>
    ///<param name="startVertex"> the starting vertex.</param>
    ///<param name="endVertex"> the last vertex of the path.</param>
    ///<param name="vertexList"> the list of vertices of the path.</param>
    ///<param name="edgeList"> the list of edges of the path.</param>
    ///<param name="weight"> the total weight of the path.</param>
    public GraphWalk(
        IGraph<TNode, TEdge> graph,
        TNode                startVertex,
        TNode                endVertex,
        IList<TNode>         vertexList,
        IList<TEdge>         edgeList,
        double               weight
    )
    {
        // Some necessary but not sufficient conditions for valid paths
        if (vertexList == null && edgeList == null)
        {
            throw new ArgumentException("Vertex list and edge list cannot both be null!");
        }

        if (startVertex != null && vertexList != null && edgeList != null && edgeList.Count + 1 != vertexList.Count)
        {
            throw new ArgumentException(
                "VertexList and edgeList do not correspond to the same path (cardinality of vertexList +1 must equal the cardinality of the edgeList)"
            );
        }

        if (startVertex == null ^ endVertex == null)
        {
            throw new ArgumentException(
                "Either the start and end vertices must both be null, or they must both be not null (one of them is null)"
            );
        }

        this.graph       = Objects.requireNonNull(graph);
        this.startVertex = startVertex;
        this.endVertex   = endVertex;
        this.vertexList  = vertexList;
        this.edgeList    = edgeList;
        this.weight      = weight;
    }

    public virtual IGraph<TNode, TEdge> Graph
    {
        get
        {
            return graph;
        }
    }

    public virtual TNode StartVertex
    {
        get
        {
            return startVertex;
        }
    }

    public virtual TNode EndVertex
    {
        get
        {
            return endVertex;
        }
    }

    public virtual IList<TEdge> EdgeList
    {
        get
        {
            return (edgeList != null ? edgeList : GraphPath.this.getEdgeList());
        }
    }

    public virtual IList<TNode> VertexList
    {
        get
        {
            return (vertexList != null ? vertexList : GraphPath.this.getVertexList());
        }
    }

    public virtual double Weight
    {
        get
        {
            return weight;
        }
        set
        {
            this.weight = value;
        }
    }


    public virtual int Length
    {
        get
        {
            if (edgeList != null)
            {
                return edgeList.Count;
            }
            else if (vertexList != null && vertexList.Count > 0)
            {
                return vertexList.Count - 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public override string ToString()
    {
        if (vertexList != null)
        {
            return vertexList.ToString();
        }
        else
        {
            return edgeList.ToString();
        }
    }

    public override bool Equals(object o)
    {
        if (o == null || !(o is GraphWalk))
        {
            return false;
        }
        else if (this == o)
        {
            return true;
        }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") GraphWalk<TNode, TEdge> other = (GraphWalk<TNode, TEdge>) o;
        var other = (GraphWalk<TNode, TEdge>)o;
        if (Empty && other.Empty)
        {
            return true;
        }

        if (Empty)
        {
            return false;
        }

        if (!this.startVertex.Equals(other.StartVertex) || !this.endVertex.Equals(other.EndVertex))
        {
            return false;
        }

        // If this path is expressed as a vertex list, we may get away by comparing the other path's
        // vertex list
        // This only works if its vertexList identifies a unique path in the graph
        if (this.edgeList == null && !other.Graph.Type.AllowingMultipleEdges)
        {
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: return this.vertexList.equals(other.getVertexList());
            return this.vertexList.SequenceEqual(other.VertexList);
        }
        else // Unlucky, we need to compare the edge lists,
        {
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: return this.getEdgeList().equals(other.getEdgeList());
            return this.EdgeList.SequenceEqual(other.EdgeList);
        }
    }

    public override int GetHashCode()
    {
        var hashCode = 1;
        if (Empty)
        {
            return hashCode;
        }

        hashCode = 31 * hashCode + startVertex.GetHashCode();
        hashCode = 31 * hashCode + endVertex.GetHashCode();

        if (edgeList != null)
        {
            return 31 * hashCode + edgeList.GetHashCode();
        }
        else
        {
            return 31 * hashCode + vertexList.GetHashCode();
        }
    }

    ///<summary>
    ///Reverses the direction of the walk. In case of directed/mixed graphs, the arc directions will
    ///be reversed. An exception is thrown if reversing an arc $(u,TNode)$ is impossible because arc
    ///$(TNode,u)$ is not present in the graph. The weight of the resulting walk equals the sum of edge
    ///weights in the walk.
    ///</summary>
    ///<exception cref="InvalidGraphWalkException"> if the path is invalid.</exception>
    ///<returns>a reversed GraphWalk.</returns>
    public virtual GraphWalk<TNode, TEdge> Reverse()
    {
        return Reverse(null);
    }

    /// <summary>
    /// Reverses the direction of the walk. In case of directed/mixed graphs, the arc directions will
    /// be reversed. An exception is thrown if reversing an arc $(u,TNode)$ is impossible because arc
    /// $(TNode,u)$ is not present in the graph.
    /// </summary>
    /// <param name="walkWeightCalculator"> Function used to calculate the weight of the reversed GraphWalk.</param>
    /// <exception cref="InvalidGraphWalkException"> if the path is invalid.</exception>
    /// <returns>a reversed GraphWalk.</returns>
    public virtual GraphWalk<TNode, TEdge> Reverse(Func<GraphWalk<TNode, TEdge>, double> walkWeightCalculator)
    {
        IList<TNode> revVertexList = null;
        IList<TEdge> revEdgeList   = null;
        double       revWeight     = 0;

        if (vertexList != null)
        {
            revVertexList = new List<TNode>(this.vertexList);
            revVertexList.Reverse();
            if (graph.Type.isUndirected())
            {
                revWeight = this.weight;
            }

            // Check validity of the path. If the path is invalid, then calculating its weight may
            // result in an undefined exception.
            // If an edgeList is provided, then this check can be postponed to the construction of
            // the reversed edge list
            if (!graph.Type.isUndirected() && edgeList == null)
            {
                for (var i = 0; i < revVertexList.Count - 1; i++)
                {
                    var u    = revVertexList[i];
                    var node = revVertexList[i + 1];
                    TEdge edge = graph.GetEdge(u, node);
                    if (edge == null)
                    {
                        throw new InvalidGraphWalkException(
                            "this walk cannot be reversed. The graph does not contain a reverse arc for arc " +
                            graph.GetEdge(node, u)
                        );
                    }
                    else
                    {
                        revWeight += graph.GetEdgeWeight(edge);
                    }
                }
            }
        }

        if (edgeList != null)
        {
            revEdgeList = new List<TEdge>(this.edgeList.Count);

            if (graph.Type.isUndirected())
            {
                ((List<TEdge>)revEdgeList).AddRange(this.edgeList);
                revEdgeList.Reverse();
                revWeight = this.weight;
            }
            else
            {
                IEnumerator<TEdge> listIterator = this.edgeList.listIterator(edgeList.Count);
                while (listIterator.hasPrevious())
                {
                    TEdge edge    = listIterator.previous();
                    TNode u       = graph.GetEdgeSource(edge);
                    TNode node    = graph.GetEdgeTarget(edge);
                    TEdge revEdge = graph.GetEdge(node, u);
                    if (revEdge == null)
                    {
                        throw new InvalidGraphWalkException(
                            "this walk cannot be reversed. The graph does not contain a reverse arc for arc " + edge
                        );
                    }

                    revEdgeList.Add(revEdge);
                    revWeight += graph.GetEdgeWeight(revEdge);
                }
            }
        }

        // Update weight of reversed walk
        var gw =
            new GraphWalk<TNode, TEdge>(this.graph, this.endVertex, this.startVertex, revVertexList, revEdgeList, 0);
        if (walkWeightCalculator == null)
        {
            gw.weight = revWeight;
        }
        else
        {
            gw.weight = walkWeightCalculator(gw);
        }

        return gw;
    }

    /// <summary>
    /// Concatenates the specified GraphWalk to the end of this GraphWalk. This action can only be
    /// performed if the end vertex of this GraphWalk is the same as the start vertex of the
    /// extending GraphWalk.
    /// </summary>
    /// <param name="extension"> GraphPath used for the concatenation.</param>
    /// <param name="walkWeightCalculator"> Function used to calculate the weight of the GraphWalk obtained
    ///        after the concatenation.</param>
    /// <returns>a GraphWalk that represents the concatenation of this object's walk followed by the
    ///         walk specified in the extension argument.</returns>
    public virtual GraphWalk<TNode, TEdge> Concat(
        GraphWalk<TNode, TEdge>                      extension,
        Func<GraphWalk<TNode, TEdge>, double> walkWeightCalculator
    )
    {
        if (Empty)
        {
            throw new ArgumentException("An empty path cannot be extended");
        }

        if (!this.endVertex.Equals(extension.StartVertex))
        {
            throw new ArgumentException(
                "This path can only be extended by another path if the end vertex of the orginal path and the start vertex of the extension are equal."
            );
        }

        IList<TNode> concatVertexList = null;
        IList<TEdge> concatEdgeList   = null;

        if (vertexList != null)
        {
            concatVertexList = new List<TNode>(this.vertexList);
            IList<TNode> vertexListExtension = extension.VertexList;
            ((List<TNode>)concatVertexList).AddRange(vertexListExtension.subList(1, vertexListExtension.Count));
        }

        if (edgeList != null)
        {
            concatEdgeList = new List<TEdge>(this.edgeList);
            ((List<TEdge>)concatEdgeList).AddRange(extension.EdgeList);
        }

        var gw = new GraphWalk<TNode, TEdge>(this.graph,
            startVertex,
            extension.EndVertex,
            concatVertexList,
            concatEdgeList,
            0
        );
        gw.Weight = walkWeightCalculator(gw);
        return gw;
    }

    /// <summary>
    /// Returns true if the path is an empty path, that is, a path with startVertex=endVertex=null
    /// and with an empty vertex and edge list.
    /// </summary>
    /// <returns>Returns true if the path is an empty path.</returns>
    public virtual bool Empty
    {
        get
        {
            return startVertex == null;
        }
    }

    /// <summary>
    /// Convenience method which verifies whether the given path is feasible wrt the input graph and
    /// forms an actual path.
    /// </summary>
    /// <exception cref="InvalidGraphWalkException"> if the path is invalid.</exception>
    public virtual void Verify()
    {
        if (Empty) // Empty path
        {
            return;
        }

        if (vertexList != null && vertexList.Count > 0)
        {
            if (!startVertex.Equals(vertexList[0]))
            {
                throw new InvalidGraphWalkException("The start vertex must be the first vertex in the vertex list");
            }

            if (!endVertex.Equals(vertexList[vertexList.Count - 1]))
            {
                throw new InvalidGraphWalkException("The end vertex must be the last vertex in the vertex list");
            }

            // All vertices and edges in the path must be contained in the graph
            if (!graph.VertexSet().ContainsAll(vertexList))
            {
                throw new InvalidGraphWalkException("Not all vertices in the path are contained in the graph");
            }

            if (edgeList == null)
            {
                // Verify sequence
                IEnumerator<TNode> it = vertexList.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                TNode u = it.next();
                while (it.MoveNext())
                {
                    var node = it.Current;
                    if (graph.GetEdge(u, node) == null)
                    {
                        throw new InvalidGraphWalkException(
                            "The vertexList does not constitute to a feasible path. Edge (" + u + "," + node +
                            " does not exist in the graph."
                        );
                    }

                    u = node;
                }
            }
        }

        if (edgeList != null && edgeList.Count > 0)
        {
            if (!Graphs.TestIncidence(graph, edgeList[0], startVertex))
            {
                throw new InvalidGraphWalkException("The first edge in the edge list must leave the start vertex");
            }

            if (!graph.EdgeSet().ContainsAll(edgeList))
            {
                throw new InvalidGraphWalkException("Not all edges in the path are contained in the graph");
            }

            if (vertexList == null)
            {
                TNode u = startVertex;
                foreach (TEdge edge in edgeList)
                {
                    if (!Graphs.TestIncidence(graph, edge, u))
                    {
                        throw new InvalidGraphWalkException(
                            "The edgeList does not constitute to a feasible path. Conflicting edge: " + edge
                        );
                    }

                    u = Graphs.GetOppositeVertex(graph, edge, u);
                }

                if (!u.Equals(endVertex))
                {
                    throw new InvalidGraphWalkException(
                        "The path defined by the edgeList does not end in the endVertex."
                    );
                }
            }
        }

        if (vertexList != null && edgeList != null)
        {
            // Verify that the path is an actual path in the graph
            if (edgeList.Count + 1 != vertexList.Count)
            {
                throw new InvalidGraphWalkException(
                    "VertexList and edgeList do not correspond to the same path (cardinality of vertexList +1 must equal the cardinality of the edgeList)"
                );
            }

            for (var i = 0; i < vertexList.Count - 1; i++)
            {
                TNode u    = vertexList[i];
                TNode node = vertexList[i + 1];
                TEdge edge = EdgeList[i];

                if (graph.Type.isDirected())
                {
                    // Directed graph
                    if (!graph.GetEdgeSource(edge).Equals(u) || !graph.GetEdgeTarget(edge).Equals(node))
                    {
                        throw new InvalidGraphWalkException("VertexList and edgeList do not form a feasible path");
                    }
                }
                else
                {
                    // Undirected or mixed
                    if (!Graphs.TestIncidence(graph, edge, u) || !Graphs.GetOppositeVertex(graph, edge, u).Equals(node))
                    {
                        throw new InvalidGraphWalkException("VertexList and edgeList do not form a feasible path");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Convenience method which creates an empty walk.
    /// </summary>
    /// <param name="graph"> input graph.</param>
    /// <typeparam name="TNode"> vertex type.</typeparam>
    /// <typeparam name="TEdge"> edge type.</typeparam>
    /// <returns>an empty walk.</returns>
    public static GraphWalk<TNode, TEdge> EmptyWalk<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        return new GraphWalk<TNode, TEdge>(graph,
            default(TNode),
            default(TNode),
            java.util.Collections.emptyList(),
            java.util.Collections.emptyList(),
            0.0
        );
    }

    /// <summary>
    /// Convenience method which creates a walk consisting of a single vertex with weight 0.0.
    /// </summary>
    /// <param name="graph"> input graph.</param>
    /// <param name="node"> single vertex.</param>
    /// <typeparam name="TNode"> vertex type.</typeparam>
    /// <typeparam name="TEdge"> edge type.</typeparam>
    /// <returns>an empty walk.</returns>
    public static GraphWalk<TNode, TEdge> SingletonWalk<TNode, TEdge>(IGraph<TNode, TEdge> graph, TNode node)
    {
        return SingletonWalk(graph, TNode, 0d);
    }

    /// <summary>
    /// Convenience method which creates a walk consisting of a single vertex.
    /// </summary>
    /// <param name="graph"> input graph.</param>
    /// <param name="node"> single vertex.</param>
    /// <param name="weight"> weight of the path.</param>
    /// <typeparam name="TNode"> vertex type.</typeparam>
    /// <typeparam name="TEdge"> edge type.</typeparam>
    /// <returns>an empty walk.</returns>
    public static GraphWalk<TNode, TEdge> SingletonWalk<TNode, TEdge>(
        IGraph<TNode, TEdge> graph,
        TNode                node,
        double               weight
    )
    {
        return new GraphWalk<TNode, TEdge>(graph,
            TNode,
            TNode,
            Collections.singletonList(TNode),
            java.util.Collections.emptyList(),
            weight
        );
    }
}
