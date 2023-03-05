using System.Diagnostics;

/*
 * (C) Copyright 2017-2021, by Joris Kinable and Contributors.
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
namespace NGraphT.Core;

using Util;

/// <summary>
/// Collection of methods which provide numerical graph information.
///
/// <remarks>Author: Joris Kinable.</remarks>
/// <remarks>Author: Alexandru Valeanu.</remarks>
/// </summary>
public abstract class GraphMetrics
{
    /// <summary>
    /// Compute the <a href="http://mathworld.wolfram.com/GraphDiameter.html">diameter</a> of the
    /// graph. The diameter of a graph is defined as $\max_{TNode\in TNode}\epsilon(TNode)$, where $\epsilon(TNode)$
    /// is the eccentricity of vertex $v$. In other words, this method computes the 'longest shortest
    /// path'. Two special cases exist. If the graph has no vertices, the diameter is 0. If the graph
    /// is disconnected, the diameter is <#### cref="Double.POSITIVE_INFINITY"/>.
    /// <para>
    /// For more fine-grained control over this method, or if you need additional distance metrics
    /// such as the graph radius, consider using <#### cref="NGraphT.Core.alg.shortestpath.GraphMeasurer"/>
    /// instead.
    /// </para>
    ///
    /// </summary>
    /// <param name="graph"> input graph.</param>
    /// @param <TNode> graph vertex type.</param>
    /// @param <TEdge> graph edge type.</param>
    /// <returns>the diameter of the graph.</returns>
    public static double GetDiameter<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        return (new GraphMeasurer<>(graph)).Diameter;
    }

    /// <summary>
    /// Compute the <a href="http://mathworld.wolfram.com/GraphRadius.html">radius</a> of the graph.
    /// The radius of a graph is defined as $\min_{TNode\in TNode}\epsilon(TNode)$, where $\epsilon(TNode)$ is the
    /// eccentricity of vertex $v$. Two special cases exist. If the graph has no vertices, the radius
    /// is 0. If the graph is disconnected, the diameter is <#### cref="Double.POSITIVE_INFINITY"/>.
    /// <para>
    /// For more fine-grained control over this method, or if you need additional distance metrics
    /// such as the graph diameter, consider using <#### cref="NGraphT.Core.alg.shortestpath.GraphMeasurer"/>
    /// instead.
    /// </para>
    ///
    /// </summary>
    /// <param name="graph"> input graph.</param>
    /// @param <TNode> graph vertex type.</param>
    /// @param <TEdge> graph edge type.</param>
    /// <returns>the diameter of the graph.</returns>
    public static double GetRadius<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        return (new GraphMeasurer<>(graph)).Radius;
    }

    /// <summary>
    /// Compute the <a href="http://mathworld.wolfram.com/Girth.html">girth</a> of the graph. The
    /// girth of a graph is the length (number of edges) of the smallest cycle in the graph. Acyclic
    /// graphs are considered to have infinite girth. For directed graphs, the length of the shortest
    /// directed cycle is returned (see Bang-Jensen, J., Gutin, G., Digraphs: Theory, Algorithms and
    /// Applications, Springer Monographs in Mathematics, ch 1, ch 8.4.). Simple undirected graphs
    /// have a girth of at least 3 (triangle cycle). Directed graphs and Multigraphs have a girth of
    /// at least 2 (parallel edges/arcs), and in Pseudo graphs have a girth of at least 1
    /// (self-loop).
    /// <para>
    /// This implementation is loosely based on these <a href=
    /// "http://webcourse.cs.technion.ac.il/234247/Winter2003-2004/ho/WCFiles/Girth.pdf">notes</a>.
    /// In essence, this method invokes a Breadth-First search from every vertex in the graph. A
    /// single Breadth-First search takes $O(n+m)$ time, where $n$ is the number of vertices in the
    /// graph, and $m$ the number of edges. Consequently, the runtime complexity of this method is
    /// $O(n(n+m))=O(mn)$.
    /// </para>
    /// <para>
    /// An algorithm with the same worst case runtime complexity, but a potentially better average
    /// runtime complexity of $O(n^2)$ is described in: Itai, A. Rodeh, M. Finding a minimum circuit
    /// in a graph. SIAM J. Comput. Vol 7, No 4, 1987.
    /// </para>
    ///
    /// </summary>
    /// <param name="graph"> input graph.</param>
    /// @param <TNode> graph vertex type.</param>
    /// @param <TEdge> graph edge type.</param>
    /// <returns>girth of the graph, or <#### cref="Integer.MAX_VALUE"/> if the graph is acyclic.</returns>
    public static int GetGirth<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        const int Nil = -1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean isAllowingMultipleEdges = graph.getType().isAllowingMultipleEdges();
        var isAllowingMultipleEdges = graph.Type.AllowingMultipleEdges;

        // Ordered sequence of vertices
        IList<TNode> vertices = new List<TNode>(graph.VertexSet());
        // Index map of vertices in ordered sequence
        IDictionary<TNode, int> indexMap = new Dictionary<TNode, int>();
        for (var i = 0; i < vertices.Count; i++)
        {
            indexMap[vertices[i]] = i;
        }

        // Objective
        var girth = int.MaxValue;
        // Array storing the depth of each vertex in the search tree
        var depth = new int[vertices.Count];
        // Queue for BFS
        LinkedList<TNode> queue = new LinkedList<TNode>();

        // Check whether the graph has self-loops
        if (graph.Type.AllowingSelfLoops)
        {
            foreach (var node in vertices)
            {
                if (graph.ContainsEdge(node, node))
                {
                    return 1;
                }
            }
        }

        NeighborCache<TNode, TEdge> neighborIndex = new NeighborCache<TNode, TEdge>(graph);

        if (graph.Type.Undirected)
        {
            // Array which keeps track of the search tree structure to prevent revisiting parent
            // nodes
            var parent = new int[vertices.Count];

            // Start a BFS search tree from each vertex. The search stops when a triangle (smallest
            // possible cycle) is found.
            // The last two vertices can be ignored.
            for (var i = 0; i < vertices.Count - 2 && girth > 3; i++)
            {
                // Reset data structures
                Arrays.Fill(depth,  Nil);
                Arrays.Fill(parent, Nil);
                queue.Clear();

                depth[i] = 0;
                queue.AddLast(vertices[i]);
                int depthU;

                do
                {
                    TNode   u      = queue.RemoveFirst();
                    var indexU = indexMap[u];
                    depthU = depth[indexU];

                    // Visit all neighbors of vertex u
                    foreach (TNode node in neighborIndex.NeighborsOf(u))
                    {
                        var indexV = indexMap[node];

                        if (parent[indexU] == indexV)
                        {
                            // Skip the parent of vertex u, unless there
                            // are multiple edges between u and TNode
                            if (!isAllowingMultipleEdges || graph.GetAllEdges(u, node).Count == 1)
                            {
                                continue;
                            }
                        }

                        var depthV = depth[indexV];
                        if (depthV == Nil)
                        {
                            // New neighbor discovered
                            queue.AddLast(node);
                            depth[indexV]  = depthU + 1;
                            parent[indexV] = indexU;
                        }
                        else
                        {
                            // Rediscover neighbor: found cycle.
                            girth = Math.Min(girth, depthU + depthV + 1);
                        }
                    }
                } while (queue.Count > 0 && 2 * (depthU + 1) - 1 < girth);
            }
        }
        else
        {
            // Directed case
            for (var i = 0; i < vertices.Count - 1 && girth > 2; i++)
            {
                // Reset data structures
                Arrays.Fill(depth, Nil);
                queue.Clear();

                depth[i] = 0;
                queue.AddLast(vertices[i]);
                int depthU;

                do
                {
                    TNode   u      = queue.RemoveFirst();
                    var indexU = indexMap[u];
                    depthU = depth[indexU];

                    // Visit all neighbors of vertex u
                    foreach (TNode node in neighborIndex.SuccessorsOf(u))
                    {
                        var indexV = indexMap[node];

                        var depthV = depth[indexV];
                        if (depthV == Nil)
                        {
                            // New neighbor discovered
                            queue.AddLast(node);
                            depth[indexV] = depthU + 1;
                        }
                        else if (depthV == 0)
                        {
                            // Rediscover root: found cycle.
                            girth = Math.Min(girth, depthU + depthV + 1);
                        }
                    }
                } while (queue.Count > 0 && depthU + 1 < girth);
            }
        }

        Debug.Assert(graph.Type.Undirected        && graph.Type.Simple && girth >= 3 ||
                     graph.Type.AllowingSelfLoops && girth >= 1 ||
                     girth >= 2 && (graph.Type.Directed || graph.Type.AllowingMultipleEdges)
        );
        return girth;
    }

    /// <summary>
    /// An $O(|TNode|^3)$ (assuming vertexSubset provides constant time indexing) naive implementation
    /// for counting non-trivial triangles in an undirected graph induced by the subset of vertices.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <param name="vertexSubset"> the vertex subset.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>the number of triangles in the graph induced by vertexSubset.</returns>
    internal static long NaiveCountTriangles<TNode, TEdge>(IGraph<TNode, TEdge> graph, IList<TNode> vertexSubset)
    {
        long total = 0;

        if (graph.Type.AllowingMultipleEdges)
        {
            for (var i = 0; i < vertexSubset.Count; i++)
            {
                for (var j = i + 1; j < vertexSubset.Count; j++)
                {
                    for (var k = j + 1; k < vertexSubset.Count; k++)
                    {
                        var u = vertexSubset[i];
                        var node = vertexSubset[j];
                        var w = vertexSubset[k];

                        var uvEdgeCount = graph.GetAllEdges(u, node).Count;
                        if (uvEdgeCount == 0)
                        {
                            continue;
                        }

                        var vwEdgeCount = graph.GetAllEdges(node, w).Count;
                        if (vwEdgeCount == 0)
                        {
                            continue;
                        }

                        var wuEdgeCount = graph.GetAllEdges(w, u).Count;
                        if (wuEdgeCount == 0)
                        {
                            continue;
                        }

                        total += uvEdgeCount * vwEdgeCount * wuEdgeCount;
                    }
                }
            }
        }
        else
        {
            for (var i = 0; i < vertexSubset.Count; i++)
            {
                for (var j = i + 1; j < vertexSubset.Count; j++)
                {
                    for (var k = j + 1; k < vertexSubset.Count; k++)
                    {
                        var u = vertexSubset[i];
                        var node = vertexSubset[j];
                        var w = vertexSubset[k];

                        if (graph.ContainsEdge(u, node) && graph.ContainsEdge(node, w) && graph.ContainsEdge(w, u))
                        {
                            total++;
                        }
                    }
                }
            }
        }

        return total;
    }

    /// <summary>
    /// An $O(|TEdge|^{3/2})$ algorithm for counting the number of non-trivial triangles in an undirected
    /// graph. A non-trivial triangle is formed by three distinct vertices all connected to each
    /// other.
    ///
    /// <para>
    /// For more details of this algorithm see Ullman, Jeffrey: "Mining of Massive Datasets",
    /// Cambridge University Press, Chapter 10
    /// </para>
    ///
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>the number of triangles in the graph.</returns>
    /// <exception cref="NullReferenceException"> if {@code graph} is {@code null} </exception>
    /// <exception cref="ArgumentException"> if {@code graph} is not undirected.</exception>
    public static long GetNumberOfTriangles<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        GraphTests.RequireUndirected(graph);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int sqrtV = (int) Math.sqrt(graph.vertexSet().size());
        var sqrtV = (int)Math.Sqrt(graph.VertexSet().Count);

        IList<TNode> vertexList = new List<TNode>(graph.VertexSet());

        /*
         * The book suggest the following comparator: "Compare vertices based on their degree. If
         * equal compare them of their actual value, since they are all integers".
         */

        // Fix vertex order for unique comparison of vertices
        IDictionary<TNode, int> vertexOrder = CollectionUtil.NewHashMapWithExpectedSize(graph.VertexSet().Count);
        var                 k           = 0;
        foreach (var node in graph.VertexSet())
        {
            vertexOrder[node] = k++;
        }

        IComparer<TNode> comparator = Comparator.comparingInt(graph.degreeOf).thenComparingInt(System.identityHashCode)
            .thenComparingInt(vertexOrder.get);

        vertexList.Sort(comparator);

        // vertex TNode is a heavy-hitter iff degree(TNode) >= sqrtV
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
        IList<TNode> heavyHitterVertices = vertexList.stream().filter(x => graph.DegreeOf(x) >= sqrtV)
            .collect(Collectors.toCollection(ArrayList::new));

        // count the number of triangles formed from only heavy-hitter vertices
        var numberTriangles = NaiveCountTriangles(graph, heavyHitterVertices);

        foreach (var edge in graph.EdgeSet())
        {
            var v1 = graph.GetEdgeSource(edge);
            var v2 = graph.GetEdgeTarget(edge);

            if (v1 == v2)
            {
                continue;
            }

            if (graph.DegreeOf(v1) < sqrtV || graph.DegreeOf(v2) < sqrtV)
            {
                // ensure that v1 <= v2 (swap them otherwise)
                if (comparator.Compare(v1, v2) > 0)
                {
                    var tmp = v1;
                    v1 = v2;
                    v2 = tmp;
                }

                foreach (var edge in graph.EdgesOf(v1))
                {
                    var u = Graphs.GetOppositeVertex(graph, edge, v1);

                    // check if the triangle is non-trivial: u, v1, v2 are distinct vertices
                    if (u == v1 || u == v2)
                    {
                        continue;
                    }

                    /*
                     * Check if v2 <= u and if (u, v2) is a valid edge. If both of them are true,
                     * then we have a new triangle (v1, v2, u) and all three vertices in the
                     * triangle are ordered (v1 <= v2 <= u) so we count it only once.
                     */
                    if (comparator.Compare(v2, u) <= 0 && graph.ContainsEdge(u, v2))
                    {
                        numberTriangles++;
                    }
                }
            }
        }

        return numberTriangles;
    }
}
