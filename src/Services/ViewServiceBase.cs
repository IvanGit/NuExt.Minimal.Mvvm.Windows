using System.Windows;
using System.Windows.Controls;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// An abstract base class that provides services related to views in an MVVM framework.
    /// </summary>
    /// <typeparam name="T">The type of the framework element that this service will manage.</typeparam>
    public abstract class ViewServiceBase<T> : ServiceBase<T> where T : FrameworkElement
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the <see cref="ViewLocator"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewLocatorProperty = DependencyProperty.Register(
            nameof(ViewLocator), typeof(ViewLocatorBase), typeof(ViewServiceBase<T>),
            new PropertyMetadata(null, (d, e) => ((ViewServiceBase<T>)d).OnViewLocatorChanged((ViewLocatorBase?)e.OldValue, (ViewLocatorBase?)e.NewValue)));

        /// <summary>
        /// Identifies the <see cref="ViewTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewTemplateProperty = DependencyProperty.Register(
            nameof(ViewTemplate), typeof(DataTemplate), typeof(ViewServiceBase<T>),
            new PropertyMetadata(null, (d, e) => ((ViewServiceBase<T>)d).OnViewTemplateChanged((DataTemplate?)e.OldValue, (DataTemplate?)e.NewValue)));

        /// <summary>
        /// Identifies the <see cref="ViewTemplateSelector"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewTemplateSelectorProperty = DependencyProperty.Register(
            nameof(ViewTemplateSelector), typeof(DataTemplateSelector), typeof(ViewServiceBase<T>),
            new PropertyMetadata(null, (d, e) => ((ViewServiceBase<T>)d).OnViewTemplateSelectorChanged((DataTemplateSelector?)e.OldValue, (DataTemplateSelector?)e.NewValue)));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the view locator used to locate views for the view models.
        /// </summary>
        public ViewLocatorBase? ViewLocator
        {
            get => (ViewLocatorBase?)GetValue(ViewLocatorProperty);
            set => SetValue(ViewLocatorProperty, value);
        }

        /// <summary>
        /// Gets or sets the data template used for the views.
        /// </summary>
        public DataTemplate? ViewTemplate
        {
            get => (DataTemplate?)GetValue(ViewTemplateProperty);
            set => SetValue(ViewTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the data template selector used to select the appropriate data template for the views.
        /// </summary>
        public DataTemplateSelector? ViewTemplateSelector
        {
            get => (DataTemplateSelector?)GetValue(ViewTemplateSelectorProperty);
            set => SetValue(ViewTemplateSelectorProperty, value);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when the <see cref="ViewLocator"/> property changes.
        /// </summary>
        /// <param name="oldValue">The old value of the view locator.</param>
        /// <param name="newValue">The new value of the view locator.</param>
        protected virtual void OnViewLocatorChanged(ViewLocatorBase? oldValue, ViewLocatorBase? newValue)
        {

        }

        /// <summary>
        /// Called when the <see cref="ViewTemplate"/> property changes.
        /// </summary>
        /// <param name="oldValue">The old value of the view template.</param>
        /// <param name="newValue">The new value of the view template.</param>
        protected virtual void OnViewTemplateChanged(DataTemplate? oldValue, DataTemplate? newValue)
        {

        }

        /// <summary>
        /// Called when the <see cref="ViewTemplateSelector"/> property changes.
        /// </summary>
        /// <param name="oldValue">The old value of the view template selector.</param>
        /// <param name="newValue">The new value of the view template selector.</param>
        protected virtual void OnViewTemplateSelectorChanged(DataTemplateSelector? oldValue, DataTemplateSelector? newValue)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates and initializes a view asynchronously based on the specified parameters.
        /// </summary>
        /// <param name="documentType">The type of document to create the view for.</param>
        /// <param name="viewModel">The view model associated with the view.</param>
        /// <param name="parentViewModel">The parent view model, if any.</param>
        /// <param name="parameter">Additional parameter for initializing the view.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created view object.</returns>
        protected ValueTask<object?> CreateAndInitializeViewAsync(string? documentType, object? viewModel, object? parentViewModel, object? parameter, CancellationToken cancellationToken)
        {
            return GetViewLocator().CreateAndInitializeViewAsync(documentType, viewModel, parentViewModel, parameter, ViewTemplate, ViewTemplateSelector, cancellationToken);
        }

        /// <summary>
        /// Gets the view locator instance to use for locating views.
        /// </summary>
        /// <returns>The view locator instance.</returns>
        protected ViewLocatorBase GetViewLocator()
        {
            return ViewLocator ?? Windows.ViewLocator.Default;
        }

        #endregion
    }
}
