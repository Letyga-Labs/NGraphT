using System.Diagnostics.CodeAnalysis;

namespace NGraphT.Core.DotNetUtil;

internal interface IQueue<TElement>
{
    void Enqueue(TElement element);

    TElement Peek();

    bool TryPeek([MaybeNullWhen(false)] out TElement result);

    TElement Dequeue();

    bool TryDequeue([MaybeNullWhen(false)] out TElement result);
}
