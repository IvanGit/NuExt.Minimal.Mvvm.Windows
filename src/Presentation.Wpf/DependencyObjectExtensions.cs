using System.Windows.Media;

namespace System.Windows
{
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// Search for an element of a certain type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="parent">The parent element.</param>
        /// <returns></returns>
        public static T? FindChild<T>(this DependencyObject parent) where T : DependencyObject
        {
#if NET
            ArgumentNullException.ThrowIfNull(parent);
#else
            Throw.IfNull(parent);
#endif
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T correctlyTyped)
                {
                    return correctlyTyped;
                }

                T? descendent = FindChild<T>(child);
                if (descendent != null)
                {
                    return descendent;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively searches for and returns the first child element with the specified name in the visual tree.
        /// </summary>
        /// <param name="parent">The starting point in the visual tree.</param>
        /// <param name="name">The name of the child element to find.</param>
        /// <returns>The first child element with the specified name, or null if no such element exists.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the parent or name parameter is null.</exception>
        public static FrameworkElement? FindChildByName(this DependencyObject parent, string name)
        {
#if NET
            ArgumentNullException.ThrowIfNull(parent);
#else
            Throw.IfNull(parent);
#endif
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(name);
#else
            Throw.IfNullOrEmpty(name);
#endif
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is FrameworkElement frameworkElement && frameworkElement.Name == name)
                    return frameworkElement;

                var result = FindChildByName(child, name);
                if (result is not null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Recursively searches for and returns all child elements of a specified type within the visual tree of a given parent element.
        /// </summary>
        /// <typeparam name="T">The type of the child elements to find. Must derive from DependencyObject.</typeparam>
        /// <param name="parent">The parent element to start the search from. Cannot be null.</param>
        /// <returns>An IEnumerable containing all found child elements of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the parent parameter is null.</exception>
        public static IEnumerable<T> FindChildren<T>(this DependencyObject parent) where T : DependencyObject
        {
#if NET
            ArgumentNullException.ThrowIfNull(parent);
#else
            Throw.IfNull(parent);
#endif

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    yield return typedChild;

                foreach (T childOfChild in FindChildren<T>(child))
                    yield return childOfChild;
            }
        }

        /// <summary>
        /// Finds the first parent of a specified type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the parent to find.</typeparam>
        /// <param name="source">The starting point in the visual tree.</param>
        /// <returns>The first parent of the specified type, or null if no such parent exists.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source parameter is null.</exception>
        public static T? FindParent<T>(this DependencyObject source) where T : DependencyObject
        {
#if NET
            ArgumentNullException.ThrowIfNull(source);
#else
            Throw.IfNull(source);
#endif
            var obj = source;
            while (obj != null)
            {
                obj = VisualTreeHelper.GetParent(obj);
                if (obj is T parent) return parent;
            }
            return null;
        }
    }
}
