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

using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NGraphT.Core.Util;

/// <summary>
/// Generates elements from the input collection in random order.
///
/// <para>
/// An element can be generated only once. After all elements have been generated, this generator
/// halts. At every step, an element is generated uniformly at random, which means that every element
/// has an equal probability to be generated. This implementation is based on the
/// <a href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle"> Fisher-Yates algorithm</a>.
/// The generator is unbiased meaning the every permutation is equally likely.
/// </para>
/// </summary>
///
/// <typeparam name="T">element type.</typeparam>
///
/// <remarks>Author: Timofey Chudakov.</remarks>
[SuppressMessage("Security", "CA5394:Do not use insecure randomness")]
public sealed class ElementsSequenceGenerator<T> : IEnumerator<T>, IEnumerable<T>
{
    /// <summary>
    /// Input elements ordered as a list. This list is being decreased in size as the elements are
    /// generated.
    /// </summary>
    private readonly IList<T> _elements;

    /// <summary>
    /// Random instance used by this generator.
    /// </summary>
    private readonly Random _rng;

    private T? _current;

    /// <summary>
    /// Constructs a new <see cref="ElementsSequenceGenerator{T}"/>.
    /// </summary>
    /// <param name="elements"> a collection of elements to generate elements from.</param>
    public ElementsSequenceGenerator(ICollection<T> elements)
        : this(elements, (int)Stopwatch.GetTimestamp())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="ElementsSequenceGenerator{T}"/> using the specified <c>seed</c>. Two
    /// different generators with the same seed will produce identical sequences given that the same
    /// collection of elements is provided.
    /// </summary>
    /// <param name="elements"> a collection of elements to generate elements from.</param>
    /// <param name="seed"> a seed for the random number generator.</param>
    public ElementsSequenceGenerator(ICollection<T> elements, int seed)
        : this(elements, new Random(seed))
    {
    }

    /// <summary>
    /// Constructs a new <see cref="ElementsSequenceGenerator{T}"/> using the specified random number
    /// generator <c>rng</c>. Two different generators will produce identical sequences from a
    /// collection of elements given that the random number generator produces the same sequence of
    /// numbers.
    /// </summary>
    /// <param name="elements"> a collection of elements to generate elements from.</param>
    /// <param name="rng"> a random number generator.</param>
    public ElementsSequenceGenerator(ICollection<T> elements, Random rng)
    {
        _elements = new List<T>(elements);
        _rng      = rng;
    }

    object IEnumerator.Current => Current!;

    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations")]
    public T Current => _current ?? throw new NoSuchElementException();

    public bool MoveNext()
    {
        if (_elements.Count == 0)
        {
            return false;
        }

        var index  = _rng.Next(_elements.Count);
        var result = _elements[index];

        _elements[index] = _elements[^1];
        _elements.RemoveAt(_elements.Count - 1);

        _current = result;
        return true;
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {
        // empty
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        return this;
    }
}
