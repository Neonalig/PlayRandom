namespace PlayRandom;

public static class EnumerableExtensions {

    /// <summary> Shuffles the elements of the specified collection in-place. </summary>
    /// <typeparam name="T"> The type of the elements in the collection. </typeparam>
    /// <param name="Collection"> The collection to shuffle. </param>
    public static void Shuffle<T>( this IList<T> Collection ) {
        int Count = Collection.Count;
        while (Count > 1) {
            Count--;
            int Index = Random.Shared.Next(Count + 1);
            (Collection[Index], Collection[Count]) = (Collection[Count], Collection[Index]);
        }
    }

    /// <summary> Attempts to find the given <paramref name="Value"/> in the given <paramref name="Collection"/>. </summary>
    /// <param name="Collection"> The collection to search. </param>
    /// <param name="Value"> The value to find. </param>
    /// <param name="Found"> The found value, if any. </param>
    /// <typeparam name="T0"> The type of the elements in the collection. </typeparam>
    /// <typeparam name="T1"> The type of the value to find. </typeparam>
    /// <returns> <see langword="true"/> if the value was found; otherwise, <see langword="false"/>. </returns>
    public static bool TryFind<T0, T1>( this IEnumerable<T0> Collection, T1 Value, [NotNullWhen(true)] out T0? Found ) where T0 : IEquatable<T1> {
        foreach (T0 Item in Collection) {
            if (Item.Equals(Value)) {
                Found = Item;
                return true;
            }
        }

        Found = default;
        return false;
    }

}
