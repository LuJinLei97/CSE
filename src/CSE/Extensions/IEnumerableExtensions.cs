using System.Collections.ObjectModel;

using JinLei.Extensions;

namespace CSE.Extensions;

public static partial class IEnumerableExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> values) => new(values.GetSelfOrEmpty());
}