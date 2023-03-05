namespace NGraphT.Core.DotNetUtil;

internal static class CollectionUtils
{
    public static IEnumerable<(T Previous, T Current)> Pairwise<T>(this IEnumerable<T> source)
    {
        var previous = default(T);

        using var it = source.GetEnumerator();
        if (it.MoveNext())
        {
            previous = it.Current;
        }

        while (it.MoveNext())
        {
            var current = it.Current;
            yield return (Previous: previous!, Current: current);
            previous = current;
        }
    }
}
