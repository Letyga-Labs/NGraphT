/*
 * (C) Copyright 2017-2021, by Dimitrios Michail and Contributors.
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

/// <summary>
/// Default implementation of an edge function which uses a map to store values.
/// </summary>
///
/// <remarks>Author: Dimitrios Michail.</remarks>
/// <typeparam name="TEdge"> the edge type.</typeparam>
/// <typeparam name="T"> the value type.</typeparam>
public class DefaultEdgeFunction<TEdge, T> : Func<TEdge, T>
{
    protected internal readonly IDictionary<TEdge, T> Map;
    protected internal readonly T                     DefaultValue;

    ///<summary>
    ///Create a new function.
    ///</summary>
    ///<param name="defaultValue"> the default value.</param>
    public DefaultEdgeFunction(T defaultValue)
        : this(defaultValue, new Dictionary<TEdge, T>())
    {
    }

    ///<summary>
    ///Create a new function.
    ///</summary>
    ///<param name="defaultValue"> the default value.</param>
    ///<param name="map"> the underlying map.</param>
    public DefaultEdgeFunction(T defaultValue, IDictionary<TEdge, T> map)
    {
        DefaultValue = Objects.requireNonNull(defaultValue, "Default value cannot be null");
        Map          = Objects.requireNonNull(map,          "Map cannot be null");
    }

    ///<summary>
    ///Get the function value for an edge.
    ///</summary>
    ///<param name="edge"> the edge.</param>
    public override T Apply(TEdge edge)
    {
        return Map.GetOrDefault(edge, DefaultValue);
    }

    ///<summary>
    ///Get the function value for an edge.
    ///</summary>
    ///<param name="edge"> the edge.</param>
    ///<returns>the function value for the edge.</returns>
    public virtual T Get(TEdge edge)
    {
        return Map.GetOrDefault(edge, DefaultValue);
    }

    ///<summary>
    ///Set the function value for an edge.
    ///</summary>
    ///<param name="edge"> the edge.</param>
    ///<param name="value"> the value.</param>
    public virtual void Set(TEdge edge, T value)
    {
        Map[edge] = value;
    }
}
