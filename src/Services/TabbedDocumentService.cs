﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// The TabbedDocumentService class is responsible for managing tabbed documents within a UI. 
    /// It extends the DocumentServiceBase and implements interfaces for asynchronous document management and disposal. 
    /// This service allows for the creation, binding, and lifecycle management of tabbed documents within MetroTabControl.
    /// </summary>
    public sealed class TabbedDocumentService : DocumentServiceBase<TabControl>, IAsyncDocumentManagerService, IAsyncDisposable
    {
        #region TabbedDocument

        private class TabbedDocument : AsyncDisposable, IAsyncDocument
        {
            private readonly AsyncLifetime _lifetime = new(continueOnCapturedContext: true);
            private bool _isClosing;

            public TabbedDocument(TabbedDocumentService owner, TabItem tabItem)
            {
                _ = owner ?? throw new ArgumentNullException(nameof(owner));
                TabItem = tabItem ?? throw new ArgumentNullException(nameof(tabItem));

                _lifetime.AddDisposable(CancellationTokenSource);
                _lifetime.AddBracket(() => owner._documents.Add(this), () => owner._documents.Remove(this));
                _lifetime.Add(() => TabControl?.Items.Remove(TabItem));//second, remove tab item
                _lifetime.Add(() => TabItem.ClearStyle());//first, clear tab item style
                _lifetime.AddBracket(
                    () => ViewModelHelper.SetDataContextBinding(TabItem.Content, FrameworkElement.DataContextProperty, TabItem),
                    () => ViewModelHelper.ClearDataContextBinding(FrameworkElement.DataContextProperty, TabItem));//third
                _lifetime.Add(DetachContent);//second, detach content
                _lifetime.AddAsync(DisposeViewModelAsync);//first, dispose vm
                _lifetime.AddBracket(() => SetDocument(TabItem, this), () => SetDocument(TabItem, null));
                _lifetime.AddBracket(
                    () => TabItem.IsVisibleChanged += OnTabIsVisibleChanged,
                    () => TabItem.IsVisibleChanged -= OnTabIsVisibleChanged);
                _lifetime.AddBracket(
                    () => ViewModelHelper.SetViewTitleBinding(TabItem.Content, HeaderedContentControl.HeaderProperty, TabItem),
                    () => ViewModelHelper.ClearViewTitleBinding(HeaderedContentControl.HeaderProperty, TabItem));

                var dpd = DependencyPropertyDescriptor.FromProperty(HeaderedContentControl.HeaderProperty, typeof(HeaderedContentControl));
                if (dpd != null)
                {
                    _lifetime.AddBracket(
                        () => dpd.AddValueChanged(TabItem, OnTabHeaderChanged),
                        () => dpd.RemoveValueChanged(TabItem, OnTabHeaderChanged));
                }
                Debug.Assert(dpd != null);
                Debug.Assert(TabItem.DataContext == ViewModelHelper.GetViewModelFromView(TabItem.Content));
            }

            #region Properties

            private CancellationTokenSource CancellationTokenSource { get; } = new();

            public bool DisposeOnClose { get; set; }

            public bool HideInsteadOfClose { get; set; }

            public object Id { get; set; } = null!;

            private TabControl? TabControl => TabItem.Parent as TabControl;

            private TabItem TabItem { get; }

            public string? Title
            {
                get => TabItem.Header?.ToString();
                set => TabItem.Header = value;
            }

            #endregion

            #region Event Handlers

            private void OnTabHeaderChanged(object? sender, EventArgs e)
            {
                OnPropertyChanged(EventArgsCache.TitlePropertyChanged);
            }

            private void OnTabIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (TabItem.Content is UIElement element)
                {
                    element.Visibility = TabItem.Visibility;
                }
            }

            #endregion

            #region Methods

            public async ValueTask CloseAsync(bool force = true)
            {
                if (_lifetime.IsTerminated || IsDisposing)
                {
                    return;
                }
                if (force)
                {
#if NET8_0_OR_GREATER
                    await CancellationTokenSource.CancelAsync();
#else
                    CancellationTokenSource.Cancel();
#endif
                }
                if (_isClosing)
                {
                    return;
                }
                try
                {
                    _isClosing = true;
                    if (!force)
                    {
                        try
                        {
                            var viewModel = ViewModelHelper.GetViewModelFromView(TabItem.Content);
                            if (viewModel is IAsyncDocumentContent documentContent && await documentContent.CanCloseAsync(CancellationTokenSource.Token) == false)
                            {
                                CancellationTokenSource.Token.ThrowIfCancellationRequested();
                                return;
                            }
                        }
                        catch (OperationCanceledException) when (CancellationTokenSource.IsCancellationRequested)
                        {
                            //do nothing
                        }
                    }
                    CloseTab();
                    await DisposeAsync().ConfigureAwait(false);
                }
                finally
                {
                    _isClosing = false;
                }
            }

            private void CloseTab()
            {
                TabItem.Visibility = Visibility.Collapsed;
            }

            private void DetachContent()
            {
                var view = TabItem.Content;
                Debug.Assert(view != null);
                //First, detach DataContext from view
                ViewModelHelper.DetachViewModel(view);
                //Second, detach Content from tab item
                TabItem.Content = null;
                Debug.Assert(TabItem.DataContext == null);
            }

            private async ValueTask DisposeViewModelAsync()
            {
                var viewModel = ViewModelHelper.GetViewModelFromView(TabItem.Content);
                Debug.Assert(viewModel != null);
                if (DisposeOnClose && viewModel is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
            }

            public void Hide()
            {
                if (TabItem.Visibility != Visibility.Collapsed)
                {
                    TabItem.Visibility = Visibility.Collapsed;
                }
                TabItem.IsSelected = false;
            }

            protected override ValueTask OnDisposeAsync()
            {
                return _lifetime.DisposeAsync();
            }

            public void Show()
            {
                if (TabItem.Visibility != Visibility.Visible)
                {
                    TabItem.Visibility = Visibility.Visible;
                }
                TabItem.IsSelected = true;
            }

            #endregion
        }

        #endregion

        private readonly ObservableCollection<IAsyncDocument> _documents = [];
        private bool _isActiveDocumentChanging;
        private IDisposable? _subscription;

        #region Dependency Properties

        public static readonly DependencyProperty ActiveDocumentProperty = DependencyProperty.Register(
            nameof(ActiveDocument), typeof(IAsyncDocument), typeof(TabbedDocumentService),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((TabbedDocumentService)d).OnActiveDocumentChanged(e.OldValue as IAsyncDocument, e.NewValue as IAsyncDocument)));

        public static readonly DependencyProperty UnresolvedViewTypeProperty = DependencyProperty.Register(
            nameof(UnresolvedViewType), typeof(Type), typeof(TabbedDocumentService));

        #endregion

        public TabbedDocumentService()
        {
            if (ViewModelBase.IsInDesignMode) return;
            (_documents as INotifyPropertyChanged).PropertyChanged += OnDocumentsPropertyChanged;
        }

        #region Events

        public event EventHandler<ActiveDocumentChangedEventArgs>? ActiveDocumentChanged;

        #endregion

        #region Properties

        public IAsyncDocument? ActiveDocument
        {
            get => (IAsyncDocument)GetValue(ActiveDocumentProperty);
            set => SetValue(ActiveDocumentProperty, value);
        }

        public int Count => _documents.Count;

        public IEnumerable<IAsyncDocument> Documents => _documents;

        public Type? UnresolvedViewType
        {
            get => (Type)GetValue(UnresolvedViewTypeProperty);
            set => SetValue(UnresolvedViewTypeProperty, value);
        }

        #endregion

        #region Event Handlers

        private void OnActiveDocumentChanged(IAsyncDocument? oldValue, IAsyncDocument? newValue)
        {
            if (!_isActiveDocumentChanging)
            {
                try
                {
                    _isActiveDocumentChanging = true;
                    newValue?.Show();
                }
                finally
                {
                    _isActiveDocumentChanging = false;
                }
            }
            ActiveDocumentChanged?.Invoke(this, new ActiveDocumentChangedEventArgs(oldValue, newValue));
        }

        private void OnDocumentsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_documents.Count))
            {
                OnPropertyChanged(nameof(Count));
            }
        }

        private static async void OnTabControlItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems is { Count: > 0 })
                    {
                        foreach (var item in e.OldItems)
                        {
                            if (item is not TabItem tab)
                            {
                                continue;
                            }
                            var document = GetDocument(tab);
                            if (document is not null)
                            {
                                await document.CloseAsync(force: true).ConfigureAwait(false);
                            }
                        }
                    }
                    break;
            }
        }

        private void OnTabControlLoaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(Equals(sender, AssociatedObject));
            if (sender is FrameworkElement fe)
            {
                fe.Loaded -= OnTabControlLoaded;
            }
            Debug.Assert(_subscription == null);
            Disposable.DisposeAndNull(ref _subscription);
            _subscription = SubscribeTabControl(AssociatedObject);
        }

        private void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isActiveDocumentChanging)
            {
                return;
            }
            Debug.Assert(Equals(sender, AssociatedObject));
            if (sender is not TabControl tabControl)
            {
                return;
            }
            try
            {
                _isActiveDocumentChanging = true;
                ActiveDocument = (tabControl.SelectedItem is TabItem tabItem) ? GetDocument(tabItem) : null;
            }
            finally
            {
                _isActiveDocumentChanging = false;
            }
        }

        #endregion

        #region Methods

        public async ValueTask<IAsyncDocument> CreateDocumentAsync(string? documentType, object? viewModel,
            object? parentViewModel, object? parameter, CancellationToken cancellationToken = default)
        {
            Throw.IfNull(AssociatedObject);
            cancellationToken.ThrowIfCancellationRequested();
            object? view;
            if (documentType == null && ViewTemplate == null && ViewTemplateSelector == null)
            {
                view = GetUnresolvedView() ?? await GetViewLocator().GetOrCreateViewAsync(documentType, cancellationToken);
                await GetViewLocator().InitializeViewAsync(view, viewModel, parentViewModel, parameter, cancellationToken);
            }
            else
            {
                view = await CreateAndInitializeViewAsync(documentType, viewModel, parentViewModel, parameter, cancellationToken);
            }
            var tab = new TabItem
            {
                Header = "Item",
                Content = view
            };
            AssociatedObject.Items.Add(tab);
            var document = new TabbedDocument(this, tab);
            return document;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_documents.Count == 0)
                {
                    return;
                }
                await Task.WhenAll(_documents.ToList().Select(x => x.CloseAsync().AsTask())).ConfigureAwait(false);
                if (_documents.Count == 0)
                {
                    return;
                }

                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                void OnDocumentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                {
                    if (_documents.Count == 0)
                    {
                        tcs.TrySetResult(true);
                    }
                }

                _documents.CollectionChanged += OnDocumentsCollectionChanged;
                try
                {
                    if (_documents.Count == 0)
                    {
                        tcs.TrySetResult(true);
                    }
                    await tcs.Task.ConfigureAwait(false);
                }
                finally
                {
                    _documents.CollectionChanged -= OnDocumentsCollectionChanged;
                }
            }
            finally
            {
                (_documents as INotifyPropertyChanged).PropertyChanged -= OnDocumentsPropertyChanged;
            }
        }

        private object? GetUnresolvedView()
        {
            return UnresolvedViewType == null ? null : Activator.CreateInstance(UnresolvedViewType);
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();
            Debug.Assert(_subscription == null);
            if (AssociatedObject!.IsLoaded)
            {
                Disposable.DisposeAndNull(ref _subscription);
                _subscription = SubscribeTabControl(AssociatedObject);
            }
            else
            {
                AssociatedObject.Loaded += OnTabControlLoaded;
            }
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            AssociatedObject!.Loaded -= OnTabControlLoaded;
            Debug.Assert(_subscription != null);
            Disposable.DisposeAndNull(ref _subscription);
            base.OnDetaching();
        }

        private Lifetime? SubscribeTabControl(TabControl? tabControl)
        {
            if (tabControl == null)
            {
                return null;
            }
            if (tabControl.ItemsSource != null)
            {
                throw new InvalidOperationException("Can't use not null ItemsSource in this service");
            }
            var lifetime = new Lifetime();
            if (tabControl.Items is INotifyCollectionChanged collection)
            {
                lifetime.AddBracket(() => collection.CollectionChanged += OnTabControlItemsCollectionChanged,
                    () => collection.CollectionChanged -= OnTabControlItemsCollectionChanged);
            }
            lifetime.AddBracket(() => tabControl.SelectionChanged += OnTabControlSelectionChanged,
                () => tabControl.SelectionChanged -= OnTabControlSelectionChanged);
            return lifetime;
        }

        #endregion
    }

    internal static partial class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs TitlePropertyChanged = new(nameof(IAsyncDocument.Title));
    }
}
