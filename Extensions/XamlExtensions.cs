namespace PlayRandom;

public static class XamlExtensions {
    /// <summary> Finds the ancestor of the specified type. </summary>
    /// <typeparam name="T"> The type of the ancestor to find. </typeparam>
    /// <param name="Element"> The element to start searching from. </param>
    /// <returns> The ancestor of the specified type, or null if no ancestor of the specified type was found. </returns>
    public static T? FindAncestor<T>( this DependencyObject? Element ) where T : DependencyObject {
        while (Element != null) {
            if (Element is T Parent) {
                return Parent;
            }

            Element = VisualTreeHelper.GetParent(Element);
        }

        return null;
    }
}
