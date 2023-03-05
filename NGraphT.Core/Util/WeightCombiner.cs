/*
 * (C) Copyright 2009-2021, by Ilya Razenshteyn and Contributors.
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

namespace NGraphT.Core.Util;

/// <summary>
/// Binary operator for edge weights. There are some prewritten operators.
/// </summary>
public interface IWeightCombiner
{
    /// <summary>
    /// Sum of weights.
    /// </summary>
    public static IWeightCombiner Sum = (IWeightCombiner & Serializable)(a, B) => a + b;

    /// <summary>
    /// Multiplication of weights.
    /// </summary>
    public static IWeightCombiner Mult = (IWeightCombiner & Serializable)(a, B) => a* B;

    /// <summary>
    /// Minimum weight.
    /// </summary>
    public static IWeightCombiner Min = (IWeightCombiner & Serializable) Math.min;

    /// <summary>
    /// Maximum weight.
    /// </summary>
    public static IWeightCombiner Max = (IWeightCombiner & Serializable) Math.max;

    /// <summary>
    /// First weight.
    /// </summary>
    public static IWeightCombiner First = (IWeightCombiner & Serializable)(a, B) => a;

    /// <summary>
    /// Second weight.
    /// </summary>
    public static IWeightCombiner Second = (IWeightCombiner & Serializable)(a, B) => b;

    /// <summary>
    /// Combines two weights.
    /// </summary>
    /// <param name="a"> first weight.</param>
    /// <param name="b"> second weight.</param>>
    /// <returns>result of the operator.</returns>
    double Combine(double a, double b);
}
