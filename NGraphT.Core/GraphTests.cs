/*
 * (C) Copyright 2003-2021, by Barak Naveh, Dimitrios Michail and Contributors.
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

/// <summary>
/// A collection of utilities to test for various graph properties.
///
/// <remarks>Author: Barak Naveh.</remarks>
/// <remarks>Author: Dimitrios Michail.</remarks>
/// <remarks>Author: Joris Kinable.</remarks>
/// <remarks>Author: Alexandru Valeanu.</remarks>
/// </summary>
public abstract class GraphTests
{
    private const string GraphCannotBeNull               = "Graph cannot be null";
    private const string GraphMustBeDirectedOrUndirected = "Graph must be directed or undirected";
    private const string GraphMustBeUndirected           = "Graph must be undirected";
    private const string GraphMustBeDirected             = "Graph must be directed";
    private const string GraphMustBeWeighted             = "Graph must be weighted";

    /// <summary>
    /// Test whether a graph is empty. An empty graph on n nodes consists of n isolated vertices with
    /// no edges.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is empty, false otherwise.</returns>
    public static bool IsEmpty<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);
        return graph.EdgeSet().Count == 0;
    }

    /// <summary>
    /// Check if a graph is simple. A graph is simple if it has no self-loops and multiple (parallel)
    /// edges.
    /// </summary>
    /// <param name="graph"> a graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if a graph is simple, false otherwise.</returns>
    public static bool IsSimple<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);

        var type = graph.Type;
        if (type.Simple)
        {
            return true;
        }

        // no luck, we have to check
        foreach (var node in graph.VertexSet())
        {
            ISet<TNode> neighbors = new HashSet<TNode>();
            foreach (var edge in graph.OutgoingEdgesOf(node))
            {
                var u = Graphs.GetOppositeVertex(graph, edge, node);
                if (u.Equals(node) || !neighbors.Add(u))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Check if a graph has self-loops. A self-loop is an edge with the same source and target
    /// vertices.
    /// </summary>
    /// <param name="graph"> a graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if a graph has self-loops, false otherwise.</returns>
    public static bool HasSelfLoops<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);

        if (!graph.Type.AllowingSelfLoops)
        {
            return false;
        }

        // no luck, we have to check
        foreach (var edge in graph.EdgeSet())
        {
            if (graph.GetEdgeSource(edge).Equals(graph.GetEdgeTarget(edge)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Check if a graph has multiple edges (parallel edges), that is, whether the graph contains two
    /// or more edges connecting the same pair of vertices.
    /// </summary>
    /// <param name="graph"> a graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if a graph has multiple edges, false otherwise.</returns>
    public static bool HasMultipleEdges<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);

        if (!graph.Type.AllowingMultipleEdges)
        {
            return false;
        }

        // no luck, we have to check
        foreach (var node in graph.VertexSet())
        {
            ISet<TNode> neighbors = new HashSet<TNode>();
            foreach (var edge in graph.OutgoingEdgesOf(node))
            {
                var u = Graphs.GetOppositeVertex(graph, edge, node);
                if (!neighbors.Add(u))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Test whether a graph is complete. A complete undirected graph is a simple graph in which
    /// every pair of distinct vertices is connected by a unique edge. A complete directed graph is a
    /// directed graph in which every pair of distinct vertices is connected by a pair of unique
    /// edges (one in each direction).
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is complete, false otherwise.</returns>
    public static bool IsComplete<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);
        var n = graph.VertexSet().Count;
        int allEdges;
        if (graph.Type.Directed)
        {
            allEdges = Math.multiplyExact(n, n - 1);
        }
        else if (graph.Type.Undirected)
        {
            if (n % 2 == 0)
            {
                allEdges = Math.multiplyExact(n / 2, n - 1);
            }
            else
            {
                allEdges = Math.multiplyExact(n, (n - 1) / 2);
            }
        }
        else
        {
            throw new ArgumentException(GraphMustBeDirectedOrUndirected);
        }

        return graph.EdgeSet().Count == allEdges && IsSimple(graph);
    }

    /// <summary>
    /// Test if the inspected graph is connected. A graph is connected when, while ignoring edge
    /// directionality, there exists a path between every pair of vertices. In a connected graph,
    /// there are no unreachable vertices. When the inspected graph is a <i>directed</i> graph, this
    /// method returns true if and only if the inspected graph is <i>weakly</i> connected. An empty
    /// graph is <i>not</i> considered connected.
    ///
    /// <para>
    /// This method does not performing any caching, instead recomputes everything from scratch. In
    /// case more control is required use <seealso cref="ConnectivityInspector"/> directly.
    ///
    /// </para>
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is connected, false otherwise.</returns>
    /// <seealso cref="ConnectivityInspector"/>
    public static bool IsConnected<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);
        return (new ConnectivityInspector<>(graph)).Connected;
    }

    /// <summary>
    /// Tests if the inspected graph is biconnected. A biconnected graph is a connected graph on two
    /// or more vertices having no cutpoints.
    ///
    /// <para>
    /// This method does not performing any caching, instead recomputes everything from scratch. In
    /// case more control is required use
    /// <seealso cref="NGraphT.Core.alg.connectivity.BiconnectivityInspector"/> directly.
    ///
    /// </para>
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is biconnected, false otherwise.</returns>
    /// <seealso cref="NGraphT.Core.alg.connectivity.BiconnectivityInspector"/>
    public static bool IsBiconnected<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);
        return (new BiconnectivityInspector<>(graph)).Biconnected;
    }

    /// <summary>
    /// Test whether a directed graph is weakly connected.
    ///
    /// <para>
    /// This method does not performing any caching, instead recomputes everything from scratch. In
    /// case more control is required use <seealso cref="ConnectivityInspector"/> directly.
    ///
    /// </para>
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is weakly connected, false otherwise.</returns>
    /// <seealso cref="ConnectivityInspector"/>
    public static bool IsWeaklyConnected<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        return IsConnected(graph);
    }

    /// <summary>
    /// Test whether a graph is strongly connected.
    ///
    /// <para>
    /// This method does not performing any caching, instead recomputes everything from scratch. In
    /// case more control is required use <seealso cref="KosarajuStrongConnectivityInspector"/> directly.
    ///
    /// </para>
    /// <para>
    /// In case of undirected graphs this method delegated to <seealso cref="isConnected(Graph)"/>.
    ///
    /// </para>
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is strongly connected, false otherwise.</returns>
    /// <seealso cref="KosarajuStrongConnectivityInspector"/>
    public static bool IsStronglyConnected<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);
        if (graph.Type.Undirected)
        {
            return IsConnected(graph);
        }
        else
        {
            return (new KosarajuStrongConnectivityInspector<>(graph)).StronglyConnected;
        }
    }

    /// <summary>
    /// Test whether an undirected graph is a tree.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is tree, false otherwise.</returns>
    public static bool IsTree<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        if (!graph.Type.Undirected)
        {
            throw new ArgumentException(GraphMustBeUndirected);
        }

        return (graph.EdgeSet().Count == (graph.VertexSet().Count - 1)) && IsConnected(graph);
    }

    /// <summary>
    /// Test whether an undirected graph is a forest. A forest is a set of disjoint trees. By
    /// definition, any acyclic graph is a forest. This includes the empty graph and the class of
    /// tree graphs.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is forest, false otherwise.</returns>
    public static bool IsForest<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        if (!graph.Type.Undirected)
        {
            throw new ArgumentException(GraphMustBeUndirected);
        }

        if (graph.VertexSet().Count == 0) // null graph is not a forest
        {
            return false;
        }

        int nrConnectedComponents = (new ConnectivityInspector<>(graph)).ConnectedSets().Count;
        return graph.EdgeSet().Count + nrConnectedComponents == graph.VertexSet().Count;
    }

    /// <summary>
    /// Test whether a graph is <a href="https://en.wikipedia.org/wiki/Overfull_graph">overfull</a>.
    /// A graph is overfull if $|TEdge|&gt;\Delta(G)\lfloor |TNode|/2 \rfloor$, where $\Delta(G)$ is the
    /// maximum degree of the graph.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is overfull, false otherwise.</returns>
    public static bool IsOverfull<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        int maxDegree = graph.VertexSet().Select(graph.degreeOf).Max().getAsInt();
        return graph.EdgeSet().Count > maxDegree * Math.Floor(graph.VertexSet().Count / 2.0);
    }

    /// <summary>
    /// Test whether an undirected graph is a
    /// <a href="https://en.wikipedia.org/wiki/Split_graph">split graph</a>. A split graph is a graph
    /// in which the vertices can be partitioned into a clique and an independent set. Split graphs
    /// are a special class of chordal graphs. Given the degree sequence $d_1 \geq,\dots,\geq d_n$ of
    /// $G$, a graph is a split graph if and only if : \[\sum_{i=1}^m d_i = m (m - 1) + \sum_{i=m +
    /// 1}^nd_i\], where $m = \max_i \{d_i\geq i-1\}$. If the graph is a split graph, then the $m$
    /// vertices with the largest degrees form a maximum clique in $G$, and the remaining vertices
    /// constitute an independent set. See Brandstadt, A., Le, TNode., Spinrad, J. Graph Classes: A
    /// Survey. Philadelphia, PA: SIAM, 1999. for details.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is a split graph, false otherwise.</returns>
    public static bool IsSplit<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        RequireUndirected(graph);
        if (!IsSimple(graph) || graph.VertexSet().Count == 0)
        {
            return false;
        }

        IList<int> degrees = new List<int>(graph.VertexSet().Count);
        ((List<int>)degrees).AddRange(graph.VertexSet().Select(graph.degreeOf).ToList());
        degrees.Sort(Collections.reverseOrder()); // sort degrees descending order
        // Find m = \max_i \{d_i\geq i-1\}
        var m = 1;
        for (; m < degrees.Count && degrees[m] >= m; m++)
        {
        }

        m--;

        var left = 0;
        for (var i = 0; i <= m; i++)
        {
            left += degrees[i];
        }

        var right = m * (m + 1);
        for (var i = m + 1; i < degrees.Count; i++)
        {
            right += degrees[i];
        }

        return left == right;
    }

    /// <summary>
    /// Test whether a graph is bipartite.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is bipartite, false otherwise.</returns>
    /// <seealso cref="BipartitePartitioning.isBipartite()"/>
    public static bool IsBipartite<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    {
        return (new BipartitePartitioning<>(graph)).Bipartite;
    }

    /// <summary>
    /// Test whether a partition of the vertices into two sets is a bipartite partition.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <param name="firstPartition"> the first vertices partition.</param>
    /// <param name="secondPartition"> the second vertices partition.</param>
    /// <returns>true if the partition is a bipartite partition, false otherwise.</returns>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <seealso cref="BipartitePartitioning.isValidPartitioning(PartitioningAlgorithm.Partitioning)"/>
//JAVA TO C# CONVERTER TODO TASK: The following line could not be converted:
    @SuppressWarnings("unchecked") public static <TNode, TEdge> boolean
        IsBipartitePartition(IGraph<TNode, TEdge> graph, Set<? extends TNode> firstPartition, Set<? Extends TNode> secondPartition)
    {
        return (new BipartitePartitioning<>(graph)).IsValidPartitioning(
            new PartitioningAlgorithmPartitioningImpl<TNode>(java.util.Arrays.asList((ISet<TNode>)firstPartition,
                    (ISet<TNode>)secondPartition
                )
            )
        );
    }

    /// <summary>
    /// Tests whether a graph is <a href="http://mathworld.wolfram.com/CubicGraph.html">cubic</a>. A
    /// graph is cubic if all vertices have degree 3.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is cubic, false otherwise.</returns>
    public static <TNode, TEdge> bool IsCubic(IGraph<TNode, TEdge> graph)
    {
        foreach (TNode node in graph.VertexSet())
        {
            if (graph.DegreeOf(node) != 3)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Test whether a graph is Eulerian. An undirected graph is Eulerian if it is connected and each
    /// vertex has an even degree. A directed graph is Eulerian if it is strongly connected and each
    /// vertex has the same incoming and outgoing degree. Test whether a graph is Eulerian. An
    /// <a href="http://mathworld.wolfram.com/EulerianGraph.html">Eulerian graph</a> is a graph
    /// containing an <a href="http://mathworld.wolfram.com/EulerianCycle.html">Eulerian cycle</a>.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam>
    /// </param>
    /// <returns>true if the graph is Eulerian, false otherwise.</returns>
    /// <seealso cref="HierholzerEulerianCycle.isEulerian(Graph)"/>
    public static <TNode, TEdge> bool IsEulerian(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);
        return (new HierholzerEulerianCycle<TNode, TEdge>()).IsEulerian(graph);
    }

    /// <summary>
    /// Checks whether a graph is chordal. A <a href="https://en.wikipedia.org/wiki/Chordal_graph">
    /// chordal graph</a> is one in which all cycles of four or more vertices have a chord, which is
    /// an edge that is not part of the cycle but connects two vertices of the cycle.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is chordal, false otherwise.</returns>
    /// <seealso cref="ChordalityInspector.isChordal()"/>
    public static <TNode, TEdge> bool IsChordal(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);
        return (new ChordalityInspector<>(graph)).Chordal;
    }

    /// <summary>
    /// Checks whether a graph is <a href="http://www.graphclasses.org/classes/gc_14.html">weakly
    /// chordal</a>.
    /// <para>
    /// The following definitions are equivalent:
    /// <ol>
    /// <li>A graph is weakly chordal (weakly triangulated) if neither it nor its complement contains
    /// a <a href="http://mathworld.wolfram.com/ChordlessCycle.html">chordless cycles</a> with five
    /// or more vertices.</li>
    /// <li>A 2-pair in a graph is a pair of non-adjacent vertices $x$, $y$ such that every chordless
    /// path has exactly two edges. A graph is weakly chordal if every connected
    /// <a href="https://en.wikipedia.org/wiki/Induced_subgraph">induced subgraph</a> $H$ that is not
    /// a complete graph, contains a 2-pair.</li>
    /// </ol>
    ///
    /// </para>
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is weakly chordal, false otherwise.</returns>
    /// <seealso cref="WeakChordalityInspector.isWeaklyChordal()"/>
    public static <TNode, TEdge> bool IsWeaklyChordal(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);
        return (new WeakChordalityInspector<>(graph)).WeaklyChordal;
    }

    /// <summary>
    /// Tests whether an undirected graph meets Ore's condition to be Hamiltonian.
    /// 
    /// Let $G$ be a (finite and simple) graph with $n \geq 3$ vertices. We denote by $deg(TNode)$ the
    /// degree of a vertex $v$ in $G$, i.TEdge. the number of incident edges in $G$ to $v$. Then, Ore's
    /// theorem states that if $deg(TNode) + deg(w) \geq n$ for every pair of distinct non-adjacent
    /// vertices $v$ and $w$ of $G$, then $G$ is Hamiltonian.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph meets Ore's condition, false otherwise.</returns>
    /// <seealso cref="NGraphT.Core.alg.tour.PalmerHamiltonianCycle"/>
    public static <TNode, TEdge> bool HasOreProperty(IGraph<TNode, TEdge> graph)
    {
        RequireUndirected(graph);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int n = graph.vertexSet().size();
        var n = graph.VertexSet().Count;

        if (!graph.Type.Simple || n < 3)
        {
            return false;
        }

        IList<TNode> vertexList = new List<TNode>(graph.VertexSet());

        for (var i = 0; i < vertexList.Count; i++)
        {
            for (var j = i + 1; j < vertexList.Count; j++)
            {
                TNode node = vertexList[i];
                TNode w = vertexList[j];

                if (!node.Equals(w) && !graph.ContainsEdge(node, w) && graph.DegreeOf(node) + graph.DegreeOf(w) < n)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Tests whether an undirected graph is triangle-free (i.TEdge. no three distinct vertices form a
    /// triangle of edges).
    /// 
    /// The implementation of this method uses <seealso cref="GraphMetrics.getNumberOfTriangles(Graph)"/>.
    /// </summary>
    /// <param name="graph"> the input graph.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is triangle-free, false otherwise.</returns>
    public static <TNode, TEdge> bool IsTriangleFree(IGraph<TNode, TEdge> graph)
    {
        return GraphMetrics.GetNumberOfTriangles(graph) == 0;
    }

    /// <summary>
    /// Checks that the specified graph is perfect. Due to the Strong Perfect Graph Theorem Berge
    /// Graphs are the same as perfect Graphs. The implementation of this method is delegated to
    /// <seealso cref="NGraphT.Core.alg.cycle.BergeGraphInspector"/>
    /// </summary>
    /// <param name="graph"> the graph reference to check for being perfect or not.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is perfect, false otherwise.</returns>
    public static <TNode, TEdge> bool IsPerfect(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);
        return (new BergeGraphInspector<TNode, TEdge>()).IsBerge(graph);
    }

    /// <summary>
    /// Checks that the specified graph is planar. A graph is
    /// <a href="https://en.wikipedia.org/wiki/Planar_graph">planar</a> if it can be drawn on a
    /// two-dimensional plane without any of its edges crossing. The implementation of the method is
    /// delegated to the <seealso cref="NGraphT.Core.alg.planar.BoyerMyrvoldPlanarityInspector"/>. Also, use
    /// this class to get a planar embedding of the graph in case it is planar, or a Kuratowski
    /// subgraph as a certificate of nonplanarity.
    /// </summary>
    /// <param name="graph"> the graph to test planarity of.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the graph is planar, false otherwise.</returns>
    /// <seealso cref="PlanarityTestingAlgorithm"/>
    /// <seealso cref="BoyerMyrvoldPlanarityInspector"/>
    public static <TNode, TEdge> bool IsPlanar(IGraph<TNode, TEdge> graph)
    {
        Objects.requireNonNull(graph, GraphCannotBeNull);
        return (new BoyerMyrvoldPlanarityInspector<>(graph)).Planar;
    }

    /// <summary>
    /// Checks whether the {@code graph} is a <a href=
    /// "https://en.wikipedia.org/wiki/Kuratowski%27s_theorem#Kuratowski_subgraphs">Kuratowski
    /// subdivision</a>. Effectively checks whether the {@code graph} is a $K_{3,3}$ subdivision or
    /// $K_{5}$ subdivision
    /// </summary>
    /// <param name="graph"> the graph to test.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the {@code graph} is a Kuratowski subdivision, false otherwise.</returns>
    public static <TNode, TEdge> bool IsKuratowskiSubdivision(IGraph<TNode, TEdge> graph)
    {
        return IsK33Subdivision(graph) || IsK5Subdivision(graph);
    }

    /// <summary>
    /// Checks whether the {@code graph} is a $K_{3,3}$ subdivision.
    /// </summary>
    /// <param name="graph"> the graph to test.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the {@code graph} is a $K_{3,3}$ subdivision, false otherwise.</returns>
    public static <TNode, TEdge> bool IsK33Subdivision(IGraph<TNode, TEdge> graph)
    {
        IList<TNode> degree3 = new List<TNode>();
        // collect all vertices with degree 3
        foreach (TNode vertex in graph.VertexSet())
        {
            var degree = graph.DegreeOf(vertex);
            if (degree == 3)
            {
                degree3.Add(vertex);
            }
            else if (degree != 2)
            {
                return false;
            }
        }

        if (degree3.Count != 6)
        {
            return false;
        }

        TNode       vertex    = degree3.RemoveAndReturn(degree3.Count - 1);
        ISet<TNode> reachable = ReachableWithDegree(graph, vertex, 3);
        if (reachable.Count != 3)
        {
            return false;
        }

        degree3.RemoveAll(reachable);
        return reachable.SetEquals(ReachableWithDegree(graph, degree3[0], 3)) &&
               reachable.SetEquals(ReachableWithDegree(graph, degree3[1], 3));
    }

    /// <summary>
    /// Checks whether the {@code graph} is a $K_5$ subdivision.
    /// </summary>
    /// <param name="graph"> the graph to test.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>true if the {@code graph} is a $K_5$ subdivision, false otherwise.</returns>
    public static <TNode, TEdge> bool IsK5Subdivision(IGraph<TNode, TEdge> graph)
    {
        ISet<TNode> degree5 = new HashSet<TNode>();
        foreach (TNode vertex in graph.VertexSet())
        {
            var degree = graph.DegreeOf(vertex);
            if (degree == 4)
            {
                degree5.Add(vertex);
            }
            else if (degree != 2)
            {
                return false;
            }
        }

        if (degree5.Count != 5)
        {
            return false;
        }

        foreach (TNode vertex in degree5)
        {
            ISet<TNode> reachable = ReachableWithDegree(graph, vertex, 4);
            if (reachable.Count != 4 || !degree5.ContainsAll(reachable) || reachable.Contains(vertex))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Uses BFS to find all vertices of the {@code graph} which have a degree {@code degree}. This
    /// method doesn't advance to new nodes after it finds a node with a degree {@code degree}
    /// </summary>
    /// <param name="graph"> the graph to search in.</param>
    /// <param name="startVertex"> the start vertex.</param>
    /// <param name="degree"> the degree of desired vertices.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>all vertices of the {@code graph} reachable from {@code startVertex}, which have
    ///         degree {@code degree}</returns>
    private static <TNode, TEdge> ISet<TNode> ReachableWithDegree(IGraph<TNode, TEdge> graph, TNode startVertex, int degree)
    {
        ISet<TNode>       visited   = new HashSet<TNode>();
        ISet<TNode>       reachable = new HashSet<TNode>();
        LinkedList<TNode> queue     = new LinkedList<TNode>();
        queue.AddLast(startVertex);
        while (queue.Count > 0)
        {
            TNode current = queue.RemoveFirst();
            visited.Add(current);
            foreach (TEdge edge in graph.EdgesOf(current))
            {
                TNode opposite = Graphs.GetOppositeVertex(graph, edge, current);
                if (visited.Contains(opposite))
                {
                    continue;
                }

                if (graph.DegreeOf(opposite) == degree)
                {
                    reachable.Add(opposite);
                }
                else
                {
                    queue.AddLast(opposite);
                }
            }
        }

        return reachable;
    }

    /// <summary>
    /// Checks that the specified graph is directed and throws a customized
    /// <seealso cref="System.ArgumentException"/> if it is not. Also checks that the graph reference is not
    /// {@code null} and throws a <seealso cref="System.NullReferenceException"/> if it is.
    /// </summary>
    /// <param name="graph"> the graph reference to check for beeing directed and not null.</param>
    /// <param name="message"> detail message to be used in the event that an exception is thrown.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>{@code graph} if directed and not {@code null}</returns>
    /// <exception cref="NullReferenceException"> if {@code graph} is {@code null} </exception>
    /// <exception cref="ArgumentException"> if {@code graph} is not directed.</exception>
    public static <TNode, TEdge> IGraph<TNode, TEdge> RequireDirected(IGraph<TNode, TEdge> graph, string message)
    {
        if (graph == null)
        {
            throw new NullReferenceException(GraphCannotBeNull);
        }

        if (!graph.Type.Directed)
        {
            throw new ArgumentException(message);
        }

        return graph;
    }

    /// <summary>
    /// Checks that the specified graph is directed and throws an <seealso cref="System.ArgumentException"/> if
    /// it is not. Also checks that the graph reference is not {@code null} and throws a
    /// <seealso cref="System.NullReferenceException"/> if it is.
    /// </summary>
    /// <param name="graph"> the graph reference to check for beeing directed and not null.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>{@code graph} if directed and not {@code null}</returns>
    /// <exception cref="NullReferenceException"> if {@code graph} is {@code null} </exception>
    /// <exception cref="ArgumentException"> if {@code graph} is not directed.</exception>
    public static <TNode, TEdge> IGraph<TNode, TEdge> RequireDirected(IGraph<TNode, TEdge> graph)
    {
        return RequireDirected(graph, GraphMustBeDirected);
    }

    /// <summary>
    /// Checks that the specified graph is undirected and throws a customized
    /// <seealso cref="System.ArgumentException"/> if it is not. Also checks that the graph reference is not
    /// {@code null} and throws a <seealso cref="System.NullReferenceException"/> if it is.
    /// </summary>
    /// <param name="graph"> the graph reference to check for being undirected and not null.</param>
    /// <param name="message"> detail message to be used in the event that an exception is thrown.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>{@code graph} if undirected and not {@code null}</returns>
    /// <exception cref="NullReferenceException"> if {@code graph} is {@code null} </exception>
    /// <exception cref="ArgumentException"> if {@code graph} is not undirected.</exception>
    public static <TNode, TEdge> IGraph<TNode, TEdge> RequireUndirected(IGraph<TNode, TEdge> graph, string message)
    {
        if (graph == null)
        {
            throw new NullReferenceException(GraphCannotBeNull);
        }

        if (!graph.Type.Undirected)
        {
            throw new ArgumentException(message);
        }

        return graph;
    }

    /// <summary>
    /// Checks that the specified graph is undirected and throws an <seealso cref="System.ArgumentException"/>
    /// if it is not. Also checks that the graph reference is not {@code null} and throws a
    /// <seealso cref="System.NullReferenceException"/> if it is.
    /// </summary>
    /// <param name="graph"> the graph reference to check for being undirected and not null.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>{@code graph} if undirected and not {@code null}</returns>
    /// <exception cref="NullReferenceException"> if {@code graph} is {@code null} </exception>
    /// <exception cref="ArgumentException"> if {@code graph} is not undirected.</exception>
    public static <TNode, TEdge> IGraph<TNode, TEdge> RequireUndirected(IGraph<TNode, TEdge> graph)
    {
        return RequireUndirected(graph, GraphMustBeUndirected);
    }

    /// <summary>
    /// Checks that the specified graph is directed or undirected and throws a customized
    /// <seealso cref="System.ArgumentException"/> if it is not. Also checks that the graph reference is not
    /// {@code null} and throws a <seealso cref="System.NullReferenceException"/> if it is.
    /// </summary>
    /// <param name="graph"> the graph reference to check for beeing directed or undirected and not null.</param>
    /// <param name="message"> detail message to be used in the event that an exception is thrown.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>{@code graph} if directed and not {@code null}</returns>
    /// <exception cref="NullReferenceException"> if {@code graph} is {@code null} </exception>
    /// <exception cref="ArgumentException"> if {@code graph} is mixed.</exception>
    public static <TNode, TEdge> IGraph<TNode, TEdge> RequireDirectedOrUndirected(IGraph<TNode, TEdge> graph, string message)
    {
        if (graph == null)
        {
            throw new NullReferenceException(GraphCannotBeNull);
        }

        if (!graph.Type.Directed && !graph.Type.Undirected)
        {
            throw new ArgumentException(message);
        }

        return graph;
    }

    /// <summary>
    /// Checks that the specified graph is directed and throws an <seealso cref="System.ArgumentException"/> if
    /// it is not. Also checks that the graph reference is not {@code null} and throws a
    /// <seealso cref="System.NullReferenceException"/> if it is.
    /// </summary>
    /// <param name="graph"> the graph reference to check for beeing directed and not null.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>{@code graph} if directed and not {@code null}</returns>
    /// <exception cref="NullReferenceException"> if {@code graph} is {@code null} </exception>
    /// <exception cref="ArgumentException"> if {@code graph} is mixed.</exception>
    public static <TNode, TEdge> IGraph<TNode, TEdge> RequireDirectedOrUndirected(IGraph<TNode, TEdge> graph)
    {
        return RequireDirectedOrUndirected(graph, GraphMustBeDirectedOrUndirected);
    }

    /// <summary>
    /// Checks that the specified graph is weighted and throws a customized
    /// <seealso cref="System.ArgumentException"/> if it is not. Also checks that the graph reference is not
    /// {@code null} and throws a <seealso cref="System.NullReferenceException"/> if it is.
    /// </summary>
    /// <param name="graph"> the graph reference to check for being weighted and not null.</param>
    /// <typeparam name="TNode">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The graph edge type.</typeparam> </param>
    /// <returns>{@code graph} if directed and not {@code null}</returns>
    /// <exception cref="NullReferenceException"> if {@code graph} is {@code null} </exception>
    /// <exception cref="ArgumentException"> if {@code graph} is not weighted.</exception>
    public static <TNode, TEdge> IGraph<TNode, TEdge> RequireWeighted(IGraph<TNode, TEdge> graph)
    {
        if (graph == null)
        {
            throw new NullReferenceException(GraphCannotBeNull);
        }

        if (!graph.Type.Weighted)
        {
            throw new ArgumentException(GraphMustBeWeighted);
        }

        return graph;
    }
}
