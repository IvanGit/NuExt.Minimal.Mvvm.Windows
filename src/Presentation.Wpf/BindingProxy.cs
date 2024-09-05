namespace System.Windows
{
    /// <summary>
    /// The BindingProxy class is used to pass DataContext between elements without a direct binding path. 
    /// Inherits from Freezable for use in XAML resources and advanced binding scenarios.
    /// </summary>
    /// <remarks>
    /// This class is particularly useful in WPF applications where you need to bind data
    /// across different elements that might be separated by templates or other containers
    /// which do not permit direct DataContext inheritance.
    /// 
    /// Example usage:
    /// <code>
    /// <![CDATA[
    /// <Window.Resources>
    ///     <BindingProxy x:Key="proxy" DataContext="{Binding SomeViewModel}" />
    /// </Window.Resources>
    ///
    /// <Grid DataContext="{Binding Source={StaticResource proxy}, Path=DataContext}">
    ///     <!-- Your UI elements here -->
    /// </Grid>
    /// ]]>
    /// </code>
    /// </remarks>
    public class BindingProxy : Freezable
    {
        // DependencyProperty to hold the DataContext.
        public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register(nameof(DataContext), typeof(object), typeof(BindingProxy), new PropertyMetadata(null, (d, e) => ((BindingProxy)d).OnDataContextChanged(e.NewValue, e.OldValue)));

        /// <summary>
        /// Gets or sets the DataContext for this <see cref="BindingProxy"/>.
        /// </summary>
        public object DataContext
        {
            get => GetValue(DataContextProperty);
            set => SetValue(DataContextProperty, value);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BindingProxy"/> class.
        /// This method is required by the <see cref="Freezable"/> base class.
        /// </summary>
        /// <returns>A new instance of <see cref="BindingProxy"/>.</returns>
        /// <seealso cref="Freezable.CreateInstanceCore()"/>
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        protected virtual void OnDataContextChanged(object? newValue, object? oldValue)
        {
        }
    }
}
