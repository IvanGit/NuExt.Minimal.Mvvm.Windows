using System.Diagnostics;
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides a base class for services that manage documents associated with UI elements.
    /// </summary>
    /// <typeparam name="T">The type of FrameworkElement associated with the service.</typeparam>
    public abstract class DocumentServiceBase<T> : ViewServiceBase<T> where T : FrameworkElement
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the Document attached dependency property.
        /// This property is used to associate an asynchronous document (IAsyncDocument) with a DependencyObject.
        /// </summary>
        public static readonly DependencyProperty DocumentProperty = DependencyProperty.RegisterAttached(
            "Document", typeof(IAsyncDocument), typeof(DocumentServiceBase<T>), new PropertyMetadata(null, (d, e) => OnDocumentChanged(d, (IAsyncDocument?)e.OldValue, (IAsyncDocument?)e.NewValue)));

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles changes to the Document attached property.
        /// </summary>
        /// <param name="element">The element on which the property was changed.</param>
        /// <param name="oldDocument">The old value of the Document property.</param>
        /// <param name="newDocument">The new value of the Document property.</param>
        private static void OnDocumentChanged(DependencyObject element, IAsyncDocument? oldDocument, IAsyncDocument? newDocument)
        {
            var doc = GetDocument(element);
            Debug.Assert(doc == newDocument);
        }

        #endregion

        #region Dependency Methods

        /// <summary>
        /// Gets the value of the Document attached property from a specified DependencyObject.
        /// </summary>
        /// <param name="element">The DependencyObject from which to read the value.</param>
        /// <returns>The current value of the Document attached property.</returns>
        public static IAsyncDocument? GetDocument(DependencyObject element)
        {
            return (IAsyncDocument?)element.GetValue(DocumentProperty);
        }

        /// <summary>
        /// Sets the value of the Document attached property on a specified DependencyObject.
        /// </summary>
        /// <param name="element">The DependencyObject on which to set the value.</param>
        /// <param name="value">The new value for the Document attached property.</param>
        public static void SetDocument(DependencyObject element, IAsyncDocument? value)
        {
            element.SetValue(DocumentProperty, value);
        }

        #endregion
    }
}
