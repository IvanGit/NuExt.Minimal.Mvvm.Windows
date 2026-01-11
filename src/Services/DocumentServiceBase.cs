using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides a base class for services that manage documents associated with UI elements.
    /// </summary>
    public abstract class DocumentServiceBase : ViewServiceBase
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the <see cref="ActiveDocument"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ActiveDocumentProperty = DependencyProperty.Register(
            nameof(ActiveDocument), typeof(IAsyncDocument), typeof(DocumentServiceBase),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((DocumentServiceBase)d).OnActiveDocumentChanged(e.OldValue as IAsyncDocument, e.NewValue as IAsyncDocument)));

        /// <summary>
        /// Identifies the Document attached dependency property.
        /// This property is used to associate an asynchronous document (IAsyncDocument) with a DependencyObject.
        /// </summary>
        public static readonly DependencyProperty DocumentProperty = DependencyProperty.RegisterAttached(
            "Document", typeof(IAsyncDocument), typeof(DocumentServiceBase), new PropertyMetadata(null, (d, e) => OnDocumentChanged(d, (IAsyncDocument?)e.OldValue, (IAsyncDocument?)e.NewValue)));

        /// <summary>
        /// Identifies the <see cref="FallbackViewType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FallbackViewTypeProperty = DependencyProperty.Register(
            nameof(FallbackViewType), typeof(Type), typeof(DocumentServiceBase));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the currently active document.
        /// </summary>
        /// <value>
        /// The active <see cref="IAsyncDocument"/> instance, or <see langword="null"/> if no document is active.
        /// </value>
        public IAsyncDocument? ActiveDocument
        {
            get => (IAsyncDocument)GetValue(ActiveDocumentProperty);
            set => SetValue(ActiveDocumentProperty, value);
        }

        /// <summary>
        /// Gets or sets the type of the fallback view to create when a view cannot be resolved.
        /// </summary>
        public Type? FallbackViewType
        {
            get => (Type)GetValue(FallbackViewTypeProperty);
            set => SetValue(FallbackViewTypeProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="ActiveDocument"/> property value changes.
        /// </summary>
        public event EventHandler<ActiveDocumentChangedEventArgs>? ActiveDocumentChanged;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="ActiveDocumentChanged"/> event.
        /// </summary>
        /// <param name="oldValue">The previously active document, or <see langword="null"/>.</param>
        /// <param name="newValue">The newly active document, or <see langword="null"/>.</param>
        protected virtual void OnActiveDocumentChanged(IAsyncDocument? oldValue, IAsyncDocument? newValue)
        {
            ActiveDocumentChanged?.Invoke(this, new ActiveDocumentChangedEventArgs(oldValue, newValue));
        }

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

        #region Methods

        /// <summary>
        /// Creates an instance of the view type specified by <see cref="FallbackViewType"/> using its parameterless constructor.
        /// </summary>
        /// <returns>A new view instance or <see langword="null"/> if no type is specified.</returns>
        /// <remarks>
        /// This fallback method is invoked when primary view resolution fails. 
        /// The specified type must have a public parameterless constructor.
        /// </remarks>
        protected virtual object? CreateFallbackView()
        {
            return FallbackViewType == null ? null : Activator.CreateInstance(FallbackViewType);
        }

        /// <inheritdoc/>
        protected override ValueTask<object?> CreateViewAsync(string? documentType, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTask.FromCanceled<object?>(cancellationToken);
            }

            if (documentType == null && ViewTemplate == null && ViewTemplateSelector == null)
            {
                try
                {
                    var view = CreateFallbackView();
                    return view != null ? ValueTask.FromResult<object?>(view) : GetViewLocator().GetOrCreateViewAsync(documentType, cancellationToken);
                }
                catch (Exception ex)
                {
                    return ValueTask.FromException<object?>(ex);
                }
            }
            return base.CreateViewAsync(documentType, cancellationToken);
        }

        #endregion
    }
}
