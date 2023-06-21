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

using System.Runtime.InteropServices;

namespace NGraphT.Core.Util;

[StructLayout(LayoutKind.Auto)]
public readonly struct UnorderedPair<T1, T2> : IEquatable<UnorderedPair<T1, T2>>
{
    public UnorderedPair(T1 first, T2 second)
    {
        First  = first;
        Second = second;
    }

    public T1 First  { get; }
    public T2 Second { get; }

    public static bool operator ==(UnorderedPair<T1, T2> left, UnorderedPair<T1, T2> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(UnorderedPair<T1, T2> left, UnorderedPair<T1, T2> right)
    {
        return !left.Equals(right);
    }

    public bool Equals(UnorderedPair<T1, T2> other)
    {
        return (Equals(First, other.First) && Equals(Second,  other.Second)) ||
               (Equals(First, other.Second) && Equals(Second, other.First));
    }

    public override bool Equals(object? obj)
    {
        return obj is UnorderedPair<T1, T2> other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash1 = First?.GetHashCode() ?? 0;
        var hash2 = Second?.GetHashCode() ?? 0;
        return hash1 > hash2 ? hash1 * 31 + hash2 : hash2 * 31 + hash1;
    }
}
