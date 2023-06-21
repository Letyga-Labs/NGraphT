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

namespace NGraphT.Core.DotNetUtil;

internal class PriorityQueueAdapter<TElement> : IQueue<TElement>
{
    private readonly PriorityQueue<TElement, TElement> _delegate;

    public PriorityQueueAdapter(PriorityQueue<TElement, TElement> @delegate)
    {
        _delegate = @delegate;
    }

    public void Enqueue(TElement element)
    {
        _delegate.Enqueue(element, element);
    }

    public TElement Peek()
    {
        return _delegate.Peek();
    }

    public bool TryPeek([MaybeNullWhen(false)] out TElement result)
    {
        return _delegate.TryPeek(out result, out _);
    }

    public TElement Dequeue()
    {
        return _delegate.Dequeue();
    }

    public bool TryDequeue([MaybeNullWhen(false)] out TElement result)
    {
        return _delegate.TryDequeue(out result, out _);
    }
}