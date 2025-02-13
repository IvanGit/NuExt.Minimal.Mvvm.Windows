﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides helper methods for managing the relationship between views and view models,
    /// including attaching and detaching view models to views, retrieving properties from view models,
    /// and setting or clearing bindings for view model properties on views.
    /// </summary>
    public static class ViewModelHelper
    {
        /// <summary>
        /// The name of the title property used in view models.
        /// </summary>
        public const string TitlePropertyName = "Title";

        /// <summary>
        /// Attaches a view model to the specified view by setting the appropriate property.
        /// </summary>
        /// <param name="view">The view to which the view model will be attached. It can be a <see cref="ContentPresenter"/>, <see cref="FrameworkElement"/>, or <see cref="FrameworkContentElement"/>.</param>
        /// <param name="viewModel">The view model to attach to the view.</param>
        public static void AttachViewModel(object? view, object? viewModel)
        {
            switch (view)
            {
                case ContentPresenter cp: cp.Content = viewModel; break;
                case FrameworkElement fe: fe.DataContext = viewModel; break;
                case FrameworkContentElement fce: fce.DataContext = viewModel; break;
            }
        }

        /// <summary>
        /// Detaches the view model from the specified view by clearing the appropriate property.
        /// </summary>
        /// <param name="view">The view from which the view model will be detached. It can be a <see cref="ContentPresenter"/>, <see cref="FrameworkElement"/>, or <see cref="FrameworkContentElement"/>.</param>
        public static void DetachViewModel(object? view)
        {
            switch (view)
            {
                case ContentPresenter cp: cp.Content = null; break;
                case FrameworkElement fe: fe.DataContext = null; break;
                case FrameworkContentElement fce: fce.DataContext = null; break;
            }
        }

        /// <summary>
        /// Retrieves the view model associated with a specified view.
        /// </summary>
        /// <param name="view">The view object, which can be of type <see cref="FrameworkElement"/> or <see cref="FrameworkContentElement"/>.</param>
        /// <returns>Returns the DataContext if the view is of type <see cref="FrameworkElement"/> or <see cref="FrameworkContentElement"/>; otherwise, returns null.</returns>
        public static object? GetViewModelFromView(object? view)
        {
            return view switch
            {
                FrameworkElement fe => fe.DataContext,
                FrameworkContentElement fce => fce.DataContext,
                _ => null
            };
        }

        /// <summary>
        /// Retrieves the title of the view model if it exists.
        /// </summary>
        /// <param name="viewModel">The view model object from which to retrieve the title.</param>
        /// <returns>Returns the title of the view model if it exists; otherwise, returns null.</returns>
        public static string? GetViewModelTitle(object viewModel)
        {
            Throw.IfNull(viewModel);
            if (viewModel is IAsyncDocumentContent documentContent)
            {
                return documentContent.Title;
            }
            var titleProperty = viewModel.GetType().GetProperty(TitlePropertyName);
            return titleProperty != null && titleProperty.PropertyType == typeof(string) && titleProperty is { CanRead: true } ? (string?)titleProperty.GetValue(viewModel) : null;
        }

        /// <summary>
        /// Initializes the view and view model asynchronously based on the specified parameters.
        /// </summary>
        /// <param name="view">The view to initialize.</param>
        /// <param name="viewModel">The view model associated with the view.</param>
        /// <param name="parentViewModel">The parent view model, if any.</param>
        /// <param name="parameter">Additional parameter for initializing the view.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async ValueTask InitializeViewAsync(object? view, object? viewModel, object? parentViewModel, object? parameter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // First, initialize parameters before data context is set
            if (viewModel is ViewModelBase viewModelBase)
            {
                viewModelBase.ParentViewModel ??= parentViewModel;
                viewModelBase.Parameter ??= parameter;
            }
            // Second, attach the view model to the view
            AttachViewModel(view, viewModel);
            // Third, initialize the view model asynchronously if it has not been initialized yet
            if (viewModel is ViewModelBase { IsInitialized: false } initializable)
            {
                await initializable.InitializeAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sets a DataContext binding to the view model associated with the given view.
        /// </summary>
        /// <param name="view">The view from which the view model will be retrieved.</param>
        /// <param name="property">The dependency property on the target object to which the binding will be set.</param>
        /// <param name="target">The object on which to set the binding.</param>
        public static void SetDataContextBinding(object? view, DependencyProperty property, DependencyObject target)
        {
            BindingOperations.SetBinding(target, property, new Binding()
            {
                Path = new PropertyPath(nameof(FrameworkElement.DataContext)),
                Source = view,
                Mode = BindingMode.OneWay
            });
        }

        /// <summary>
        /// Sets a binding to the 'Title' property of the view model associated with the given view.
        /// </summary>
        /// <param name="view">The view from which the view model will be retrieved.</param>
        /// <param name="property">The dependency property on the target object to which the binding will be set.</param>
        /// <param name="target">The object on which to set the binding.</param>
        public static void SetViewTitleBinding(object? view, DependencyProperty property, DependencyObject target)
        {
            var viewModel = GetViewModelFromView(view);
            if (viewModel != null && ViewModelHasTitleProperty(viewModel))
            {
                BindingOperations.SetBinding(target, property, new Binding()
                {
                    Path = new PropertyPath(TitlePropertyName),
                    Source = viewModel,
                    Mode = BindingMode.TwoWay
                });
            }
            Debug.Assert(viewModel != null);
        }

        /// <summary>
        /// Checks if the provided ViewModel has a 'Title' property that is readable and writable.
        /// </summary>
        /// <param name="viewModel">The ViewModel object to check.</param>
        /// <returns>Returns true if the ViewModel contains a 'Title' property that can be read from and written to; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided viewModel is null.</exception>
        public static bool ViewModelHasTitleProperty(object viewModel)
        {
            Throw.IfNull(viewModel);
            if (viewModel is DocumentContentViewModelBase)
            {
                return true;
            }
            var titleProperty = viewModel.GetType().GetProperty(TitlePropertyName);
            return titleProperty != null && titleProperty.PropertyType == typeof(string) && titleProperty is { CanRead: true, CanWrite: true };
        }
    }
}
