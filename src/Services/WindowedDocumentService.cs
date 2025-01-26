using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// The WindowedDocumentService class is responsible for managing windowed documents within a UI. 
    /// It extends the DocumentServiceBase and implements interfaces for asynchronous document management and disposal. 
    /// This service allows for the creation, binding, and lifecycle management of windowed documents within the main window.
    /// </summary>
    public sealed class WindowedDocumentService : DocumentServiceBase<Window>, IAsyncDocumentManagerService, IAsyncDisposable
    {
        #region WindowedDocument

        private class WindowedDocument : AsyncDisposable, IAsyncDocument
        {
            private readonly AsyncLifetime _lifetime = new(continueOnCapturedContext: true);
            private bool _isClosing;

            public WindowedDocument(WindowedDocumentService owner, Window window)
            {
                Owner = owner ?? throw new ArgumentNullException(nameof(owner));
                Window = window ?? throw new ArgumentNullException(nameof(window));

                _lifetime.AddDisposable(CancellationTokenSource);
                _lifetime.AddBracket(() => owner._documents.Add(this), () => owner._documents.Remove(this));
                _lifetime.Add(() => Window.ClearStyle());
                _lifetime.Add(() => BindingOperations.ClearBinding(Window, FrameworkElement.DataContextProperty));
                _lifetime.Add(DetachContent);
                _lifetime.AddAsync(DisposeViewModelAsync);
                _lifetime.AddBracket(() => SetDocument(Window, this), () => SetDocument(Window, null));
                _lifetime.AddBracket(
                    () => Window.Activated += OnWindowActivated,
                    () => Window.Activated -= OnWindowActivated);
                _lifetime.AddBracket(
                    () => Window.Closed += OnWindowClosed,
                    () => Window.Closed -= OnWindowClosed);
                _lifetime.AddBracket(
                    () => Window.Closing += OnWindowClosing,
                    () => Window.Closing -= OnWindowClosing);
                _lifetime.AddBracket(
                    () => Window.Deactivated += OnWindowDeactivated,
                    () => Window.Deactivated -= OnWindowDeactivated);
                _lifetime.AddBracket(
                    () => ViewModelHelper.SetViewTitleBinding(Window.Content, Window.TitleProperty, Window),
                    () => BindingOperations.ClearBinding(Window, Window.TitleProperty));

                var dpd = DependencyPropertyDescriptor.FromProperty(Window.TitleProperty, typeof(Window));
                if (dpd != null)
                {
                    _lifetime.AddBracket(
                        () => dpd.AddValueChanged(Window, OnWindowTitleChanged),
                        () => dpd.RemoveValueChanged(Window, OnWindowTitleChanged));
                }
                Debug.Assert(dpd != null);
                Debug.Assert(Window.DataContext == ViewModelHelper.GetViewModelFromView(Window.Content));
            }

            #region Properties

            private CancellationTokenSource CancellationTokenSource { get; } = new();

            public bool DisposeOnClose { get; set; }

            public bool HideInsteadOfClose { get; set; }

            public object Id { get; set; } = null!;

            public string? Title
            {
                get => Window.Title;
                set => Window.Title = value;
            }

            private Window Window { get; }

            private WindowedDocumentService Owner { get; }

            #endregion

            #region Event Handlers

            private void OnWindowActivated(object? sender, EventArgs e)
            {
                Debug.Assert(GetDocument((Window)sender!) == this);
                Owner.ActiveDocument = this;
            }

            private void OnWindowClosed(object? sender, EventArgs e)
            {
                Debug.Assert(GetDocument((Window)sender!) == this);
                Debug.Assert(Owner.ActiveDocument != this);
            }

            private void OnWindowClosing(object? sender, CancelEventArgs e)
            {
                Debug.Assert(GetDocument((Window)sender!) == this);
                if (e.Cancel || _isClosing)
                {
                    return;
                }

                e.Cancel = true;
                Window.Dispatcher.InvokeAsync(async () => await CloseWindowAsync(Window, CancellationTokenSource.Token));
            }

            private static async ValueTask CloseWindowAsync(Window window, CancellationToken cancellationToken)
            {
                try
                {
                    var viewModel = ViewModelHelper.GetViewModelFromView(window.Content);
                    Debug.Assert(viewModel is IAsyncDocumentContent);
                    if (viewModel is IAsyncDocumentContent documentContent && await documentContent.CanCloseAsync(cancellationToken) == false)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    //do nothing
                }

                var document = GetDocument(window);
                if (document?.HideInsteadOfClose == true)
                {
                    document.Hide();
                    return;
                }
                if (document != null)
                {
                    await document.CloseAsync(force: true).ConfigureAwait(false);
                    return;
                }
                Debug.Assert(window.DataContext == null);//viewModel is disposed
                if (window.Dispatcher.CheckAccess())
                {
                    window.Close();
                    return;
                }
                await window.Dispatcher.InvokeAsync(window.Close);
            }

            private void OnWindowDeactivated(object? sender, EventArgs e)
            {
                Debug.Assert(GetDocument((Window)sender!) == this);
                if (Owner.ActiveDocument == this)
                {
                    Owner.ActiveDocument = null;
                }
            }

            private void OnWindowTitleChanged(object? sender, EventArgs e)
            {
                OnPropertyChanged(EventArgsCache.TitlePropertyChanged);
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
                            var viewModel = ViewModelHelper.GetViewModelFromView(Window.Content);
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
                    CloseWindow();
                    await DisposeAsync().ConfigureAwait(false);
                }
                finally
                {
                    _isClosing = false;
                }
            }

            private void CloseWindow()
            {
                Window.Hide();
                Window.Close();
            }

            private void DetachContent()
            {
                var view = Window.Content;
                Debug.Assert(view != null);
                //First, detach DataContext from view
                ViewModelHelper.DetachViewModel(view);
                //Second, detach Content from window
                Window.Content = null;
                Debug.Assert(Window.DataContext == null);
            }

            private async ValueTask DisposeViewModelAsync()
            {
                var viewModel = ViewModelHelper.GetViewModelFromView(Window.Content);
                Debug.Assert(viewModel != null);
                if (DisposeOnClose && viewModel is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
            }

            public void Hide()
            {
                Window.Hide();
            }

            protected override ValueTask OnDisposeAsync()
            {
                return _lifetime.DisposeAsync();
            }

            public void Show()
            {
                Window.Show();
                Window.Activate();
            }

            #endregion
        }

        #endregion

        private readonly ObservableCollection<IAsyncDocument> _documents = [];

        #region Dependency Properties

        public static readonly DependencyProperty ActiveDocumentProperty = DependencyProperty.Register(
            nameof(ActiveDocument), typeof(IAsyncDocument), typeof(WindowedDocumentService),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((WindowedDocumentService)d).OnActiveDocumentChanged(e.OldValue as IAsyncDocument, e.NewValue as IAsyncDocument)));

        public static readonly DependencyProperty SetWindowOwnerProperty = DependencyProperty.Register(
            nameof(SetWindowOwner), typeof(bool), typeof(WindowedDocumentService));

        public static readonly DependencyProperty UnresolvedViewTypeProperty = DependencyProperty.Register(
            nameof(UnresolvedViewType), typeof(Type), typeof(WindowedDocumentService));

        public static readonly DependencyProperty WindowStartupLocationProperty = DependencyProperty.Register(
            nameof(WindowStartupLocation), typeof(WindowStartupLocation), typeof(WindowedDocumentService), new PropertyMetadata(WindowStartupLocation.CenterScreen));

        public static readonly DependencyProperty WindowStyleProperty = DependencyProperty.Register(
            nameof(WindowStyle), typeof(Style), typeof(WindowedDocumentService));

        public static readonly DependencyProperty WindowStyleSelectorProperty = DependencyProperty.Register(
            nameof(WindowStyleSelector), typeof(StyleSelector), typeof(WindowedDocumentService));

        public static readonly DependencyProperty WindowTypeProperty = DependencyProperty.Register(
            nameof(WindowType), typeof(Type), typeof(WindowedDocumentService));

        #endregion

        public WindowedDocumentService()
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

        public bool SetWindowOwner
        {
            get => (bool)GetValue(SetWindowOwnerProperty);
            set => SetValue(SetWindowOwnerProperty, value);
        }

        public Type? UnresolvedViewType
        {
            get => (Type)GetValue(UnresolvedViewTypeProperty);
            set => SetValue(UnresolvedViewTypeProperty, value);
        }

        public WindowStartupLocation WindowStartupLocation
        {
            get => (WindowStartupLocation)GetValue(WindowStartupLocationProperty);
            set => SetValue(WindowStartupLocationProperty, value);
        }

        public Style? WindowStyle
        {
            get => (Style)GetValue(WindowStyleProperty);
            set => SetValue(WindowStyleProperty, value);
        }

        public StyleSelector? WindowStyleSelector
        {
            get => (StyleSelector)GetValue(WindowStyleSelectorProperty);
            set => SetValue(WindowStyleSelectorProperty, value);
        }

        public Type? WindowType
        {
            get => (Type)GetValue(WindowTypeProperty);
            set => SetValue(WindowTypeProperty, value);
        }

        #endregion

        #region Event Handlers

        private void OnActiveDocumentChanged(IAsyncDocument? oldValue, IAsyncDocument? newValue)
        {
            Debug.Assert(oldValue != newValue);
            newValue?.Show();
            ActiveDocumentChanged?.Invoke(this, new ActiveDocumentChangedEventArgs(oldValue, newValue));
        }

        private void OnDocumentsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_documents.Count))
            {
                OnPropertyChanged(EventArgsCache.CountPropertyChanged);
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
            }
            else
            {
                view = await CreateViewAsync(documentType, cancellationToken);
            }

            var window = CreateWindow(view, viewModel);
            ViewModelHelper.SetDataContextBinding(view, FrameworkElement.DataContextProperty, window);

            await ViewModelHelper.InitializeViewAsync(view, viewModel, parentViewModel, parameter, cancellationToken);

            var document = new WindowedDocument(this, window);
            return document;
        }

        private Window CreateWindow(object? view, object? viewModel)
        {
            var window = WindowType != null ? (Window)Activator.CreateInstance(WindowType)! : new Window();
            window.Title = "Untitled";
            window.Content = view;
            var windowStyle = GetWindowStyle(window, viewModel);
            if (windowStyle != null)
            {
                window.Style = windowStyle;
            }

            if (SetWindowOwner)
            {
                window.Owner = AssociatedObject;
            }
            window.WindowStartupLocation = WindowStartupLocation;

            return window;
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

        private Style? GetWindowStyle(Window window, object? viewModel)
        {
            // WindowStyle has first stab
            var style = WindowStyle;

            // no WindowStyle set, try WindowStyleSelector
            if (style == null && WindowStyleSelector != null)
            {
                style = WindowStyleSelector.SelectStyle(viewModel, window);
            }

            return style;
        }

        #endregion
    }
}
