using System.Collections.ObjectModel;

using JinLei.Extensions;

namespace CSE.Extensions;

public static partial class Extensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> values) => new(values.GetSelfOrEmpty());

    public static IEnumerable<T> ForEach<T>(this Memory<T> memory, Func<T, int, bool> wherePredicate = default, Func<T, int, bool> whilePredicate = default)
    {
        for(var i = 0; i < memory.Length; i++)
        {
            if(whilePredicate?.Invoke(memory.Span[i], i) == false)
            {
                yield break;
            }

            if(wherePredicate?.Invoke(memory.Span[i], i) == true)
            {
                yield return memory.Span[i];
            }
        }
    }
}