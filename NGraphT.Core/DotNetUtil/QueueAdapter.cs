using System.Diagnostics.CodeAnalysis;

namespace NGraphT.Core.DotNetUtil;

internal class QueueAdapter<TElement> : IQueue<TElement>
{
    private readonly Queue<TElement> _delegate;

    public QueueAdapter(Queue<TElement> @delegate)
    {
        _delegate = @delegate;
    }

    public void Enqueue(TElement element)
    {
        _delegate.Enqueue(element);
    }

    public TElement Peek()
    {
        return _delegate.Peek();
    }

    public bool TryPeek([MaybeNullWhen(false)] out TElement result)
    {
        return _delegate.TryPeek(out result);
    }

    public TElement Dequeue()
    {
        return _delegate.Dequeue();
    }

    public bool TryDequeue([MaybeNullWhen(false)] out TElement result)
    {
        return _delegate.TryDequeue(out result);
    }
}
