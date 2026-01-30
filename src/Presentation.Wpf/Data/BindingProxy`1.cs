namespace System.Windows
{
    /// <summary>
    /// Passes DataContext between elements without a direct binding path. 
    /// Inherits from Freezable for use in XAML resources and advanced binding scenarios.
    /// This generic version enables type-safe data binding by specifying the DataContext type.
    /// </summary>
    /// <typeparam name="T">The type of the DataContext.</typeparam>
    /// <remarks>
    /// This class is particularly useful in WPF applications where you need to bind data
    /// across different elements that might be separated by templates or other containers
    /// which do not permit direct DataContext inheritance.
    ///
    /// Example usage:
    ///
    /// Define a concrete implementation for your ViewModel:
    /// <code><![CDATA[
    /// public class ExampleViewModelBindingProxy : BindingProxy<ExampleViewModel>
    /// {
    /// }
    /// ]]></code>
    ///
    /// Use it in XAML:
    /// <code><![CDATA[
    /// <Window x:Class="YourNamespace.MainWindow"
    ///         xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///         xmlns:local="clr-namespace:YourNamespace"
    ///         Title="MainWindow" Height="350" Width="525">
    ///     <Window.Resources>
    ///         <!-- Define the Proxy with DataContext Binding -->
    ///         <local:ExampleViewModelBindingProxy x:Key="proxy" DataContext="{Binding SomeViewModel}" />
    ///     </Window.Resources>
    ///
    ///     <Grid DataContext="{Binding Source={StaticResource proxy}, Path=DataContext}">
    ///         <!-- Your UI elements here -->
    ///     </Grid>
    /// </Window>
    /// ]]></code>
    /// </remarks>
    public class BindingProxy<T>: Freezable where T: class
    {
        // DependencyProperty to hold the DataContext of type T.
        public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register(
            nameof(DataContext), typeof(T), typeof(BindingProxy<T>), 
            new PropertyMetadata(null, (d, e) => ((BindingProxy<T>)d).OnDataContextChanged(e.OldValue as T, e.NewValue as T)));

        /// <summary>
        /// Gets or sets the DataContext for this <see cref="BindingProxy{T}"/>.
        /// </summary>
        public T DataContext
        {
            get => (T)GetValue(DataContextProperty);
            set => SetValue(DataContextProperty, value);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BindingProxy{T}"/> class.
        /// This method is required by the <see cref="Freezable"/> base class.
        /// </summary>
        /// <returns>A new instance of <see cref="BindingProxy{T}"/>.</returns>
        /// <seealso cref="Freezable.CreateInstanceCore()"/>
        protected override Freezable CreateInstanceCore()
        {
            return (Freezable)Activator.CreateInstance(GetType())!;
        }

        protected virtual void OnDataContextChanged(T? oldValue, T? newValue)
        {
        }
    }
}
