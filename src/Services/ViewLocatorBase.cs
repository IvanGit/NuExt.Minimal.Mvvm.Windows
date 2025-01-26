using Minimal.Mvvm.Windows.Controls;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// An abstract base class that provides methods to locate and initialize views.
    /// </summary>
    public abstract class ViewLocatorBase
    {
        #region Methods

        /// <summary>
        /// Creates a view asynchronously based on the specified parameters.
        /// </summary>
        /// <param name="documentType">The type of document to create the view for.</param>
        /// <param name="viewTemplate">The data template used for the view.</param>
        /// <param name="viewTemplateSelector">The data template selector used to select the appropriate data template.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created view object.</returns>
        public virtual async ValueTask<object?> CreateViewAsync(string? documentType, DataTemplate? viewTemplate, DataTemplateSelector? viewTemplateSelector, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (viewTemplate != null || viewTemplateSelector != null)
            {
                return new ContentPresenter()
                {
                    ContentTemplate = viewTemplate,
                    ContentTemplateSelector = viewTemplateSelector
                };
            }
            return await GetOrCreateViewAsync(documentType, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets or creates a view asynchronously based on the specified view name.
        /// </summary>
        /// <param name="viewName">The name of the view to get or create.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created view object.</returns>
        public virtual ValueTask<object?> GetOrCreateViewAsync(string? viewName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var viewType = GetViewType(viewName);
            return viewType != null ? new ValueTask<object?>(CreateViewFromType(viewType, viewName)) : new ValueTask<object?>(CreateFallbackView(viewName));
        }

        /// <summary>
        /// Gets the type of the view based on the specified view name.
        /// </summary>
        /// <param name="viewName">The name of the view.</param>
        /// <returns>The type of the view.</returns>
        protected abstract Type? GetViewType(string? viewName);

        /// <summary>
        /// Creates a view from the specified view type.
        /// </summary>
        /// <param name="viewType">The type of the view to create.</param>
        /// <param name="viewName">The name of the view.</param>
        /// <returns>The created view object.</returns>
        protected virtual object? CreateViewFromType(Type viewType, string? viewName)
        {
            Throw.IfNull(viewType);
            var ctor = viewType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, [], null);
            if (ctor != null)
            {
                return ctor.Invoke(null);
            }
            return Activator.CreateInstance(viewType,
                BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.OptionalParamBinding,
                null, null, null);
        }

        /// <summary>
        /// Creates a fallback view when the specified view type cannot be found.
        /// </summary>
        /// <param name="viewName">The name of the view.</param>
        /// <returns>The created fallback view object.</returns>
        protected virtual object CreateFallbackView(string? viewName)
        {
            string errorMessage;
            if (string.IsNullOrEmpty(viewName)) errorMessage = "ViewType is not specified.";
            else if (ViewModelBase.IsInDesignMode) errorMessage = $"[{viewName}]";
            else errorMessage = $"\"{viewName}\" type not found.";
            return new FallbackView() { Text = errorMessage };
        }

        #endregion
    }
}
