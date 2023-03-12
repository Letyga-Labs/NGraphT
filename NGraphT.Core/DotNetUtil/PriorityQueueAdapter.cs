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
